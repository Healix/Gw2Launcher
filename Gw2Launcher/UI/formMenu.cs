using Gw2Launcher.UI.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formMenu : Base.StackFormBase
    {
        const int FADE_DURATION = 100;
        const string PAGE_ALL = "−";

        private class PageTextBox : Control
        {
            public event EventHandler ValueChanged;

            private Controls.IntegerTextBox textbox;

            public PageTextBox()
            {
                textbox = new Controls.IntegerTextBox()
                {
                    Visible = false,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Size = this.Size,
                    BorderStyle = System.Windows.Forms.BorderStyle.None,
                    BackColor = this.BackColor,
                    ForeColor = this.ForeColor,
                    TextAlign = HorizontalAlignment.Center,
                    ReverseMouseWheelDirection = true,
                    Minimum = 0,
                    Maximum = 99,
                };
                textbox.LostFocus += textbox_LostFocus;
                textbox.SizeChanged += textbox_SizeChanged;
                textbox.ValueChanged += textbox_ValueChanged;
                this.Controls.Add(textbox);
            }

            public Controls.IntegerTextBox TextBox
            {
                get
                {
                    return textbox;
                }
            }

            public int Value
            {
                get
                {
                    return textbox.Value;
                }
                set
                {
                    textbox.Value = value;
                }
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                base.OnBackColorChanged(e);

                textbox.BackColor = this.BackColor;
            }

            protected override void OnForeColorChanged(EventArgs e)
            {
                base.OnForeColorChanged(e);

                textbox.ForeColor = this.ForeColor;
            }

            void textbox_ValueChanged(object sender, EventArgs e)
            {
                this.Text = textbox.Value == 0 ? PAGE_ALL : textbox.Value.ToString();

                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }

            void textbox_SizeChanged(object sender, EventArgs e)
            {
                textbox.Location = new Point(0, this.Height / 2 - textbox.Height / 2);
            }

            void textbox_LostFocus(object sender, EventArgs e)
            {
                ShowTextBox(false);
            }

            public void ShowTextBox(bool visible)
            {
                if (visible)
                {
                    textbox.Visible = true;
                    textbox.Focus();
                }
                else
                {
                    textbox.Visible = false;
                }
                this.Invalidate();
            }

            public override string Text
            {
                get
                {
                    return base.Text;
                }
                set
                {
                    base.Text = value;
                    if (!textbox.Visible)
                        this.Invalidate();
                }
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);

                textbox.SelectAll();
                ShowTextBox(true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (!textbox.Visible)
                    TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textbox.Bounds, this.ForeColor, this.BackColor, TextFormatFlags.TextBoxControl | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

                base.OnPaint(e);
            }
        }

        private class MenuItemPanel : Controls.StackPanel
        {
        }

        private class MenuItemLabel : Base.BaseLabel
        {
            private bool hovered;

            public MenuItemLabel()
            {
                _BackColorHovered = Color.FromArgb(240, 240, 240);
            }

            private Color _BackColor;
            public override Color BackColor
            {
                get
                {
                    return _BackColor;
                }
                set
                {
                    _BackColor = value;
                    if (!hovered)
                        base.BackColor = value;
                }
            }

            private Color _BackColorHovered;
            [DefaultValue(typeof(Color), "240,240,240")]
            public Color BackColorHovered
            {
                get
                {
                    return _BackColorHovered;
                }
                set
                {
                    _BackColorHovered = value;
                    if (hovered)
                        base.BackColor = value;
                }
            }

            protected UiColors.Colors _BackColorHoveredName = UiColors.Colors.Custom;
            [DefaultValue(UiColors.Colors.Custom)]
            [TypeConverter(typeof(UiColorTypeConverter))]
            [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
            public UiColors.Colors BackColorHoveredName
            {
                get
                {
                    return _BackColorHoveredName;
                }
                set
                {
                    if (_BackColorHoveredName != value)
                    {
                        _BackColorHoveredName = value;
                        RefreshColors();
                    }
                }
            }

            private Color _BorderColor;
            [DefaultValue(typeof(Color), "")]
            public Color BorderColor
            {
                get
                {
                    return _BorderColor;
                }
                set
                {
                    _BorderColor = value;
                    if (!hovered)
                        this.Invalidate();
                }
            }

            protected UiColors.Colors _BorderColorName = UiColors.Colors.Custom;
            [DefaultValue(UiColors.Colors.Custom)]
            [TypeConverter(typeof(UiColorTypeConverter))]
            [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
            public UiColors.Colors BorderColorName
            {
                get
                {
                    return _BorderColorName;
                }
                set
                {
                    if (_BorderColorName != value)
                    {
                        _BorderColorName = value;
                        RefreshColors();
                    }
                }
            }

            private Color _BorderColorHovered;
            [DefaultValue(typeof(Color), "")]
            public Color BorderColorHovered
            {
                get
                {
                    return _BorderColorHovered;
                }
                set
                {
                    _BorderColorHovered = value;
                    if (hovered)
                        this.Invalidate();
                }
            }

            protected UiColors.Colors _BorderColorHoveredName = UiColors.Colors.Custom;
            [DefaultValue(UiColors.Colors.Custom)]
            [TypeConverter(typeof(UiColorTypeConverter))]
            [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
            public UiColors.Colors BorderColorHoveredName
            {
                get
                {
                    return _BorderColorHoveredName;
                }
                set
                {
                    if (_BorderColorHoveredName != value)
                    {
                        _BorderColorHoveredName = value;
                        RefreshColors();
                    }
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                hovered = true;
                base.BackColor = BackColorHovered;
                this.Invalidate();

                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                hovered = false;
                base.BackColor = _BackColor;
                this.Invalidate();

                base.OnMouseLeave(e);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                var g = e.Graphics;

                g.Clear(base.BackColor);

                var bc = hovered ? _BorderColorHovered : _BorderColor;

                if (bc.A > 0)
                {
                    using (var pen = new Pen(bc, 1))
                    {
                        var w = this.Width - 1;
                        var h = this.Height - 1;

                        g.DrawLine(pen, 0, 0, w, 0);
                        g.DrawLine(pen, 0, h, w, h);
                    }
                }
            }

            public override void RefreshColors()
            {
                base.RefreshColors();

                if (_BackColorHoveredName != UiColors.Colors.Custom)
                    this.BackColorHovered = UiColors.GetColor(_BackColorHoveredName);
                if (_BorderColorName != UiColors.Colors.Custom)
                    this.BorderColor = UiColors.GetColor(_BorderColorName);
                if (_BorderColorHoveredName != UiColors.Colors.Custom)
                    this.BorderColorHovered = UiColors.GetColor(_BorderColorHoveredName);
            }
        }

        private class Shadow : Form
        {
            protected override bool ShowWithoutActivation
            {
                get
                {
                    return true;
                }
            }

            public int OffsetY
            {
                get;
                set;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x00080000; //WS_EX_LAYERED
                    return cp;
                }
            }
        }

        public event EventHandler<byte> PageChanged;
        public event EventHandler<MenuItem> MenuItemSelected;

        private event EventHandler Fading;

        public enum MenuItem
        {
            Settings,
            Search,
            CloseAllAccounts,
        }

        private Point[] background;
        private BufferedGraphics buffer;
        private bool redraw;
        private Control attachedTo;
        private Shadow shadow;
        private bool created;
        private bool animated;
        private PopupUtil popup;
        private int activated;

        public formMenu(Form owner)
        {
            InitializeComponents();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            animated = Windows.Native.NativeMethods.IsDwmCompositionEnabled();

            _Page = 0;
            _Pages = 1;
            textPage.Text = PAGE_ALL;

            this.TransparencyKey = Color.Magenta;

            redraw = true;

            textPage.ValueChanged += textPage_ValueChanged;
            panelPage.MouseWheel += page_MouseWheel;
            this.Disposed += formMenu_Disposed;

            popup = new PopupUtil(owner, this);
            popup.Deactivating += popup_Deactivating;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            textPage.ForeColor = this.ForeColor;
        }

        protected override void SetVisibleCore(bool value)
        {
            if (value)
                popup.Enabled = true;
            base.SetVisibleCore(value);
        }

        void formMenu_Disposed(object sender, EventArgs e)
        {
            if (shadow != null)
                shadow.Dispose();
            if (buffer != null)
                buffer.Dispose();
        }

        private void textPage_ValueChanged(object sender, EventArgs e)
        {
            Page = (byte)textPage.Value;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                OnHide();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:

                    if (!textPage.ContainsFocus)
                        ShowPageInput((byte)(keyData - Keys.D0));

                    break;
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        private byte _Page;
        /// <summary>
        /// Page to display; note page 0 displays all pages
        /// </summary>
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
                    textPage.Value = value;
                    //if (value == 0)
                    //    textPage.Text = PAGE_ALL;
                    //else
                    //    textPage.Text = value.ToString();
                    //if (value > _Pages)
                    //    Pages = value;

                    if (PageChanged != null)
                        PageChanged(this, value);
                }
            }
        }

        private byte _Pages;
        public byte Pages
        {
            get
            {
                return _Pages;
            }
            set
            {
                if (_Pages != value)
                {
                    _Pages = value;
                    if (_Page > value)
                        Page = value;
                }
            }
        }

        private bool _ShowCloseAll;
        public bool ShowCloseAll
        {
            get
            {
                return _ShowCloseAll;
            }
            set
            {
                if (_ShowCloseAll != value)
                {
                    _ShowCloseAll = value;

                    panelContainer.SuspendLayout();
                    labelCloseAll.Visible = value;
                    panelCloseAllSep.Visible = value;
                    panelContainer.ResumeLayout();
                }
            }
        }

        public void PageNext()
        {
            if (_Page >= _Pages)
                Page = 0;
            else
                ++Page;
        }

        public void PagePrevious()
        {
            if (_Page == 0)
                Page = _Pages;
            else
                --Page;
        }

        void formMenu_SizeChanged(object sender, EventArgs e)
        {
            shadow.Bounds = new Rectangle(this.Left, this.Top + shadow.OffsetY, this.Width, this.Height - shadow.OffsetY);
        }

        void formMenu_LocationChanged(object sender, EventArgs e)
        {
            shadow.Bounds = new Rectangle(this.Left, this.Top + shadow.OffsetY, this.Width, this.Height - shadow.OffsetY);
        }

        void formMenu_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
                shadow.Hide();
        }

        void page_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e is HandledMouseEventArgs)
                ((HandledMouseEventArgs)e).Handled = true;

            if (e.Delta > 0)
            {
                PagePrevious();
            }
            else
            {
                PageNext();
            }
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
            background = null;

            this.Invalidate();
        }

        public void Show(Form owner, Control attachTo)
        {
            activated = Environment.TickCount;

            var l = attachTo.PointToScreen(Point.Empty);
            var arrowSize = panelContainer.Top - 1;

            l = new Point(l.X + attachTo.Width / 2 - arrowSize * 2 - 1, l.Y + attachTo.Height - attachTo.Height / 8 - arrowSize - attachTo.Height / 4);

            this.Location = l;
            this.Opacity = 0;
            this.attachedTo = attachTo;

            if (!created && animated)
            {
                created = true;

                shadow = new Shadow()
                {
                    FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                    StartPosition = FormStartPosition.Manual,
                    ShowInTaskbar = false,
                    OffsetY = arrowSize,
                    Bounds = new Rectangle(l.X, l.Y + arrowSize, this.Width, this.Height - arrowSize),
                    Opacity = this.Opacity,
                    BackColor = this.TransparencyKey,
                    TransparencyKey = this.TransparencyKey,
                };

                if (!Settings.StyleDisableWindowShadows.Value && Windows.WindowShadow.Enable(shadow.Handle))
                {
                    this.LocationChanged += formMenu_LocationChanged;
                    this.SizeChanged += formMenu_SizeChanged;
                    this.VisibleChanged += formMenu_VisibleChanged;
                }
                else
                {
                    shadow.Dispose();
                    shadow = null;
                }
            }

            if (shadow != null)
            {
                shadow.Opacity = 0;

                shadow.Show(owner);
                this.Show(shadow);
            }
            else
            {
                this.Show(owner);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                var g = buffer.Graphics;
                var pw = (int)(GetScaling() + 0.5f);

                if (background == null)
                {
                    int l = 0,
                        t = 0,
                        r = this.Width,
                        b = this.Height;
                    if (pw <= 1)
                    {
                        --r;
                        --b;
                    }
                    var arrowSize = panelContainer.Top - pw;
                    var arrowBounds = new Rectangle(arrowSize + 1, t, arrowSize * 2, arrowSize);

                    background = new Point[]
                    {
                        new Point(l,arrowBounds.Bottom),
                        new Point(arrowBounds.Left,arrowBounds.Bottom),
                        new Point(arrowBounds.Left + arrowBounds.Width / 2, arrowBounds.Top),
                        new Point(arrowBounds.Right, arrowBounds.Bottom),
                        new Point(r, arrowBounds.Bottom),
                        new Point(r, b),
                        new Point(l,b)
                    };
                }

                g.Clear(this.TransparencyKey);

                using (var brush = new SolidBrush(this.BackColor))
                {
                    g.FillPolygon(brush, background);
                }

                using (var pen = new Pen(UiColors.GetColor(UiColors.Colors.MainBorder), pw)
                    {
                        Alignment = System.Drawing.Drawing2D.PenAlignment.Inset,
                    })
                {
                    g.DrawPolygon(pen, background);
                }
            }

            buffer.Render(e.Graphics);
        }

        private void arrowLeft_MouseDown(object sender, MouseEventArgs e)
        {
            PagePrevious();
        }

        private void arrowRight_MouseDown(object sender, MouseEventArgs e)
        {
            PageNext();
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                Windows.Native.NativeMethods.BringWindowToTop(this.Handle);
            }
            base.OnVisibleChanged(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (this.Opacity < 1 && Fading == null)
            {
                var t = FadeTo(1, this.Top + attachedTo.Height / 4, FADE_DURATION);
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
        }

        void popup_Deactivating(object sender, EventArgs e)
        {
            //backup for when the menu is deactivated on show (linux)
            if (Environment.TickCount - activated < 100)
            {
                var w = Gw2Launcher.Windows.Native.NativeMethods.GetForegroundWindow();
                if (w == popup.Owner.Handle || w == this.Handle || w == IntPtr.Zero)
                {
                    if (!Gw2Launcher.Windows.Native.NativeMethods.SetForegroundWindow(this.Handle))
                    {
                        AutoHide();
                    }
                    else
                    {
                        popup.Deactivated = false;
                    }
                    return;
                }
            }

            HidePopup();
        }

        private async void AutoHide()
        {
            var wasActive = false;
            var boundsChanged = true;
            var bounds = new Rectangle();

            EventHandler onActivated = delegate
            {
                wasActive = true;
            };
            EventHandler onBoundsChanged = delegate
            {
                boundsChanged = true;
            };

            this.Activated += onActivated;
            this.SizeChanged += onBoundsChanged;
            this.LocationChanged += onBoundsChanged;

            while (true)
            {
                await Task.Delay(500);

                if (this.IsDisposed)
                    return;
                if (wasActive)
                    break;
                if (boundsChanged)
                {
                    boundsChanged = false;
                    bounds = Rectangle.Inflate(this.Bounds, Cursor.Size.Width / 2, Cursor.Size.Height / 2);
                }
                if (!bounds.Contains(Cursor.Position))
                {
                    HidePopup();
                    break;
                }
            }

            this.Activated -= onActivated;
            this.SizeChanged -= onBoundsChanged;
            this.LocationChanged -= onBoundsChanged;
        }

        public void HidePopup()
        {
            if (this.Visible)
                OnHide();
        }

        private async void OnHide()
        {
            if (await FadeTo(0, this.Top, FADE_DURATION))
            {
                if (shadow != null)
                    shadow.Hide();
                this.Hide();
            }
        }

        private async Task<bool> FadeTo(double to, int toY, int duration)
        {
            if (Fading != null)
                Fading(this, EventArgs.Empty);

            var from = this.Opacity;
            var fromY = this.Location.Y;
            if (from == to && fromY == toY)
                return true;

            var _duration = (to < from ? from - to : to - from) * duration;
            var abort = false;
            var first = from < 1;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            EventHandler onChange = delegate
            {
                abort = true;
            };

            EventHandler onVisibleChanged = delegate
            {
                if (!this.Visible)
                    abort = true;
            };

            this.Disposed += onChange;
            this.Fading += onChange;
            this.VisibleChanged += onVisibleChanged;

            do
            {
                if (duration > 0)
                    await Task.Delay(10);

                if (abort)
                    break;

                if (first)
                {
                    this.Update();
                    first = false;

                    if (!animated)
                        this.Opacity = to;
                }

                var p = sw.ElapsedMilliseconds / _duration;

                if (p >= 1)
                {
                    this.Opacity = to;
                    this.Top = toY;

                    if (shadow != null)
                    {
                        shadow.Opacity = to;
                    }

                    break;
                }
                else
                {
                    var o = from + (to - from) * p;

                    if (animated)
                        this.Opacity = o;
                    this.Top = fromY + (int)((toY - fromY) * p);

                    if (shadow != null)
                    {
                        shadow.Opacity = o;
                    }
                }
            }
            while (true);

            this.Disposed -= onChange;
            this.Fading -= onChange;
            this.VisibleChanged -= onVisibleChanged;

            return !abort;
        }

        private void OnMenuItemSelected(MenuItem i)
        {
            OnHide();

            if (MenuItemSelected != null)
                MenuItemSelected(this, i);
        }

        private void labelSearch_Click(object sender, EventArgs e)
        {
            OnMenuItemSelected(MenuItem.Search);
        }

        private void labelSettings_Click(object sender, EventArgs e)
        {
            OnMenuItemSelected(MenuItem.Settings);
        }

        private void labelCloseAll_Click(object sender, EventArgs e)
        {
            OnMenuItemSelected(MenuItem.CloseAllAccounts);
        }

        public void ShowPageInput(byte page)
        {
            textPage.Value = page;
            textPage.TextBox.Select(textPage.Text.Length, 0);
            textPage.ShowTextBox(true);
        }

        public void ShowPageInput(string input = null)
        {
            byte v;
            if (input != null && byte.TryParse(input, out v))
            {
                ShowPageInput(v);
            }
            else
            {
                textPage.TextBox.SelectAll();
                textPage.ShowTextBox(true);
            }
        }

        public override void RefreshColors()
        {
            base.RefreshColors();

            redraw = true;
            this.Invalidate();
        }
    }
}
