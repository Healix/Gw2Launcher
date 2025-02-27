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
using Gw2Launcher.Api;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Dailies
{
    public partial class formDailies : Base.BaseForm
    {
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

        private class Popup : Base.BaseForm
        {
            private DailyAchievement control;
            private Image defaultImage;

            public Popup(Image defaultImage)
            {
                this.defaultImage = defaultImage;

                InitializeComponents();
            }

            protected override void OnInitializeComponents()
            {
                this.Opacity = 0;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.Manual;
                this.BackColorName = UiColors.Colors.DailiesBackColor;
                this.ForeColorName = UiColors.Colors.DailiesText;

                control = new DailyAchievement()
                {
                    IconSize = new Size(64, 64),
                    IconVisible = true,
                    NameVisible = true,
                    NameFont = new System.Drawing.Font("Segoe UI Semibold", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    DescriptionVisible = true,
                    DescriptionFont = new System.Drawing.Font("Segoe UI Semilight", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    LevelVisible = false,
                    LevelFont = new System.Drawing.Font("Segoe UI Semilight", 8.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    Location = new Point(5, 5),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left,
                    BackColorName = UiColors.Colors.DailiesBackColor,
                    FavEnabled = false,
                    FavSize = new Size(16, 14),
                    FavVisibility = DailyAchievement.FavoriteVisibility.Always,
                };

                this.Controls.Add(control);
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
                    createParams.ExStyle |= (int)(WindowStyle.WS_EX_TRANSPARENT | WindowStyle.WS_EX_LAYERED | WindowStyle.WS_EX_NOACTIVATE);
                    return createParams;
                }
            }

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                if (this.Visible)
                {
                    FadeIn();
                }
                else
                {
                    this.Opacity = 0;
                }

                base.OnVisibleChanged(e);
            }

            private async void FadeIn()
            {
                const int DELAY = 100;
                const int DURATION = 100;

                var active = true;

                EventHandler onVisible = null;
                onVisible = delegate
                {
                    if (!this.Visible)
                    {
                        active = false;
                        this.VisibleChanged -= onVisible;
                    }
                };
                this.VisibleChanged += onVisible;

                await Task.Delay(DELAY);

                var start = DateTime.UtcNow;

                while (active)
                {
                    var p = DateTime.UtcNow.Subtract(start).TotalMilliseconds / DURATION;
                    if (p >= 1)
                    {
                        this.Opacity = 0.95f;
                        this.VisibleChanged -= onVisible;

                        break;
                    }
                    else
                    {
                        this.Opacity = p * 0.95f;
                    }

                    await Task.Delay(10);
                }

            }

            public void SetData(DailyAchievement.IDataSource daily, Daily.Category category)
            {
                control.DataSource = daily;

                if (control.IconValue == null)
                {
                    var icon = defaultImage;
                    if (category != null)
                    {
                        icon = category.GetIcon();
                        if (icon == null)
                            icon = defaultImage;
                    }
                    control.IconValue = icon;
                }

                var size = control.GetPreferredSize(new Size(this.Width - 10, Int32.MaxValue));

                control.Size = size;
                this.Height = size.Height + 10;

                this.Invalidate(true);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                base.OnPaintBackground(e);

                using (var p = new Pen(UiColors.GetColor(UiColors.Colors.DailiesSeparator)))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
                }
            }

            public DailyAchievement Control
            {
                get
                {
                    return control;
                }
            }
        }

        private class MinimizedWindow : Base.BaseForm
        {
            private Form parent;
            private formDailies owner;
            private FlatShapeButton buttonMinimize;
            private HorizontalAlignment alignment;
            private bool visible, wasVisible, wasMinimized, firstShow;

            public MinimizedWindow(formDailies owner, Form parent)
            {
                this.owner = owner;
                this.parent = parent;
                this.Owner = parent;

                InitializeComponents();

                PositionToParent();
            }

            protected override void OnInitializeComponents()
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.BackColorName = UiColors.Colors.DailiesMinimizeBackColor;

                var h = this.Handle; //force

                this.Size = new Size(18, 66);

                this.alignment = owner.alignment;

                buttonMinimize = new FlatShapeButton()
                {
                    ShapeAlignment = ContentAlignment.MiddleCenter,
                    ShapeDirection = alignment == HorizontalAlignment.Left ? ArrowDirection.Left : ArrowDirection.Right,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
                    Location = new Point(1, 1),
                    Size = new Size(this.Width - 2, this.Height - 2),
                    ShapeSize = new Size(4, 8),
                    BackColorHoveredName = UiColors.Colors.DailiesMinimizeBackColorHovered,
                    ForeColorName = UiColors.Colors.DailiesMinimizeArrow,
                    ForeColorHoveredName = UiColors.Colors.DailiesMinimizeArrowHovered,
                };

                this.Controls.Add(buttonMinimize);

                buttonMinimize.Click += buttonMinimize_Click;
                buttonMinimize.MouseHover += buttonMinimize_MouseHover;

                parent.LocationChanged += parent_LocationChanged;
                parent.SizeChanged += parent_SizeChanged;
                parent.VisibleChanged += parent_VisibleChanged;

                owner.VisibleChanged += owner_VisibleChanged;
            }

            void buttonMinimize_MouseHover(object sender, EventArgs e)
            {
                if (!Settings.ShowDailies.Value.HasFlag(Settings.DailiesMode.Positioned))
                    owner.Show(parent.ContainsFocus);
            }

            protected override bool ShowWithoutActivation
            {
                get
                {
                    return true;
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                base.OnPaintBackground(e);

                using (var p = new Pen(UiColors.GetColor(UiColors.Colors.MainBorder)))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
                }
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                }
                base.OnFormClosing(e);
            }

            void buttonMinimize_Click(object sender, EventArgs e)
            {
                owner.Show(true);
                firstShow = true;
            }

            void parent_VisibleChanged(object sender, EventArgs e)
            {
                if (!parent.Visible)
                {
                    if (wasVisible = this.Visible)
                        this.Visible = false;
                }
                else if (wasVisible)
                {
                    if (!firstShow)
                        SlideIn(200, 200);
                    else if (parent.WindowState != FormWindowState.Minimized)
                        this.Visible = true;
                }
            }

            private async void SlideIn(int delay, int duration)
            {
                var active = true;

                this.Opacity = 0;
                this.Visible = true;

                EventHandler onVisible = null;
                onVisible = delegate
                {
                    if (!this.Visible)
                    {
                        active = false;
                        PositionToParent();
                        this.Opacity = 1;
                        this.Owner = parent;
                        this.VisibleChanged -= onVisible;
                    }
                };
                this.VisibleChanged += onVisible;

                await Task.Delay(delay);

                if (!active)
                    return;

                var start = DateTime.UtcNow;
                var offsetX = this.Width * 3 / 2;

                var setX = new Action<int>(
                    delegate(int offset)
                    {
                        if (owner.Left < parent.Left)
                            this.Left = parent.Left - this.Width - owner.padding + offset;
                        else
                            this.Left = parent.Right + owner.padding - offset;
                    });

                this.Owner = null;
                setX(offsetX);

                parent.BringToFront();

                while (active)
                {
                    var p = DateTime.UtcNow.Subtract(start).TotalMilliseconds / duration;
                    if (p >= 1)
                    {
                        setX(0);

                        this.Opacity = 1f;
                        this.Owner = parent;
                        this.VisibleChanged -= onVisible;

                        parent.BringToFront();

                        break;
                    }
                    else
                    {
                        setX((int)(offsetX * (1 - p)));
                        this.Opacity = p;
                    }

                    await Task.Delay(10);
                }
            }

            void parent_SizeChanged(object sender, EventArgs e)
            {
                PositionToParent();
            }

            void parent_LocationChanged(object sender, EventArgs e)
            {
                PositionToParent();

                if (wasMinimized)
                {
                    if (parent.WindowState != FormWindowState.Minimized)
                    {
                        if (!owner.Visible)
                        {
                            SlideIn(200, 200);
                        }
                        wasMinimized = false;
                    }
                }
                else
                {
                    wasMinimized = parent.WindowState == FormWindowState.Minimized;
                }
            }

            void owner_VisibleChanged(object sender, EventArgs e)
            {
                if (owner.Visible)
                {
                    wasVisible = false;
                    if (this.ContainsFocus)
                        owner.Focus();
                    this.Hide();
                }
            }

            public void Show(bool focus)
            {
                if (parent.Visible)
                {
                    this.Show(parent);
                    if (focus)
                        this.Focus();
                }
                else
                {
                    wasVisible = true;
                }
            }

            protected override void SetVisibleCore(bool value)
            {
                base.SetVisibleCore(visible = value);
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                if (this.Visible != visible)
                {
                    SetVisibleCore(visible);

                    if (visible)
                    {
                        this.Opacity = 0;
                    }
                    else
                    {
                        this.Refresh();
                        this.Opacity = 1;
                    }
                }
                else
                    base.OnVisibleChanged(e);
            }

            public void PositionToParent()
            {
                var screen = Screen.FromControl(parent).WorkingArea;
                int x,
                    y = parent.Top + parent.Height / 2 - this.Height / 2;

                if (owner.Left < parent.Left)
                {
                    if (this.alignment != HorizontalAlignment.Left)
                    {
                        this.alignment = HorizontalAlignment.Left;
                        buttonMinimize.ShapeDirection = ArrowDirection.Left;
                    }

                    x = parent.Left - this.Width - owner.padding;
                }
                else
                {
                    if (this.alignment != HorizontalAlignment.Right)
                    {
                        this.alignment = HorizontalAlignment.Right;
                        buttonMinimize.ShapeDirection = ArrowDirection.Right;
                    }

                    x = parent.Right + owner.padding;
                }

                this.Location = new Point(x, y);
            }

            public override void RefreshColors()
            {
                base.RefreshColors();

                this.Invalidate();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    UiColors.ColorsChanged -= OnColorsChanged;
                    parent.LocationChanged -= parent_LocationChanged;
                    parent.SizeChanged -= parent_SizeChanged;
                    parent.VisibleChanged -= parent_VisibleChanged;
                    owner.VisibleChanged -= owner_VisibleChanged;
                }
                base.Dispose(disposing);
            }
        }

        private class ApiRequest : Tools.Api.ApiRequestManager.DataRequest
        {
            public ApiRequest(ApiData.DataType type, Settings.IAccount account, Settings.ApiDataKey key, RequestOptions options = RequestOptions.None)
                : base(type, account, key, options)
            {

            }

            public WatchedAccount Watched
            {
                get;
                set;
            }
        }

        private class DataGroup
        {
            public DataType type;
            public ItemGroup[] groups;
            public ItemGroup favorites;
            public object source;

            public bool Contains(ItemGroup g)
            {
                return groups != null && g != null && g.index < groups.Length && groups[g.index] == g;
            }

            public ItemGroup GetGroupFromIndex(int i)
            {
                if (groups != null && i < groups.Length)
                {
                    return groups[i];
                }

                return null;
            }
        }

        private class ItemGroup
        {
            public ushort id;
            public object source;
            public IData[] items;
            public DailyCategoryBar bar;
            public AccountSquares squares;
            public LastUpdatedLabel updated;
            public DailyAchievement[] controls;
            public Daily.Category category;
            public WatchedAccount watched;
            public int count;
            public bool collapsed;
            public int index;
        }

        private class LastUpdatedLabel : Label
        {
            private byte minutes;

            public DateTime Date
            {
                get;
                set;
            }

            public bool UpdateText()
            {
                var m = (int)(DateTime.UtcNow.Subtract(Date).TotalMinutes + 0.1f);

                if (m < 1)
                {
                    m = 1;
                }
                else if (m > 30)
                {
                    m = 0;
                }

                if (this.minutes != m)
                {
                    this.minutes = (byte)m;

                    if (m == 0)
                    {
                        this.Text = "";
                    }
                    else if (m  == 1)
                    {
                        this.Text = "1 minute ago";
                    }
                    else
                    {
                        this.Text = m + " minutes ago";
                    }
                }

                this.Enabled = m != 0;
                return m != 0;
            }
        }

        private enum DataType : byte
        {
            None = 0,

            Daily = 1,
            DailyToday = 1,
            DailyTomorrow = 1 | 2,

            Vault = 4 | 8 | 16,
            VaultDaily = 4,
            VaultWeekly = 8,
            VaultSpecial = 16,
        }
        private const byte DATA_TYPES = 5;

        private interface IData : DailyAchievement.IDataSource
        {
            bool Favorite
            {
                get;
                set;
            }
        }

        private class ObjectiveDataSource : IData
        {
            protected Image icon;

            public ObjectiveDataSource(Tools.Api.VaultObjectives.ObjectiveData source)
            {
                this.Source = source;
            }

            public Tools.Api.VaultObjectives.ObjectiveData Source
            {
                get;
                set;
            }

            public ushort ID
            {
                get
                {
                    return Source.ID;
                }
            }

            public string Name
            {
                get
                {
                    return Source.Title;
                }
            }

            public string Description
            {
                get 
                {
                    return null;
                }
            }

            public bool IsUnknown
            {
                get 
                {
                    return false;
                }
            }

            public Image Icon
            {
                get
                {
                    return icon;
                }
                set
                {
                    icon = value;
                }
            }

            public Settings.TaggedType Tagged
            {
                get;
                set;
            }

            public bool Favorite
            {
                get
                {
                    return Tagged == Settings.TaggedType.Favorite;
                }
                set
                {
                    if (value)
                    {
                        Tagged = Settings.TaggedType.Favorite;
                    }
                    else if (Tagged == Settings.TaggedType.Favorite)
                    {
                        Tagged = Settings.TaggedType.None;
                    }
                }
            }
        }

        private class AchievementDataSource : IData
        {
            protected Image icon;

            public AchievementDataSource(Daily.Achievement source)
            {
                this.Source = source;
            }

            public Daily.Achievement Source
            {
                get;
                set;
            }

            public ushort ID
            {
                get
                {
                    return Source.ID;
                }
            }

            public string Name
            {
                get
                {
                    return Source.Name;
                }
            }

            public string Description
            {
                get
                {
                    return Source.Requirement;
                }
            }

            public bool IsUnknown
            {
                get
                {
                    return Source.IsUnknown;
                }
            }

            public Image Icon
            {
                get
                {
                    if (icon != null)
                    {
                        return icon;
                    }
                    return Source.GetIcon();
                }
                set
                {
                    icon = value;
                }
            }

            public Settings.TaggedType Tagged
            {
                get;
                set;
            }

            public bool Favorite
            {
                get
                {
                    return Tagged == Settings.TaggedType.Favorite;
                }
                set
                {
                    if (value)
                    {
                        Tagged = Settings.TaggedType.Favorite;
                    }
                    else if (Tagged == Settings.TaggedType.Favorite)
                    {
                        Tagged = Settings.TaggedType.None;
                    }
                }
            }
        }

        private class WatchedAccount
        {
            public WatchedAccount(ushort id, Settings.IGw2Account account)
            {
                this.id = id;
                this.account = account;
                this.api = account.Api;
            }

            public readonly ushort id;
            public readonly Settings.IGw2Account account;
            public readonly Settings.ApiDataKey api;
            public bool watched;
            public float position;
            public DateTime date;
            public ApiRequest request;
            public int group;

            public bool GetGroup(DataGroup data, out ItemGroup g)
            {
                g = data.GetGroupFromIndex(group);

                return g != null && g.watched == this;
            }

            public void Abort()
            {
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        private class TabData
        {
            public DateTime Date
            {
                get;
                set;
            }

            public int DateElapsedInSeconds
            {
                get
                {
                    if (Date == DateTime.MinValue)
                    {
                        return int.MaxValue;
                    }
                    else
                    {
                        return (int)DateTime.UtcNow.Subtract(Date).TotalSeconds;
                    }
                }
            }

            public HashSet<ushort> Collapsed
            {
                get;
                set;
            }

            public Dictionary<ushort, WatchedAccount> Watched
            {
                get;
                set;
            }

            public bool Refresh
            {
                get;
                set;
            }

            public byte Retries
            {
                get;
                set;
            }

            public bool Retrying
            {
                get
                {
                    return RetryAt.Ticks > 0;
                }
                set
                {
                    if (value)
                    {
                        RetryAt = DateTime.UtcNow.AddMinutes(3);
                    }
                    else
                    {
                        RetryAt = DateTime.MinValue;
                    }
                }
            }

            public DateTime RetryAt
            {
                get;
                set;
            }
        }

        private enum ShowOnLoadOptions : byte
        {
            None,
            Always,
            Favorite,
        }

        private enum ActivityType : byte
        {
            None,
            LinkActive,
            ApiActive,
        }

        private DataType isLoading, isRetrying;
        private Daily.Achievements dailies;
        private Tools.Api.VaultObjectives vob;
        private TabData[] tabs;
        private DataType currentTab, loadedTab;
        private Popup popup;
        private Daily da;
        private Image[] imageDefault;
        private Font fontBar, fontName, fontDescription, fontUpdated;
        private Util.ReusableControls reusable;
        private FlatVerticalButton selectedTab;
        private HorizontalAlignment alignment;
        private MinimizedWindow minimized;
        private byte retryCount;
        private bool
            linkedToParent,
            minimizeOnMouseLeave,
            waitingToMinimize,
            loadOnShow,
            sizing,
            wasVisible;
        private ShowOnLoadOptions showOnLoad;
        private int padding;
        private Point sizingOrigin;
        private RECT sizingBounds;
        private DataGroup data;
        private DateTime retryingAt;
        private ushort[] categories;
        private int reposition;

        private Form parent;

        public formDailies(Form parent, Tools.Api.ApiRequestManager apiManager)
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            InitializeComponents();

            reposition = int.MaxValue;

            tabs = new TabData[]
            {
                new TabData(),
                new TabData(),
                new TabData(),
                new TabData(),
            };

            this.parent = parent;
            this.Opacity = 0;
            this.KeyPreview = true;

            panelContent.BackColor = UiColors.GetColor(UiColors.Colors.DailiesSeparator);

            alignment = HorizontalAlignment.Right;

            fontBar = new System.Drawing.Font("Segoe UI Semibold", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fontName = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fontDescription = new System.Drawing.Font("Segoe UI Semilight", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fontUpdated = new System.Drawing.Font("Segoe UI", 6f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            if (parent.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None || !NativeMethods.IsDwmCompositionEnabled())
                padding = 5;

            //padding = (parent.Width - parent.ClientSize.Width) / 2;
            //if (padding > 5)
            //    padding = 0;
            //else
            //    padding = 5;

            imageDefault = new Image[3];
            imageDefault[0] = Properties.Resources.icon42684;
            if (Settings.ShowDailiesCategories.HasValue)
                categories = Settings.ShowDailiesCategories.Value;
            else
                categories = Daily.GetDefaultCategories();

            da = new Daily();
            vob = new Tools.Api.VaultObjectives(apiManager);

            vob.DataChanged += vob_DataChanged;
            vob.AccountDataChanged += vob_AccountDataChanged;

            popup = new Popup(imageDefault[0]);
            popup.Owner = this;

            panelContainer.MouseWheel += panelContainer_MouseWheel;
            panelContainer.MouseHover += panelContainer_MouseHover;
            this.MouseWheel += panelContainer_MouseWheel;
            Settings.ShowDailies.ValueChanged += Settings_ValueChanged;
            Settings.ShowDailiesLanguage.ValueChanged += Language_ValueChanged;
            Settings.ShowDailiesCategories.ValueChanged += Categories_ValueChanged;
            parent.VisibleChanged += parent_VisibleChanged;

            Client.Launcher.MumbleLinkVerified += Launcher_MumbleLinkVerified;
            Client.Launcher.CefSessions.SessionEvent += CefSessions_SessionEvent;
            Client.Launcher.AccountExited += Launcher_AccountExited;

            Settings_ValueChanged(Settings.ShowDailies, EventArgs.Empty);

            if (Settings.ShowDailies.Value.HasFlag(Settings.DailiesMode.AutoLoad))
                SelectTab(DataType.DailyToday);
            else
                loadOnShow = true;
        }
        
        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            buttonToday.MinimumSize = new Size(0, buttonTomorrow.Height);
        }

        void parent_VisibleChanged(object sender, EventArgs e)
        {
            if (linkedToParent)
            {
                if (!parent.Visible)
                {
                    if (wasVisible = this.Visible)
                        this.Visible = false;
                }
                else if (wasVisible)
                    this.Visible = true;
            }
        }

        void Settings_ValueChanged(object sender, EventArgs e)
        {
            var setting = sender as Settings.ISettingValue<Settings.DailiesMode>;
            var options = setting.Value;

            if (!setting.HasValue || (options & Settings.DailiesMode.Show) == 0)
            {
                if (this.IsHandleCreated)
                {
                    this.Dispose();
                    return;
                }
            }

            var bounds = Settings.WindowBounds[typeof(formDailies)];
            var positioned = (options & Settings.DailiesMode.Positioned) != 0;

            if ((options & Settings.DailiesMode.AutoLoadFavorite) != 0)
                showOnLoad = ShowOnLoadOptions.Favorite;
            else if ((options & Settings.DailiesMode.AutoLoad) != 0)
                showOnLoad = ShowOnLoadOptions.Always;
            else
                showOnLoad = ShowOnLoadOptions.None;

            AutoMinimize = !positioned;

            showOnTopToolStripMenuItem.Checked = (options & Settings.DailiesMode.TopMost) != 0;
            TopMost = positioned && (options & Settings.DailiesMode.TopMost) != 0;

            if (positioned)
            {
                if (bounds.HasValue && bounds.Value.X != int.MinValue)
                {
                    this.Bounds = Util.ScreenUtil.Constrain(bounds.Value);
                    LinkedToParent = false;
                }
                else
                    LinkedToParent = true;
            }
            else
            {
                LinkedToParent = true;

                if (bounds.HasValue && bounds.Value.X == int.MinValue)
                    this.Size = bounds.Value.Size;
            }
        }

        void Language_ValueChanged(object sender, EventArgs e)
        {
            RefreshDailies(true);
        }

        void Categories_ValueChanged(object sender, EventArgs e)
        {
            var old = this.categories;
            var categories = this.categories = Settings.ShowDailiesCategories.Value;
            var tab = GetTab(DataType.DailyToday);

            if (!tab.Refresh && dailies != null)
            {
                var b = true;

                foreach (var c in categories)
                {
                    if (dailies.GetCategory(c) == null)
                    {
                        b = false;
                        break;
                    }
                }


                if (b)
                {
                    //can use existing cache

                    if ((loadedTab & DataType.Daily) != 0)
                    {
                        loadedTab = DataType.None;
                    }
                }
                else
                {
                    tab.Refresh = true;
                }
            }

            if ((currentTab & DataType.Daily) != 0)
            {
                RefreshDailies(false, false);
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        void parent_SizeChanged(object sender, EventArgs e)
        {
            PositionToParent();
        }

        void parent_LocationChanged(object sender, EventArgs e)
        {
            PositionToParent();
        }

        private void PositionToParent()
        {
            var screen = Screen.FromControl(parent).WorkingArea;
            int x,
                y = parent.Top + parent.Height / 2 - this.Height / 2;

            if (parent.Right + this.Width <= screen.Right)
            {
                x = parent.Right + padding;

                SetAlignment(HorizontalAlignment.Right);
            }
            else
            {
                x = parent.Left - this.Width - padding;

                if (x < screen.Left)
                {
                    x = screen.Left;
                }

                SetAlignment(HorizontalAlignment.Left);
            }

            if (y < parent.Top)
            {
                if (parent.Top >= screen.Top && y < screen.Top)
                    y = screen.Top;
                else if (parent.Bottom < screen.Bottom && y + this.Height > parent.Bottom && y + this.Height > screen.Bottom)
                    y = screen.Bottom - this.Height;
            }

            this.Location = new Point(x, y);
        }

        void panelContainer_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                scrollV.Value -= Scale(150);
            }
            else if (e.Delta < 0)
            {
                scrollV.Value += Scale(150);
            }

            if (e is HandledMouseEventArgs)
                ((HandledMouseEventArgs)e).Handled = true;

            OnInputReceived();
        }

        void panelContainer_MouseHover(object sender, EventArgs e)
        {
            OnInputReceived();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            OnInputReceived();
        }

        private void OnInputReceived()
        {
            if (minimizeOnMouseLeave && !waitingToMinimize)
            {
                MinimizeOnMouseLeave();
            }
        }

        private void buttonToday_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
            else if (selectedTab != sender)
            {
                SelectTab(DataType.DailyToday);
            }
            else
            {
                SelectTab(DataType.DailyTomorrow);
            }
        }

        private void buttonTomorrow_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
            else if (selectedTab != sender)
            {
                SelectTab(DataType.DailyTomorrow);
            }
            else
            {
                SelectTab(DataType.DailyToday);
            }
        }

        void control_MouseClick(object sender, MouseEventArgs e)
        {
            var control = (DailyAchievement)sender;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (control.DataSource is AchievementDataSource)
                {
                    var d = (AchievementDataSource)control.DataSource;
                    var groups = this.data.groups;
                    var b = !control.FavSelected;

                    popup.Control.FavSelected = b;
                    d.Favorite = b;
                    Settings.FavoriteDailies[d.ID] = b;

                    if (b)
                    {
                        Settings.TaggedDailies[d.ID] = Settings.TaggedType.Favorite;
                    }
                    else
                    {
                        Settings.TaggedDailies.Remove(d.ID);
                    }

                    if (groups != null)
                    {
                        foreach (var g in groups)
                        {
                            for (var i = 0; i < g.count; i++)
                            {
                                var c = g.controls[i];

                                if (c.DataSource.ID == d.ID)
                                {
                                    c.FavSelected = b;
                                    c.DataSource.Favorite = b;
                                }
                            }
                        }
                    }
                }
            }
        }

        void control_MouseLeave(object sender, EventArgs e)
        {
            if (popup.Visible)
                popup.Hide();
        }

        void control_MouseEnter(object sender, EventArgs e)
        {
            var control = (DailyAchievement)sender;

            if (control.DataSource != null && !string.IsNullOrEmpty(control.DataSource.Description))
            {
                popup.Width = control.Width + scrollV.Width + 1;

                popup.SetData(control.DataSource, control.Category);

                var y = this.Top + panelContainer.Top + panelContent.Top + control.Top;
                var x = this.Left + panelContainer.Left - 1;
                if (alignment == HorizontalAlignment.Left)
                    x -= scrollV.Width - 1;
                popup.Location = new Point(x, y + control.Height / 2 - popup.Height / 2);

                if (!popup.Visible)
                {
                    popup.Show(this);
                }
            }

            OnInputReceived();
        }

        private void scrollV_ValueChanged(object sender, int e)
        {
            panelContent.Top = -e;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.F5)
            {
                RefreshDailies(e.Control);

                e.Handled = true;
            }
        }

        private async void RefreshDailies(bool clearCache, bool refresh = true)
        {
            switch (currentTab)
            {
                case DataType.DailyToday:
                case DataType.DailyTomorrow:

                    await da.Reset(clearCache);

                    break;
            }

            if (!this.Visible)
                loadOnShow = true;
            else
                GetData(currentTab, refresh);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            using (var p = new Pen(UiColors.GetColor(UiColors.Colors.MainBorder)))
            {
                e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
            }

        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (linkedToParent && !sizing)
                PositionToParent();

            scrollV.Maximum = panelContent.Height - panelContainer.Height;
        }

        public void OnDailyReset()
        {
            vob.Clear(Vault.VaultType.Daily);

            if ((Settings.ShowDailies.Value & (Settings.DailiesMode.AutoLoad | Settings.DailiesMode.AutoLoadFavorite)) != 0)
            {
                AutoShow = true;
                SelectTab(DataType.DailyToday);
            }
            else if (this.Visible)
            {
                SelectTab(DataType.DailyToday, (currentTab & DataType.Daily) != 0);
            }
            else
            {
                this.LoadOnShow = true;
            }
        }

        public void OnWeeklyReset()
        {
            vob.Clear(Vault.VaultType.Weekly);

        }

        public bool AutoShow
        {
            get;
            set;
        }

        public bool LoadOnShow
        {
            get
            {
                return loadOnShow;
            }
            set
            {
                if (value && this.Visible)
                {
                    SelectTab(DataType.DailyToday);
                }
                else
                {
                    loadOnShow = value;
                }
            }
        }

        private void SelectTab(DataType type, bool select = true)
        {
            FlatVerticalButton button;

            switch (type)
            {
                case DataType.DailyToday:

                    button = buttonToday;

                    panelTabs.SuspendLayout();

                    buttonDaySwap.ShapeDirection = ArrowDirection.Right;
                    buttonToday.Visible = true;
                    buttonTomorrow.Visible = false;

                    panelTabs.ResumeLayout();

                    break;
                case DataType.DailyTomorrow:

                    button = buttonTomorrow;
                    
                    panelTabs.SuspendLayout();

                    buttonDaySwap.ShapeDirection = ArrowDirection.Left;
                    buttonTomorrow.Visible = true;
                    buttonToday.Visible = false;

                    panelTabs.ResumeLayout();

                    break;
                case DataType.VaultDaily:

                    button = buttonVault;

                    break;
                case DataType.VaultWeekly:

                    button = buttonWeekly;

                    break;
                case DataType.VaultSpecial:

                    button = buttonSpecial;

                    break;
                default:
                    button = null;
                    break;
            }

            if (select)
            {
                if (selectedTab != button)
                {
                    if (selectedTab != null)
                        selectedTab.Selected = false;
                    if (button != null)
                        button.Selected = true;
                    selectedTab = button;
                }

                GetData(type);
            }
        }

        private Util.ReusableControls.IResult<DailyAchievement> CreateDailyControls(int count)
        {
            return reusable.Create<DailyAchievement>(count, new Func<DailyAchievement>(
                delegate
                {
                    var control = new DailyAchievement()
                    {
                        BackColorName = UiColors.Colors.DailiesBackColor,
                        NameVisible = true,
                        NameFont = fontName,
                        IconSize = Scale(32, 32),
                        FavSize = Scale(16, 14),
                        FavEnabled = true,
                        FavVisibility = DailyAchievement.FavoriteVisibility.Selected,
                        IconVisible = true,
                        Size = new Size(panelContent.Width, Scale(50)),
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    };

                    control.MouseEnter += control_MouseEnter;
                    control.MouseLeave += control_MouseLeave;
                    control.MouseClick += control_MouseClick;

                    return control;
                }));
        }

        private Util.ReusableControls.IResult<DailyCategoryBar> CreateBarControls(int count)
        {
            return reusable.CreateOrAll<DailyCategoryBar>(count,
                delegate
                {
                    var bar = new DailyCategoryBar()
                    {
                        Font = fontBar,
                        Padding = new Padding(Scale(10), 0, 0, 0),
                        ArrowBarWidth = Scale(50),
                        BackColorName = UiColors.Colors.DailiesHeader,
                        Size = new Size(panelContent.Width, Scale(35)),
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    };

                    bar.Collapsed += bar_Collapsed;
                    bar.Expanded += bar_Expanded;
                    bar.DropDownSelectedItemChanged += bar_DropDownSelectedItemChanged;
                    bar.EyeClicked += bar_EyeClicked;

                    return bar;
                });
        }

        private Util.ReusableControls.IResult<AccountSquares> CreateSquaresControls(int count)
        {
            return reusable.CreateOrAll<AccountSquares>(count,
                delegate
                {
                    var control = new AccountSquares()
                    {
                        BackColorName = UiColors.Colors.DailiesBackColor,
                        Padding = new Padding(0, Scale(2), Scale(2), Scale(2)),
                        Size = new Size(panelContent.Width, Scale(12)),
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    };

                    control.SelectedChanged += squares_SelectedChanged;

                    return control;
                });
        }

        private Util.ReusableControls.IResult<LastUpdatedLabel> CreateLastUpdatedControls(int count)
        {
            return reusable.CreateOrAll<LastUpdatedLabel>(count,
                delegate
                {
                    var control = new LastUpdatedLabel()
                    {
                        AutoSize = false,
                        Enabled = false,
                        Size = new Size(panelContent.Width, Scale(18)),
                        Font = fontUpdated,
                        ForeColor = Util.Color.Gradient(UiColors.GetColor(UiColors.Colors.DailiesTextLight), panelContent.BackColor, 0.5f),
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                        BackColor = UiColors.GetColor(UiColors.Colors.DailiesBackColor),
                    };

                    control.EnabledChanged += updated_EnabledChanged;

                    return control;
                });
        }

        void updated_EnabledChanged(object sender, EventArgs e)
        {
            var g = (ItemGroup)((LastUpdatedLabel)sender).Tag;

            if (g != null)
            {
                PendingReposition(g.index);
            }
        }

        private DataType GetTypeFromVaultKey(ushort key)
        {
            switch (key >> 8)
            {
                case 1:

                    return DataType.VaultDaily;

                case 2:

                    return DataType.VaultWeekly;

                case 3:

                    return DataType.VaultSpecial;

                default:

                    return DataType.None;
            }
        }

        private byte GetIDFromVaultKey(ushort key)
        {
            return (byte)(key & 255);
        }

        private ushort GetVaultKey(DataType type, byte id)
        {
            ushort k;

            switch (type)
            {
                case DataType.VaultDaily:

                    k = 1 << 8;

                    break;
                case DataType.VaultWeekly:

                    k = 2 << 8;

                    break;
                case DataType.VaultSpecial:

                    k = 3 << 8;

                    break;
                default:

                    return id;
            }

            return (ushort)(id | k);
        }

        private async void LoadDefaultIcons()
        {
            var icons = await da.GetIcons(new int[] { 42676, 338457 });

            if (icons[0].Empty || icons[1].Empty)
            {
                var categories = await da.GetCategories(new ushort[] { 3, 13 });

                foreach (var c in categories)
                {
                    var img = c.GetIcon();

                    if (img != null)
                    {
                        switch (c.ID)
                        {
                            case 3:

                                imageDefault[1] = img;

                                break;
                            case 13:

                                imageDefault[2] = img;

                                break;
                        }
                    }
                }
            }
            else
            {
                foreach (var i in icons)
                {
                    var img = i.GetImage();

                    if (img != null)
                    {
                        switch (i.GetID())
                        {
                            case 42676:

                                imageDefault[1] = img;

                                break;
                            case 338457:

                                imageDefault[2] = img;

                                break;
                        }
                    }
                }
            }

            if (this.data != null)
            {
                switch (this.data.type)
                {
                    case DataType.VaultDaily:
                    case DataType.VaultWeekly:
                    case DataType.VaultSpecial:

                        foreach (var g in this.data.groups)
                        {
                            if (g.count > 0)
                            {
                                for (var i = 0; i < g.count; i++)
                                {
                                    if (g.items[i] is ObjectiveDataSource)
                                    {
                                        switch (((ObjectiveDataSource)g.items[i]).Source.Type)
                                        {
                                            case Vault.ObjectiveType.PvP:

                                                g.controls[i].IconValue = imageDefault[1];

                                                break;
                                            case Vault.ObjectiveType.WvW:

                                                g.controls[i].IconValue = imageDefault[2];

                                                break;
                                            default:

                                                continue;
                                        }
                                    }
                                }
                            }
                        }

                        break;
                }
            }
        }

        private void SetupControls(DataType type, object data)
        {
            if (reusable == null)
                reusable = new Util.ReusableControls();
            else
                reusable.ReleaseAll();

            int x = 0,
                y = 0,
                firstIndex = 0,
                lastIndex,
                groupCount,
                itemCount,
                barCount;

            var showIcon = true;
            var gfav = new ItemGroup();
            var tab = GetTab(type);

            switch (type)
            {
                case DataType.DailyToday:
                case DataType.DailyTomorrow:

                    var dailies = (Daily.Achievements)data;

                    groupCount = categories.Length;
                    itemCount = dailies.Count;
                    barCount = dailies.Categories.Length;

                    gfav.collapsed = Settings.HiddenDailyCategories.Contains(0);

                    break;
                case DataType.VaultDaily:
                case DataType.VaultWeekly:
                case DataType.VaultSpecial:

                    var objectives = (Tools.Api.VaultObjectives.ObjectivesGroup[])data;
                    var objs = new Tools.Api.VaultObjectives.ObjectivesGroup[objectives.Length];

                    itemCount = 0;
                    barCount = 0;
                    gfav.collapsed = false;

                    for (var i = 0; i < objectives.Length; i++)
                    {
                        if (objectives[i] != null && objectives[i].Count > 0 && objectives[i].HasAccounts)
                        {
                            objs[barCount] = objectives[i];

                            barCount++;
                            itemCount += objectives[i].Objectives.Length;
                        }
                    }

                    //Array.Sort<Tools.Api.VaultObjectives.ObjectivesGroup>(objs, 0, barCount, Comparer<Tools.Api.VaultObjectives.ObjectivesGroup>.Create(new Comparison<Tools.Api.VaultObjectives.ObjectivesGroup>(
                    //    delegate(Tools.Api.VaultObjectives.ObjectivesGroup a, Tools.Api.VaultObjectives.ObjectivesGroup b)
                    //    {
                    //        return a.ID.CompareTo(b.ID);
                    //    })));

                    groupCount = barCount;
                    data = objs;

                    break;
                default:

                    return;
            }


            var items = CreateDailyControls(itemCount);
            var bars = CreateBarControls(barCount + 1);
            var groups = new ItemGroup[groupCount + 1];
            var dgroup = this.data = new DataGroup()
            {
                type = type,
                groups = groups,
                favorites = gfav,
            };
            var favorites = new List<IData>();

            lastIndex = groups.Length - 1;

            if (gfav.collapsed)
            {
                groups[lastIndex--] = gfav;
            }
            else
            {
                groups[firstIndex++] = gfav;
            }

            if ((type & DataType.Daily) != 0)
            {
                #region Dailies

                var dailies = (Daily.Achievements)data;
                var t = type == DataType.DailyTomorrow ? Daily.Achievements.GroupType.Tomorrow : Daily.Achievements.GroupType.Today;

                foreach (var id in categories)
                {
                    var c = dailies.GetCategory(id);

                    var group = new ItemGroup()
                    {
                        id = id,
                        category = c,
                        collapsed = Settings.HiddenDailyCategories.Contains(id),
                    };

                    Daily.Achievement[] das;

                    if (c != null)
                    {
                        das = dailies.GetAchievements(t, c.Index);
                    }
                    else
                    {
                        das = null;
                    }

                    if (das != null)
                    {
                        group.items = new IData[das.Length];

                        var i = 0;

                        foreach (var a in das)
                        {
                            if (!a.IsUnknown)
                            {
                                Settings.TaggedType tt;
                                if (Settings.TaggedDailies.TryGetValue(a.ID, out tt) && tt == Settings.TaggedType.Ignored)
                                {
                                    continue;
                                }

                                var d = new AchievementDataSource(a)
                                {
                                    Tagged = tt,
                                };

                                if (tt == Settings.TaggedType.Favorite)
                                {
                                    if (a.Icon == null)
                                        d.Icon = c.Icon.GetImage();
                                    favorites.Add(d);
                                }

                                group.items[i++] = d;
                            }
                        }

                        group.count = i;
                    }

                    if (group.count > 0)
                    {
                        var bar = group.bar = bars.GetNext();

                        bar.SetState(group.collapsed);
                        bar.Text = c.Name;
                        bar.ButtonEyeVisible = false;
                        bar.ButtonDropDownArrowVisible = false;
                        bar.DropDownItems = null;
                    }

                    if (group.collapsed)
                        groups[lastIndex--] = group;
                    else
                        groups[firstIndex++] = group;
                }

                #endregion
            }
            else if ((type & DataType.Vault) != 0)
            {
                var squares = CreateSquaresControls(barCount);
                var updated = CreateLastUpdatedControls(barCount);

                #region Vault

                var objectives = (Tools.Api.VaultObjectives.ObjectivesGroup[])data;
                var hasOtherIcons = false;
                var wcount = 0;

                HashSet<ushort> hfav = null;

                for (var i = 0; i < barCount; i++)
                {
                    var group = new ItemGroup()
                    {
                        id = GetVaultKey(type, objectives[i].ID),
                        collapsed = tab.Collapsed != null && tab.Collapsed.Contains(objectives[i].ID),
                    };

                    var objs = objectives[i].Objectives;
                    var c = new Daily.Category()
                    {
                        ID = group.id,
                        Name = group.id.ToString(),
                    };
                    
                    group.items = new IData[objs.Length];
                    group.category = c;
                    group.source = objectives[i];

                    for (var k = 0; k < objs.Length; k++)
                    {
                        if (objs[k].ID == 133)
                        {
                            //skipping login
                        }
                        else
                        {
                            var d = new ObjectiveDataSource(objs[k]);

                            group.items[group.count++] = d;

                            switch (objs[k].Type)
                            {
                                case Vault.ObjectiveType.PvP:

                                    d.Icon = imageDefault[1];
                                    if (d.Icon == null)
                                    {
                                        hasOtherIcons = true;
                                    }

                                    break;
                                case Vault.ObjectiveType.WvW:

                                    d.Icon = imageDefault[2];
                                    if (d.Icon == null)
                                    {
                                        hasOtherIcons = true;
                                    }

                                    break;
                            }
                        }

                        

                    }

                    if (group.count > 0)
                    {
                        var bar = group.bar = bars.GetNext();

                        bar.SetState(group.collapsed);
                        bar.ButtonEyeVisible = true;
                        bar.ButtonEyeTimerEnabled = false;
                        bar.DropDownItems = GetDropDownItems(objectives[i].Accounts);
                        bar.ButtonDropDownArrowVisible = bar.DropDownItems != null && bar.DropDownItems.Length > 1;

                        var watched = false;
                        int index;
                        Settings.IAccount selected;

                        if (objectives[i].HasAccounts)
                        {
                            selected = objectives[i].Accounts[0];
                            index = -1;

                            if (tab.Watched != null)
                            {
                                lock (tab.Watched)
                                {
                                    WatchedAccount wa;

                                    if (tab.Watched.TryGetValue(group.id, out wa))
                                    {
                                        index = GetDropDownItem(bar.DropDownItems, wa.account);

                                        if (index == -1)
                                        {
                                            //index = 0;
                                            tab.Watched.Remove(group.id);
                                        }
                                        else
                                        {
                                            watched = wa.watched;
                                            selected = wa.account;

                                            group.watched = wa;
                                            bar.ButtonEyeTimerEnabled = wa.request != null;
                                        }
                                    }
                                }
                            }

                            if (index == -1)
                            {
                                index = GetDropDownItem(bar.DropDownItems, selected);
                            }

                            bar.Text = selected.Name;
                        }
                        else
                        {
                            selected = null;
                            index = -1;

                            bar.Text = c.Name;
                        }

                        bar.DropDownSelectedIndex = index;

                        //var selected = objectives[i].HasAccounts ? objectives[i].Accounts[0] : null;
                        //var watched = false;

                        //if (tab.Watched != null)
                        //{
                        //    WatchedAccount wa;
                        //    if (tab.Watched.TryGetValue(objectives[i].ID, out wa))
                        //    {
                        //        var index = GetDropDownItem(bar.DropDownItems, wa.account);

                        //        if (index == -1)
                        //        {
                        //            if (selected != null)
                        //            {
                        //                index = 0;
                        //            }
                        //            tab.Watched.Remove(objectives[i].ID);
                        //        }
                        //        else
                        //        {
                        //            watched = wa.watched;
                        //            selected = wa.account;
                        //        }

                        //        bar.DropDownSelectedIndex = index;
                        //    }
                        //}

                        //if (selected != null)
                        //{
                        //    bar.Text = selected.Name;
                        //}
                        //else
                        //{
                        //    bar.Text = c.Name;
                        //}

                        bar.ButtonEyeEnabled = watched;

                        if (watched)
                        {
                            ++wcount;
                        }
                        else
                        {
                            bar.SetApi(null, null);
                        }

                        var square = group.squares = squares.GetNext();

                        square.SetAccounts(objectives[i].Accounts);
                        square.Selected = selected;
                        square.Tag = group;

                        var updatedLabel = group.updated = updated.GetNext();

                        updatedLabel.Font = new System.Drawing.Font("Segoe UI", 7f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        updatedLabel.Height = Scale(18);
                        updatedLabel.TextAlign = ContentAlignment.TopRight;
                        updatedLabel.BackColor = UiColors.GetColor(UiColors.Colors.DailiesBackColor);
                        updatedLabel.ForeColor = Util.Color.Gradient(UiColors.GetColor(UiColors.Colors.DailiesTextLight), panelContent.BackColor, 0.4f);
                        updatedLabel.Enabled = false;
                        updatedLabel.Date = DateTime.MinValue;
                        updatedLabel.Tag = group;

                    }

                    if (group.collapsed)
                        groups[lastIndex--] = group;
                    else
                        groups[firstIndex++] = group;
                }

                if (wcount > 0)
                {
                    Util.ScheduledEvents.Register(OnScheduledRefreshWatched, 5000, Util.ScheduledEvents.RegisterOptions.Async);
                }

                if (hasOtherIcons)
                {
                    if (imageDefault[1] == null)
                    {
                        imageDefault[1] = imageDefault[2] = imageDefault[0];
                        LoadDefaultIcons();
                    }
                }

                #endregion

                squares.HideRemaining();
                squares.AddNew(panelContent);
                updated.HideRemaining();
                updated.AddNew(panelContent);
            }

            var favs = CreateDailyControls(favorites.Count);

            if (favorites.Count > 0)
            {
                var group = gfav;
                var bar = group.bar = bars.GetNext();

                group.category = new Daily.Category()
                {
                    Name = "Favorites",
                };

                group.items = favorites.ToArray();
                group.count = group.items.Length;

                bar.SetState(group.collapsed);
                bar.Text = group.category.Name;
                bar.ButtonEyeVisible = false;
                bar.ButtonDropDownArrowVisible = false;
                bar.DropDownItems = null;
            }

            if (firstIndex != groups.Length)
            {
                var count = groups.Length - firstIndex;
                if (count > 1)
                    Array.Reverse(groups, firstIndex, count);
            }

            firstIndex = 0;

            foreach (var group in groups)
            {
                group.index = firstIndex++;

                if (group.count > 0)
                {
                    var bar = group.bar;
                    var controls = items;

                    if (group.id == 0)
                    {
                        controls = favs;
                    }

                    bar.Tag = group;
                    bar.Location = new Point(x, y);
                    bar.Visible = true;

                    y += bar.Height;

                    var visible = !group.collapsed;
                    var watched = group.bar.ButtonEyeVisible && group.bar.ButtonEyeEnabled;

                    group.controls = new DailyAchievement[group.count];

                    if (group.squares != null)
                    {
                        var b = visible && group.squares.Count > 1;

                        group.squares.Location = new Point(x, y);
                        group.squares.Visible = b;

                        if (b)
                            y += group.squares.Height;
                    }

                    if (group.watched != null)
                    {
                        group.watched.group = group.index;
                    }

                    for (var i = 0; i < group.count; i++)
                    {
                        var d = group.items[i];

                        if (d.IsUnknown)
                        {
                            Util.Logging.Log("Unknown data group");
                        }

                        var control = group.controls[i] = controls.GetNext();

                        control.DataSource = d;
                        control.Category = group.category;
                        control.FavSelected = d.Favorite;
                        control.IconVisible = showIcon;
                        control.ProgressVisible = watched;
                        control.ProgressDisplayedVisible = watched;

                        if (showIcon && control.IconValue == null)
                        {
                            var icon = imageDefault[0];
                            if (group.category != null)
                            {
                                icon = group.category.GetIcon();
                                if (icon == null)
                                    icon = imageDefault[0];
                            }
                            control.IconValue = icon;
                        }

                        control.Location = new Point(x, y);
                        control.Visible = visible;

                        if (visible)
                            y += control.Height;
                    }

                    if (watched)
                    {
                        //if (!UpdateProgress(group))
                        //{
                        //    if (group.watched != null)
                        //    {
                        //        group.watched.Abort();
                        //    }
                        //}

                        OnWatchedChanged(bar);
                    }

                    if (group.updated != null)
                    {
                        var b = visible && group.updated.Enabled;

                        group.updated.Location = new Point(x, y);
                        group.updated.Visible = b;

                        if (b)
                            y += group.updated.Height;
                    }

                }
            }

            bars.HideRemaining();
            items.HideRemaining();
            favs.HideRemaining();

            reusable.HideRemaining();

            panelContent.Height = y;
            scrollV.Maximum = y - panelContainer.Height;

            bars.AddNew(panelContent);
            items.AddNew(panelContent);
            favs.AddNew(panelContent);

            reposition = int.MaxValue;
        }

        private bool UpdateProgress(ItemGroup group)
        {
            if (group.bar.ButtonEyeVisible && group.bar.ButtonEyeEnabled && group.squares != null)
            {
                var selected = group.squares.Selected;
                var og = group.source as Tools.Api.VaultObjectives.ObjectivesGroup;
                
                if (og != null && selected != null)
                {
                    var ao = vob.GetObjectives(selected);

                    if (ao != null)
                    {
                        var remaining = group.count;

                        for (var i = 0; i < group.count; i++)
                        {
                            var c = group.controls[i];

                            c.ProgressVisible = true;
                            c.ProgressDisplayedVisible = true;
                            c.ProgressDisplayedTotal = ((ObjectiveDataSource)group.items[i]).Source.ProgressComplete;

                            var o = ao.GetObjective(og.Type, group.items[i].ID, i);

                            if (o != null)
                            {
                                c.ProgressDisplayedValue = o.ProgressCurrent;

                                if (o.Claimed)
                                {
                                    c.ProgressClaimed = true;
                                    --remaining;
                                }
                                else if (o.ProgressCurrent == 0)
                                {
                                    c.ProgressValue = 0;
                                }
                                else
                                {
                                    var max = ((ObjectiveDataSource)group.items[i]).Source.ProgressComplete;

                                    if (max != 0)
                                    {
                                        if (o.ProgressCurrent == max)
                                        {
                                            c.ProgressValue = 1;
                                        }
                                        else
                                        {
                                            c.ProgressValue = (float)o.ProgressCurrent / max;
                                        }
                                    }
                                    else
                                    {
                                        c.ProgressValue = 0;
                                    }
                                }
                            }
                            else
                            {
                                c.ProgressValue = 0;
                                c.ProgressDisplayedValue = 0;
                            }
                        }

                        if (remaining > 0)
                        {
                            group.updated.Date = ao.GetDate(og.Type);
                            group.updated.UpdateText();
                        }
                        else if (group.updated.Date != DateTime.MinValue)
                        {
                            group.updated.Date = DateTime.MinValue;
                            group.updated.UpdateText();
                        }

                        return remaining > 0;
                    }
                }
            }

            if (group.updated.Date != DateTime.MinValue)
            {
                group.updated.Date = DateTime.MinValue;
                group.updated.UpdateText();
            }

            for (var i = 0; i < group.count; i++)
            {
                group.controls[i].ProgressVisible = false;
            }

            return false;
        }

        private int GetDropDownItem(object[] items, Settings.IAccount account, int i = -1)
        {
            if (account != null && items != null)
            {
                var _items = (Util.ComboItem<Settings.IAccount>[])items;

                if (i != -1 && i < _items.Length)
                {
                    if (_items[i].Value == account)
                    {
                        return i;
                    }
                    else if (_items.Length == 1)
                    {
                        return -1;
                    }
                }

                for (i = 0; i < _items.Length; i++)
                {
                    if (_items[i].Value == account)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private Util.ComboItem<Settings.IAccount>[] GetDropDownItems(Settings.IAccount[] accounts)
        {
            var count = accounts.Length;
            
            while (count > 0 && accounts[count - 1] == null)
            {
                --count;
            }

            if (count == 0)
            {
                return null;
            }

            var items = new Util.ComboItem<Settings.IAccount>[count];

            for (var i = 0; i < count; i++)
            {
                items[i] = new Util.ComboItem<Settings.IAccount>(accounts[i], accounts[i].Name);
            }

            if (count > 1)
            {
                Array.Sort<Util.ComboItem<Settings.IAccount>>(items, new Comparison<Util.ComboItem<Settings.IAccount>>(
                    delegate(Util.ComboItem<Settings.IAccount> a, Util.ComboItem<Settings.IAccount> b)
                    {
                        var c = a.Value.Name.CompareTo(b.Value.Name);
                        if (c == 0)
                            c = a.Value.UID.CompareTo(b.Value.UID);
                        return c;
                    }));
            }

            return items;
        }

        private void PositionGroups(int startAt)
        {
            var groups = this.data.groups;
            int y = 0;

            if (startAt != 0)
            {
                for (var i = startAt - 1; i >= 0; i--)
                {
                    var g = groups[i];
                    if (g.count == 0)
                        continue;

                    if (g.collapsed)
                    {
                        y = g.bar.Bottom;
                    }
                    else if (g.updated != null && g.updated.Enabled)
                    {
                        y = g.updated.Bottom;
                    }
                    else
                    {
                        y = g.controls[g.count - 1].Bottom;
                    }

                    break;
                }
            }

            for (int i = startAt, l = groups.Length; i < l; i++)
            {
                var g = groups[i];
                if (g.count == 0)
                    continue;
                var visible = !g.collapsed;

                g.bar.Top = y;

                y += g.bar.Height;

                if (g.squares != null)
                {
                    var b = visible && g.squares.Count > 1;

                    if (b)
                    {
                        g.squares.Top = y;
                        y += g.squares.Height;
                    }

                    g.squares.Visible = b;
                }

                for (int gi = 0; gi < g.count; gi++)
                {
                    var c = g.controls[gi];

                    if (visible)
                    {
                        c.Top = y;
                        y += c.Height;
                    }

                    c.Visible = visible;
                }

                if (g.updated != null)
                {
                    var b = visible && g.updated.Enabled;

                    if (b)
                    {
                        g.updated.Top = y;
                        y += g.updated.Height;
                    }

                    g.updated.Visible = b;
                }

            }

            panelContent.Height = y;
            scrollV.Maximum = y - panelContainer.Height;
        }

        private void OnCollapsedChanged(ItemGroup group, bool collapsed)
        {
            Settings.HiddenDailyCategories[group.id] = collapsed;

            group.collapsed = collapsed;
            PositionGroups(group.index);
        }

        void bar_Expanded(object sender, EventArgs e)
        {
            OnCollapsedChanged((ItemGroup)((DailyCategoryBar)sender).Tag, false);
        }

        void bar_Collapsed(object sender, EventArgs e)
        {
            OnCollapsedChanged((ItemGroup)((DailyCategoryBar)sender).Tag, true);
        }

        private void OnWatchedChanged(DailyCategoryBar bar)
        {
            var g = (ItemGroup)bar.Tag;

            if (g.squares == null)
                return;

            var selected = (Settings.IGw2Account)g.squares.Selected;
            //var selected = (Util.ComboItem<Settings.IAccount>)bar.DropDownSelectedItem;

            if (selected != null && this.data.Contains(g))
            {
                var tab = GetTab(this.data.type);

                if (tab.Watched == null)
                {
                    tab.Watched = new Dictionary<ushort, WatchedAccount>();
                }

                var watched = bar.ButtonEyeVisible && bar.ButtonEyeEnabled;
                var wa = g.watched;
                ApiRequest r;

                lock (tab.Watched)
                {
                    if (wa != null)
                    {
                        r = wa.request;

                        if (wa.account == selected)
                        {
                            wa.watched = watched;

                            if (watched)
                            {
                                r = null;
                            }
                        }
                        else
                        {
                            wa = null;
                        }
                    }
                    else
                    {
                        r = null;
                    }

                    if (wa == null)
                    {
                        tab.Watched[g.id] = wa = new WatchedAccount(g.id, selected)
                        {
                            watched = watched,
                            group = g.index,
                        };

                        g.watched = wa;
                    }
                }

                if (r != null)
                {
                    r.Abort();
                }

                if (watched)
                {
                    if (wa.request == null)
                    {
                        var ao = vob.GetObjectives(selected);
                        var vt = GetVaultType(this.data.type);

                        if (ao != null && !ao.IsComplete(vt) && (ao.IsPending(vt) || Client.Launcher.IsActive(selected)))
                        {
                            Util.Logging.LogEvent(wa.account, "[OnWatchedChanged] Queueing request for [" + this.data.type + "]");

                            QueueApiRequest(this.data.type, wa, false);
                        }
                    }

                    bar.SetApi(selected, vob.ApiManager.DataSource, GetApiType(this.data.type));
                    
                    //Tools.Mumble.MumbleData.PositionData pd;
                    //Client.Launcher.GetMumbleLink(null).Subscribe(Tools.Mumble.MumbleMonitor.DataScope.Basic)

                    //vob.Refresh(GetVaultType(this.data.type), (Settings.IGw2Account)selected, Tools.Api.VaultObjectives.RefreshOptions.Update);
                }
                else
                {
                    bar.SetApi(null, null);
                }

                bar.ButtonEyeTimerEnabled = wa.request != null;
                UpdateProgress(g);
            }
        }

        private bool QueueApiRequest(DataType t, WatchedAccount w, bool nocache)
        {
            var api = w.account.Api;

            if (api == null)
            {
                return false;
            }

            var r = new ApiRequest(GetApiType(t), w.account, api, nocache ? ApiData.DataRequest.RequestOptions.NoCache : ApiData.DataRequest.RequestOptions.None)
            {
                Watched = w,
            };

            r.Complete += OnWatchedRequestComplete;
            r.DataAvailable += OnWatchedRequestDataAvailable;

            w.request = r;

            vob.ApiManager.Queue(r);

            return true;
        }

        void OnWatchedRequestDataAvailable(object sender, ApiData.RequestDataAvailableEventArgs e)
        {
            if (e.Status == ApiData.DataStatus.Error)
                return;

            var r = (ApiRequest)sender;
            var t = GetType(e.Type);

            if (currentTab == t)
            {
                var wa = r.Watched;
                ItemGroup g;

                if (wa.watched && wa.GetGroup(this.data, out g) && Client.Launcher.IsActive(r.Account))
                {
                    if (DateTime.UtcNow.Subtract(wa.date).TotalMinutes < 3)
                    {
                        e.Repeat = true;

                        Util.Logging.LogEvent(r.Account, "[OnWatchedRequestDataAvailable] Repeating request for [" + r.Type + "]");
                    }
                }
            }
        }

        void OnWatchedRequestComplete(object sender, EventArgs e)
        {
            var r = (ApiRequest)sender;
            var wa = r.Watched;

            if (wa.request == r)
            {
                wa.request = null;

                var t = GetType(r.Type);

                if (currentTab == t)
                {
                    Util.Invoke.Async(this, delegate
                    {
                        ItemGroup g;

                        if (currentTab == t && wa.watched && wa.request == null && wa.GetGroup(this.data, out g))
                        {
                            g.bar.ButtonEyeTimerEnabled = GetPending(wa.account.Api, r.Type) != 0;
                        }
                    });
                }
            }
        }

        private int GetPending(Settings.ApiDataKey api, ApiData.DataType t)
        {
            if (api != null)
            {
                var c = vob.ApiManager.DataSource.GetCache(api.Key);

                if (c != null)
                {
                    return c.GetPending(t);
                }
            }

            return 0;
        }

        void bar_EyeClicked(object sender, EventArgs e)
        {
            var bar = (DailyCategoryBar)sender;

            OnWatchedChanged(bar);
            DoPendingReposition();

            if (bar.ButtonEyeVisible && bar.ButtonEyeEnabled)
            {
                Util.ScheduledEvents.Register(OnScheduledRefreshWatched, 5000);
            }
        }

        void bar_DropDownSelectedItemChanged(object sender, EventArgs e)
        {
            var bar = (DailyCategoryBar)sender;
            var selected = (Util.ComboItem<Settings.IAccount>)bar.DropDownSelectedItem;
            var g = (ItemGroup)bar.Tag;

            if (selected != null)
            {
                bar.Text = selected.Value.Name;

                if (g.squares != null)
                {
                    g.squares.Selected = selected.Value;
                }

                OnWatchedChanged(bar);
                DoPendingReposition();
            }
        }

        void squares_SelectedChanged(object sender, EventArgs e)
        {
            var squares = (AccountSquares)sender;

            if (squares.Selected != null)
            {
                var g = (ItemGroup)squares.Tag;
                var i = GetDropDownItem(g.bar.DropDownItems, squares.Selected);

                if (i != -1)
                {
                    g.bar.DropDownSelectedIndex = i;
                    g.bar.Text = squares.Selected.Name;

                    OnWatchedChanged(g.bar);
                    DoPendingReposition();
                }
            }
        }

        private TabData GetTab(DataType t)
        {
            switch (t)
            {
                case DataType.DailyToday:
                case DataType.DailyTomorrow:

                    return tabs[0];

                case DataType.VaultDaily:

                    return tabs[1];

                case DataType.VaultWeekly:

                    return tabs[2];

                case DataType.VaultSpecial:

                    return tabs[3];
            }

            return null;
        }

        private ItemGroup GetGroup(ushort id)
        {
            var data = this.data;

            if (data != null)
            {
                var groups = data.groups;

                for (var i = 0; i < groups.Length; i++)
                {
                    if (groups[i].id == id)
                    {
                        return groups[i];
                    }
                }
            }

            return null;
        }

        private async void GetData(DataType type, bool refresh = false)
        {
            var tab = GetTab(type);
            if (tab == null)
                return;
            currentTab = type;
            if (IsDisposed)
                return;
            if ((isLoading & type) != 0)
            {
                OnTabLoading();
                return;
            }
            isLoading |= type;

            var sliderValue = scrollV.Value;
            var display = type;
            var changed = false;
            var error = false;
            object o = null;

            if ((type & DataType.Daily) != 0)
            {
                #region Dailies

                var current = this.dailies;
                o = current;

                if (tab.Refresh)
                {
                    tab.Refresh = false;
                    refresh = true;
                }

                if (categories != null && categories.Length == 0)
                {
                    refresh = false;
                }
                else if (!refresh)
                {
                    if (current == null || tab.Retrying && DateTime.UtcNow >= tab.RetryAt)
                    {
                        refresh = true;
                    }
                    else
                    {
                        var age = current.Age;

                        if (age != 0)
                        {
                            if (type == DataType.DailyToday && age == 1 && current.Tomorrow != null)
                            {
                                //can use yesterday's tomorrow for today
                                display = DataType.DailyTomorrow;
                                changed = true;
                            }
                            else
                            {
                                refresh = true;
                            }
                        }
                        else if (!current.Verified && DateTime.UtcNow.Subtract(current.Date.Date).TotalHours >= 1)
                        {
                            refresh = true;
                        }
                        else if (type == DataType.DailyTomorrow && current.Tomorrow == null)
                        {
                            if (DateTime.UtcNow.Subtract(current.Date).TotalMinutes > 1)
                            {
                                refresh = true;
                            }
                            else
                            {
                                o = null;
                            }
                        }
                    }
                }

                if (refresh)
                {
                    try
                    {
                        tab.Retrying = false;

                        OnTabLoading();

                        //if (isRetrying != DailyType.None)
                        //{
                        //    isRetrying = DailyType.None;
                        //    labelRetry.Visible = false;

                        //    Util.ScheduledEvents.Unregister(OnScheduledRetry);
                        //}

                        o = await da.GetDailies(categories);

                        this.dailies = (Daily.Achievements)o;
                        loadOnShow = false;
                        changed = true;
                    }
                    catch (Exception e)
                    {
                        if (e is Daily.DailyNotModifiedException)
                        {
                            if (current != null)
                            {
                                if (current.Date < DateTime.UtcNow.Date || type == DataType.DailyTomorrow && current.Tomorrow == null)
                                {
                                    o = null;
                                }
                            }
                        }
                        else
                        {
                            Util.Logging.Log(e);
                            o = null;
                            error = true;
                        }
                    }

                    if (IsDisposed)
                    {
                        return;
                    }
                }

                #endregion
            }
            else if (type == DataType.VaultDaily || type == DataType.VaultWeekly || type == DataType.VaultSpecial)
            {
                #region Vault

                var vt = GetVaultType(type);
                Tools.Api.VaultObjectives.RefreshStatus rs = null;

                if (changed = tab.Refresh)
                {
                    tab.Refresh = false;
                }
                if (tab.DateElapsedInSeconds > 60)
                {
                    rs = vob.Refresh(vt, tab.Date == DateTime.MinValue);
                    tab.Date = DateTime.UtcNow;
                }
                var objectives = vob.GetObjectives(vt);

                if (rs != null)
                {
                    var hasData = false;

                    if (objectives != null)
                    {
                        for (var i = 0; i < objectives.Length; i++)
                        {
                            if (objectives[i].Summary != 0)
                            {
                                hasData = true;

                                break;
                            }
                        }
                    }

                    if (!hasData)
                    {
                        OnTabLoading();

                        var d = DateTime.UtcNow.AddSeconds(10);

                        do
                        {
                            await Task.Delay(500);
                        }
                        while (!rs.IsComplete && d > DateTime.UtcNow);

                        if (tab.Refresh)
                        {
                            changed = true;
                            tab.Refresh = false;
                        }

                        objectives = vob.GetObjectives(vt);
                    }
                }

                o = objectives;
                
                #endregion
            }

            isLoading &= ~type;

            if ((type & currentTab) != 0)
            {
                waitingBounce.Visible = false;

                if (type != currentTab)
                {
                    //displayed tab changed while loading
                    display = currentTab;
                }

                if (type == DataType.DailyToday || type == DataType.DailyTomorrow)
                {
                    #region Dailies

                    if (categories != null && categories.Length == 0)
                    {
                        OnTabError("No categories selected");
                    }
                    else
                    {
                        var dailies = (Daily.Achievements)o;
                        var hasData = dailies != null && dailies.GetGroup(display == DataType.DailyTomorrow ? Daily.Achievements.GroupType.Tomorrow : Daily.Achievements.GroupType.Today) != null;

                        if (refresh)
                        {
                            if (hasData)
                            {
                                tab.Retries = 0;
                                retryCount = 0;

                                if (!dailies.Verified)
                                {
                                    var date = DateTime.UtcNow;
                                    var minutes = (int)date.Subtract(date.Date).TotalMinutes;

                                    if (minutes < 10)
                                    {
                                        minutes = 5;
                                    }
                                    else
                                    {
                                        minutes = 61 - minutes;
                                    }

                                    Util.Logging.LogEvent("[daily] OnScheduledRefresh in " + minutes);

                                    if (minutes > 0)
                                    {
                                        Util.ScheduledEvents.Register(OnScheduledDailiesRefresh, date.AddMinutes(minutes));
                                    }
                                }
                            }
                            else
                            {
                                var date = DateTime.UtcNow;
                                var minutes = date.Subtract(date.Date).TotalMinutes;

                                if (tab.Retries < 2 || minutes < 15)
                                {
                                    ++tab.Retries;

                                    isRetrying = currentTab;
                                    retryCount++;

                                    var delay = 60;

                                    if (minutes < 10)
                                        delay *= 3;

                                    retryingAt = date.AddSeconds(delay);
                                    tab.RetryAt = date.AddSeconds(delay);

                                    labelRetry.Text = "";

                                    //Util.ScheduledEvents.Register(OnScheduledRetry, 1000);
                                }
                                else
                                {
                                    tab.Retrying = false;
                                    tab.Retries = 0;

                                    isRetrying = DataType.None;
                                    retryCount = 0;
                                }
                            }
                        }

                        if (hasData && dailies.Count > 0)
                        {
                            OnTabLoaded(display, changed, sliderValue, dailies);
                        }
                        else
                        {
                            OnTabError(error ? "Unable to retrieve dailies" : "Unavailable");
                        }

                        var retrying = tab.Retrying && !(hasData && dailies.Count > 0);

                        if (retrying)
                        {
                            labelRetry.Text = "";
                            Util.ScheduledEvents.Register(OnScheduledRetry, 1000);
                        }

                        labelRetry.Visible = retrying;

                        //labelRetry.Visible = isRetrying == currentTab;
                        //labelRetry.Visible = tab.Retrying && !(hasData && dailies.Count > 0);
                    }

                    #endregion
                }
                else if (type == DataType.VaultDaily || type == DataType.VaultWeekly || type == DataType.VaultSpecial)
                {
                    #region Vault

                    var objectives = (Tools.Api.VaultObjectives.ObjectivesGroup[])o;
                    var hasData = false;

                    if (objectives != null && objectives.Length > 0)
                    {
                        for (var i = 0; i < objectives.Length; i++)
                        {
                            if (objectives[i] != null && objectives[i].Count > 0 && objectives[i].HasAccounts)
                            {
                                hasData = true;
                                break;
                            }
                        }
                    }

                    if (hasData)
                    {
                        OnTabLoaded(type, changed, sliderValue, objectives);
                    }
                    else
                    {
                        OnTabError("Unavailable");
                    }

                    #endregion
                }
            }
        }

        private void OnTabLoading()
        {
            panelContent.Visible = false;
            panelMessage.Visible = false;
            scrollV.Maximum = 0;
            labelRetry.Visible = false;
            waitingBounce.Visible = true;
        }

        private void OnTabError(string message)
        {
            labelMessage.Text = message;

            //labelMessage.MaximumSize = new Size(panelContainer.Width * 3 / 4, panelContainer.Height);
            //labelMessage.Location = new Point(panelContainer.Width / 2 - labelMessage.Width / 2, panelContainer.Height / 2 - labelMessage.Height / 2);
            //labelMessage.Visible = true;

            scrollV.Maximum = 0;
            labelRetry.Visible = false;
            panelMessage.Visible = true;
            panelContent.Visible = false;

            if (!this.Visible)
                loadOnShow = true;
        }

        private void OnTabLoaded(DataType display, bool changed, int sliderValue, object data)
        {
            if (changed || loadedTab != display)
            {
                if ((loadedTab & display) == 0)
                {
                    sliderValue = 0;
                }

                loadedTab = display;

                if (popup.Visible)
                    popup.Hide();

                SetupControls(display, data);

                if (changed)
                {
                    sliderValue = 0;

                    if (this.AutoShow && showOnLoad != ShowOnLoadOptions.None)
                    {
                        this.AutoShow = false;

                        var b = false;

                        if (showOnLoad == ShowOnLoadOptions.Favorite)
                        {
                            var gfav = this.data.favorites;

                            if (gfav != null && gfav.count > 0)
                            {
                                b = true;
                            }
                        }
                        else if (showOnLoad == ShowOnLoadOptions.Always)
                        {
                            b = true;
                        }

                        if (b)
                        {
                            if (this.Visible)
                            {
                                if (!TopMost && !this.ContainsFocus)
                                {
                                    Windows.FindWindow.ForceWindowToFront(this);
                                }
                            }
                            else
                            {
                                Show(false);
                            }
                        }
                    }
                }
            }
            else
            {
                scrollV.Maximum = panelContent.Height - panelContainer.Height;
            }

            scrollV.Value = sliderValue;

            panelMessage.Visible = false;
            panelContent.Visible = true;
        }

        private Util.ScheduledEvents.Ticks OnScheduledDailiesRefresh()
        {
            if ((currentTab & DataType.Daily) != 0)
            {
                if (dailies != null && (!dailies.Verified || dailies.Tomorrow == null))
                {
                    RefreshDailies(false, true);
                }
            }

            return Util.ScheduledEvents.Ticks.None;
        }

        private Util.ScheduledEvents.Ticks OnScheduledRetry()
        {
            var tab = GetTab(currentTab);

            if (tab != null && tab.Retrying)
            {
                var ticks = DateTime.UtcNow.Ticks;
                var ms = (tab.RetryAt.Ticks - ticks) / 10000;

                if (ms > 1000)
                {
                    labelRetry.Text = "retrying in " + ms / 1000;

                    return new Util.ScheduledEvents.Ticks(Util.ScheduledEvents.TickType.MillisecondTicks, ticks / 10000 + ms % 1000 + 500);
                }
                else
                {
                    labelRetry.Text = "";
                    GetData(currentTab);
                }
            }

            return Util.ScheduledEvents.Ticks.None;
        }

        private Util.ScheduledEvents.Ticks OnScheduledVaultSpecial()
        {
            if (this.Visible)
            {
                MonitorVaultSpecial(true);
            }

            return Util.ScheduledEvents.Ticks.None;
        }

        private Util.ScheduledEvents.Ticks OnScheduledVaultSpecialRefresh()
        {
            if (_MonitorVaultSpecial && Client.Launcher.GetActiveGameProcessCount(Client.Launcher.AccountType.GuildWars2) > 0)
            {
                Task.Run(new Action(QueueScheduledVaultSpecialRefresh));

                return new Util.ScheduledEvents.Ticks(DateTime.UtcNow.AddMinutes(10));
            }

            return Util.ScheduledEvents.Ticks.None;
        }

        private async Task<ActivityType> FindActive(Settings.IGw2Account gw2)
        {
            var api = gw2.Api;

            if (api != null && (api.Permissions & TokenInfo.Permissions.Progression) != 0)
            {
                var cache = vob.ApiManager.DataSource.GetCache(api.Key);
                var b = true;

                if (cache != null)
                {
                    if (DateTime.UtcNow.Subtract(cache.LastModifiedLocal).TotalMinutes < 5)
                    {
                        return ActivityType.ApiActive;
                    }
                    else
                    {
                        b = DateTime.UtcNow > cache.NextRequest;
                    }
                }

                if (b)
                {
                    var m = Client.Launcher.GetMumbleLink(gw2);

                    if (m != null && m.IsValid)
                    {
                        try
                        {
                            using (var s = m.Subscribe(Tools.Mumble.MumbleMonitor.DataScope.Basic))
                            {
                                if (await s.Refresh(1000))
                                {
                                    return ActivityType.LinkActive;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                }
            }

            return ActivityType.None;
        }

        private async void QueueScheduledVaultSpecialRefresh()
        {
            ActivityType t = ActivityType.None;
            Settings.IGw2Account taccount = null;

            var w = NativeMethods.GetForegroundWindow();
            Settings.IAccount focused = null;

            if (w != IntPtr.Zero)
            {
                uint pid;
                NativeMethods.GetWindowThreadProcessId(w, out pid);
                if (pid != 0)
                {
                    var a = Client.Launcher.GetAccountFromProcessId((int)pid);

                    if (a != null && a.Type == Settings.AccountType.GuildWars2)
                    {
                        focused = a;
                        taccount = (Settings.IGw2Account)a;
                        t = await FindActive(taccount);
                    }
                }
            }

            if (t == ActivityType.None)
            {
                var accounts = Client.Launcher.GetActiveProcessesWithState(Client.Launcher.AccountState.ActiveGame);

                foreach (var a in accounts)
                {
                    if (a.Type == Settings.AccountType.GuildWars2)
                    {
                        if (a == focused)
                            continue;

                        taccount = (Settings.IGw2Account)a;
                        t = await FindActive(taccount);

                        if (t != ActivityType.None || !_MonitorVaultSpecial)
                        {
                            break;
                        }
                    }
                }
            }

            if (t != ActivityType.None && _MonitorVaultSpecial)
            {
                var api = ((Settings.IGw2Account)taccount).Api;

                if (api != null)
                {
                    switch (t)
                    {
                        case ActivityType.ApiActive:

                            vob.ApiManager.Queue(new ApiRequest(ApiData.DataType.VaultSpecial, taccount, api, ApiData.DataRequest.RequestOptions.None));

                            break;
                        case ActivityType.LinkActive:

                            vob.ApiManager.Queue(new ApiRequest(ApiData.DataType.Account, taccount, api, ApiData.DataRequest.RequestOptions.NoCache));

                            break;
                    }
                }
            }
        }

        void Launcher_MumbleLinkVerified(Settings.IAccount account, Tools.Mumble.MumbleMonitor.IMumbleProcess e)
        {
            Vault.VaultType current;

            if ((currentTab & DataType.Vault) != 0)
            {
                current = GetVaultType(currentTab);
                vob.Refresh(current, (Settings.IGw2Account)account, Tools.Api.VaultObjectives.RefreshOptions.Delayed);
            }
            else
            {
                current = (Vault.VaultType)(-1);
            }

            var types = new Vault.VaultType[] 
            { 
                Vault.VaultType.Daily, 
                Vault.VaultType.Weekly, 
                Vault.VaultType.Special,
            };

            foreach (var t in types)
            {
                if (t != current)
                {
                    vob.Refresh(t, (Settings.IGw2Account)account, Tools.Api.VaultObjectives.RefreshOptions.NoQuery);
                }
            }

            if (_MonitorVaultSpecial)
            {
                Util.ScheduledEvents.Register(OnScheduledVaultSpecialRefresh, DateTime.UtcNow.AddMinutes(10), Util.ScheduledEvents.RegisterOptions.Async);
            }

        }

        private void Launcher_AccountExited(Settings.IAccount account)
        {
            vob.Refresh((Settings.IGw2Account)account);
        }

        private void CefSessions_SessionEvent(object sender, Tools.Chromium.CefSessionMonitor.SessionEventArgs e)
        {
            if (e.Type == Tools.Chromium.CefSessionMonitor.SessionEventArgs.EventType.VaultClosed)
            {
                var a = (Settings.IGw2Account)e.Account;
                var currentTab = this.currentTab;

                vob.Refresh(a);

                if ((currentTab & DataType.Vault) != 0)
                {
                    var tab = GetTab(currentTab);
                    var vt = GetVaultType(currentTab);
                    var o = vob.GetObjectives(vt, a);

                    if (o == null)
                    {
                        vob.Refresh(vt, a, Tools.Api.VaultObjectives.RefreshOptions.None);
                    }
                    else if (tab.Watched != null)
                    {
                        if (o != null)
                        {
                            lock (tab.Watched)
                            {
                                WatchedAccount wa;

                                if (!tab.Watched.TryGetValue(GetVaultKey(currentTab, o.ID), out wa) || !wa.watched || wa.account != a)
                                {
                                    return;
                                }
                            }

                            vob.Refresh(vt, a, Tools.Api.VaultObjectives.RefreshOptions.Delayed | Tools.Api.VaultObjectives.RefreshOptions.Update);
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Util.ScheduledEvents.Unregister(OnScheduledRetry);
                Util.ScheduledEvents.Unregister(OnScheduledDailiesRefresh);

                Client.Launcher.MumbleLinkVerified -= Launcher_MumbleLinkVerified;
                Client.Launcher.CefSessions.SessionEvent -= CefSessions_SessionEvent;

                Settings.ShowDailies.ValueChanged -= Settings_ValueChanged;
                Settings.ShowDailiesLanguage.ValueChanged -= Language_ValueChanged;

                parent.VisibleChanged -= parent_VisibleChanged;

                MonitorVaultSpecial(false);

                if (components != null)
                    components.Dispose();

                if (minimized != null)
                {
                    minimized.Dispose();
                    minimized = null;
                }

                if (reusable != null)
                {
                    reusable.Dispose();
                    reusable = null;
                }

                if (dailies != null)
                {
                    dailies.Dispose();
                    dailies = null;
                }

                LinkedToParent = false;
            }
            base.Dispose(disposing);
        }

        public void Show(bool focus)
        {
            bool autoMinimize = !Settings.ShowDailies.Value.HasFlag(Settings.DailiesMode.Positioned);

            if (!this.Visible)
            {
                if (autoMinimize)
                {
                    SetShape(buttonMinimize, FlatShapeButton.IconShape.Ellipse);
                }
                else
                {
                    SetShape(buttonMinimize, FlatShapeButton.IconShape.Arrow);
                    if (alignment == HorizontalAlignment.Right)
                        buttonMinimize.ShapeDirection = ArrowDirection.Left;
                    else
                        buttonMinimize.ShapeDirection = ArrowDirection.Right;
                }

                this.minimizeOnMouseLeave = autoMinimize;

                if (loadOnShow)
                {
                    loadOnShow = false;
                    SelectTab(DataType.DailyToday);
                }

                if (linkedToParent)
                {
                    if (!parent.Visible)
                    {
                        EventHandler onVisible = null;
                        onVisible = delegate
                        {
                            if (parent.Visible)
                            {
                                parent.VisibleChanged -= onVisible;
                                if (!this.IsDisposed && !this.Visible)
                                {
                                    this.Show(parent);
                                    if (autoMinimize)
                                        MinimizeOnMouseLeave();
                                    if (focus)
                                        this.Focus();
                                }
                            }
                        };
                        parent.VisibleChanged += onVisible;
                    }
                    else
                    {
                        this.Show(parent);
                    }
                }
                else
                    this.Show();

                if (autoMinimize && this.Visible)
                    MinimizeOnMouseLeave();
            }
            if (focus && this.Visible)
                this.Focus();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                this.Refresh();
                this.Opacity = 1;
                NativeMethods.ShowWindow(this.Handle, ShowWindowCommands.ShowNoActivate);
                MonitorVaultSpecial(true);
            }
            else
            {
                this.Opacity = 0;
                MonitorVaultSpecial(false);
            }

            base.OnVisibleChanged(e);
        }

        public bool AutoMinimize
        {
            get
            {
                return minimizeOnMouseLeave;
            }
            set
            {
                if (minimizeOnMouseLeave != value)
                {
                    minimizeOnMouseLeave = value;
                    if (value)
                    {
                        SetShape(buttonMinimize, FlatShapeButton.IconShape.Ellipse);
                        MinimizeOnMouseLeave();
                    }
                    else
                        SetShape(buttonMinimize, FlatShapeButton.IconShape.Arrow);
                }
            }
        }

        public bool LinkedToParent
        {
            get
            {
                return linkedToParent;
            }
            set
            {
                if (value != linkedToParent)
                {
                    linkedToParent = value;

                    if (value)
                    {
                        this.Owner = parent;

                        parent.LocationChanged += parent_LocationChanged;
                        parent.SizeChanged += parent_SizeChanged;

                        PositionToParent();
                    }
                    else
                    {
                        this.Owner = null;

                        parent.LocationChanged -= parent_LocationChanged;
                        parent.SizeChanged -= parent_SizeChanged;

                        minimizeOnMouseLeave = false;

                        SetShape(buttonMinimize, FlatShapeButton.IconShape.Arrow);
                        buttonMinimize.ShapeDirection = ArrowDirection.Left;
                        SetAlignment(HorizontalAlignment.Right);
                    }
                }
            }
        }

        private void SetAlignment(HorizontalAlignment alignment)
        {
            if (this.alignment == alignment)
                return;
            this.alignment = alignment;

            AnchorStyles a1, a2;

            if (alignment == HorizontalAlignment.Right)
            {
                a1 = ~AnchorStyles.Left;
                a2 = AnchorStyles.Right;

                buttonMinimize.ShapeAlignment = ContentAlignment.MiddleRight;
                buttonMinimize.ShapeDirection = ArrowDirection.Left;
            }
            else
            {
                a1 = ~AnchorStyles.Right;
                a2 = AnchorStyles.Left;

                buttonMinimize.ShapeAlignment = ContentAlignment.MiddleLeft;
                buttonMinimize.ShapeDirection = ArrowDirection.Right;
            }

            panelTabs.Left = this.Width - panelTabs.Right;
            buttonMinimize.Left = this.Width - buttonMinimize.Right;
            scrollV.Left = this.Width - scrollV.Right;
            panelContainer.Left = this.Width - panelContainer.Right;

            panelTabs.Anchor = panelTabs.Anchor & a1 | a2;
            buttonMinimize.Anchor = buttonMinimize.Anchor & a1 | a2;
            scrollV.Anchor = scrollV.Anchor & a1 | a2;
        }

        public void Minimize(bool focus)
        {
            if (this.Visible || minimized == null || minimized.IsDisposed || !minimized.Visible)
            {
                if (minimized == null || minimized.IsDisposed)
                {
                    minimized = new MinimizedWindow(this, parent);
                    minimized.VisibleChanged += minimized_VisibleChanged;
                    minimized.Shown += minimized_Shown;
                }

                if (!minimized.Visible)
                {
                    minimized.Show(focus);
                    if (this.Visible)
                        this.Hide();
                }
            }
            else if (focus)
                minimized.Focus();
        }

        void minimized_VisibleChanged(object sender, EventArgs e)
        {
            if (minimized.Visible && this.Visible)
            {
                NativeMethods.ShowWindow(minimized.Handle, ShowWindowCommands.ShowNoActivate);
                if (this.ContainsFocus)
                    minimized.Focus();
                this.Hide();
            }
        }

        void minimized_Shown(object sender, EventArgs e)
        {
            minimized.Shown -= minimized_Shown;
            base.OnShown(e);
        }

        private void SetShape(FlatShapeButton button, FlatShapeButton.IconShape shape)
        {
            button.Shape = shape;

            switch (shape)
            {
                case FlatShapeButton.IconShape.Arrow:

                    button.ShapeSize = new Size(4, 8);

                    break;
                case FlatShapeButton.IconShape.Ellipse:

                    button.ShapeSize = new Size(5, 5);

                    break;
            }
        }

        private void buttonMinimize_Click(object sender, EventArgs e)
        {
            if (minimizeOnMouseLeave)
            {
                minimizeOnMouseLeave = false;

                SetShape(buttonMinimize, FlatShapeButton.IconShape.Arrow);
                buttonMinimize.ShapeDirection = alignment == HorizontalAlignment.Right ? ArrowDirection.Left : ArrowDirection.Right;
            }
            else
            {
                Minimize(true);
            }
        }

        private async void MinimizeOnMouseLeave()
        {
            if (waitingToMinimize)
                return;

            waitingToMinimize = true;

            do
            {
                await Task.Delay(500);

                if (minimizeOnMouseLeave && this.Visible)
                {
                    if (!new Rectangle(this.Left - 50, this.Top - 50, this.Width + 100, this.Height + 100).Contains(Cursor.Position) && !MouseButtons.HasFlag(MouseButtons.Left))
                        break;
                }
                else
                {
                    waitingToMinimize = false;
                    return;
                }
            }
            while (true);

            waitingToMinimize = false;
            this.Minimize(false);
        }

        private int Abs(int i)
        {
            if (i < 0)
                return -i;
            return i;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason== CloseReason.UserClosing)
            {
                e.Cancel = true;
                Minimize(true);
            }
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            Point p;
            RECT r;
            int w, h;

            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCLBUTTONDBLCLK:

                    break;
                case WindowMessages.WM_NCHITTEST:

                    base.WndProc(ref m);

                    if (m.Result == (IntPtr)HitTest.Client)
                    {
                        p = this.PointToClient(new Point(m.LParam.GetValue32()));

                        //if (p.Y > buttonTomorrow.Bottom && p.Y < buttonMinimize.Top)
                        //{
                        if (alignment == HorizontalAlignment.Right)
                        {
                            if (p.X >= this.Width - 5)
                            {
                                m.Result = (IntPtr)HitTest.BottomRight;
                            }
                            else
                                m.Result = (IntPtr)HitTest.Caption;
                        }
                        else
                        {
                            if (p.X <= 5)
                            {
                                m.Result = (IntPtr)HitTest.BottomLeft;
                            }
                            else
                                m.Result = (IntPtr)HitTest.Caption;
                        }
                        //}
                    }

                    break;
                case WindowMessages.WM_SIZING:

                    base.WndProc(ref m);

                    if (linkedToParent)
                    {
                        r = (RECT)m.GetLParam(typeof(RECT));

                        var screen = Screen.FromControl(parent).WorkingArea;
                        var mid = (parent.Top + parent.Height / 2);
                        h = r.bottom - mid;
                        r.top = mid - h;

                        if (r.bottom - r.top < this.MinimumSize.Height)
                        {
                            var mh = this.MinimumSize.Height / 2;
                            r.top = mid - mh;
                            r.bottom = mid + mh;
                        }

                        if (r.top < screen.Top && parent.Top >= screen.Top)
                            r.top = screen.Top;
                        if (r.bottom > screen.Bottom && parent.Bottom <= screen.Bottom)
                            r.bottom = screen.Bottom;

                        System.Runtime.InteropServices.Marshal.StructureToPtr(r, m.LParam, false);
                    }

                    break;
                case WindowMessages.WM_MOVING:

                    base.WndProc(ref m);

                    r = (RECT)m.GetLParam(typeof(RECT));
                    p = Point.Subtract(Cursor.Position, (Size)sizingOrigin);

                    w = r.right - r.left;
                    h = r.bottom - r.top;

                    r.left = sizingBounds.left + p.X;
                    r.top = sizingBounds.top + p.Y;
                    r.right = r.left + w;
                    r.bottom = r.top + h;

                    if (r.top < parent.Bottom && r.bottom > parent.Top)
                    {
                        if (Abs(parent.Right + padding - r.left) < 10)
                        {
                            r.left = parent.Right + padding;
                            r.right = r.left + w;
                            r.top = parent.Top + parent.Height / 2 - h / 2;
                            r.bottom = r.top + h;
                        }
                        else if (Abs(parent.Left - padding - r.right) < 10)
                        {
                            r.right = parent.Left - padding;
                            r.left = r.right - w;
                            r.top = parent.Top + parent.Height / 2 - h / 2;
                            r.bottom = r.top + h;
                        }
                    }

                    System.Runtime.InteropServices.Marshal.StructureToPtr(r, m.LParam, false);

                    break;
                case WindowMessages.WM_ENTERSIZEMOVE:

                    base.WndProc(ref m);

                    sizing = true;
                    sizingOrigin = Cursor.Position;
                    sizingBounds = new RECT()
                    {
                        left = this.Left,
                        right = this.Right,
                        top = this.Top,
                        bottom = this.Bottom,
                    };

                    if (!linkedToParent && parent.Visible)
                    {
                        this.LocationChanged += OnBeginLocationChanged;
                    }

                    break;
                case WindowMessages.WM_EXITSIZEMOVE:

                    base.WndProc(ref m);

                    sizing = false;
                    this.LocationChanged -= OnBeginLocationChanged;

                    bool l;
                    if (Settings.IsRunningWine)
                        l = this.Top < parent.Bottom && this.Bottom > parent.Top && (Abs(parent.Right + padding - this.Left) < 10 || Abs(parent.Left - padding - this.Right) < 10);
                    else
                        l = (this.Left == parent.Right + padding || this.Right == parent.Left - padding);
                    if (l && l == linkedToParent)
                        PositionToParent();
                    else
                        LinkedToParent = l;

                    if (minimized != null)
                        minimized.PositionToParent();

                    if (linkedToParent)
                    {
                        Settings.WindowBounds[this.GetType()].Value = new Rectangle(new Point(int.MinValue, int.MinValue), this.Size);
                        Settings.ShowDailies.Value &= ~Settings.DailiesMode.Positioned;
                    }
                    else
                    {
                        Settings.WindowBounds[this.GetType()].Value = this.Bounds;
                        Settings.ShowDailies.Value |= Settings.DailiesMode.Positioned;
                    }

                    break;
                default:

                    base.WndProc(ref m);

                    break;
            }
        }

        void OnBeginLocationChanged(object sender, EventArgs e)
        {
            this.LocationChanged -= OnBeginLocationChanged;

            if (!linkedToParent && parent.Visible)
            {
                parent.BringToFront();
                this.Focus();
            }
        }

        public override void RefreshColors()
        {
            base.RefreshColors();

            panelContent.BackColor = UiColors.GetColor(UiColors.Colors.DailiesSeparator);
            labelRetry.ForeColor = UiColors.GetColor(UiColors.Colors.DailiesTextLight);
            buttonDaySwap.ForeColor = Util.Color.Gradient(panelTabs.ForeColor, panelTabs.BackColor, 0.5f);
            
            foreach (Control c in panelContent.Controls)
            {
                if (c is LastUpdatedLabel)
                {
                    c.BackColor = UiColors.GetColor(UiColors.Colors.DailiesBackColor);
                }
            }

            this.Invalidate();
        }

        private void categoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new formDailyCategories(categories, dailies != null ? dailies.Categories : null))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (Util.Array.Equals<ushort>(Daily.GetDefaultCategories(), f.SelectedCategories))
                    {
                        Settings.ShowDailiesCategories.Clear();
                    }
                    else if (!Util.Array.Equals<ushort>(Settings.ShowDailiesCategories.Value, f.SelectedCategories))
                    {
                        Settings.ShowDailiesCategories.Value = f.SelectedCategories;
                    }
                }
            }
        }

        private void favoritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTaggedDialog(Settings.TaggedType.Favorite);
        }

        private void ignoredToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTaggedDialog(Settings.TaggedType.Ignored);
        }

        private void showOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!showOnTopToolStripMenuItem.Checked)
            {
                Settings.ShowDailies.Value |= Settings.DailiesMode.TopMost;
            }
            else
            {
                Settings.ShowDailies.Value &= ~Settings.DailiesMode.TopMost;
            }
        }

        private void buttonVault_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
            else
            {
                DataType t;

                if (sender == buttonSpecial)
                {
                    t = DataType.VaultSpecial;

                    if (buttonSpecial.ShowNotification)
                    {
                        if (currentTab == DataType.VaultSpecial)
                        {
                            buttonSpecial.ShowNotification = false;
                            buttonSpecial.SelectedChanged -= buttonSpecial_SelectedChanged;
                        }
                        else
                        {
                            buttonSpecial.SelectedChanged += buttonSpecial_SelectedChanged;
                        }
                    }
                }
                else if (sender == buttonWeekly)
                {
                    t = DataType.VaultWeekly;
                }
                else
                {
                    t = DataType.VaultDaily;
                }

                SelectTab(t);
            }
        }

        void buttonSpecial_SelectedChanged(object sender, EventArgs e)
        {
            if (!buttonSpecial.Selected)
            {
                buttonSpecial.ShowNotification = false;
                buttonSpecial.SelectedChanged -= buttonSpecial_SelectedChanged;
            }
        }

        private Util.ScheduledEvents.Ticks OnScheduledVaultRefresh()
        {
            if ((currentTab & DataType.Vault) != 0)
            {
                if (GetTab(currentTab).Refresh)
                {
                    RefreshDailies(false, false);
                }
            }

            return Util.ScheduledEvents.Ticks.None;
        }

        private DataType GetType(ApiData.DataType t)
        {
            switch (t)
            {
                case ApiData.DataType.VaultDaily:

                    return DataType.VaultDaily;

                case ApiData.DataType.VaultWeekly:

                    return DataType.VaultWeekly;

                case ApiData.DataType.VaultSpecial:

                    return DataType.VaultSpecial;

                default:

                    return DataType.None;
            }
        }

        private ApiData.DataType GetApiType(DataType t)
        {
            switch (t)
            {
                case DataType.VaultWeekly:

                    return ApiData.DataType.VaultWeekly;

                case DataType.VaultSpecial:

                    return ApiData.DataType.VaultSpecial;
            
                case DataType.VaultDaily:
                default:

                    return ApiData.DataType.VaultDaily;
            }
        }

        private DataType GetType(Vault.VaultType t)
        {
            switch (t)
            {
                case Vault.VaultType.Daily:

                    return DataType.VaultDaily;

                case Vault.VaultType.Weekly:

                    return DataType.VaultWeekly;

                case Vault.VaultType.Special:

                    return DataType.VaultSpecial;
            }

            return DataType.None;
        }

        private Vault.VaultType GetVaultType(DataType t)
        {
            switch (t)
            {
                case DataType.VaultWeekly:

                    return Vault.VaultType.Weekly;

                case DataType.VaultSpecial:

                    return Vault.VaultType.Special;

                case DataType.VaultDaily:
                default:

                    return Vault.VaultType.Daily;
            }
        }

        private void PendingReposition(int i)
        {
            if (i < reposition)
            {
                if (i < 0)
                {
                    reposition = 0;
                }
                else
                {
                    reposition = i;
                }
            }
        }

        private void DoPendingReposition()
        {
            if (reposition != int.MaxValue)
            {
                PositionGroups(reposition);
                reposition = int.MaxValue;
            }
        }

        void vob_AccountDataChanged(object sender, Tools.Api.VaultObjectives.DataChangedEventArgs e)
        {
            if ((e.Changed & Tools.Api.VaultObjectives.ChangeType.Objectives) == 0 && (e.Changed & (Tools.Api.VaultObjectives.ChangeType.Date | Tools.Api.VaultObjectives.ChangeType.Values)) != 0)
            {
                var t = GetType(e.Type);

                if (currentTab == t)
                {
                    var tab = GetTab(t);

                    if (tab.Watched != null)
                    {
                        WatchedAccount wa;

                        lock (tab.Watched)
                        {
                            if (!tab.Watched.TryGetValue(GetVaultKey(t, e.Group.ID), out wa) || !wa.watched || wa.account != e.Data.Account)
                            {
                                return;
                            }
                        }

                        Util.Invoke.Async(this, delegate
                        {
                            if (currentTab == t && IsLoaded(t))
                            {
                                ItemGroup g;
                                if (wa.GetGroup(this.data, out g))
                                {
                                    if (!UpdateProgress(g))
                                    {
                                        wa.Abort();
                                    }
                                    DoPendingReposition();
                                }
                            }
                        });
                    }
                }
            }
        }

        void vob_DataChanged(object sender, Tools.Api.VaultObjectives.DataChangedEventArgs e)
        {
            var t = GetType(e.Type);

            if (t != DataType.None)
            {
                var tab = GetTab(t);

                if (t == DataType.VaultSpecial)
                {
                    if ((e.Changed & Tools.Api.VaultObjectives.ChangeType.Date) != 0)
                    {
                        if (_MonitorVaultSpecial)
                        {
                            Util.Invoke.Async(this, delegate
                            {
                                MonitorVaultSpecial(false);
                            });
                        }
                    }

                    if ((e.Changed & Tools.Api.VaultObjectives.ChangeType.ObjectivesAdded) != 0)
                    {
                        buttonSpecial.ShowNotification = true;
                        Util.Logging.LogEvent("New special objectives");
                    }
                }

                if ((e.Changed & (Tools.Api.VaultObjectives.ChangeType.Cleared | Tools.Api.VaultObjectives.ChangeType.Objectives)) != 0)
                {
                    tab.Refresh = true;

                    if (currentTab == t)
                    {
                        Util.ScheduledEvents.Register(OnScheduledVaultRefresh, 1000);
                    }
                }
                else if ((e.Changed & Tools.Api.VaultObjectives.ChangeType.Accounts) != 0 && currentTab == t && e.Group != null)
                {
                    Util.Invoke.Async(this, delegate
                    {
                        if (currentTab == t && IsLoaded(t))
                        {
                            foreach (var g in this.data.groups)
                            {
                                if (g.count > 0 && g.source == e.Group)
                                {
                                    if ((e.Changed & Tools.Api.VaultObjectives.ChangeType.Accounts) != 0)
                                    {
                                        var items = GetDropDownItems(e.Group.Accounts);

                                        if (items == null)
                                        {
                                            tab.Refresh = true;
                                            Util.ScheduledEvents.Register(OnScheduledVaultRefresh, 5000);
                                        }
                                        else
                                        {
                                            var selected = (Util.ComboItem<Settings.IAccount>)g.bar.DropDownSelectedItem;

                                            int i;

                                            if (selected != null)
                                            {
                                                i = GetDropDownItem(items, selected.Value, g.bar.DropDownSelectedIndex);
                                            }
                                            else
                                            {
                                                i = -1;
                                            }

                                            if (i == -1)
                                            {
                                                //an account that was selected was removed from the group
                                                i = 0;
                                                g.bar.Text = items[i].Value.Name;

                                                if (g.watched != null)
                                                {
                                                    g.watched.Abort();
                                                    g.watched = null;

                                                    if (tab.Watched != null)
                                                    {
                                                        lock (tab.Watched)
                                                        {
                                                            tab.Watched.Remove(e.Group.ID);
                                                        }
                                                    }

                                                    if (g.bar.ButtonEyeEnabled)
                                                    {
                                                        g.bar.ButtonEyeEnabled = false;
                                                        UpdateProgress(g);
                                                    }
                                                }
                                            }

                                            g.bar.DropDownItems = items;
                                            g.bar.DropDownSelectedIndex = i;
                                            g.bar.ButtonDropDownArrowVisible = items.Length > 1;

                                            if (g.squares != null)
                                            {
                                                var b = g.squares.Count > 1;

                                                g.squares.SetAccounts(e.Group.Accounts);
                                                g.squares.Selected = items[i].Value;

                                                if (b != (g.squares.Count > 1))
                                                {
                                                    PendingReposition(g.index);
                                                }
                                            }
                                        }
                                    }

                                    DoPendingReposition();

                                    break;
                                }
                            }
                        }
                    });
                }
            }
        }

        private bool IsLoaded(DataType t)
        {
            return (isLoading & t) == 0 && this.data != null && this.data.type == t;
        }

        private Util.ScheduledEvents.Ticks OnScheduledRefreshWatched()
        {
            if ((currentTab & DataType.Vault) == 0 || !IsLoaded(currentTab))
            {
                return Util.ScheduledEvents.Ticks.None;
            }

            var count = 0;
            var vt = GetVaultType(currentTab);
            var at = GetApiType(currentTab);

            foreach (var g in this.data.groups)
            {
                var wa = g.watched;

                if (wa == null || !wa.watched)
                {
                    continue;
                }

                if (g.updated != null)
                {
                    g.updated.UpdateText();
                }

                var ao = vob.GetObjectives(wa.account);

                if (ao == null || !ao.IsComplete(vt))
                {
                    ++count;

                    if (ao != null && Client.Launcher.IsActive(wa.account))
                    {
                        Refresh(wa);
                    }
                }
                else
                {
                    wa.date = DateTime.MinValue;
                }

                if (wa.request == null)
                {
                    g.bar.ButtonEyeTimerEnabled = GetPending(wa.account.Api, at) != 0;
                }
            }

            DoPendingReposition();

            if (count > 0)
            {
                return new Util.ScheduledEvents.Ticks(60000);
            }
            else
            {
                return Util.ScheduledEvents.Ticks.None;
            }
        }

        private async void Refresh(WatchedAccount w, int limit = 10000)
        {
            var refresh = false;
            var t = this.currentTab;
            var l = Client.Launcher.GetMumbleLink(w.account);
            Tools.Mumble.MumbleMonitor.IMumbleSubscriber m;

            if (l != null && l.IsValid && (m = l.Subscribe(Tools.Mumble.MumbleMonitor.DataScope.Basic)) != null)
            {
                try
                {
                    if (await m.Refresh(limit))
                    {
                        Tools.Mumble.MumbleData.PositionData d;

                        if (m.GetData<Tools.Mumble.MumbleData.PositionData>(out d))
                        {
                            var sum = d.fAvatarPosition[0] + d.fAvatarPosition[1] + d.fAvatarPosition[2];

                            if (w.position != sum)
                            {
                                w.position = sum;
                                w.date = DateTime.UtcNow;
                                refresh = true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
                finally
                {
                    m.Dispose();
                }
            }
            else
            {
                w.date = DateTime.UtcNow;
                refresh = true;
            }

            if (refresh)
            {
                ItemGroup g;

                if (t == currentTab && w.watched && w.request == null && w.GetGroup(data, out g))
                {
                    Util.Logging.LogEvent(w.account, "[Refresh] Queueing request for [" + currentTab + "]");

                    if (QueueApiRequest(currentTab, w, false))
                    {
                        g.bar.ButtonEyeTimerEnabled = true;
                    }
                }
            }
        }

        private void buttonTomorrow_SizeChanged(object sender, EventArgs e)
        {
            if (buttonToday.MinimumSize.Height != buttonTomorrow.Height)
            {
                buttonToday.MinimumSize = new Size(0, buttonTomorrow.Height);
            }
        }

        private void buttonDaySwap_Click(object sender, EventArgs e)
        {
            if (buttonDaySwap.ShapeDirection == ArrowDirection.Right)
            {
                SelectTab(DataType.DailyTomorrow);
            }
            else
            {
                SelectTab(DataType.DailyToday);
            }

        }

        private void buttonToday_MouseEnteredChanged(object sender, EventArgs e)
        {
            buttonDaySwap.IsHovered = (buttonDaySwap.ShapeDirection == ArrowDirection.Right ? buttonToday : buttonTomorrow).IsMouseEntered;
        }

        private void buttonDaySwap_MouseEnteredChanged(object sender, EventArgs e)
        {
            var b = buttonDaySwap.IsMouseEntered || (buttonDaySwap.ShapeDirection == ArrowDirection.Right ? buttonToday : buttonTomorrow).IsMouseEntered;
            buttonToday.IsHovered = b;
            buttonTomorrow.IsHovered = b;
            buttonDaySwap.IsHovered = b;
        }

        private void buttonToday_SelectedChanged(object sender, EventArgs e)
        {
            buttonDaySwap.Selected = (buttonDaySwap.ShapeDirection == ArrowDirection.Right ? buttonToday : buttonTomorrow).Selected;
        }

        private bool _MonitorVaultSpecial;
        private void MonitorVaultSpecial(bool enabled)
        {
            var next = GetVaultSpecialDelay(GetVaultSpecialLastModified());

            if (enabled && DateTime.UtcNow >= next)
            {
                if (!_MonitorVaultSpecial)
                {
                    _MonitorVaultSpecial = true;
                    vob.ApiManager.DataSource.DataAvailable += OnApiDataAvailable;
                    Util.ScheduledEvents.Register(OnScheduledVaultSpecialRefresh, DateTime.UtcNow.AddMinutes(10));
                }
            }
            else if (_MonitorVaultSpecial)
            {
                _MonitorVaultSpecial = false;
                vob.ApiManager.DataSource.DataAvailable -= OnApiDataAvailable;
                Util.ScheduledEvents.Register(OnScheduledVaultSpecial, next);
            }
            else if (enabled)
            {
                Util.ScheduledEvents.Register(OnScheduledVaultSpecial, next);
            }
        }

        private DateTime GetVaultSpecialDelay(DateTime last)
        {
            var n = DateTime.UtcNow;

            //new special objectives can be added at any time
            //check every 3 hours
            //check every 1 hour on tuesday during usual patch times

            if (n.DayOfWeek == DayOfWeek.Tuesday && n.Hour >= 14 && n.Hour <= 21)
            {
                if (n.Hour >= 16)
                {
                    return last.AddHours(1);
                }
                else
                {
                    return last.AddMinutes(16 * 60 - (n.Minute + n.Hour * 60) + 1);
                }
            }

            return last.AddHours(3);
        }

        private DateTime GetVaultSpecialLastModified()
        {
            var objectives = vob.GetObjectives(Vault.VaultType.Special);
            var last = DateTime.MinValue;

            if (objectives != null)
            {
                foreach (var o in objectives)
                {
                    if (o.LastModified > last)
                    {
                        last = o.LastModified;
                    }
                }
            }

            return last;
        }

        private Settings.IGw2Account GetAccount(ApiData.IApiKey key)
        {
            Settings.ApiDataKey k;

            if (key is Settings.ApiDataKey)
            {
                k = (Settings.ApiDataKey)key;
            }
            else
            {
                Settings.ISettingValue<Settings.ApiDataKey> v;

                if (Settings.ApiKeys.TryGetValue(key.Key, out v))
                {
                    k = v.Value;

                    if (k == null)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return k.GetLastUsedAccount();
        }

        void OnApiDataAvailable(object sender, ApiData.DataAvailableEventArgs e)
        {
            if (e.Type == ApiData.DataType.Account && e.Status == ApiData.DataStatus.Changed && (e.Key.Permissions & TokenInfo.Permissions.Progression) != 0)
            {
                var last = GetVaultSpecialLastModified();
                var n = DateTime.UtcNow;

                if (n.Subtract(last).TotalHours > 3 && n.Subtract(e.LastModifiedInLocalTime).TotalMinutes < 10)
                {
                    var a = GetAccount(e.Key);

                    if (a != null)
                    {
                        vob.ApiManager.Queue(new ApiRequest(ApiData.DataType.VaultSpecial, a, a.Api, ApiData.DataRequest.RequestOptions.None));
                    }
                }
            }
        }

        private void ShowTaggedDialog(Settings.TaggedType type)
        {
            using (var f = new formDailyFavorites(da, type))
            {
                f.ShowDialog(this);

                if (f.Modified && this.data != null && (this.data.type & DataType.Daily) != 0)
                {
                    var refresh = type == Settings.TaggedType.Ignored;
                    var groups = this.data.groups;

                    if (!refresh)
                    {
                        foreach (var g in groups)
                        {
                            for (var i = 0; i < g.count; i++)
                            {
                                var c = g.controls[i];

                                Settings.TaggedType tt;
                                Settings.TaggedDailies.TryGetValue(c.DataSource.ID, out tt);

                                if (c.DataSource.Tagged != tt)
                                {
                                    if (c.DataSource.Tagged == Settings.TaggedType.Ignored || tt == Settings.TaggedType.Ignored)
                                    {
                                        refresh = true;
                                        break;
                                    }

                                    c.DataSource.Tagged = tt;
                                    c.FavSelected = tt == Settings.TaggedType.Favorite;
                                }
                            }
                        }
                    }

                    if (refresh)
                    {
                        loadedTab = DataType.None;
                        RefreshDailies(false, false);
                    }
                }
            }
        }
    }
}
