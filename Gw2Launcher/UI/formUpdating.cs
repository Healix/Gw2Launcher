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
        private Dictionary<Settings.IAccount, AccountItem> accounts;
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
            this.accounts = new Dictionary<Settings.IAccount, AccountItem>();
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
                    this.accounts[account] = new AccountItem(account);
                }
            }

            if (Net.AssetProxy.ServerController.Enabled)
            {
                proxy = Net.AssetProxy.ServerController.Active;
                if (proxy != null)
                {
                    proxy.ResponseDataReceived += proxy_ResponseDataReceived;
                    proxy.RequestDataReceived += proxy_RequestDataReceived;
                    label4.Text = "UL / DL";
                }
            }

            Client.Launcher.AccountStateChanged += Launcher_AccountStateChanged;
            Client.Launcher.AccountProcessActivated += Launcher_AccountProcessActivated;
            Client.Launcher.AllQueuedLaunchesComplete += Launcher_AllQueuedLaunchesComplete;

            Settings.GW2Path.ValueChanged += GW2Path_ValueChanged;
            GW2Path_ValueChanged(null, EventArgs.Empty);
            
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
            if (!this.accounts.ContainsKey(account))
                this.accounts[account] = new AccountItem(account);

            if (cancelDelayedComplete != null)
            {
                cancelDelayedComplete.Cancel();
                cancelDelayedComplete.Dispose();
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

                Util.Invoke.Required(this, delegate
                {
                    labelTotalSize.Text = Util.Text.FormatBytes(datSize);

                    if (cancelWatch == null || cancelWatch.IsCancellationRequested)
                    {
                        cancelWatch = new CancellationTokenSource();
                        WatchDat(cancelWatch.Token);
                    }
                });
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
            if (Util.Invoke.IfRequiredAsync(this,
                delegate
                {
                    Launcher_AllQueuedLaunchesComplete(sender, e);
                }))
            {
                return;
            }

            if (remaining > 0)
            {
                if (cancelDelayedComplete != null)
                {
                    cancelDelayedComplete.Cancel();
                    cancelDelayedComplete.Dispose();
                }

                cancelDelayedComplete = new CancellationTokenSource();
                OnAllQueuedLaunchesCompleteAsync(cancelDelayedComplete.Token);
            }
        }

        private async void OnAllQueuedLaunchesCompleteAsync(CancellationToken cancel)
        {
            try
            {
                await Task.Delay(3000, cancel);
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                return;
            }
            if (remaining > 0 && Client.Launcher.GetPendingLaunchCount() == 0 && Client.Launcher.GetActiveProcesses().Count == 0)
            {
                OnComplete();
            }  
        }

        private async void WatchDat(CancellationToken token)
        {
            Process p = null;
            PerformanceCounter counter = null;
            PerformanceCounter counterRead = null;
            bool doCounters = proxy == null,
                 queryCounters = true;
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
                    if (counter != null || counterRead != null)
                    {
                        using (counter) { }
                        using (counterRead) { }
                        counter = null;
                        counterRead = null;
                    }
                    if (p != null)
                    {
                        await Task.Run(
                            delegate
                            {
                                try
                                {
                                    if (queryCounters)
                                    {
                                        if (!Util.ProcessUtil.QueryPerformanceCounters(p.Id))
                                        {
                                            if (!p.HasExited)
                                                doCounters = false;
                                            throw new NotSupportedException();
                                        }
                                    }

                                    var name = Util.PerfCounter.GetInstanceName(p);
                                    if (name != null)
                                    {
                                        counter = Util.PerfCounter.GetCounter(Util.PerfCounter.CategoryName.Process, Util.PerfCounter.CounterName.IOWriteByesPerSecond, name);
                                        counterRead = Util.PerfCounter.GetCounter(Util.PerfCounter.CategoryName.Process, Util.PerfCounter.CounterName.IOReadBytesPerSecond, name);
                                        if (counter == null || counterRead == null)
                                            throw new NullReferenceException();

                                        queryCounters = false;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);

                                    if (counter != null || counterRead != null)
                                    {
                                        using (counter) { }
                                        using (counterRead) { }
                                        counter = null;
                                        counterRead = null;
                                    }
                                }
                            });
                    }

                    lastDownloaded = 0;
                    lastUploaded = 0;
                }

                try
                {
                    await Task.Delay(1000, token);
                }
                catch
                {
                    break;
                }

                if (doCounters)
                {
                    if (counter != null)
                    {
                        try
                        {
                            //var val = (long)counter.NextValue();
                            //bool update = false;
                            //if (val != 0)
                            //{
                            //    totalBytes += val;
                            //    update = true;
                            //}
                            //val = (long)counterRead.NextValue();
                            //if (val != 0)
                            //{
                            //    totalBytesRead += val;
                            //    update = true;
                            //}

                            var update = false;
                            var raw = counter.RawValue;
                            var diff = raw - lastUploaded;
                            lastUploaded = raw;
                            if (diff > 0)
                            {
                                totalBytes += diff;
                                update = true;
                            }
                            raw = counterRead.RawValue;
                            diff = raw - lastDownloaded;
                            lastDownloaded = raw;
                            if (diff > 0)
                            {
                                totalBytesRead += diff;
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

                            if (counter != null || counterRead != null)
                            {
                                using (counter) { }
                                using (counterRead) { }
                                counter = null;
                                counterRead = null;
                            }
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

            if (counter != null || counterRead != null)
            {
                using (counter) { }
                using (counterRead) { }
                counter = null;
                counterRead = null;
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
                if (proxy == null)
                {
                    if (cancelWatch != null && !cancelWatch.IsCancellationRequested)
                    {
                        cancelWatch.Cancel();
                        cancelWatch.Dispose();
                    }

                    cancelWatch = new CancellationTokenSource();
                    WatchDat(cancelWatch.Token);
                }

                //try
                //{
                //    if (p.HasExited)
                //        p = null;
                //}
                //catch  (Exception e)
                //{
                //    Util.Logging.Log(e);
                //    p = null;
                //}
            }

            //if (p == null)
            //{
            //}
            //else
            //{
            //    ShowCloseTooltip(p);
            //}
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
            
            if (Util.Invoke.IfRequiredAsync(this, 
                delegate
                {
                    Launcher_AccountProcessActivated(account, e);
                }))
            {
                return;
            }

            activeProcess = e;
            OnProcessChanged();
        }

        void Launcher_AccountStateChanged(Settings.IAccount account, Client.Launcher.AccountStateEventArgs e)
        {
            if (Util.Invoke.IfRequiredAsync(this,
                delegate
                {
                    Launcher_AccountStateChanged(account, e);
                }))
            {
                return;
            }

            AccountItem item;
            if (accounts.TryGetValue(account, out item))
            {
                timeout = DateTime.UtcNow;
                OnTimeoutChanged();

                switch (e.State)
                {
                    //case Client.Launcher.AccountState.Active:
                    //case Client.Launcher.AccountState.ActiveGame:
                    case Client.Launcher.AccountState.UpdatingVisible:
                    case Client.Launcher.AccountState.Updating:

                        activeAccount = item.Account;
                        var p = e.Data as Process;
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
                                OnComplete();
                            }
                        }

                        break;
                }
            }
        }

        //private /*async*/ void ShowCloseTooltip(Process p)
        //{
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
        //}

        private void OnComplete()
        {
            isComplete = true;

            if (cancelDelayedComplete != null)
            {
                cancelDelayedComplete.Cancel();
                cancelDelayedComplete.Dispose();
                cancelDelayedComplete = null;
            }

            if (cancelWatch != null)
            {
                cancelWatch.Cancel();
                cancelWatch.Dispose();
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

            if (cancelDelayedComplete != null && !cancelDelayedComplete.IsCancellationRequested)
            {
                cancelDelayedComplete.Cancel();
                cancelDelayedComplete.Dispose();
                cancelDelayedComplete = null;
            }

            if (cancelWatch != null && !cancelWatch.IsCancellationRequested)
            {
                cancelWatch.Cancel();
                cancelWatch.Dispose();
                cancelWatch = null;
            }

            if (!isComplete && remaining != 0)
            {
                result = DialogResult.Abort;
                if (remaining != total)
                    Client.Launcher.CancelAndKillActiveLaunches();
                else
                    Client.Launcher.CancelPendingLaunches();
            }

            this.DialogResult = result;
        }

        private void labelAbort_Click(object sender, EventArgs e)
        {
            labelAbort.Enabled = false;
            labelAbort.ForeColor = SystemColors.GrayText;
            this.result = DialogResult.Abort;
            //Client.Launcher.CancelPendingLaunches();
            //Client.Launcher.KillActiveLaunches();
            Client.Launcher.CancelAndKillActiveLaunches();
        }

        private void formUpdating_Load(object sender, EventArgs e)
        {

        }

        private void formUpdating_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
