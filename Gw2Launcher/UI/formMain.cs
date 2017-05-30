using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.UI.Controls;

namespace Gw2Launcher.UI
{
    public partial class formMain : Form
    {
        private bool autoSizeGrid;
        private NotifyIcon notifyIcon;

        private Dictionary<ushort, AccountGridButton> buttons;
        private formAccountTooltip tooltip;
        private formUpdating updatingWindow;
        private formAssetProxy assetProxyWindow;

        private byte activeWindows;
        private List<Form> windows;
        private FormWindowState windowState;

        private event EventHandler ActiveWindowChanged;

        private class DialogLock : IDisposable
        {
            private Action onDispose;
            public DialogLock(Action onDispose)
            {
                this.onDispose = onDispose;
            }
            public void Dispose()
            {
                lock (this)
                {
                    if (onDispose != null)
                    {
                        onDispose();
                        onDispose = null;
                    }
                }
            }
        }

        public formMain()
        {
            InitializeComponent();

            windows = new List<Form>();
            buttons = new Dictionary<ushort, AccountGridButton>();

            Client.Launcher.AccountStateChanged += Launcher_AccountStateChanged;
            Client.Launcher.LaunchException += Launcher_LaunchException;
            Client.Launcher.AccountLaunched += Launcher_AccountLaunched;
            Client.Launcher.AccountExited += Launcher_AccountExited;
            Client.Launcher.ActiveProcessCountChanged += Launcher_ActiveProcessCountChanged;
            Client.Launcher.BuildUpdated += Launcher_BuildUpdated;
            Client.Launcher.AccountQueued += Launcher_AccountQueued;

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(Properties.Resources.Gw2, SystemInformation.SmallIconSize);
            notifyIcon.Visible = !Settings.ShowTray.HasValue || Settings.ShowTray.Value;
            notifyIcon.Text = "Gw2 Launcher";
            notifyIcon.ContextMenuStrip = contextNotify;
            notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            
            gridContainer.AddAccountClick += gridContainer_AddAccountClick;
            gridContainer.AccountMouseClick += gridContainer_AccountMouseClick;

            contextMenu.Closed += contextMenu_Closed;

            Settings.ShowTray.ValueChanged += SettingsShowTray_ValueChanged;
            Settings.WindowBounds[typeof(formMain)].ValueChanged += SettingsWindowBounds_ValueChanged;
            Settings.ShowAccount.ValueChanged += ShowAccount_ValueChanged;
            Settings.FontLarge.ValueChanged += FontLarge_ValueChanged;
            Settings.FontSmall.ValueChanged += FontSmall_ValueChanged;

            windowState = this.WindowState;

            this.Resize += formMain_Resize;
            this.Shown += formMain_Shown;

            LoadAccounts();
            LoadSettings();

            Client.Launcher.Scan();
        }

