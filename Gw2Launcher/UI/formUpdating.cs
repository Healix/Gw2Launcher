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
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private Dictionary<ushort, AccountItem> accounts;
        private int remaining, total;

        private string datPath;
        private long datSize, bytesChanged;
        private DateTime datLastWrite;
        private CancellationTokenSource cancelWatch;
        private int timeout;
        private Process activeProcess;
        private Settings.IAccount activeAccount;
        private Point lastPosition;

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

            this.result = DialogResult.OK;
            this.closeOnCompletion = closeOnCompletion;
            this.accounts = new Dictionary<ushort, AccountItem>();
            this.total = accounts.Count * 2;
            this.remaining = total;

            Settings.GW2Path.ValueChanged += GW2Path_ValueChanged;
            GW2Path_ValueChanged(null, EventArgs.Empty);

            labelName.Text = "Waiting...";

            long size;
            DateTime lastWrite;
            try
            {
                FileInfo fi = new FileInfo(datPath);
                size = fi.Length;
                lastWrite = fi.LastWriteTimeUtc;
            }
            catch
            {
                size = 0;
                lastWrite = DateTime.MinValue;
            }

            datSize = size;
            datLastWrite = lastWrite;
            timeout = Environment.TickCount;
            labelTotalSize.Text = size > 0 ? FormatBytes(size) : "---";
            labelDifference.Text = "---";

            progressUpdating.Maximum = total;

            foreach (var account in accounts)
            {
                this.accounts[account.UID] = new AccountItem(account);
            }

            Client.Launcher.AccountStateChanged += Launcher_AccountStateChanged;
            Client.Launcher.AccountProcessChanged += Launcher_AccountProcessChanged;

            cancelWatch = new CancellationTokenSource();
            WatchDat(cancelWatch.Token);

            if (!alreadyQueued)
                this.Shown += formUpdating_Shown;
        }

        public formUpdating(List<Settings.IAccount> accounts)
            : this(accounts, false, false)
        {
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
                catch
                {
                    datSize = 0;
                    datLastWrite = DateTime.MinValue;
                }
            }
        }

        private async void WatchDat(CancellationToken token)
        {
            bool watching = true;

            do
            {
                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException)
                {
                    watching = false;
                }


                long size;
                DateTime lastWrite;
                try
                {
                    FileInfo fi = new FileInfo(datPath);
                    size = fi.Length;
                    lastWrite = fi.LastWriteTimeUtc;
                }
                catch
                {
                    size = 0;
                    lastWrite = DateTime.MinValue;
                }
                int t = Environment.TickCount;

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
                        labelTotalSize.Text = FormatBytes(size);
                        labelDifference.Text = FormatBytes(bytesChanged);
                    }
                }
                else if (datLastWrite != lastWrite)
                {
                    datLastWrite = lastWrite;
                    timeout = t;
                    OnTimeoutChanged();
                }
                else if (t - timeout > 60000)
                {
                    if (!panelWarning.Visible)
                    {
                        panelWarning.BringToFront();
                        panelWarning.Visible = true;
                    }
                }
            }
            while (watching);
        }

        private void OnTimeoutChanged()
        {
            if (panelWarning.Visible)
                panelWarning.Visible = false;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes > 858993459) //0.8 GB
            {
                return string.Format("{0:0.##} GB", bytes / 1073741824d);
            }
            else if (bytes > 838860) //0.8 MB
            {
                return string.Format("{0:0.##} MB", bytes / 1048576d);
            }
            else if (bytes > 819) //0.8 KB
            {
                return string.Format("{0:0.##} KB", bytes / 1024d);
            }
            else
            {
                return bytes + " bytes";
            }
        }

        private void OnProcessChanged()
        {
            var p = activeProcess;

            if (p != null)
            {
                try
                {
                    if (p.HasExited)
                        p = null;
                }
                catch 
                {
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

        void Launcher_AccountProcessChanged(Settings.IAccount account, Process e)
        {
            if (activeAccount != account)
                return;

            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            Launcher_AccountProcessChanged(account, e);
                        }));
                }
                catch { }
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
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            Launcher_AccountStateChanged(uid, state, previousState, data);
                        }));
                }
                catch { }
                return;
            }

            AccountItem item;
            if (accounts.TryGetValue(uid,out item))
            {
                timeout = Environment.TickCount;
                OnTimeoutChanged();

                switch (state)
                {
                    case Client.Launcher.AccountState.Active:
                    case Client.Launcher.AccountState.UpdatingVisible:
                    case Client.Launcher.AccountState.Updating:

                        if (item.State == 0)
                        {
                            if (state == Client.Launcher.AccountState.UpdatingVisible)
                            {
                                activeAccount = item.Account;
                                activeProcess = data as Process;
                                OnProcessChanged();
                            }

                            labelName.Text = "Updating " + item.Account.Name;
                            item.State = 1;
                            remaining--;
                            progressUpdating.Value = total - remaining;
                        }

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

                        if (item.State < 2)
                        {
                            if (item.State == 0)
                                remaining--;
                            item.State = 2;
                            remaining--;
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

        private async void ShowCloseTooltip(Process p)
        {
            //gw2 will automatically close once the update is complete, so this isn't really needed
            return;

            while (!p.HasExited)
            {
                try
                {
                    double ms = DateTime.Now.Subtract(p.StartTime).TotalMilliseconds;
                    if (ms > 3000)
                        break;
                    else
                    {
                        await Task.Delay(3001 - (int)ms);
                    }
                }
                catch 
                {
                    return;
                }
            }

            using (formAccountTooltip t = new formAccountTooltip())
            {
                t.Show(null, "Close Guild Wars 2 once the update is complete", 0);

                bool setOwner = false;

                while (p != null && t != null && !t.IsDisposed)
                {
                    if (t.Visible && t.WindowState == FormWindowState.Normal)
                    {
                        try
                        {
                            Rectangle r = Windows.WindowSize.GetWindowRect(p);

                            if (!setOwner)
                            {
                                try
                                {
                                    int h = p.MainWindowHandle.ToInt32();
                                    if (h != 0)
                                    {
                                        SetWindowLong(t.Handle, -8, h);
                                        setOwner = true;
                                    }
                                }
                                catch { }
                            }

                            if (r.X != lastPosition.X || r.Y != lastPosition.Y)
                            {
                                lastPosition = new Point(r.X, r.Y);
                                t.AttachTo(new Rectangle(r.Right - 211, r.Y + 176, 17, 17), 5, AnchorStyles.Right);
                                t.BringToFront();
                            }
                        }
                        catch
                        {
                            return;
                        }
                    }
                    await Task.Delay(100);
                }
            }
        }

        private void OnComplete()
        {
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

            if (cancelWatch != null)
                cancelWatch.Cancel();

            if (remaining != 0)
            {
                result = DialogResult.Abort;
                Client.Launcher.CancelPendingLaunches();
                Client.Launcher.KillActiveLaunches();
            }

            Settings.GW2Path.ValueChanged -= GW2Path_ValueChanged;
            Client.Launcher.AccountStateChanged -= Launcher_AccountStateChanged;
            Client.Launcher.AccountProcessChanged -= Launcher_AccountProcessChanged;

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
