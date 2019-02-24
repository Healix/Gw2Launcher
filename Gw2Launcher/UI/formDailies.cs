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

namespace Gw2Launcher.UI
{
    public partial class formDailies : Form
    {
        private class Popup : Form
        {
            private DailyAchievement control;
            private Image defaultImage;

            public Popup(Image defaultImage)
            {
                this.defaultImage = defaultImage;

                this.Opacity = 0;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.Manual;
                this.BackColor = Color.White;

                control = new DailyAchievement()
                {
                    IconSize = new Size(64, 64),
                    IconVisible = true,
                    NameVisible = true,
                    NameFont = new System.Drawing.Font("Segoe UI Semibold", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    DescriptionVisible = true,
                    DescriptionFont = new System.Drawing.Font("Segoe UI Semilight", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    LevelVisible = true,
                    LevelFont = new System.Drawing.Font("Segoe UI Semilight", 8.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                    Location = new Point(5, 5),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left,
                    BackColor = Color.White,
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

            public void SetData(DailyAchievements.Daily daily)
            {
                control.IconValue = daily.Icon != null ? daily.Icon : defaultImage;
                control.NameValue = daily.Name;
                control.DescriptionValue = daily.Requirement;

                if (daily.MinLevel == daily.MaxLevel)
                    control.LevelValue = "Level " + daily.MinLevel.ToString();
                else if (daily.MaxLevel < 80)
                    control.LevelValue = daily.MinLevel + " to " + daily.MaxLevel;
                else
                    control.LevelValue = "Level " + daily.MinLevel + "+";

                var size = control.GetPreferredSize(new Size(this.Width - 10, Int32.MaxValue));

                control.Size = size;
                this.Height = size.Height + 10;

                this.Invalidate(true);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                base.OnPaintBackground(e);

                e.Graphics.DrawRectangle(SystemPens.ControlLight, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private class MinimizedWindow : Form
        {
            private Form parent;
            private formDailies owner;
            private FlatShapeButton buttonMinimize;
            private HorizontalAlignment alignment;
            private bool visible, wasVisible;

            public MinimizedWindow(formDailies owner, Form parent)
            {
                this.owner = owner;
                this.parent = parent;
                this.Owner = parent;

                this.FormBorderStyle = FormBorderStyle.None;
                this.BackColor = Color.White;
                this.ShowInTaskbar = false;

                var h = this.Handle; //force

                this.Size = new System.Drawing.Size(18, 66);

                this.alignment = owner.alignment;

                buttonMinimize = new FlatShapeButton()
                {
                    ShapeAlignment = ContentAlignment.MiddleCenter,
                    ShapeDirection = alignment == HorizontalAlignment.Left ? ArrowDirection.Left : ArrowDirection.Right,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
                    Location = new Point(1, 1),
                    Size = new Size(this.Width - 2, this.Height - 2),
                    ShapeSize = new Size(5, 9),
                    BackColorHovered = SystemColors.ControlLight,
                    ForeColor = SystemColors.GrayText,
                    ForeColorHovered = Util.Color.Darken(SystemColors.GrayText, 0.5f),
                };

                this.Controls.Add(buttonMinimize);

                buttonMinimize.Click += buttonMinimize_Click;
                buttonMinimize.MouseHover += buttonMinimize_MouseHover;

                parent.LocationChanged += parent_LocationChanged;
                parent.SizeChanged += parent_SizeChanged;
                parent.VisibleChanged += parent_VisibleChanged;

                owner.VisibleChanged += owner_VisibleChanged;

                PositionToParent();
            }

            void buttonMinimize_MouseHover(object sender, EventArgs e)
            {
                if (!Settings.ShowDailies.Value.HasFlag(Settings.DailiesMode.Positioned))
                    owner.Show(false);
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

                e.Graphics.DrawRectangle(SystemPens.WindowFrame, 0, 0, this.Width - 1, this.Height - 1);
            }

            void buttonMinimize_Click(object sender, EventArgs e)
            {
                owner.Show(true);
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
                    this.Visible = true;
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

            void owner_VisibleChanged(object sender, EventArgs e)
            {
                if (owner.Visible)
                    this.Hide();
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

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    parent.LocationChanged -= parent_LocationChanged;
                    parent.SizeChanged -= parent_SizeChanged;
                    parent.VisibleChanged -= parent_VisibleChanged;
                    owner.VisibleChanged -= owner_VisibleChanged;
                }
                base.Dispose(disposing);
            }
        }

        private class DailyGroup
        {
            public byte id;
            public DailyAchievements.Daily[] dailies;
            public DailyCategoryBar bar;
            public DailyAchievement[] controls;
            public int count;
            public bool collapsed;
            public int index;
        }

        private enum DailyType : byte
        {
            None = 0,
            Today = 1,
            Tomorrow = 2,
        }

        private DailyType isLoading, isRetrying;
        private DailyAchievements.Dailies[] dailies;
        private DailyType currentTab;
        private Popup popup;
        private DailyAchievements da;
        private Image imageDefault;
        private Font fontBar, fontName, fontDescription;
        private Util.ReusableControls reusable;
        private FlatVerticalButton selectedTab;
        private HorizontalAlignment alignment;
        private MinimizedWindow minimized;
        private byte retryCount;
        private bool 
            linkedToParent,
            minimizeOnMouseLeave,
            waitingToMinimize,
            showOnLoad,
            loadOnShow,
            sizing,
            wasVisible;
        private int 
            padding;
        private Point sizingOrigin;
        private RECT sizingBounds;
        private DailyGroup[] groups;

        private Form parent;

        public formDailies(Form parent)
        {
            InitializeComponent();

            SetStyle(ControlStyles.ResizeRedraw, true);

            this.parent = parent;
            this.KeyPreview = true;

            alignment = HorizontalAlignment.Right;

            fontBar = new System.Drawing.Font("Segoe UI Semibold", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fontName = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fontDescription = new System.Drawing.Font("Segoe UI Semilight", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            padding = (parent.Width - parent.ClientSize.Width) / 2;
            if (padding > 5)
                padding = 0;
            else
                padding = 5;

            imageDefault = Properties.Resources.icon42684;

            da = new DailyAchievements();
            dailies = new DailyAchievements.Dailies[3];

            popup = new Popup(imageDefault);
            popup.Owner = this;

            buttonMinimize.ForeColorHovered = Util.Color.Darken(SystemColors.GrayText, 0.5f);

            panelContainer.MouseWheel += panelContainer_MouseWheel;
            panelContainer.MouseHover += panelContainer_MouseHover;
            this.MouseWheel += panelContainer_MouseWheel;

            Settings.ShowDailies.ValueChanged += Settings_ValueChanged;
            Settings_ValueChanged(Settings.ShowDailies, EventArgs.Empty);

            if (Settings.ShowDailies.Value.HasFlag(Settings.DailiesMode.AutoLoad))
                SelectTab(DailyType.Today);
            else
                loadOnShow = true;

            parent.VisibleChanged += parent_VisibleChanged;
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

            if (!setting.HasValue || !setting.Value.HasFlag(Settings.DailiesMode.Show))
            {
                this.Dispose();
                return;
            }

            var bounds = Settings.WindowBounds[typeof(formDailies)];
            var positioned = setting.Value.HasFlag(Settings.DailiesMode.Positioned);

            showOnLoad = setting.Value.HasFlag(Settings.DailiesMode.AutoLoad);

            AutoMinimize = !positioned;

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
                scrollV.Value -= 150;
            }
            else if (e.Delta < 0)
            {
                scrollV.Value += 150;
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

        private void buttonToday_Click(object sender, EventArgs e)
        {
            SelectTab(DailyType.Today);
        }

        private void buttonTomorrow_Click(object sender, EventArgs e)
        {
            SelectTab(DailyType.Tomorrow);
        }

        void control_MouseLeave(object sender, EventArgs e)
        {
            if (popup.Visible)
                popup.Hide();
        }

        void control_MouseEnter(object sender, EventArgs e)
        {
            var control = (DailyAchievement)sender;
            popup.Width = control.Width + scrollV.Width + 1;

            popup.SetData(control.Daily);

            var y = this.Top + panelContainer.Top + panelContent.Top + control.Top;
            popup.Location = new Point(this.Left, y + control.Height / 2 - popup.Height / 2);

            if (!popup.Visible)
                popup.Show(this);

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
                switch (currentTab)
                {
                    case DailyType.Today:
                    case DailyType.Tomorrow:

                        da.Reset(e.Control);
                        GetDailies(currentTab);

                        break;
                }

                e.Handled = true;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.DrawRectangle(SystemPens.WindowFrame, 0, 0, this.Width - 1, this.Height - 1);

            //using (var pen = new Pen(SystemBrushes.ControlDark))
            //{
            //    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            //    var r = Rectangle.FromLTRB(buttonTomorrow.Right - 5, buttonTomorrow.Bottom + 5, buttonTomorrow.Right - 2, buttonMinimize.Top - 5);

            //    e.Graphics.DrawLine(pen, r.Right, r.Top, r.Right, r.Bottom);
            //    e.Graphics.DrawLine(pen, r.Right - 2, r.Top + 1, r.Right - 2, r.Bottom - 2);
            //}
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (linkedToParent && !sizing)
                PositionToParent();

            scrollV.Maximum = panelContent.Height - panelContainer.Height;
        }

        public void LoadToday()
        {
            SelectTab(DailyType.Today);
        }

        public void LoadTomorrow()
        {
            SelectTab(DailyType.Tomorrow);
        }

        public bool LoadOnShow
        {
            get
            {
                return loadOnShow;
            }
            set
            {
                loadOnShow = value;
            }
        }

        private void SelectTab(DailyType type)
        {
            FlatVerticalButton button;

            switch (type)
            {
                case DailyType.Today:
                    button = buttonToday;
                    break;
                case DailyType.Tomorrow:
                    button = buttonTomorrow;
                    break;
                default:
                    button = null;
                    break;
            }

            if (selectedTab != button)
            {
                if (selectedTab != null)
                    selectedTab.Selected = false;
                if (button != null)
                    button.Selected = true;
                selectedTab = button;
            }

            GetDailies(type);
        }

        private void SetupControls(DailyAchievements.Dailies dailies)
        {
            if (reusable == null)
                reusable = new Util.ReusableControls();
            else
                reusable.ReleaseAll();

            var achievements = reusable.CreateOrAll<DailyAchievement>(dailies.Count,
                delegate
                {
                    var control = new DailyAchievement()
                    {
                        BackColor = Color.White,
                        NameVisible = true,
                        NameFont = fontName,
                        IconSize = new Size(32, 32),
                        IconVisible = true,
                        Size = new Size(panelContent.Width, 50),
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    };

                    control.MouseEnter += control_MouseEnter;
                    control.MouseLeave += control_MouseLeave;

                    return control;
                });

            var bars = reusable.CreateOrAll<DailyCategoryBar>(dailies.Categories.Length + 1,
                delegate
                {
                    var bar = new DailyCategoryBar()
                    {
                        Font = fontBar,
                        BackColor = SystemColors.ControlLight,
                        Size = new Size(panelContent.Width, 35),
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    };

                    bar.Collapsed += bar_Collapsed;
                    bar.Expanded += bar_Expanded;

                    return bar;
                });

            int x = 0,
                y = 0;

            byte id = 0;

            var groups = this.groups = new DailyGroup[dailies.Categories.Length + 1];
            var lowlevel = new DailyGroup()
            {
                id = 0,
                dailies = new DailyAchievements.Daily[dailies.LowlevelCount],
                collapsed = Settings.HiddenDailyCategories[0].Value,
            };
            bool addedLow = false;
            int firstIndex = 0,
                lastIndex = groups.Length - 1;

            foreach (var c in dailies.Categories)
            {
                if (c.Name == "Fractals" && !addedLow)
                {
                    if (lowlevel.collapsed)
                        groups[lastIndex--] = lowlevel;
                    else
                        groups[firstIndex++] = lowlevel;
                    addedLow = true;
                }

                id++;

                var group = new DailyGroup()
                {
                    id = id,
                    dailies = new DailyAchievements.Daily[c.Dailies.Length],
                    collapsed = Settings.HiddenDailyCategories[id].Value,
                };

                foreach (var d in c.Dailies)
                {
                    if (d.MaxLevel < 80)
                    {
                        lowlevel.dailies[lowlevel.count++] = d;
                    }
                    else
                    {
                        group.dailies[group.count++] = d;
                    }
                }

                if (group.count > 0)
                {
                    var bar = group.bar = bars.GetNext();

                    bar.SetState(group.collapsed);
                    bar.Text = c.Name;
                }

                if (group.collapsed)
                    groups[lastIndex--] = group;
                else
                    groups[firstIndex++] = group;
            }

            if (lowlevel.count > 0)
            {
                var bar = lowlevel.bar = bars.GetNext();

                bar.Tag = lowlevel;
                bar.SetState(lowlevel.collapsed);
                bar.Text = "Pre level 80";
            }

            if (!addedLow)
                groups[firstIndex++] = lowlevel;

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

                    bar.Tag = group;
                    bar.Location = new Point(x, y);
                    bar.Visible = true;

                    y += bar.Height + 1;

                    var visible = !group.collapsed;
                    group.controls = new DailyAchievement[group.count];

                    for (var i = 0; i < group.count; i++)
                    {
                        var control = group.controls[i] = achievements.GetNext();
                        var d = group.dailies[i];

                        control.Daily = d;
                        control.IconValue = d.Icon != null ? d.Icon : imageDefault;
                        control.Location = new Point(x, y);
                        control.Visible = visible;

                        if (visible)
                            y += control.Height + 1;
                    }

                    if (visible)
                        y--;
                }
            }

            while (bars.HasNext)
            {
                bars.GetNext().Visible = false;
            }

            while (achievements.HasNext)
            {
                achievements.GetNext().Visible = false;
            }

            panelContent.Height = y;
            scrollV.Maximum = y - panelContainer.Height;

            if (bars.New != null)
                panelContent.Controls.AddRange(bars.New);
            if (achievements.New != null)
                panelContent.Controls.AddRange(achievements.New);
        }

        private void PositionGroups(int startAt)
        {
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
                        y = g.bar.Bottom + 1;
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

                y += g.bar.Height + 1;

                for (int gi = 0; gi < g.count; gi++)
                {
                    var c = g.controls[gi];
                    
                    if (visible)
                    {
                        c.Top = y;
                        y += c.Height + 1;
                    }

                    c.Visible = visible;
                }

                if (visible)
                    y--;
            }

            panelContent.Height = y;
            scrollV.Maximum = y - panelContainer.Height;
        }

        private void OnCollapsedChanged(DailyGroup group, bool collapsed)
        {
            var v = Settings.HiddenDailyCategories[group.id];
            if (collapsed)
                v.Value = true;
            else
                v.Clear();
            group.collapsed = collapsed;
            PositionGroups(group.index);
        }

        void bar_Expanded(object sender, EventArgs e)
        {
            OnCollapsedChanged((DailyGroup)((DailyCategoryBar)sender).Tag, false);
        }

        void bar_Collapsed(object sender, EventArgs e)
        {
            OnCollapsedChanged((DailyGroup)((DailyCategoryBar)sender).Tag, true);
        }

        private async void GetDailies(DailyType type)
        {
            currentTab = type;
            if ((isLoading & type) == type)
                return;
            isLoading |= type;

            if (isRetrying != DailyType.None && isRetrying != type)
            {
                isRetrying = DailyType.None;
                retryCount = 0;

                Util.ScheduledEvents.Unregister(OnScheduledRetry);
            }

            DailyAchievements.Dailies dailies;
            DailyAchievements.Dailies current = this.dailies[(int)type];

            var sliderValue = scrollV.Value;

            Action onBegin =
                delegate
                {
                    waitingBounce.Visible = true;
                    panelContent.Visible = false;
                    labelMessage.Visible = false;
                    scrollV.Maximum = 0;
                };

            switch (type)
            {
                case DailyType.Today:

                    dailies = await da.GetToday(onBegin);

                    if (dailies != current && current != null)
                        current.Dispose();

                    break;
                case DailyType.Tomorrow:

                    dailies = await da.GetTomorrow(onBegin);

                    break;
                default:

                    return;
            }

            var changed = this.dailies[(int)type] != dailies;

            this.dailies[(int)type] = dailies;

            isLoading &= ~type;

            if (type == currentTab)
            {
                waitingBounce.Visible = false;

                if (dailies == null)
                {
                    isRetrying = type;
                    retryCount++;

                    if (retryCount < 5)
                    {
                        Util.ScheduledEvents.Register(OnScheduledRetry, 60000);
                    }
                    else
                    {
                        isRetrying = DailyType.None;
                        retryCount = 0;
                    }
                }
                else if (retryCount != 0)
                    retryCount = 0;

                if (dailies != null && dailies.Count > 0)
                {
                    if (this.dailies[0] != dailies)
                    {
                        this.dailies[0] = dailies;

                        if (popup.Visible)
                            popup.Hide();
                        SetupControls(dailies);
                    }

                    if (changed && showOnLoad && !this.Visible)
                    {
                        sliderValue = 0;
                        Show(false);
                    }

                    panelContent.Visible = true;
                    scrollV.Value = sliderValue;
                }
                else
                {
                    labelMessage.Text = "Unable to retrieve dailies";
                    labelMessage.MaximumSize = new Size(panelContainer.Width * 3 / 4, panelContainer.Height);
                    labelMessage.Location = new Point(panelContainer.Width / 2 - labelMessage.Width / 2, panelContainer.Height / 2 - labelMessage.Height / 2);
                    labelMessage.Visible = true;
                    panelContent.Visible = false;

                    if (!this.Visible)
                        loadOnShow = true;
                }
            }
        }

        private int OnScheduledRetry()
        {
            if (isRetrying != DailyType.None)
            {
                GetDailies(isRetrying);
            }

            return -1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Util.ScheduledEvents.Unregister(OnScheduledRetry);

                parent.VisibleChanged -= parent_VisibleChanged;

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
                    for (var i = dailies.Length - 1; i > 0; i--)
                    {
                        if (dailies[i] != null)
                            dailies[i].Dispose();
                    }
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
                    SelectTab(DailyType.Today);
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
                NativeMethods.ShowWindow(this.Handle, ShowWindowCommands.ShowNoActivate);

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

            if (alignment == HorizontalAlignment.Right)
            {
                buttonToday.Left = this.Width - buttonToday.Right;
                buttonTomorrow.Left = this.Width - buttonTomorrow.Right;
                buttonMinimize.Left = this.Width - buttonMinimize.Right;
                scrollV.Left = this.Width - scrollV.Right;
                panelContainer.Left = this.Width - panelContainer.Right;

                buttonMinimize.ShapeAlignment = ContentAlignment.MiddleRight;
                buttonMinimize.ShapeDirection = ArrowDirection.Left;
            }
            else
            {
                buttonToday.Left = this.Width - buttonToday.Right;
                buttonTomorrow.Left = this.Width - buttonTomorrow.Right;
                buttonMinimize.Left = this.Width - buttonMinimize.Right;
                scrollV.Left = this.Width - scrollV.Right;
                panelContainer.Left = this.Width - panelContainer.Right;

                buttonMinimize.ShapeAlignment = ContentAlignment.MiddleLeft;
                buttonMinimize.ShapeDirection = ArrowDirection.Right;
            }
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
                    minimized.Show(parent);
            }

            if (focus)
                minimized.Focus();
        }

        void minimized_VisibleChanged(object sender, EventArgs e)
        {
            if (minimized.Visible && this.Visible)
            {
                NativeMethods.ShowWindow(minimized.Handle, ShowWindowCommands.ShowNoActivate);
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
                    
                    button.ShapeSize = new Size(5, 9);

                    break;
                case FlatShapeButton.IconShape.Ellipse:

                    button.ShapeSize = new Size(6, 6);

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
            if (waitingToMinimize || !this.DesktopBounds.Contains(Cursor.Position))
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
                        p = this.PointToClient(new Point(m.LParam.ToInt32()));

                        if (p.Y > buttonTomorrow.Bottom && p.Y < buttonMinimize.Top)
                        {
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
                        }
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
                        parent.BringToFront();
                        this.Focus();
                    }

                    break;
                case WindowMessages.WM_EXITSIZEMOVE:

                    base.WndProc(ref m);

                    sizing = false;

                    var l = (this.Left == parent.Right + padding || this.Right == parent.Left - padding);
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
    }
}
