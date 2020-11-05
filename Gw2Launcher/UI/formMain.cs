using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Gw2Launcher.UI.Controls;
using Gw2Launcher.Windows.Native;
using System.Runtime.InteropServices;

namespace Gw2Launcher.UI
{
    public partial class formMain : Base.BaseForm
    {
        const int FADE_DURATION = 100;
        const byte MAX_PAGES = 99;

        private event EventHandler<int> ActiveWindowsChanged;
        private event EventHandler Fading;

        private class FormValue<T> : IDisposable where T : Form 
        {
            public FormValue()
            {
            }

            public T Form
            {
                get;
                set;
            }

            public bool IsDisposed
            {
                get
                {
                    return this.Form != null && this.Form.IsDisposed;
                }
            }

            public bool IsActive
            {
                get
                {
                    return this.Form != null && !this.Form.IsDisposed;
                }
            }

            public void Dispose()
            {
                var f = this.Form;
                if (f != null && !f.IsDisposed)
                {
                    Util.Invoke.Required(f,f.Dispose);
                }
            }
        }

        private class TransparentPanel : Panel
        {
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == (int)WindowMessages.WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HitTest.Transparent;
                }
            }
        }

        private class TransparentStackPanel : StackPanel
        {
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == (int)WindowMessages.WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HitTest.Transparent;
                }
            }
        }

        private class TitleLabel : Control
        {
            private bool remeasure;
            private Size measured;

            protected override void OnTextChanged(EventArgs e)
            {
                remeasure = true;
                base.OnTextChanged(e);
                this.Invalidate();
            }

            protected override void OnFontChanged(EventArgs e)
            {
                remeasure = true;
                base.OnFontChanged(e);
                this.Invalidate();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                this.Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (remeasure)
                {
                    remeasure = false;
                    measured = TextRenderer.MeasureText(e.Graphics, this.Text, this.Font, Size.Empty, TextFormatFlags.SingleLine);
                }

                int w = this.Width,
                    h = this.Height;
                int pw = this.Parent.Width;

                var mw = measured.Width;
                var x = pw / 2 - mw / 2 - this.Left;
                var y = h / 2 - measured.Height / 2;
                if (x < 0)
                    x = 0;
                var r = x + mw;

                if (r > w)
                {
                    x = w - mw;
                    if (x < 0)
                    {
                        x = 0;
                        mw = w;
                    }
                }

                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, new Rectangle(x, y, mw, measured.Height), this.ForeColor, this.BackColor, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == (int)WindowMessages.WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HitTest.Transparent;
                    return;
                }

                base.WndProc(ref m);
            }
        }

        private class MenuButton : FlatShapeButton
        {
            public MenuButton()
                : base()
            {
                _Page = 0;
            }

            private byte _Page;
            [DefaultValue(0)]
            public byte Page
            {
                get
                {
                    return _Page;
                }
                set
                {
                    if (_Page != value)
                    {
                        _Page = value;
                        OnRedrawRequired();
                    }
                }
            }

            protected override void OnPaintBuffer(Graphics g)
            {
                base.OnPaintBuffer(g);

                if (_Page > 0)
                {
                    int width = this.Width,
                        height = this.Height;

                    var scale = g.DpiX / 96f;

                    TextRenderer.DrawText(g, _Page.ToString(), this.Font, new Rectangle(0, 0, width - (int)(2 * scale), height / 2 + this.FontHeight), this.ForeColorCurrent, this.BackColorCurrent, TextFormatFlags.Bottom | TextFormatFlags.Right | TextFormatFlags.NoPadding);
                }
            }
        }

        private bool autoSizeGrid;
        private NotifyIcon notifyIcon;
        private bool canShow, initialized;
        private DateTime lastCrashDelete;
        private bool isInModalLoop;

        private Dictionary<ushort, AccountGridButton> buttons;
        private formAccountTooltip tooltip;
        private formUpdating updatingWindow;
        private FormValue<formAssetProxy> assetProxyWindow;
        private FormValue<formBackgroundPatcher> bpWindow;
        private formProgressOverlay bpProgress;
        private FormValue<formAccountBar> abWindow, qlWindow;
        private Tools.QueuedAccountApi accountApi;
        private formDailies dailies;
        private Tools.Screenshots screenshotMonitor;
        private AccountGridButton focusedButton;
        private Tools.JumpList jumplist;
        private Util.EnabledEvent resizingEvents;
        private BufferedGraphics buffer;
        private bool redraw;
        private Image defaultBackground;
        private formMenu mpWindow;
        private formMaskOverlay.Manager maskManager;

        private bool shown;

        private byte activeWindows;
        private List<Form> windows;
        
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
            InitializeComponents();

            SetStyle(ControlStyles.ResizeRedraw, true);
            this.Opacity = 0;

            if (Settings.ReadOnly)
                this.Text += " [ReadOnly]";

            windows = new List<Form>();
            buttons = new Dictionary<ushort, AccountGridButton>();
            canShow = true;
            redraw = true;

            KeyPreview = true;

            //disableAutomaticLoginsToolStripMenuItem1.Tag = 0;
            applyWindowedBoundsToolStripMenuItem1.Tag = 0;

            Application.EnterThreadModal += Application_EnterThreadModal;
            Application.LeaveThreadModal += Application_LeaveThreadModal;
            this.FormClosing += formMain_FormClosing;

            buttonMenu.MouseWheel += buttonMenu_MouseWheel;
            //buttonMenu.MouseEnter += buttonMenu_MouseEnter;

            Settings.AccountSorting.SortingChanged += AccountSorting_SortingChanged;

            Client.Launcher.AccountStateChanged += Launcher_AccountStateChanged;
            Client.Launcher.LaunchException += Launcher_LaunchException;
            Client.Launcher.AccountLaunched += Launcher_AccountLaunched;
            Client.Launcher.AccountExited += Launcher_AccountExited;
            Client.Launcher.AnyActiveProcessCountChanged += Launcher_AnyActiveProcessCountChanged;
            Client.Launcher.BuildUpdated += Launcher_BuildUpdated;
            Client.Launcher.AccountQueued += Launcher_AccountQueued;
            Client.Launcher.NetworkAuthorizationRequired += Launcher_NetworkAuthorizationRequired;
            Client.Launcher.AccountWindowEvent+=Launcher_AccountWindowEvent;
            Client.Launcher.AllQueuedLaunchesCompleteAllAccountsExited += Launcher_AllQueuedLaunchesCompleteAllAccountsExited;

            contextNotify.Opening += contextNotify_Opening;

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(Properties.Resources.Gw2Launcher, SystemInformation.SmallIconSize);
            notifyIcon.Visible = !Settings.ShowTray.HasValue || Settings.ShowTray.Value;
            notifyIcon.Text = "Gw2Launcher";
            notifyIcon.ContextMenuStrip = contextNotify;
            notifyIcon.DoubleClick += notifyIcon_DoubleClick;

            mpWindow = new formMenu();
            mpWindow.VisibleChanged += mpWindow_VisibleChanged;
            mpWindow.PageChanged += mpWindow_PageChanged;
            mpWindow.MenuItemSelected += mpWindow_MenuItemSelected;

            if (Settings.StyleColumns.Value > 0)
            {
                gridContainer.GridColumnsAuto = false;
                gridContainer.GridColumns = Settings.StyleColumns.Value;
            }
            else
            {
                gridContainer.GridColumnsAuto = true;
            }

            gridContainer.GridSpacing = 3;
            gridContainer.AddAccountClick += gridContainer_AddAccountClick;
            gridContainer.AccountMouseClick += gridContainer_AccountMouseClick;
            gridContainer.AccountBeginDrag += gridContainer_AccountBeginDrag;
            gridContainer.AccountBeginMousePressed += gridContainer_AccountBeginMousePressed;
            gridContainer.AccountMousePressed += gridContainer_AccountMousePressed;
            gridContainer.AccountBeginMouseClick += gridContainer_AccountBeginMouseClick;
            gridContainer.AccountSelection += gridContainer_AccountSelection;
            gridContainer.AccountNoteClicked += gridContainer_AccountNoteClicked;

            contextMenu.Closed += contextMenu_Closed;
            contextMenu.Opening += contextMenu_Opening;

            Settings.ShowTray.ValueChanged += SettingsShowTray_ValueChanged;
            Settings.WindowBounds[typeof(formMain)].ValueChanged += SettingsWindowBounds_ValueChanged;
            Settings.StyleShowAccount.ValueChanged += OnButtonStyleChanged;
            Settings.StyleShowColor.ValueChanged += OnButtonStyleChanged;
            Settings.StyleHighlightFocused.ValueChanged += StyleHighlightFocused_ValueChanged;
            Settings.FontName.ValueChanged += OnButtonStyleChanged;
            Settings.FontStatus.ValueChanged += OnButtonStyleChanged;
            Settings.FontUser.ValueChanged += OnButtonStyleChanged;
            Settings.StyleShowIcon.ValueChanged += OnButtonStyleChanged;
            Settings.StyleColors.ValueChanged += OnButtonStyleChanged;
            Settings.StyleBackgroundImage.ValueChanged += StyleBackgroundImage_ValueChanged;
            Settings.StyleColumns.ValueChanged += StyleColumns_ValueChanged;
            Settings.BackgroundPatchingEnabled.ValueChanged += BackgroundPatchingEnabled_ValueChanged;
            Settings.TopMost.ValueChanged += TopMost_ValueChanged;
            Settings.ShowDailies.ValueChanged += ShowDailies_ValueChanged;
            Settings.ScreenshotNaming.ValueChanged += ScreenshotSettings_ValueChanged;
            Settings.ScreenshotConversion.ValueChanged += ScreenshotSettings_ValueChanged;
            Settings.ShowKillAllAccounts.ValueChanged += ShowKillAllAccounts_ValueChanged;
            Settings.ShowLaunchAllAccounts.ValueChanged += ShowLaunchAllAccounts_ValueChanged;
            Settings.JumpList.ValueChanged += JumpList_ValueChanged;
            Settings.PreventTaskbarMinimize.ValueChanged += PreventTaskbarMinimize_ValueChanged;
            Settings.ProcessPriority.ValueChanged += ProcessPriority_ValueChanged;

            Tools.BackgroundPatcher.Instance.PatchReady += bp_PatchReady;
            Tools.BackgroundPatcher.Instance.PatchBeginning += bp_PatchBeginning;
            Tools.BackgroundPatcher.Instance.DownloadManifestsComplete += bp_DownloadManifestsComplete;
            Tools.BackgroundPatcher.Instance.Error += bp_Error;
            Tools.BackgroundPatcher.Instance.Complete += bp_Complete;
            Tools.AutoUpdate.NewBuildAvailable += AutoUpdate_NewBuildAvailable;

            Util.ScheduledEvents.Register(OnDailyResetCallback, GetNextDaily());
            Util.ScheduledEvents.Register(PurgeTemp, 0);

            LoadAccounts();
            LoadSettings();

            if (Settings.NotesNotifications.HasValue)
                Settings.NotesNotifications.ValueChanged += NotesNotificationsHasValue_ValueChanged;
            else
                Settings.NotesNotifications.ValueChanged += NotesNotificationsNoValue_ValueChanged;

            //PurgeTemp();
            PurgeNotes();

            MainWindowHandle = this.Handle; //force creation (required for Scan)
            Client.Launcher.Scan(Client.Launcher.AccountType.Any);
        }

        public static IntPtr MainWindowHandle
        {
            get;
            private set;
        }

        void formMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Client.Launcher.HasPendingNetworkAuthorizationRequests)
            {
                this.FormClosing -= formMain_FormClosing;

                //try to close remaining sessions within a few seconds
                //not including sessions currently being used

                Task t = null;
                try
                {
                    t = Client.Launcher.ClearNetworkAuthorization(true, true);
                    if (t.IsCompleted)
                    {
                        t.Dispose();
                        t = null;
                    }
                    else
                    {
                        t = Task.WhenAny(t, Task.Delay(5000));
                    }
                }
                catch
                {
                    t = null;
                }

                if (t == null || t.IsCompleted)
                {
                    return;
                }

                e.Cancel = true;

                Util.Invoke.Async(this,
                    delegate
                    {
                        using (BeforeShowDialog(false))
                        {
                            ShowWaiting(async delegate(formWaiting f, Action close)
                            {
                                try
                                {
                                    await t;
                                }
                                catch (Exception ex)
                                {
                                    Util.Logging.Log(ex);
                                }

                                close();
                            });
                        }

                        this.Close();
                    });
            }
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override void OnDpiChanged()
        {
            base.OnDpiChanged();

            gridContainer.GridSpacing = Scale(3);

            panelTop.Width = this.Width - panelTop.Left * 2;
            panelContainer.Width = this.Width - panelContainer.Left * 2;
            panelBottom.Width = this.Width - panelBottom.Left * 2;
        }

        void mpWindow_VisibleChanged(object sender, EventArgs e)
        {
            buttonMenu.Selected = mpWindow.Visible;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            
            if (buttonMenu.IsHovered)
            {
                buttonMenu_MouseWheel(this, e);
            }
            else
            {
                gridContainer.DoMouseWheel(e);
            }
        }

        void buttonMenu_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!buttonMenu.IsHovered)
            {
                return;
            }

            if (e is HandledMouseEventArgs)
                ((HandledMouseEventArgs)e).Handled = true;

            if (contextMenu.Visible)
                contextMenu.Hide();
            if (contextSelection.Visible)
                contextSelection.Hide();

            if (e.Delta > 0)
            {
                mpWindow.PagePrevious();
            }
            else
            {
                mpWindow.PageNext();
            }
        }

        void mpWindow_MenuItemSelected(object sender, formMenu.MenuItem e)
        {
            switch (e)
            {
                case formMenu.MenuItem.Search:

                    if (_ShowFilter)
                    {
                        textFilter.Focus();
                    }
                    else
                        ShowFilter = true;

                    break;
                case formMenu.MenuItem.Settings:

                    settingsToolStripMenuItem_Click(null, null);

                    break;
                case formMenu.MenuItem.CloseAllAccounts:

                    Client.Launcher.CancelAndKillActiveLaunches(Client.Launcher.AccountType.Any);

                    break;
            }
        }

        void mpWindow_PageChanged(object sender, byte e)
        {
            buttonMenu.Page = e;
            gridContainer.Page = e;
            Settings.SelectedPage.Value = e;
        }

        void StyleBackgroundImage_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<string>)sender;

            Image i;

            try
            {
                if (v.Value != null)
                    i = Bitmap.FromFile(v.Value);
                else
                    i = null;
            }
            catch
            {
                i = null;
            }

            if (defaultBackground == i)
                return;
            using (defaultBackground) { }
            defaultBackground = i;

            foreach (var button in buttons.Values)
            {
                button.DefaultBackgroundImage = defaultBackground;
            }
        }

        void StyleColumns_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<byte>)sender;
            if (v.Value > 0)
            {
                gridContainer.GridColumnsAuto = false;
                gridContainer.GridColumns = v.Value;
            }
            else
            {
                gridContainer.GridColumnsAuto = true;
            }
        }

        void ShowKillAllAccounts_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<bool>)sender;
            buttonKillAll.Visible = v.Value && Client.Launcher.GetActiveProcessCount() > 0;
        }

        void ShowLaunchAllAccounts_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<bool>)sender;
            buttonLaunchAll.Visible = v.Value;
        }

        void AccountSorting_SortingChanged(object sender, EventArgs e)
        {
            var o = Settings.Sorting.Value;

            switch (o.Sorting.Mode)
            {
                case Settings.SortMode.CustomGrid:
                case Settings.SortMode.CustomList:

                    SetSorting(o.Sorting.Mode, o.Sorting.Descending, true);

                    break;
            }
        }

        void JumpList_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<Settings.JumpListOptions>)sender;

            if (v.HasValue && v.Value.HasFlag(Settings.JumpListOptions.Enabled))
            {
                if (jumplist == null && Windows.JumpList.IsSupported)
                {
                    jumplist = new Tools.JumpList(this.Handle);
                }
            }
            else if (jumplist != null)
            {
                jumplist.Dispose(true);
                jumplist = null;
            }
        }

        void PreventTaskbarMinimize_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<bool>)sender;

            if (v.Value)
            {
                Windows.WindowLong.Remove(this.Handle, GWL.GWL_STYLE, WindowStyle.WS_MINIMIZEBOX);
            }
            else
            {
                Windows.WindowLong.Add(this.Handle, GWL.GWL_STYLE, WindowStyle.WS_MINIMIZEBOX);
            }
        }

        void Application_LeaveThreadModal(object sender, EventArgs e)
        {
            isInModalLoop = false;
            OnIsInModalLoopChanged();
        }

        void Application_EnterThreadModal(object sender, EventArgs e)
        {
            isInModalLoop = true;
            OnIsInModalLoopChanged();
        }

        private void OnIsInModalLoopChanged()
        {
            if (abWindow != null && abWindow.IsActive)
            {
                abWindow.Form.CanLaunch = !isInModalLoop;
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            labelTitle.Text = this.Text;
        }

        protected override void OnClick(EventArgs e)
        {
            gridContainer.ClearSelected();

            base.OnClick(e);
        }

        public static void Swap(ToolStripItemCollection to, ToolStripItemCollection from)
        {
            var count = from.Count;
            if (count > 0)
            {
                var items = new ToolStripItem[count];
                from.CopyTo(items, 0);
                to.AddRange(items);
            }
        }

        void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            Swap(toolsToolStripMenuItem1.DropDownItems, toolsToolStripMenuItem.DropDownItems);

            OnContextOpening();
        }

        void contextNotify_Opening(object sender, CancelEventArgs e)
        {
            Swap(toolsToolStripMenuItem.DropDownItems, toolsToolStripMenuItem1.DropDownItems);

            //disableAutomaticLoginsToolStripMenuItem2.Enabled = (int)disableAutomaticLoginsToolStripMenuItem1.Tag > 0;
            applyWindowedBoundsToolStripMenuItem2.Enabled = (int)applyWindowedBoundsToolStripMenuItem1.Tag > 0;

            OnContextOpening();
        }

        void OnContextOpening()
        {
            var accountBar = Settings.AccountBar.Enabled.Value;
            showAccountBarToolStripMenuItem.Visible = accountBar;
            toolStripMenuItem10.Visible = accountBar;
        }

        private void contextSelection_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            Swap(selectedToolStripMenuItem.DropDownItems, contextSelection.Items);
        }

        void contextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (selectedToolStripMenuItem.Tag == null)
                gridContainer.ClearSelected();
        }

        private int PurgeTemp()
        {
            if (Client.Launcher.GetPendingLaunchCount() > 0 || Client.Launcher.GetActiveProcessCount() > 0)
            {
                EventHandler onExit = null;
                onExit = delegate
                {
                    Client.Launcher.AllQueuedLaunchesCompleteAllAccountsExited -= onExit;
                    PurgeTemp();
                };
                Client.Launcher.AllQueuedLaunchesCompleteAllAccountsExited += onExit;
            }
            else
            {
                Task.Factory.StartNew(new Action(
                   delegate
                   {
                       try
                       {
                           Tools.Temp.Purge(7);
                       }
                       catch (Exception ex)
                       {
                           Util.Logging.Log(ex);
                       }
                   }));
            }

            return 3 * 24 * 60 * 60 * 1000;
        }

        private async void PurgeNotes()
        {
            var limit = DateTime.UtcNow.AddDays(-30);
            Tools.Notes store = null;

            try
            {
                foreach (var a in Util.Accounts.GetAccounts())
                {
                    var notes = a.Notes;
                    if (notes == null || notes.Count == 0)
                        continue;

                    Settings.Notes.Note n;
                    while (notes.Count > 0 && limit > (n = notes[0]).Expires)
                    {
                        if (notes.Remove(n))
                        {
                            try
                            {
                                if (store == null)
                                    store = new Tools.Notes();
                                await store.RemoveAsync(n.SID);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                                notes.Add(n);
                                break;
                            }
                        }
                    }
                }

                if (store != null)
                    await store.CloseAsync();
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
            finally
            {
                if (store != null)
                    store.Dispose();
            }
        }

        public void ShowPatchProxy()
        {
            if (Util.Invoke.IfRequired(this, ShowPatchProxy))
                return;

            var fv = assetProxyWindow;

            if (fv == null || fv.IsDisposed)
            {
                assetProxyWindow = fv = new FormValue<formAssetProxy>();

                var t = new System.Threading.Thread(new System.Threading.ThreadStart(
                    delegate
                    {
                        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                        var f = fv.Form = new formAssetProxy();

                        if (this.WindowState != FormWindowState.Minimized)
                        {
                            f.StartPosition = FormStartPosition.Manual;
                            var screen = Screen.FromRectangle(this.Bounds);
                            int x = this.Location.X + this.Width + 5;
                            if (x >= screen.WorkingArea.Width / 2)
                                x = this.Location.X - f.Width - 5;
                            f.Location = new Point(x, this.Location.Y + this.Height / 2 - f.Height / 2);
                            f.DesktopBounds = Util.RectangleConstraint.ConstrainToScreen(f.DesktopBounds);
                        }
                        else
                            f.StartPosition = FormStartPosition.CenterScreen;

                        try
                        {
                            Application.Run(f);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Crash(e);
                        }

                        assetProxyWindow = null;
                    }));

                t.IsBackground = true;
                t.SetApartmentState(System.Threading.ApartmentState.STA);
                t.Start();
            }
            else
            {
                var f = fv.Form;
                if (f != null && !f.IsDisposed)
                    FocusFormWindow(f);
            }
        }

        public void ShowBackgroundPatcher()
        {
            if (Util.Invoke.IfRequired(this, ShowBackgroundPatcher))
                return;

            var fv = bpWindow;

            if (fv == null || fv.IsDisposed)
            {
                bpWindow = fv = new FormValue<formBackgroundPatcher>();

                var t = new System.Threading.Thread(new System.Threading.ThreadStart(
                    delegate
                    {
                        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                        var f = fv.Form = new formBackgroundPatcher();

                        if (this.WindowState != FormWindowState.Minimized)
                        {
                            f.StartPosition = FormStartPosition.Manual;
                            var screen = Screen.FromRectangle(this.Bounds);
                            int x = this.Location.X + this.Width + 5;
                            if (x >= screen.WorkingArea.Width / 2)
                                x = this.Location.X - f.Width - 5;
                            f.Location = new Point(x, this.Location.Y + this.Height / 2 - f.Height / 2);
                            f.DesktopBounds = Util.RectangleConstraint.ConstrainToScreen(f.DesktopBounds);
                        }
                        else
                            f.StartPosition = FormStartPosition.CenterScreen;

                        try
                        {
                            Application.Run(f);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Crash(e);
                        }

                        bpWindow = null;
                    }));

                t.IsBackground = true;
                t.SetApartmentState(System.Threading.ApartmentState.STA);
                t.Start();
            }
            else
            {
                var f = fv.Form;
                if (f != null && !f.IsDisposed)
                    FocusFormWindow(f);
            }
        }

        public void ShowQuickLaunch(int type = -1)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    ShowQuickLaunch(type);
                }))
                return;

            var fv = qlWindow;

            if (fv == null || fv.IsDisposed)
            {
                qlWindow = fv = new FormValue<formAccountBar>();

                var t = new System.Threading.Thread(new System.Threading.ThreadStart(
                    delegate
                    {
                        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                        var f = fv.Form = new formAccountBar(formAccountBar.AccountBarMode.QuickLaunch);

                        f.HideGw2 = type == (int)Settings.AccountType.GuildWars1;
                        f.HideGw1 = type == (int)Settings.AccountType.GuildWars2;

                        f.Initialize(true);

                        try
                        {
                            Application.Run(f);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Crash(e);
                        }

                        qlWindow = null;
                    }));

                t.IsBackground = true;
                t.SetApartmentState(System.Threading.ApartmentState.STA);
                t.Start();
            }
            else
            {
                var f = fv.Form;
                if (f != null && !f.IsDisposed)
                {
                    Util.Invoke.Required(f,
                        delegate
                        {
                            f.HideGw2 = type == (int)Settings.AccountType.GuildWars1;
                            f.HideGw1 = type == (int)Settings.AccountType.GuildWars2;

                            f.ShowPopup();
                            FocusFormWindow(f);
                        });
                }
            }
        }

        public void ShowAccountBar(bool forceShow)
        {
            if (Util.Invoke.IfRequired(this, 
                delegate
                {
                    ShowAccountBar(forceShow);
                }))
                return;

            var fv = abWindow;

            if (fv == null || fv.IsDisposed)
            {
                abWindow = fv = new FormValue<formAccountBar>();

                var t = new System.Threading.Thread(new System.Threading.ThreadStart(
                    delegate
                    {
                        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                        var f = fv.Form = new formAccountBar();
                        f.Initialize(forceShow);

                        try
                        {
                            Application.Run(f);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Crash(e);
                        }

                        abWindow = null;
                    }));

                t.IsBackground = true;
                t.SetApartmentState(System.Threading.ApartmentState.STA);
                t.Start();
            }
            else
            {
                var f = fv.Form;
                if (f != null && !f.IsDisposed && forceShow)
                {
                    Util.Invoke.Required(f,
                        delegate
                        {
                            if (!f.Visible)
                                f.Show();
                            FocusFormWindow(f);
                        });
                    FocusFormWindow(this);
                }
            }
        }

        private void FocusFormWindow(Form f)
        {
            Util.Invoke.Required(f,
                delegate
                {
                    var h = f.Handle;
                    if (h != IntPtr.Zero)
                    {
                        try
                        {
                            Windows.FindWindow.FocusWindow(h);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                });
        }

        private int OnNoteCallback()
        {
            var now = DateTime.UtcNow;
            var min = DateTime.MaxValue;
            var count = 0;
            var canShow = true;
            var checkedActive = false;

            foreach (var b in buttons.Values)
            {
                if (b.AccountData == null)
                    continue;

                if (Settings.NotesNotifications.HasValue)
                {
                    var notes = b.AccountData.Notes;
                    if (notes == null)
                        continue;

                    foreach (var n in notes)
                    {
                        if (n.Notify)
                        {
                            if (now < n.Expires)
                            {
                                if (n.Expires < min)
                                    min = n.Expires;
                                break;
                            }
                            else
                            {
                                if (!checkedActive && (checkedActive = true) && Settings.NotesNotifications.Value.OnlyWhileActive)
                                    canShow = Windows.LastInput.IsActive;

                                if (canShow)
                                {
                                    n.Notify = false;
                                    ShowNoteNotification(n, b.AccountData, count++ * 1000);
                                }
                            }
                        }
                    }
                }

                if (now >= b.LastNoteUtc)
                    b.LastNoteUtc = DateTime.MinValue;
                else if (b.LastNoteUtc < min)
                    min = b.LastNoteUtc;
            }

            if (min != DateTime.MaxValue)
            {
                var i = min.Subtract(now).TotalMilliseconds + 1;

                if (i > 5000)
                {
                    if (!canShow)
                        return 5000;
                    else if (i > int.MaxValue)
                        return int.MaxValue;
                }
                else if (i < 1000)
                {
                    if (count > 0)
                        return 1000;
                    else if (i < 0)
                        return 0;
                }

                return (int)i;
            }
            else if (!canShow)
                return 5000;

            return -1;
        }

        private async void ShowNoteNotification(Settings.Notes.Note note, Settings.IAccount account, int delay)
        {
            if (delay > 0)
                await Task.Delay(delay);

            var v = Settings.NotesNotifications.Value;
            string text;

            try
            {
                var r = await Tools.Notes.GetRange(note.SID);
                text = r[0];
            }
            catch
            {
                text = null;
            }

            if (!string.IsNullOrEmpty(text))
                formNotify.ShowNote(v.Screen, v.Anchor, text, account.Name);
        }

        private int GetNextDaily()
        {
            var ticks = DateTime.UtcNow.Ticks;
            const long TICKS_PER_DAY = 864000000000;

            int millisToNextDay = (int)(((ticks / TICKS_PER_DAY + 1) * TICKS_PER_DAY - ticks) / 10000) + 1;
            if (millisToNextDay < 0)
                return -1;

            return millisToNextDay;
        }

        private int OnDailyResetCallback()
        {
            foreach (var button in buttons.Values)
            {
                var account = button.AccountData;
                if (account != null)
                {
                    var refresh = account.ShowDailyLogin;

                    if (account.Type == Settings.AccountType.GuildWars2)
                    {
                        var gw2 = (Settings.IGw2Account)account;
                        //set the last used for active accounts if either the daily or played time is being tracked, so that the login icon doesn't take priority
                        if ((gw2.ShowDailyCompletion || gw2.ApiData != null && gw2.ApiData.Played != null) && Client.Launcher.GetState(gw2) == Client.Launcher.AccountState.ActiveGame)
                        {
                            if (gw2.ApiData != null && gw2.ApiData.Played != null)
                                QueuedAccountApi.Schedule(gw2, gw2.LastUsedUtc.Date, 0);
                            button.LastUsedUtc = DateTime.UtcNow;
                        }

                        refresh = refresh || gw2.ShowDailyCompletion;
                    }

                    if (refresh)
                    {
                        button.Redraw();
                    }
                }
            }

            if (dailies != null && !dailies.IsDisposed)
            {
                var d = Settings.ShowDailies.Value;
                if (d.HasFlag(Settings.DailiesMode.Show))
                {
                    if (d.HasFlag(Settings.DailiesMode.AutoLoad))
                        dailies.LoadToday();
                    else
                        dailies.LoadOnShow = true;
                }
            }

            if (jumplist != null)
            {
                jumplist.RefreshAsync();
            }

            return GetNextDaily();
        }

        private Tools.QueuedAccountApi QueuedAccountApi
        {
            get
            {
                if (accountApi == null)
                {
                    accountApi = new Tools.QueuedAccountApi();
                    accountApi.DataReceived += accountApi_DataReceived;
                }
                return accountApi;
            }
        }

        void Launcher_AccountQueued(Settings.IAccount account, Client.Launcher.LaunchMode e)
        {
            if (e == Client.Launcher.LaunchMode.Update || e == Client.Launcher.LaunchMode.UpdateVisible)
            {
                var cancel = new System.Threading.CancellationTokenSource(3000);

                try
                {
                    Util.Invoke.Required(this, delegate
                    {
                        if (updatingWindow != null && !updatingWindow.IsDisposed)
                        {
                            updatingWindow.AddAccount(account);
                            cancel.Cancel();
                            return;
                        }

                        List<Settings.IAccount> accounts = new List<Settings.IAccount>();
                        accounts.Add(account);

                        formUpdating f = updatingWindow = new formUpdating(accounts, true, true);

                        Util.Invoke.Async(this, delegate
                        {
                            var hide = Settings.Silent && !this.Visible;

                            using (BeforeShowDialog())
                            {
                                using (AddWindow(f))
                                {
                                    f.Shown += delegate
                                    {
                                        try
                                        {
                                            if (cancel != null)
                                                cancel.Cancel();
                                        }
                                        catch { }
                                    };
                                    if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                                    {
                                        if (hide)
                                        {
                                            this.WindowState = FormWindowState.Minimized;
                                        }

                                        if (Settings.CheckForNewBuilds.Value || Tools.AutoUpdate.IsEnabled())
                                            UpdateLastKnownBuild();
                                    }
                                    updatingWindow = null;
                                }
                            }
                        });
                    });

                    cancel.Token.WaitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                finally
                {
                    cancel.Dispose();
                    cancel = null;
                }
            }
        }

        void Launcher_BuildUpdated(Client.Launcher.BuildUpdatedEventArgs e)
        {
            var dats = new HashSet<Settings.IDatFile>();
            var accounts = new List<Settings.IAccount>(Settings.Accounts.Count);
            var sort = Settings.GuildWars2.DatUpdaterEnabled.Value;
            var minimum = int.MaxValue;
            var index = 0;

            //only accounts with a unique dat file need to be updated

            foreach (var a in Util.Accounts.GetGw2Accounts())
            {
                if ((a.DatFile == null || dats.Add(a.DatFile)))
                {
                    accounts.Add(a);

                    if (sort && minimum > 0)
                    {
                        try
                        {
                            if (a.DatFile != null)
                            {
                                var l = new FileInfo(a.DatFile.Path).Length;
                                if (l < minimum)
                                {
                                    minimum = (int)l;
                                    index = accounts.Count - 1;
                                }
                            }
                            else
                            {
                                minimum = 0;
                                index = accounts.Count - 1;
                            }
                        }
                        catch { }
                    }
                }
            }

            if (sort && index != 0)
            {
                var a = accounts[index];
                accounts[index] = accounts[0];
                accounts[0] = a;
            }

            if (accounts.Count > 0)
                e.Update(accounts);
        }

        private formWaiting ShowWaiting()
        {
            formWaiting w = new formWaiting(this);
            w.Owner = this;

            OnChildFormCreated(w);

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

        /// <summary>
        /// Shows the waiting overlay
        /// </summary>
        /// <param name="a">Action to be called, with parameters formWaiting, close()</param>
        private void ShowWaiting(Action<formWaiting, Action> a)
        {
            using (var f = ShowWaiting())
            {
                Action close = delegate
                {
                    Util.Invoke.Required(f, f.Close);
                };

                f.Shown += delegate
                {
                    a(f, close);
                };

                f.ShowDialog(this);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            shown = true;

            //note that the form will start minimized when silent, so this may not be called on launch -- use Initialize() instead

            if (!Settings.Silent)
            {
                NativeMethods.SetForegroundWindow(this.Handle);

                if (Settings.CheckForNewBuilds.Value && !Settings.AutoUpdate.Value)
                {
                    CheckBuild();
                }
            }

            if (Settings.IsRunningWine)
            {
                resizingEvents = new Util.EnabledEvent();
                resizingEvents.EnabledChanged += resizingEvents_EnabledChanged;
            }

            FadeTo(1);
        }

        private void Initialize()
        {
            var s = Settings.WindowBounds[typeof(UI.formMain)];

            Point location;
            Size size;

            if (s.HasValue && !s.Value.Size.IsEmpty)
            {
                this.AutoSizeGrid = false;
                size = s.Value.Size;
            }
            else
            {
                this.AutoSizeGrid = true;
                size = ResizeAuto(Point.Empty);
            }

            if (s.HasValue && !s.Value.Location.Equals(new Point(int.MinValue, int.MinValue)))
                location = Util.ScreenUtil.Constrain(s.Value.Location, size);
            else
            {
                var bounds = Screen.PrimaryScreen.WorkingArea;
                var l = Point.Add(bounds.Location, new Size(bounds.Width / 2 - size.Width / 2, bounds.Height / 2 - size.Height / 2 + this.Height - this.ClientSize.Height));

                if (l.Y + size.Height > bounds.Bottom)
                {
                    l.Y = bounds.Bottom - size.Height;
                    if (l.Y < bounds.Top)
                    {
                        l.Y = bounds.Top;
                    }
                }

                location = l;
            }

            var page = Settings.SelectedPage.Value;
            if (page > 0 && page < mpWindow.Pages)
                mpWindow.Page = page;

            if (this.AutoSizeGrid)
            {
                size = ResizeAuto(location);
            }

            try
            {
                this.Bounds = new Rectangle(location, size);
                NativeMethods.SetWindowPos(this.Handle, IntPtr.Zero, location.X, location.Y, size.Width, size.Height, SetWindowPosFlags.SWP_NOZORDER);
            }
            catch { }

            Tools.AutoUpdate.Initialize();

            this.TopMost = Settings.TopMost.Value;

            if (Settings.LastProgramVersion.HasValue)
            {
                var nextCheck = Settings.LastProgramVersion.Value.LastCheck.AddDays(30);
                if (DateTime.UtcNow > nextCheck)
                    CheckVersion();
                else
                    Util.ScheduledEvents.Register(OnCheckVersionCallback, nextCheck.Ticks / 10000);
            }

            Util.ScheduledEvents.Register(OnNoteCallback, 0);

            if (Settings.AccountBar.Enabled.Value)
                ShowAccountBar(false);

            ShowDailies_ValueChanged(Settings.ShowDailies, EventArgs.Empty);

            if ((Settings.JumpList.Value & Settings.JumpListOptions.Enabled) == Settings.JumpListOptions.Enabled && Windows.JumpList.IsSupported)
            {
                jumplist = new Tools.JumpList(this.Handle);
            }

            if (Settings.Silent)
            {
                if (Settings.ShowTray.Value && Settings.MinimizeToTray.Value)
                {
                    this.DisableNextVisibilityChange = true;
                }
                else
                {
                    this.WindowState = FormWindowState.Minimized;
                }
            }
        }

        private int OnCheckVersionCallback()
        {
            long remaining = -1;

            if (Settings.LastProgramVersion.HasValue)
            {
                var nextCheck = Settings.LastProgramVersion.Value.LastCheck.AddDays(30);
                remaining = (nextCheck.Ticks - DateTime.UtcNow.Ticks) / 10000;

                if (remaining < 0)
                {
                    CheckVersion();
                    remaining = -1;
                }
            }

            if (remaining > int.MaxValue)
                return int.MaxValue;
            return (int)remaining;
        }

        private async void ShowChangelog()
        {
            var f = new formChangelog();
            if (await f.LoadChangelog())
            {
                using (f)
                {
                    f.ShowDialog(this);
                }
            }
            else
                f.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);

            if (disposing)
            {
                if (notifyIcon != null)
                {
                    try
                    {
                        notifyIcon.Visible = false;
                    }
                    catch { }
                    notifyIcon.Dispose();
                }
                if (maskManager != null)
                    maskManager.Dispose();
                if (screenshotMonitor != null)
                    screenshotMonitor.Dispose();
                if (abWindow != null)
                    abWindow.Dispose();
                if (bpWindow != null)
                    bpWindow.Dispose();
                if (assetProxyWindow != null)
                    assetProxyWindow.Dispose();
                if (buffer != null)
                    buffer.Dispose();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                if (!Settings.PreventTaskbarMinimize.Value)
                {
                    cp.Style |= (int)WindowStyle.WS_MINIMIZEBOX;
                }

                return cp;
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!initialized)
            {
                initialized = true;
                Initialize();
                if (canShow)
                    this.Update();
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

            if (Client.Launcher.GetActiveProcessCount(Client.Launcher.AccountType.GuildWars2) == 0)
            {
                Client.Launcher.AccountEventHandler<Client.Launcher.AccountStateEventArgs> onStateChanged = null;
                onStateChanged = delegate(Settings.IAccount a, Client.Launcher.AccountStateEventArgs e)
                {
                    if (a.Type == Settings.AccountType.GuildWars2 && e.State == Client.Launcher.AccountState.ActiveGame)
                    {
                        Client.Launcher.AccountStateChanged -= onStateChanged;
                        if (build > Settings.LastKnownBuild.Value)
                            Settings.LastKnownBuild.Value = build;
                    }
                };
                Client.Launcher.AccountStateChanged += onStateChanged;
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

                foreach (var a in Util.Accounts.GetGw2Accounts())
                {
                    if (a.DatFile == null || dats.Add(a.DatFile))
                    {
                        accounts.Add(a);
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

                    foreach (var a in Util.Accounts.GetGw2Accounts())
                    {
                        if (a.DatFile == null || dats.Add(a.DatFile))
                        {
                            accounts.Add(a);
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
                                    ShowUpdateAccounts(parent, accounts,
                                        delegate
                                        {
                                            Settings.LastKnownBuild.Value = build;
                                        });
                                }
                            }
                        }
                    }
                }
            }
        }

        void resizingEvents_EnabledChanged(object sender, bool e)
        {
            if (e)
            {
                this.LocationChanged += formMain_LocationChanged;
                this.SizeChanged += formMain_SizeChanged;
            }
            else
            {
                this.LocationChanged -= formMain_LocationChanged;
                this.SizeChanged -= formMain_SizeChanged;
            }
        }

        void formMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.AutoSizeGrid = false;
                Settings.WindowBounds[typeof(formMain)].Value = this.Bounds;
            }
        }

        void formMain_LocationChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                var s = Settings.WindowBounds[typeof(formMain)];

                if (s.Value.Size.IsEmpty)
                    s.Value = new Rectangle(this.Location, Size.Empty);
                else
                    s.Value = this.Bounds;
            }
        }

        void Launcher_AllQueuedLaunchesCompleteAllAccountsExited(object sender, EventArgs e)
        {
            if (Settings.ShowKillAllAccounts.Value)
            {
                Util.Invoke.Async(this,
                    delegate
                    {
                        buttonKillAll.Visible = false;
                    });
            }
        }

        void Launcher_AnyActiveProcessCountChanged(Client.Launcher.AccountType type, ushort e)
        {
            var b = e > 0;

            if (mpWindow.ShowCloseAll != b)
            {
                Util.Invoke.Async(this,
                    delegate
                    {
                        mpWindow.ShowCloseAll = b;

                        if (!b || Settings.ShowKillAllAccounts.Value)
                            buttonKillAll.Visible = b || Client.Launcher.GetPendingLaunchCount() > 0;
                    });
            }

            if (e == 0)
            {
                if (Settings.BringToFrontOnExit.Value && !Settings.Silent && updatingWindow == null && Client.Launcher.GetPendingLaunchCount() == 0)
                {
                    Util.Invoke.Async(this,
                        delegate
                        {
                            ShowToFront();
                        });
                }
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

        private void OnMinimized()
        {
            if (Settings.ShowTray.Value && Settings.MinimizeToTray.Value)
                MinimizeToTray();
        }

        private async void MinimizeToTray()
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                if (this.Visible && !await FadeTo(0, FADE_DURATION))
                    return;
                this.WindowState = FormWindowState.Minimized;
            }

            //await Task.Delay(200);

            this.Hide();
        }

        private async void ShowToFront(bool immediate = false, bool focus = true)
        {
            if (!this.Visible)
            {
                this.Opacity = 0;
                this.Show();
            }

            if (this.Opacity < 1 || Fading != null)
            {
                this.WindowState = FormWindowState.Normal;

                if (focus)
                {
                    try
                    {
                        await Windows.FindWindow.FocusWindowAsync(this.Handle, true);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                if (immediate)
                {
                    FadeTo(1);
                }
                else if (!await FadeTo(1, FADE_DURATION))
                    return;
            }
            else
            {
                if (WindowState != FormWindowState.Normal)
                {
                    if (!immediate)
                        await Task.Delay(10);

                    this.WindowState = FormWindowState.Normal;
                }

                if (focus)
                {
                    try
                    {
                        await Windows.FindWindow.FocusWindowAsync(this.Handle, true);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }

        void Launcher_AccountExited(Settings.IAccount account)
        {
            if (account.RecordLaunches)
            {
                Tools.Statistics.Record(Tools.Statistics.RecordType.Exited, account.UID);
            }
        }

        private void SetFocusedAccount(Settings.IAccount account)
        {
            AccountGridButton button;
            if (buttons.TryGetValue(account.UID, out button))
                SetFocusedButton(button);
        }

        private void SetFocusedButton(AccountGridButton button)
        {
            if (focusedButton == button)
                return;
            else if (focusedButton != null)
                focusedButton.IsFocused = false;

            if (Settings.StyleHighlightFocused.Value && button != null)
                button.IsFocused = true;

            focusedButton = button;
        }

        void Launcher_AccountWindowEvent(Settings.IAccount account, Client.Launcher.AccountWindowEventEventArgs e)
        {
            switch (e.Type)
            {
                case Client.Launcher.AccountWindowEventEventArgs.EventType.Focused:

                    Util.Invoke.Required(this,
                        delegate
                        {
                            SetFocusedAccount(account);
                        });

                    break;
                case Client.Launcher.AccountWindowEventEventArgs.EventType.WindowLoaded:

                    if (account.WindowOptions.HasFlag(Settings.WindowOptions.Windowed | Settings.WindowOptions.PreventChanges) && !account.WindowBounds.IsEmpty && !Settings.IsRunningWine)
                    {
                        lock (this)
                        {
                            if (maskManager == null)
                                maskManager = new formMaskOverlay.Manager();
                        }

                        maskManager.Add(account, e.Process, e.Handle);
                    }

                    break;
            }
        }

        void Launcher_AccountLaunched(Settings.IAccount account)
        {
            account.TotalUses++;

            if (account.RecordLaunches)
            {
                Tools.Statistics.Record(Tools.Statistics.RecordType.Launched, account.UID);
            }

            //note: deleting the cache immediately could potentially delete files being used, but not yet locked

            //if (Settings.DeleteCacheOnLaunch.Value)
            //{
            //    Task.Run(new Action(
            //        delegate
            //        {
            //            try
            //            {
            //                Tools.Gw2Cache.Delete(uid);
            //            }
            //            catch (Exception ex)
            //            {
            //                Util.Logging.Log(ex);
            //            }
            //        }));
            //}

            if (Settings.DeleteCrashLogsOnLaunch.Value && DateTime.UtcNow.Subtract(lastCrashDelete).TotalDays > 1)
            {
                lastCrashDelete = DateTime.UtcNow;

                Task.Factory.StartNew(new Action(
                   delegate
                   {
                       try
                       {
                           Tools.Gw2Logs.Delete();
                       }
                       catch (Exception ex)
                       {
                           Util.Logging.Log(ex);
                       }
                   }));
            }
        }

        void Launcher_NetworkAuthorizationRequired(object sender, Client.Launcher.NetworkAuthorizationRequiredEventsArgs e)
        {
            Util.Invoke.Required(this, delegate
            {
                OnNetworkAuthorizationRequired(e);
            });
        }

        void Launcher_LaunchException(Settings.IAccount account, Client.Launcher.LaunchExceptionEventArgs e)
        {
            if (e.Exception is Client.Launcher.InvalidPathException)
            {
                var type = ((Client.Launcher.InvalidPathException)e.Exception).Type;
                Settings.ISettingValue<string> v;
                string path;

                switch (type)
                {
                    case Client.Launcher.AccountType.GuildWars2:

                        v = Settings.GuildWars2.Path;

                        break;
                    case Client.Launcher.AccountType.GuildWars1:

                        v = Settings.GuildWars1.Path;

                        break;
                    default:

                        return;
                }

                path = v.Value;

                Util.Invoke.Required(this, 
                    delegate
                    {
                        OnSettingsInvalid(type);
                    });

                if (v.Value != path)
                    e.Retry = true;
            }
            else if (e.Exception is Client.Launcher.BadUsernameOrPasswordException)
            {
                string username = Util.Users.GetUserName(account.WindowsAccount);
                Util.Invoke.Required(this, delegate
                {
                    e.Retry = OnPasswordRequired(username) == DialogResult.OK;
                });
            }
            else if (e.Exception is Client.Launcher.DatFileNotInitialized)
            {
                Util.Invoke.Required(this, delegate
                {
                    e.Retry = OnDatFileNotInitialized(account) == DialogResult.OK;
                });
            }
        }

        private DialogResult OnDatFileNotInitialized(Settings.IAccount account)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddMessageBox(this))
                {
                    DialogResult r = MessageBox.Show(f, "The Local.dat file for \"" + account.Name + "\" is being used for the first time. GW2 will not be able to modify your settings while allowing multiple clients to be opened.\n\nWould you like to run GW2 normally so that your settings can be modified?", "Local.dat first use", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                    switch (r)
                    {
                        case DialogResult.Yes:
                            return DialogResult.OK;
                        case DialogResult.No:
                            ((Settings.IGw2Account)account).DatFile.IsInitialized = true;
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
                using (formPassword f = (formPassword)AddWindow(new formPassword(username)))
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

        private void OnSettingsInvalid(Client.Launcher.AccountType type)
        {
            using (BeforeShowDialog())
            {
                using (var f = (formSettings)AddWindow(new formSettings()))
                {
                    f.SampleButton.Width = gridContainer.CurrentButtonWidth;

                    switch (type)
                    {
                        case Client.Launcher.AccountType.GuildWars1:

                            f.SelectPanel(formSettings.Panels.GuildWars1);

                            break;
                        case Client.Launcher.AccountType.GuildWars2:

                            f.SelectPanel(formSettings.Panels.GuildWars2);

                            break;
                    }

                    f.ShowDialog(this);
                }
            }
        }

        private void OnNetworkAuthorizationRequired(Client.Launcher.NetworkAuthorizationRequiredEventsArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formNetworkAuthorizationRequired(e)))
                {
                    f.Shown += delegate
                    {
                        try
                        {
                            Windows.FindWindow.FocusWindow(f.Handle, true);
                        }
                        catch { }
                    };
                    f.ShowDialog(this);
                }
            }
        }

        void Launcher_AccountStateChanged(Settings.IAccount a, Client.Launcher.AccountStateEventArgs e)
        {
            if (Util.Invoke.IfRequiredAsync(this,
                delegate
                {
                    Launcher_AccountStateChanged(a, e);
                }))
                return;

            AccountGridButton button;
            if (buttons.TryGetValue(e.UID, out button))
            {
                if (button.Tag != e.Data)
                {
                    button.Tag = e.Data;
                    OnButtonDataChanged(button, e.Data);
                }

                if (button.AccountData != null)
                {
                    var account = button.AccountData;
                    var exited = e.PreviousState == Client.Launcher.AccountState.ActiveGame;

                    //as a backup in case catching the DX window failed, catch the exit time
                    //if (previousState == Client.Launcher.AccountState.Active && state == Client.Launcher.AccountState.Exited && data is TimeSpan)
                    //{
                    //    TimeSpan elapsed = (TimeSpan)data;
                    //    if (elapsed != TimeSpan.MinValue && elapsed.TotalMinutes > 10)
                    //        exited = true;
                    //}

                    if (exited)
                    {
                        var d = DateTime.UtcNow;
                        account.LastUsedUtc = d;
                        button.LastUsedUtc = d;

                        if (Settings.Sorting.Value.Sorting.Mode == Settings.SortMode.LastUsed || Settings.Sorting.Value.Grouping.HasFlag(Settings.GroupMode.Active))
                            gridContainer.Sort(Settings.Sorting.Value);

                        if (account.Type == Settings.AccountType.GuildWars2)
                        {
                            var gw2 = (Settings.IGw2Account)account;

                            if (gw2.ApiData != null)
                            {
                                var minutes = e.Data is TimeSpan ? (int)((TimeSpan)e.Data).TotalMinutes : 1;

                                if (minutes >= 1)
                                {
                                    var points = gw2.ApiData.DailyPoints;
                                    var played = gw2.ApiData.Played;

                                    if (played != null && played.LastChange.Date != DateTime.UtcNow.Date || points != null && points.Value < Api.Account.MAX_AP && points.LastChange.Date != DateTime.UtcNow.Date)
                                        QueuedAccountApi.Schedule(gw2, null, 0);
                                }
                            }
                        }
                    }
                    else if (e.PreviousState == Client.Launcher.AccountState.Active && e.State != Client.Launcher.AccountState.ActiveGame && Settings.Sorting.Value.Grouping.HasFlag(Settings.GroupMode.Active))
                    {
                        gridContainer.Sort(Settings.Sorting.Value);
                    }

                    if (account.Type == Settings.AccountType.GuildWars2)
                    {
                        var gw2 = (Settings.IGw2Account)account;

                        if (e.State == Client.Launcher.AccountState.Active && gw2.ApiData != null)
                        {
                            //note that played time is always checked on the daily launch because what was previously stored on exit is outdated due to the API giving cached responses
                            //whereas points only need to be checked if the state isn't okay, as it only updates once per day

                            var points = gw2.ApiData.DailyPoints;

                            var checkPlayed = gw2.ApiData.Played != null;
                            var checkPoints = points != null && points.State != Settings.ApiCacheState.OK && points.Value < Api.Account.MAX_AP && points.LastChange.Date != DateTime.UtcNow.Date;

                            if (checkPoints || checkPlayed)
                            {
                                if (account.LastUsedUtc.Date != DateTime.UtcNow.Date)
                                {
                                    QueuedAccountApi.Schedule(gw2, gw2.LastUsedUtc.Date, 0);
                                }
                                else
                                {
                                    if (checkPoints)
                                        gw2.ApiData.DailyPoints.State = Settings.ApiCacheState.Pending;
                                }
                            }
                        }
                    }

                    if (screenshotMonitor != null)
                    {
                        if (exited)
                            screenshotMonitor.Remove(account);
                        else if (e.State == Client.Launcher.AccountState.ActiveGame)
                            screenshotMonitor.Add(account);
                    }
                }

                switch (e.State)
                {
                    case Client.Launcher.AccountState.Active:
                        button.SetStatus("active",  AccountGridButton.StatusColors.Ok);
                        if (Settings.Sorting.Value.Grouping.HasFlag(Settings.GroupMode.Active))
                            gridContainer.Sort(Settings.Sorting.Value);
                        break;
                    case Client.Launcher.AccountState.ActiveGame:
                        button.SetStatus("active", AccountGridButton.StatusColors.Ok);
                        DateTime d = DateTime.UtcNow;
                        if (button.AccountData != null)
                            button.AccountData.LastUsedUtc = d;
                        button.LastUsedUtc = d;
                        if (Settings.Sorting.Value.Sorting.Mode == Settings.SortMode.LastUsed || Settings.Sorting.Value.Grouping.HasFlag(Settings.GroupMode.Active))
                            gridContainer.Sort(Settings.Sorting.Value);
                        break;
                    case Client.Launcher.AccountState.Waiting:
                        button.SetStatus("waiting", AccountGridButton.StatusColors.Waiting);
                        break;
                    case Client.Launcher.AccountState.Launching:
                        button.SetStatus("launching", AccountGridButton.StatusColors.Waiting);
                        break;
                    case Client.Launcher.AccountState.None:
                        button.SetStatus(null, AccountGridButton.StatusColors.Default);
                        break;
                    case Client.Launcher.AccountState.Updating:
                    case Client.Launcher.AccountState.UpdatingVisible:
                        button.SetStatus("updating", AccountGridButton.StatusColors.Waiting);
                        break;
                    case Client.Launcher.AccountState.WaitingForOtherProcessToExit:
                        button.SetStatus("close GW2 to continue", AccountGridButton.StatusColors.Waiting);
                        break;
                    case Client.Launcher.AccountState.WaitingForAuthentication:
                        button.SetStatus("authenticating", AccountGridButton.StatusColors.Waiting);
                        break;
                    case Client.Launcher.AccountState.Error:
                        button.SetStatus("failed", AccountGridButton.StatusColors.Error);
                        break;
                    case Client.Launcher.AccountState.Exited:
                        if (focusedButton == button)
                            SetFocusedButton(null);
                        break;
                }
            }
        }

        void accountApi_DataReceived(object sender, Tools.QueuedAccountApi.DataEventArgs e)
        {
            if (Util.Invoke.IfRequiredAsync(this,
                delegate
                {
                    accountApi_DataReceived(sender, e);
                }))
                return;

            var d = e.Account.ApiData;
            if (d == null)
                return;

            var played = d.Played;
            var points = d.DailyPoints;

            var total = (ushort)(e.Response.DailyAP + e.Response.MonthlyAP);
            bool updatedPoints = false,
                 updatedPlayed = false;
            sbyte _isActive = 0;

            Func<bool> isActive = delegate
            {
                if (_isActive == 0)
                {
                    switch (Client.Launcher.GetState(e.Account))
                    {
                        case Client.Launcher.AccountState.Active:
                        case Client.Launcher.AccountState.ActiveGame:
                            _isActive = 1;
                            break;
                        default:
                            _isActive = -1;
                            break;
                    }
                }

                return _isActive == 1;
            };

            if (e.Data is DateTime)
            {
                //this was the first launch of the day, so this data is for the last recorded launch date
                if (points != null)
                {
                    points.LastChange = (DateTime)e.Data;
                    updatedPoints = true;
                }
                if (played != null)
                {
                    played.LastChange = (DateTime)e.Data;
                    updatedPlayed = true;
                }
            }
            else if (e.Date.Date != DateTime.UtcNow.Date)
            {
                //the day changed, this data is no longer valid
                if (points != null && (points.State == Settings.ApiCacheState.None || !isActive()))
                {
                    points.LastChange = e.Date;
                    updatedPoints = true;
                }
                if (played != null && (played.State == Settings.ApiCacheState.None || !isActive()))
                {
                    played.LastChange = e.Date;
                    updatedPlayed = true;
                }
            }
            else
            {
                bool rescheduled = false;

                if (e.Data is Settings.ApiCacheState)
                {
                    //only looking to update those with the specified state
                    var state = (Settings.ApiCacheState)e.Data;
                    if (points != null && points.State != state)
                        points = null;
                    if (played != null && played.State != state)
                        played = null;
                }

                if (points != null)
                {
                    if (points.State == Settings.ApiCacheState.None)
                    {
                        points.LastChange = DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0));
                        updatedPoints = true;
                    }
                    else if (points.Value != total)
                    {
                        points.LastChange = e.Date;
                        updatedPoints = true;
                    }
                    else if (!rescheduled)
                    {
                        //either the daily wasn't completed, or the api hasn't updated yet, which could be 10 minutes behind
                        //- the api response is cached for 5 minutes and can be another 5 minutes behind

                        if (e.Response.LastModified < e.DateScheduled)
                        {
                            if (rescheduled = e.Attempt == 0 || e.ResponsePreviousAttempt.Age != e.Response.Age && e.Attempt < 2)
                                e.Reschedule(330000);
                        }
                    }
                }
                if (played != null)
                {
                    if (played.State == Settings.ApiCacheState.None)
                    {
                        played.LastChange = e.Account.LastUsedUtc;
                        updatedPlayed = true;
                    }
                    else if (played.Value != e.Response.Age)
                    {
                        played.LastChange = e.Date;
                        updatedPlayed = true;
                    }
                    else if (!rescheduled)
                    {
                        if (e.Response.LastModified < e.DateScheduled)
                        {
                            if (rescheduled = e.Attempt == 0)
                                e.Reschedule(330000);
                        }
                    }
                }
            }

            if (updatedPoints)
            {
                points.Value = total;

                if (points.LastChange.Date != DateTime.UtcNow.Date && isActive())
                    points.State = Settings.ApiCacheState.Pending;
                else
                    points.State = Settings.ApiCacheState.OK;

                if (e.Account.ShowDailyCompletion)
                {
                    e.Account.LastDailyCompletionUtc = points.LastChange;

                    AccountGridButton b;
                    if (buttons.TryGetValue(e.Account.UID, out b))
                        b.LastDailyCompletionUtc = points.LastChange;
                }
            }
            if (updatedPlayed)
            {
                played.Value = e.Response.Age;
                played.State = Settings.ApiCacheState.Pending; //it's always outdated

                if (e.Account.ShowDailyLogin)
                {
                    AccountGridButton b;
                    if (buttons.TryGetValue(e.Account.UID, out b))
                        b.LastDailyLoginUtc = played.LastChange;
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
            var fontName = Settings.FontName.HasValue ? Settings.FontName.Value : UI.Controls.AccountGridButton.FONT_NAME;
            var fontStatus = Settings.FontStatus.HasValue ? Settings.FontStatus.Value : UI.Controls.AccountGridButton.FONT_STATUS;
            var fontUser = Settings.FontUser.HasValue ? Settings.FontUser.Value : UI.Controls.AccountGridButton.FONT_USER;
            bool showAccount = !Settings.StyleShowAccount.HasValue || Settings.StyleShowAccount.Value,
                 showColor = Settings.StyleShowColor.Value,
                 showIcon = Settings.StyleShowIcon.Value;
            var colors =  Settings.StyleColors.HasValue ?  Settings.StyleColors.Value : AccountGridButton.DefaultColors;

            gridContainer.SetStyle(fontName, fontStatus, fontUser, showAccount, showColor, showIcon, colors);
        }

        void OnButtonStyleChanged(object sender, EventArgs e)
        {
            Util.Invoke.Async(this, OnStyleChanged);
        }

        void StyleHighlightFocused_ValueChanged(object sender, EventArgs e)
        {
            if (focusedButton != null)
                focusedButton.IsFocused = ((Settings.ISettingValue<bool>)sender).Value;
        }

        void SettingsWindowBounds_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<Rectangle>;
            if (!setting.HasValue)
            {
                AutoSizeGrid = true;

                var size = ResizeAuto(Point.Empty);
                var bounds = Screen.PrimaryScreen.WorkingArea;
                var l = Point.Add(bounds.Location, new Size(bounds.Width / 2 - size.Width / 2, bounds.Height / 2 - size.Height / 2 + this.Height - this.ClientSize.Height));

                if (l.Y + size.Height > bounds.Bottom)
                {
                    l.Y = bounds.Bottom - size.Height;
                    if (l.Y < bounds.Top)
                    {
                        l.Y = bounds.Top;
                    }
                }

                size = ResizeAuto(l);

                this.Bounds = new Rectangle(l, size);
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

        void TopMost_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<bool>;

            this.TopMost = setting.Value;
        }

        void ScreenshotSettings_ValueChanged(object sender, EventArgs e)
        {
            var enabled = Settings.ScreenshotConversion.HasValue || Settings.ScreenshotNaming.HasValue;
            if (enabled)
            {
                if (screenshotMonitor == null)
                {
                    screenshotMonitor = new Tools.Screenshots();
                    screenshotMonitor.ScreenshotProcessed += screenshotMonitor_ScreenshotProcessed;
                    if (Client.Launcher.GetActiveProcessCount() > 0)
                    {
                        foreach (var a in Util.Accounts.GetAccounts())
                        {
                            if (Client.Launcher.GetState(a) == Client.Launcher.AccountState.ActiveGame)
                                screenshotMonitor.Add(a);
                        }
                    }
                }
            }
            else if (screenshotMonitor != null)
            {
                screenshotMonitor.Dispose();
                screenshotMonitor = null;
            }
        }

        void screenshotMonitor_ScreenshotProcessed(object sender, string e)
        {
            if (!Settings.ScreenshotNotifications.HasValue)
                return;

            var attach = Settings.ScreenshotNotifications.Value;
            Image image;

            try
            {
                image = Image.FromFile(e);
            }
            catch 
            {
                return;
            }

            Util.Invoke.Async(this,
                delegate
                {
                    UI.formNotify.ShowImage(attach.Screen, attach.Anchor, "", image, true);
                });
        }

        void ProcessPriority_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<Settings.ProcessPriorityClass>;

            try
            {
                using (var p = Process.GetCurrentProcess())
                {
                    if (setting.HasValue)
                        p.PriorityClass = setting.Value.ToProcessPriorityClass();
                    else
                        p.PriorityClass = ProcessPriorityClass.Normal;
                }
            }
            catch { }
        }

        void NotesNotificationsHasValue_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<Settings.NotificationScreenAttachment>;

            if (!setting.HasValue)
            {
                setting.ValueChanged -= NotesNotificationsHasValue_ValueChanged;
                setting.ValueChanged += NotesNotificationsNoValue_ValueChanged;
            }
        }

        void NotesNotificationsNoValue_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<Settings.NotificationScreenAttachment>;

            if (setting.HasValue)
            {
                setting.ValueChanged -= NotesNotificationsNoValue_ValueChanged;
                setting.ValueChanged += NotesNotificationsHasValue_ValueChanged;

                Util.ScheduledEvents.Register(OnNoteCallback, 0);
            }
        }

        void ShowDailies_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<Settings.DailiesMode>;

            if (setting.Value.HasFlag(Settings.DailiesMode.Show))
            {
                if (dailies == null || dailies.IsDisposed)
                {
                    dailies = new formDailies(this);
                    OnChildFormCreated(dailies);

                    dailies.VisibleChanged += dailies_VisibleChanged;
                    dailies.Minimize(false);
                }
            }
            else if (dailies != null)
            {
                dailies.Dispose();
                dailies = null;
            }
        }

        void dailies_VisibleChanged(object sender, EventArgs e)
        {
            if (dailies.Visible && dailies.LinkedToParent && !this.Visible)
            {
                ShowToFront();
            }
        }

        void bp_Error(object sender, string message, Exception exception)
        {
            if (Util.Invoke.IfRequiredAsync(this,
                delegate
                {
                    bp_Error(sender, message, exception);
                }))
            {
                return;
            }

            DisposeBpProgress();

            if (Settings.BackgroundPatchingNotifications.HasValue)
            {
                var v = Settings.BackgroundPatchingNotifications.Value;
                formNotify.Show(formNotify.NotifyType.Error, v.Screen, v.Anchor, new Tools.BackgroundPatcher.PatchEventArgs());
            }
        }

        void bp_Complete(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequiredAsync(this,
                delegate
                {
                    bp_Complete(sender, e);
                }))
            {
                return;
            }

            DisposeBpProgress();
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
            //    formNotify.Show(formNotify.NotifyType.DownloadingFiles, e);
        }

        void bp_PatchBeginning(object sender, Tools.BackgroundPatcher.PatchEventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    bp_PatchBeginning(sender, e);
                }))
            {
                return;
            }

            DisposeBpProgress();

            if (Settings.BackgroundPatchingNotifications.HasValue)
            {
                var v = Settings.BackgroundPatchingNotifications.Value;
                formNotify.Show(formNotify.NotifyType.DownloadingManifests, v.Screen, v.Anchor, e);
            }

            if (Settings.BackgroundPatchingProgress.HasValue)
            {
                bpProgress = new formProgressOverlay()
                {
                    Bounds = Settings.BackgroundPatchingProgress.Value,
                };
                bpProgress.Progress.Maximum = 1000;
                bpProgress.Show();
                Tools.BackgroundPatcher.Instance.ProgressChanged += bp_ProgressChanged;
            }
        }

        void bp_ProgressChanged(object sender, float e)
        {
            var p = bpProgress.Progress;
            p.Value = (long)(e * p.Maximum);
        }

        void bp_PatchReady(object sender, Tools.BackgroundPatcher.PatchEventArgs e)
        {
            if (Util.Invoke.IfRequiredAsync(this,
                delegate
                {
                    bp_PatchReady(sender, e);
                }))
            {
                return;
            }

            DisposeBpProgress();

            if (Settings.BackgroundPatchingNotifications.HasValue)
            {
                var v = Settings.BackgroundPatchingNotifications.Value;
                formNotify.Show(formNotify.NotifyType.PatchReady, v.Screen, v.Anchor, e);
            }

            if (Settings.AutoUpdate.Value && Settings.LocalAssetServerEnabled.Value)
                DoAutoUpdate(e.Build);
        }

        private void DisposeBpProgress()
        {
            if (bpProgress != null)
            {
                Tools.BackgroundPatcher.Instance.ProgressChanged -= bp_ProgressChanged;
                using (bpProgress)
                {
                    bpProgress.Visible = false;
                }
                bpProgress = null;
            }
        }

        private void LoadSettings()
        {
            OnStyleChanged();

            if (!Settings.Sorting.Value.IsEmpty)
            {
                var o = Settings.Sorting.Value;

                SetSorting(o.Sorting.Mode, o.Sorting.Descending, false);
                SetGrouping(o.Grouping.Mode, o.Grouping.Descending, true);
            }

            ScreenshotSettings_ValueChanged(Settings.ScreenshotConversion, EventArgs.Empty);
            StyleBackgroundImage_ValueChanged(Settings.StyleBackgroundImage, EventArgs.Empty);
            buttonLaunchAll.Visible = Settings.ShowLaunchAllAccounts.Value;
            ProcessPriority_ValueChanged(Settings.ProcessPriority, EventArgs.Empty);
        }

        private void LoadAccounts()
        {
            var pages = 0;

            gridContainer.SuspendAdd();

            foreach (var a in Util.Accounts.GetAccounts())
            {
                AddAccount(a, false);

                if (a.Pages != null)
                {
                    var pg = a.Pages[a.Pages.Length - 1].Page;
                    if (pg > pages)
                        pages = pg;
                }
            }

            gridContainer.ResumeAdd();

            if (pages < MAX_PAGES)
                pages++;
            mpWindow.Pages = (byte)pages;

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
                        if (shown)
                            this.Size = ResizeAuto(this.Location);
                    }
                    else
                        gridContainer.ContentHeightChanged -= gridContainer_ContentHeightChanged;
                }
            }
        }

        void gridContainer_ContentHeightChanged(object sender, EventArgs e)
        {
            if (shown && string.IsNullOrEmpty(gridContainer.Filter))
                this.Size = ResizeAuto(this.Location);
        }

        protected Size ResizeAuto(Point location)
        {
            if (resizingEvents != null)
                resizingEvents.Enabled = false;

            var screen = Screen.FromControl(this).WorkingArea;
            var columns = Settings.StyleColumns.Value;
            if (columns < 1)
                columns = 1;
            int height = this.Height - gridContainer.ClientSize.Height + gridContainer.GetContentHeight(columns); //gridContainer.GetContentHeight(1) + (this.Height - this.ClientSize.Height) + panelContainer.Location.Y + gridContainer.Location.Y * 2 + 2 + panelBottom.Bottom - panelContainer.Bottom;
            int width = this.Width - gridContainer.Width + Scale(250); //(int)(250 * scale + 0.5f) + (this.Width - this.ClientSize.Width) + panelContainer.Location.X * 2 + gridContainer.Location.X * 2 + 2;
            
            if (location.Y + height > screen.Bottom)
            {
                height = screen.Bottom - location.Y;
                var min = Scale(100);
                if (height < min)
                    height = min;
            }

            return new Size(width, height);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            labelTitle.ForeColor = Color.Black;
            buttonClose.ForeColor = buttonMenu.ForeColor = Color.FromArgb(100, 100, 100);
            buttonMinimize.ForeColor = Color.FromArgb(128, 128, 128);

            if (this.Opacity < 1 && Fading == null)
            {
                var t = FadeTo(1, FADE_DURATION);
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            labelTitle.ForeColor = Color.FromArgb(140, 140, 140);
            buttonClose.ForeColor = buttonMenu.ForeColor = Color.FromArgb(150, 150, 150);
            buttonMinimize.ForeColor = Color.FromArgb(168, 168, 168);
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

            if (e.Cancel)
                return;

            //Task t;
            //if (Client.Launcher.HasPendingNetworkAuthorizationRequests)
            //{
            //    t = Task.Run(new Func<Task>(
            //        delegate
            //        {
            //            return Client.Launcher.ClearNetworkAuthorization(false, true);
            //        }));
            //}
            //else
            //    t = null;

            if (Net.AssetProxy.ServerController.Enabled)
            {
                var s = Net.AssetProxy.ServerController.Server;
                if (s != null)
                    s.Stop();
            }

            //if (t != null)
            //{
            //    try
            //    {
            //        t.Wait(5000);
            //    }
            //    catch { }
            //}
        }

        void gridContainer_AddAccountClick(object sender, EventArgs e)
        {
            AddAccount();
        }

        void gridContainer_AccountMousePressed(object sender, EventArgs e)
        {
            var button = (AccountGridButton)sender;
            var setting = Settings.ActionActiveLPress;

            try
            {
                switch (Client.Launcher.GetState(button.AccountData))
                {
                    case Client.Launcher.AccountState.Active:
                    case Client.Launcher.AccountState.ActiveGame:

                        if (setting.Value == Settings.ButtonAction.Close || !setting.HasValue)
                            Client.Launcher.Kill(button.AccountData);

                        break;
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        void gridContainer_AccountNoteClicked(object sender, EventArgs e)
        {
            ShowNotesDialog((AccountGridButton)sender);
        }

        void gridContainer_AccountBeginMouseClick(object sender, AccountGridButtonContainer.MousePressedEventArgs e)
        {
            var button = (AccountGridButton)sender;
            if (button.AccountData == null)
                return;

            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:

                    gridContainer.ClearSelected();

                    break;
                case System.Windows.Forms.MouseButtons.Middle:
                    break;
                default:
                    return;
            }

            var state = Client.Launcher.GetState(button.AccountData);
            Settings.ButtonAction action;
            Settings.ISettingValue<Settings.ButtonAction> v;
            bool isActive;

            action = Settings.ButtonAction.None;

            switch (state)
            {
                case Client.Launcher.AccountState.Active:
                case Client.Launcher.AccountState.ActiveGame:

                    isActive = true;

                    break;
                default:

                    isActive = false;

                    break;
            }

            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:

                    if (isActive)
                        v = Settings.ActionActiveLClick;
                    else
                        v = Settings.ActionInactiveLClick;

                    if (v.HasValue)
                    {
                        action = v.Value;
                    }
                    else
                    {
                        if (isActive)
                            action = Settings.ButtonAction.Focus;
                        else
                            action = Settings.ButtonAction.Launch;
                    }

                    break;
                case System.Windows.Forms.MouseButtons.Middle:

                    if (isActive)
                        v = Settings.ActionActiveMClick;
                    else
                        v = Settings.ActionInactiveMClick;

                    if (v.HasValue)
                    {
                        action = v.Value;
                    }
                    else
                    {
                        if (isActive)
                            action = Settings.ButtonAction.ShowAuthenticator;
                        else
                            action = Settings.ButtonAction.ShowAuthenticator;
                    }

                    break;
                default:

                    return;
            }

            switch (action)
            {
                case Settings.ButtonAction.None:

                    return;
                case Settings.ButtonAction.Focus:
                case Settings.ButtonAction.FocusAndCopyAuthenticator:

                    var p = Client.Launcher.GetProcess(button.AccountData);
                    if (p != null)
                    {
                        e.Handled = true;
                        e.FlashColor = button.Colors[Settings.AccountGridButtonColors.Colors.ActionFocusFlash];

                        FocusWindowAsync(p);
                    }

                    break;
                case Settings.ButtonAction.Close:

                    e.Handled = true;
                    e.FlashColor = button.Colors[Settings.AccountGridButtonColors.Colors.ActionExitFlash];

                    Client.Launcher.Kill(button.AccountData);

                    break;
                case Settings.ButtonAction.LaunchSingle:

                    Client.Launcher.Launch(button.AccountData, Client.Launcher.LaunchMode.LaunchSingle);

                    break;
                case Settings.ButtonAction.Launch:

                    Client.Launcher.Launch(button.AccountData, Client.Launcher.LaunchMode.Launch);

                    break;
            }

            switch (action)
            {
                case Settings.ButtonAction.CopyAuthenticator:
                case Settings.ButtonAction.FocusAndCopyAuthenticator:
                case Settings.ButtonAction.ShowAuthenticator:

                    var totp = button.AccountData.TotpKey;
                    if (totp != null)
                    {
                        try
                        {
                            var ticks = DateTime.UtcNow.Ticks;
                            var remaining = Tools.Totp.GetRemainingTicks();
                            if (remaining < 30000000)
                            {
                                ticks += Tools.Totp.OTP_LIFESPAN_TICKS;
                                remaining += (int)Tools.Totp.OTP_LIFESPAN_TICKS;
                            }
                            var code = new String(Tools.Totp.Generate(totp, ticks));
                            button.ShowTotpCode(code, remaining / 10000);
                            if (action != Settings.ButtonAction.ShowAuthenticator)
                                Clipboard.SetText(code);
                        }
                        catch { }
                    }

                    break;
            }
        }

        void gridContainer_AccountBeginMousePressed(object sender, AccountGridButtonContainer.MousePressedEventArgs e)
        {
            var button = (AccountGridButton)sender;
            var setting = Settings.ActionActiveLPress;

            try
            {
                switch (Client.Launcher.GetState(button.AccountData))
                {
                    case Client.Launcher.AccountState.Active:
                    case Client.Launcher.AccountState.ActiveGame:

                        if (setting.Value == Settings.ButtonAction.Close || !setting.HasValue)
                        {
                            e.Handled = true;
                            e.FlashColor = button.Colors[Settings.AccountGridButtonColors.Colors.ActionExitFlash];
                            e.FillColor = button.Colors[Settings.AccountGridButtonColors.Colors.ActionExitFill];
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        void gridContainer_AccountBeginDrag(object sender, HandledMouseEventArgs e)
        {
            var button = (AccountGridButton)sender;

            try
            {
                if (dragHelper == null)
                    dragHelper = Windows.DragHelper.Initialize(this);

                Icon icon = null;
                string pathIcon = null;

                if (button.AccountData != null)
                {
                    switch (button.AccountData.Type)
                    {
                        case Settings.AccountType.GuildWars2:

                            pathIcon = Settings.GuildWars2.Path.Value;
                            icon = Gw2Launcher.Properties.Resources.Gw2;

                            break;
                        case Settings.AccountType.GuildWars1:

                            pathIcon = Settings.GuildWars1.Path.Value;
                            icon = Gw2Launcher.Properties.Resources.Gw1;

                            break;
                    }
                }

                if (!Settings.UseDefaultIconForShortcuts.Value || !File.Exists(pathIcon))
                {
                    pathIcon = null;
                    icon = Gw2Launcher.Properties.Resources.Gw2Launcher;
                }

                using (icon = new Icon(icon, 64, 64))
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
                            var stream = new System.IO.MemoryStream(3000);
                            Windows.Shortcut.Create(stream, path, "-l:silent -l:uid:" + account.UID, "Launch " + account.Name, pathIcon);
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
                            e.Handled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        void gridContainer_AccountSelection(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
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

                //disableAutomaticLoginsToolStripMenuItem1.Enabled = (int)disableAutomaticLoginsToolStripMenuItem1.Tag > 0;
                applyWindowedBoundsToolStripMenuItem1.Enabled = (int)applyWindowedBoundsToolStripMenuItem1.Tag > 0;

                contextMenu.Tag = null;

                Swap(contextSelection.Items, selectedToolStripMenuItem.DropDownItems);

                OnContextOpening();

                contextSelection.Show(Cursor.Position);
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
                    editmultipleToolStripMenuItem.Visible = false;

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
                    editmultipleToolStripMenuItem.Visible = selected.Count > 1;

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
                //disableAutomaticLoginsToolStripMenuItem1.Enabled = (int)disableAutomaticLoginsToolStripMenuItem1.Tag > 0;
                applyWindowedBoundsToolStripMenuItem1.Enabled = (int)applyWindowedBoundsToolStripMenuItem1.Tag > 0;

                contextMenu.Tag = button;

                contextMenu.Show(Cursor.Position);
            }
            else if (e.Button == MouseButtons.Left)
            {

            }
        }

        private async void FocusWindowAsync(Process p)
        {
            try
            {
                await Task.Run(
                    delegate
                    {
                        Windows.FindWindow.FocusWindow(p);
                    });
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
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
                            using (e.Response)
                            {
                                switch (((System.Net.HttpWebResponse)e.Response).StatusCode)
                                {
                                    case System.Net.HttpStatusCode.NotFound:
                                    case System.Net.HttpStatusCode.Moved:
                                    case System.Net.HttpStatusCode.Redirect:
                                        return -2;
                                }
                            }
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
                Settings.LastProgramVersion.Value = new Settings.LastCheckedVersion(DateTime.UtcNow, (ushort)v);

                if (v > Program.RELEASE_VERSION)
                {
                    var fc = new formChangelog();

                    if (!await fc.LoadChangelog())
                    {
                        fc.Dispose();
                        fc = null;
                    }

                    if (!this.Visible || !this.ContainsFocus)
                    {
                        EventHandler onActivated = null;
                        onActivated = delegate
                        {
                            this.Activated -= onActivated;
                            ShowNewVersion(fc);
                        };
                        this.Activated += onActivated;
                    }
                    else
                    {
                        ShowNewVersion(fc);
                    }
                }
                else
                {
                    Util.ScheduledEvents.Register(OnCheckVersionCallback, int.MaxValue);
                }
            }
            else if (v == -2) //no longer exists
            {
                Settings.LastProgramVersion.Value = new Settings.LastCheckedVersion(DateTime.UtcNow, 0);
            }
            else
            {
                //failed, retry in 12 hours
                Util.ScheduledEvents.Register(OnCheckVersionCallback, 43200000);
            }
        }

        private async void ShowNewVersion(formChangelog fc)
        {
            await Task.Delay(1000);

            if (activeWindows > 0)
            {
                EventHandler<int> onChanged = null;
                onChanged = delegate(object o, int count)
                {
                    if (count == 0)
                    {
                        ActiveWindowsChanged -= onChanged;
                        ShowNewVersion(fc);
                    }
                };
                ActiveWindowsChanged += onChanged;
                return;
            }

            using (BeforeShowDialog())
            {
                if (fc != null && fc.IsDisposed)
                    fc = null;
                using (var parent = fc == null ? AddMessageBox(this) : null)
                {
                    using (fc)
                    {
                        if (fc != null && fc.ShowDialog(this) == System.Windows.Forms.DialogResult.Yes ||
                            fc == null && MessageBox.Show(parent, "A new version of Gw2Launcher is available.\n\nWould you like to update now?", "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            using (formVersionUpdate f = (formVersionUpdate)AddWindow(new formVersionUpdate()))
                            {
                                f.ShowDialog(this);
                            }
                        }
                        else
                        {
                            Util.ScheduledEvents.Register(OnCheckVersionCallback, int.MaxValue);
                        }
                    }
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = (formSettings)AddWindow(new formSettings()))
                {
                    f.SampleButton.Width = gridContainer.CurrentButtonWidth;

                    f.ShowDialog(this);
                }
            }
        }

        private Form AddMessageBox(Form owner)
        {
            Form form = new Form();
            form.Owner = owner;
            form.TopMost = owner.TopMost;

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

        private IDisposable BeforeShowDialog(bool ensureVisible = true)
        {
            if (ensureVisible)
            {
                if (this.WindowState == FormWindowState.Minimized || !this.Visible || this.Opacity < 1 || Fading != null)
                {
                    ShowToFront(true, false);
                    //NativeMethods.ShowWindow(this.Handle, ShowWindowCommands.ShowNormal);
                    //if (this.Opacity < 1)
                    //    FadeTo(1);
                }
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

            if (ActiveWindowsChanged != null)
                ActiveWindowsChanged(this, activeWindows);
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
                button.Dispose();

                OnAccountRemoved(account);
            }

            Settings.Accounts[account.UID].Clear();
        }

        private void AddAccount()
        {
            using (BeforeShowDialog())
            {
                var type = Settings.AccountType.GuildWars2;

                if (Settings.GuildWars1.Path.HasValue || !Settings.GuildWars2.Path.HasValue)
                {
                    using (var f = (formAccountType)AddWindow(new formAccountType()))
                    {
                        if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            type = f.Selected;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                using (formAccount f = (formAccount)AddWindow(new formAccount(type)))
                {
                    f.SampleButton.Width = gridContainer.CurrentButtonWidth;
                    f.SampleButton.DefaultBackgroundImage = defaultBackground;

                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        if (mpWindow.Page > 0)
                        {
                            f.Account.Pages = new Settings.PageData[] { new Settings.PageData(mpWindow.Page, ushort.MaxValue) };
                        }

                        if (mpWindow.Page == mpWindow.Pages && mpWindow.Pages < MAX_PAGES)
                            ++mpWindow.Pages;

                        AddAccount(f.Account, true);
                    }
                }
            }
        }

        private void AddAccount(Settings.IAccount account, bool sort)
        {
            var button = new AccountGridButton();
            button.AccountData = account;
            button.DefaultBackgroundImage = defaultBackground;
            if (string.IsNullOrEmpty(account.WindowsAccount))
                button.AccountName = "(current user)";

            button.MouseEnter += button_MouseEnter;
            button.MouseLeave += button_MouseLeave;

            buttons.Add(account.UID, button);

            gridContainer.Add(button);

            if (sort && !Settings.Sorting.Value.IsEmpty)
                gridContainer.Sort(Settings.Sorting.Value);

            OnAccountAdded(account);
        }

        void OnAccountAdded(Settings.IAccount account)
        {
            if (account.Windowed)
                applyWindowedBoundsToolStripMenuItem1.Tag = (int)applyWindowedBoundsToolStripMenuItem1.Tag + 1;
            //if (account.AutomaticLogin)
            //    disableAutomaticLoginsToolStripMenuItem1.Tag = (int)disableAutomaticLoginsToolStripMenuItem1.Tag + 1;

            if (account.Type == Settings.AccountType.GuildWars2)
            {
                var gw2 = (Settings.IGw2Account)account;
                var d = gw2.ApiData;
                if (d != null && (d.Played != null && d.Played.State == Settings.ApiCacheState.None || d.DailyPoints != null && d.DailyPoints.State == Settings.ApiCacheState.None))
                {
                    QueuedAccountApi.Schedule(gw2, Settings.ApiCacheState.None, 0);
                }
            }
        }

        void OnAccountRemoved(Settings.IAccount account)
        {
            if (account.Windowed)
                applyWindowedBoundsToolStripMenuItem1.Tag = (int)applyWindowedBoundsToolStripMenuItem1.Tag - 1;
            //if (account.AutomaticLogin)
            //    disableAutomaticLoginsToolStripMenuItem1.Tag = (int)disableAutomaticLoginsToolStripMenuItem1.Tag - 1;
        }

        void OnBeforeAccountSettingsUpdated(Settings.IAccount account)
        {
            OnAccountRemoved(account);
        }

        void OnAccountSettingsUpdated(Settings.IAccount account, bool cancelled)
        {
            OnAccountAdded(account);
        }

        void OnChildFormCreated(Form form)
        {
            form.Shown += OnChildFormShown;
        }

        void OnChildFormShown(object sender, EventArgs e)
        {
            var form = (Form)sender;
            form.Shown -= OnChildFormShown;

            this.TopMost = Settings.TopMost.Value; //trigger zorder update
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
            {
                tooltip = new formAccountTooltip();
                OnChildFormCreated(tooltip);
            }

            NativeMethods.SetWindowPos(tooltip.Handle, (IntPtr)0, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOOWNERZORDER);

            tooltip.AttachTo(control, null, -Scale(8));
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

        private void SetSorting(Settings.SortMode mode, bool descending, bool applyNow)
        {
            var sorting = Settings.Sorting.Value;
            Settings.Sorting.Value = sorting = new Settings.SortingOptions(mode, descending, sorting.Grouping);

            nameToolStripMenuItem.Checked = mode == Settings.SortMode.Name;
            windowsAccountToolStripMenuItem.Checked = mode == Settings.SortMode.Account;
            lastUsedToolStripMenuItem.Checked = mode == Settings.SortMode.LastUsed;
            sortCustomToolStripMenuItem.Checked = mode == Settings.SortMode.CustomList || mode == Settings.SortMode.CustomGrid;
            if (sortCustomToolStripMenuItem.Checked)
            {
                listToolStripMenuItem.Checked = mode == Settings.SortMode.CustomList;
                gridToolStripMenuItem.Checked = mode == Settings.SortMode.CustomGrid;
            }

            ascendingToolStripMenuItem.Checked = !descending;
            descendingToolStripMenuItem.Checked = descending;

            if (applyNow)
                gridContainer.Sort(sorting);
        }

        private void SetGrouping(Settings.GroupMode mode, bool descending, bool applyNow)
        {
            var sorting = Settings.Sorting.Value;
            Settings.Sorting.Value = sorting = new Settings.SortingOptions(sorting.Sorting, mode, descending);
            
            groupByTypeMenuItem.Checked = (mode & Settings.GroupMode.Type) == Settings.GroupMode.Type;
            groupByActiveMenuItem.Checked = (mode & Settings.GroupMode.Active) == Settings.GroupMode.Active;

            groupByAscendingMenuItem.Checked = !descending;
            groupByDescendingMenuItem.Checked = descending;

            if (applyNow)
                gridContainer.Sort(sorting);
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
            var descending = Settings.Sorting.Value.Sorting.Descending;

            if (sender == nameToolStripMenuItem)
                mode = Settings.SortMode.Name;
            else if (sender == windowsAccountToolStripMenuItem)
                mode = Settings.SortMode.Account;
            else if (sender == lastUsedToolStripMenuItem)
                mode = Settings.SortMode.LastUsed;
            else if (sender == sortCustomToolStripMenuItem)
            {
                if (gridToolStripMenuItem.Checked)
                    mode = Settings.SortMode.CustomGrid;
                else
                    mode = Settings.SortMode.CustomList;
                descending = false;
            }
            else if (sender == listToolStripMenuItem)
                mode = Settings.SortMode.CustomList;
            else if (sender == gridToolStripMenuItem)
                mode = Settings.SortMode.CustomGrid;
            else
                return;

            if (Settings.Sorting.Value.Sorting.Mode == mode)
                mode = Settings.SortMode.None;

            SetSorting(mode, descending, true);
        }

        private void OnGroupingModeClick(object sender)
        {
            Settings.GroupMode mode;
            var descending = Settings.Sorting.Value.Grouping.Descending;

            if (sender == groupByActiveMenuItem)
                mode = Settings.GroupMode.Active;
            else if (sender == groupByTypeMenuItem)
                mode = Settings.GroupMode.Type;
            else
                return;

            var grouping = Settings.Sorting.Value.Grouping.Mode;

            //toggle the selected mode
            grouping = (grouping & ~mode) | (grouping ^ mode) & mode;
            
            SetGrouping(grouping, descending, true);
        }

        private void ascendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!((ToolStripMenuItem)sender).Checked)
                SetSorting(Settings.Sorting.Value.Sorting.Mode, false, true);
        }

        private void descendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!((ToolStripMenuItem)sender).Checked)
                SetSorting(Settings.Sorting.Value.Sorting.Mode, true, true);
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridContainer.ClearSelected();
        }

        private bool OnAccountFileChanged(Settings.IGw2Account account, Client.FileManager.FileType type, Settings.IFile fileBefore, Settings.IFile fileAfter)
        {
            //replace the old file instead of creating a new one if possible
            var replace = false;

            if (fileBefore != null && fileAfter != null && fileBefore.References == 0 && fileAfter.References == 1 && fileBefore.UID != fileAfter.UID)
            {
                string existing = null;

                if (!string.IsNullOrEmpty(fileBefore.Path) && File.Exists(fileBefore.Path))
                {
                    try
                    {
                        File.Delete(fileBefore.Path);
                        existing = fileBefore.Path;
                        fileBefore.Path = null;
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                if (!string.IsNullOrEmpty(fileAfter.Path) && File.Exists(fileAfter.Path))
                {
                    string path;
                    if (existing == null)
                    {
                        try
                        {
                            path = Path.GetTempFileName();
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                            path = Util.FileUtil.GetTemporaryFileName(DataPath.AppDataAccountDataTemp);
                        }
                    }
                    else
                        path = existing;

                    if (path != null)
                    {
                        try
                        {
                            if (File.Exists(path))
                                File.Delete(path);
                            Util.FileUtil.MoveFile(fileAfter.Path, path, false, true);

                            fileAfter.Path = path;
                            replace = true;
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                }
                else
                {
                    fileBefore.Path = fileAfter.Path;
                    replace = true;
                }
            }

            if (replace)
            {
                fileBefore.Path = fileAfter.Path;
                fileBefore.IsInitialized = false;
                fileAfter.Path = null;
                
                switch (type)
                {
                    case Client.FileManager.FileType.Dat:
                        account.DatFile = (Settings.IDatFile)fileBefore;
                        Settings.RemoveDatFile((Settings.IDatFile)fileAfter);
                        break;
                    case Client.FileManager.FileType.Gfx:
                        account.GfxFile = (Settings.IGfxFile)fileBefore;
                        Settings.RemoveGfxFile((Settings.IGfxFile)fileAfter);
                        break;
                }
            }

            return replace;
        }

        private void CheckAccountFiles(IList<Settings.IAccount> accounts, Settings.IFile[] datFilesBefore, Settings.IFile[] gfxFilesBefore)
        {
            var delete = new List<Settings.IFile>(datFilesBefore.Length + gfxFilesBefore.Length);

            for (var i = accounts.Count - 1; i >= 0; --i)
            {
                var account = accounts[i] as Settings.IGw2Account;
                if (account == null)
                    continue;
                var datBefore = (Settings.IDatFile)datFilesBefore[i];
                var gfxBefore = (Settings.IGfxFile)gfxFilesBefore[i];

                if (datBefore != null && datBefore != account.DatFile && OnAccountFileChanged(account, Client.FileManager.FileType.Dat, datBefore, account.DatFile))
                    datBefore = null;
                if (gfxBefore != null && gfxBefore != account.GfxFile && OnAccountFileChanged(account, Client.FileManager.FileType.Gfx, gfxBefore, account.GfxFile))
                    gfxBefore = null;

                if (datBefore != null && datBefore.References == 0 && datBefore != account.DatFile && !string.IsNullOrEmpty(datBefore.Path) && File.Exists(datBefore.Path))
                {
                    if (datBefore.Path.StartsWith(DataPath.AppDataAccountData, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Client.FileManager.Delete(datBefore);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }

                        Settings.DatFiles[datBefore.UID].Clear();
                    }
                    else if (!datBefore.Path.Equals(Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Dat), StringComparison.OrdinalIgnoreCase))
                    {
                        delete.Add(datBefore);
                    }
                }

                if (gfxBefore != null && gfxBefore.References == 0 && gfxBefore != account.GfxFile && !string.IsNullOrEmpty(gfxBefore.Path) && File.Exists(gfxBefore.Path))
                {
                    if (gfxBefore.Path.StartsWith(DataPath.AppDataAccountData, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Client.FileManager.Delete(gfxBefore);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }

                        Settings.GfxFiles[gfxBefore.UID].Clear();
                    }
                    else if (!gfxBefore.Path.Equals(Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Gfx), StringComparison.OrdinalIgnoreCase))
                    {
                        delete.Add(gfxBefore);
                    }
                }
            }

            if (delete.Count > 0)
            {
                var count = 0;
                var sb = new StringBuilder(delete.Count * 250);
                sb.Append("The following file" + (delete.Count == 1 ? " is" : "s are") + " no longer being used:\n");
                foreach (var f in delete)
                {
                    if (++count == 11)
                    {
                        var r = delete.Count - 10;
                        if (r > 5)
                        {
                            sb.AppendLine("... and " + r + " more");
                            break;
                        }
                    }
                    sb.AppendLine(f.Path);
                }
                sb.Append("\n\nWould you like to delete " + (count == 1 ? "it?" : "them?"));

                using (var parent = AddMessageBox(this))
                {
                    if (MessageBox.Show(parent, sb.ToString(), "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        foreach (var f in delete)
                        {
                            try
                            {
                                if (f is Settings.IDatFile)
                                    Client.FileManager.Delete((Settings.IDatFile)f);
                                else if (f is Settings.IGfxFile)
                                    Client.FileManager.Delete((Settings.IGfxFile)f);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }
                        }
                    }
                }

                foreach (var f in delete)
                {
                    if (f is Settings.IDatFile)
                        Settings.DatFiles[f.UID].Clear();
                    else if (f is Settings.IGfxFile)
                        Settings.GfxFiles[f.UID].Clear();
                }
            }
        }

        private void OnEditAccount(AccountGridButton button, IList<AccountGridButton> selected)
        {
            //warning: editing multiple account types is supported, but only type-specific options of the master type will be available

            using (BeforeShowDialog())
            {
                var account = button.AccountData;
                IList<Settings.IAccount> accounts;
                IList<AccountGridButton> buttons;

                if (selected != null)
                {
                    buttons = new List<AccountGridButton>(selected.Count);
                    accounts = new List<Settings.IAccount>(selected.Count);

                    buttons.Add(button);
                    accounts.Add(account);

                    foreach (var b in selected)
                    {
                        if (b.AccountData != null && b != button) // && b.AccountData.Type == account.Type
                        {
                            buttons.Add(b);
                            accounts.Add(b.AccountData);
                        }
                    }
                }
                else
                {
                    accounts = new Settings.IAccount[] { account };
                    buttons = new AccountGridButton[] { button };
                }

                using (formAccount f = (formAccount)AddWindow(new formAccount(accounts)))
                {
                    f.SampleButton.Width = gridContainer.CurrentButtonWidth;
                    f.SampleButton.DefaultBackgroundImage = defaultBackground;

                    var count = accounts.Count;

                    Settings.IFile[] datBefore, gfxBefore;
                    string[] nameBefore, userBefore;

                    datBefore = new Settings.IFile[count];
                    gfxBefore = new Settings.IFile[count];
                    nameBefore = new string[count];
                    userBefore = new string[count];

                    for (var i = 0; i < count; i++ )
                    {
                        nameBefore[i] = accounts[i].Name;
                        userBefore[i] = accounts[i].WindowsAccount;

                        if (accounts[i].Type == account.Type)
                        {
                            switch (account.Type)
                            {
                                case Settings.AccountType.GuildWars1:

                                    var gw1 = (Settings.IGw1Account)accounts[i];
                                    datBefore[i] = gw1.DatFile;

                                    break;
                                case Settings.AccountType.GuildWars2:

                                    var gw2 = (Settings.IGw2Account)accounts[i];
                                    datBefore[i] = gw2.DatFile;
                                    gfxBefore[i] = gw2.GfxFile;

                                    break;
                            }
                        }

                        OnBeforeAccountSettingsUpdated(accounts[i]);
                    }

                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        if (account.Type == Settings.AccountType.GuildWars2)
                            CheckAccountFiles(accounts, datBefore, gfxBefore);

                        var sort = false;

                        for (var i = count - 1; i >= 0; --i)
                        {
                            var a = accounts[i];
                            var b = buttons[i];
                            b.AccountData = a;
                            if (string.IsNullOrEmpty(a.WindowsAccount))
                                b.AccountName = "(current user)";

                            OnAccountSettingsUpdated(a, false);

                            if (!sort)
                            {
                                switch (Settings.Sorting.Value.Sorting.Mode)
                                {
                                    case Settings.SortMode.Account:
                                        sort = userBefore[i] != a.WindowsAccount;
                                        break;
                                    case Settings.SortMode.Name:
                                        sort = nameBefore[i] != a.Name;
                                        break;
                                }
                            }

                            gridContainer.RefreshFilter(button, i == 0 && !sort);
                        }

                        if (sort)
                            gridContainer.Sort(Settings.Sorting.Value);
                    }
                    else
                    {
                        foreach (var a in accounts)
                        {
                            OnAccountSettingsUpdated(a, true);
                        }
                    }
                }
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var button = contextMenu.Tag as AccountGridButton;
            if (button != null && button.AccountData != null)
            {
                OnEditAccount(button, null);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selected = GetSelected();
            if (selected.Count == 0)
                return;

            int accounts = 0;
            var names = new StringBuilder();

            foreach (var b in selected)
            {
                var account = b.AccountData;
                if (account == null)
                {
                    gridContainer.Remove(b);
                    b.Dispose();
                    continue;
                }

                names.Append('"');
                names.Append(account.Name);
                names.Append("\", ");
                accounts++;
            }

            if (accounts == 0)
                return;

            names.Length -= 2;
            string message = "Are you sure you want to delete ";

            if (accounts == 1)
                message += names.ToString() + "?";
            else
                message += "the following accounts?\n\n" + names.ToString();

            using (BeforeShowDialog())
            {
                using (var parent = AddMessageBox(this))
                {
                    if (MessageBox.Show(parent, message, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;

                    var sids = new HashSet<ushort>();

                    foreach (var b in selected)
                    {
                        var account = b.AccountData;
                        if (account == null)
                            continue;

#warning Unchecked delete IFiles by FileManager on account deletion
                        try
                        {
                            Client.FileManager.Delete(account);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }

                        if (account.Notes != null)
                        {
                            foreach (var n in account.Notes)
                            {
                                sids.Add(n.SID);
                            }
                        }
                        
                        RemoveAccount(account);
                    }

                    if (sids.Count > 0)
                    {
                        Task.Run(new Action(
                            delegate
                            {
                                using (var store = new Tools.Notes())
                                {
                                    foreach (var sid in sids)
                                    {
                                        store.Remove(sid);
                                    }
                                }
                            }));
                    }
                }
            }

            UpdatePageCount();
        }

        private void updateAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateAccounts(GetVisible());
        }

        private void UpdateAccounts(IEnumerable<AccountGridButton> buttons)
        {
            var accounts = new List<Settings.IAccount>(Settings.Accounts.Count);
            var dats = new HashSet<Settings.IFile>();
            var sort = Settings.GuildWars2.DatUpdaterEnabled.Value;
            var minimum = int.MaxValue;
            var index = 0;

            //only accounts with a unique dat file need to be updated
            //only applies to GW2

            foreach (var button in buttons)
            {
                var account = button.AccountData;
                if (account != null)
                {
                    Settings.IFile dat;
                    if (account.Type == Settings.AccountType.GuildWars1)
                    {
                        dat = ((Settings.IGw1Account)account).DatFile;
                        continue;
                    }
                    else
                        dat = ((Settings.IGw2Account)account).DatFile;

                    if (dat == null || dats.Add(dat))
                    {
                        accounts.Add(account);

                        if (sort && minimum > 0 && account.Type == Settings.AccountType.GuildWars2)
                        {
                            try
                            {
                                if (dat != null)
                                {
                                    var l = new FileInfo(dat.Path).Length;
                                    if (l < minimum)
                                    {
                                        minimum = (int)l;
                                        index = accounts.Count - 1;
                                    }
                                }
                                else
                                {
                                    minimum = 0;
                                    index = accounts.Count - 1;
                                }
                            }
                            catch { }
                        }
                    }
                }
            }

            if (sort && index != 0)
            {
                var a = accounts[index];
                accounts[index] = accounts[0];
                accounts[0] = a;
            }

            if (accounts != null && accounts.Count > 0)
            {
                using (BeforeShowDialog())
                {
                    ShowUpdateAccounts(null, accounts,
                        delegate
                        {
                            if (Settings.CheckForNewBuilds.Value || Tools.AutoUpdate.IsEnabled())
                                UpdateLastKnownBuild();
                        });
                }
            }
        }

        /// <summary>
        /// Shows the update dialog and queues the specified accounts
        /// </summary>
        /// <param name="parent">Parent for message boxes, or null</param>
        /// <param name="accounts">Accounts to update</param>
        /// <param name="onSuccess">On completion of the update</param>
        private void ShowUpdateAccounts(Form parent, IList<Settings.IAccount> accounts, Action onSuccess)
        {
            Func<Settings.AccountType, bool> hasActive = delegate(Settings.AccountType type)
            {
                foreach (var account in accounts)
                {
                    if (account.Type == type && Client.Launcher.GetState(account) != Client.Launcher.AccountState.None)
                        return true;
                }
                return false;
            };

            while (Client.Launcher.GetActiveProcessCount(Client.Launcher.AccountType.GuildWars2) > 0 || hasActive(Settings.AccountType.GuildWars2))
            {
                bool bparent = parent == null;
                if (bparent)
                    parent = AddMessageBox(this);
                try
                {
                    if (MessageBox.Show(parent, "All active Guild Wars 2 processes will be closed. Are you sure?", "Close GW2 to continue", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) != DialogResult.Retry)
                        return;

                    ShowWaiting(async delegate(formWaiting f, Action onComplete)
                    {
                        var t = Task.Factory.StartNew(new Action(
                            delegate
                            {
                                int retries = 0;

                                while (Client.Launcher.GetActiveProcessCount(Client.Launcher.AccountType.GuildWars2) > 0 || hasActive(Settings.AccountType.GuildWars2))
                                {
                                    if (retries++ >= 10)
                                        break;

                                    try
                                    {
                                        Client.Launcher.CancelAndKillActiveLaunches(Client.Launcher.AccountType.GuildWars2);
                                        System.Threading.Thread.Sleep(1000);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                    }
                                }
                            }));

                        try
                        {
                            await t;
                        }
                        catch { }

                        onComplete();
                    });
                }
                finally
                {
                    if (bparent)
                    {
                        parent.Dispose();
                        parent = null;
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
                        if (onSuccess != null)
                            onSuccess();
                    }
                    updatingWindow = null;
                }
            }
            else
            {
                launch();
            }
        }

        private void updateSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateAccounts(GetSelected());
        }

        private IList<Controls.AccountGridButton> GetVisible()
        {
            return gridContainer.GetVisible();
        }

        private IList<Controls.AccountGridButton> GetSelected()
        {
            var selected = gridContainer.GetSelected();
            if (selected.Count == 0)
            {
                var button = contextMenu.Tag as Controls.AccountGridButton;
                if (button != null)
                    return new Controls.AccountGridButton[] { button };
                return new Controls.AccountGridButton[0];
            }
            else
                return selected;
        }

        private IEnumerable<Settings.IAccount> GetSelectedAccounts()
        {
            foreach (var b in GetSelected())
            {
                if (b.AccountData != null)
                    yield return b.AccountData;
            }
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
            foreach (var button in GetVisible())
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
            foreach (var button in GetVisible())
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

        private void deleteCacheFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formGw2Cache()))
                {
                    f.ShowDialog(this);
                }
            }
        }

        private void ShowKillAllActiveProcesses(Settings.AccountType type)
        {
            var s = Settings.GetSettings(type);

            if (string.IsNullOrEmpty(s.Path.Value))
                return;

            using (BeforeShowDialog())
            {
                using (var f = AddMessageBox(this))
                {
                    var path = s.Path.Value;

                    if (MessageBox.Show(f, "Attempt to kill all processes with the following path?\n\n\"" + path + "\"", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        Task.Run(new Action(
                            delegate
                            {
                                switch (type)
                                {
                                    case Settings.AccountType.GuildWars1:
                                        Client.Launcher.KillAllActiveProcesses(Client.Launcher.AccountType.GuildWars1);
                                        break;
                                    case Settings.AccountType.GuildWars2:
                                        Client.Launcher.KillAllActiveProcesses(Client.Launcher.AccountType.GuildWars2);
                                        break;
                                }
                            }));
                    }
                }
            }
        }

        private void ShowKillMutex(Settings.AccountType type)
        {
            using (BeforeShowDialog())
            {
                using (var parent = AddMessageBox(this))
                {
                    if (MessageBox.Show(parent, "All processes will be scanned for the mutex that prevents multiple clients from launching. This should only be used when launching clients outside of this program.\n\nThis may take a minute. Are you sure?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        ShowWaiting(async delegate(formWaiting f, Action onComplete)
                        {
                            var t = Task.Factory.StartNew(new Action(
                                delegate
                                {
                                    Util.ProcessUtil.KillMutexWindow(type);
                                }));

                            try
                            {
                                await t;
                            }
                            catch { }

                            onComplete();
                        });
                    }
                }
            }
        }

        private void killAllActiveProcessesGw1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowKillAllActiveProcesses(Settings.AccountType.GuildWars1);
        }

        private void killMutexGw1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowKillMutex(Settings.AccountType.GuildWars1);
        }

        private void killAllActiveProcessesGw2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowKillAllActiveProcesses(Settings.AccountType.GuildWars2);
        }

        private void killMutexGw2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowKillMutex(Settings.AccountType.GuildWars2);
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

        private void backgroundPatchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowBackgroundPatcher();
        }

        private void createShortcutAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateShortcut(GetVisible());
        }

        private void createShortcutSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateShortcut(GetSelected());
        }
        
        private void CreateShortcut(IList<AccountGridButton> selected)
        {
            var accounts = new List<Settings.IAccount>();
            var single = false;

            foreach (var button in selected)
            {
                var account = button.AccountData;
                if (account != null)
                    accounts.Add(account);
            }

            if (accounts.Count > 1)
            {
                using (BeforeShowDialog())
                {
                    using (var f = (formShortcutType)AddWindow(new formShortcutType()))
                    {
                        if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            single = f.CreateSingle;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }

            CreateShortcut(accounts, single);
        }

        private void CreateShortcut(IList<Settings.IAccount> accounts, bool single)
        {
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
                var icons = new Dictionary<Settings.AccountType, string>(2);

                if (Settings.UseDefaultIconForShortcuts.Value)
                {
                    icons[Settings.AccountType.GuildWars1] = Settings.GuildWars1.Path.Value;
                    icons[Settings.AccountType.GuildWars2] = Settings.GuildWars2.Path.Value;
                    foreach (var k in icons.Keys)
                    {
                        var icon = icons[k];
                        if (icon != null && !File.Exists(icon))
                            icons[k] = null;
                    }
                }

                if (single)
                {
                    var name = accounts[0].Name + " and " + (accounts.Count - 1) + " more";
                    var output = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), getName(name) + ".lnk");
                    if (!System.IO.File.Exists(output))
                    {
                        string icon;
                        if (!icons.TryGetValue(accounts[0].Type, out icon))
                            icon = null;
                        var uids = new StringBuilder(accounts.Count * 3);
                        foreach (var a in accounts)
                        {
                            uids.Append(a.UID);
                            uids.Append(',');
                        }
                        uids.Length--;
                        Windows.Shortcut.Create(output, path, "-l:silent -l:uid:" + uids.ToString(), "Launch " + name, icon);
                    }
                }
                else
                {
                    foreach (var account in accounts)
                    {
                        var output = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), getName(account.Name) + ".lnk");
                        if (!System.IO.File.Exists(output))
                        {
                            string icon;
                            if (!icons.TryGetValue(account.Type, out icon))
                                icon = null;
                            Windows.Shortcut.Create(output, path, "-l:silent -l:uid:" + account.UID, "Launch " + account.Name, icon);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                using (BeforeShowDialog())
                {
                    using (var parent = AddMessageBox(this))
                    {
                        MessageBox.Show(this, "Unable to create shortcut\n\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            Windows.WindowShadow.Enable(this.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            #region WM_GW2LAUNCHER

            if (m.Msg == Messaging.Messager.WM_GW2LAUNCHER)
            {
                switch ((Messaging.Messager.MessageType)m.WParam.GetValue())
                {
                    case Messaging.Messager.MessageType.Show:

                        m.Result = this.Handle;
                        ShowToFront();

                        break;
                    case Messaging.Messager.MessageType.Launch:

                        try
                        {
                            var v = Settings.Accounts[(ushort)m.LParam.GetValue()];

                            if (v.HasValue)
                            {
                                var a = v.Value;

                                if (Client.Launcher.IsActive(a))
                                {
                                    var p = Client.Launcher.GetProcess(a);
                                    if (p != null)
                                        FocusWindowAsync(p);
                                }
                                else
                                {
                                    this.BeginInvoke(new MethodInvoker(
                                        delegate
                                        {
                                            Client.Launcher.LaunchMode mode;
                                            if (Settings.ActionInactiveLClick.Value == Settings.ButtonAction.LaunchSingle)
                                                mode = Client.Launcher.LaunchMode.LaunchSingle;
                                            else
                                                mode = Client.Launcher.LaunchMode.Launch;
                                            Client.Launcher.Launch(a, mode);
                                        }));
                                }
                            }
                        }
                        catch { }

                        break;
                    case Messaging.Messager.MessageType.LaunchMap:

                        try
                        {
                            var launch = Messaging.LaunchMessage.FromMap(m.LParam.GetValue32());

                            this.BeginInvoke(new MethodInvoker(
                                delegate
                                {
                                    Client.Launcher.LaunchMode mode;
                                    if (Settings.ActionInactiveLClick.Value == Settings.ButtonAction.LaunchSingle)
                                        mode = Client.Launcher.LaunchMode.LaunchSingle;
                                    else
                                        mode = Client.Launcher.LaunchMode.Launch;
                                    foreach (var uid in launch.accounts)
                                    {
                                        var v = Settings.Accounts[uid];
                                        if (v.HasValue)
                                            Client.Launcher.Launch(v.Value, mode, launch.args);
                                    }
                                }));
                        }
                        catch { }

                        break;
                    case Messaging.Messager.MessageType.UpdateMap:

                        try
                        {
                            var update = Messaging.UpdateMessage.FromMap(m.LParam.GetValue32());

                            this.BeginInvoke(new MethodInvoker(
                                delegate
                                {
                                    Client.Launcher.Update(update.files);
                                }));
                        }
                        catch { }

                        break;
                    case Messaging.Messager.MessageType.QuickLaunch:

                        try
                        {
                            var type = m.LParam.GetValue32();

                            this.BeginInvoke(new MethodInvoker(
                                delegate
                                {
                                    ShowQuickLaunch(type);
                                }));
                        }
                        catch { }

                        break;
                    case Messaging.Messager.MessageType.TotpCode:

                        //var uid = m.LParam.GetValue32(); //not used
                        Task.Run(
                            delegate
                            {
                                try
                                {
                                    PrintTotpCodeToForegroundWindow();
                                }
                                catch { }
                            });

                        break;
                }

                base.WndProc(ref m);

                return;
            }

            #endregion

            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_MOVE:

                    if (this.WindowState == FormWindowState.Minimized)
                    {
                        OnMinimized();
                    }

                    break;
                case WindowMessages.WM_MOUSEACTIVATE:

                    if (resizingEvents != null)
                        resizingEvents.Enabled = true;

                    break;
                case WindowMessages.WM_SIZING:

                    var r = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                    var w = r.right - r.left;
                    var h = r.bottom - r.top;

                    if (w > this.ClientSize.Width || h > this.ClientSize.Height)
                    {
                        if (buffer != null)
                            buffer.Dispose();
                        buffer = BufferedGraphicsManager.Current.Allocate(this.CreateGraphics(), new Rectangle(0, 0, w, h));
                        PaintBackground(buffer.Graphics, w, h);
                        buffer.Render();
                    }

                    break;
                //case WindowMessages.WM_NCCALCSIZE:

                //    var r = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                //    var padding = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
                //    var borderW = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXSIZEFRAME) + padding;
                //    var borderH = NativeMethods.GetSystemMetrics(SystemMetric.SM_CYSIZEFRAME) + padding;

                //    r.left += borderW;
                //    r.right -= borderW;
                //    r.bottom -= borderH;

                //    Marshal.StructureToPtr(r, m.LParam, false);

                //    break;
                case WindowMessages.WM_NCLBUTTONDBLCLK:

                    return;
                case WindowMessages.WM_NCHITTEST:
                    
                    base.WndProc(ref m);

                    if (m.Result == (IntPtr)HitTest.Client)
                    {
                        const int MOVE_SIZE = 10;

                        var p = this.PointToClient(new Point(m.LParam.GetValue32()));
                        var size = this.ClientSize;

                        if (p.Y < panelTop.Bottom)
                        {
                            if (p.Y < MOVE_SIZE)
                            {
                                if (p.X < MOVE_SIZE)
                                {
                                    m.Result = (IntPtr)HitTest.TopLeft;
                                }
                                else if (p.X > size.Width - MOVE_SIZE)
                                {
                                    m.Result = (IntPtr)HitTest.TopRight;
                                }
                                else
                                {
                                    m.Result = (IntPtr)HitTest.Top;
                                }
                            }
                            else if (p.X < MOVE_SIZE)
                            {
                                m.Result = (IntPtr)HitTest.Left;
                            }
                            else if (p.X > size.Width - MOVE_SIZE)
                            {
                                m.Result = (IntPtr)HitTest.Right;
                            }
                            else
                            {
                                if (mpWindow == null || !mpWindow.Visible)
                                    m.Result = (IntPtr)HitTest.Caption;
                                else
                                    m.Result = (IntPtr)HitTest.Client;
                            }
                        }
                        else if (p.Y > size.Height - sizeDragButton1.Height && p.X > size.Width - sizeDragButton1.Height)
                        {
                            m.Result = (IntPtr)HitTest.BottomRight;
                        }
                        else if (p.Y > size.Height - MOVE_SIZE)
                        {
                            if (p.X < MOVE_SIZE)
                            {
                                m.Result = (IntPtr)HitTest.BottomLeft;
                            }
                            else if (p.X > size.Width - MOVE_SIZE)
                            {
                                m.Result = (IntPtr)HitTest.BottomRight;
                            }
                            else
                            {
                                m.Result = (IntPtr)HitTest.Bottom;
                            }
                        }
                        else if (p.X < MOVE_SIZE)
                        {
                            m.Result = (IntPtr)HitTest.Left;
                        }
                        else if (p.X > size.Width - MOVE_SIZE)
                        {
                            m.Result = (IntPtr)HitTest.Right;
                        }
                    }

                    return;
            }

            base.WndProc(ref m);
        }

        private void PrintTotpCodeToForegroundWindow()
        {
            uint pid;
            var h = NativeMethods.GetForegroundWindow();
            NativeMethods.GetWindowThreadProcessId(h, out pid);

            foreach (var a in Client.Launcher.GetActiveProcesses())
            {
                var p = Client.Launcher.GetProcess(a);
                if (p != null)
                {
                    if (p.Id == pid)
                    {
                        if (a.TotpKey != null)
                        {
                            try
                            {
                                foreach (var c in Tools.Totp.Generate(a.TotpKey))
                                {
                                    if (!NativeMethods.PostMessage(h, 0x0102, (IntPtr)c, IntPtr.Zero))
                                        break;
                                }
                            }
                            catch { }
                        }

                        return;
                    }
                }
            }

            //unknown

            ShowTotpCodeWindow();
        }

        private void ShowTotpCodeWindow()
        {
            var h = NativeMethods.GetForegroundWindow();
            var vars = new List<Client.Variables.Variable>();

            foreach (var b in gridContainer.GetButtons())
            {
                var a = b.AccountData;

                if (a != null && a.TotpKey != null)
                {
                    var totp = a.TotpKey;
                    vars.Add(new Client.Variables.Variable(Client.Variables.VariableType.Account, a.Name, "",
                        delegate
                        {
                            return totp;
                        }));
                }
            }

            if (vars.Count > 0)
            {
                var t = new System.Threading.Thread(new System.Threading.ThreadStart(
                    delegate
                    {
                        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                        using (var f = new formVariables(vars))
                        {
                            f.TopMost = true;

                            f.VariableSelected += delegate(object o, Client.Variables.Variable v)
                            {
                                try
                                {
                                    var totp = Tools.Totp.Generate((byte[])v.GetValue(null));

                                    //note there's no restriction on where the code will be pasted, thus the clipboard will be used

                                    Clipboard.SetText(new string(totp));

                                    if (NativeMethods.SetForegroundWindow(h))
                                    {
                                        SendKeys.SendWait("^V");
                                    }
                                    else
                                    {
                                        //not all windows will support this

                                        foreach (var c in totp)
                                        {
                                            if (!NativeMethods.PostMessage(h, WindowMessages.WM_CHAR, (IntPtr)c, IntPtr.Zero))
                                                break;
                                        }
                                    }
                                }
                                catch { }

                                f.Close();
                            };

                            f.Shown += delegate
                            {
                                var b = false;
                                try
                                {
                                    NativeMethods.BringWindowToTop(f.Handle);
                                    b = NativeMethods.SetForegroundWindow(f.Handle);
                                }
                                catch { }
                                if (!b)
                                    f.CloseOnMouseLeave();
                            };

                            f.ShowAtCursor();

                            try
                            {
                                Application.Run(f);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Crash(e);
                            }
                        }
                    }));

                t.IsBackground = true;
                t.SetApartmentState(System.Threading.ApartmentState.STA);
                t.Start();
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
            await ApplyWindowedBounds(GetVisible());
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
                        foreach (var button in GetVisible())
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

        private async void authenticateOnNextLaunchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reauthenticateOnNextLaunchToolStripMenuItem.Enabled = false;

            foreach (var a in Util.Accounts.GetAccounts())
            {
                if (a.NetworkAuthorizationState == Settings.NetworkAuthorizationState.OK)
                    a.NetworkAuthorizationState = Settings.NetworkAuthorizationState.Unknown;
            }

            await Client.Launcher.ClearNetworkAuthorization(false, false);

            reauthenticateOnNextLaunchToolStripMenuItem.Enabled = true;
        }

        private void ShowNotesDialog(AccountGridButton button)
        {
            var account = button.AccountData;
            if (account == null)
                return;

            using (BeforeShowDialog())
            {
                using (var f = (formNotes)AddWindow(new formNotes(account)))
                {
                    f.NoteChanged += delegate(object o, Settings.Notes.Note n)
                    {
                        var notes = account.Notes;
                        if (notes != null)
                        {
                            bool used;

                            if (used = n.Expires > button.LastNoteUtc)
                                button.LastNoteUtc = n.Expires;
                            else
                                used = n.Notify && Settings.NotesNotifications.HasValue;

                            if (used && n.Expires < Util.ScheduledEvents.GetDate(OnNoteCallback))
                            {
                                var ms = n.Expires.Subtract(DateTime.UtcNow).TotalMilliseconds + 1;

                                int _ms;
                                if (ms > int.MaxValue)
                                    _ms = int.MaxValue;
                                else if (ms < 0)
                                    _ms = 0;
                                else
                                    _ms = (int)ms;

                                Util.ScheduledEvents.Register(OnNoteCallback, _ms);
                            }
                        }
                    };

                    var bounds = Settings.WindowBounds[f.GetType()];
                    if (bounds.HasValue)
                    {
                        f.StartPosition = FormStartPosition.Manual;
                        f.Bounds = Util.ScreenUtil.Constrain(bounds.Value); 
                    }

                    f.ShowDialog(this);

                    if (f.Modified)
                    {
                        if (account.Notes != null)
                            button.LastNoteUtc = account.Notes.ExpiresLast;
                        else
                            button.LastNoteUtc = DateTime.MinValue;
                    }
                }
            }
        }

        private void notesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var button = contextMenu.Tag as AccountGridButton;
            if (button != null)
                ShowNotesDialog(button);
        }

        private async void newMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var button = contextMenu.Tag as AccountGridButton;
            if (button != null && button.AccountData != null)
            {
                var account = button.AccountData;

                using (BeforeShowDialog())
                {
                    using (var f = (formNote)AddWindow(new formNote()))
                    {
                        if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            ushort sid;
                            try
                            {
                                var sids = await Tools.Notes.AddRange(new string[] { f.Message });
                                sid = sids[0];
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                                MessageBox.Show(this, "An error is preventing the note from being created:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            var n = new Settings.Notes.Note(f.Expires, sid, f.NotifyOnExpiry);

                            var notes = account.Notes;
                            if (notes == null)
                                account.Notes = notes = new Settings.Notes();

                            notes.Add(n);

                            bool used;

                            if (used = n.Expires > button.LastNoteUtc)
                                button.LastNoteUtc = n.Expires;
                            else
                                used = n.Notify && Settings.NotesNotifications.HasValue;

                            if (used && n.Expires < Util.ScheduledEvents.GetDate(OnNoteCallback))
                            {
                                var ms = n.Expires.Subtract(DateTime.UtcNow).TotalMilliseconds + 1;

                                int _ms;
                                if (ms > int.MaxValue)
                                    _ms = int.MaxValue;
                                else if (ms < 0)
                                    _ms = 0;
                                else
                                    _ms = (int)ms;

                                Util.ScheduledEvents.Register(OnNoteCallback, _ms);
                            }
                        }
                    }
                }
            }
        }

        private void panelContainer_MouseClick(object sender, MouseEventArgs e)
        {
            gridContainer.ClearSelected();
        }

        private void showAccountBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAccountBar(true);
        }

        private void sortCustomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnSortingModeClick(sender);
        }

        private void buttonKillAll_Click(object sender, EventArgs e)
        {
            Client.Launcher.CancelAndKillActiveLaunches(Client.Launcher.AccountType.Any);
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accounts = new List<Settings.IAccount>(GetSelectedAccounts());
            if (accounts.Count > 0)
            {
                using (BeforeShowDialog())
                {
                    using (var f = (formClone)AddWindow(new formClone(accounts)))
                    {
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            if (mpWindow.Page == mpWindow.Pages && mpWindow.Pages < MAX_PAGES)
                                ++mpWindow.Pages;

                            if (f.Accounts.Count > 0)
                            {
                                foreach (var a in f.Accounts)
                                {
                                    if (mpWindow.Page > 0)
                                    {
                                        a.Pages = new Settings.PageData[] { new Settings.PageData(mpWindow.Page, ushort.MaxValue) };
                                    }

                                    AddAccount(a, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void cloneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            cloneToolStripMenuItem_Click(sender, e);
        }

        private void hiddenUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BeforeShowDialog())
            {
                using (var f = AddWindow(new formManagedInactiveUsers()))
                {
                    f.ShowDialog(this);
                }
            }
        }

        private void PaintBackground(Graphics g, int w, int h)
        {
            g.Clear(this.BackColor);

            using (var brush = new SolidBrush(panelBottom.BackColor))
            {
                g.FillRectangle(brush, panelTop.Left, panelTop.Top, w - panelTop.Left * 2, panelTop.Height);
            }

            var psize = (int)(GetScaling() + 0.5f);
            var phalf = psize / 2;

            using (var p = new Pen(Color.FromArgb(200, 200, 200), psize))
            {
                int y;

                y = panelTop.Bottom + phalf - psize;
                g.DrawLine(p, panelTop.Left, y, w - panelTop.Left, y);

                y = panelBottom.Top + phalf - psize;
                g.DrawLine(p, panelBottom.Left, y, w - panelBottom.Left, y);

                p.Color = Color.FromArgb(120, 120, 120);
                g.DrawRectangle(p, phalf, phalf, w - psize, h - psize);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                PaintBackground(buffer.Graphics, this.ClientSize.Width, this.ClientSize.Height);
            }

            buffer.Render(e.Graphics);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            redraw = true;
            this.Invalidate();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void buttonMinimize_Click(object sender, EventArgs e)
        {
            if (await FadeTo(0, FADE_DURATION))
            {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void sortCustomToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            groupByToolStripMenuItem.Visible = !sortCustomToolStripMenuItem.Checked;
            listToolStripMenuItem.Visible = sortCustomToolStripMenuItem.Checked;
            gridToolStripMenuItem.Visible = sortCustomToolStripMenuItem.Checked;
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!((ToolStripMenuItem)sender).Checked)
                OnSortingModeClick(sender);
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!((ToolStripMenuItem)sender).Checked)
                OnSortingModeClick(sender);
        }

        private bool _ShowFilter;
        private bool ShowFilter
        {
            get
            {
                return _ShowFilter;
            }
            set
            {
                if (_ShowFilter != value)
                {
                    _ShowFilter = value;

                    var cs = this.ClientSize;
                    int h, ch;

                    if (value)
                    {
                        h = textFilter.Height + textFilter.Margin.Vertical;
                    }
                    else
                    {
                        h = sizeDragButton1.Height;
                    }

                    ch = h - panelBottom.Height;

                    this.SuspendLayout();
                    panelBottom.SuspendLayout();

                    textFilter.Visible = value;
                    buttonFilterClose.Visible = value;
                    sizeDragButton1.Visible = !value;

                    panelBottom.SetBounds(panelBottom.Left, panelBottom.Bottom - h, panelBottom.Width, h);
                    panelContainer.Height -= ch;

                    panelBottom.ResumeLayout();
                    this.ResumeLayout();

                    redraw = true;
                    this.Invalidate(Rectangle.FromLTRB(0, panelContainer.Bottom, cs.Width, this.Height), false);

                    if (value)
                    {
                        if (textFilter.TextLength > 0)
                            gridContainer.Filter = textFilter.Text;

                        textFilter.Focus();
                        //KeyPreview = false;
                    }
                    else
                    {
                        gridContainer.Filter = null;
                        if (autoSizeGrid && shown)
                            this.Size = ResizeAuto(this.Location);
                        //KeyPreview = true;
                    }
                }
            }
        }

        private void buttonFilterClose_Click(object sender, EventArgs e)
        {
            ShowFilter = false;
        }

        private bool pendingFilter;
        private async void OnFilterChanged()
        {
            if (pendingFilter)
                return;
            pendingFilter = true;

            try
            {
                await Task.Delay(500);
            }
            catch
            {
                return;
            }
            finally
            {
                pendingFilter = false;
            }

            if (!textFilter.Visible)
                return;

            gridContainer.Filter = textFilter.Text;
        }

        private void textFilter_TextChanged(object sender, EventArgs e)
        {
            OnFilterChanged();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:

                        if (!_ShowFilter || !textFilter.ContainsFocus)
                        {
                            gridContainer.SelectAll();
                            e.Handled = true;
                        }

                        break;
                    case Keys.F:

                        if (!_ShowFilter)
                        {
                            ShowFilter = true;
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                        }
                        else if (!textFilter.ContainsFocus)
                        {
                            textFilter.Focus();
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                        }

                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:

                        if (_ShowFilter)
                        {
                            ShowFilter = false;
                        }
                        else
                        {
                            gridContainer.ClearSelected();
                        }

                        e.Handled = true;
                        e.SuppressKeyPress = true;

                        break;
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar))
            {
                if (_ShowFilter)
                {
                    textFilter.SelectedText = e.KeyChar.ToString();
                    textFilter.Focus();
                }
                else
                {
                    textFilter.Text = e.KeyChar.ToString();
                    textFilter.Select(textFilter.TextLength, 0);

                    ShowFilter = true;
                }
                e.Handled = true;
            }

            base.OnKeyPress(e);
        }

        private void groupByActiveMenuItem_Click(object sender, EventArgs e)
        {
            OnGroupingModeClick(sender);
        }

        private void groupByTypeMenuItem_Click(object sender, EventArgs e)
        {
            OnGroupingModeClick(sender);
        }

        private void groupByAscendingMenuItem_Click(object sender, EventArgs e)
        {
            if (!((ToolStripMenuItem)sender).Checked)
                SetGrouping(Settings.Sorting.Value.Grouping.Mode, false, true);
        }

        private void groupByDescendingMenuItem_Click(object sender, EventArgs e)
        {
            if (!((ToolStripMenuItem)sender).Checked)
                SetGrouping(Settings.Sorting.Value.Grouping.Mode, true, true);
        }

        private void FadeTo(double to)
        {
            if (Fading != null)
                Fading(this, EventArgs.Empty);

            this.Update();
            this.Opacity = to;
        }

        private async Task<bool> FadeTo(double to, int duration)
        {
            if (Fading != null)
                Fading(this, EventArgs.Empty);

            var from = this.Opacity;
            if (from == to)
                return true;

            var _duration = (to < from ? from - to : to - from) * duration;
            var abort = false;
            var first = true;
            var start = Environment.TickCount;

            EventHandler e = delegate
            {
                abort = true;
            };

            this.Disposed += e;
            this.Fading += e;
            this.VisibleChanged += e;

            do
            {
                await Task.Delay(10);

                if (abort)
                    break;

                if (first)
                {
                    this.Update();
                    first = false;
                }

                var p = (Environment.TickCount - start) / _duration;

                if (p >= 1)
                {
                    this.Opacity = to;

                    break;
                }
                else
                {
                    this.Opacity = from + (to - from) * p;
                }
            }
            while (true);

            this.Disposed -= e;
            this.Fading -= e;
            this.VisibleChanged -= e;

            return !abort;
        }

        private void buttonMenu_MouseDown(object sender, MouseEventArgs e)
        {
            if (!mpWindow.Visible)
                mpWindow.Show(this, buttonMenu);
        }

        private void buttonMenu_Click(object sender, EventArgs e)
        {
        }

        private void UpdatePageCount()
        {
            var pages = 0;

            foreach (var a in Util.Accounts.GetAccounts())
            {
                if (a.Pages != null)
                {
                    var pg = a.Pages[a.Pages.Length - 1].Page;
                    if (pg > pages)
                        pages = pg;
                }
            }

            if (pages < MAX_PAGES)
                ++pages;
            mpWindow.Pages = (byte)pages;
        }
        
        private void showOnPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selected = GetSelected();
            if (selected.Count == 0)
                return;

            using (BeforeShowDialog())
            {
                using (var f = (formMoveToPage)AddWindow(new formMoveToPage(mpWindow.Page, mpWindow.Pages)))
                {
                    if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        var page = f.Page;
                        if (page == mpWindow.Page || page == 0)
                        {
                            if (!f.RemoveCurrent)
                                return;
                            page = 0;
                        }

                        foreach (var i in selected)
                        {
                            var paging = i.Paging;

                            if (paging == null)
                            {
                                if (page != 0)
                                {
                                    paging = new AccountGridButton.PagingData(new Settings.PageData[] { new Settings.PageData(page, 0) });
                                }
                            }
                            else
                            {
                                int existing = -1,
                                    remove = -1;
                                var sort = false;
                                var pages = paging.Pages;

                                for (var j = 0; j < pages.Length; j++)
                                {
                                    if (pages[j].Page == page)
                                    {
                                        existing = j;
                                        if (!f.RemoveCurrent || remove != -1)
                                            break;
                                    }
                                    else if (pages[j].Page == mpWindow.Page && f.RemoveCurrent)
                                    {
                                        remove = j;
                                        if (existing != -1)
                                            break;
                                    }
                                }

                                if (page != 0 && existing == -1)
                                {
                                    if (remove != -1)
                                    {
                                        pages = new Settings.PageData[pages.Length];
                                        Array.Copy(paging.Pages, pages, pages.Length);
                                        pages[remove] = new Settings.PageData(page, 0);

                                        remove = -1;
                                    }
                                    else
                                    {
                                        pages = new Settings.PageData[pages.Length + 1];
                                        Array.Copy(paging.Pages, pages, pages.Length - 1);
                                        pages[pages.Length - 1] = new Settings.PageData(page, ushort.MaxValue);
                                    }

                                    sort = true;
                                }

                                if (remove != -1)
                                {
                                    if (pages.Length == 1)
                                    {
                                        pages = null;
                                        paging = null;
                                    }
                                    else
                                    {
                                        pages = new Settings.PageData[pages.Length - 1];
                                        Array.Copy(paging.Pages, pages, pages.Length);
                                        if (remove < pages.Length)
                                        {
                                            pages[remove] = paging.Pages[pages.Length];
                                            sort = true;
                                        }
                                    }
                                }

                                if (sort)
                                {
                                    Array.Sort<Settings.PageData>(pages,
                                        delegate(Settings.PageData a, Settings.PageData b)
                                        {
                                            return a.Page.CompareTo(b.Page);
                                        });
                                }

                                if (paging != null)
                                    paging.Pages = pages;
                            }

                            i.Paging = paging;

                            if (i.AccountData != null)
                            {
                                i.AccountData.Pages = paging != null ? paging.Pages : null;
                            }
                        }

                        if (page < mpWindow.Page)
                        {
                            if (mpWindow.Page == mpWindow.Pages - 1 || mpWindow.Page == MAX_PAGES)
                            {
                                UpdatePageCount();
                            }
                        }
                        else
                        {
                            var pages = page;
                            if (pages < MAX_PAGES)
                                ++pages;
                            if (pages >= mpWindow.Pages)
                                mpWindow.Pages = (byte)pages;
                        }

                        if (mpWindow.Page != 0)
                            gridContainer.RefreshFilter();
                    }
                }
            }
        }

        private void editmultipleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var button = contextMenu.Tag as AccountGridButton;
            var selected = selectedToolStripMenuItem.Tag as IList<AccountGridButton>;

            if (button != null && button.AccountData != null)
            {
                OnEditAccount(button, selected);
            }
        }
    }
}
