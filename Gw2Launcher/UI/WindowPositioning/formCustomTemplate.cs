using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.WindowPositioning
{
    public partial class formCustomTemplate : Base.BaseForm
    {
        private const int SIZING_SIZE = 10;

        public class WindowRects
        {
            public WindowRects(Rectangle screen, Rectangle[] windows)
            {
                this.Screen = screen;
                this.Windows = windows;
            }

            public Rectangle Screen
            {
                get;
                private set;
            }

            public Rectangle[] Windows
            {
                get;
                private set;
            }
        }

        private class WindowBlock : Form
        {
            private BufferedGraphics background;
            private bool redraw;

            public WindowBlock()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.UserPaint, true);

                this.TransparencyKey = this.BackColor = Color.Black;
                this.ForeColor = Color.FromArgb(20, 20, 20);
                this.StartPosition = FormStartPosition.Manual;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;

                redraw = true;
                Opacity = 0.85;

                this.Disposed += WindowBlock_Disposed;
                this.MouseMove += WindowBlock_MouseMove;
                this.MouseDown += WindowBlock_MouseDown;
            }

            void WindowBlock_MouseCaptureChanged(object sender, EventArgs e)
            {
                this.MouseMove += WindowBlock_MouseMove;
                this.MouseCaptureChanged -= WindowBlock_MouseCaptureChanged;
            }

            void WindowBlock_MouseDown(object sender, MouseEventArgs e)
            {
                this.MouseCaptureChanged += WindowBlock_MouseCaptureChanged;
                this.MouseMove -= WindowBlock_MouseMove;
            }

            void WindowBlock_MouseMove(object sender, MouseEventArgs e)
            {
                var a = AnchorStyles.None;

                if (e.X < SIZING_SIZE)
                {
                    a = AnchorStyles.Left;
                }
                else if (e.X > this.Width - SIZING_SIZE)
                {
                    a = AnchorStyles.Right;
                }

                if (e.Y < SIZING_SIZE)
                {
                    a |= AnchorStyles.Top;
                }
                else if (e.Y > this.Height - SIZING_SIZE)
                {
                    a |= AnchorStyles.Bottom;
                }

                switch (a)
                {
                    case AnchorStyles.None:
                        this.Cursor = Cursors.Default;
                        break;
                    case AnchorStyles.Left:
                    case AnchorStyles.Right:
                        this.Cursor = Cursors.SizeWE;
                        break;
                    case AnchorStyles.Top:
                    case AnchorStyles.Bottom:
                        this.Cursor = Cursors.SizeNS;
                        break;
                    case AnchorStyles.Left | AnchorStyles.Top:
                    case AnchorStyles.Right | AnchorStyles.Bottom:
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                    case AnchorStyles.Left | AnchorStyles.Bottom:
                    case AnchorStyles.Right | AnchorStyles.Top:
                        this.Cursor = Cursors.SizeNESW;
                        break;
                }
            }

            void WindowBlock_Disposed(object sender, EventArgs e)
            {
                if (background != null)
                    background.Dispose();
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= (int)(WindowStyle.WS_EX_COMPOSITED | WindowStyle.WS_EX_LAYERED);
                    return cp;
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                Hovered = true;
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                Hovered = false;
                base.OnMouseLeave(e);
            }

            protected override void OnActivated(EventArgs e)
            {
                Selected = true;
                base.OnActivated(e);
            }

            protected override void OnDeactivate(EventArgs e)
            {
                Selected = false;
                base.OnDeactivate(e);
            }

            protected override void OnLocationChanged(EventArgs e)
            {
                base.OnLocationChanged(e);

                OnRedrawRequired();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);

                if (background != null)
                {
                    background.Dispose();
                    background = null;
                }
                OnRedrawRequired();
            }

            private void OnRedrawRequired()
            {
                if (redraw)
                    return;
                redraw = true;
                this.Invalidate();
            }

            private bool _Hovered;
            public bool Hovered
            {
                get
                {
                    return _Hovered;
                }
                set
                {
                    if (_Hovered != value)
                    {
                        _Hovered = value;
                        if (!_Selected && !_Error)
                            OnRedrawRequired();
                    }
                }
            }

            private bool _Error;
            public bool Error
            {
                get
                {
                    return _Error;
                }
                set
                {
                    if (_Error != value)
                    {
                        _Error = value;
                        OnRedrawRequired();
                    }
                }
            }

            private bool _Selected;
            public bool Selected
            {
                get
                {
                    return _Selected;
                }
                set
                {
                    if (_Selected != value)
                    {
                        _Selected = value;
                        if (!_Error)
                            OnRedrawRequired();
                    }
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                if (redraw)
                {
                    redraw = false;

                    if (background == null)
                        background = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                    var g = background.Graphics;
                    var scale = g.DpiX / 96f;

                    Color c;

                    if (_Error)
                    {
                        g.Clear(Color.FromArgb(235, 201, 217));
                        c = Color.FromArgb(189, 153, 170);
                    }
                    else if (_Selected)
                    {
                        g.Clear(Color.FromArgb(201, 217, 235));
                        c = Color.FromArgb(153, 170, 189);
                    }
                    else if (_Hovered)
                    {
                        g.Clear(Color.FromArgb(220, 220, 220));
                        c = Color.FromArgb(160, 160, 160);
                    }
                    else
                    {
                        g.Clear(Color.FromArgb(200, 200, 200));
                        c = Color.FromArgb(140, 140, 140);
                    }

                    using (var p = new Pen(c, 2))
                    {
                        g.DrawRectangle(p, 1, 1, this.Width - 2, this.Height - 2);
                    }

                    var o  = -(int)(5 * scale + 0.5f);
                    var r = Rectangle.Inflate(this.ClientRectangle, o, o);

                    TextRenderer.DrawText(g, this.Left + ", " + this.Top, this.Font, r, this.ForeColor, TextFormatFlags.Left | TextFormatFlags.Top);
                    TextRenderer.DrawText(g, this.Width + " x " + this.Height, this.Font, r, this.ForeColor, TextFormatFlags.Right | TextFormatFlags.Bottom);
                }

                background.Render(e.Graphics);
            }
        }

        private BufferedGraphics background;
        private bool redraw;

        private Screen[] screens;
        private formCustomTemplate[] backgrounds;
        private formCustomTemplate master;
        private WindowBlock activeWindow;
        private Size minimum;
        private List<WindowBlock> windows;
        private Point origin;
        private int[] offset;
        private AnchorStyles sizing;
        private Rectangle currentScreen;
        private Rectangle originRect;

        /// <summary>
        /// Initializes windows with relative positioning
        /// </summary>
        public formCustomTemplate(params WindowRects[] windows)
            : this((formCustomTemplate)null)
        {
            foreach (var wr in windows)
            {
                var screen = Screen.FromRectangle(wr.Screen).Bounds;

                foreach (var w in wr.Windows)
                {
                    var wb = new WindowBlock()
                    {
                        Bounds = Util.ScreenUtil.Constrain(new Rectangle(screen.X + w.X, screen.Y + w.Y, w.Width, w.Height), screen),
                        MinimumSize = minimum,
                    };

                    this.windows.Add(wb);
                }
            }
        }

        /// <summary>
        /// Initializes windows with absolute positioning
        /// </summary>
        public formCustomTemplate(Size minimum, params Rectangle[] windows)
            : this((formCustomTemplate)null)
        {
            this.minimum = new Size(minimum.Width > 0 ? minimum.Width : 200, minimum.Height > 0 ? minimum.Height : 200);

            if (windows != null)
            {
                foreach (var w in windows)
                {
                    var wb = new WindowBlock()
                    {
                        Bounds = w,
                        MinimumSize = minimum,
                    };

                    this.windows.Add(wb);
                }
            }
        }

        private formCustomTemplate(formCustomTemplate master)
        {
            InitializeComponents();

            redraw = true;
            Opacity = 0.85;
            TopMost = true;

            if (master == null)
            {
                screens = Screen.AllScreens;
                backgrounds = new formCustomTemplate[screens.Length];

                windows = new List<WindowBlock>();
                _GridSize = new System.Drawing.Size(10, 10);

                backgrounds[0] = this;
                this.Bounds = screens[0].Bounds;

                this.MouseDown += formCustomTemplate_MouseDown;

                for (var i = 1; i < backgrounds.Length; i++)
                {
                    backgrounds[i] = new formCustomTemplate(this)
                    {
                        Bounds = screens[i].Bounds,
                        _GridSize = _GridSize,
                    };
                    backgrounds[i].MouseDown += formCustomTemplate_MouseDown;
                }
            }
            else
            {
                this.master = master;
            }

            this.Disposed += formCustomTemplate_Disposed;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        void formCustomTemplate_Disposed(object sender, EventArgs e)
        {
            if (background != null)
                background.Dispose();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= (int)(WindowStyle.WS_EX_COMPOSITED | WindowStyle.WS_EX_LAYERED);
                return cp;
            }
        }

        void formCustomTemplate_MouseDown(object sender, MouseEventArgs e)
        {
            var f = (Form)sender;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var p = Cursor.Position;

                currentScreen = Screen.FromPoint(p).Bounds;
                originRect = new Rectangle(p.X - 4, p.Y - 4, 8, 8);

                activeWindow = new WindowBlock()
                {
                    Location = new Point((p.X - currentScreen.X) / _GridSize.Width * _GridSize.Width + currentScreen.X, (p.Y - currentScreen.Y) / _GridSize.Height * _GridSize.Height + currentScreen.Y),
                    Error = true,
                    Selected = true,
                    MinimumSize = _GridSize,
                };

                var h = activeWindow.Handle;
                activeWindow.Size = _GridSize;
                activeWindow.Show(backgrounds[backgrounds.Length - 1]);

                f.MouseMove += formCustomTemplate_MouseMove;
                f.MouseCaptureChanged += formCustomTemplate_MouseCaptureChanged;
                activeWindow.SizeChanged += activeWindow_SizeChanged;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                NativeMethods.ReleaseCapture();

                deleteToolStripMenuItem.Enabled = false;
                contextMenu.Show(Cursor.Position);
            }
        }

        void activeWindow_SizeChanged(object sender, EventArgs e)
        {
            var c = (WindowBlock)sender;

            if (c.Error)
            {
                c.MinimumSize = new Size(c.Width > minimum.Width ? minimum.Width : c.Width, c.Height > minimum.Height ? minimum.Height : c.Height);
                if (c.MinimumSize == minimum)
                    c.Error = false;
            }
            else
            {
                c.SizeChanged -= activeWindow_SizeChanged;
            }
        }

        void formCustomTemplate_MouseCaptureChanged(object sender, EventArgs e)
        {
            var f = (Form)sender;

            if (activeWindow.Error)
            {
                activeWindow.Dispose();
            }
            else
            {
                windows.Add(activeWindow);
                activeWindow.MouseDown += activeWindow_MouseDown;
                activeWindow.FormClosing += activeWindow_FormClosing;
                activeWindow.KeyDown += activeWindow_KeyDown;
                activeWindow = null;
            }

            f.MouseMove -= formCustomTemplate_MouseMove;
            f.MouseCaptureChanged -= formCustomTemplate_MouseCaptureChanged;
        }

        void activeWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.Focus();

                var f = (WindowBlock)sender;
                windows.Remove(f);
                f.Dispose();
            }
        }

        private int SnapX(int x)
        {
            if (_GridSize.Width > 1)
            {
                var sx = (int)((float)(x - currentScreen.X) / _GridSize.Width + 0.5f) * _GridSize.Width + currentScreen.X;
                var dx = currentScreen.Right - x;

                if (dx < 0)
                    dx = -dx;
                if (dx < _GridSize.Width / 2)
                    sx = currentScreen.Right;

                return sx;
            }

            return x;
        }

        private int SnapY(int y)
        {
            if (_GridSize.Height > 1)
            {
                var sy = (int)((float)(y - currentScreen.Y) / _GridSize.Height + 0.5f) * _GridSize.Height + currentScreen.Y;
                var dy = currentScreen.Bottom - y;

                if (dy < 0)
                    dy = -dy;
                if (dy < _GridSize.Height / 2)
                    sy = currentScreen.Bottom;

                return sy;
            }

            return y;
        }

        void activeWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Close();
            }
        }

        void activeWindow_MouseDown(object sender, MouseEventArgs e)
        {
            var c = (Form)sender;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                origin = Cursor.Position;
                sizing = AnchorStyles.None;
                currentScreen = Screen.FromPoint(origin).Bounds;
                originRect = new Rectangle(origin.X - 4, origin.Y - 4, 8, 8);

                if (e.X < SIZING_SIZE)
                {
                    sizing = AnchorStyles.Left;
                }
                else if (e.X > c.Width - SIZING_SIZE)
                {
                    sizing = AnchorStyles.Right;
                }

                if (e.Y < SIZING_SIZE)
                {
                    sizing |= AnchorStyles.Top;
                }
                else if (e.Y > c.Height - SIZING_SIZE)
                {
                    sizing |= AnchorStyles.Bottom;
                }

                if (sizing == AnchorStyles.None)
                    offset = new int[] { c.Left, c.Top };
                else
                    offset = new int[] { c.Left, c.Top, c.Right, c.Bottom };

                c.MouseMove += activeWindow_MouseMove;
                c.MouseCaptureChanged += activeWindow_MouseCaptureChanged;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                NativeMethods.ReleaseCapture();

                deleteToolStripMenuItem.Enabled = true;
                contextMenu.Show(Cursor.Position);
            }
        }

        void activeWindow_MouseCaptureChanged(object sender, EventArgs e)
        {
            var f = (Form)sender;

            f.MouseMove -= activeWindow_MouseMove;
            f.MouseCaptureChanged -= activeWindow_MouseCaptureChanged;
        }

        void activeWindow_MouseMove(object sender, MouseEventArgs e)
        {
            var f = (Form)sender;
            var p = Cursor.Position;
            if (originRect.Width != 0)
            {
                if (originRect.Contains(p))
                    return;
                originRect = Rectangle.Empty;
            }
            if (!currentScreen.Contains(p))
                currentScreen = Screen.FromPoint(p).Bounds;

            if (sizing == AnchorStyles.None)
            {
                f.Location = new Point(SnapX(p.X - origin.X + offset[0]), SnapY(p.Y - origin.Y + offset[1]));
            }
            else
            {
                int x, y, r, b;

                x = offset[0];
                y = offset[1];
                r = offset[2];
                b = offset[3];

                switch (sizing & (AnchorStyles.Left | AnchorStyles.Right))
                {
                    case AnchorStyles.Left:
                        x = SnapX(p.X - origin.X + x);
                        if (x + minimum.Width > r)
                            x = r - minimum.Width;
                        break;
                    case AnchorStyles.Right:
                        r = SnapX(p.X - origin.X + r);
                        break;
                }

                switch (sizing & (AnchorStyles.Top | AnchorStyles.Bottom))
                {
                    case AnchorStyles.Top:
                        y = SnapY(p.Y - origin.Y + y);
                        if (y + minimum.Height > b)
                            y = b - minimum.Height;
                        break;
                    case AnchorStyles.Bottom:
                        b = SnapY(p.Y - origin.Y + b);
                        break;
                }

                f.Bounds = new Rectangle(x, y, r - x, b - y);
            }
        }

        void formCustomTemplate_MouseMove(object sender, MouseEventArgs e)
        {
            var p = Cursor.Position;
            if (!currentScreen.Contains(p))
                currentScreen = Screen.FromPoint(p).Bounds;

            var w = SnapX(p.X) - activeWindow.Left;
            var h = SnapY(p.Y) - activeWindow.Top;

            if (w < _GridSize.Width)
                w = _GridSize.Width;

            if (h < _GridSize.Height)
                h = _GridSize.Height;

            activeWindow.Size = new Size(w, h);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (backgrounds != null)
            {
                for (var i = 1; i < backgrounds.Length; i++)
                {
                    backgrounds[i].Show(backgrounds[i - 1]);
                }

                var last = backgrounds[backgrounds.Length - 1];

                foreach (var w in windows)
                {
                    w.Show(last);

                    w.MouseDown += activeWindow_MouseDown;
                    w.FormClosing += activeWindow_FormClosing;
                    w.KeyDown += activeWindow_KeyDown;
                }

                saveToolStripMenuItem.Enabled = Overwrite;

                this.Focus();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (backgrounds == null)
            {
                if (e.CloseReason == CloseReason.UserClosing && master != null)
                {
                    master.Close();
                }
            }
            else
            {
                for (var i = 1; i < backgrounds.Length; i++)
                {
                    backgrounds[i].Dispose();
                }

                foreach (var w in windows)
                {
                    w.Dispose();
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (background != null)
            {
                background.Dispose();
                background = null;
            }
            OnRedrawRequired();
        }

        private void OnRedrawRequired()
        {
            if (redraw)
                return;
            redraw = true;
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (background == null)
                    background = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                var g = background.Graphics;

                g.Clear(this.BackColor);

                using (var pen = new Pen(Color.FromArgb(240, 240, 240)))
                {
                    if (_GridSize.Width > 2)
                    {
                        for (int x = _GridSize.Width, w = this.Width - 1; x < w; x += _GridSize.Width)
                        {
                            g.DrawLine(pen, x, 0, x, this.Height);
                        }
                    }

                    if (_GridSize.Height > 2)
                    {
                        for (int y = _GridSize.Height, h = this.Height - 1; y < h; y += _GridSize.Height)
                        {
                            g.DrawLine(pen, 0, y, this.Width, y);
                        }
                    }
                }
            }

            background.Render(e.Graphics);
        }

        private Size _GridSize;
        private Size GridSize
        {
            get
            {
                return _GridSize;
            }
            set
            {
                if (master == null)
                {
                    foreach (var f in backgrounds)
                    {
                        f._GridSize = value;
                        f.OnRedrawRequired();
                    }
                }
                else
                {
                    master.GridSize = value;
                }
            }
        }

        public WindowRects[] Result
        {
            get;
            private set;
        }

        public bool Overwrite
        {
            get;
            set;
        }

        private WindowRects[] GetResult()
        {
            var screens = new Dictionary<Screen, List<Rectangle>>();
            List<Rectangle> rects = null;
            var current = Rectangle.Empty;

            foreach (var w in windows)
            {
                var r = w.Bounds;

                if (!current.Contains(r))
                {
                    var screen = Screen.FromRectangle(r);
                    if (!screens.TryGetValue(screen, out rects))
                        screens[screen] = rects = new List<Rectangle>();
                }

                rects.Add(r);
            }

            var result = new WindowRects[screens.Count];
            var i = 0;

            foreach (var screen in screens.Keys)
            {
                var s = screens[screen];
                var rs = new Rectangle[s.Count];

                for (var j = rs.Length - 1; j >= 0; --j )
                {
                    rs[j] = new Rectangle(s[j].X - screen.Bounds.X, s[j].Y - screen.Bounds.Y, s[j].Width, s[j].Height);
                }

                result[i++] = new WindowRects(screen.Bounds, rs);
            }

            return result;
        }

        private void saveAsNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Overwrite = false;
            Result = GetResult();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Overwrite = true;
            Result = GetResult();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var w in windows)
            {
                if (w.Focused)
                {
                    this.Focus();
                    windows.Remove(w);
                    w.Dispose();
                    break;
                }
            }
        }

        private void gridSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new formGridSize(_GridSize))
            {
                f.StartPosition = FormStartPosition.Manual;
                f.Location = Util.ScreenUtil.CenterScreen(f.Size, Screen.FromPoint(Cursor.Position));

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    GridSize = f.Value;
                }
            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
