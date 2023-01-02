using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Gw2Launcher.UI.Controls;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    class formAccountBar : Base.BaseForm
    {
        private const byte SNAP_SIZE = 15;
        private const byte SNAP_SIZE_RELEASE = 30;

        private class IconValue : IDisposable
        {
            public IconValue(Image image, bool disposable)
            {
                this.Image = image;
                this.disposable = disposable;
            }

            public Image Image
            {
                get;
                private set;
            }

            private bool disposable;

            public void Dispose()
            {
                if (disposable)
                {
                    using (Image) { }
                }
            }
        }

        private class AccountButton
        {
            public AccountBarButton button;
            public short index;
            public Settings.IAccount account;
            public bool visible;
            public DateTime activated;
            
            public AccountButton(Settings.IAccount account)
            {
                this.account = account;
                this.index = -1;
            }

            private IconValue icon;
            public IconValue Icon
            {
                get
                {
                    return icon;
                }
                set
                {
                    if (icon != value)
                    {
                        using (icon) { }
                        icon = value;
                        if (value != null && button != null)
                        {
                            button.BackgroundImage = value.Image;
                        }
                    }
                }
            }

            private bool isActive;
            public bool IsActive
            {
                get
                {
                    return isActive;
                }
                set
                {
                    if (isActive != value)
                    {
                        isActive = value;

                        if (value)
                            activated = DateTime.UtcNow;

                        if (button != null)
                            SetIsActiveProperties();
                    }
                }
            }

            private bool isInUse;
            public bool IsInUse
            {
                get
                {
                    return isInUse;
                }
                set
                {
                    if (isInUse != value)
                    {
                        isInUse = value;

                        if (button != null)
                            SetIsActiveProperties();
                    }
                }
            }

            public AccountBarButton CreateButton(Font font)
            {
                button = new AccountBarButton()
                {
                    Account = account,
                    BackColor = Util.Color.Lighten(Color.Black, 0.025f), //0.05f
                    BackColorHovered = Util.Color.Lighten(Color.Black, 0.1f),
                    BackColorSelected = Util.Color.Lighten(Color.Black, 0.15f),
                    Text = account.Name,
                    Font = font,
                    Visible = false,
                    IconVisible = true,
                    TextVisible = true,
                };

                SetIsActiveProperties();

                return button;
            }

            private void SetIsActiveProperties()
            {
                if (isActive)
                {
                    button.ForeColor = Util.Color.Darken(Color.White, 0.20f);
                    button.ForeColorHovered = Color.White;
                    button.IconOpacity = 255;
                    button.BackColor = Util.Color.Lighten(Color.Black, 0.075f);
                }
                else if (isInUse)
                {
                    button.ForeColor = Util.Color.Lighten(Color.DimGray, 0.1f);
                    button.ForeColorHovered = Util.Color.Lighten(Color.DimGray, 0.25f);
                    button.IconOpacity = 192;
                    button.BackColor = Util.Color.Lighten(Color.Black, 0.05f);
                }
                else
                {
                    button.ForeColor = Util.Color.Darken(Color.DimGray, 0.20f);
                    button.ForeColorHovered = Util.Color.Lighten(Color.DimGray, 0.25f);
                    button.IconOpacity = 127;
                    button.BackColor = Util.Color.Lighten(Color.Black, 0.025f);
                }
            }

            public void SetLayout(Orientation orientation, Settings.ScreenAnchor docked)
            {
                if (orientation == Orientation.Horizontal)
                {
                    if (docked == Settings.ScreenAnchor.Top)
                        button.ColorKeyAlignment = AccountBarButton.EdgeAlignment.Top;
                    else
                        button.ColorKeyAlignment = AccountBarButton.EdgeAlignment.Bottom;

                    button.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                }
                else
                {
                    if (docked == Settings.ScreenAnchor.Right)
                        button.ColorKeyAlignment = AccountBarButton.EdgeAlignment.Right;
                    else
                        button.ColorKeyAlignment = AccountBarButton.EdgeAlignment.Left;

                    button.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
                }
            }
        }

        private class Snap
        {
            public int left, top, right, bottom;

            public Snap(Rectangle r)
            {
                left = r.Left;
                top = r.Top;
                right = r.Right;
                bottom = r.Bottom;
            }
        }

        private class BarButton : TransparentFlatButton
        {
            public BarButton(bool transparent)
            {
                this.Transparent = transparent;
            }
        }

        private class AccountButtonComparer : IComparer<AccountButton>
        {
            private Settings.SortMode sorting;
            private Settings.GroupMode grouping;
            private bool sortingReversed;
            private bool groupingReversed;
            private Util.AlphanumericStringComparer stringComparer;

            public AccountButtonComparer(Settings.SortingOptions options)
            {
                sorting = options.Sorting.Mode;
                sortingReversed = options.Sorting.Descending;
                grouping = options.Grouping.Mode;
                groupingReversed = options.Grouping.Descending;

                switch (sorting)
                {
                    case Settings.SortMode.Account:
                    case Settings.SortMode.Name:

                        this.stringComparer = new Util.AlphanumericStringComparer();

                        break;
                }
            }

            public int Compare(AccountButton a, AccountButton b)
            {
                int result;

                if (a.account.Pinned != b.account.Pinned)
                {
                    if (a.account.Pinned)
                        return -1;
                    else
                        return 1;
                }

                if (grouping != Settings.GroupMode.None)
                {
                    result = 0;

                    if ((grouping & Settings.GroupMode.Active) == Settings.GroupMode.Active)
                    {
                        result = -a.IsActive.CompareTo(b.IsActive);
                    }

                    if (result == 0 && (grouping & Settings.GroupMode.Type) == Settings.GroupMode.Type)
                    {
                        result = a.account.Type.CompareTo(b.account.Type);
                    }

                    if (result != 0)
                    {
                        if (groupingReversed)
                            return -result;
                        return result;
                    }
                }

                switch (sorting)
                {
                    case Settings.SortMode.LaunchTime:

                        result = a.activated.CompareTo(b.activated);

                        break;
                    case Settings.SortMode.Name:

                        result = stringComparer.Compare(a.account.Name, b.account.Name);

                        break;
                    case Settings.SortMode.CustomGrid:
                    case Settings.SortMode.CustomList:

                        result = a.account.SortKey.CompareTo(b.account.SortKey);

                        break;
                    case Settings.SortMode.None:
                    default:

                        result = 0;

                        break;
                }

                if (result == 0)
                {
                    result = a.account.UID.CompareTo(b.account.UID);
                }

                if (sortingReversed)
                    return -result;

                return result;
            }
        }

        public enum AccountBarMode
        {
            Default,
            QuickLaunch
        }

        private Dictionary<ushort, AccountButton> accounts;
        private List<AccountButton> buttons;
        private AccountButton focused;
        private Settings.ScreenAnchor docked;
        private BarButton barTop, barBottom;
        private Orientation orientation;
        private Settings.SortingOptions sorting;
        private Image[] imageDefault;
        private bool isDirty, styleChanged, pendingIndexes;
        private formAccountTooltip tooltip;

        private Point sizingOrigin;
        private RECT sizingBounds;
        private bool snapped;
        private int
            snapX,
            snapY;
        private Snap[] snaps;
        private Rectangle boundsBar;
        private Settings.ScreenAnchor boundsBarType;
        private bool delayedSorting;

        private Panel panelContent, panelContainer;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem menuSortBy, menuSortByActive, menuSortByName, menuSortByCustom, menuSortByAscending, menuSortByDescending, menuGroupByActive, menuOnlyShowActive,
            menuOptions, menuStyleName, menuStyleIcon, menuStyleColor, menuStyleClose, menuStyleHighlightLastFocused, menuStyleHorizontal, menuStyleVertical, menuAutoHide,
            menuHide, menuTopMost, menuDisable, menuGroupByType, menuOnlyShowInactive, menuHideGw1, menuHideGw2;

        private Settings.IAccount dockedTo;
        private bool canDockResize, canShow, canLaunch;
        private bool isLocked;
        private AccountBarMode mode;

        public formAccountBar()
            : this(AccountBarMode.Default)
        {

        }

        public formAccountBar(AccountBarMode mode)
        {
            this.mode = mode;
            orientation = Orientation.Vertical;
            docked = Settings.ScreenAnchor.None;

            InitializeComponents();

            var isize = Scale(16);
            imageDefault = new Image[2];
            using (var icon = new Icon(Properties.Resources.Gw2, isize, isize))
            {
                imageDefault[(byte)Settings.AccountType.GuildWars2] = icon.ToBitmap();
            }
            using (var icon = new Icon(Properties.Resources.Gw1, isize, isize))
            {
                imageDefault[(byte)Settings.AccountType.GuildWars1] = icon.ToBitmap();
            }

            panelContainer.MouseWheel += panelContainer_MouseWheel;

            if (mode == AccountBarMode.Default)
            {
                panelContainer.MouseClick += panelContainer_MouseClick;
                barTop.MouseDown += barTop_MouseDown;
            }

            accounts = new Dictionary<ushort, AccountButton>();
            buttons = new List<AccountButton>();

            sorting = Settings.AccountBar.Sorting.Value;

            boundsBarType = docked;

            if (!Settings.AccountBar.Options.HasValue)
            {
                Settings.AccountBar.Options.Value = Settings.AccountBarOptions.TopMost;
                Settings.AccountBar.Sorting.Value = sorting = new Settings.SortingOptions(Settings.SortMode.LaunchTime, false, Settings.GroupMode.Active, false);
            }

            if (!Settings.AccountBar.Style.HasValue)
                Settings.AccountBar.Style.Value = Settings.AccountBarStyles.Color | Settings.AccountBarStyles.Exit | Settings.AccountBarStyles.HighlightFocused | Settings.AccountBarStyles.Icon | Settings.AccountBarStyles.Name;

            if (mode == AccountBarMode.Default)
            {
                InitializeMenu();

                if (!Settings.AccountBar.Enabled.Value)
                    Settings.AccountBar.Enabled.Value = true;

                AccountBarOptions_ValueChanged(Settings.AccountBar.Options, EventArgs.Empty);
                AccountBarStyle_ValueChanged(Settings.AccountBar.Style, EventArgs.Empty);

                if (Settings.AccountBar.Docked.HasValue)
                    LayoutDocked = boundsBarType = Settings.AccountBar.Docked.Value;

                var v = Settings.WindowBounds[this.GetType()];
                if (v.HasValue)
                {
                    boundsBar = Util.ScreenUtil.Constrain(v.Value);
                }
                else
                {
                    var screen = Screen.PrimaryScreen.Bounds;
                    boundsBar = new Rectangle(screen.X + Scale(10), screen.Top + Scale(10), Scale(200), Scale(300));
                }

                if (docked != Settings.ScreenAnchor.None)
                {
                    var screen = Screen.FromRectangle(boundsBar);
                    canDockResize = screen.WorkingArea != screen.Bounds;
                }

                Settings.AccountBar.Enabled.ValueChanged += AccountBarEnabled_ValueChanged;
                Settings.AccountBar.Options.ValueChanged += AccountBarOptions_ValueChanged;
                Settings.AccountBar.Style.ValueChanged += AccountBarStyle_ValueChanged;

                Settings.WindowBounds[this.GetType()].ValueCleared += WindowBounds_ValueCleared;
            }
            else
            {
                AccountBarStyle_ValueChanged(Settings.AccountBar.Style, EventArgs.Empty);

                Settings.AccountBar.Style.ValueChanged += AccountBarStyle_ValueChanged;

                ShowTopMost = true;
            }

            Settings.Accounts.ValueChanged += Accounts_ValueChanged;
            Settings.Accounts.ValueAdded += Accounts_ValueAdded;
            Settings.Accounts.ValueRemoved += Accounts_ValueRemoved;

            Settings.AccountSorting.SortingChanged += AccountSorting_SortingChanged;

            Client.Launcher.AccountStateChanged += Launcher_AccountStateChanged;
            Client.Launcher.AccountWindowEvent += Launcher_AccountWindowEvent;
            Client.Launcher.AccountTopMostWindowEvent += Launcher_AccountTopMostWindowEvent;
            Client.Launcher.AccountProcessExited += Launcher_AccountProcessExited;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            this.Text = "Accounts";
            this.BackColor = Color.Black;
            this.Opacity = 0f;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.CanLaunch = true;

            panelContainer = new Panel()
            {
                Visible = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
            };

            panelContent = new Panel()
            {
                Visible = true,
                Size = panelContainer.Size,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
            };

            barTop = new BarButton(false)
            {
                BackColor = Color.FromArgb(15, 15, 15),
                BackColorHovered = Color.FromArgb(20, 20, 20),
            };

            barBottom = new BarButton(true)
            {
                BackColor = Color.FromArgb(15, 15, 15),
                BackColorHovered = Color.FromArgb(20, 20, 20),
            };


            panelContainer.Controls.Add(panelContent);
            this.Controls.AddRange(new Control[] { panelContainer, barTop, barBottom });
        }

        public void Initialize(bool forceShow)
        {
            var h = this.Handle;
            SetLayoutBounds();

            var changed = false;
            var active = new HashSet<ushort>();

            foreach (var account in Client.Launcher.GetActiveProcesses())
            {
                switch (Client.Launcher.GetState(account))
                {
                    case Client.Launcher.AccountState.Active:
                    case Client.Launcher.AccountState.ActiveGame:

                        active.Add(account.UID);

                        break;
                }
            }

            foreach (var account in Util.Accounts.GetAccounts())
            {
                AccountButton b;

                accounts[account.UID] = b = Create(account);
                bool visible;
                if (b.IsActive = active.Contains(account.UID))
                    visible = !OnlyShowInactive;
                else
                    visible = !OnlyShowActive;
                if (visible && !IsHidden(b.account.Type))
                {
                    if (b.IsActive && b.visible)
                        _ActiveCount++;
                    SetVisible(b, true);
                    changed = true;
                }

                Settings.Accounts[account.UID].ValueCleared += Account_ValueCleared;
            }

            if (!sorting.IsEmpty)
                SetSorting(sorting, changed);

            DisableNextVisibilityChange = !forceShow && AutoHide && ActiveCount == 0;

            if (mode == AccountBarMode.QuickLaunch)
            {
                ShowPopup();
            }
        }

        void WindowBounds_ValueCleared(object sender, Rectangle e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    WindowBounds_ValueCleared(sender, e);
                }))
                return;

            int w, h;
            if (HorizontalLayout)
            {
                w = Screen.FromPoint(boundsBar.Location).WorkingArea.Width / 2;
                h = (this.FontHeight - 1) * 2;
            }
            else
            {
                w = Scale(200);
                h = Scale(300);
            }

            LayoutDocked = Settings.ScreenAnchor.None;

            var screen = Screen.PrimaryScreen.Bounds;
            boundsBar = new Rectangle(screen.X + Scale(10), screen.Top + Scale(10), w, h);
            boundsBarType = Settings.ScreenAnchor.None;

            this.MinimumSize = this.MaximumSize = Size.Empty;
            this.Bounds = boundsBar;
            OnLayoutChanged();

            OnButtonsChanged();
        }

        private bool _ShowTopMost;
        private bool ShowTopMost
        {
            get
            {
                return _ShowTopMost;
            }
            set
            {
                if (_ShowTopMost != value)
                {
                    _ShowTopMost = value;
                    if (menuTopMost != null)
                        menuTopMost.Checked = _ShowTopMost;

                    if (this.IsHandleCreated)
                    {
                        IntPtr z;
                        if (value)
                            z = (IntPtr)WindowZOrder.HWND_TOPMOST;
                        else
                            z = (IntPtr)WindowZOrder.HWND_NOTOPMOST;

                        NativeMethods.SetWindowPos(this.Handle, z, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
                    }
                }
            }
        }

        private bool _ShowClose;
        private bool ShowCloseButton
        {
            get
            {
                return _ShowClose;
            }
            set
            {
                if (_ShowClose != value)
                {
                    _ShowClose = value;
                    if (menuStyleClose != null)
                        menuStyleClose.Checked = value;
                    OnStyleChanged();
                }
            }
        }

        private bool _ShowColor;
        private bool ShowColorKey
        {
            get
            {
                return _ShowColor;
            }
            set
            {
                if (_ShowColor != value)
                {
                    _ShowColor = value;
                    if (menuStyleColor != null)
                        menuStyleColor.Checked = value;
                    OnStyleChanged();
                }
            }
        }

        private bool _ShowName;
        private bool ShowAccountName
        {
            get
            {
                return _ShowName;
            }
            set
            {
                if (_ShowName != value)
                {
                    _ShowName = value;
                    if (menuStyleName != null)
                        menuStyleName.Checked = value;
                    OnStyleChanged();
                }
            }
        }

        private bool _ShowIcon;
        private bool ShowAccountIcon
        {
            get
            {
                return _ShowIcon;
            }
            set
            {
                if (_ShowIcon != value)
                {
                    _ShowIcon = value;
                    if (menuStyleIcon != null)
                        menuStyleIcon.Checked = value;
                    OnStyleChanged();
                }
            }
        }

        private bool _ShowFocusedHighlight;
        private bool ShowFocusedHighlight
        {
            get
            {
                return _ShowFocusedHighlight;
            }
            set
            {
                if (_ShowFocusedHighlight != value)
                {
                    _ShowFocusedHighlight = value;
                    if (menuStyleHighlightLastFocused != null)
                        menuStyleHighlightLastFocused.Checked = value;

                    if (focused != null && focused.button != null)
                        focused.button.Selected = value;
                }
            }
        }

        private bool _HorizontalLayout;
        private bool HorizontalLayout
        {
            get
            {
                return _HorizontalLayout;
            }
            set
            {
                if (_HorizontalLayout != value)
                {
                    _HorizontalLayout = value;

                    if (value)
                        orientation = Orientation.Horizontal;
                    else
                        orientation = Orientation.Vertical;

                    if (docked == Settings.ScreenAnchor.None)
                    {
                        if (menuStyleVertical != null)
                            menuStyleVertical.Checked = !value;
                        if (menuStyleHorizontal != null)
                            menuStyleHorizontal.Checked = value;

                        OnLayoutChanged();

                        OnButtonsChanged();
                    }
                }
            }
        }

        private bool _OnlyShowActive;
        private bool OnlyShowActive
        {
            get
            {
                return _OnlyShowActive;
            }
            set
            {
                if (_OnlyShowActive != value)
                {
                    _OnlyShowActive = value;
                    if (menuOnlyShowActive != null)
                        menuOnlyShowActive.Checked = value;
                    OnIndexResetRequired();
                }
            }
        }

        private bool _OnlyShowInactive;
        private bool OnlyShowInactive
        {
            get
            {
                return _OnlyShowInactive;
            }
            set
            {
                if (_OnlyShowInactive != value)
                {
                    _OnlyShowInactive = value;
                    if (menuOnlyShowInactive != null)
                        menuOnlyShowInactive.Checked = value;
                    OnIndexResetRequired();
                }
            }
        }

        private bool _HideGw1;
        public bool HideGw1
        {
            get
            {
                return _HideGw1;
            }
            set
            {
                if (_HideGw1 != value)
                {
                    _HideGw1 = value;
                    if (menuHideGw1 != null)
                        menuHideGw1.Checked = value;
                    OnIndexResetRequired();
                }
            }
        }

        private bool _HideGw2;
        public bool HideGw2
        {
            get
            {
                return _HideGw2;
            }
            set
            {
                if (_HideGw2 != value)
                {
                    _HideGw2 = value;
                    if (menuHideGw2 != null)
                        menuHideGw2.Checked = value;
                    OnIndexResetRequired();
                }
            }
        }

        private bool _AutoHide;
        private bool AutoHide
        {
            get
            {
                return _AutoHide;
            }
            set
            {
                if (_AutoHide != value)
                {
                    _AutoHide = value;
                    if (menuAutoHide != null)
                        menuAutoHide.Checked = value;

                    if (this.IsHandleCreated)
                    {
                        if (value)
                        {
                            if (menuAutoHide == null || !menuAutoHide.Visible)
                                this.Visible = _ActiveCount > 0;
                        }
                        else if (!this.Visible && this.Enabled)
                        {
                            this.Visible = true;
                        }
                    }
                }
            }
        }

        private int _ActiveCount;
        private int ActiveCount
        {
            get
            {
                return _ActiveCount;
            }
            set
            {
                if (_ActiveCount != value)
                {
                    if (_AutoHide)
                    {
                        if (value == 0)
                        {
                            if (this.IsHandleCreated)
                            {
                                if (!this.Bounds.Contains(Cursor.Position))
                                {
                                    this.Visible = false;
                                }
                                else
                                {
                                    AutoHideOnMouseLeave();
                                }
                            }
                        }
                        else if (_ActiveCount == 0 && this.Enabled)
                        {
                            if (this.IsHandleCreated)
                                this.Visible = true;
                        }
                    }

                    _ActiveCount = value;
                }
            }
        }

        private async void AutoHideOnMouseLeave()
        {
            do
            {
                await Task.Delay(1000);

                var counter = 0;

                while (!this.Bounds.Contains(Cursor.Position))
                {
                    if (!_AutoHide || _ActiveCount != 0)
                        return;

                    if (++counter == 3)
                    {
                        this.Visible = false;

                        return;
                    }

                    await Task.Delay(1000);
                }
            }
            while (_AutoHide && _ActiveCount == 0);
        }

        public bool CanLaunch
        {
            get
            {
                if (isLocked)
                    return false;
                return canLaunch;
            }
            set
            {
                if (canLaunch != value)
                    canLaunch = value;
            }
        }

        void AccountSorting_SortingChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    AccountSorting_SortingChanged(sender, e);
                }))
                return;

            switch (sorting.Sorting.Mode)
            {
                case Settings.SortMode.CustomGrid:
                case Settings.SortMode.CustomList:

                    SortButtons();

                    break;
            }
        }

        void AccountBarStyle_ValueChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    AccountBarStyle_ValueChanged(sender, e);
                }))
                return;

            var v = (Settings.ISettingValue<Settings.AccountBarStyles>)sender;

            if (v.HasValue)
            {
                var style = v.Value;

                ShowColorKey = style.HasFlag(Settings.AccountBarStyles.Color);
                ShowFocusedHighlight = style.HasFlag(Settings.AccountBarStyles.HighlightFocused);
                ShowCloseButton = style.HasFlag(Settings.AccountBarStyles.Exit);
                ShowAccountIcon = style.HasFlag(Settings.AccountBarStyles.Icon);
                ShowAccountName = style.HasFlag(Settings.AccountBarStyles.Name);
            }
        }

        void AccountBarEnabled_ValueChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    AccountBarEnabled_ValueChanged(sender, e);
                }))
                return;

            var v = (Settings.ISettingValue<bool>)sender;

            if (!v.Value)
                this.Dispose();
        }

        void AccountBarOptions_ValueChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    AccountBarOptions_ValueChanged(sender, e);
                }))
                return;

            var v = (Settings.ISettingValue<Settings.AccountBarOptions>)sender;

            if (v.HasValue)
            {
                var options = v.Value;

                HorizontalLayout = options.HasFlag(Settings.AccountBarOptions.HorizontalLayout);
                OnlyShowActive = options.HasFlag(Settings.AccountBarOptions.OnlyShowActive);
                OnlyShowInactive = options.HasFlag(Settings.AccountBarOptions.OnlyShowInactive);
                AutoHide = options.HasFlag(Settings.AccountBarOptions.AutoHide);
                ShowTopMost = options.HasFlag(Settings.AccountBarOptions.TopMost);
                HideGw1 = options.HasFlag(Settings.AccountBarOptions.HideGw1);
                HideGw2 = options.HasFlag(Settings.AccountBarOptions.HideGw2);
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                var style = WindowStyle.WS_EX_TOOLWINDOW | WindowStyle.WS_EX_LAYERED;
                if (_ShowTopMost)
                    style |= WindowStyle.WS_EX_TOPMOST;
                createParams.ExStyle |= (int)style;
                return createParams;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.Opacity = 0.98f;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (ShowTopMost)
            {
                NativeMethods.SetWindowPos(this.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
            }
        }

        void panelContainer_MouseWheel(object sender, MouseEventArgs e)
        {
            switch (LayoutOrientation)
            {
                case Orientation.Horizontal:

                    int w;
                    if (panelContent.Width > (w = panelContainer.Width))
                    {
                        var change = w / 5;
                        var l = panelContent.Left;

                        if (e.Delta < 0)
                            change = -change;
                        l += change;

                        if (l > 0)
                            l = 0;
                        else if (l + panelContent.Width < w)
                            l = w - panelContent.Width;

                        panelContent.Left = l;
                    }

                    break;
                case Orientation.Vertical:

                    int h;
                    if (panelContent.Height > (h = panelContainer.Height))
                    {
                        var change = h / 5;
                        var t = panelContent.Top;

                        if (e.Delta < 0)
                            change = -change;
                        t += change;

                        if (t > 0)
                            t = 0;
                        else if (t + panelContent.Height < h)
                            t = h - panelContent.Height;

                        panelContent.Top = t;
                    }

                    break;
            }
        }

        private void InitializeMenu()
        {
            menuSortBy = new Controls.ToolStripMenuItemStayOpenOnClick()
            {
                Text = "Sort by",
                StayOpenOnClick = true,
            };

            var menuSortByOnlyShow = new Controls.ToolStripMenuItemStayOpenOnClick()
            {
                Text = "Only show...",
                StayOpenOnClick = true,
            };

            menuSortByOnlyShow.DropDownItems.AddRange(new ToolStripMenuItem[]
                {
                    menuOnlyShowActive = new ToolStripMenuItemStayOpenOnClick("Active accounts",null,OnMenuOptionClicked,true),
                    menuOnlyShowInactive = new ToolStripMenuItemStayOpenOnClick("Inactive accounts",null,OnMenuOptionClicked,true),
                });

            var menuSortByHide = new Controls.ToolStripMenuItemStayOpenOnClick()
            {
                Text = "Hide...",
                StayOpenOnClick = true,
            };

            menuSortByHide.DropDownItems.AddRange(new ToolStripMenuItem[]
                {
                    menuHideGw1 = new ToolStripMenuItemStayOpenOnClick("Guild Wars 1 accounts",null,OnMenuOptionClicked,true),
                    menuHideGw2 = new ToolStripMenuItemStayOpenOnClick("Guild Wars 2 accounts",null,OnMenuOptionClicked,true),
                });

            menuSortBy.DropDownItems.AddRange(new ToolStripItem[]
                {
                    menuGroupByActive = new ToolStripMenuItemStayOpenOnClick("Active",null,OnMenuGroupByClicked,true),
                    menuGroupByType = new ToolStripMenuItemStayOpenOnClick("Type",null,OnMenuGroupByClicked,true),
                    new ToolStripSeparator(),
                    menuSortByActive = new ToolStripMenuItemStayOpenOnClick("Launch time",null,OnMenuSortByModeClicked,true),
                    menuSortByName = new ToolStripMenuItemStayOpenOnClick("Name",null,OnMenuSortByModeClicked,true),
                    menuSortByCustom = new ToolStripMenuItemStayOpenOnClick("Custom",null,OnMenuSortByModeClicked,true),
                    new ToolStripSeparator(),
                    menuSortByAscending = new ToolStripMenuItemStayOpenOnClick("Ascending",null,OnMenuSortByOrderClicked,true),
                    menuSortByDescending = new ToolStripMenuItemStayOpenOnClick("Descending",null,OnMenuSortByOrderClicked,true),
                    new ToolStripSeparator(),
                    menuSortByOnlyShow,
                    menuSortByHide
                });

            menuSortByAscending.Checked = true;

            menuOptions = new Controls.ToolStripMenuItemStayOpenOnClick()
            {
                Text = "Options",
                StayOpenOnClick = true,
            };

            menuOptions.DropDownItems.AddRange(new ToolStripItem[]
                {
                    menuStyleName = new ToolStripMenuItemStayOpenOnClick("Show name",null, OnMenuStyleClicked,true),
                    menuStyleIcon = new ToolStripMenuItemStayOpenOnClick("Show icon",null, OnMenuStyleClicked,true),
                    menuStyleColor = new ToolStripMenuItemStayOpenOnClick("Show color",null, OnMenuStyleClicked,true),
                    new ToolStripSeparator(),
                    menuStyleClose = new ToolStripMenuItemStayOpenOnClick("Show account exit button",null, OnMenuStyleClicked,true),
                    new ToolStripSeparator(),
                    menuStyleHorizontal = new ToolStripMenuItemStayOpenOnClick("Horizontal layout",null, OnMenuStyleClicked,true),
                    menuStyleVertical = new ToolStripMenuItemStayOpenOnClick("Vertical layout",null, OnMenuStyleClicked,true),
                    new ToolStripSeparator(),
                    menuAutoHide = new ToolStripMenuItemStayOpenOnClick("Hide when no accounts are active",null,OnMenuOptionClicked,true),
                    menuStyleHighlightLastFocused = new ToolStripMenuItemStayOpenOnClick("Highlight last focused window",null, OnMenuStyleClicked,true),
                    menuTopMost = new ToolStripMenuItemStayOpenOnClick("Show on top of other windows",null,OnMenuOptionClicked,true),
                    new ToolStripSeparator(),
                    menuDisable = new ToolStripMenuItemStayOpenOnClick("Lock",null,OnMenuOptionClicked,true),
                });

            menuStyleVertical.Checked = true;

            menuHide = new ToolStripMenuItem("Hide", null, OnMenuHideClicked);

            contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.AddRange(new ToolStripItem[]
                {
                    menuSortBy,
                    menuOptions,
                    new ToolStripSeparator(),
                    menuHide,
                });
        }

        private void SetSorting(Settings.SortingOptions options, bool applyNow)
        {
            if (this.mode == AccountBarMode.Default)
            {
                var sorting = options.Sorting.Mode;
                var grouping = options.Grouping;
                var descending = options.Sorting.Descending;
                
                menuGroupByActive.Checked = grouping.HasFlag(Settings.GroupMode.Active);
                menuGroupByType.Checked = grouping.HasFlag(Settings.GroupMode.Type);
                menuSortByActive.Checked = sorting == Settings.SortMode.LaunchTime;
                menuSortByName.Checked = sorting == Settings.SortMode.Name;
                menuSortByCustom.Checked = sorting == Settings.SortMode.CustomList;

                menuSortByAscending.Checked = !descending;
                menuSortByDescending.Checked = descending;
            }

            if (applyNow)
            {
                SortButtons();
            }
        }

        private void SetOption(Settings.AccountBarOptions option, bool value)
        {
            var options = Settings.AccountBar.Options.Value;
            if (value)
                options |= option;
            else
                options &= ~option;

            if (value)
            {
                switch (option)
                {
                    case Settings.AccountBarOptions.OnlyShowActive:
                        options &= ~Settings.AccountBarOptions.OnlyShowInactive;
                        break;
                    case Settings.AccountBarOptions.OnlyShowInactive:
                        options &= ~Settings.AccountBarOptions.OnlyShowActive;
                        break;
                }
            }

            Settings.AccountBar.Options.Value = options;
        }

        private void SetStyle(Settings.AccountBarStyles style, bool value)
        {
            var styles = Settings.AccountBar.Style.Value;
            if (value)
                styles |= style;
            else
                styles &= ~style;
            Settings.AccountBar.Style.Value = styles;
        }

        private void OnMenuOptionClicked(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if (item == menuAutoHide)
            {
                SetOption(Settings.AccountBarOptions.AutoHide, !AutoHide);
            }
            else if (item == menuOnlyShowActive)
            {
                SetOption(Settings.AccountBarOptions.OnlyShowActive, !OnlyShowActive);
            }
            else if (item == menuTopMost)
            {
                SetOption(Settings.AccountBarOptions.TopMost, !ShowTopMost);
            }
            else if (item == menuDisable)
            {
                menuDisable.Checked = isLocked = !isLocked;
            }
            else if (item == menuOnlyShowInactive)
            {
                SetOption(Settings.AccountBarOptions.OnlyShowInactive, !OnlyShowInactive);
            }
            else if (item == menuHideGw1)
            {
                SetOption(Settings.AccountBarOptions.HideGw1, !HideGw1);
            }
            else if (item == menuHideGw2)
            {
                SetOption(Settings.AccountBarOptions.HideGw2, !HideGw2);
            }
        }

        private void OnMenuStyleClicked(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if (item == menuStyleName)
            {
                SetStyle(Settings.AccountBarStyles.Name, !ShowAccountName);
                //if (!ShowAccountName && !ShowAccountIcon)
                //    SetStyle(Settings.AccountBarStyles.Icon, true);
            }
            else if (item == menuStyleIcon)
            {
                SetStyle(Settings.AccountBarStyles.Icon, !ShowAccountIcon);
                //if (!ShowAccountName && !ShowAccountIcon)
                //    SetStyle(Settings.AccountBarStyles.Name, true);
            }
            else if (item == menuStyleColor)
            {
                SetStyle(Settings.AccountBarStyles.Color, !ShowColorKey);
            }
            else if (item == menuStyleHorizontal)
            {
                var b = menuStyleHorizontal.Checked;

                SetOption(Settings.AccountBarOptions.HorizontalLayout, true);

                if (!b)
                {
                    LayoutDocked = Settings.ScreenAnchor.None;
                    SetLayoutBounds();
                }

                OnButtonsChanged();
            }
            else if (item == menuStyleVertical)
            {
                var b = menuStyleVertical.Checked;

                SetOption(Settings.AccountBarOptions.HorizontalLayout, false);

                if (!b)
                {
                    LayoutDocked = Settings.ScreenAnchor.None;
                    SetLayoutBounds();
                }

                OnButtonsChanged();
            }
            else if (item == menuStyleClose)
            {
                SetStyle(Settings.AccountBarStyles.Exit, !ShowCloseButton);
            }
            else if (item == menuStyleHighlightLastFocused)
            {
                SetStyle(Settings.AccountBarStyles.HighlightFocused, !ShowFocusedHighlight);
            }
        }

        private void OnMenuHideClicked(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void OnMenuGroupByClicked(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            Settings.GroupMode mode;

            if (item == menuGroupByActive)
                mode = Settings.GroupMode.Active;
            else if (item == menuGroupByType)
                mode = Settings.GroupMode.Type;
            else
                return;

            var grouping = sorting.Grouping.Mode;
            //toggling the selected flag
            grouping = (grouping & ~mode) | (grouping ^ mode) & mode;
            
            Settings.AccountBar.Sorting.Value = sorting = new Settings.SortingOptions(sorting.Sorting, grouping, sorting.Grouping.Descending);
            SetSorting(sorting, true);
        }

        private void OnMenuSortByModeClicked(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            Settings.SortMode mode;

            if (item == menuSortByActive)
                mode = Settings.SortMode.LaunchTime;
            else if (item == menuSortByName)
                mode = Settings.SortMode.Name;
            else if (item == menuSortByCustom)
                mode = Settings.SortMode.CustomList;
            else
                return;

            if (mode == sorting.Sorting.Mode)
                mode = Settings.SortMode.None;

            Settings.AccountBar.Sorting.Value = sorting = new Settings.SortingOptions(mode, sorting.Sorting.Descending, sorting.Grouping);
            SetSorting(sorting, true);
        }

        private void OnMenuSortByOrderClicked(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var descending = item == menuSortByDescending;

            if (descending == sorting.Sorting.Descending)
                return;

            Settings.AccountBar.Sorting.Value = sorting = new Settings.SortingOptions(sorting.Sorting.Mode, descending, sorting.Grouping);
            SetSorting(sorting, true);
        }

        private void OnIndexResetRequired()
        {
            if (!pendingIndexes)
            {
                pendingIndexes = true;
                this.Invalidate();
            }
        }

        private void ResetIndexes()
        {
            pendingIndexes = false;

            var active = 0;

            buttons.Clear();

            foreach (var b in accounts.Values)
            {
                bool visible;

                if (b.IsActive)
                    visible = !OnlyShowInactive;
                else
                    visible = !OnlyShowActive;

                if (visible && !IsHidden(b.account.Type))
                {
                    if (b.button == null)
                        CreateButton(b);

                    if (b.IsActive)
                        active++;

                    b.visible = true;
                    b.button.Visible = true;
                    b.index = (short)buttons.Count;
                    buttons.Add(b);
                }
                else
                {
                    b.visible = false;
                    if (b.button != null)
                        b.button.Visible = false;
                    b.index = -1;
                }
            }

            SortButtons();

            this._ActiveCount = active;
        }

        private Color GetColor(Settings.IAccount account)
        {
            if (account.ColorKey.IsEmpty)
                return Util.Color.FromUID(account.UID);
            else
                return account.ColorKey;
        }

        private IconValue GetIcon(Settings.IAccount account)
        {
            try
            {
                switch (account.IconType)
                {
                    case Settings.IconType.File:
                        return new IconValue(Image.FromFile(account.Icon), true);
                    case Settings.IconType.ColorKey:
                    case Settings.IconType.Gw2LauncherColorKey:
                        using (var icons = Tools.Icons.From(GetColor(account), account.IconType == Settings.IconType.Gw2LauncherColorKey))
                        {
                            return new IconValue(icons.Small.ToBitmap(), true);
                        }
                    case Settings.IconType.None:
                    default:
                        return new IconValue(imageDefault[(byte)account.Type], false);
                }
            }
            catch
            {
                return null;
            }
        }

        private void CreateButton(AccountButton b)
        {
            b.CreateButton(this.Font);
            b.SetLayout(LayoutOrientation, docked);

            b.button.TextVisible = ShowAccountName;
            b.button.IconVisible = ShowAccountIcon;
            b.button.ColorKeyVisible = ShowColorKey;
            b.button.CloseVisible = ShowCloseButton && b.IsActive;

            b.button.ColorKey = GetColor(b.account);
            b.Icon = GetIcon(b.account);

            b.button.CloseClicked += button_CloseClicked;
            b.button.BarClicked += button_BarClicked;
            b.button.MouseEnter += button_MouseEnter;
            b.button.MouseLeave += button_MouseLeave;

            panelContent.Controls.Add(b.button);
        }

        void button_MouseLeave(object sender, EventArgs e)
        {
            if (tooltip != null && tooltip.Tag == sender)
            {
                tooltip.HideTooltip();
                tooltip.Tag = null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }
            base.OnFormClosing(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            switch (LayoutOrientation)
            {
                case Orientation.Horizontal:

                    if (panelContent.Left < 0)
                    {
                        if (panelContent.Width > panelContainer.Width)
                            panelContent.Left = panelContainer.Width - panelContent.Width;
                        else
                            panelContent.Left = 0;
                    }

                    break;
                case Orientation.Vertical:

                    if (panelContent.Top < 0)
                    {
                        if (panelContent.Height > panelContainer.Height)
                            panelContent.Top = panelContainer.Height - panelContent.Height;
                        else
                            panelContent.Top = 0;
                    }

                    break;
            }
            base.OnSizeChanged(e);
        }

        void button_MouseEnter(object sender, EventArgs e)
        {
            var b = (AccountBarButton)sender;

            if (!b.IsTextClipped || isLocked)
                return;

            if (tooltip == null)
                tooltip = new formAccountTooltip();

            var bounds = new Rectangle(b.PointToScreen(Point.Empty), b.Size);

            AnchorStyles a;

            switch (docked)
            {
                case Settings.ScreenAnchor.Bottom:
                    a = AnchorStyles.Bottom;
                    break;
                case Settings.ScreenAnchor.Right:
                    a = AnchorStyles.Right;
                    break;
                case Settings.ScreenAnchor.Top:
                    a = AnchorStyles.Top;
                    break;
                case Settings.ScreenAnchor.Left:
                default:

                    switch (LayoutOrientation)
                    {
                        case Orientation.Horizontal:
                            a = AnchorStyles.Top;
                            break;
                        case Orientation.Vertical:
                        default:
                            a = AnchorStyles.Left;
                            break;
                    }

                    break;
            }

            NativeMethods.SetWindowPos(tooltip.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOOWNERZORDER);

            tooltip.AttachTo(bounds, -Scale(8), a);
            tooltip.Show(this, b.Account.Name, 100);
            tooltip.Tag = b;
        }

        async void button_BarClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var b = (AccountBarButton)sender;

                if (accounts[b.Account.UID].IsActive)
                {
                    var p = Client.Launcher.GetProcess(b.Account);

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
                else if (CanLaunch)
                {
                    var mode = Client.Launcher.LaunchMode.Launch;
                    if (Settings.ActionInactiveLClick.Value == Settings.ButtonAction.LaunchSingle)
                        mode = Client.Launcher.LaunchMode.LaunchSingle;
                    Client.Launcher.Launch(b.Account, mode);
                }
                else
                {
                    Cursor.Current = Cursors.No;
                }
            }
        }

        void button_CloseClicked(object sender, MouseEventArgs e)
        {
            var account = ((AccountBarButton)sender).Account;
            if (Client.Launcher.Kill(account))
            {
                AccountButton b;
                if (accounts.TryGetValue(account.UID, out b) && b.IsActive)
                    OnAccountExited(b);
            }
        }

        private void SetFocused(AccountButton b)
        {
            if (focused != null)
            {
                if (focused == b)
                    return;

                focused.button.Selected = false;
            }

            if (b != null && b.button != null)
            {
                focused = b;
                b.button.Selected = ShowFocusedHighlight;
            }
            else
                focused = null;
        }

        private bool IsHidden(Settings.AccountType type)
        {
            switch (type)
            {
                case Settings.AccountType.GuildWars1:

                    return _HideGw1;

                case Settings.AccountType.GuildWars2:

                    return _HideGw2;
            }

            return false;
        }

        private bool SetVisible(AccountButton b, bool visible)
        {
            if (b.visible == visible)
                return false;

            if (visible)
            {
                if (b.button == null)
                    CreateButton(b);

                if (b.index == -1)
                {
                    b.index = (short)buttons.Count;
                    buttons.Add(b);
                }
            }
            else
            {
                if (b.index != -1)
                {
                    buttons.Remove(b);

                    for (short i = b.index, count = (short)buttons.Count; i < count; i++)
                    {
                        buttons[i].index = i;
                    }

                    b.index = -1;
                }
            }

            b.visible = visible;
            b.button.Visible = visible;

            if (b.IsActive)
            {
                if (visible)
                    ActiveCount++;
                else
                    ActiveCount--;
            }

            OnButtonsChanged();

            return true;
        }

        private void OnButtonsChanged()
        {
            if (!isDirty)
            {
                isDirty = true;
                this.Invalidate();
            }
        }

        private void Remove(ushort uid)
        {
            AccountButton b;

            if (accounts.TryGetValue(uid, out b))
            {
                accounts.Remove(uid);

                if (b.index >= 0)
                {
                    buttons.Remove(b);

                    for (var i = b.index; i < buttons.Count; i++)
                    {
                        buttons[i].index = i;
                    }

                    OnButtonsChanged();
                }

                if (b.IsActive)
                {
                    b.IsActive = false;
                    if (b.visible)
                        ActiveCount--;
                }

                if (b.button != null)
                {
                    panelContent.Controls.Remove(b.button);
                    b.button.Dispose();
                }

                using (b.Icon) { }
            }
        }

        void Accounts_ValueRemoved(object sender, ushort uid)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    Remove(uid);
                });
        }

        void Accounts_ValueAdded(object sender, KeyValuePair<ushort, Settings.ISettingValue<Settings.IAccount>> e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    Accounts_ValueAdded(sender, e);
                }))
                return;

            var account = e.Value.Value;
            AccountButton b;

            if (!accounts.TryGetValue(account.UID, out b))
            {
                accounts[account.UID] = b = Create(account);

                if (!OnlyShowActive)
                {
                    SetVisible(b, !IsHidden(account.Type));
                }

                e.Value.ValueCleared += Account_ValueCleared;
            }
        }

        void Account_ValueCleared(object sender, Settings.IAccount account)
        {
            Util.Invoke.Required(this,
                delegate
                {
                    Remove(account.UID);
                });
        }

        void Accounts_ValueChanged(object sender, KeyValuePair<ushort, Settings.ISettingValue<Settings.IAccount>> e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    Accounts_ValueChanged(sender, e);
                }))
                return;

            Remove(e.Key);

            if (e.Value.HasValue)
            {
                Accounts_ValueAdded(sender, e);
            }
        }

        void account_NameChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    account_NameChanged(sender, e);
                }))
                return;

            var account = (Settings.IAccount)sender;
            AccountButton b;

            if (accounts.TryGetValue(account.UID, out b) && b.button != null)
            {
                b.button.Text = account.Name;
            }
        }

        void account_ColorKeyChanged(object sender, EventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    account_ColorKeyChanged(sender, e);
                }))
                return;

            var account = (Settings.IAccount)sender;
            AccountButton b;

            if (accounts.TryGetValue(account.UID, out b) && b.button != null)
            {
                b.button.ColorKey = GetColor(account);
            }
        }

        void account_IconChanged(object sender, EventArgs e)
        {
            var account = (Settings.IAccount)sender;
            if (account.IconType != Settings.IconType.File || string.IsNullOrEmpty(account.Icon))
                return;

            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    account_IconChanged(sender, e);
                }))
                return;

            AccountButton b;

            if (accounts.TryGetValue(account.UID, out b) && b.button != null)
            {
                b.Icon = GetIcon(account);
            }
        }

        void account_IconTypeChanged(object sender, EventArgs e)
        {
            var account = (Settings.IAccount)sender;
            if (account.IconType == Settings.IconType.File && string.IsNullOrEmpty(account.Icon))
                return;

            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    account_IconTypeChanged(sender, e);
                }))
                return;

            AccountButton b;

            if (accounts.TryGetValue(account.UID, out b) && b.button != null)
            {
                b.Icon = GetIcon(account);
            }
        }

        void account_PinnedChanged(object sender, EventArgs e)
        {
            SortButtonsDelayed();
        }

        private Orientation LayoutOrientation
        {
            get
            {
                switch (docked)
                {
                    case Settings.ScreenAnchor.Left:
                    case Settings.ScreenAnchor.Right:
                        return Orientation.Vertical;
                    case Settings.ScreenAnchor.Top:
                    case Settings.ScreenAnchor.Bottom:
                        return Orientation.Horizontal;
                }

                return orientation;
            }
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                }
            }
        }

        private Settings.ScreenAnchor LayoutDocked
        {
            get
            {
                return docked;
            }
            set
            {
                if (docked != value)
                {
                    if (mode == AccountBarMode.Default)
                    {
                        if (value != Settings.ScreenAnchor.None)
                        {
                            if (docked == Settings.ScreenAnchor.None)
                            {
                                menuStyleVertical.Checked = false;
                                menuStyleHorizontal.Checked = false;
                            }
                        }
                        else
                        {
                            menuStyleVertical.Checked = !_HorizontalLayout;
                            menuStyleHorizontal.Checked = _HorizontalLayout;
                        }
                    }

                    docked = value;
                }
            }
        }

        void barTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (!isLocked)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(this.Handle, (uint)WindowMessages.WM_NCLBUTTONDOWN, (IntPtr)HitTest.Caption, IntPtr.Zero);
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
        }

        void panelContainer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
        }

        void Launcher_AccountProcessExited(Settings.IAccount account, System.Diagnostics.Process e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    Launcher_AccountProcessExited(account, e);
                }))
                return;

            if (dockedTo == account)
            {
                dockedTo = null;
                ResizeDockedBounds(Screen.FromRectangle(this.Bounds).WorkingArea);
            }
        }

        void Launcher_AccountTopMostWindowEvent(Settings.IAccount account, Client.Launcher.AccountTopMostWindowEventEventArgs e)
        {
            if (ShowTopMost)
            {
                Util.Invoke.Required(this,
                    delegate
                    {
                        e.Add(0, this.Handle);
                    });
            }
        }

        void Launcher_AccountWindowEvent(Settings.IAccount account, Client.Launcher.AccountWindowEventEventArgs e)
        {
            if (Util.Invoke.IfRequired(this,
                delegate
                {
                    Launcher_AccountWindowEvent(account, e);
                }))
                return;

            AccountButton b;
            if (accounts.TryGetValue(account.UID, out b))
            {
                switch (e.Type)
                {
                    case Client.Launcher.AccountWindowEventEventArgs.EventType.Focused:

                        SetFocused(b);

                        if (docked != Settings.ScreenAnchor.None)
                        {
                            CheckWindowBounds(account, e.Handle);
                        }

                        break;
                    case Client.Launcher.AccountWindowEventEventArgs.EventType.Minimized:

                        if (dockedTo == account)
                        {
                            dockedTo = null;
                            ResizeDockedBounds(Screen.FromRectangle(this.Bounds).WorkingArea);
                        }

                        break;
                    case Client.Launcher.AccountWindowEventEventArgs.EventType.WindowLoaded:

                        if (docked != Settings.ScreenAnchor.None && NativeMethods.GetForegroundWindow() == e.Handle)
                        {
                            CheckWindowBounds(account, e.Handle);
                        }

                        break;
                }
            }
            else
            {
                switch (e.Type)
                {
                    case Client.Launcher.AccountWindowEventEventArgs.EventType.Focused:

                        SetFocused(null);

                        break;
                }
            }
        }

        private bool SetTopMostClient(IntPtr handle)
        {
            try
            {
                var d = NativeMethods.BeginDeferWindowPos(2);
                if (d != IntPtr.Zero)
                {
                    d = NativeMethods.DeferWindowPos(d, handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, (uint)(SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE));
                    if (d != IntPtr.Zero)
                    {
                        d = NativeMethods.DeferWindowPos(d, this.Handle, (IntPtr)WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, (uint)(SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE));
                        if (d != IntPtr.Zero)
                            return NativeMethods.EndDeferWindowPos(d);
                    }
                }
            }
            catch { }

            return false;
        }

        private void CheckWindowBounds(Settings.IAccount account, IntPtr handle)
        {
            var placement = new WINDOWPLACEMENT();
            if (NativeMethods.GetWindowPlacement(handle, ref placement))
            {
                var rect = placement.rcNormalPosition;
                var screen = Screen.FromRectangle(this.Bounds);
                var bounds = screen.Bounds;

                if (bounds.Left == rect.left && bounds.Top == rect.top && bounds.Right == rect.right && bounds.Bottom == rect.bottom)
                {
                    dockedTo = account;

                    ResizeDockedBounds(bounds);
                }
            }
        }

        private void ResizeDockedBounds(Rectangle bounds)
        {
            switch (docked)
            {
                case Settings.ScreenAnchor.Left:
                    SetBounds(bounds.Left, bounds.Top, this.Width, bounds.Height);
                    break;
                case Settings.ScreenAnchor.Right:
                    SetBounds(bounds.Right - this.Width, bounds.Top, this.Width, bounds.Height);
                    break;
                case Settings.ScreenAnchor.Bottom:
                    SetBounds(bounds.Left, bounds.Bottom - this.Height, bounds.Width, this.Height);
                    break;
                case Settings.ScreenAnchor.Top:
                    SetBounds(bounds.Left, bounds.Top, bounds.Width, this.Height);
                    break;
            }
        }

        void Launcher_AccountStateChanged(Settings.IAccount account, Client.Launcher.AccountStateEventArgs e)
        {
            switch (e.State)
            {
                case Client.Launcher.AccountState.Active:
                case Client.Launcher.AccountState.ActiveGame:
                case Client.Launcher.AccountState.Exited:
                case Client.Launcher.AccountState.Waiting:
                case Client.Launcher.AccountState.WaitingForOtherProcessToExit:
                case Client.Launcher.AccountState.Launching:
                case Client.Launcher.AccountState.None:

                    if (Util.Invoke.IfRequired(this,
                        delegate
                        {
                            Launcher_AccountStateChanged(account, e);
                        }))
                        return;

                    break;
                default:
                    return;
            }

            AccountButton b;

            switch (e.State)
            {
                case Client.Launcher.AccountState.Active:
                case Client.Launcher.AccountState.ActiveGame:

                    if (accounts.TryGetValue(e.UID, out b) && !b.IsActive)
                        OnAccountActive(b);

                    break;
                case Client.Launcher.AccountState.Exited:
                case Client.Launcher.AccountState.None:

                    if (accounts.TryGetValue(e.UID, out b))
                    {
                        b.IsInUse = false;
                        if (b.IsActive)
                            OnAccountExited(b);
                    }

                    break;
                case Client.Launcher.AccountState.Waiting:
                case Client.Launcher.AccountState.WaitingForOtherProcessToExit:
                case Client.Launcher.AccountState.Launching:

                    if (accounts.TryGetValue(e.UID, out b))
                        b.IsInUse = true;

                    break;
            }
        }

        private void OnAccountActive(AccountButton b)
        {
            if (!b.IsActive)
            {
                b.IsActive = true;
                if (b.visible)
                    ActiveCount++;
            }

            if (b.button != null)
                b.button.CloseVisible = _ShowClose;

            var changed = SetVisible(b, !OnlyShowInactive && !IsHidden(b.account.Type));

            if (b.visible)
            {
                if (changed || sorting.Sorting.Mode == Settings.SortMode.LaunchTime || sorting.Grouping.HasFlag(Settings.GroupMode.Active))
                {
                    SortButtons();
                }
            }
        }

        private void OnAccountExited(AccountButton b)
        {
            if (b.IsActive)
            {
                b.IsActive = false;
                if (b.visible)
                    ActiveCount--;
            }

            if (b.button != null)
                b.button.CloseVisible = false;

            if (focused == b)
                SetFocused(null);

            var changed = SetVisible(b, !OnlyShowActive && !IsHidden(b.account.Type));

            if (b.visible)
            {
                if (changed || sorting.Grouping.HasFlag(Settings.GroupMode.Active))
                {
                    SortButtons();
                }
            }
        }

        private AccountButton Create(Settings.IAccount account)
        {
            var b = new AccountButton(account);

            account.NameChanged += account_NameChanged;
            account.ColorKeyChanged += account_ColorKeyChanged;
            account.IconChanged += account_IconChanged;
            account.IconTypeChanged += account_IconTypeChanged;
            account.PinnedChanged += account_PinnedChanged;

            return b;
        }

        private Rectangle GetDockedBounds(Rectangle screen)
        {
            var fh = this.FontHeight - 1;
            var size = fh * 2;

            int x, y, w, h;

            if (docked == Settings.ScreenAnchor.Left || docked == Settings.ScreenAnchor.Right)
            {
                w = ShowAccountName ? size * 5 : size * 3 / 2;
                h = screen.Height;
                y = screen.Y;

                switch (boundsBarType)
                {
                    case Settings.ScreenAnchor.Left:
                    case Settings.ScreenAnchor.Right:

                        w = boundsBar.Width;

                        break;
                }

                if (docked == Settings.ScreenAnchor.Right)
                    x = screen.Right - w;
                else
                    x = screen.X;
            }
            else
            {
                w = screen.Width;
                h = size;
                x = screen.X;

                switch (boundsBarType)
                {
                    case Settings.ScreenAnchor.Top:
                    case Settings.ScreenAnchor.Bottom:

                        h = boundsBar.Height;

                        break;
                }

                if (docked == Settings.ScreenAnchor.Bottom)
                    y = screen.Bottom - h;
                else
                    y = screen.Y;
            }

            return new Rectangle(x, y, w, h);
        }

        private void SetLayoutBounds()
        {
            this.MinimumSize = this.MaximumSize = Size.Empty;

            if (docked != Settings.ScreenAnchor.None)
            {
                this.Bounds = GetDockedBounds(Screen.FromRectangle(boundsBar).WorkingArea);
            }
            else if (boundsBarType == Settings.ScreenAnchor.None)
            {
                if (!boundsBar.IsEmpty)
                    this.Bounds = boundsBar;
            }
            else
            {
                int w, h;
                if (HorizontalLayout)
                {
                    w = Screen.FromPoint(boundsBar.Location).WorkingArea.Width / 2;
                    h = (this.FontHeight - 1) * 2;

                    switch (boundsBarType)
                    {
                        case Settings.ScreenAnchor.Top:
                        case Settings.ScreenAnchor.Bottom:

                            h = boundsBar.Height;

                            break;
                    }
                }
                else
                {
                    w = 200;
                    h = 300;

                    switch (boundsBarType)
                    {
                        case Settings.ScreenAnchor.Left:
                        case Settings.ScreenAnchor.Right:

                            w = boundsBar.Width;

                            break;
                    }
                }
                this.Bounds = new Rectangle(boundsBar.Location, new Size(w, h));
            }

            OnLayoutChanged();
        }

        private void OnLayoutChanged()
        {
            var fh = this.FontHeight - 1;

            int w,
                h,
                barSize = fh * 2 / 3,
                barGap = Scale(5);

            if (LayoutOrientation == Orientation.Horizontal)
            {
                this.MinimumSize = new Size(Scale(100), fh * 3 / 2);
                this.MaximumSize = new Size(ushort.MaxValue, fh * 20);
                w = this.Width;
                h = this.Height;

                barTop.SetBounds(0, 0, barSize, h);
                barTop.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;

                barBottom.SetBounds(w - barSize, 0, barSize, h);
                barBottom.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

                var x = barTop.Right + barGap;
                panelContainer.SetBounds(x, 0, w - x * 2, h);

                panelContent.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
                panelContent.SetBounds(0, 0, panelContainer.Width, panelContainer.Height);
            }
            else
            {
                this.MinimumSize = new Size(fh * 2, Scale(100));
                this.MaximumSize = new Size(ushort.MaxValue, ushort.MaxValue);
                w = this.Width;
                h = this.Height;

                barTop.SetBounds(0, 0, w, barSize);
                barTop.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                barBottom.SetBounds(0, h - barSize, w, barSize);
                barBottom.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

                var y = barTop.Bottom + barGap;
                panelContainer.SetBounds(0, y, w, h - y * 2);

                panelContent.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                panelContent.SetBounds(0, 0, panelContainer.Width, panelContainer.Height);
            }

            foreach (var b in accounts.Values)
            {
                if (b.button != null)
                    b.SetLayout(LayoutOrientation, docked);
            }
        }

        private void SetStyle()
        {
            foreach (var b in accounts.Values)
            {
                if (b.button != null)
                {
                    b.button.TextVisible = _ShowName;
                    b.button.IconVisible = _ShowIcon;
                    b.button.ColorKeyVisible = _ShowColor;
                    b.button.CloseVisible = _ShowClose && b.IsActive;
                }
            }
        }

        private void OnStyleChanged()
        {
            styleChanged = true;
            this.Invalidate();
        }

        private void SortButtons()
        {
            delayedSorting = false;

            buttons.Sort(new AccountButtonComparer(sorting));

            for (int i = 0, count = buttons.Count; i < count; i++)
            {
                buttons[i].index = (short)i;
            }

            OnButtonsChanged();
        }

        private void SortButtonsDelayed()
        {
            if (delayedSorting)
                return;
            delayedSorting = true;

            this.BeginInvoke(new Action(SortButtons));
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (styleChanged)
            {
                styleChanged = false;
                SetStyle();
            }

            if (pendingIndexes)
            {
                ResetIndexes();
            }

            if (isDirty)
            {
                isDirty = false;
                ArrangeButtons();
            }

            base.OnPaintBackground(e);
        }

        private void ArrangeButtons()
        {
            if (pendingIndexes)
            {
                ResetIndexes();
            }

            int fh = (this.FontHeight - 1),
                x = 0,
                y = 0,
                w,
                h = fh * 2,
                count = buttons.Count;

            if (LayoutOrientation == Orientation.Horizontal)
            {
                x = 0;
                if (count > 0)
                {
                    w = panelContainer.Width / count;
                    if (w > fh * 15)
                        w = fh * 15;
                    else if (w < fh * 5)
                        w = fh * 5;

                }
                else
                    w = 0;

                h = this.Height;
            }
            else
            {
                y = 0;
                w = barTop.Width;
            }

            for (int i = 0; i < count; i++)
            {
                var b = buttons[i];

                if (!ShowAccountName)
                    b.button.Bounds = new Rectangle(x, y, w, h);
                else
                    b.button.Bounds = new Rectangle(x, y, w, h);

                if (LayoutOrientation == Orientation.Horizontal)
                    x = b.button.Right + 0;
                else
                    y = b.button.Bottom + 0;
            }

            if (LayoutOrientation == Orientation.Horizontal)
            {
                panelContent.Width = x;

                if (panelContent.Left < 0)
                {
                    if (x > panelContainer.Width)
                        panelContent.Left = panelContainer.Width - x;
                    else
                        panelContent.Left = 0;
                }
            }
            else
            {
                panelContent.Height = y;

                if (panelContent.Top < 0)
                {
                    if (y > panelContainer.Height)
                        panelContent.Top = panelContainer.Height - y;
                    else
                        panelContent.Top = 0;
                }
            }
        }

        protected override void SetVisibleCore(bool value)
        {
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

        public void ShowPopup()
        {
            if (mode != AccountBarMode.QuickLaunch)
                return;

            isDirty = false;
            ArrangeButtons();

            var p = Cursor.Position;
            var screen = Screen.FromPoint(p);
            var bounds = screen.Bounds;
            var wbounds = screen.WorkingArea;
            var h = panelContainer.Top + panelContent.Height + barBottom.Bottom - panelContainer.Bottom;
            var mh = bounds.Height / 2;
            if (h > mh)
                h = mh;

            var w = Scale(250);
            var x = p.X - w * 3 / 4;
            var y = p.Y - h / 2 + Cursor.Size.Height / 4;

            var bh = bounds.Height - wbounds.Height;

            if (bh > 0)
            {
                if (p.Y > bounds.Bottom - bh && bounds.Bottom != wbounds.Bottom)
                {
                    y = bounds.Bottom - bh - h;
                }
                else if (p.Y < bounds.Top + bh && bounds.Top != wbounds.Top)
                {
                    y = bounds.Top + bh;
                }
            }
            else if ((bh = bounds.Width - screen.WorkingArea.Width) > 0)
            {
                if (p.X > bounds.Right - bh && bounds.Right != wbounds.Right)
                {
                    x = bounds.Right - bh;
                }
                else if (p.X < bounds.Left + bh && bounds.Left != wbounds.Left)
                {
                    x = bounds.Left + bh;
                }
            }

            if (x < bounds.Left)
                x = bounds.Left;
            if (x + w > bounds.Right)
                x = bounds.Right - w;
            if (y < bounds.Top)
                y = bounds.Top;
            if (y + h > bounds.Bottom)
                y = bounds.Bottom - h;

            this.MinimumSize = Size.Empty;
            this.Bounds = new Rectangle(x, y, w, h);

            if (!this.Visible)
            {
                this.Visible = true;

                CloseOnMouseLeave();
            }
        }

        private async void CloseOnMouseLeave()
        {
            var r = Rectangle.Inflate(this.Bounds, Scale(10), Scale(10));
            var rp = Rectangle.Inflate(new Rectangle(Cursor.Position, Size.Empty), Scale(20), Scale(20));

            do
            {
                try
                {
                    await Task.Delay(500);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (rp.Width > 0)
                {
                    if (!rp.Contains(Cursor.Position))
                        rp.Width = 0;
                    else
                        continue;
                }
            }
            while (r.Contains(Cursor.Position));

            this.Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Settings.Accounts.ValueChanged -= Accounts_ValueChanged;
                Settings.Accounts.ValueAdded -= Accounts_ValueAdded;
                Settings.Accounts.ValueRemoved -= Accounts_ValueRemoved;

                if (this.mode == AccountBarMode.Default)
                {
                    Settings.WindowBounds[this.GetType()].ValueCleared -= WindowBounds_ValueCleared;

                    Settings.AccountBar.Options.ValueChanged -= AccountBarOptions_ValueChanged;
                }

                Settings.AccountBar.Style.ValueChanged -= AccountBarStyle_ValueChanged;

                Settings.AccountSorting.SortingChanged -= AccountSorting_SortingChanged;

                Client.Launcher.AccountStateChanged -= Launcher_AccountStateChanged;
                Client.Launcher.AccountWindowEvent -= Launcher_AccountWindowEvent;
                Client.Launcher.AccountTopMostWindowEvent -= Launcher_AccountTopMostWindowEvent;
                Client.Launcher.AccountProcessExited -= Launcher_AccountProcessExited;

                foreach (var uid in Settings.Accounts.GetKeys())
                {
                    var a = Settings.Accounts[uid];
                    a.ValueCleared -= Account_ValueCleared;

                    var account = a.Value;
                    if (account != null)
                    {
                        account.NameChanged -= account_NameChanged;
                        account.ColorKeyChanged -= account_ColorKeyChanged;
                        account.IconChanged -= account_IconChanged;
                        account.IconTypeChanged -= account_IconTypeChanged;
                        account.PinnedChanged -= account_PinnedChanged;
                    }
                }

                foreach (var b in accounts.Values)
                {
                    using (b.Icon) { }
                }

                if (imageDefault != null)
                {
                    foreach (var img in imageDefault)
                    {
                        img.Dispose();
                    }
                    imageDefault = null;
                }
            }

            base.Dispose(disposing);
        }

        private int Abs(int x)
        {
            if (x < 0)
                return -x;
            return x;
        }

        private bool OnSizing(Sizing sz, ref RECT r)
        {
            var p = Point.Subtract(Cursor.Position, (Size)sizingOrigin);

            r.left = sizingBounds.left;
            r.top = sizingBounds.top;
            r.right = sizingBounds.right;
            r.bottom = sizingBounds.bottom;

            switch (sz)
            {
                case Sizing.Bottom:
                case Sizing.BottomLeft:
                case Sizing.BottomRight:

                    r.bottom += p.Y;

                    break;
                case Sizing.Top:
                case Sizing.TopLeft:
                case Sizing.TopRight:

                    r.top += p.Y;

                    break;
            }

            switch (sz)
            {
                case Sizing.Right:
                case Sizing.TopRight:
                case Sizing.BottomRight:

                    r.right += p.X;

                    break;
                case Sizing.Left:
                case Sizing.TopLeft:
                case Sizing.BottomLeft:

                    r.left += p.X;

                    break;
            }

            return true;
        }

        private bool OnMoving(ref RECT r)
        {
            int w = r.right - r.left,
                h = r.bottom - r.top;

            var c = Cursor.Position;
            var p = Point.Subtract(c, (Size)sizingOrigin);

            r.left = sizingBounds.left + p.X;
            r.top = sizingBounds.top + p.Y;
            r.right = r.left + w;
            r.bottom = r.top + h;

            if (snapped && (Abs(r.left - snapX) > SNAP_SIZE_RELEASE || Abs(r.top - snapY) > SNAP_SIZE_RELEASE))
                snapped = false;

            if (!snapped)
            {
                var docked = Settings.ScreenAnchor.None;
                Snap snap = null;

                foreach (var s in snaps)
                {
                    int rw = (s.right - s.left) / 2,
                        rh = (s.bottom - s.top) / 2,
                        xmid = rw + s.left,
                        ymid = rh + s.top;

                    if (Abs(xmid - c.X) < rw * 3 / 4
                        && (c.Y >= s.top && c.Y < s.top + SNAP_SIZE || c.Y <= s.bottom && c.Y > s.bottom - SNAP_SIZE))
                    {
                        if (c.Y < ymid)
                        {
                            //top
                            snapped = true;
                            snap = s;
                            docked = Settings.ScreenAnchor.Top;

                            break;
                        }
                        else
                        {
                            //bottom
                            snapped = true;
                            snap = s;
                            docked = Settings.ScreenAnchor.Bottom;

                            break;
                        }
                    }
                    else if (Abs(ymid - c.Y) < rh * 3 / 4
                        && (c.X >= s.left && c.X < s.left + SNAP_SIZE || c.X <= s.right && c.X > s.right - SNAP_SIZE))
                    {
                        if (c.X < xmid)
                        {
                            //left
                            snapped = true;
                            snap = s;
                            docked = Settings.ScreenAnchor.Left;

                            break;
                        }
                        else
                        {
                            //right
                            snapped = true;
                            snap = s;
                            docked = Settings.ScreenAnchor.Right;

                            break;
                        }
                    }
                }

                if (docked != this.docked)
                {
                    this.LayoutDocked = docked;

                    if (docked != Settings.ScreenAnchor.None)
                    {
                        var b = GetDockedBounds(Rectangle.FromLTRB(snap.left, snap.top, snap.right, snap.bottom));

                        r.left = snapX = b.X;
                        r.top = snapY = b.Y;
                        w = b.Width;
                        h = b.Height;

                        var screen = Screen.FromRectangle(new Rectangle(r.left, r.top, w, h));
                        canDockResize = screen.WorkingArea != screen.Bounds;
                    }
                    else
                    {
                        if (boundsBarType != Settings.ScreenAnchor.None)
                        {
                            if (HorizontalLayout)
                            {
                                w = Screen.FromPoint(boundsBar.Location).WorkingArea.Width / 2;
                                h = (this.FontHeight - 1) * 2;

                                switch (boundsBarType)
                                {
                                    case Settings.ScreenAnchor.Top:
                                    case Settings.ScreenAnchor.Bottom:

                                        h = boundsBar.Height;

                                        break;
                                }
                            }
                            else
                            {
                                w = Scale(200);
                                h = Scale(300);

                                switch (boundsBarType)
                                {
                                    case Settings.ScreenAnchor.Left:
                                    case Settings.ScreenAnchor.Right:

                                        w = boundsBar.Width;

                                        break;
                                }
                            }
                        }
                        else
                        {
                            w = boundsBar.Width;
                            h = boundsBar.Height;
                        }

                        r.right = r.left + w;
                        r.bottom = r.top + h;
                    }

                    this.MinimumSize = this.MaximumSize = Size.Empty;
                    SetBounds(r.left, r.top, w, h);
                    OnLayoutChanged();

                    OnButtonsChanged();
                }
            }

            if (snapped)
            {
                r.left = snapX;
                r.right = snapX + w;

                r.top = snapY;
                r.bottom = snapY + h;
            }

            return true;
        }

        private Snap[] GetSnaps()
        {
            var screens = Screen.AllScreens;
            var snaps = new Snap[screens.Length];
            int i = 0;

            foreach (var screen in screens)
            {
                snaps[i++] = new Snap(screen.WorkingArea);
            }

            return snaps;
        }

        protected override void WndProc(ref Message m)
        {
            Point p;
            RECT r;

            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCLBUTTONDBLCLK:

                    break;
                case WindowMessages.WM_NCHITTEST:

                    base.WndProc(ref m);

                    if (m.Result == (IntPtr)HitTest.Client && !isLocked && mode == AccountBarMode.Default)
                    {
                        p = this.PointToClient(new Point(m.LParam.GetValue32()));

                        if (barTop.Bounds.Contains(p))
                        {
                            m.Result = (IntPtr)HitTest.Caption;
                        }
                        else
                        {
                            switch (docked)
                            {
                                case Settings.ScreenAnchor.Bottom:

                                    if (barBottom.Bounds.Contains(p))
                                        m.Result = (IntPtr)HitTest.Top;

                                    break;
                                case Settings.ScreenAnchor.Left:

                                    if (barBottom.Bounds.Contains(p))
                                        m.Result = (IntPtr)HitTest.Right;

                                    break;
                                case Settings.ScreenAnchor.Right:

                                    if (barBottom.Bounds.Contains(p))
                                        m.Result = (IntPtr)HitTest.Left;

                                    break;
                                case Settings.ScreenAnchor.Top:

                                    if (barBottom.Bounds.Contains(p))
                                        m.Result = (IntPtr)HitTest.Bottom;

                                    break;
                                default:

                                    var b = barBottom.Bounds;
                                    if (b.Contains(p))
                                    {
                                        if (_HorizontalLayout)
                                        {
                                            if (p.Y > b.Bottom - 10)
                                                m.Result = (IntPtr)HitTest.BottomRight;
                                            else if (p.Y < b.Top + 10)
                                                m.Result = (IntPtr)HitTest.TopRight;
                                            else
                                                m.Result = (IntPtr)HitTest.Right;
                                        }
                                        else
                                        {
                                            if (p.X > b.Right - 10)
                                                m.Result = (IntPtr)HitTest.BottomRight;
                                            else if (p.X < b.Left + 10)
                                                m.Result = (IntPtr)HitTest.BottomLeft;
                                            else
                                                m.Result = (IntPtr)HitTest.Bottom;
                                        }
                                    }

                                    break;
                            }
                        }
                    }

                    break;
                case WindowMessages.WM_SIZING:

                    base.WndProc(ref m);

                    if (Settings.IsRunningWine && mode == AccountBarMode.Default)
                    {
                        r = (RECT)m.GetLParam(typeof(RECT));
                        if (OnSizing((Sizing)m.WParam.GetValue(), ref r))
                            Marshal.StructureToPtr(r, m.LParam, false);
                    }

                    break;
                case WindowMessages.WM_MOVING:

                    base.WndProc(ref m);

                    if (mode == AccountBarMode.Default)
                    {
                        r = (RECT)m.GetLParam(typeof(RECT));
                        if (OnMoving(ref r))
                            Marshal.StructureToPtr(r, m.LParam, false);
                    }

                    break;
                case WindowMessages.WM_ENTERSIZEMOVE:

                    base.WndProc(ref m);

                    if (mode == AccountBarMode.Default)
                    {
                        snaps = GetSnaps();
                        sizingOrigin = Cursor.Position;
                        sizingBounds = new RECT()
                        {
                            left = this.Left,
                            right = this.Right,
                            top = this.Top,
                            bottom = this.Bottom,
                        };
                        snapped = false;
                    }

                    break;
                case WindowMessages.WM_EXITSIZEMOVE:

                    base.WndProc(ref m);

                    if (mode == AccountBarMode.Default)
                    {
                        switch (docked)
                        {
                            case Settings.ScreenAnchor.Left:
                            case Settings.ScreenAnchor.Right:
                            case Settings.ScreenAnchor.Top:
                            case Settings.ScreenAnchor.Bottom:

                                Settings.WindowBounds[this.GetType()].Value = boundsBar = this.Bounds;
                                boundsBarType = docked;
                                Settings.AccountBar.Docked.Value = docked;

                                break;
                            case Settings.ScreenAnchor.None:
                            default:

                                Settings.WindowBounds[this.GetType()].Value = boundsBar = this.Bounds;
                                boundsBarType = docked;
                                Settings.AccountBar.Docked.Clear();

                                if (orientation == Orientation.Horizontal)
                                    ArrangeButtons();

                                break;
                        }
                    }

                    break;
                default:

                    base.WndProc(ref m);

                    break;
            }
        }
    }
}