        public void ShowPatchProxy()
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(ShowPatchProxy));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return;
            }

            var f = assetProxyWindow;

            if (f == null || f.IsDisposed)
            {
                assetProxyWindow = f = new formAssetProxy();

                if (this.WindowState != FormWindowState.Minimized)
                {
                    f.StartPosition = FormStartPosition.Manual;
                    var screen = Screen.FromControl(this);
                    int x = this.Location.X + this.Width + 5;
                    if (x >= screen.WorkingArea.Width / 2)
                        x = this.Location.X - f.Width - 5;
                    f.Location = new Point(x, this.Location.Y + this.Height / 2 - f.Height / 2);
                    f.DesktopBounds = Util.RectangleConstraint.ConstrainToScreen(f.DesktopBounds);
                }
                else
                    f.StartPosition = FormStartPosition.CenterScreen;

                f.FormClosing += delegate
                {
                    assetProxyWindow = null;
                };

                System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(
                    delegate
                    {
                        Application.Run(f);
                    }));

                t.IsBackground = true;
                t.SetApartmentState(System.Threading.ApartmentState.STA);
                t.Start();
            }
            else
            {
                try
                {

                    f.Invoke(new MethodInvoker(
                        delegate
                        {
                            var h = f.Handle;
                            if (h != IntPtr.Zero)
                            {
                                Windows.FindWindow.ShowWindow(h, 1);
                                Windows.FindWindow.SetForegroundWindow(h);
                                Windows.FindWindow.BringWindowToTop(h);
                            }
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        void Launcher_AccountQueued(Settings.IAccount account, Client.Launcher.LaunchMode e)
        {
            if (e == Client.Launcher.LaunchMode.Update || e == Client.Launcher.LaunchMode.UpdateVisible)
            {
                System.Threading.CancellationTokenSource cancel = new System.Threading.CancellationTokenSource(3000);

                try
                {
                    this.Invoke(new MethodInvoker(
                        delegate
                        {
                            if (updatingWindow != null && !updatingWindow.IsDisposed)
                            {
                                updatingWindow.AddAccount(account);
                                if (cancel != null)
                                    cancel.Cancel();
                                return;
                            }

                            List<Settings.IAccount> accounts = new List<Settings.IAccount>();
                            accounts.Add(account);

                            formUpdating f = updatingWindow = new formUpdating(accounts, true, true);

                            this.BeginInvoke(new MethodInvoker(
                                delegate
                                {
                                    using (BeforeShowDialog())
                                    {
                                        using (AddWindow(f))
                                        {
                                            f.Shown += delegate
                                            {
                                                if (cancel != null)
                                                    cancel.Cancel();
                                            };
                                            f.ShowDialog(this);
                                            updatingWindow = null;
                                        }
                                    }
                                }));
                        }));

                    cancel.Token.WaitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    cancel.Cancel();
                }
            }
        }

        void Launcher_BuildUpdated(Client.Launcher.BuildUpdatedEventArgs e)
        {
            HashSet<Settings.IDatFile> dats = new HashSet<Settings.IDatFile>();
            List<Settings.IAccount> accounts = new List<Settings.IAccount>();

            //only accounts with a unique dat file need to be updated

            foreach (ushort uid in Settings.Accounts.GetKeys())
            {
                var a = Settings.Accounts[uid];
                if (a.HasValue && (a.Value.DatFile == null || dats.Add(a.Value.DatFile)))
                {
                    accounts.Add(a.Value);
                }
            }

            if (accounts.Count > 0)
            {
                //System.Threading.CancellationTokenSource cancel = new System.Threading.CancellationTokenSource(3000);

                //try
                //{
                //    this.BeginInvoke(new MethodInvoker(
                //        delegate
                //        {
                //            BeforeShowDialog();
                //            try
                //            {
                //                activeWindows++;
                //                using (formUpdating f = new formUpdating(accounts, true, true))
                //                {
                //                    f.Shown += delegate
                //                    {
                //                        cancel.Cancel();
                //                    };
                //                    f.ShowDialog(this);
                //                }
                //            }
                //            finally
                //            {
                //                activeWindows--;
                //            }
                //        }));
                //}
                //catch { }

                //try
                //{
                //    cancel.Token.WaitHandle.WaitOne();
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}

                e.Update(accounts);
            }
        }

        private formWaiting ShowWaiting()
        {
            formWaiting w = new formWaiting(this);
            w.Owner = this;

            EventHandler onActiveChange = delegate
            {
                foreach (var form in windows)
                {
                    if (form.Owner == null || form.Owner == this)
                        form.Owner = w;
                }
                w.SendToBack();
            };

            w.Shown += delegate
            {
                onActiveChange(null, null);
                this.ActiveWindowChanged += onActiveChange;
            };

            w.Disposed += delegate
            {
                this.ActiveWindowChanged -= onActiveChange;
            };

            return w;
        }
        
        void formMain_Shown(object sender, EventArgs e)
        {
            this.Shown -= formMain_Shown;

            if (Settings.CheckForNewBuilds)
            {
                CheckBuild();
            }
        }

        private async void CheckBuild()
        {
            int build = await Tools.Gw2Build.GetBuildAsync();
            if (build > 0 && Settings.CheckForNewBuilds && Settings.LastKnownBuild.Value != build)
            {
                //ask to update if nothing else is happening
                if (activeWindows == 0 && Client.Launcher.GetPendingLaunchCount() == 0 && Client.Launcher.GetActiveProcessCount() == 0)
                {
                    HashSet<Settings.IDatFile> dats = new HashSet<Settings.IDatFile>();
                    List<Settings.IAccount> accounts = new List<Settings.IAccount>();

                    //only accounts with a unique dat file need to be updated

                    foreach (ushort uid in Settings.Accounts.GetKeys())
                    {
                        var a = Settings.Accounts[uid];
                        if (a.HasValue && (a.Value.DatFile == null || dats.Add(a.Value.DatFile)))
                        {
                            accounts.Add(a.Value);
                        }
                    }

                    if (accounts.Count > 0)
                    {
                        using (BeforeShowDialog())
                        {
                            using (var parent = AddMessageBox(this))
                            {
                                if (MessageBox.Show(parent, "Local.dat needs to be updated.\n\nWould you like to update now?", "Update required", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    Func<bool> hasActive = delegate
                                    {
                                        foreach (var account in accounts)
                                        {
                                            if (Client.Launcher.GetState(account) != Client.Launcher.AccountState.None)
                                                return true;
                                        }
                                        return false;
                                    };

                                    if (Client.Launcher.GetActiveProcessCount() > 0 || hasActive())
                                    {
                                        using (formWaiting f = ShowWaiting())
                                        {
                                            f.Shown += delegate
                                            {
                                                Task.Factory.StartNew(new Action(
                                                    delegate
                                                    {
                                                        int retries = 0;

                                                        while (Client.Launcher.GetActiveProcessCount() > 0 || hasActive())
                                                        {
                                                            if (retries++ >= 10)
                                                                break;

                                                            try
                                                            {
                                                                Client.Launcher.CancelPendingLaunches();
                                                                Client.Launcher.KillAllActiveProcesses();
                                                                System.Threading.Thread.Sleep(1000);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Util.Logging.Log(ex);
                                                            }
                                                        }
                                                        f.Invoke(new MethodInvoker(delegate
                                                        {
                                                            f.Close();
                                                        }));
                                                    }));
                                            };

                                            f.ShowDialog(this);
                                        }
                                    }

                                    Action launch = delegate
                                    {
                                        bool first = true;
                                        foreach (var account in accounts)
                                        {
                                            Client.Launcher.LaunchMode mode;
                                            if (first)
                                            {
                                                mode = Client.Launcher.LaunchMode.UpdateVisible;
                                                first = false;
                                            }
                                            else
                                                mode = Client.Launcher.LaunchMode.Update;
                                            Client.Launcher.Launch(account, mode);
                                        }
                                    };

                                    if (updatingWindow == null || updatingWindow.IsDisposed)
                                    {
                                        using (formUpdating f = updatingWindow = (formUpdating)AddWindow(new formUpdating(null, true, true)))
                                        {
                                            f.Shown += delegate
                                            {
                                                launch();
                                            };
                                            if (f.ShowDialog(this) == DialogResult.OK)
                                                Settings.LastKnownBuild.Value = build;
                                            updatingWindow = null;
                                        }
                                    }
                                    else
                                    {
                                        launch();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void contextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (selectedToolStripMenuItem.Tag == null)
                gridContainer.ClearSelected();
        }

        void Launcher_ActiveProcessCountChanged(object sender, int e)
        {
            if (e == 0 && Settings.BringToFrontOnExit.Value)
            {
                try
                {
                    this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            ShowToFront();
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        void formMain_Resize(object sender, EventArgs e)
        {
            if (windowState != this.WindowState)
            {
                windowState = this.WindowState;
                OnWindowStateChanged();
            }
        }

        void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                ShowToFront();
            }
            else
            {
                ShowToFront();
            }
        }

        private void OnWindowStateChanged()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (Settings.ShowTray.Value && Settings.MinimizeToTray.Value)
                    MinimizeToTray();
            }
        }

        private async void MinimizeToTray()
        {
            this.WindowState = FormWindowState.Minimized;

            await Task.Delay(200);

            this.Hide();
        }

        private async void ShowToFront()
        {
            if (!this.Visible)
                this.Show();

            await Task.Delay(10);

            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Focus();
        }

        void Launcher_AccountExited(Settings.IAccount account)
        {
            if (account.RecordLaunches)
            {
                Tools.Statistics.Record(Tools.Statistics.RecordType.Exited, account.UID);
            }
        }

        void Launcher_AccountLaunched(Settings.IAccount account)
        {
            account.TotalUses++;

            if (account.RecordLaunches)
            {
                Tools.Statistics.Record(Tools.Statistics.RecordType.Launched, account.UID);
            }

            if (Settings.DeleteCacheOnLaunch.Value)
            {
                Task.Factory.StartNew(new Action(
                   delegate
                   {
                       try
                       {
                           Tools.Gw2Cache.Delete(Util.Users.GetUserName(account.WindowsAccount));
                       }
                       catch (Exception ex)
                       {
                           Util.Logging.Log(ex);
                       }
                   }));
            }
        }

        void Launcher_LaunchException(Settings.IAccount account, Client.Launcher.LaunchExceptionEventArgs e)
        {
            if (e.Exception is Client.Launcher.InvalidGW2PathException)
            {
                string path = Settings.GW2Path.Value;

                if (this.InvokeRequired)
                {
                    try
                    {
                        this.Invoke(new MethodInvoker(
                            delegate
                            {
                                OnSettingsInvalid();
                            }));
                    }
                    catch (Exception ex) 
                    {
                        Util.Logging.Log(ex);
                    }
                }
                else
                {
                    OnSettingsInvalid();
                }

                if (Settings.GW2Path.Value != path)
                    e.Retry = true;
            }
            else if (e.Exception is Client.Launcher.BadUsernameOrPasswordException)
            {
                string username = Util.Users.GetUserName(account.WindowsAccount);
                if (this.InvokeRequired)
                {
                    try
                    {
                        this.Invoke(new MethodInvoker(
                            delegate
                            {
                                e.Retry = OnPasswordRequired(username) == DialogResult.OK;
                            }));
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
                else
                {
                    e.Retry = OnPasswordRequired(username) == DialogResult.OK;
                }
            }
            else if (e.Exception is Client.Launcher.DatFileNotInitialized)
            {
                if (this.InvokeRequired)
                {
                    try
                    {
                        this.Invoke(new MethodInvoker(
                            delegate
                            {
                                e.Retry = OnDatFileNotInitialized(account) == DialogResult.OK;
                            }));
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
                else
                {
                    e.Retry = OnDatFileNotInitialized(account) == DialogResult.OK;
                }
            }
        }

        private DialogResult OnDatFileNotInitialized(Settings.IAccount account)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddMessageBox(this))
                {
                    DialogResult r = MessageBox.Show(f, account.Name + "\n\nThe Local.dat file for this account is being used for the first time. GW2 will not be able to modify your settings while allowing multiple clients to be opened.\n\nWould you like to run GW2 normally so that your settings can be modified?", "Local.dat first use", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                    switch (r)
                    {
                        case DialogResult.Yes:
                            return DialogResult.OK;
                        case DialogResult.No:
                            account.DatFile.IsInitialized = true;
                            return DialogResult.OK;
                        default:
                            return DialogResult.Cancel;
                    }
                }
            }
        }

        private DialogResult OnPasswordRequired(string username)
        {
            using (BeforeShowDialog())
            {
                using (formPassword f = (formPassword)AddWindow(new formPassword("Password for " + username + ":")))
                {
                    while (true)
                    {
                        if (f.ShowDialog(this) != DialogResult.OK || f.Password.Length == 0)
                        {
                            using (var parent = AddMessageBox(this))
                            {
                                if (MessageBox.Show(parent, "A password is required to use this account", "Password required", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Retry)
                                    return DialogResult.Cancel;
                            }
                        }
                        else
                        {
                            Security.Credentials.SetPassword(username, f.Password);
                            return DialogResult.OK;
                        }
                    }
                }
            }
        }

        private void OnSettingsInvalid()
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formSettings()))
                {
                    f.ShowDialog(this);
                }
            }
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

            AccountGridButton button;
            if (buttons.TryGetValue(uid, out button))
            {
                if (button.Tag != data)
                {
                    button.Tag = data;
                    OnButtonDataChanged(button, data);
                }

                if (button.AccountData != null)
                {
                    bool exited = previousState == Client.Launcher.AccountState.ActiveGame;

                    //as a backup in case catching the DX window failed, catch the exit time
                    if (previousState == Client.Launcher.AccountState.Active && state == Client.Launcher.AccountState.Exited && data is TimeSpan)
                    {
                        TimeSpan elapsed = (TimeSpan)data;
                        if (elapsed != TimeSpan.MinValue)
                        {
                            if (elapsed.TotalMinutes > 10)
                            {
                                exited = true;
                            }
                        }
                    }

                    if (exited)
                    {
                        DateTime d = DateTime.UtcNow;
                        if (button.AccountData != null)
                            button.AccountData.LastUsedUtc = d;
                        button.LastUsedUtc = d;
                        if (Settings.SortingMode.Value == Settings.SortMode.LastUsed)
                            gridContainer.Sort(Settings.SortingMode.Value, Settings.SortingOrder.Value);
                    }
                }

                switch (state)
                {
                    case Client.Launcher.AccountState.Active:
                        button.SetStatus("active", Color.DarkGreen);
                        break;
                    case Client.Launcher.AccountState.ActiveGame:
                        button.SetStatus("active", Color.DarkGreen);
                        DateTime d = DateTime.UtcNow;
                        if (button.AccountData != null)
                            button.AccountData.LastUsedUtc = d;
                        button.LastUsedUtc = d;
                        if (Settings.SortingMode.Value == Settings.SortMode.LastUsed)
                            gridContainer.Sort(Settings.SortingMode.Value, Settings.SortingOrder.Value);
                        break;
                    case Client.Launcher.AccountState.Waiting:
                        button.SetStatus("waiting", Color.DarkBlue);
                        break;
                    case Client.Launcher.AccountState.Launching:
                        button.SetStatus("launching", Color.DarkBlue);
                        break;
                    case Client.Launcher.AccountState.None:
                        button.SetStatus(null, Color.Empty);
                        break;
                    case Client.Launcher.AccountState.Updating:
                    case Client.Launcher.AccountState.UpdatingVisible:
                        button.SetStatus("updating", Color.DarkBlue);
                        break;
                    case Client.Launcher.AccountState.WaitingForOtherProcessToExit:
                        button.SetStatus("close GW2 to continue", Color.DarkBlue);
                        break;
                    case Client.Launcher.AccountState.Error:
                        button.SetStatus("failed", Color.DarkRed);
                        break;
                }
            }
        }

        void OnButtonDataChanged(AccountGridButton button, object data)
        {
            if (button.IsHovered)
            {
                if (data is Exception)
                    ShowTooltip(button, ((Exception)data).Message);
                else if (tooltip != null && tooltip.Visible)
                    tooltip.HideTooltip();
            }
        }

        void SettingsShowTray_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                bool showTray=!Settings.ShowTray.HasValue || Settings.ShowTray.Value;
                notifyIcon.Visible = showTray;
                if (!showTray && !this.Visible)
                {
                    this.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private void OnStyleChanged()
        {
            Font large = Settings.FontLarge.HasValue ? Settings.FontLarge.Value : UI.Controls.AccountGridButton.FONT_LARGE;
            Font small = Settings.FontSmall.HasValue ? Settings.FontSmall.Value : UI.Controls.AccountGridButton.FONT_SMALL;
            bool showAccount = !Settings.ShowAccount.HasValue || Settings.ShowAccount.Value;

            gridContainer.SetStyle(large, small, showAccount);
        }

        void FontSmall_ValueChanged(object sender, EventArgs e)
        {
            OnStyleChanged();
        }

        void FontLarge_ValueChanged(object sender, EventArgs e)
        {
            OnStyleChanged();
        }

        void ShowAccount_ValueChanged(object sender, EventArgs e)
        {
            OnStyleChanged();
        }

        void SettingsWindowBounds_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<Rectangle>;
            if (!setting.HasValue)
            {
                AutoSizeGrid = true;

                var bounds = Screen.PrimaryScreen.WorkingArea;
                this.Location = Point.Add(bounds.Location, new Size(bounds.Width / 2 - this.Size.Width / 2, bounds.Height / 3));
            }
            else if (setting.Value.Size.IsEmpty)
            {
                AutoSizeGrid = true;
            }
        }

        private void LoadSettings()
        {
            OnStyleChanged();

            if (!(Settings.SortingMode.Value == Settings.SortMode.None && Settings.SortingOrder.Value == Settings.SortOrder.Ascending))
            {
                SetSorting(Settings.SortingMode.Value, Settings.SortingOrder.Value, true);
            }
        }

        private void LoadAccounts()
        {
            foreach (var uid in Settings.Accounts.GetKeys())
            {
                var account = Settings.Accounts[uid];
                if (account.HasValue)
                {
                    AddAccount(account.Value, false);
                }
            }
        }

        public bool AutoSizeGrid
        {
            get
            {
                return autoSizeGrid;
            }
            set
            {
                if (autoSizeGrid != value)
                {
                    autoSizeGrid = value;
                    if (value)
                    {
                        gridContainer.ContentHeightChanged += gridContainer_ContentHeightChanged;
                        ResizeAuto();
                    }
                    else
                        gridContainer.ContentHeightChanged -= gridContainer_ContentHeightChanged;
                }
            }
        }

        void gridContainer_ContentHeightChanged(object sender, EventArgs e)
        {
            ResizeAuto();
        }

        protected void ResizeAuto()
        {
            var screen = Screen.FromControl(this).WorkingArea;
            int height = gridContainer.ContentHeight + (this.Height - this.ClientSize.Height) + panelContainer.Location.Y * 2 + gridContainer.Location.Y * 2 + 2;
            int width = 250 + (this.Width - this.ClientSize.Width) + panelContainer.Location.X * 2 + gridContainer.Location.X * 2 + 2;

            if (this.Location.Y + height > screen.Bottom)
            {
                height = screen.Bottom - this.Location.Y;
                if (height < 100)
                    height = 100;
            }

            this.Size = new Size(width, height);
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);

            AutoSizeGrid = false;

            Rectangle bounds = this.Bounds;
            EventHandler resizeEnd = null;
            resizeEnd = new EventHandler(
                 delegate
                 {
                     this.ResizeEnd -= resizeEnd;

                     var setting = Settings.WindowBounds[typeof(formMain)];
                     Point location = this.Location;
                     Size size = this.Size;
                     bool changed = false;

                     if (bounds.Size.Equals(size))
                     {
                         if (setting.HasValue)
                             size = setting.Value.Size;
                         else
                             size = Size.Empty;
                     }
                     else
                         changed = true;

                     if (bounds.Location.Equals(location))
                     {
                         //if (setting.HasValue)
                         //    location = setting.Value.Location;
                         //else
                         //    location = new Point(int.MinValue, int.MinValue);
                     }
                     else
                         changed = true;

                     if (changed)
                         setting.Value = new Rectangle(location, size);
                 });

            this.ResizeEnd += resizeEnd;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                //if any recorded accounts are still active, add a record for exiting the program (ID 0)
                var active = Client.Launcher.GetActiveProcesses();
                foreach (var account in active)
                {
                    if (account.RecordLaunches)
                    {
                        Tools.Statistics.Record(Tools.Statistics.RecordType.Exited, 0);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            notifyIcon.Dispose();

            Settings.Save();
            Tools.Statistics.Save();
        }

        void gridContainer_AddAccountClick(object sender, EventArgs e)
        {
            AddAccount();
        }

        void gridContainer_AccountMouseClick(object sender, MouseEventArgs e)
        {
            var button = (AccountGridButton)sender;

            if (e.Button == MouseButtons.Right)
            {
                if (!button.Selected)
                {
                    gridContainer.ClearSelected();
                    button.Selected = true;

                    clearSelectionToolStripMenuItem.Enabled = false;
                    selectedToolStripMenuItem.Tag = null;

                    applyWindowedBoundsToolStripMenuItem.Enabled = false;
                    var a = button.AccountData;
                    if (a != null && a.Windowed && !a.WindowBounds.IsEmpty)
                    {
                        var state = Client.Launcher.GetState(a);
                        if (state == Client.Launcher.AccountState.Active || state == Client.Launcher.AccountState.ActiveGame)
                            applyWindowedBoundsToolStripMenuItem.Enabled = true;
                    }
                }
                else
                {
                    var selected = gridContainer.GetSelected();
                    clearSelectionToolStripMenuItem.Enabled = true;
                    selectedToolStripMenuItem.Tag = selected;

                    applyWindowedBoundsToolStripMenuItem.Enabled = false;
                    foreach (var b in selected)
                    {
                        var a = b.AccountData;
                        if (a != null && a.Windowed && !a.WindowBounds.IsEmpty)
                        {
                            var state = Client.Launcher.GetState(a);
                            if (state == Client.Launcher.AccountState.Active || state == Client.Launcher.AccountState.ActiveGame)
                            {
                                applyWindowedBoundsToolStripMenuItem.Enabled = true;
                                break;
                            }
                        }
                    }
                }

                int pending = Client.Launcher.GetPendingLaunchCount();

                cancelPendingLaunchesToolStripMenuItem.Visible = pending > 0;
                toolStripMenuItemCancelSep.Visible = pending > 0;

                contextMenu.Tag = button;

                contextMenu.Show(Cursor.Position);
            }
            else if (e.Button == MouseButtons.Left)
            {
                gridContainer.ClearSelected();
                Client.Launcher.Launch(button.AccountData, Client.Launcher.LaunchMode.Launch);
            }
        }

        private void formMain_Load(object sender, EventArgs e)
        {
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formSettings()))
                {
                    f.ShowDialog(this);
                }
            }
        }

        private Form AddMessageBox(Form owner)
        {
            Form form = new Form();
            form.Owner = owner;

            form.Disposed += OnWindowHidden;

            //form.Disposed += delegate
            //{
            //    for (var i = windows.Count - 1; i >= 0; i--)
            //    {
            //        if (windows[i] == form)
            //        {
            //            windows.RemoveAt(i);
            //            break;
            //        }
            //    }
            //};

            windows.Add(form);
            OnActiveWindowChanged();

            return form;
        }

        private void OnWindowVisibleChanged(object sender, EventArgs e)
        {
            var form = (Form)sender;

            if (form.Visible)
            {
                windows.Add(form);
                form.Disposed += OnWindowHidden;

                OnActiveWindowChanged();
            }
            else
            {
                OnWindowHidden(sender, e);
            }
        }

        private void OnWindowHidden(object sender, EventArgs e)
        {
            var form = (Form)sender;
            
            form.Disposed -= OnWindowHidden;

            for (var i = windows.Count - 1; i >= 0; i--)
            {
                if (windows[i] == form)
                {
                    windows.RemoveAt(i);
                    break;
                }
            }
            
            OnActiveWindowChanged();
        }

        private Form AddWindow(Form form)
        {
            form.VisibleChanged += OnWindowVisibleChanged;

            return form;
        }

        private IDisposable BeforeShowDialog()
        {
            if (!this.Visible || this.WindowState == FormWindowState.Minimized)
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Visible = true;
                    this.WindowState = FormWindowState.Normal;
                    this.Visible = false;
                }
                this.Visible = true;
            }

            activeWindows++;
            OnActiveWindowsChanged();

            return new DialogLock(OnDialogLockDisposed);
        }

        private void OnDialogLockDisposed()
        {
            activeWindows--;
            OnActiveWindowsChanged();
        }

        private void OnActiveWindowChanged()
        {
            if (ActiveWindowChanged != null)
                ActiveWindowChanged(this, EventArgs.Empty);
        }

        private void OnActiveWindowsChanged()
        {
            bool isFree = activeWindows == 0;
            toolsToolStripMenuItem.Enabled = isFree;
            allToolStripMenuItem1.Enabled = isFree;
            newAccountToolStripMenuItem.Enabled = isFree;
            toolsToolStripMenuItem.Enabled = isFree;
            settingsToolStripMenuItem1.Enabled = isFree;
        }

        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formSettings()))
                {
                    f.ShowDialog(this);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddAccount();
        }

        private void newAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.Visible || this.CanFocus)
                AddAccount();
        }

        private void RemoveAccount(Settings.IAccount account)
        {
            AccountGridButton button;
            if (buttons.TryGetValue(account.UID, out button))
            {
                buttons.Remove(account.UID);
                gridContainer.Remove(button);
            }

            Settings.Accounts[account.UID].Clear();
        }

        private void AddAccount()
        {
            using (BeforeShowDialog())
            {
                using (formAccount f = (formAccount)AddWindow(new formAccount()))
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        AddAccount(f.Account, true);
                    }
                }
            }
        }

        private void AddAccount(Settings.IAccount account, bool sort)
        {
            var button = new AccountGridButton();
            button.AccountData = account;
            if (string.IsNullOrEmpty(account.WindowsAccount))
                button.AccountName = "(current user)";

            button.MouseEnter += button_MouseEnter;
            button.MouseLeave += button_MouseLeave;

            buttons.Add(account.UID, button);

            gridContainer.Add(button);

            if (sort && !(Settings.SortingMode.Value == Settings.SortMode.None && Settings.SortingOrder.Value == Settings.SortOrder.Ascending))
                gridContainer.Sort(Settings.SortingMode.Value, Settings.SortingOrder.Value);
        }

        void button_MouseLeave(object sender, EventArgs e)
        {
            var button = (AccountGridButton)sender;
            if (tooltip != null && tooltip.Tag == button)
            {
                tooltip.HideTooltip();
                tooltip.Tag = null;
            }
        }

        private void ShowTooltip(Control control, string message)
        {
            if (tooltip == null)
                tooltip = new formAccountTooltip();

            tooltip.AttachTo(control, null, -8);
            tooltip.Show(this, message, 1000);
            tooltip.Tag = control;
        }

        void button_MouseEnter(object sender, EventArgs e)
        {
            var button = (AccountGridButton)sender;
            if (button.Tag is Exception)
            {
                var ex = (Exception)button.Tag;
                string message = ex.Message;

                ShowTooltip(button, message);
            }
        }

        private void SetSorting(Settings.SortMode mode, Settings.SortOrder order, bool applyNow)
        {
            Settings.SortingMode.Value = mode;
            Settings.SortingOrder.Value = order;

            nameToolStripMenuItem.Checked = mode == Settings.SortMode.Name;
            windowsAccountToolStripMenuItem.Checked = mode == Settings.SortMode.Account;
            lastUsedToolStripMenuItem.Checked = mode == Settings.SortMode.LastUsed;

            ascendingToolStripMenuItem.Checked = order == Settings.SortOrder.Ascending;
            descendingToolStripMenuItem.Checked = order == Settings.SortOrder.Descending;

            if (applyNow)
                gridContainer.Sort(mode, order);
        }

        private void nameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnSortingModeClick(sender);
        }

        private void windowsAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnSortingModeClick(sender);
        }

        private void lastUsedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnSortingModeClick(sender);
        }

        private void OnSortingModeClick(object sender)
        {
            Settings.SortMode mode;

            if (sender == nameToolStripMenuItem)
                mode = Settings.SortMode.Name;
            else if (sender == windowsAccountToolStripMenuItem)
                mode = Settings.SortMode.Account;
            else if (sender == lastUsedToolStripMenuItem)
                mode = Settings.SortMode.LastUsed;
            else
                return;

            if (Settings.SortingMode.Value == mode)
                mode = Settings.SortMode.None;

            SetSorting(mode, Settings.SortingOrder.Value, true);
        }

        private void ascendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSorting(Settings.SortingMode.Value, Settings.SortOrder.Ascending, true);
        }

        private void descendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSorting(Settings.SortingMode.Value, Settings.SortOrder.Descending, true);
        }

        private void formMain_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridContainer.ClearSelected();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AccountGridButton button = contextMenu.Tag as AccountGridButton;
            if (button != null && button.AccountData != null)
            {
                using (BeforeShowDialog())
                {
                    var account = button.AccountData;
                    using (formAccount f = (formAccount)AddWindow(new formAccount(account)))
                    {
                        var datBefore = account.DatFile;

                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            if (datBefore != null && datBefore != account.DatFile && !string.IsNullOrEmpty(datBefore.Path) && System.IO.File.Exists(datBefore.Path))
                            {
                                if (Util.DatFiles.GetAccounts(datBefore).Count == 0)
                                {
                                    using (var parent = AddMessageBox(this))
                                    {
                                        if (MessageBox.Show(parent, "The following Local.dat file is no longer being used:\n\n" + datBefore.Path + "\n\nWould you like to delete it?", "Local.dat not used", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                                        {
                                            try
                                            {
                                                Client.DatManager.Delete(datBefore);
                                            }
                                            catch (Exception ex)
                                            {
                                                Util.Logging.Log(ex);
                                            }
                                        }
                                    }

                                    Settings.DatFiles[datBefore.UID].Clear();
                                }
                            }

                            button.AccountData = account;
                            if (string.IsNullOrEmpty(account.WindowsAccount))
                                button.AccountName = "(current user)";
                        }
                    }
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AccountGridButton button = contextMenu.Tag as AccountGridButton;
            if (button != null)
            {
                var account = button.AccountData;
                if (account != null)
                {
                    using (BeforeShowDialog())
                    {
                        using (var parent = AddMessageBox(this))
                        {
                            if (MessageBox.Show(parent, "Are you sure you want to delete \"" + account.Name + "\"?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            {
                                if (account.DatFile != null && !string.IsNullOrEmpty(account.DatFile.Path) && System.IO.File.Exists(account.DatFile.Path))
                                {
                                    if (Util.DatFiles.GetAccounts(account.DatFile).Count == 1)
                                    {
                                        if (MessageBox.Show(parent, "The following Local.dat file is no longer being used:\n\n" + account.DatFile.Path + "\n\nWould you like to delete it?", "Local.dat not used", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                                        {
                                            try
                                            {
                                                Client.DatManager.Delete(account.DatFile);
                                            }
                                            catch (Exception ex)
                                            {
                                                Util.Logging.Log(ex);
                                            }
                                        }

                                        Settings.DatFiles[account.DatFile.UID].Clear();
                                    }
                                }
                                RemoveAccount(account);
                            }
                        }
                    }
                }
                else
                {
                    gridContainer.Remove(button);
                }
            }
        }

        private void updateAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateAccounts(buttons.Values);
        }

        private void UpdateAccounts(IEnumerable<AccountGridButton> buttons)
        {
            List<Settings.IAccount> accounts = null;

            foreach (var button in buttons)
            {
                var account = button.AccountData;
                if (account != null)
                {
                    if (accounts == null)
                        accounts = new List<Settings.IAccount>();

                    accounts.Add(account);
                }
            }

            if (accounts != null && accounts.Count > 0)
            {
                Func<bool> hasActive = delegate
                {
                    foreach (var account in accounts)
                    {
                        if (Client.Launcher.GetState(account) != Client.Launcher.AccountState.None)
                            return true;
                    }
                    return false;
                };

                while (Client.Launcher.GetActiveProcessCount() > 0 || hasActive())
                {
                    using (BeforeShowDialog())
                    {
                        using (var parent = AddMessageBox(this))
                        {
                            if (MessageBox.Show(parent, "Guild Wars 2 cannot be updated while allowing multiple clients to be opened.\n\nClose all clients to continue.", "Close GW2 to continue", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Retry)
                            {
                                using (formWaiting f = ShowWaiting())
                                {
                                    f.Shown += delegate
                                    {
                                        Task.Factory.StartNew(new Action(
                                            delegate
                                            {
                                                int retries = 0;

                                                while (Client.Launcher.GetActiveProcessCount() > 0 || hasActive())
                                                {
                                                    if (retries++ >= 10)
                                                        break;

                                                    try
                                                    {
                                                        Client.Launcher.CancelPendingLaunches();
                                                        Client.Launcher.KillAllActiveProcesses();
                                                        System.Threading.Thread.Sleep(1000);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Util.Logging.Log(ex);
                                                    }
                                                }
                                                f.Invoke(new MethodInvoker(delegate
                                                {
                                                    f.Close();
                                                }));
                                            }));
                                    };

                                    f.ShowDialog(this);
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }

                Action launch = delegate
                {
                    bool first = true;
                    foreach (var account in accounts)
                    {
                        Client.Launcher.LaunchMode mode;
                        if (first)
                        {
                            mode = Client.Launcher.LaunchMode.UpdateVisible;
                            first = false;
                        }
                        else
                            mode = Client.Launcher.LaunchMode.Update;
                        Client.Launcher.Launch(account, mode);
                    }
                };

                if (updatingWindow == null || updatingWindow.IsDisposed)
                {
                    using (formUpdating f = updatingWindow = (formUpdating)AddWindow(new formUpdating(null, true, true)))
                    {
                        f.Shown += delegate
                        {
                            launch();
                        };
                        f.ShowDialog(this);
                        updatingWindow = null;
                    }
                }
                else
                {
                    launch();
                }
            }
        }

        private void updateSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateAccounts(GetSelected());
        }

        private IEnumerable<Controls.AccountGridButton> GetSelected()
        {
            var selected = gridContainer.GetSelected();
            if (selected.Count == 0)
                return new Controls.AccountGridButton[] { contextMenu.Tag as Controls.AccountGridButton };
            else
                return selected;
        }

        private void launchSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var button in GetSelected())
            {
                var account = button.AccountData;
                if (account != null)
                {
                    Client.Launcher.Launch(account, Client.Launcher.LaunchMode.Launch);
                }
            }
        }

        private void updateLocaldatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var button in buttons.Values)
            {
                var account = button.AccountData;
                if (account != null)
                {
                    Client.Launcher.Launch(account, Client.Launcher.LaunchMode.LaunchSingle);
                }
            }
        }

        private void updateLocaldatSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var button in GetSelected())
            {
                var account = button.AccountData;
                if (account != null)
                {
                    Client.Launcher.Launch(account, Client.Launcher.LaunchMode.LaunchSingle);
                }
            }
        }

        private void launchAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var button in buttons.Values)
            {
                var account = button.AccountData;
                if (account != null)
                {
                    Client.Launcher.Launch(account, Client.Launcher.LaunchMode.Launch);
                }
            }
        }

        private void launchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            launchAllToolStripMenuItem_Click(sender, e);
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateAllToolStripMenuItem_Click(sender, e);
        }

        private void updateLocaldatToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            updateLocaldatToolStripMenuItem_Click(sender, e);
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowToFront();
        }

        private void cancelPendingLaunchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client.Launcher.CancelPendingLaunches();
        }

        private void deleteGw2cacheFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formGw2Cache()))
                {
                    f.ShowDialog(this);
                }
            }
        }

        private void hideUserAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formManagedInactiveUsers()))
                {
                    f.ShowDialog(this);
                }
            }
        }

        private void killAllGW2ProcessesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.GW2Path.Value))
                return;

            using (BeforeShowDialog())
            {
                using (var f = AddMessageBox(this))
                {
                    if (MessageBox.Show(f, "Attempt to kill all processes with the following path?\n\n\"" + Settings.GW2Path.Value + "\"", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        Client.Launcher.KillAllActiveProcesses();
                    }
                }
            }
        }
        
        private async void killMutexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var parent = AddMessageBox(this))
                {
                    if (MessageBox.Show(parent, "All processes will be scanned in an attempt to find the mutex that prevents Guild Wars 2 from opening multiple times. This should only be needed if another GW2 client is active under an unknown name.\n\nThis may take a minute. Are you sure?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        using (formWaiting f = ShowWaiting())
                        {
                            f.Show(this);

                            await Task.Factory.StartNew(new Action(
                                delegate
                                {
                                    try
                                    {
                                        Util.ProcessUtil.KillMutexWindow();
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }));
                        }
                    }
                }
            }
        }

        private void deleteCacheFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteGw2cacheFoldersToolStripMenuItem_Click(sender, e);
        }

        private void hideUserAccountsToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            hideUserAccountsToolStripMenuItem_Click(sender, e);
        }

        private void killAllActiveProcessesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            killAllGW2ProcessesToolStripMenuItem_Click(sender, e);
        }

        private void killMutexToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            killMutexToolStripMenuItem_Click(sender, e);
        }

        private async void applyWindowedBoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selected = GetSelected();

            await Task.Factory.StartNew(new Action(
                delegate
                {
                    try
                    {
                        foreach (var button in selected)
                        {
                            var account = button.AccountData;
                            if (account != null /*&& account.Windowed && !account.WindowBounds.IsEmpty && Client.Launcher.GetState(account) == Client.Launcher.AccountState.Active*/)
                            {
                                Client.Launcher.ApplyWindowedBounds(account);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }));
        }

        private void supportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formSupport()))
                {
                    f.ShowDialog(this);
                }
            }
        }

        private void assetProxyServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPatchProxy();
        }

        private void assetServerInterceptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPatchProxy();
        }
    }
}
