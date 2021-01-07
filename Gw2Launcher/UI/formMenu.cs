using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        private class MenuItemPanel : Panel
        {
        }

        private class MenuItemLabel : Label
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

            private Color _BorderColor;
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

            private Color _BorderColorHovered;
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

        public formMenu()
        {
            InitializeComponents();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            animated = Windows.Native.NativeMethods.IsDwmCompositionEnabled();

            _Page = 0;
            _Pages = 1;
            labelPage.Text = PAGE_ALL;

            this.TransparencyKey = Color.Magenta;

            redraw = true;

            panelPage.MouseWheel += page_MouseWheel;
            this.Disposed += formMenu_Disposed;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void formMenu_Disposed(object sender, EventArgs e)
        {
            if (shadow != null)
                shadow.Dispose();
            if (buffer != null)
                buffer.Dispose();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                OnHide();
            }
            return base.ProcessCmdKey(ref msg, keyData);
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
                    if (value == 0)
                        labelPage.Text = PAGE_ALL;
                    else
                        labelPage.Text = value.ToString();
                    if (value > _Pages)
                        Pages = value;

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

                if (Windows.WindowShadow.Enable(shadow.Handle))
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
                var pw = (int)(g.DpiX / 96f + 0.5f);

                if (background == null)
                {
                    int l = pw / 2,
                        t = pw / 2,
                        r = this.Width - (pw + 1) / 2,
                        b = this.Height - (pw + 1) / 2;
                    var arrowSize = panelContainer.Top - 1;
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

                using (var pen = new Pen(Color.Gray, pw))
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

    }
}
