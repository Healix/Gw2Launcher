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
                if (!Settings.Dailies.Options.Value.HasFlag(Settings.DailiesOptions.Positioned))
                    owner.Show(this.ContainsFocus || parent.ContainsFocus);
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
                        parent.Focus();
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

        private class AccountsPopup : Base.BaseForm
        {
            private class Arrow : Base.BaseForm
            {
                public Arrow()
                {
                    InitializeComponents();
                }

                protected override void OnInitializeComponents()
                {
                    this.Opacity = 0;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    this.ShowInTaskbar = false;
                    this.StartPosition = FormStartPosition.Manual;
                    this.ForeColorName = UiColors.Colors.DailiesHeader;
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

                protected override void WndProc(ref Message m)
                {
                    base.WndProc(ref m);

                    if (m.Msg == (int)WindowMessages.WM_NCHITTEST)
                    {
                        m.Result = (IntPtr)HitTest.Transparent;
                    }
                }

                protected override void OnShown(EventArgs e)
                {
                    base.OnShown(e);

                    this.Size = Scale(11, 22);
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    using (var b = new SolidBrush(this.ForeColor))
                    {
                        var g = e.Graphics;
                        var w = this.Width;
                        var h = this.Height / 2;

                        Point[] points;

                        if (_RightArrow)
                        {
                            points = new Point[]
                            {
                                new Point(0,0),
                                new Point(h,h),
                                new Point(0,h*2)
                            };
                        }
                        else
                        {
                            points = new Point[]
                            {
                                new Point(w, 0),
                                new Point(w-h,h),
                                new Point(w,h*2)
                            };
                        }

                        g.FillPolygon(b, points);
                    }
                }

                private bool _RightArrow;
                public bool RightArrow
                {
                    get
                    {
                        return _RightArrow;
                    }
                    set
                    {
                        if (_RightArrow != value)
                        {
                            _RightArrow = value;
                            this.Invalidate();
                        }
                    }
                }

                public override void RefreshColors()
                {
                    base.RefreshColors();

                    var c = this.ForeColor;

                    c = Color.FromArgb(c.R, c.G, c.B > 0 ? c.B - 1 : c.B + 1);

                    this.BackColor = c;
                    this.TransparencyKey = c;
                }
            }

            private Util.ReusableControls reusable;
            private DailyAchievement attached;
            private Rectangle screen;
            private byte counter;
            private Arrow arrow;
            private Control[] group;
            private Watched.WatchedGroup wa;
            private Settings.IAccount[] accounts;
            private DailyAchievement[] controls;

            public AccountsPopup()
            {
                InitializeComponents();

                arrow = new Arrow();
            }

            protected override void OnInitializeComponents()
            {
                this.Opacity = 0;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.Manual;
                this.BackColorName = UiColors.Colors.DailiesHeader;
                this.ForeColorName = UiColors.Colors.DailiesText;
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
                    if (!arrow.Visible)
                    {
                        arrow.Show(this);
                    }

                    FadeIn();
                }
                else
                {
                    if (wa != null)
                    {
                        wa.popup = false;
                    }

                    this.Opacity = 0;
                    arrow.Opacity = 0;
                    arrow.Visible = false;
                }

                base.OnVisibleChanged(e);
            }

            private async void FadeIn()
            {
                const int DELAY = 100;
                const int DURATION = 100;
                const float MAX_OPACITY = 0.98f;

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
                        this.Opacity = MAX_OPACITY;
                        arrow.Opacity = MAX_OPACITY;

                        this.VisibleChanged -= onVisible;

                        break;
                    }
                    else
                    {
                        p *= MAX_OPACITY;

                        this.Opacity = p;
                        arrow.Opacity = p;
                    }

                    await Task.Delay(10);
                }
            }

            public bool SetData(Settings.IAccount[] accounts, bool[] claimed, ushort[] progress, ushort max, Tools.Api.VaultObjectives.RefreshStatus[] requests = null)
            {
                if (reusable == null)
                    reusable = new Util.ReusableControls();
                else
                    reusable.ReleaseAll();

                var controls = reusable.CreateOrAll<DailyAchievement>(accounts.Length, new Func<DailyAchievement>(
                    delegate
                    {
                        var control = new DailyAchievement()
                        {
                            BackColorName = UiColors.Colors.DailiesBackColor,
                            NameVisible = true,
                            NameFont = this.Font,
                            Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        };

                        return control;
                    }));

                this.accounts = accounts;
                this.Requests = requests;
                this.controls = new DailyAchievement[accounts.Length];

                this.SuspendLayout();

                var border = Scale(3);
                var sz = new Size(Scale(250) - border * 2, Scale(30));
                var y = border;

                for (var i = 0; i < accounts.Length; i++)
                {
                    if (accounts[i] == null)
                    {
                        break;
                    }

                    var d = controls.GetNext();

                    this.controls[i] = d;

                    d.ProgressVisible = true;
                    d.NameValue = accounts[i].Name;
                    d.ColorKey = accounts[i].ColorKey;
                    d.FavSize = Scale(14, 12);
                    d.SetPending(requests != null ? requests[i] : null);

                    if (claimed[i])
                    {
                        d.ProgressClaimed=true;
                    }
                    else if (progress[i] == 0 || max == 0)
                    {
                        d.ProgressValue = 0;
                    }
                    else if (progress[i] == max)
                    {
                        d.ProgressValue = 1;
                    }
                    else
                    {
                        d.ProgressValue = (float)progress[i] / max;
                    }

                    d.Bounds = new Rectangle(border, y, sz.Width, sz.Height);
                    d.Visible = true;

                    y += sz.Height;
                }

                controls.HideAndAdd(this);

                this.Size = new Size(Scale(250), y + border);

                this.ResumeLayout();

                return y > border;
            }

            public void AttachTo(DailyAchievement c, Control[] controls = null, Watched.WatchedGroup wa = null)
            {
                if (this.wa != wa)
                {
                    if (this.wa != null)
                    {
                        this.wa.popup = false;
                    }

                    this.wa = wa;

                    if (wa != null)
                    {
                        wa.popup = true;
                    }
                }

                if (attached == c)
                {
                    return;
                }

                if (attached != null)
                {
                    attached.MouseMove -= attached_MouseMove;
                    attached.MouseLeave -= attached_MouseLeave;
                }

                if (c != null)
                {
                    c.ForeColor = Color.Empty;

                    if (attached != null)
                    {
                        attached.ForeColor = Util.Color.Gradient(c.ForeColor, c.BackColor, 0.6f);
                    }
                }

                if (controls != group)
                {
                    if (group != null)
                    {
                        for (var i = 0; i < group.Length; i++)
                        {
                            group[i].ForeColor = Color.Empty;
                        }
                    }

                    group = controls;

                    if (controls != null)
                    {
                        var color = Util.Color.Gradient(c.ForeColor, c.BackColor, 0.6f);

                        for (var i = 0; i < controls.Length; i++)
                        {
                            if (controls[i] != c)
                            {
                                controls[i].ForeColor = color;
                            }
                        }
                    }
                }

                attached = c;

                if (c != null)
                {
                    c.MouseMove += attached_MouseMove;
                    c.MouseLeave += attached_MouseLeave;

                    this.screen = Screen.FromControl(c).Bounds;

                    if (this.Height > screen.Height)
                    {
                        this.Height = screen.Height;
                    }

                    MoveToCursor();
                }

                ++counter;
            }

            private void MoveToCursor()
            {
                var p = Cursor.Position;
                var cw = Cursor.Size.Width;
                var ap = attached.PointToScreen(Point.Empty);

                int x = p.X + cw,
                    y = p.Y - this.Height / 2,
                    x2, y2;

                if (p.X < ap.X + attached.Width / 3 && p.X - cw - this.Width > screen.X || x + this.Width > screen.Right)
                {
                    x2 = p.X - cw;
                    x = x2 - this.Width;
                    arrow.RightArrow = true;
                }
                else
                {
                    x2 = x - arrow.Width;
                    arrow.RightArrow = false;
                }

                if (y < screen.Top)
                {
                    y = screen.Top;
                }
                else if (y + this.Height > screen.Bottom)
                {
                    y = screen.Bottom - this.Height;
                }

                y2 = attached.PointToScreen(Point.Empty).Y + attached.Height / 2 - arrow.Height / 2;

                if (y2 < y)
                {
                    y2 = y;
                }
                else if (y2 + arrow.Height > y + this.Height)
                {
                    y2 = y + this.Height - arrow.Height;
                }

                this.Location = new Point(x, y);
                arrow.Location = new Point(x2, y2);
            }

            private async void DelayedHide()
            {
                var c = counter;

                await Task.Delay(50);

                if (counter == c)
                {
                    this.Visible = false;
                    this.AttachTo(null);
                }
            }

            void attached_MouseLeave(object sender, EventArgs e)
            {
                DelayedHide();
            }

            void attached_MouseMove(object sender, MouseEventArgs e)
            {
                MoveToCursor();
            }

            public DailyAchievement Attached
            {
                get
                {
                    return attached;
                }
            }

            public Settings.DailiesItemKey Group
            {
                get;
                set;
            }

            public Settings.IAccount[] Accounts
            {
                get
                {
                    return accounts;
                }
            }

            public Tools.Api.VaultObjectives.RefreshStatus[] Requests
            {
                get;
                private set;
            }

            public DateTime Date
            {
                get;
                set;
            }
        }

        private class ApiRequest : Tools.Api.ApiRequestManager.DataRequest
        {
            public ApiRequest(ApiData.DataType type, Settings.IAccount account, Settings.ApiDataKey key, RequestOptions options = RequestOptions.None)
                : base(type, account, key, options)
            {

            }

            public Watched.WatchedGroup Watched
            {
                get;
                set;
            }

            public Watched.AccountData AccountData
            {
                get;
                set;
            }

            public bool ForceRepeat
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

            public ItemGroup GetGroupFromAccount(Settings.IAccount a)
            {
                if (a != null && groups != null && (type & DataType.Vault) != 0)
                {
                    for (var i = 0; i < groups.Length; i++)
                    {
                        var accounts = groups[i].accounts;

                        if (accounts != null)
                        {
                            for (var j = 0; j < accounts.Length; j++)
                            {
                                if (accounts[j] == a)
                                {
                                    return groups[i];
                                }
                            }
                        }

                    }
                }

                return null;
            }
        }

        private class ItemGroup
        {
            public Settings.DailiesItemKey id;
            public ushort count;
            public ushort index;
            public ushort accountid;
            public object source;
            public Settings.IAccount[] accounts;
            public IData[] items;
            public DailyCategoryBar bar;
            public AccountSquares squares;
            public LastUpdatedLabel updated;
            public Label unavailable;
            public DailyAchievement[] controls;
            public Daily.Category category;
            public Watched.WatchedGroup watched;
            public bool collapsed;
            public DateTime focused;

            public Settings.IAccount GetSelectedAccount()
            {
                if (squares != null)
                {
                    return squares.Selected;
                }
                else if (accounts != null && accounts.Length > 0)
                {
                    return accounts[0];
                }
                else
                {
                    return null;
                }
            }

            public bool IsHidden()
            {
                return count == 0 && unavailable == null;
            }

            public Control GetBottomControl()
            {
                Control c;

                if (collapsed)
                {
                    c = bar;
                }
                else if (updated != null && updated.Enabled)
                {
                    c = updated;
                }
                else if (controls != null && count > 0)
                {
                    c = controls[count - 1];
                }
                else if (unavailable != null)
                {
                    c = unavailable;
                }
                else
                {
                    c = bar;
                }

                return c;
            }
        }

        private struct ObjectiveGroupData
        {
            public Tools.Api.VaultObjectives.ObjectivesGroup data;
            public Settings.IAccount[] accounts;
            public byte count;
            public byte index;
            /// <summary>
            /// Account index + 1; 0 is none
            /// </summary>
            public ushort selected;
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
                else if (m > 15)
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

        [Flags]
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

            public ObjectiveDataSource(Tools.Api.VaultObjectives.ObjectiveData source, ItemGroup group, byte sourceIndex)
            {
                this.Source = source;
                this.Group = group;
                this.SourceIndex = sourceIndex;
            }

            public Tools.Api.VaultObjectives.ObjectiveData Source
            {
                get;
                set;
            }

            public ItemGroup Group
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

            public byte SourceIndex
            {
                get;
                private set;
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

            public Settings.DailiesItemOptions Options
            {
                get;
                set;
            }

            public bool Favorite
            {
                get
                {
                    return Options == Settings.DailiesItemOptions.Favorite;
                }
                set
                {
                    if (value)
                    {
                        Options = Settings.DailiesItemOptions.Favorite;
                    }
                    else if (Options == Settings.DailiesItemOptions.Favorite)
                    {
                        Options = Settings.DailiesItemOptions.None;
                    }
                }
            }

            public bool IsNew
            {
                get;
                set;
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

            public bool IsNew
            {
                get;
                set;
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

            public Settings.DailiesItemOptions Options
            {
                get;
                set;
            }

            public bool Favorite
            {
                get
                {
                    return Options == Settings.DailiesItemOptions.Favorite;
                }
                set
                {
                    if (value)
                    {
                        Options = Settings.DailiesItemOptions.Favorite;
                    }
                    else if (Options == Settings.DailiesItemOptions.Favorite)
                    {
                        Options = Settings.DailiesItemOptions.None;
                    }
                }
            }
        }

        private class Watched
        {
            public class WatchedGroup
            {
                public WatchedGroup(AccountData a)
                {
                    this.accountdata = a;
                }

                public AccountData accountdata;

                public bool watched;
                //public ApiRequest request;
                public ItemGroup group;
                public bool popup;
                public bool requested;
                public byte focusedKey;

                public bool GetGroup(DataGroup data, out ItemGroup g)
                {
                    if (group != null)
                    {
                        g = data.GetGroupFromIndex(group.index);

                        if (object.ReferenceEquals(group, g))
                        {
                            return g.watched == this;
                        }
                    }

                    g = null;
                    return false;
                }

                public void Abort()
                {
                    var r = accountdata.request;

                    if (r != null)
                    {
                        accountdata.request = null;
                        r.Abort();
                    }
                }

                public bool IsAccount(AccountData a)
                {
                    return object.ReferenceEquals(this.accountdata, a);
                }

                public bool IsAccount(Settings.IAccount a)
                {
                    return object.ReferenceEquals(this.accountdata.account, a);
                }

                public void SetAccount(AccountData a, bool abort = true)
                {
                    if (!object.ReferenceEquals(accountdata, a))
                    {
                        if (abort)
                        {
                            Abort();
                        }
                        accountdata = a;
                    }
                }

                public void Dispose()
                {
                    if (accountdata != null)
                    {
                        Abort();
                        accountdata = null;
                    }
                    group = null;
                }
            }

            public class AccountData
            {
                public AccountData(Settings.IGw2Account a)
                {
                    this.account = a;
                }

                public readonly Settings.IGw2Account account;
                public DateTime date;
                public float position;
                public ApiRequest request;
            }

            private Dictionary<Settings.DailiesItemKey, WatchedGroup> watched;
            private Dictionary<ushort, AccountData> accounts;

            public Watched()
            {
                watched = new Dictionary<Settings.DailiesItemKey, WatchedGroup>();
                accounts = new Dictionary<ushort, AccountData>();
            }

            public AccountData GetAccount(Settings.IGw2Account a)
            {
                AccountData d;

                if (!accounts.TryGetValue(a.UID, out d))
                {
                    accounts[a.UID] = d = new AccountData(a);
                }

                return d;
            }

            public bool Remove(Settings.DailiesItemKey k)
            {
                return watched.Remove(k);
            }

            public void Add(Settings.DailiesItemKey k, WatchedGroup g)
            {
                watched[k] = g;
            }

            public WatchedGroup Add(Settings.DailiesItemKey k, Settings.IGw2Account a)
            {
                WatchedGroup g;

                watched[k] = g = new WatchedGroup(GetAccount(a));

                return g;
            }

            public WatchedGroup this[Settings.DailiesItemKey k]
            {
                get
                {
                    WatchedGroup g;
                    watched.TryGetValue(k, out g);
                    return g;
                }
                set
                {
                    if (value != null)
                    {
                        watched[k] = value;
                    }
                    else
                    {
                        watched.Remove(k);
                    }
                }
            }

            public AccountData this[Settings.IGw2Account a]
            {
                get
                {
                    return GetAccount(a);
                }
            }

            public bool TryGetValue(Settings.DailiesItemKey k, out WatchedGroup g)
            {
                return watched.TryGetValue(k, out g);
            }

            public void Purge()
            {
                this.accounts.Clear();

                foreach (var w in this.watched.Values)
                {
                    this.accounts[w.accountdata.account.UID] = w.accountdata;
                }
            }
        }

        private class TabData
        {
            public TabData()
            {
            }

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

        public class formDailiesVault : formDailies
        {
            public formDailiesVault(formDailies parent, Tools.Api.VaultObjectives vob)
                : base(parent, vob)
            {

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
        private DataType displayedTabs, enabledTabs;
        private DataType currentTab, loadedTab;
        private Daily.Achievements dailies;
        private Tools.Api.VaultObjectives vob;
        private TabData[] tabs;
        private Popup popup;
        private AccountsPopup popupObjectives;
        private Daily da;
        private Image[] defaultImages;
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
        private HashSet<ushort> specials;
        private Settings.IAccount focused;
        private byte focusedKey;
        private Watched watched;

        private Form parent;
        private formDailies child;

        public formDailies(Form parent, Tools.Api.ApiRequestManager apiManager)
        {
            InitializeComponents();
            RemoveInvalidItemKeys();

            this.parent = parent;
            this.vob = new Tools.Api.VaultObjectives(apiManager);
            this.watched = new Watched();

            tabs = new TabData[]
            {
                new TabData(),
                new TabData(),
                new TabData(),
                new TabData(),
            };

            if (parent.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None || !NativeMethods.IsDwmCompositionEnabled())
                padding = 5;

            //padding = (parent.Width - parent.ClientSize.Width) / 2;
            //if (padding > 5)
            //    padding = 0;
            //else
            //    padding = 5;

            defaultImages = new Image[3];
            defaultImages[0] = Properties.Resources.icon42684;
            if (Settings.Dailies.DailyCategories.HasValue)
                categories = Settings.Dailies.DailyCategories.Value;
            else
                categories = Daily.GetDefaultCategories();

            popup = new Popup(defaultImages[0]);
            popup.Owner = this;

            da = new Daily();

            Settings.Dailies.DailyCategories.ValueChanged += Categories_ValueChanged;
            Settings.Dailies.Options.ValueChanged += DailiesSettings_ValueChanged;

            Init(DataType.Daily | DataType.Vault);
            displayedTabs = enabledTabs;

            if (Settings.Dailies.KnownVaultSpecials.HasValue)
            {
                var seen = Settings.Dailies.KnownVaultSpecials.Value.Seen;
                var latest = Settings.Dailies.KnownVaultSpecials.Value.Latest;

                if (seen != null && latest != null)
                {
                    var sum = seen.Length - latest.Length;

                    if (sum == 0)
                    {
                        for (var i = 0; i < seen.Length; i++)
                        {
                            sum += seen[i];
                            sum -= latest[i];
                        }
                    }

                    if (sum != 0)
                    {
                        specials = new HashSet<ushort>(seen);
                        buttonSpecial.TopTag.Visible = true;
                    }
                }
            }

            Client.Launcher.AllQueuedLaunchesCompleteAllAccountsExited += Launcher_AllQueuedLaunchesCompleteAllAccountsExited;

            if (Settings.Dailies.Options.Value.HasFlag(Settings.DailiesOptions.AutoLoad))
                SelectTab(GetDefaultTab());
            else
                loadOnShow = true;
        }

        private formDailies(formDailies parent, Tools.Api.VaultObjectives vob)
        {
            InitializeComponents();

            this.parent = parent;
            this.vob = vob;
            this.tabs = parent.tabs;
            this.defaultImages = parent.defaultImages;
            this.watched = parent.watched;

            Init(DataType.Vault);

            SetTabs(enabledTabs);
            splitVaultObjectivesToolStripMenuItem.Checked = true;

        }

        private void Init(DataType type)
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            enabledTabs = type;
            reposition = int.MaxValue;
            alignment = HorizontalAlignment.Right;

            this.Opacity = 0;
            this.KeyPreview = true;

            panelContent.BackColor = UiColors.GetColor(UiColors.Colors.DailiesSeparator);

            fontBar = new System.Drawing.Font("Segoe UI Semibold", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fontName = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fontDescription = new System.Drawing.Font("Segoe UI Semilight", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fontUpdated = new System.Drawing.Font("Segoe UI", 7f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            if ((type & DataType.Vault) != 0)
            {
                var fontTag = new System.Drawing.Font("Segoe UI", 7f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                buttonVault.BottomTag = new FlatVerticalButton.TagData()
                {
                    BackColor = Color.Gray,
                    ForeColor = Color.Black,
                    Font = fontTag,
                };

                buttonWeekly.BottomTag = new FlatVerticalButton.TagData()
                {
                    BackColor = Color.Gray,
                    ForeColor = Color.Black,
                    Font = fontTag,
                };

                buttonSpecial.TopTag = new FlatVerticalButton.TagData()
                {
                    BackColor = Color.Gold,
                    ForeColor = Color.Black,
                    Font = fontTag,
                    Text = "new",
                };

                vob.DataChanged += vob_DataChanged;
                vob.AccountDataChanged += vob_AccountDataChanged;

                Client.Launcher.AccountWindowEvent += Launcher_AccountWindowEvent;
                Settings.Dailies.VaultOptions.ValueChanged += VaultSettings_ValueChanged;
                Settings.Dailies.VaultSorting.ValueChanged += VaultSorting_ValueChanged;
                Settings.Dailies.VaultObjectiveSorting.ValueChanged += VaultObjectiveSorting_ValueChanged;

                VaultSettings_ValueChanged(Settings.Dailies.VaultOptions, EventArgs.Empty);
                VaultSorting_ValueChanged(Settings.Dailies.VaultSorting, EventArgs.Empty);
                VaultObjectiveSorting_ValueChanged(Settings.Dailies.VaultObjectiveSorting, EventArgs.Empty);
            }

            if ((type & DataType.Daily) != 0)
            {
            }

            Settings.Dailies.Language.ValueChanged += Language_ValueChanged;

            panelContainer.MouseWheel += panelContainer_MouseWheel;
            panelContainer.MouseHover += panelContainer_MouseHover;
            this.MouseWheel += panelContainer_MouseWheel;
            parent.VisibleChanged += parent_VisibleChanged;

            Client.Launcher.MumbleLinkVerified += Launcher_MumbleLinkVerified;
            Client.Launcher.CefSessions.SessionEvent += CefSessions_SessionEvent;
            Client.Launcher.AccountExited += Launcher_AccountExited;

            if (IsMainWindow)
            {
                DailiesSettings_ValueChanged(Settings.Dailies.Options, EventArgs.Empty);
            }

            var now = DateTime.UtcNow;
            Util.ScheduledEvents.Register(OnScheduledBeforeDailyReset, Util.Date.GetNextDay(now).AddHours(-1));
            Util.ScheduledEvents.Register(OnScheduledBeforeWeeklyReset, Util.Date.GetNextWeek(now).AddHours(-24));
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            buttonToday.MinimumSize = new Size(0, buttonTomorrow.Height);
        }

        public bool IsMainWindow
        {
            get
            {
                return !(parent is formDailies);
            }
        }

        void parent_VisibleChanged(object sender, EventArgs e)
        {
            if (linkedToParent || !IsMainWindow)
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

        void Launcher_AllQueuedLaunchesCompleteAllAccountsExited(object sender, EventArgs e)
        {
            lock (watched)
            {
                watched.Purge();
            }
        }

        void VaultSettings_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<Settings.DailiesVaultOptions>)sender;
            var o = v.Value;

            if ((o & Settings.DailiesVaultOptions.Split) == 0 && child != null)
            {
                ShowSplitVaultObjectives(false);
            }

            autoScrollToCurrentAccountToolStripMenuItem.Checked = (o & Settings.DailiesVaultOptions.AutoScroll) != 0;
            autoSelectCurrentAccountToolStripMenuItem.Checked = (o & Settings.DailiesVaultOptions.AutoSelect) != 0;
        }

        void VaultSorting_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<Settings.DailiesVaultSorting>)sender;
            var o = v.Value;
            var s = o & Settings.DailiesVaultSorting.Sorting;

            descendingToolStripMenuItem.Checked = (o & Settings.DailiesVaultSorting.Descending) != 0;
            groupToolStripMenuItem.Checked = s == Settings.DailiesVaultSorting.Group;
            focusedToolStripMenuItem.Checked = s == Settings.DailiesVaultSorting.Focused;
            accountToolStripMenuItem.Checked = s == Settings.DailiesVaultSorting.Account;
        }

        void VaultObjectiveSorting_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<Settings.DailiesVaultObjectiveSorting>)sender;
            var o = v.Value;

            objectiveDescendingToolStripMenuItem.Checked = (o & Settings.DailiesVaultObjectiveSorting.Descending) != 0;
            objectiveIdToolStripMenuItem.Checked = (o & Settings.DailiesVaultObjectiveSorting.ID) != 0;
            objectiveNameToolStripMenuItem.Checked = (o & Settings.DailiesVaultObjectiveSorting.Name) != 0;
            objectiveProgressToolStripMenuItem.Checked = (o & Settings.DailiesVaultObjectiveSorting.Progress) != 0;
        }

        void DailiesSettings_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<Settings.DailiesOptions>)sender;
            var o = v.Value;

            if (!v.HasValue || (o & Settings.DailiesOptions.Show) == 0)
            {
                if (this.IsHandleCreated)
                {
                    this.Dispose();
                }
                return;
            }

            var bounds = Settings.WindowBounds[typeof(formDailies)];
            var positioned = (o & Settings.DailiesOptions.Positioned) != 0;

            if ((o & Settings.DailiesOptions.AutoLoadFavorite) != 0)
                showOnLoad = ShowOnLoadOptions.Favorite;
            else if ((o & Settings.DailiesOptions.AutoLoad) != 0)
                showOnLoad = ShowOnLoadOptions.Always;
            else
                showOnLoad = ShowOnLoadOptions.None;

            AutoMinimize = !positioned;

            showOnTopToolStripMenuItem.Checked = (o & Settings.DailiesOptions.TopMost) != 0;
            TopMost = positioned && (o & Settings.DailiesOptions.TopMost) != 0;

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
            var categories = this.categories = Settings.Dailies.DailyCategories.Value;
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
                ShowMenu(DataType.Daily);
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
                ShowMenu(DataType.Daily);
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

        private void CreateObjectivesPopup(DailyAchievement control)
        {
            var d = (ObjectiveDataSource)control.DataSource;
            var ok = false;

            if (d.Group.source is ObjectiveGroupData)
            {
                var ogd = (ObjectiveGroupData)d.Group.source;
                var og = ogd.data;
                var _accounts = ogd.accounts;

                if (_accounts.Length > 0)
                {
                    var changed = true;
                    var canRequest = true;

                    Settings.IAccount[] accounts = null;
                    Tools.Api.VaultObjectives.RefreshStatus[] requests = null;

                    if (popupObjectives != null)
                    {
                        accounts = popupObjectives.Accounts;
                        requests = popupObjectives.Requests;

                        changed = !object.ReferenceEquals(accounts, _accounts);

                        //if (popupObjectives.Group == d.Group.id && accounts != null && accounts.Length == _accounts.Length)
                        //{
                        //    changed = false;

                        //    for (var i = 0; i < accounts.Length; i++)
                        //    {
                        //        if (_accounts[i] != accounts[i])
                        //        {
                        //            changed = true;

                        if (requests != null)
                        {
                            for (var i = 0; i < requests.Length; i++)
                            {
                                if (requests[i] != null)
                                {
                                    if (changed)
                                    {
                                        requests[i].Abort();
                                        requests[i].Dispose();
                                    }
                                    else if (requests[i].IsComplete)
                                    {
                                        requests[i] = null;
                                    }
                                }
                            }
                        }
                    }

                    if (changed)
                    {
                        //accounts = new Settings.IAccount[_accounts.Length];
                        accounts = _accounts;
                        requests = new Tools.Api.VaultObjectives.RefreshStatus[accounts.Length];

                        //Array.Copy(_accounts, accounts, accounts.Length);
                    }
                    else
                    {
                        canRequest = requests != null && DateTime.UtcNow.Subtract(popupObjectives.Date).TotalMinutes > 1;
                    }

                    var claimed = new bool[accounts.Length];
                    var progress = new ushort[accounts.Length];

                    var objs = og.Objectives;
                    var oi = -1;
                    var count = 0;

                    if (d.SourceIndex < objs.Length && objs[d.SourceIndex].ID == d.Source.ID)
                    {
                        oi = d.SourceIndex;
                    }
                    else
                    {
                        for (var i = 0; i < objs.Length; i++)
                        {
                            if (objs[i].ID == d.Source.ID)
                            {
                                oi = i;

                                break;
                            }
                        }
                    }

                    for (; count < accounts.Length; count++)
                    {
                        var ao = vob.GetObjectives(accounts[count]);

                        if (ao != null)
                        {
                            var o = ao.GetObjective(og.Type, d.Source.ID, oi);

                            if (canRequest && (requests[count] == null || requests[count].IsComplete) && DateTime.UtcNow.Subtract(ao.GetDate(og.Type)).TotalMinutes > 5)
                            {
                                var b = ao.IsPending(og.Type);

                                if (!b && !ao.IsComplete(og.Type) && Client.Launcher.GetState(accounts[count]) == Client.Launcher.AccountState.ActiveGame)
                                {
                                    ao.SetPending(og.Type, true);
                                    b = true;
                                }

                                if (b)
                                {
                                    requests[count] = vob.Refresh(og.Type, (Settings.IGw2Account)accounts[count], Tools.Api.VaultObjectives.RefreshOptions.None | Tools.Api.VaultObjectives.RefreshOptions.Update);
                                }
                            }

                            if (o != null)
                            {
                                claimed[count] = o.Claimed;
                                progress[count] = o.ProgressCurrent;
                            }
                        }
                    }

                    if (count > 0)
                    {
                        if (popupObjectives == null || popupObjectives.IsDisposed)
                        {
                            popupObjectives = new AccountsPopup()
                            {
                                TopMost = true,
                            };
                        }

                        if (canRequest)
                        {
                            popupObjectives.Date = DateTime.UtcNow;
                        }
                        popupObjectives.Group = d.Group.id;

                        if (popupObjectives.SetData(accounts, claimed, progress, d.Source.ProgressComplete, requests))
                        {
                            var wa = d.Group.watched;

                            if (wa == null)
                            {
                                OnWatchedChanged(d.Group.bar);
                                wa = d.Group.watched;
                            }
                            
                            popupObjectives.AttachTo(control, d.Group.controls, wa);

                            if (!popupObjectives.Visible)
                            {
                                popupObjectives.Show(this);
                            }

                            ok = true;
                        }
                        else
                        {
                            popupObjectives.AttachTo(null);
                        }
                    }
                }
            }

            if (!ok && popupObjectives != null && popupObjectives.Visible)
            {
                popupObjectives.Visible = false;
            }
        }

        void control_MouseClick(object sender, MouseEventArgs e)
        {
            var control = (DailyAchievement)sender;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if ((currentTab & DataType.Vault) != 0 && control.DataSource is ObjectiveDataSource)
                {
                    if (popupObjectives == null || !popupObjectives.Visible)
                    {
                        CreateObjectivesPopup(control);
                    }
                    else
                    {
                        popupObjectives.Visible = false;
                        popupObjectives.AttachTo(null);
                    }
                }
                else if (control.DataSource is AchievementDataSource)
                {
                    var d = (AchievementDataSource)control.DataSource;
                    var groups = this.data.groups;
                    var b = !control.FavSelected;
                    var k = new Settings.DailiesItemKey(Settings.DailiesKeyType.DailyObjective, d.ID);

                    popup.Control.FavSelected = b;
                    d.Favorite = b;
                    if (b)
                    {
                        Settings.Dailies.ItemOptions[k] = Settings.DailiesItemOptions.Favorite;
                    }
                    else
                    {
                        Settings.Dailies.ItemOptions.Remove(k);
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
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenuDaily.Tag = control;
                contextMenuDaily.Show(Cursor.Position);
            }
        }

        void control_MouseLeave(object sender, EventArgs e)
        {
            if (popup != null && popup.Visible)
                popup.Hide();
        }

        void control_MouseEnter(object sender, EventArgs e)
        {
            var control = (DailyAchievement)sender;

            if ((currentTab & DataType.Vault) != 0 && control.DataSource is ObjectiveDataSource)
            {
                if (popupObjectives != null && popupObjectives.Visible)
                {
                    CreateObjectivesPopup(control);
                }
            }
            else if (control.DataSource != null && !string.IsNullOrEmpty(control.DataSource.Description))
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

        private async void RefreshDailies(bool clearCache, bool refresh = true, bool reload = false, bool silent = false)
        {
            switch (currentTab)
            {
                case DataType.DailyToday:
                case DataType.DailyTomorrow:

                    await da.Reset(clearCache);

                    break;
            }

            if (!this.Visible)
            {
                loadOnShow = true;
                loadedTab = DataType.None;
            }
            else
                GetData(currentTab, refresh, reload, silent);
        }

        private void ReloadDailies()
        {
            RefreshDailies(false, false, true, true);
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

            if ((Settings.Dailies.Options.Value & (Settings.DailiesOptions.AutoLoad | Settings.DailiesOptions.AutoLoadFavorite)) != 0)
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
                    SelectTab(GetDefaultTab());
                }
                else
                {
                    loadOnShow = value;
                }
            }
        }

        private DataType GetDefaultTab()
        {
            if ((displayedTabs & DataType.DailyToday) != 0)
            {
                return DataType.DailyToday;
            }
            else if ((displayedTabs & DataType.VaultDaily) != 0)
            {
                return DataType.VaultDaily;
            }

            return DataType.None;
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
                    buttonTomorrow.Visible = (displayedTabs & DataType.Vault) == 0;

                    panelTabs.ResumeLayout();

                    break;
                case DataType.DailyTomorrow:

                    button = buttonTomorrow;
                    
                    panelTabs.SuspendLayout();

                    buttonDaySwap.ShapeDirection = ArrowDirection.Left;
                    buttonTomorrow.Visible = true;
                    buttonToday.Visible = (displayedTabs & DataType.Vault) == 0;

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

                currentTab = type;

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
                        ForeColor = Util.Color.Gradient(UiColors.GetColor(UiColors.Colors.DailiesTextLight), panelContent.BackColor, 0.4f),
                        TextAlign = ContentAlignment.TopRight,
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                        BackColor = UiColors.GetColor(UiColors.Colors.DailiesBackColor),
                    };

                    control.EnabledChanged += updated_EnabledChanged;

                    return control;
                });
        }

        private Util.ReusableControls.IResult<Label> CreateUnavailableControls(int count)
        {
            return reusable.CreateOrAll<Label>(count,
                delegate
                {
                    var control = new Label()
                    {
                        AutoSize = false,
                        Size = new Size(panelContent.Width, Scale(50)),
                        Padding = new Padding(Scale(10), 0, 0, 0),
                        ForeColor = Util.Color.Gradient(UiColors.GetColor(UiColors.Colors.DailiesTextLight), panelContent.BackColor, 0.5f),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                        BackColor = UiColors.GetColor(UiColors.Colors.DailiesBackColor),
                        Text = "Unavailable",
                    };

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

        private Settings.DailiesItemKey GetKey(DataType type, ushort id, bool objective)
        {
            Settings.DailiesKeyType k;

            switch (type)
            {
                case DataType.DailyToday:
                case DataType.DailyTomorrow:

                    k = objective ? Settings.DailiesKeyType.DailyObjective : Settings.DailiesKeyType.DailyCategory;

                    break;
                case DataType.VaultDaily:

                    if (combineAccountsToolStripMenuItem.Checked)
                        k = Settings.DailiesKeyType.VaultDailyCategory;
                    else
                        k = Settings.DailiesKeyType.VaultDailyAccountCategory;

                    break;
                case DataType.VaultSpecial:

                    if (combineAccountsToolStripMenuItem.Checked)
                        k = Settings.DailiesKeyType.VaultSpecialCategory;
                    else
                        k = Settings.DailiesKeyType.VaultSpecialAccountCategory;

                    break;
                case DataType.VaultWeekly:

                    if (combineAccountsToolStripMenuItem.Checked)
                        k = Settings.DailiesKeyType.VaultWeeklyCategory;
                    else
                        k = Settings.DailiesKeyType.VaultWeeklyAccountCategory;

                    break;
                default:

                    k = Settings.DailiesKeyType.Invalid;

                    break;
            }

            return new Settings.DailiesItemKey(k, id);
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

                                defaultImages[1] = img;

                                break;
                            case 13:

                                defaultImages[2] = img;

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

                                defaultImages[1] = img;

                                break;
                            case 338457:

                                defaultImages[2] = img;

                                break;
                        }
                    }
                }
            }

            if (this.data != null && (this.data.type & DataType.Vault) != 0)
            {
                foreach (var g in this.data.groups)
                {
                    for (var i = 0; i < g.count; i++)
                    {
                        if (g.items[i] is ObjectiveDataSource)
                        {
                            switch (((ObjectiveDataSource)g.items[i]).Source.Type)
                            {
                                case Vault.ObjectiveType.PvP:

                                    g.controls[i].IconValue = defaultImages[1];

                                    break;
                                case Vault.ObjectiveType.WvW:

                                    g.controls[i].IconValue = defaultImages[2];

                                    break;
                                default:

                                    continue;
                            }
                        }
                    }
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
                barCount,
                labelCount = 0;

            var showIcon = true;
            ItemGroup gfav = null;
            var tab = GetTab(type);

            switch (type)
            {
                case DataType.DailyToday:
                case DataType.DailyTomorrow:

                    var dailies = (Daily.Achievements)data;

                    gfav = new ItemGroup()
                    {
                        id = GetKey(type, 0, false),
                    };
                    groupCount = categories.Length;
                    itemCount = dailies.Count;
                    barCount = dailies.Categories.Length;
                    
                    Settings.DailiesItemOptions o;
                    gfav.collapsed = Settings.Dailies.ItemOptions.TryGetValue(new Settings.DailiesItemKey(Settings.DailiesKeyType.DailyCategory, 0), out o) && (o & Settings.DailiesItemOptions.Collapsed) != 0;
                    break;
                case DataType.VaultDaily:
                case DataType.VaultWeekly:
                case DataType.VaultSpecial:

                    var ogdata = (Tools.Api.VaultObjectives.ObjectivesGroup[])data;
                    ObjectiveGroupData[] og;

                    if (combineAccountsToolStripMenuItem.Checked)
                    {
                        itemCount = 0;
                        barCount = 0;

                        og = new ObjectiveGroupData[ogdata.Length];

                        for (var i = 0; i < ogdata.Length; i++)
                        {
                            if (ogdata[i] != null)
                            {
                                var accounts = ogdata[i].GetAccounts();
                                if (accounts == null)
                                    continue;
                                var l = ogdata[i].Count;

                                og[barCount] = new ObjectiveGroupData()
                                {
                                    data = ogdata[i],
                                    index = (byte)i,
                                    count = (byte)l,
                                    accounts = accounts,
                                    selected = 0,
                                };

                                barCount++;

                                if (l > 0)
                                {
                                    itemCount += l;
                                }
                                else
                                {
                                    labelCount++;
                                }
                            }
                        }
                    }
                    else
                    {
                        barCount = 0;
                        itemCount = 0;

                        var _accounts = new Settings.IAccount[ogdata.Length][];

                        for (var i = 0; i < ogdata.Length; i++)
                        {
                            if (ogdata[i] != null)
                            {
                                _accounts[i] = ogdata[i].GetAccounts();

                                if (_accounts[i] != null)
                                {
                                    barCount += _accounts[i].Length;
                                }
                            }
                        }

                        og = new ObjectiveGroupData[barCount];
                        barCount = 0;

                        for (var i = 0; i < ogdata.Length; i++)
                        {
                            if (ogdata[i] != null)
                            {
                                var accounts = _accounts[i];
                                if (accounts == null)
                                    continue;
                                var l = ogdata[i].Count;

                                for (var j = 0; j < accounts.Length; j++)
                                {
                                    if (barCount == og.Length)
                                    {
                                        break;
                                    }

                                    og[barCount] = new ObjectiveGroupData()
                                    {
                                        data = ogdata[i],
                                        index = (byte)i,
                                        count = (byte)l,
                                        accounts = accounts,
                                        selected = (ushort)(j+1),
                                    };

                                    barCount++;

                                    if (l > 0)
                                    {
                                        itemCount += l;
                                    }
                                    else
                                    {
                                        labelCount++;
                                    }
                                }
                            }
                        }
                    }


                    //gfav.collapsed = false;

                    //Array.Sort<Tools.Api.VaultObjectives.ObjectivesGroup>(objs, 0, barCount, Comparer<Tools.Api.VaultObjectives.ObjectivesGroup>.Create(new Comparison<Tools.Api.VaultObjectives.ObjectivesGroup>(
                    //    delegate(Tools.Api.VaultObjectives.ObjectivesGroup a, Tools.Api.VaultObjectives.ObjectivesGroup b)
                    //    {
                    //        return a.ID.CompareTo(b.ID);
                    //    })));

                    groupCount = barCount;
                    data = og;

                    break;
                default:

                    return;
            }


            var items = CreateDailyControls(itemCount);
            var bars = CreateBarControls(barCount + 1);
            var groups = new ItemGroup[gfav == null ? groupCount : groupCount + 1];
            var dgroup = this.data = new DataGroup()
            {
                type = type,
                groups = groups,
                favorites = gfav,
            };
            var favorites = new List<IData>();

            lastIndex = groups.Length - 1;

            if (gfav != null)
            {
                if (gfav.collapsed)
                {
                    groups[lastIndex--] = gfav;
                }
                else
                {
                    groups[firstIndex++] = gfav;
                }
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
                        id = GetKey(type, id, false),
                        category = c,
                    };

                    Settings.DailiesItemOptions o;
                    if (Settings.Dailies.ItemOptions.TryGetValue(group.id, out o) && (o & Settings.DailiesItemOptions.Collapsed) != 0)
                    {
                        group.collapsed = true;
                    }

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

                        ushort i = 0;

                        foreach (var a in das)
                        {
                            if (!a.IsUnknown)
                            {
                                if (Settings.Dailies.ItemOptions.TryGetValue(new Settings.DailiesItemKey(Settings.DailiesKeyType.DailyObjective, a.ID), out o) && o == Settings.DailiesItemOptions.Ignored)
                                {
                                    continue;
                                }

                                var d = new AchievementDataSource(a)
                                {
                                    Options = o,
                                };

                                if (o == Settings.DailiesItemOptions.Favorite)
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
                var squares = CreateSquaresControls(combineAccountsToolStripMenuItem.Checked ? barCount : 0);
                var updated = CreateLastUpdatedControls(barCount);
                var unavailable = CreateUnavailableControls(labelCount);
                var hasNew = false;
                HashSet<ushort> known = null;

                if (type == DataType.VaultSpecial)
                {
                    if (Settings.Dailies.KnownVaultSpecials.HasValue)
                    {
                        if (specials != null)
                        {
                            known = specials;
                        }
                        else
                        {
                            var ids = Settings.Dailies.KnownVaultSpecials.Value.SeenOrLatest;
                            if (ids == null)
                                ids = new ushort[0];
                            known = new HashSet<ushort>(ids);
                        }
                    }
                    else
                    {
                        hasNew = true;
                    }
                }

                #region Vault

                var og = (ObjectiveGroupData[])data;
                var hasOtherIcons = false;
                var wcount = 0;
                var combined = combineAccountsToolStripMenuItem.Checked;

                HashSet<ushort> hfav = null;

                for (var i = 0; i < barCount; i++)
                {
                    var accounts = og[i].accounts;
                    var selected = accounts[og[i].selected > 0 ? og[i].selected - 1 : 0];
                    Settings.DailiesItemOptions o;
                    ItemGroup group;

                    if (combined)
                    {
                        group = new ItemGroup()
                        {
                            id = GetKey(type, og[i].data.ID,false),
                            accounts = accounts,
                            accountid = selected.UID,
                        };
                    }
                    else
                    {
                        group = new ItemGroup()
                        {
                            id = GetKey(type, selected.UID, false),
                            accounts = new Settings.IAccount[] { selected },
                            accountid = selected.UID,
                        };
                    }


                    var count = og[i].count;
                    var objs = og[i].data.Objectives;
                    var c = new Daily.Category()
                    {
                        ID = group.id.ID,
                    };

                    group.items = new IData[count];
                    group.category = c;
                    group.source = og[i];
                    group.collapsed = Settings.Dailies.ItemOptions.TryGetValue(group.id, out o) && (o & Settings.DailiesItemOptions.Collapsed) != 0;

                    for (var k = 0; k < count; k++)
                    {
                        if (objs[k].ID == 133)
                        {
                            //skipping login
                        }
                        else
                        {
                            var d = new ObjectiveDataSource(objs[k], group, (byte)k);

                            group.items[group.count++] = d;

                            switch (objs[k].Type)
                            {
                                case Vault.ObjectiveType.PvP:

                                    d.Icon = defaultImages[1];
                                    if (d.Icon == null)
                                    {
                                        hasOtherIcons = true;
                                    }

                                    break;
                                case Vault.ObjectiveType.WvW:

                                    d.Icon = defaultImages[2];
                                    if (d.Icon == null)
                                    {
                                        hasOtherIcons = true;
                                    }

                                    break;
                            }

                            if (known != null && !known.Contains(objs[k].ID))
                            {
                                d.IsNew = true;
                                hasNew = true;
                            }
                        }

                        

                    }

                    if (hasNew && type == DataType.VaultSpecial)
                    {
                        specials = known;

                    }

                    if (group.count > 0 || true)
                    {
                        var bar = group.bar = bars.GetNext();

                        bar.SetState(group.collapsed);
                        bar.ButtonEyeVisible = true;
                        bar.ButtonEyeTimerEnabled = false;
                        bar.DropDownItems = combined ? GetDropDownItems(accounts) : null;
                        bar.ButtonDropDownArrowVisible = bar.DropDownItems != null && bar.DropDownItems.Length > 1;

                        if (combined)
                            group.focused = Client.Launcher.GetLastFocused(accounts).Value;
                        else
                            group.focused = Client.Launcher.GetLastFocused(selected).Value;

                        var watched = false;
                        int index = -1;
                        //Settings.IAccount selected;

                        lock (this.watched)
                        {
                            Watched.WatchedGroup wa;

                            if (this.watched.TryGetValue(group.id, out wa))
                            {
                                if (combined)
                                {
                                    if (autoSelectCurrentAccountToolStripMenuItem.Checked && wa.focusedKey != focusedKey && focused != null && (index = GetDropDownItem(bar.DropDownItems, focused)) != -1)
                                    {
                                        selected = ((Util.ComboItem<Settings.IAccount>[])bar.DropDownItems)[index].Value;

                                        if (!wa.IsAccount(selected))
                                        {
                                            wa.SetAccount(this.watched.GetAccount((Settings.IGw2Account)selected));

                                            //wa.Abort();
                                            //this.watched[group.id] = wa = new Watched.WatchedGroup(this.watched.GetAccount((Settings.IGw2Account)selected))
                                            //{
                                            //    watched = wa.watched,
                                            //};
                                        }
                                    }
                                    else
                                    {
                                        index = GetDropDownItem(bar.DropDownItems, wa.accountdata.account);
                                    }
                                }
                                else if (wa.IsAccount(selected))
                                {
                                    index = 0;
                                }

                                watched = wa.watched;

                                if (index == -1)
                                {
                                    wa.Dispose();
                                    this.watched.Remove(group.id);
                                }
                                else
                                {
                                    group.watched = wa;
                                    wa.group = group;
                                    wa.focusedKey = focusedKey;

                                    if (combined)
                                    {
                                        selected = wa.accountdata.account;
                                    }
                                }
                            }
                        }

                        if (index == -1)
                        {
                            if (combined)
                            {
                                if (autoSelectCurrentAccountToolStripMenuItem.Checked && focused != null && (index = GetDropDownItem(bar.DropDownItems, focused)) != -1)
                                {
                                    selected = ((Util.ComboItem<Settings.IAccount>[])bar.DropDownItems)[index].Value;
                                }
                                else
                                {
                                    index = GetDropDownItem(bar.DropDownItems, selected);
                                }
                            }
                            else
                            {
                                index = 0;
                            }

                            if (Settings.Dailies.ItemOptions.TryGetValue(group.id, out o) && (o & Settings.DailiesItemOptions.Watched) != 0)
                            {
                                watched = true;
                            }
                        }

                        bar.Text = selected.Name;

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

                        if (combined)
                        {
                            var square = group.squares = squares.GetNext();

                            square.SetAccounts(accounts);
                            square.Selected = selected;
                            square.Tag = group;
                        }

                        var updatedLabel = group.updated = updated.GetNext();

                        updatedLabel.Enabled = false;
                        updatedLabel.Date = DateTime.MinValue;
                        updatedLabel.Tag = group;

                        if (count == 0)
                        {
                            group.unavailable = unavailable.GetNext();
                        }

                        if (Settings.Dailies.VaultObjectiveSorting.Value != 0 || hasNew)
                        {
                            SortObjectives(group, selected, Settings.Dailies.VaultObjectiveSorting.Value);
                        }
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
                    if (defaultImages[1] == null)
                    {
                        defaultImages[1] = defaultImages[2] = defaultImages[0];
                        LoadDefaultIcons();
                    }
                }

                #endregion

                squares.HideAndAdd(panelContent);
                updated.HideAndAdd(panelContent);
                unavailable.HideAndAdd(panelContent);
            }

            var favs = CreateDailyControls(gfav != null ? favorites.Count : 0);

            if (gfav != null && favorites.Count > 0)
            {
                var group = gfav;
                var bar = group.bar = bars.GetNext();

                group.category = new Daily.Category()
                {
                    Name = "Favorites",
                };

                group.items = favorites.ToArray();
                group.count = (ushort)group.items.Length;

                bar.SetState(group.collapsed);
                bar.Text = group.category.Name;
                bar.ButtonEyeVisible = false;
                bar.ButtonDropDownArrowVisible = false;
                bar.DropDownItems = null;
            }

            if ((type & DataType.Vault) != 0 && Settings.Dailies.VaultSorting.Value != Settings.DailiesVaultSorting.None)
            {
                Sort(groups, groups.Length, Settings.Dailies.VaultSorting.Value & Settings.DailiesVaultSorting.Sorting, (Settings.Dailies.VaultSorting.Value & Settings.DailiesVaultSorting.Descending) != 0);
            }
            else if (firstIndex != groups.Length)
            {
                var count = groups.Length - firstIndex;
                if (count > 1)
                    Array.Reverse(groups, firstIndex, count);
            }

            firstIndex = 0;

            foreach (var group in groups)
            {
                group.index = (ushort)firstIndex++;

                if (group.count > 0 || (type & DataType.Vault) != 0)
                {
                    var bar = group.bar;
                    var controls = items;

                    if (group.id.Type == Settings.DailiesKeyType.DailyCategory && group.id.ID == 0)
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
                        control.ColorKey = d.IsNew ? Color.Gold : Color.Empty;

                        if (showIcon && control.IconValue == null)
                        {
                            var icon = defaultImages[0];
                            if (group.category != null)
                            {
                                icon = group.category.GetIcon();
                                if (icon == null)
                                    icon = defaultImages[0];
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

                        OnWatchedChanged(bar, false, false);
                    }

                    if (group.unavailable != null)
                    {
                        var b = group.count == 0;

                        group.unavailable.Location = new Point(x, y);
                        group.unavailable.Visible = b;

                        if (b)
                            y += group.unavailable.Height;
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

            bars.HideAndAdd(panelContent);
            items.HideAndAdd(panelContent);
            favs.HideAndAdd(panelContent);

            reusable.HideRemaining();

            panelContent.Height = y;
            scrollV.Maximum = y - panelContainer.Height;

            reposition = int.MaxValue;
        }

        private bool UpdateProgress(ItemGroup group, bool sort)
        {
            if (group.bar.ButtonEyeVisible && group.bar.ButtonEyeEnabled && group.accounts != null)
            {
                var selected = group.GetSelectedAccount();
                var changed = false;

                if (group.source != null && selected != null)
                {
                    var og = ((ObjectiveGroupData)group.source).data;
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

                            var o = ao.GetObjective(og.Type, group.items[i].ID, ((ObjectiveDataSource)group.items[i]).SourceIndex);
                            var v = c.ProgressValueRaw;

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

                            if (c.ProgressValueRaw != v)
                            {
                                changed = true;
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

                        if (changed && sort)
                        {
                            if ((Settings.Dailies.VaultObjectiveSorting.Value & Settings.DailiesVaultObjectiveSorting.Progress) != 0)
                            {
                                SortObjectives(group, selected, Settings.Dailies.VaultObjectiveSorting.Value);
                                PendingReposition(group.index);
                            }
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
                var a = accounts[i];

                if (a != null)
                {
                    items[i] = new Util.ComboItem<Settings.IAccount>(a, a.Name);
                }
                else
                {
                    if (i == 0)
                    {
                        return null;
                    }

                    Array.Resize<Util.ComboItem<Settings.IAccount>>(ref items, i);
                    count = i;

                    break;
                }
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
                    
                    if (g.count == 0 && g.unavailable == null)
                    {
                        continue;
                    }

                    y = g.GetBottomControl().Bottom;

                    break;
                }
            }

            for (int i = startAt, l = groups.Length; i < l; i++)
            {
                var g = groups[i];
                if (g.IsHidden())
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

                if (g.unavailable != null)
                {
                    var b = visible && g.count == 0;

                    if (b)
                    {
                        g.unavailable.Top = y;
                        y += g.unavailable.Height;
                    }

                    g.unavailable.Visible = b;
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
            Settings.DailiesItemOptions o;
            if (Settings.Dailies.ItemOptions.TryGetValue(group.id, out o))
            {
                if (collapsed)
                    o |= Settings.DailiesItemOptions.Collapsed;
                else
                    o &= ~Settings.DailiesItemOptions.Collapsed;
                Settings.Dailies.ItemOptions[group.id] = o;
            }
            else if (collapsed)
            {
                Settings.Dailies.ItemOptions[group.id] = Settings.DailiesItemOptions.Collapsed;
            }

            group.collapsed = collapsed;
            DoPendingReposition(group.index);
        }

        void bar_Expanded(object sender, EventArgs e)
        {
            OnCollapsedChanged((ItemGroup)((DailyCategoryBar)sender).Tag, false);
        }

        void bar_Collapsed(object sender, EventArgs e)
        {
            OnCollapsedChanged((ItemGroup)((DailyCategoryBar)sender).Tag, true);
        }

        private void OnWatchedChanged(DailyCategoryBar bar, bool refresh = false, bool sort = true)
        {
            var g = (ItemGroup)bar.Tag;

            if (g.accounts == null)
                return;

            var selected = (Settings.IGw2Account)g.GetSelectedAccount();
            //var selected = (Util.ComboItem<Settings.IAccount>)bar.DropDownSelectedItem;

            if (selected != null && this.data.Contains(g))
            {
                var tab = GetTab(this.data.type);
                var watched = bar.ButtonEyeVisible && bar.ButtonEyeEnabled;
                var wa = g.watched;

                lock (this.watched)
                {
                    if (wa != null)
                    {
                        if (!wa.IsAccount(selected))
                        {
                            //if (refresh)
                            wa.SetAccount(this.watched.GetAccount(selected), !refresh);
                        }

                        wa.watched = watched;

                        var r = wa.accountdata.request;

                        if (r != null && (!watched || r.Type != GetApiType(this.data.type)))
                        {
                            wa.Abort();
                            wa.requested = false;
                        }
                    }

                    if (wa == null)
                    {
                        this.watched[g.id] = wa = new Watched.WatchedGroup(this.watched.GetAccount(selected))
                        {
                            watched = watched,
                            group = g,
                            focusedKey = focusedKey,
                        };

                        g.watched = wa;
                    }
                }

                if (watched)
                {
                    if (wa.accountdata.request == null)
                    {
                        var ao = vob.GetObjectives(selected);
                        var vt = GetVaultType(this.data.type);

                        if (ao != null && !ao.IsComplete(vt) && (ao.IsPending(vt) || Client.Launcher.IsActive(selected)))
                        {
                            if (QueueApiRequest(this.data.type, wa, false, refresh))
                            {
                                //wa.requested = false;

                            }
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

                bar.ButtonEyeTimerEnabled = wa.requested && wa.accountdata.request != null;
                UpdateProgress(g, sort);
            }
        }

        private bool QueueApiRequest(DataType t, Watched.WatchedGroup w, bool nocache, bool repeat = false)
        {
            var api = w.accountdata.account.Api;

            if (api == null)
            {
                return false;
            }

            var r = new ApiRequest(GetApiType(t), w.accountdata.account, api, nocache ? ApiData.DataRequest.RequestOptions.NoCache : ApiData.DataRequest.RequestOptions.None)
            {
                Watched = w,
                AccountData = w.accountdata,
                ForceRepeat = repeat,
            };

            r.Complete += OnWatchedRequestComplete;
            r.DataAvailable += OnWatchedRequestDataAvailable;

            w.accountdata.request = r;
            w.requested = nocache || DateTime.UtcNow > vob.ApiManager.DataSource.GetNext(api.Key);

            vob.ApiManager.Queue(r);

            return true;
        }

        void OnWatchedRequestDataAvailable(object sender, ApiData.RequestDataAvailableEventArgs e)
        {
            if (e.Status == ApiData.DataStatus.Error)
                return;

            var r = (ApiRequest)sender;
            var wa = r.Watched;
            var d = r.AccountData;

            if (d.request != r || IsDisposed)
            {
                return;
            }

            var t = GetType(e.Type);
            var b = currentTab == t && wa.watched && (r.RepeatCount == 0 && r.ForceRepeat || DateTime.UtcNow.Subtract(wa.accountdata.date).TotalMinutes < 3) && Client.Launcher.IsActive(r.Account);

            if (!wa.requested && wa.IsAccount(d))
            {
                wa.requested = true;
            }

            Util.Invoke.Async(this, delegate
            {
                ItemGroup g;

                if (d.request == r)
                {
                    if (!b)
                    {
                        d.request = null;
                    }

                    if (currentTab == t && wa.watched && wa.IsAccount(d) && wa.GetGroup(this.data, out g))
                    {
                        g.bar.ButtonEyeTimerEnabled = wa.requested && d.request != null;
                    }
                    else if (b)
                    {
                        r.Abort();
                        d.request = null;
                    }
                }
                else if (b)
                {
                    r.Abort();
                }

                //if (d.request == r && currentTab == t && wa.watched && wa.IsAccount(d) && wa.GetGroup(this.data, out g))
                //{
            });

            if (b)
            {
                e.Repeat = true;

            }

        }

        void OnWatchedRequestComplete(object sender, EventArgs e)
        {
            var r = (ApiRequest)sender;
            //var wa = r.Watched;
            var d = r.AccountData;

            if (d.request != r || IsDisposed)
            {
                return;
            }

            Util.Invoke.Async(this, delegate
            {
                //var t = GetType(r.Type);

                if (d.request == r)
                {
                    d.request = null;

                    ItemGroup g;

                    //if (currentTab == t && wa.watched && wa.IsAccount(d) && wa.GetGroup(this.data, out g))
                    //{
                }
            });

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
            var g = (ItemGroup)bar.Tag;

            OnWatchedChanged(bar);
            DoPendingReposition();

            if (bar.ButtonEyeVisible && bar.ButtonEyeEnabled)
            {
                Settings.Dailies.ItemOptions[g.id] |= Settings.DailiesItemOptions.Watched;

                Util.ScheduledEvents.Register(OnScheduledRefreshWatched, 5000);
            }
            else
            {
                Settings.Dailies.ItemOptions[g.id] &= ~Settings.DailiesItemOptions.Watched;
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
                    if (groups[i].id.ID == id)
                    {
                        return groups[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns if the data contains data for the specified type
        /// </summary>
        private bool IsValid(Daily.Achievements dailies, DataType type)
        {
            switch (dailies.Age)
            {
                case 0:

                    return type == DataType.DailyToday || type == DataType.DailyTomorrow && dailies.Tomorrow != null;

                case 1:

                    return type == DataType.DailyToday && dailies.Tomorrow != null;
            }

            return false;
        }

        /// <summary>
        /// Displays data for dailies
        /// </summary>
        /// <param name="type">Type to refresh</param>
        /// <param name="refresh">True to force a refresh</param>
        /// <param name="reload">True to force reloading displayed data</param>
        /// <param name="silent">True to hide download when the data is already displayed</param>
        private async void GetData(DataType type, bool refresh = false, bool reload = false, bool silent = false)
        {
            var tab = GetTab(type);
            if (tab == null || IsDisposed)
                return;
            //currentTab = type;
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
                    else if (!current.Verified && DateTime.UtcNow.Subtract(current.Date.Date).TotalHours >= 1)
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

                        //hide loading if data isn't expected to change
                        if (!silent || current == null || (loadedTab & DataType.Daily) == 0 || !IsValid(current, type) || !panelContent.Visible)
                        {
                            OnTabLoading();
                        }

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
                        if ((type & currentTab) != 0)
                        {
                            OnTabLoading();
                        }

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

            if ((type & currentTab) != 0 && !IsDisposed)
            {
                waitingBounce.Visible = false;

                if (type != currentTab)
                {
                    //displayed tab changed while loading
                    display = currentTab;
                }

                if ((type & DataType.Daily) != 0)
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
                            OnTabLoaded(display, changed, reload, scrollV.Maximum == 0 ? sliderValue : scrollV.Value, dailies);
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
                        OnTabLoaded(type, changed, reload, sliderValue, objectives);
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

        private void OnTabLoaded(DataType display, bool changed, bool reload, int sliderValue, object data)
        {
            if (changed || reload || loadedTab != display)
            {
                if ((loadedTab & display) == 0)
                {
                    sliderValue = 0;
                }

                loadedTab = display;

                if (popup != null && popup.Visible)
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

                if ((currentTab & DataType.Vault) != 0 && autoScrollToCurrentAccountToolStripMenuItem.Checked && focused != null)
                {
                    if (ScrollTo(focused, sliderValue))
                    {
                        sliderValue = scrollV.Value;
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
                    RefreshDailies(false, true, false, true);
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

        /// <summary>
        /// Checks if an account is active, either by recent api usage or link activity
        /// </summary>
        private async Task<ActivityType> IsActive(Settings.IGw2Account gw2)
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
                        t = await IsActive(taccount);
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
                        t = await IsActive(taccount);

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
            if ((displayedTabs & DataType.Vault) != 0)
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
            }

            if (_MonitorVaultSpecial)
            {
                Util.ScheduledEvents.Register(OnScheduledVaultSpecialRefresh, DateTime.UtcNow.AddMinutes(10), Util.ScheduledEvents.RegisterOptions.Async);
            }

        }

        private void Launcher_AccountExited(Settings.IAccount account)
        {
            if (focused == account)
            {
                focused = null;
            }

            if ((displayedTabs & DataType.Vault) == 0)
                return;

            if (account.Type == Settings.AccountType.GuildWars2)
            {
                var a = (Settings.IGw2Account)account;

                vob.Refresh(a);

            }
        }

        void Launcher_AccountWindowEvent(Settings.IAccount account, Client.Launcher.AccountWindowEventEventArgs e)
        {
            if (e.Type == Client.Launcher.AccountWindowEventEventArgs.EventType.Focused)
            {
                if (focused != account)
                {
                    focused = account;
                    ++focusedKey;

                    if (this.IsHandleCreated)
                        Util.Invoke.Async(this, OnFocusedChanged);
                }
            }
        }

        private void CefSessions_SessionEvent(object sender, Tools.Chromium.CefSessionMonitor.SessionEventArgs e)
        {
            if ((displayedTabs & DataType.Vault) == 0)
                return;

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
                    else
                    {
                        lock (watched)
                        {
                            Watched.WatchedGroup wa;

                            var id = combineAccountsToolStripMenuItem.Checked ? o.ID : a.UID;

                            if (!watched.TryGetValue(GetKey(currentTab, id, false), out wa) || !wa.watched || !wa.IsAccount(a))
                            {
                                return;
                            }
                        }

                        vob.Refresh(vt, a, Tools.Api.VaultObjectives.RefreshOptions.Delayed | Tools.Api.VaultObjectives.RefreshOptions.Update | Tools.Api.VaultObjectives.RefreshOptions.Latest);
                    }
                }
            }
        }

        private void SortObjectives(ItemGroup[] groups, int count, Settings.DailiesVaultObjectiveSorting sorting)
        {
            for (var i = 0; i < groups.Length; i++)
            {
                if (groups[i].count > 0)
                {
                    SortObjectives(groups[i], groups[i].GetSelectedAccount(), sorting);
                }
            }
        }

        private void SortObjectives(ItemGroup group, Settings.IAccount selected, Settings.DailiesVaultObjectiveSorting sorting)
        {
            var ao = (sorting & Settings.DailiesVaultObjectiveSorting.Progress) != 0 && selected != null ? vob.GetObjectives(selected) : null;

            Array.Sort<IData, DailyAchievement>(group.items, group.controls, 0, group.count, Comparer<IData>.Create(new Comparison<IData>(
                delegate(IData a, IData b)
                {
                    if (a.IsNew != b.IsNew)
                    {
                        return a.IsNew ? -1 : 1;
                    }

                    int r;

                    if ((sorting & Settings.DailiesVaultObjectiveSorting.Progress) != 0 && ao != null)
                    {
                        var d1 = (ObjectiveDataSource)a;
                        var d2 = (ObjectiveDataSource)b;
                        var o1 = ao.GetObjective(((ObjectiveGroupData)group.source).data.Type, d1.ID, d1.SourceIndex);
                        var o2 = ao.GetObjective(((ObjectiveGroupData)group.source).data.Type, d2.ID, d2.SourceIndex);

                        if (o1 == null && o2 == null)
                        {
                            r = 0;
                        }
                        else
                        {
                            var claimed1 = o1 != null && o1.Claimed;
                            var claimed2 = o2 != null && o2.Claimed;

                            //claimed on bottom, higher progress on top

                            if (claimed1)
                            {
                                r = claimed2 ? 0 : 1;
                            }
                            else if (claimed2)
                            {
                                r = -1;
                            }
                            else
                            {
                                var progress1 = o1 != null ? o1.ProgressCurrent : 0;
                                var progress2 = o2 != null ? o2.ProgressCurrent : 0;

                                if (progress1 == 0)
                                {
                                    r = progress2 == 0 ? 0 : 1;
                                }
                                else if (progress2 == 0)
                                {
                                    r = -1;
                                }
                                else if (d1.Source.ProgressComplete == d2.Source.ProgressComplete)
                                {
                                    r = -progress1.CompareTo(progress2);
                                }
                                else
                                {
                                    r = -(progress1 / (float)d1.Source.ProgressComplete).CompareTo((progress2 / (float)d2.Source.ProgressComplete));
                                }
                            }
                        }
                    }
                    else
                    {
                        r = 0;
                    }

                    if (r == 0 && (sorting & Settings.DailiesVaultObjectiveSorting.Name) != 0)
                    {
                        if (a.Name != null)
                        {
                            r = a.Name.CompareTo(b.Name);
                        }
                        else
                        {
                            r = b.Name == null ? 0 : 1;
                        }
                    }

                    if (r == 0 && (sorting & Settings.DailiesVaultObjectiveSorting.ID) != 0)
                    {
                        r = a.ID.CompareTo(b.ID);
                    }

                    if (r == 0)
                    {
                        r = ((ObjectiveDataSource)a).SourceIndex.CompareTo(((ObjectiveDataSource)b).SourceIndex);
                    }

                    if ((sorting & Settings.DailiesVaultObjectiveSorting.Descending) != 0)
                        return -r;
                    else
                        return r;
                })));
        }

        private void Sort(ItemGroup[] groups, int count, Settings.DailiesVaultSorting sorting, bool descending)
        {
            Array.Sort<ItemGroup>(groups, 0, count, Comparer<ItemGroup>.Create(new Comparison<ItemGroup>(
                delegate(ItemGroup a, ItemGroup b)
                {
                    if (a.IsHidden() || b.IsHidden())
                    {
                        return b.IsHidden().CompareTo(a.IsHidden());
                    }
                    else if (a.collapsed == b.collapsed)
                    {
                        int r;

                        switch (sorting)
                        {
                            case Settings.DailiesVaultSorting.Group:

                                r = ((ObjectiveGroupData)a.source).data.ID.CompareTo(((ObjectiveGroupData)b.source).data.ID);

                                break;
                            case Settings.DailiesVaultSorting.Focused:

                                r = -a.focused.CompareTo(b.focused);

                                break;
                            case Settings.DailiesVaultSorting.Account:

                                r = a.accountid.CompareTo(b.accountid);

                                break;
                            default:

                                r = 0;

                                break;
                        }

                        if (r == 0)
                        {
                            r = ((ObjectiveGroupData)a.source).index.CompareTo(((ObjectiveGroupData)b.source).index);
                        }

                        return descending ? -r : r;
                    }
                    else
                    {
                        var r = a.collapsed ? 1 : -1;

                        if (sorting == Settings.DailiesVaultSorting.Focused && descending)
                        {
                            r = -r;
                        }

                        return r;
                    }
                })));

            for (var i = 0; i < count; i++)
            {
                groups[i].index = (ushort)i;
            }
        }

        private bool ScrollTo(Settings.IAccount a, int scrollV)
        {
            var g = this.data.GetGroupFromAccount(a);

            if (g != null)
            {
                return ScrollTo(g, scrollV);
            }

            return false;
        }

        private bool ScrollTo(ItemGroup g, int scrollV)
        {
            var c = g.GetBottomControl();

            if (c != null)
            {
                var y1 = g.bar.Top - scrollV;
                var y2 = c.Bottom - scrollV;
                var h = panelContainer.Height;

                if (y2 < 0 || y1 > h)
                {
                    this.scrollV.Value = g.bar.Top;

                    return true;
                }
                else if (y1 < 0)
                {
                    if (y2 < h)
                    {
                        this.scrollV.Value = g.bar.Top;

                        return true;
                    }
                }
                else if (y2 > h)
                {
                    if (c.Bottom - g.bar.Top < h)
                    {
                        this.scrollV.Value = c.Bottom - h;

                        return true;
                    }
                }

            }

            return false;
        }

        private void OnFocusedChanged()
        {
            if ((currentTab & DataType.Vault) != 0 && (autoScrollToCurrentAccountToolStripMenuItem.Checked || autoSelectCurrentAccountToolStripMenuItem.Checked || focusedToolStripMenuItem.Checked) && Invalidate(currentTab))
            {
                var a = this.focused;
                var g = data.GetGroupFromAccount(a);

                if (g != null)
                {
                    g.focused = DateTime.UtcNow;

                    //select
                    if (autoSelectCurrentAccountToolStripMenuItem.Checked && g.bar.ButtonDropDownArrowVisible && g.GetSelectedAccount() != a)
                    {
                        var i = GetDropDownItem(g.bar.DropDownItems, a);

                        if (i != -1)
                        {
                            if (g.squares != null)
                            {
                                g.squares.Selected = a;
                            }
                            g.bar.DropDownSelectedIndex = i;
                            g.bar.Text = a.Name;

                            OnWatchedChanged(g.bar, true);
                        }
                    }

                    //sort
                    if (!g.collapsed && focusedToolStripMenuItem.Checked)
                    {
                        if (descendingToolStripMenuItem.Checked)
                        {
                            var l = data.groups.Length - 1;

                            if (g.index < l)
                            {
                                PendingReposition(g.index);

                                for (var i = g.index; i < l; i++)
                                {
                                    data.groups[i] = data.groups[i + 1];
                                    data.groups[i].index = i;
                                }
                                data.groups[l] = g;
                                g.index = (ushort)l;
                            }
                        }
                        else if (g.index > 0)
                        {
                            for (var i = g.index; i > 0; --i)
                            {
                                data.groups[i] = data.groups[i - 1];
                                data.groups[i].index = i;
                            }
                            data.groups[0] = g;
                            g.index = 0;

                            PendingReposition(0);
                        }
                    }

                    DoPendingReposition();

                    //scroll
                    if (!g.collapsed && autoScrollToCurrentAccountToolStripMenuItem.Checked)
                    {
                        ScrollTo(g, scrollV.Value);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (vob != null)
                {
                    vob.DataChanged -= vob_DataChanged;
                    vob.AccountDataChanged -= vob_AccountDataChanged;
                }

                if (_MonitorVaultSpecial)
                {
                    MonitorVaultSpecial(false);
                }

                Util.ScheduledEvents.Unregister(
                    OnScheduledRetry,
                    OnScheduledDailiesRefresh,
                    OnScheduledBeforeDailyReset,
                    OnScheduledBeforeWeeklyReset,
                    OnScheduledVaultSpecial,
                    OnScheduledVaultSpecialRefresh);

                Client.Launcher.MumbleLinkVerified -= Launcher_MumbleLinkVerified;
                Client.Launcher.CefSessions.SessionEvent -= CefSessions_SessionEvent;
                Client.Launcher.AccountExited -= Launcher_AccountExited;

                if (IsMainWindow)
                {
                    Settings.Dailies.DailyCategories.ValueChanged -= Categories_ValueChanged;
                    Settings.Dailies.Options.ValueChanged -= DailiesSettings_ValueChanged;
                }

                if ((enabledTabs & DataType.Vault) != 0)
                {
                    Client.Launcher.AccountWindowEvent -= Launcher_AccountWindowEvent;
                    Settings.Dailies.VaultOptions.ValueChanged -= VaultSettings_ValueChanged;
                    Settings.Dailies.VaultSorting.ValueChanged -= VaultSorting_ValueChanged;
                    Settings.Dailies.VaultObjectiveSorting.ValueChanged -= VaultObjectiveSorting_ValueChanged;
                }

                Settings.Dailies.Language.ValueChanged -= Language_ValueChanged;

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
                    dailies.Dispose();
                    dailies = null;
                }

                LinkedToParent = false;
            }
            base.Dispose(disposing);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (IsMainWindow && (Settings.Dailies.VaultOptions.Value & Settings.DailiesVaultOptions.Split) != 0)
            {
                ShowSplitVaultObjectives(true);
            }
        }

        public void Show(bool focus)
        {
            if (IsMainWindow)
            {
                bool autoMinimize = (Settings.Dailies.Options.Value & Settings.DailiesOptions.Positioned) == 0;

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

                    if (loadOnShow || currentTab == DataType.None)
                    {
                        loadOnShow = false;
                        SelectTab(GetDefaultTab());
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
                                        if ((Settings.Dailies.Options.Value & Settings.DailiesOptions.Positioned) == 0)
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
            }
            else
            {
                if (!this.Visible)
                {
                    if (loadOnShow)
                    {
                        loadOnShow = false;
                        SelectTab(GetDefaultTab());
                    }

                    this.Show(parent);
                }
            }
            if (focus && this.Visible)
                this.Focus();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            var v = this.Visible;

            if (v)
            {
                if (loadOnShow)
                {
                    loadOnShow = false;
                    SelectTab(GetDefaultTab());
                }

                this.Refresh();
                this.Opacity = 1;
            }
            else
            {
                this.Opacity = 0;
            }

            if (IsMainWindow)
            {
                MonitorVaultSpecial(v);
            }

            if (child != null)
            {
                child.Visible = v;
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
            if (IsMainWindow)
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
            else
            {
                ShowSplitVaultObjectives(false);
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
            this.Minimize(this.ContainsFocus);
        }

        private int Abs(int i)
        {
            if (i < 0)
                return -i;
            return i;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (IsMainWindow)
                {
                    e.Cancel = true;
                    Minimize(true);
                }
                else
                {
                    var f = (formDailies)parent;

                    f.SetTabs(f.displayedTabs | this.displayedTabs);
                }
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

                    if (IsMainWindow)
                    {
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
                    }

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

                    if (!linkedToParent && parent.Visible && IsMainWindow)
                    {
                        this.LocationChanged += OnBeginLocationChanged;
                    }

                    break;
                case WindowMessages.WM_EXITSIZEMOVE:

                    base.WndProc(ref m);

                    sizing = false;

                    if (IsMainWindow)
                    {
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
                            Settings.Dailies.Options.Value &= ~Settings.DailiesOptions.Positioned;
                        }
                        else
                        {
                            Settings.WindowBounds[this.GetType()].Value = this.Bounds;
                            Settings.Dailies.Options.Value |= Settings.DailiesOptions.Positioned;
                        }
                    }
                    else
                    {
                        var t = this.GetType();

                        if (t != typeof(formDailies))
                        {
                            Settings.WindowBounds[t].Value = this.Bounds;
                        }
                    }

                    break;
                case WindowMessages.WM_NCMOUSELEAVE:

                    base.WndProc(ref m);

                    foreach (Control c in panelTabs.Controls)
                    {
                        if (c is FlatButton && ((FlatButton)c).IsMouseEntered)
                        {
                        }
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
                        Settings.Dailies.DailyCategories.Clear();
                    }
                    else if (!Util.Array.Equals<ushort>(Settings.Dailies.DailyCategories.Value, f.SelectedCategories))
                    {
                        Settings.Dailies.DailyCategories.Value = f.SelectedCategories;
                    }
                }
            }
        }

        private void favoritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTaggedDialog(Settings.DailiesItemOptions.Favorite);
        }

        private void ignoredToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTaggedDialog(Settings.DailiesItemOptions.Ignored);
        }

        private void showOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!showOnTopToolStripMenuItem.Checked)
            {
                Settings.Dailies.Options.Value |= Settings.DailiesOptions.TopMost;
            }
            else
            {
                Settings.Dailies.Options.Value &= ~Settings.DailiesOptions.TopMost;
            }
        }

        private void buttonVault_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ShowMenu(DataType.Vault);
            }
            else
            {
                DataType t;

                if (sender == buttonSpecial)
                {
                    t = DataType.VaultSpecial;

                    if (buttonSpecial.TopTag.Visible)
                    {
                        if (currentTab == DataType.VaultSpecial)
                        {
                            if (this.data != null && this.data.type == t)
                            {
                                foreach (var g in this.data.groups)
                                {
                                    for (var i = 0; i < g.count; i++)
                                    {
                                        g.controls[i].ColorKey = Color.Empty;

                                    }
                                }
                            }

                            specials = null;
                            if (Settings.Dailies.KnownVaultSpecials.HasValue && Settings.Dailies.KnownVaultSpecials.Value.Seen != null)
                            {
                                Settings.Dailies.KnownVaultSpecials.Value = Settings.Dailies.KnownVaultSpecials.Value.ToLatest();
                            }

                            SetSpecialVisible(false);
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

        private void SetSpecialVisible(bool visible, HashSet<ushort> specials = null, sbyte broadcast = 0)
        {
            if ((enabledTabs & DataType.VaultSpecial) != 0)
            {
                buttonSpecial.TopTag.Visible = visible;
                this.specials = specials;
            }

            if (broadcast <= 0 && parent is formDailies)
            {
                ((formDailies)parent).SetSpecialVisible(visible, specials, -1);
            }

            if (broadcast >= 0 && child != null)
            {
                child.SetSpecialVisible(visible, specials, 1);
            }
        }

        void buttonSpecial_SelectedChanged(object sender, EventArgs e)
        {
            if (!buttonSpecial.Selected)
            {
                if (buttonSpecial.TopTag.Visible)
                {
                    specials = null;
                    if (Settings.Dailies.KnownVaultSpecials.HasValue && Settings.Dailies.KnownVaultSpecials.Value.Seen != null)
                    {
                        Settings.Dailies.KnownVaultSpecials.Value = Settings.Dailies.KnownVaultSpecials.Value.ToLatest();
                    }
                    SetSpecialVisible(false);
                }
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

        private void DoPendingReposition(int i)
        {
            if (i < 0)
            {
                i = 0;
            }
            else if (reposition < i)
            {
                i = reposition;
            }

            PositionGroups(i);
            reposition = int.MaxValue;
        }

        void vob_AccountDataChanged(object sender, Tools.Api.VaultObjectives.DataChangedEventArgs e)
        {
            if ((e.Changed & Tools.Api.VaultObjectives.ChangeType.Objectives) == 0 && (e.Changed & (Tools.Api.VaultObjectives.ChangeType.Date | Tools.Api.VaultObjectives.ChangeType.Values)) != 0)
            {
                var t = GetType(e.Type);

                if (currentTab == t)
                {
                    var tab = GetTab(t);

                    Watched.WatchedGroup wa;

                    lock (watched)
                    {
                        var id = combineAccountsToolStripMenuItem.Checked ? e.Group.ID : e.Data.Account.UID;

                        if (!watched.TryGetValue(GetKey(t, id, false), out wa) || !wa.popup && (!wa.watched || !wa.IsAccount(e.Data.Account)))
                        {
                            return;
                        }
                    }

                    Util.Invoke.Async(this, delegate
                    {
                        if (Invalidate(t))
                        {
                            ItemGroup g;
                            if (wa.GetGroup(this.data, out g))
                            {
                                if (wa.popup)
                                {
                                    if (popupObjectives.Visible && popupObjectives.Attached != null)
                                    {
                                        CreateObjectivesPopup(popupObjectives.Attached);
                                    }
                                }

                                if (wa.watched && wa.IsAccount(e.Data.Account))
                                {
                                    if (!UpdateProgress(g, true))
                                    {
                                        wa.Abort();
                                    }
                                }

                                DoPendingReposition();
                            }
                        }
                    });
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

                    if ((e.Changed & Tools.Api.VaultObjectives.ChangeType.Objectives) != 0)
                    {
                        if (IsMainWindow)
                        {
                            var od = e.Group.Objectives;
                            var ids = new ushort[od.Length];
                            var seen = Settings.Dailies.KnownVaultSpecials.HasValue ? Settings.Dailies.KnownVaultSpecials.Value.SeenOrLatest : null;
                            var changed = seen == null;
                            var h = !changed ? new HashSet<ushort>(seen) : null;

                            for (var i = 0; i < od.Length; i++)
                            {
                                ids[i] = od[i].ID;

                                if (!changed && !h.Contains(ids[i]))
                                {
                                    changed = true;
                                }
                            }

                            if (changed)
                            {
                                Settings.Dailies.KnownVaultSpecials.Value = new Settings.KnownVaultObjectives(ids, seen);
                                SetSpecialVisible(seen != null, h);
                            }
                        }
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
                        if (!combineAccountsToolStripMenuItem.Checked)
                        {
                            tab.Refresh = true;

                            if (currentTab == t)
                            {
                                Util.ScheduledEvents.Register(OnScheduledVaultRefresh, 1000);
                            }
                        }
                        else if (Invalidate(t))
                        {
                            foreach (var g in this.data.groups)
                            {
                                if (g.IsHidden())
                                    continue;

                                var ogd = (ObjectiveGroupData)g.source;

                                if (!object.ReferenceEquals(ogd.data, e.Group))
                                    continue;

                                if ((e.Changed & Tools.Api.VaultObjectives.ChangeType.Accounts) != 0)
                                {
                                    var accounts = e.Group.GetAccounts();
                                    var items = GetDropDownItems(accounts);

                                    g.accounts = accounts;

                                    ogd.accounts = accounts;
                                    g.source = ogd;

                                    if (items == null)
                                    {
                                        tab.Refresh = true;
                                        Util.ScheduledEvents.Register(OnScheduledVaultRefresh, 5000);
                                    }
                                    else
                                    {
                                        var hasFocused = false;

                                        if (focused != null && (currentTab & DataType.Vault) != 0 && (autoScrollToCurrentAccountToolStripMenuItem.Checked || autoSelectCurrentAccountToolStripMenuItem.Checked || focusedToolStripMenuItem.Checked))
                                        {
                                            if (GetDropDownItem(g.bar.DropDownItems, focused) == -1)
                                            {
                                                for (var j = 0; j < accounts.Length; j++)
                                                {
                                                    if (accounts[j] == focused)
                                                    {
                                                        hasFocused = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

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

                                        var wchanged = false;

                                        if (i == -1)
                                        {
                                            //an account that was selected was removed from the group
                                            i = 0;
                                            g.bar.Text = items[i].Value.Name;

                                            if (g.watched != null)
                                            {
                                                wchanged = true;

                                                g.watched.Dispose();
                                                g.watched = null;

                                                lock (watched)
                                                {
                                                    watched.Remove(g.id);
                                                }

                                            }
                                        }

                                        g.bar.DropDownItems = items;
                                        g.bar.DropDownSelectedIndex = i;
                                        g.bar.ButtonDropDownArrowVisible = items.Length > 1;

                                        if (g.squares != null)
                                        {
                                            var b = g.squares.Count > 1;

                                            g.squares.SetAccounts(accounts);
                                            g.squares.Selected = items[i].Value;

                                            if (b != (g.squares.Count > 1))
                                            {
                                                PendingReposition(g.index);
                                            }
                                        }

                                        if (wchanged)
                                        {
                                            OnWatchedChanged(g.bar);
                                        }

                                        if (hasFocused)
                                        {
                                            OnFocusedChanged();
                                        }
                                    }
                                }

                                DoPendingReposition();

                                break;
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

        /// <summary>
        /// Returns true if the tab is being shown. If not shown, the tab will be reloaded the next time it's shown
        /// </summary>
        private bool Invalidate(DataType t)
        {
            if (currentTab == t)
            {
                return IsLoaded(t);
            }
            else if (loadedTab == t)
            {
                loadedTab = DataType.None;
            }

            return false;
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

                var ao = vob.GetObjectives(wa.accountdata.account);

                if (ao == null || !ao.IsComplete(vt))
                {
                    ++count;

                    if (ao != null && Client.Launcher.IsActive(wa.accountdata.account))
                    {
                        Refresh(wa);
                    }
                }
                else if (wa.accountdata.request != null)
                {
                    wa.Abort();
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

        private async void Refresh(Watched.WatchedGroup wa, int limit = 10000)
        {
            var refresh = false;
            var t = this.currentTab;
            var a = wa.accountdata;
            var l = Client.Launcher.GetMumbleLink(a.account);
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

                            if (a.position != sum)
                            {
                                a.position = sum;
                                a.date = DateTime.UtcNow;
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
                a.date = DateTime.UtcNow;
                refresh = true;
            }

            if (refresh)
            {
                ItemGroup g;

                if (t == currentTab && wa.watched && wa.accountdata.request == null && wa.GetGroup(data, out g) && wa.IsAccount(a))
                {
                    if (QueueApiRequest(currentTab, wa, false))
                    {
                        //g.bar.ButtonEyeTimerEnabled = false;
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

        private void ShowTaggedDialog(Settings.DailiesItemOptions type)
        {
            using (var f = new formDailyFavorites(da, type))
            {
                f.ShowDialog(this);

                if (f.Modified && this.data != null && (this.data.type & DataType.Daily) != 0)
                {
                    var refresh = type == Settings.DailiesItemOptions.Ignored;
                    var groups = this.data.groups;

                    if (!refresh)
                    {
                        foreach (var g in groups)
                        {
                            for (var i = 0; i < g.count; i++)
                            {
                                var c = g.controls[i];

                                Settings.DailiesItemOptions tt;
                                Settings.Dailies.ItemOptions.TryGetValue(new Settings.DailiesItemKey(Settings.DailiesKeyType.DailyObjective, c.DataSource.ID), out tt);

                                if (c.DataSource.Options != tt)
                                {
                                    if (c.DataSource.Options == Settings.DailiesItemOptions.Ignored || tt == Settings.DailiesItemOptions.Ignored)
                                    {
                                        refresh = true;
                                        break;
                                    }

                                    c.DataSource.Options = tt;
                                    c.FavSelected = tt == Settings.DailiesItemOptions.Favorite;
                                }
                            }
                        }
                    }

                    if (refresh)
                    {
                        ReloadDailies();
                    }
                }
            }
        }

        private Util.ScheduledEvents.Ticks OnScheduledBeforeDailyReset()
        {
            const int MILLIS_BEFORE = 60 * 60 * 1000;

            var ticks = DateTime.UtcNow.Ticks / 10000;
            var next = (ticks / Util.Date.MILLIS_PER_DAY + 1) * Util.Date.MILLIS_PER_DAY;
            var s = (next - ticks) / 1000;
            string t;

            if (s > 3600) //over 1 hour - next reset
            {
                t = null;
                next -= MILLIS_BEFORE;
            }
            else
            {
                if (s > 60 * 5 && dailies != null && (!dailies.Verified || dailies.Tomorrow == null || dailies.Age >= 1))
                {
                    //the api can return a mix of yesterday/today dailies for up to an hour after reset
                    //to confirm the dailies, tomorrow needs to be cached before reset

                    GetData((currentTab & DataType.Daily) != 0 ? currentTab : DataType.DailyToday, true, false, true);
                }

                if (s > 30) //show minutes
                {
                    var m = (int)(s / 60f + 0.5f);

                    t = m + "m";
                    next += -(m - 1) * 60 * 1000;
                }
                else //expired
                {
                    t = null;
                    next += Util.Date.MILLIS_PER_DAY - MILLIS_BEFORE;
                }
            }

            buttonVault.BottomTag.Text = t;
            buttonVault.BottomTag.Visible = t != null;

            return new Util.ScheduledEvents.Ticks(Util.ScheduledEvents.TickType.MillisecondTicks, next);
        }

        private Util.ScheduledEvents.Ticks OnScheduledBeforeWeeklyReset()
        {
            //show a timer 24h before weekly reset

            var now = DateTime.UtcNow;
            var next = Util.Date.GetNextWeek(now);
            var s = (int)(next.Subtract(now).Ticks / 10000000);
            string t;

            if (s > 24*60*60) //over 24 hours - reset to next week
            {
                t = null;
                next = next.AddHours(-24);
            }
            else if (s > 90 * 60) //over 90 minutes - show hours
            {
                var h = (int)(s / 3600f + 0.5f);

                t = h + "h";
                if (h > 2) //hour timer
                {
                    next = next.AddSeconds(-(h - 1) * 3600 - 1800);
                }
                else //begin 90m timer
                {
                    next = next.AddSeconds(-90 * 60);
                }
            }
            else if (s > 30) //over 30 seconds - show minutes
            {
                var m = (int)(s / 60f + 0.5f);

                t = m + "m";
                next = next.AddSeconds(-(m - 1) * 60);
            }
            else //expired
            {
                t = null;
                next = now.AddMinutes(1);
            }

            buttonWeekly.BottomTag.Text = t;
            buttonWeekly.BottomTag.Visible = t != null;

            return new Util.ScheduledEvents.Ticks(next);
        }

        private void refreshDailiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetData((currentTab & DataType.Daily) != 0 ? currentTab : DataType.DailyToday, true, true, true);
        }

        private void buttonInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("IsMouseEntered?\t" + buttonToday.IsMouseEntered + ", " + buttonTomorrow.IsMouseEntered + ", " + buttonDaySwap.IsMouseEntered + ", " + buttonVault.IsMouseEntered + ", " + buttonWeekly.IsMouseEntered + ", " + buttonSpecial.IsMouseEntered +
                "\nIsHovered?\t" + buttonToday.IsHovered + ", " + buttonTomorrow.IsHovered + ", " + buttonDaySwap.IsHovered + ", " + buttonVault.IsHovered + ", " + buttonWeekly.IsHovered + ", " + buttonSpecial.IsHovered +
                "\nSelected?\t" + buttonToday.Selected + ", " + buttonTomorrow.Selected + ", " + buttonDaySwap.Selected + ", " + buttonVault.Selected + ", " + buttonWeekly.Selected + ", " + buttonSpecial.Selected);


            foreach (Control c in panelTabs.Controls)
            {
                if (c is FlatButton && ((FlatButton)c).IsMouseEntered)
                {
                    NativeMethods.PostMessage(c.Handle, WindowMessages.WM_MOUSELEAVE, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        private void combineAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            combineAccountsToolStripMenuItem.Checked ^= true;

            var keys = Settings.Dailies.ItemOptions.GetKeys();

            for (var i = 0; i < keys.Length; i++)
            {
                switch (keys[i].Type)
                {
                    case Settings.DailiesKeyType.VaultDailyCategory:
                    case Settings.DailiesKeyType.VaultSpecialCategory:
                    case Settings.DailiesKeyType.VaultWeeklyCategory:
                    case Settings.DailiesKeyType.VaultDailyAccountCategory:
                    case Settings.DailiesKeyType.VaultSpecialAccountCategory:
                    case Settings.DailiesKeyType.VaultWeeklyAccountCategory:

                        Settings.Dailies.ItemOptions.Remove(keys[i]);

                        break;
                }
            }

            if ((currentTab & DataType.Vault) != 0)
            {
                ReloadDailies();
            }
        }

        private void ShowSplitVaultObjectives(bool show)
        {
            if (IsMainWindow)
            {
                if (show)
                {
                    if (child != null && !child.IsDisposed)
                    {
                        if (!child.Visible)
                        {
                            child.Show(this);
                        }
                    }
                    else
                    {
                        child = new formDailiesVault(this, vob);

                        var bounds = Settings.WindowBounds[typeof(formDailiesVault)];

                        if (bounds.HasValue)
                        {
                            child.Bounds = Util.ScreenUtil.Constrain(bounds.Value);
                        }
                        else
                        {
                            var screen = Screen.FromControl(this).WorkingArea;
                            var l = this.Location;

                            if (this.Right + this.Width / 4 > screen.Right)
                            {
                                l.X -= this.Width / 4;
                            }
                            else
                            {
                                l.X += this.Width / 4;
                            }

                            if (this.Bottom + this.Height / 4 > screen.Bottom)
                            {
                                l.Y -= this.Height / 4;
                            }
                            else
                            {
                                l.Y += this.Height / 4;
                            }

                            child.Bounds = new Rectangle(l, this.Size);
                        }

                        if (buttonSpecial.TopTag.Visible)
                            child.buttonSpecial.TopTag.Visible = true;
                        child.SelectTab((currentTab & DataType.Vault) != 0 ? currentTab : DataType.VaultDaily);
                        child.Show(false);
                    }

                    SetTabs(DataType.Daily);
                }
                else if (child != null)
                {
                    child.Dispose();
                    child = null;
                    SetTabs(DataType.Daily | DataType.Vault);
                }

                splitVaultObjectivesToolStripMenuItem.Checked = show;
            }
            else if (!show)
            {
                this.Dispose();
            }

            if (show)
            {
                Settings.Dailies.VaultOptions.Value |= Settings.DailiesVaultOptions.Split;
            }
            else
            {
                Settings.Dailies.VaultOptions.Value &= ~Settings.DailiesVaultOptions.Split;
            }
        }

        private void splitVaultObjectivesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var b = splitVaultObjectivesToolStripMenuItem.Checked;

            if (b)
            {
                Settings.WindowBounds[typeof(formDailiesVault)].Clear();
            }

            ShowSplitVaultObjectives(!b);
        }

        private void SetTabs(DataType types)
        {
            panelTabs.SuspendLayout();

            this.displayedTabs = types;

            var daily = (types & DataType.Daily) != 0;
            var vault = (types & DataType.Vault) != 0;

            buttonToday.Visible = daily && (buttonDaySwap.ShapeDirection == ArrowDirection.Right || !vault);
            buttonTomorrow.Visible = daily && (buttonDaySwap.ShapeDirection == ArrowDirection.Left || !vault);
            buttonDaySwap.Visible = daily && vault;

            if (daily)
            {
                buttonToday.Margin = vault ? Padding.Empty : buttonVault.Margin;
            }

            panelSep.Visible = daily && vault;

            buttonVault.Visible = vault;
            buttonWeekly.Visible = vault;
            buttonSpecial.Visible = vault;

            if (IsMainWindow)
            {
                splitVaultObjectivesToolStripMenuItem.Checked = !vault;
            }

            if (currentTab != DataType.None && (currentTab & types) == 0)
            {
                if (daily)
                {
                    SelectTab(DataType.DailyToday);
                }
                else if (vault)
                {
                    SelectTab(DataType.Vault);
                }
            }

            panelTabs.ResumeLayout();
        }

        private void SetOption(Settings.DailiesVaultOptions o, bool value)
        {
            if (value)
            {
                Settings.Dailies.VaultOptions.Value |= o;
            }
            else
            {
                Settings.Dailies.VaultOptions.Value &= ~o;
            }
        }

        private void autoSelectCurrentAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetOption(Settings.DailiesVaultOptions.AutoSelect, !autoSelectCurrentAccountToolStripMenuItem.Checked);
        }

        private void autoScrollToCurrentAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetOption(Settings.DailiesVaultOptions.AutoScroll, !autoScrollToCurrentAccountToolStripMenuItem.Checked);
        }

        private void RemoveInvalidItemKeys()
        {
            var keys = Settings.Dailies.ItemOptions.GetKeys();
            if (keys.Length == 0)
                return;

            byte[] ids = null;
            var combined = combineAccountsToolStripMenuItem.Checked;

            for (var i = 0; i < keys.Length; i++)
            {
                byte flag;

                switch (keys[i].Type)
                {
                    case Settings.DailiesKeyType.VaultDailyCategory:

                        flag = 1;

                        break;
                    case Settings.DailiesKeyType.VaultWeeklyCategory:

                        flag = 2;

                        break;
                    case Settings.DailiesKeyType.VaultDailyAccountCategory:
                    case Settings.DailiesKeyType.VaultWeeklyAccountCategory:

                        if (combined || !Settings.Accounts.Contains(keys[i].ID))
                        {
                            Settings.Dailies.ItemOptions.Remove(keys[i]);
                        }

                        continue;
                    default:

                        continue;
                }

                if (combined && keys[i].ID > 0 && keys[i].ID <= 255)
                {
                    if (ids == null)
                    {
                        //ids are from 1 to 255 (0 is none)

                        var v = Settings.ApiKeys.GetValues();
                        ids = new byte[256];

                        for (var j = 0; j < v.Length; j++)
                        {
                            if (v[j].HasValue)
                            {
                                var g = v[j].Value.Data.VaultGroup;

                                if (g.Daily > 0)
                                {
                                    ids[g.Daily] |= 1;
                                }
                                if (g.Weekly > 0)
                                {
                                    ids[g.Weekly] |= 2;
                                }
                            }
                        }
                    }

                    if ((ids[keys[i].ID] & flag) == flag)
                    {
                        continue;
                    }
                }

                Settings.Dailies.ItemOptions.Remove(keys[i]);
            }
        }

        private void sortingGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = (ToolStripMenuItem)sender;
            Settings.DailiesVaultSorting s;

            if (t == descendingToolStripMenuItem)
            {
                s = Settings.Dailies.VaultSorting.Value & Settings.DailiesVaultSorting.Sorting;

                if (!t.Checked)
                {
                    s |= Settings.DailiesVaultSorting.Descending;
                }
            }
            else
            {
                if (t.Checked)
                {
                    s = Settings.DailiesVaultSorting.None;
                }
                else if (t == groupToolStripMenuItem)
                {
                    s = Settings.DailiesVaultSorting.Group;
                }
                else if (t == focusedToolStripMenuItem)
                {
                    s = Settings.DailiesVaultSorting.Focused;
                }
                else if (t == accountToolStripMenuItem)
                {
                    s = Settings.DailiesVaultSorting.Account;
                }
                else
                {
                    return;
                }

                s |= Settings.Dailies.VaultSorting.Value & ~Settings.DailiesVaultSorting.Sorting;
            }

            Settings.Dailies.VaultSorting.Value = s;

            if ((currentTab & DataType.Vault) != 0 && Invalidate(currentTab))
            {
                Sort(data.groups, data.groups.Length, s & Settings.DailiesVaultSorting.Sorting, (s & Settings.DailiesVaultSorting.Descending) != 0);
                DoPendingReposition(0);
            }
        }

        private void ShowMenu(DataType t)
        {
            var dailies = (t & DataType.Daily) != 0;
            var vault = (t & DataType.Vault) != 0;

            categoriesToolStripMenuItem.Visible = dailies;
            favoritesToolStripMenuItem.Visible = dailies;
            ignoredToolStripMenuItem.Visible = dailies;
            showOnTopToolStripMenuItem.Visible = dailies;

            sortGroupsByToolStripMenuItem.Visible = vault;
            sortObjectivesByToolStripMenuItem.Visible = vault;
            autoScrollToCurrentAccountToolStripMenuItem.Visible = vault;
            autoSelectCurrentAccountToolStripMenuItem.Visible = vault;
            combineAccountsToolStripMenuItem.Visible = vault;

            contextMenu.Show(Cursor.Position);
        }

        private void ignoreDailyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var control = (DailyAchievement)contextMenuDaily.Tag;

            if (control.DataSource is AchievementDataSource)
            {
                var d = (AchievementDataSource)control.DataSource;
                var k = new Settings.DailiesItemKey(Settings.DailiesKeyType.DailyObjective, d.ID);

                Settings.Dailies.ItemOptions[k] = Settings.DailiesItemOptions.Ignored;

                ReloadDailies();
            }
        }

        private void sortingObjectivesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var t = (ToolStripMenuItem)sender;
            var v = Settings.Dailies.VaultObjectiveSorting.Value;
            Settings.DailiesVaultObjectiveSorting s;
            
            if (t == objectiveDescendingToolStripMenuItem)
            {
                s = Settings.DailiesVaultObjectiveSorting.Descending;
            }
            else if (t == objectiveIdToolStripMenuItem)
            {
                s = Settings.DailiesVaultObjectiveSorting.ID;
            }
            else if (t == objectiveNameToolStripMenuItem)
            {
                s = Settings.DailiesVaultObjectiveSorting.Name;
            }
            else if (t == objectiveProgressToolStripMenuItem)
            {
                s = Settings.DailiesVaultObjectiveSorting.Progress;
            }
            else
            {
                return;
            }

            if (t.Checked)
            {
                v &= ~s;
            }
            else
            {
                v |= s;
            }

            if (Settings.Dailies.VaultObjectiveSorting.Value != v)
            {
                Settings.Dailies.VaultObjectiveSorting.Value = v;

                if ((currentTab & DataType.Vault) != 0 && Invalidate(currentTab))
                {
                    SortObjectives(data.groups, data.groups.Length, v);
                    DoPendingReposition(0);
                }
            }
        }
    }
}
