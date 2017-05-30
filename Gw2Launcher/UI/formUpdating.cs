using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Gw2Launcher.UI
{
    public partial class formUpdating : Form
    {
        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private Dictionary<ushort, AccountItem> accounts;
        private int remaining, total;

        private string datPath;
        private long datSize, bytesChanged, totalBytes, totalBytesRead, totalBytesDownloaded, totalBytesUploaded;
        private DateTime datLastWrite;
        private CancellationTokenSource cancelWatch, cancelDelayedComplete;
        private DateTime timeout;
        private Process activeProcess;
        private Settings.IAccount activeAccount;
        private bool isComplete;
        private ManualResetEvent processChanged;
        //private Point lastPosition;
        private Net.AssetProxy.ProxyServer proxy;

        private bool closeOnCompletion;
        private DialogResult result;

        private class AccountItem
        {
            public AccountItem(Settings.IAccount account)
            {
                this.Account = account;
            }

            public Settings.IAccount Account;
            public byte State;
        }

        public formUpdating(List<Settings.IAccount> accounts, bool alreadyQueued, bool closeOnCompletion)
        {
            InitializeComponent();

            this.processChanged = new ManualResetEvent(false);
            this.result = DialogResult.OK;
            this.closeOnCompletion = closeOnCompletion;
            this.accounts = new Dictionary<ushort, AccountItem>();
            this.total = accounts != null ? accounts.Count * 2 : 0;
            this.remaining = total;

            labelName.Text = "Waiting...";
            labelTotalSize.Text = "---";
            labelWritten.Text = "---";
            timeout = DateTime.UtcNow;

            progressUpdating.Maximum = total;

            if (accounts != null)
            {
                foreach (var account in accounts)
                {
                    this.accounts[account.UID] = new AccountItem(account);
                }
            }

            if (Net.AssetProxy.ServerController.Enabled)
            {
                proxy = Net.AssetProxy.ServerController.Active;
                proxy.ResponseDataReceived += proxy_ResponseDataReceived;
                proxy.RequestDataReceived += proxy_RequestDataReceived;
                label4.Text = "UL / DL";
            }

            Client.Launcher.AccountStateChanged += Launcher_AccountStateChanged;
            Client.Launcher.AccountProcessActivated += Launcher_AccountProcessActivated;
            Client.Launcher.AllQueuedLaunchesComplete += Launcher_AllQueuedLaunchesComplete;

            Settings.GW2Path.ValueChanged += GW2Path_ValueChanged;
            GW2Path_ValueChanged(null, EventArgs.Empty);

            long size;
            DateTime lastWrite;
            try
            {
                FileInfo fi = new FileInfo(datPath);
                size = fi.Length;
                lastWrite = fi.LastWriteTimeUtc;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                size = 0;
                lastWrite = DateTime.MinValue;
            }

            if (!alreadyQueued)
                this.Shown += formUpdating_Shown;
        }

        public formUpdating(List<Settings.IAccount> accounts)
            : this(accounts, false, true)
        {
        }

        public void AddAccount(Settings.IAccount account)
        {
            this.total += 2;
            this.remaining += 2;
            progressUpdating.Maximum = total;
            if (!this.accounts.ContainsKey(account.UID))
                this.accounts[account.UID] = new AccountItem(account);

            if (cancelDelayedComplete != null)
            {
                cancelDelayedComplete.Cancel();
                cancelDelayedComplete = null;
            }

            if (isComplete)
            {
                isComplete = false;
                labelAbort.Enabled = true;
                labelAbort.ForeColor = Color.FromArgb(49, 121, 242);
                labelName.Text = "Waiting...";
                label1.ForeColor = SystemColors.ControlText;
            }
        }

        void GW2Path_ValueChanged(object sender, EventArgs e)
        {
            string path = Settings.GW2Path.Value;
            if (!string.IsNullOrEmpty(path))
            {
                datPath = Path.Combine(Path.GetDirectoryName(path), "Gw2.dat");
                try
                {
                    FileInfo fi = new FileInfo(datPath);
                    datSize = fi.Length;
                    datLastWrite = fi.LastWriteTimeUtc;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    datSize = 0;
                    datLastWrite = DateTime.MinValue;
                }

                try
                {
                    Action a = delegate
                    {
                        labelTotalSize.Text = Util.Text.FormatBytes(datSize);

                        if (cancelWatch == null || cancelWatch.IsCancellationRequested)
                        {
                            cancelWatch = new CancellationTokenSource();
                            WatchDat(cancelWatch.Token);
                        }
                    };

                    if (this.InvokeRequired)
                        this.Invoke(a);
                    else
                        a();
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        void proxy_ResponseDataReceived(object sender, ArraySegment<byte> e)
        {
            lock (this)
            {
                totalBytesDownloaded += e.Count;
            }
        }

        void proxy_RequestDataReceived(object sender, ArraySegment<byte> e)
        {
            lock (this)
            {
                totalBytesUploaded += e.Count;
            }
        }

        void Launcher_AllQueuedLaunchesComplete(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(
                    delegate
                    {
                        Launcher_AllQueuedLaunchesComplete(sender, e);
                    }));
                return;
            }

            if (remaining > 0)
            {
                if (cancelDelayedComplete != null)
                    cancelDelayedComplete.Cancel();

                cancelDelayedComplete = new CancellationTokenSource();

                Action a = async delegate
                {
                    try
                    {
                        await Task.Delay(3000, cancelDelayedComplete.Token);
                    }
                    catch (TaskCanceledException ex)
                    {
                        Util.Logging.Log(ex);
                        return;
                    }
                    if (remaining > 0 && Client.Launcher.GetPendingLaunchCount() == 0 && Client.Launcher.GetActiveProcesses().Count == 0)
                    {
                        OnComplete();
                    }                    
                };

                a();
            }
        }

        private string GetInstanceName(Process process)
        {
            if (PerformanceCounterCategory.CounterExists("ID Process", "Process"))
            {
                Process[] processes = Process.GetProcessesByName(process.ProcessName);
                if (processes.Length > 0)
                {
                    for (var i = processes.Length - 1; i >= 0; i--)
                    {
                        string name = i == 0 ? process.ProcessName : process.ProcessName + "#" + i;

                        try
                        {
                            PerformanceCounter counter = new PerformanceCounter("Process", "ID Process", name);

                            if (process.Id == counter.RawValue)
                            {
                                return name;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                }
            }

            return null;
        }

        private async void WatchDat(CancellationToken token)
        {
            Process p = null;
            PerformanceCounter counter = null;
            PerformanceCounter counterRead = null;
            bool doCounters = proxy == null;
            long lastDownloaded = 0;
            long lastUploaded = 0;
            long lastDownloadSample = 0;
            int lastSpeed = 0;
            DateTime nextUpdate = DateTime.UtcNow.AddSeconds(3);
            string dcache = Util.Text.FormatBytes(0), ucache = Util.Text.FormatBytes(0), dscache = "";

            do
            {
                if (doCounters && p != activeProcess)
                {
                    p = activeProcess;
                    if (counter != null)
                    {
                        counter.Dispose();
                        counterRead.Dispose();
                        counter = null;
                    }
                    if (p != null)
                    {
                        await Task.Run(
                            delegate
                            {
                                try
                                {
                                    string name = GetInstanceName(p);
                                    if (name != null)
                                    {
                                        counter = new PerformanceCounter("Process", "IO Write Bytes/sec", name);
                                        counterRead = new PerformanceCounter("Process", "IO Read Bytes/sec", name);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }
                            });
                    }
                }

                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException e)
                {
                    Util.Logging.Log(e);
                    break;
                }

                if (doCounters)
                {
                    if (counter != null)
                    {
                        try
                        {
                            var val = (long)counter.NextValue();
                            bool update = false;
                            if (val != 0)
                            {
                                totalBytes += val;
                                update = true;
                            }
                            val = (long)counterRead.NextValue();
                            if (val != 0)
                            {
                                totalBytesRead += val;
                                update = true;
                            }
                            if (update)
                            {
                                labelWritten.Text = Util.Text.FormatBytes(totalBytesRead) + " / " + Util.Text.FormatBytes(totalBytes);
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            counter.Dispose();
                            counterRead.Dispose();
                            counter = null;
                        }
                    }
                }
                else
                {
                    bool update = false;
                    var bytesDownloaded = this.totalBytesDownloaded;

                    if (lastDownloaded != bytesDownloaded)
                    {
                        lastDownloaded = bytesDownloaded;
                        dcache = Util.Text.FormatBytes(lastDownloaded);
                        update = true;
                    }
                    if (lastUploaded != totalBytesUploaded)
                    {
                        lastUploaded = totalBytesUploaded;
                        ucache = Util.Text.FormatBytes(lastUploaded);
                        update = true;
                    }

                    if (bytesDownloaded > 0)
                    {
                        var now = DateTime.UtcNow;
                        if (now > nextUpdate)
                        {
                            var s = now.Subtract(nextUpdate).TotalSeconds + 3;
                            if (s > 0)
                            {
                                var sample = (int)((bytesDownloaded - lastDownloadSample) / s + 0.5);
                                if (sample < 1024)
                                    sample = 0;

                                if (sample != lastSpeed)
                                {
                                    lastSpeed = sample;
                                    if (sample != 0)
                                        dscache = " @ " + Util.Text.FormatBytes(sample) + "/s";
                                    else
                                        dscache = "";
                                    update = true;
                                }

                                nextUpdate = now.AddSeconds(3);
                                lastDownloadSample = bytesDownloaded;
                            }
                        }
                    }

                    if (update)
                        labelWritten.Text = ucache + " / " + dcache + dscache;
                }

                long size;
                DateTime lastWrite;
                try
                {
                    FileInfo fi = new FileInfo(datPath);
                    size = fi.Length;
                    lastWrite = fi.LastWriteTimeUtc;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    size = 0;
                    lastWrite = DateTime.MinValue;
                }

                var t = DateTime.UtcNow;
                if (size != datSize)
                {
                    long difference = size - datSize;
                    datSize = size;
                    datLastWrite = lastWrite;
                    timeout = t;
                    OnTimeoutChanged();

                    if (difference > 0)
                    {
                        bytesChanged += difference;
                        labelTotalSize.Text = Util.Text.FormatBytes(size) + (bytesChanged > 0 ? " (+" + Util.Text.FormatBytes(bytesChanged) + ")" : "");
                    }
                }
                else if (datLastWrite != lastWrite)
                {
                    datLastWrite = lastWrite;
                    timeout = t;
                    OnTimeoutChanged();
                }
                else if (t.Subtract(timeout).TotalMilliseconds > 60000)
                {
                    if (!panelWarning.Visible)
                    {
                        panelWarning.BringToFront();
                        panelWarning.Visible = true;
                    }
                }
            }
            while (!token.IsCancellationRequested);

            if (counter != null)
            {
                counter.Dispose();
                counterRead.Dispose();
            }
        }

        private void OnTimeoutChanged()
        {
            if (panelWarning.Visible)
                panelWarning.Visible = false;
        }

        private void OnProcessChanged()
        {
            var p = activeProcess;

            if (p != null)
            {
                if (proxy == null && cancelWatch != null && !cancelWatch.IsCancellationRequested)
                {
                    cancelWatch.Cancel();
                    cancelWatch = new CancellationTokenSource();
                    WatchDat(cancelWatch.Token);
                }

                try
                {
                    if (p.HasExited)
                        p = null;
                }
                catch  (Exception e)
                {
                    Util.Logging.Log(e);
                    p = null;
                }
            }

            if (p == null)
            {
            }
            else
            {
                ShowCloseTooltip(p);
            }
        }

        void formUpdating_Shown(object sender, EventArgs e)
        {
            this.Shown -= formUpdating_Shown;

            bool first = true;

            foreach (var item in accounts.Values)
            {
                if (first)
                {
                    Client.Launcher.Launch(item.Account, Client.Launcher.LaunchMode.UpdateVisible);
                    first = false;
                }
                else
                    Client.Launcher.Launch(item.Account, Client.Launcher.LaunchMode.Update);
            }
        }

        void Launcher_AccountProcessActivated(Settings.IAccount account, Process e)
        {
            if (activeAccount != account)
                return;
            
            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            Launcher_AccountProcessActivated(account, e);
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                return;
            }

            activeProcess = e;
            OnProcessChanged();
        }

        void Launcher_AccountStateChanged(ushort uid, Client.Launcher.AccountState state, Client.Launcher.AccountState previousState, object data)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            Launcher_AccountStateChanged(uid, state, previousState, data);
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                return;
            }

            AccountItem item;
            if (accounts.TryGetValue(uid,out item))
            {
                timeout = DateTime.UtcNow;
                OnTimeoutChanged();

                switch (state)
                {
                    //case Client.Launcher.AccountState.Active:
                    //case Client.Launcher.AccountState.ActiveGame:
                    case Client.Launcher.AccountState.UpdatingVisible:
                    case Client.Launcher.AccountState.Updating:

                        activeAccount = item.Account;
                        var p = data as Process;
                        if (p != activeProcess)
                        {
                            activeProcess = p;
                            OnProcessChanged();
                        }
                            
                        labelName.Text = "Updating " + item.Account.Name;
                        item.State = 1;
                        remaining--;
                        if (remaining >= 0)
                            progressUpdating.Value = total - remaining;

                        break;
                    case Client.Launcher.AccountState.Error:
                    case Client.Launcher.AccountState.None:

                        if (activeAccount == item.Account)
                        {
                            activeAccount = null;
                            if (activeProcess != null)
                            {
                                activeProcess = null;
                                OnProcessChanged();
                            }
                        }

                        if (item.State == 1)
                        {
                            //if (item.State == 0)
                            //    remaining--;
                            item.State = 2;
                            remaining--;
                            if (remaining >= 0)
                                progressUpdating.Value = total - remaining;

                            if (this.remaining == 0)
                            {
                                if (cancelWatch != null)
                                    cancelWatch.Cancel();
                                OnComplete();
                            }
                        }

                        break;
                }
            }
        }

        private /*async*/ void ShowCloseTooltip(Process p)
        {
            //gw2 will automatically close once the update is complete, so this isn't really needed

            //while (!p.HasExited)
            //{
            //    try
            //    {
            //        double ms = DateTime.Now.Subtract(p.StartTime).TotalMilliseconds;
            //        if (ms > 3000)
            //            break;
            //        else
            //        {
            //            await Task.Delay(3001 - (int)ms);
            //        }
            //    }
            //    catch 
            //    {
            //        return;
            //    }
            //}

            //using (formAccountTooltip t = new formAccountTooltip())
            //{
            //    t.Show(null, "Close Guild Wars 2 once the update is complete", 0);

            //    bool setOwner = false;

            //    while (p != null && t != null && !t.IsDisposed)
            //    {
            //        if (t.Visible && t.WindowState == FormWindowState.Normal)
            //        {
            //            try
            //            {
            //                Rectangle r = Windows.WindowSize.GetWindowRect(p);

            //                if (!setOwner)
            //                {
            //                    try
            //                    {
            //                        int h = p.MainWindowHandle.ToInt32();
            //                        if (h != 0)
            //                        {
            //                            SetWindowLong(t.Handle, -8, h);
            //                            setOwner = true;
            //                        }
            //                    }
            //                    catch { }
            //                }

            //                if (r.X != lastPosition.X || r.Y != lastPosition.Y)
            //                {
            //                    lastPosition = new Point(r.X, r.Y);
            //                    t.AttachTo(new Rectangle(r.Right - 211, r.Y + 176, 17, 17), 5, AnchorStyles.Right);
            //                    t.BringToFront();
            //                }
            //            }
            //            catch
            //            {
            //                return;
            //            }
            //        }
            //        await Task.Delay(100);
            //    }
            //}
        }

        private void OnComplete()
        {
            isComplete = true;

            if (cancelWatch != null)
            {
                cancelWatch.Cancel();
                cancelWatch = null;
            }

            if (labelAbort.Enabled)
                labelName.Text = "Complete";
            else
                labelName.Text = "Cancelled";

            labelAbort.Enabled = false;
            labelAbort.ForeColor = SystemColors.GrayText;
            label1.ForeColor = SystemColors.GrayText;

            if (closeOnCompletion)
            {
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (proxy != null)
            {
                proxy.ResponseDataReceived -= proxy_ResponseDataReceived;
                proxy.RequestDataReceived -= proxy_RequestDataReceived;
            }

            Settings.GW2Path.ValueChanged -= GW2Path_ValueChanged;
            Client.Launcher.AccountStateChanged -= Launcher_AccountStateChanged;
            Client.Launcher.AccountProcessActivated -= Launcher_AccountProcessActivated;
            Client.Launcher.AllQueuedLaunchesComplete -= Launcher_AllQueuedLaunchesComplete;

            if (cancelDelayedComplete != null)
                cancelDelayedComplete.Cancel();

            if (cancelWatch != null)
                cancelWatch.Cancel();

            if (!isComplete && remaining != 0)
            {
                result = DialogResult.Abort;
                Client.Launcher.CancelPendingLaunches();
                if (remaining != total)
                    Client.Launcher.KillActiveLaunches();
            }

            this.DialogResult = result;
        }

        private void labelAbort_Click(object sender, EventArgs e)
        {
            labelAbort.Enabled = false;
            labelAbort.ForeColor = SystemColors.GrayText;
            this.result = DialogResult.Abort;
            Client.Launcher.CancelPendingLaunches();
            Client.Launcher.KillActiveLaunches();
        }

        private void formUpdating_Load(object sender, EventArgs e)
        {

        }

        private void formUpdating_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
