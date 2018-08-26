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
        private bool canShow, initialized;
        private DateTime lastCacheDelete;

        private Dictionary<ushort, AccountGridButton> buttons;
        private formAccountTooltip tooltip;
        private formUpdating updatingWindow;
        private formAssetProxy assetProxyWindow;
        private formBackgroundPatcher bpWindow;

        private byte activeWindows;
        private List<Form> windows;
        private FormWindowState windowState;
        
        private Windows.DragHelper.DragHelperInstance dragHelper;

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
            canShow = true;

            disableAutomaticLoginsToolStripMenuItem1.Tag = 0;
            applyWindowedBoundsToolStripMenuItem1.Tag = 0;

            Client.Launcher.AccountStateChanged += Launcher_AccountStateChanged;
            Client.Launcher.LaunchException += Launcher_LaunchException;
            Client.Launcher.AccountLaunched += Launcher_AccountLaunched;
            Client.Launcher.AccountExited += Launcher_AccountExited;
            Client.Launcher.ActiveProcessCountChanged += Launcher_ActiveProcessCountChanged;
            Client.Launcher.BuildUpdated += Launcher_BuildUpdated;
            Client.Launcher.AccountQueued += Launcher_AccountQueued;

            contextNotify.Opening += contextNotify_Opening;

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(Properties.Resources.Gw2, SystemInformation.SmallIconSize);
            notifyIcon.Visible = !Settings.ShowTray.HasValue || Settings.ShowTray.Value;
            notifyIcon.Text = "Gw2Launcher";
            notifyIcon.ContextMenuStrip = contextNotify;
            notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            
            gridContainer.AddAccountClick += gridContainer_AddAccountClick;
            gridContainer.AccountMouseClick += gridContainer_AccountMouseClick;
            gridContainer.AccountBeginDrag += gridContainer_AccountBeginDrag;

            contextMenu.Closed += contextMenu_Closed;

            Settings.ShowTray.ValueChanged += SettingsShowTray_ValueChanged;
            Settings.WindowBounds[typeof(formMain)].ValueChanged += SettingsWindowBounds_ValueChanged;
            Settings.ShowAccount.ValueChanged += ShowAccount_ValueChanged;
            Settings.FontLarge.ValueChanged += FontLarge_ValueChanged;
            Settings.FontSmall.ValueChanged += FontSmall_ValueChanged;
            Settings.BackgroundPatchingEnabled.ValueChanged += BackgroundPatchingEnabled_ValueChanged;
            
            Tools.BackgroundPatcher.Instance.PatchReady += bp_PatchReady;
            Tools.BackgroundPatcher.Instance.PatchBeginning += bp_PatchBeginning;
            Tools.BackgroundPatcher.Instance.DownloadManifestsComplete += bp_DownloadManifestsComplete;
            Tools.BackgroundPatcher.Instance.Error += bp_Error;
            Tools.AutoUpdate.NewBuildAvailable += AutoUpdate_NewBuildAvailable;

            this.Resize += formMain_Resize;
            this.Shown += formMain_Shown;

            LoadAccounts();
            LoadSettings();

            Client.Launcher.Scan();

            var h = this.Handle; //force creation
        }

        void contextNotify_Opening(object sender, CancelEventArgs e)
        {
            disableAutomaticLoginsToolStripMenuItem2.Enabled = (int)disableAutomaticLoginsToolStripMenuItem1.Tag > 0;
            applyWindowedBoundsToolStripMenuItem2.Enabled = (int)applyWindowedBoundsToolStripMenuItem1.Tag > 0;
        }

        private void Initialize()
        {
            windowState = this.WindowState;
            Tools.AutoUpdate.Initialize();

            if (Settings.Silent)
            {
                if (Settings.LastProgramVersion.HasValue)
                {
                    var v = Settings.LastProgramVersion.Value;
                    if (Math.Abs(DateTime.UtcNow.Subtract(Settings.LastProgramVersion.Value.lastCheck).TotalDays) > 30)
                        CheckVersion();
                }
            }
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

                var t = new System.Threading.Thread(new System.Threading.ThreadStart(
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

        public void ShowBackgroundPatcher()
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(ShowBackgroundPatcher));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return;
            }

            var f = bpWindow;

            if (f == null || f.IsDisposed)
            {
                bpWindow = f = new formBackgroundPatcher();

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
                    bpWindow = null;
                };

                var t = new System.Threading.Thread(new System.Threading.ThreadStart(
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
                                            if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                                            {
                                                if (Settings.CheckForNewBuilds.Value || Tools.AutoUpdate.IsEnabled())
                                                    UpdateLastKnownBuild();
                                            }
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

            if (!Settings.Silent)
            {
                if (Settings.CheckForNewBuilds.Value && !Settings.AutoUpdate.Value)
                {
                    CheckBuild();
                }

                if (Settings.LastProgramVersion.HasValue)
                {
                    var v = Settings.LastProgramVersion.Value;
                    if (Math.Abs(DateTime.UtcNow.Subtract(Settings.LastProgramVersion.Value.lastCheck).TotalDays) > 30) //v.version <= Program.RELEASE_VERSION && 
                        CheckVersion();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                notifyIcon.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!initialized)
            {
                initialized = true;
                Initialize();
            }

            if (canShow)
                base.SetVisibleCore(value);
            else
            {
                base.SetVisibleCore(false);
                canShow = true;
            }
        }

        public bool DisableNextVisibilityChange
        {
            get
            {
                return !canShow;
            }
            set
            {
                canShow = !value;
            }
        }

        void AutoUpdate_NewBuildAvailable(object sender, int build)
        {
            if (Settings.AutoUpdate.Value)
            {
                if (Settings.BackgroundPatchingEnabled.Value && Settings.LocalAssetServerEnabled.Value)
                    Tools.BackgroundPatcher.Instance.Start();
                else
                    DoAutoUpdate(build);
            }
            else if (Settings.BackgroundPatchingEnabled.Value)
            {
                Tools.BackgroundPatcher.Instance.Start();
            }
        }

        private void DoAutoUpdate(int build)
        {
            //autoupdate will be ignored if the game is already running -- assuming the user is active and will update when they want to

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
            }
        }

        private async void UpdateLastKnownBuild()
        {
            int build = await Tools.Gw2Build.GetBuildAsync();
            if (build > 0)
                Settings.LastKnownBuild.Value = build;
        }

        private async void CheckBuild()
        {
            int build = await Tools.Gw2Build.GetBuildAsync();
            if (build > 0 && Settings.CheckForNewBuilds.Value && Settings.LastKnownBuild.Value != build)
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
                                                                Client.Launcher.CancelAndKillActiveLaunches();
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
            if (e == 0 && Settings.BringToFrontOnExit.Value && !Settings.Silent)
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

            //SetWindowPos(this.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE);
            //SetWindowPos(this.Handle, (IntPtr)WindowZOrder.HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE);

            Windows.FindWindow.ShowWindow(this.Handle, 5);
            Windows.FindWindow.SetForegroundWindow(this.Handle);
            Windows.FindWindow.BringWindowToTop(this.Handle);
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
                if (DateTime.UtcNow.Subtract(lastCacheDelete).TotalMinutes > 1)
                {
                    lastCacheDelete = DateTime.UtcNow;

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

        void OnButtonsLoaded()
        {

        }

        void OnAccountDataChanged(Settings.IAccount account)
        {
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

        void BackgroundPatchingEnabled_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<bool>;
            if (!setting.HasValue || !setting.Value)
            {
                Tools.BackgroundPatcher.Instance.Stop(false);
            }
        }

        void bp_Error(object sender, string message, Exception exception)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            bp_Error(sender, message, exception);
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                return;
            }

            if (Settings.BackgroundPatchingNotifications.HasValue)
            {
                var v = Settings.BackgroundPatchingNotifications.Value;
                formBuildNotify.Show(formBuildNotify.NotifyType.Error, v.screen, v.anchor, new Tools.BackgroundPatcher.PatchEventArgs());
            }
        }

        void bp_DownloadManifestsComplete(object sender, Tools.BackgroundPatcher.DownloadProgressEventArgs e)
        {
            //if (this.InvokeRequired)
            //{
            //    try
            //    {
            //        this.BeginInvoke(new MethodInvoker(
            //            delegate
            //            {
            //                bp_DownloadManifestsComplete(sender, e);
            //            }));
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.Logging.Log(ex);
            //    }
            //    return;
            //}

            //if (Settings.BackgroundPatchingNotifications.Value)
            //    formBuildNotify.Show(formBuildNotify.NotifyType.DownloadingFiles, e);
        }

        void bp_PatchBeginning(object sender, Tools.BackgroundPatcher.PatchEventArgs e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            bp_PatchBeginning(sender, e);
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                return;
            }

            if (Settings.BackgroundPatchingNotifications.HasValue)
            {
                var v = Settings.BackgroundPatchingNotifications.Value;
                formBuildNotify.Show(formBuildNotify.NotifyType.DownloadingManifests, v.screen, v.anchor, e);
            }
        }

        void bp_PatchReady(object sender, Tools.BackgroundPatcher.PatchEventArgs e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            bp_PatchReady(sender, e);
                        }));
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                return;
            }

            if (Settings.BackgroundPatchingNotifications.HasValue)
            {
                var v = Settings.BackgroundPatchingNotifications.Value;
                formBuildNotify.Show(formBuildNotify.NotifyType.PatchReady, v.screen, v.anchor, e);
            }

            if (Settings.AutoUpdate.Value && Settings.LocalAssetServerEnabled.Value)
                DoAutoUpdate(e.Build);
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

            OnButtonsLoaded();
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
        }

        void gridContainer_AddAccountClick(object sender, EventArgs e)
        {
            AddAccount();
        }

        void gridContainer_AccountBeginDrag(object sender, MouseEventArgs e)
        {
            var button = (AccountGridButton)sender;

            try
            {
                if (dragHelper == null)
                    dragHelper = Windows.DragHelper.Initialize(this);

                using (var icon = new Icon(global::Gw2Launcher.Properties.Resources.Gw2, 64, 64))
                {
                    using (var image = new Bitmap(icon.Width, icon.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (var g = Graphics.FromImage(image))
                        {
                            g.DrawIcon(icon, 0, 0);
                        }

                        Color key = Color.Magenta;
                        Point offset = new Point(image.Width / 2, image.Height / 2);

                        Util.Bitmap.ReplaceTransparentPixels(image, key);

                        Settings.IAccount[] accounts;
                        var buttons = gridContainer.GetSelected();
                        var count = 0;
                        if (!button.Selected && button.AccountData != null)
                        {
                            accounts = new Settings.IAccount[buttons.Count + 1];
                            accounts[count++] = button.AccountData;
                        }
                        else
                            accounts = new Settings.IAccount[buttons.Count];

                        foreach (var b in buttons)
                        {
                            if (b.AccountData != null)
                                accounts[count++] = b.AccountData;
                        }

                        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        var files = new Windows.DragHelper.FileDescriptor[count];

                        var invalid = System.IO.Path.GetInvalidFileNameChars();
                        Func<string, string> getName = delegate(string name)
                        {
                            StringBuilder sb = new StringBuilder(name);
                            foreach (var c in invalid)
                                sb.Replace(c, '_');
                            return sb.ToString();
                        };

                        Func<Windows.DragHelper.FileDescriptor, System.IO.Stream> getContent = delegate(Windows.DragHelper.FileDescriptor file)
                        {
                            var account = accounts[file.Index];
                            var stream = new System.IO.MemoryStream(1500);
                            Windows.Shortcut.Create(stream, path, "-l:silent -l:uid:" + account.UID, "Launch " + account.Name);
                            stream.Position = 0;
                            return stream;
                        };

                        for (var i = 0; i < count; i++)
                        {
                            var account = accounts[i];

                            var f = files[i] = new Windows.DragHelper.FileDescriptor()
                            {
                                Name = getName(account.Name) + ".lnk",
                                Length = -1,
                                GetContent = getContent
                            };
                        }

                        using (var data = Windows.DragHelper.CreateVirtualFileDropDataObject(files))
                        {
                            var result = Windows.DragHelper.DoDragDrop(button, data, DragDropEffects.Copy, false, image, offset, key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
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
                disableAutomaticLoginsToolStripMenuItem1.Enabled = (int)disableAutomaticLoginsToolStripMenuItem1.Tag > 0;
                applyWindowedBoundsToolStripMenuItem1.Enabled = (int)applyWindowedBoundsToolStripMenuItem1.Tag > 0;

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

        private Task<int> GetVersion()
        {
            return Task.Run<int>(new Func<int>(
                delegate
                {
                    var request = System.Net.HttpWebRequest.CreateHttp(formVersionUpdate.UPDATE_BASE_URL + "version");
                    request.Timeout = 5000;
                    request.AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip;

                    try
                    {
                        using (var response = request.GetResponse())
                        {
                            using (var r = new System.IO.StreamReader(response.GetResponseStream()))
                            {
                                return Int32.Parse(r.ReadLine());
                            }
                        }
                    }
                    catch (System.Net.WebException e)
                    {
                        Util.Logging.Log(e);
                        if (e.Response != null)
                        {
                            using (e.Response) { }
                            return -2;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }

                    return -1;
                }));
        }

        private async void CheckVersion()
        {
            var v = await GetVersion();
            if (v > 0)
            {
                Settings.LastProgramVersion.Value = new Settings.LastCheckedVersion()
                {
                    lastCheck = DateTime.UtcNow,
                    version = (ushort)v
                };

                if (v > Program.RELEASE_VERSION)
                {
                    await Task.Delay(1000);

                    while (activeWindows > 0)
                        await Task.Delay(5000);

                    using (BeforeShowDialog())
                    {
                        using (var parent = AddMessageBox(this))
                        {
                            if (MessageBox.Show(parent, "A new version of Gw2Launcher is available.\n\nWould you like to update now?", "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                using (formVersionUpdate f = (formVersionUpdate)AddWindow(new formVersionUpdate()))
                                {
                                    f.ShowDialog(this);
                                }
                            }
                        }
                    }
                }
            }
            else if (v == -2)
            {
                Settings.LastProgramVersion.Value = new Settings.LastCheckedVersion()
                {
                    lastCheck = DateTime.UtcNow,
                    version = 0
                };
            }
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

        public Form AddWindow(Form form)
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

                OnAccountRemoved(account);
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

            OnAccountAdded(account);
        }

        void OnAccountAdded(Settings.IAccount account)
        {
            if (account.Windowed)
                applyWindowedBoundsToolStripMenuItem1.Tag = (int)applyWindowedBoundsToolStripMenuItem1.Tag + 1;
            if (account.AutomaticLogin)
                disableAutomaticLoginsToolStripMenuItem1.Tag = (int)disableAutomaticLoginsToolStripMenuItem1.Tag + 1;
        }

        void OnAccountRemoved(Settings.IAccount account)
        {
            if (account.Windowed)
                applyWindowedBoundsToolStripMenuItem1.Tag = (int)applyWindowedBoundsToolStripMenuItem1.Tag - 1;
            if (account.AutomaticLogin)
                disableAutomaticLoginsToolStripMenuItem1.Tag = (int)disableAutomaticLoginsToolStripMenuItem1.Tag - 1;
        }

        void OnBeforeAccountSettingsUpdated(Settings.IAccount account)
        {
            OnAccountRemoved(account);
        }

        void OnAccountSettingsUpdated(Settings.IAccount account, bool cancelled)
        {
            OnAccountAdded(account);
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

                        OnBeforeAccountSettingsUpdated(account);

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

                            OnAccountSettingsUpdated(account, false);
                        }
                        else
                        {
                            OnAccountSettingsUpdated(account, true);
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
                                                        Client.Launcher.CancelAndKillActiveLaunches();
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
                        if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            if (Settings.CheckForNewBuilds.Value || Tools.AutoUpdate.IsEnabled())
                                UpdateLastKnownBuild();
                        }
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
            await ApplyWindowedBounds(GetSelected());
        }

        private Task ApplyWindowedBounds(IEnumerable<AccountGridButton> selected)
        {
            return Task.Factory.StartNew(new Action(
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

        private void backgroundPatchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowBackgroundPatcher();
        }

        private void backgroundPatchingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowBackgroundPatcher();
        }

        private void createShortcutAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateShortcut(buttons.Values);
        }

        private void createShortcutSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateShortcut(GetSelected());
        }

        private void CreateShortcut(IEnumerable<AccountGridButton> selected)
        {
            List<Settings.IAccount> accounts = new List<Settings.IAccount>();

            foreach (var button in selected)
            {
                var account = button.AccountData;
                if (account != null)
                    accounts.Add(account);
            }

            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            Func<string, string> getName = delegate(string name)
            {
                StringBuilder sb = new StringBuilder(name);
                foreach (var c in invalid)
                    sb.Replace(c, '_');
                return sb.ToString();
            };

            try
            {
                foreach (var account in accounts)
                {
                    var output = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), getName(account.Name) + ".lnk");
                    if (!System.IO.File.Exists(output))
                        Windows.Shortcut.Create(output, path, "-l:silent -l:uid:" + account.UID, "Launch " + account.Name);
                }
            }
            catch (Exception e)
            {
                using (BeforeShowDialog())
                {
                    using (var parent = AddMessageBox(this))
                    {
                        MessageBox.Show(this, "Unable to create shortcuts\n\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == Messaging.Messager.WM_GW2LAUNCHER)
            {
                switch ((Messaging.Messager.MessageType)m.WParam)
                {
                    case Messaging.Messager.MessageType.Show:

                        m.Result = this.Handle;
                        ShowToFront();

                        break;
                    case Messaging.Messager.MessageType.Launch:

                        try
                        {
                            ushort uid = (ushort)m.LParam;

                            this.BeginInvoke(new MethodInvoker(
                                delegate
                                {
                                    var v = Settings.Accounts[uid];
                                    if (v.HasValue)
                                        Client.Launcher.Launch(v.Value, Client.Launcher.LaunchMode.Launch);
                                }));
                        }
                        catch { }

                        break;
                    case Messaging.Messager.MessageType.LaunchMap:
                        
                        try
                        {
                            var launch = Messaging.LaunchMessage.FromMap((int)m.LParam);

                            this.BeginInvoke(new MethodInvoker(
                                delegate
                                {
                                    foreach (var uid in launch.accounts)
                                    {
                                        var v = Settings.Accounts[uid];
                                        if (v.HasValue)
                                            Client.Launcher.Launch(v.Value, Client.Launcher.LaunchMode.Launch, launch.args);
                                    }
                                }));
                        }
                        catch { }

                        break;
                }
            }
        }

        private void disableAutomaticLoginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            disableAutomaticLoginsToolStripMenuItem.Checked = !disableAutomaticLoginsToolStripMenuItem.Checked;
            disableAutomaticLoginsToolStripMenuItem1.Checked = disableAutomaticLoginsToolStripMenuItem.Checked;
            disableAutomaticLoginsToolStripMenuItem2.Checked = disableAutomaticLoginsToolStripMenuItem.Checked;

            Settings.DisableAutomaticLogins = disableAutomaticLoginsToolStripMenuItem.Checked;
        }

        private async void applyWindowedBoundsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            await ApplyWindowedBounds(buttons.Values);
        }

        private void killProcessSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddMessageBox(this))
                {
                    if (MessageBox.Show(f, "Are you sure want to kill all processes associated with the selected accounts?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        foreach (var button in GetSelected())
                        {
                            var account = button.AccountData;
                            if (account != null)
                            {
                                Client.Launcher.Kill(account);
                            }
                        }
                    }
                }
            }
        }

        private void killProcessAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddMessageBox(this))
                {
                    if (MessageBox.Show(f, "Are you sure want to kill all associated processes?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        foreach (var button in buttons.Values)
                        {
                            var account = button.AccountData;
                            if (account != null)
                            {
                                Client.Launcher.Kill(account);
                            }
                        }
                    }
                }
            }
        }
    }
}
