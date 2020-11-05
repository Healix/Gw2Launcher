using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    public partial class formAccountTooltip : Base.BaseForm
    {
        //private const int INNER_PADDING = 10;
        //private const int ARROW_SIZE_BASE = 20;
        //private const int ARROW_SIZE_HEIGHT = ARROW_SIZE_BASE / 2;

        private bool invoking;
        private CancellationTokenSource cancelToken;

        private AnchorStyles arrowAnchor;
        private Rectangle arrowBounds;
        private Point[] background;
        private Rectangle clientArea;
        private Size clientAreaSize;

        private string message;

        private Pen pen;
        private SolidBrush brush;
        private BufferedGraphics buffer;
        private bool redraw;

        private Rectangle attachedRect;
        private Control attachedTo;
        private Control attachedBoundary;
        private int attachedPadding;
        private AnchorStyles defaultArrowAnchor;

        public formAccountTooltip()
        {
            InitializeComponents();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            brush = new SolidBrush(Color.Black);
            pen = new Pen(brush);

            this.BackColor = Color.White;
            this.TransparencyKey = Color.Red;

            arrowAnchor = AnchorStyles.Left;
            redraw = true;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                cp.ExStyle |= (int)(WindowStyle.WS_EX_NOACTIVATE | WindowStyle.WS_EX_TOOLWINDOW | WindowStyle.WS_EX_TRANSPARENT);

                return cp;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
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
                pen.Dispose();
                brush.Dispose();
                if (buffer != null)
                    buffer.Dispose();
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

            OnRedrawRequired();
        }

        private void OnRedrawRequired()
        {
            if (redraw)
                return;

            redraw = true;
            this.Invalidate();
        }

        /// <summary>
        /// Attaches this window to the specified control. This window will move as it does.
        /// </summary>
        /// <param name="control">The control to attach to</param>
        /// <param name="boundary">Optional, this window's arrow will stay within the bounds of the control</param>
        /// <param name="padding">The offset of this window's arrow to the control</param>
        public void AttachTo(Control control, Control boundary, int padding)
        {
            if (this.Visible)
                this.Hide();

            attachedTo = control;
            attachedBoundary = boundary;
            attachedPadding = padding;
            defaultArrowAnchor = AnchorStyles.None;

            //while (control != null)
            //{
            //    control.LocationChanged += control_Changed;
            //    control.SizeChanged += control_Changed;

            //    control = control.Parent;
            //}

            //OnAttachedLocationChanged();
        }

        public void AttachTo(Rectangle r, int padding, AnchorStyles defaultArrowAnchor)
        {
            if (this.Visible)
                this.Hide();

            attachedRect = r;
            attachedTo = null;
            attachedBoundary = null;
            attachedPadding = padding;
            this.defaultArrowAnchor = defaultArrowAnchor;
            OnAttachedLocationChanged();
        }

        private async Task TransitionOpacity(int duration, double from, double to, CancellationToken token)
        {
            const int LIMIT = 20;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            this.Opacity = from;

            while (true)
            {
                double progress;

                if (duration > 0)
                {
                    var remaining = (int)(duration - sw.ElapsedMilliseconds);
                    try
                    {
                        if (remaining > LIMIT)
                            await Task.Delay(LIMIT,token);
                        else if (remaining > 0)
                            await Task.Delay(remaining,token);
                    }
                    catch (TaskCanceledException ex)
                    {
                        Util.Logging.Log(ex);
                        return;
                    }
                    progress = (double)sw.ElapsedMilliseconds / duration;
                }
                else
                {
                    progress = 1;
                }

                if (progress >= 1)
                {
                    this.Opacity = to;
                    break;
                }
                else
                {
                    this.Opacity = from + (to - from) * progress;
                }
            }
        }

        public async void Show(IWin32Window owner, string message, int delay)
        {
            if (cancelToken != null)
            {
                cancelToken.Cancel();
                cancelToken.Dispose();
                cancelToken = null;
            }

            if (delay > 0)
            {
                cancelToken = new CancellationTokenSource();
                try
                {
                    await Task.Delay(delay, cancelToken.Token);
                }
                catch (TaskCanceledException ex)
                {
                    Util.Logging.Log(ex);
                    return;
                }
            }

            if (message != null)
                SetMessage(message);

            if (!this.Visible)
            {
                this.Opacity = 0;
                this.Show(owner);

                cancelToken = new CancellationTokenSource();
                try
                {
                    await this.TransitionOpacity(100, 0, 0.98, cancelToken.Token);
                }
                catch (TaskCanceledException ex)
                {
                    Util.Logging.Log(ex);
                    return;
                }
            }
            else
            {
                this.Opacity = 0.98f;
            }
        }

        public async void HideTooltip()
        {
            if (cancelToken != null)
            {
                cancelToken.Cancel();
                cancelToken.Dispose();
            }

            cancelToken = new CancellationTokenSource();

            try
            {
                await this.TransitionOpacity(100, 0.99, 0, cancelToken.Token);
            }
            catch (TaskCanceledException ex)
            {
                Util.Logging.Log(ex);
                return;
            }

            this.Hide();
        }

        void parentForm_VisibleChanged(object sender, EventArgs e)
        {
            bool v = ((Form)sender).Visible;
            this.Visible = v;
            if (v)
                OnAttachedLocationChanged();
        }

        public void SetMessage(string message)
        {
            using (var g = this.CreateGraphics())
            {
                Screen screen;
                if (attachedTo != null)
                    screen = Screen.FromControl(attachedTo);
                else
                    screen = Screen.FromRectangle(attachedRect);
                int maxWidth = screen.WorkingArea.Width / 4;
                int maxHeight = screen.WorkingArea.Height / 2;
                if (maxWidth < 100)
                    maxWidth = 100;
                if (maxHeight < 100)
                    maxHeight = 100;

                var size = TextRenderer.MeasureText(g, message, this.Font, new Size(maxWidth, maxHeight), TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis | TextFormatFlags.EndEllipsis);

                clientAreaSize = new Size(size.Width, maxHeight > size.Height ? size.Height : maxHeight);
            }
            if (!object.ReferenceEquals(this.message, message))
            {
                this.message = message;
                OnRedrawRequired();
            }
            OnAttachedLocationChanged();
        }

        void control_Changed(object sender, EventArgs e)
        {
            if (!invoking && this.Visible)
            {
                invoking = true;

                try
                {
                    this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            invoking = false;
                            OnAttachedLocationChanged();
                        }));
                }
                catch  (Exception ex)
                {
                    Util.Logging.Log(ex);
                    invoking = false;
                }
            }
        }

        private void OnAttachedLocationChanged()
        {
            int l, r, t, b;

            try
            {
                if (this.attachedBoundary == null)
                {
                    if (attachedTo != null)
                    {
                        var control = attachedTo.Parent;

                        l = attachedTo.Location.X;
                        r = attachedTo.Location.X + attachedTo.Width;
                        t = attachedTo.Location.Y;
                        b = attachedTo.Location.Y + attachedTo.Height;

                        while (control.Parent != null)
                        {
                            var boundary = control.Bounds;

                            if (boundary.X != 0)
                            {
                                l += boundary.X;
                                r += boundary.X;
                            }
                            if (boundary.Y != 0)
                            {
                                t += boundary.Y;
                                b += boundary.Y;
                            }

                            if (l < boundary.Left)
                                l = boundary.Left;
                            else if (l > boundary.Right)
                                l = boundary.Right;
                            if (r < boundary.Left)
                                r = boundary.Left;
                            else if (r > boundary.Right)
                                r = boundary.Right;
                            if (t < boundary.Top)
                                t = boundary.Top;
                            else if (t > boundary.Bottom)
                                t = boundary.Bottom;
                            if (b < boundary.Top)
                                b = boundary.Top;
                            else if (b > boundary.Bottom)
                                b = boundary.Bottom;

                            control = control.Parent;
                        }

                        var p = attachedTo.TopLevelControl.PointToScreen(Point.Empty);

                        l += p.X;
                        r += p.X;
                        t += p.Y;
                        b += p.Y;
                    }
                    else
                    {
                        l = attachedRect.Left;
                        r = attachedRect.Right;
                        t = attachedRect.Top;
                        b = attachedRect.Bottom;
                    }
                }
                else
                {
                    var boundary = new Rectangle(this.attachedBoundary.PointToScreen(Point.Empty), this.attachedBoundary.Size);
                    var p = attachedTo.PointToScreen(Point.Empty);

                    l = p.X;
                    r = p.X + attachedTo.Width;
                    t = p.Y;
                    b = p.Y + attachedTo.Height;

                    if (l < boundary.Left)
                        l = boundary.Left;
                    else if (l > boundary.Right)
                        l = boundary.Right;
                    if (r < boundary.Left)
                        r = boundary.Left;
                    else if (r > boundary.Right)
                        r = boundary.Right;
                    if (t < boundary.Top)
                        t = boundary.Top;
                    else if (t > boundary.Bottom)
                        t = boundary.Bottom;
                    if (b < boundary.Top)
                        b = boundary.Top;
                    else if (b > boundary.Bottom)
                        b = boundary.Bottom;
                }

                var screen = Screen.FromRectangle(Rectangle.FromLTRB(l, t, r, b)).WorkingArea;
                var anchor = AnchorStyles.None;

                int INNER_PADDING = Scale(10);
                int ARROW_SIZE_BASE = Scale(20);
                int ARROW_SIZE_HEIGHT = ARROW_SIZE_BASE / 2;
                int pw = Scale(1);

                //estimated size with an empty space for both a vertical and horizontal arrow (only one will be used)
                Size size = new Size(clientAreaSize.Width + INNER_PADDING * 2 + ARROW_SIZE_HEIGHT, clientAreaSize.Height + INNER_PADDING * 2 + ARROW_SIZE_HEIGHT);

                if (defaultArrowAnchor != AnchorStyles.None)
                {
                    switch (defaultArrowAnchor)
                    {
                        case AnchorStyles.Left:
                            if (r + size.Width + attachedPadding < screen.Right)
                                anchor = AnchorStyles.Left;
                            else
                                anchor = AnchorStyles.Right;
                            break;
                        case AnchorStyles.Right:
                            if (l - size.Width + attachedPadding > screen.Left)
                                anchor = AnchorStyles.Right;
                            else
                                anchor = AnchorStyles.Left;
                            break;
                        case AnchorStyles.Top:
                            if (b + size.Height + attachedPadding < screen.Bottom)
                                anchor = AnchorStyles.Top;
                            else
                                anchor = AnchorStyles.Bottom;
                            break;
                        case AnchorStyles.Bottom:
                            if (t - size.Height + attachedPadding > screen.Top)
                                anchor = AnchorStyles.Bottom;
                            else
                                anchor = AnchorStyles.Top;
                            break;
                    }

                    //if (defaultArrowAnchor == AnchorStyles.Left && r + size.Width + attachedPadding < screen.Right)
                    //    anchor = AnchorStyles.Left;
                    //else if (defaultArrowAnchor == AnchorStyles.Right && l - size.Width + attachedPadding > screen.Left)
                    //    anchor = AnchorStyles.Right;
                    //else if (defaultArrowAnchor == AnchorStyles.Top && b + size.Height + attachedPadding < screen.Bottom)
                    //    anchor = AnchorStyles.Top;
                    //else if (defaultArrowAnchor == AnchorStyles.Bottom && t - size.Height + attachedPadding > screen.Top)
                    //    anchor = AnchorStyles.Bottom;
                }

                if (anchor == AnchorStyles.None)
                {
                    if (r + size.Width + attachedPadding < screen.Right)
                        anchor = AnchorStyles.Left;
                    else if (l - size.Width + attachedPadding > screen.Left)
                        anchor = AnchorStyles.Right;
                    else if (b + size.Height + attachedPadding < screen.Bottom)
                        anchor = AnchorStyles.Top;
                    else if (t - size.Height + attachedPadding > screen.Top)
                        anchor = AnchorStyles.Bottom;
                }

                Rectangle clientArea;
                Rectangle bounds;
                Rectangle arrowBounds;
                var space = Scale(5);

                if (anchor == AnchorStyles.Left || anchor == AnchorStyles.Right)
                {
                    size.Height -= ARROW_SIZE_HEIGHT;

                    int y = t + (b - t) / 2 - size.Height / 2;
                    if (y < screen.Top)
                        y = screen.Top;
                    else if (y + size.Height > screen.Bottom)
                        y = screen.Bottom - size.Height;

                    int aY = (t + (b - t) / 2) - y - ARROW_SIZE_BASE / 2;
                    if (aY < space)
                        aY = space;
                    else if (aY + ARROW_SIZE_BASE + space > size.Height)
                        aY = size.Height - ARROW_SIZE_BASE - space;

                    if (anchor == AnchorStyles.Left)
                    {
                        l += attachedPadding;
                        r += attachedPadding;

                        clientArea = new Rectangle(new Point(ARROW_SIZE_HEIGHT + INNER_PADDING, INNER_PADDING + 1), clientAreaSize);
                        bounds = new Rectangle(new Point(r, y), size);
                        arrowBounds = new Rectangle(0, aY, ARROW_SIZE_HEIGHT, ARROW_SIZE_BASE);
                    }
                    else
                    {
                        l -= attachedPadding;
                        r -= attachedPadding;

                        clientArea = new Rectangle(new Point(INNER_PADDING, INNER_PADDING + 1), clientAreaSize);
                        bounds = new Rectangle(new Point(l - size.Width, y), size);
                        arrowBounds = new Rectangle(size.Width - ARROW_SIZE_HEIGHT - 1, aY, ARROW_SIZE_HEIGHT, ARROW_SIZE_BASE);
                    }
                }
                else if (anchor == AnchorStyles.Top || anchor == AnchorStyles.Bottom)
                {
                    size.Width -= ARROW_SIZE_HEIGHT;

                    int x = l + (r - l) / 2 - size.Width / 2;
                    if (x < screen.Left)
                        x = screen.Left;
                    else if (x + size.Width > screen.Right)
                        x = screen.Right - size.Width;

                    int aX = (l + (r - l) / 2) - x - ARROW_SIZE_BASE / 2;
                    if (aX < space)
                        aX = space;
                    else if (aX + ARROW_SIZE_BASE + space > size.Width)
                        aX = size.Width - ARROW_SIZE_BASE - space;

                    if (anchor == AnchorStyles.Top)
                    {
                        t += attachedPadding;
                        b += attachedPadding;

                        clientArea = new Rectangle(new Point(INNER_PADDING, ARROW_SIZE_HEIGHT + INNER_PADDING + 1), clientAreaSize);
                        bounds = new Rectangle(new Point(x, b), size);
                        arrowBounds = new Rectangle(aX, 0, ARROW_SIZE_BASE, ARROW_SIZE_HEIGHT);
                    }
                    else
                    {
                        t -= attachedPadding;
                        b -= attachedPadding;

                        clientArea = new Rectangle(new Point(INNER_PADDING, INNER_PADDING + 1), clientAreaSize);
                        bounds = new Rectangle(new Point(x, t - size.Height), size);
                        arrowBounds = new Rectangle(aX, size.Height - ARROW_SIZE_HEIGHT - 1, ARROW_SIZE_BASE, ARROW_SIZE_HEIGHT);
                    }
                }
                else
                {
                    clientArea = new Rectangle(new Point(INNER_PADDING, INNER_PADDING + 1), clientAreaSize);
                    bounds = new Rectangle(new Point(l, t), size);
                    arrowBounds = Rectangle.Empty;
                }

                bool boundsChanged = !this.Bounds.Equals(bounds);
                bool clientAreaChanged = !this.clientArea.Equals(clientArea);

                if (boundsChanged)
                    this.Bounds = bounds;

                if (clientAreaChanged)
                    this.clientArea = clientArea;

                if (boundsChanged || !this.arrowBounds.Equals(arrowBounds))
                {
                    this.arrowAnchor = anchor;
                    this.arrowBounds = arrowBounds;
                    background = null;
                    OnRedrawRequired();
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private void GenerateBounds(int pw)
        {
            int pw2 = pw / 2;
            int l = pw2,
                t = pw2,
                r = this.Width - (pw + 1) / 2,
                b = this.Height - (pw + 1) / 2;

            switch (arrowAnchor)
            {
                case AnchorStyles.Left:

                    background = new Point[]
                    {
                        new Point(arrowBounds.Right + pw2, t),
                        new Point(r, t),
                        new Point(r, b),
                        new Point(arrowBounds.Right + pw2, b),
                        new Point(arrowBounds.Right + pw2, arrowBounds.Bottom),
                        new Point(arrowBounds.Left + pw2, arrowBounds.Top + arrowBounds.Height / 2),
                        new Point(arrowBounds.Right + pw2, arrowBounds.Top)
                    };

                    break;

                case AnchorStyles.Right:

                    pw2 = (pw + 1) / 2;
                    background = new Point[]
                    {
                        new Point(l, t),
                        new Point(arrowBounds.Left - pw2, t),
                        new Point(arrowBounds.Left - pw2, arrowBounds.Top),
                        new Point(arrowBounds.Right - pw2, arrowBounds.Top + arrowBounds.Height / 2),
                        new Point(arrowBounds.Left - pw2, arrowBounds.Bottom),
                        new Point(arrowBounds.Left - pw2, b),
                        new Point(l, b)
                    };

                    break;

                case AnchorStyles.Top:

                    background = new Point[]
                    {
                        new Point(l, arrowBounds.Bottom + pw2),
                        new Point(arrowBounds.Left,arrowBounds.Bottom + pw2),
                        new Point(arrowBounds.Left + arrowBounds.Width / 2, arrowBounds.Top + pw2),
                        new Point(arrowBounds.Right, arrowBounds.Bottom + pw2),
                        new Point(r, arrowBounds.Bottom + pw2),
                        new Point(r, b),
                        new Point(l,b)
                    };

                    break;

                case AnchorStyles.Bottom:

                    pw2 = (pw + 1) / 2;
                    background = new Point[]
                    {
                        new Point(l,t),
                        new Point(r,t),
                        new Point(r,arrowBounds.Top - pw2),
                        new Point(arrowBounds.Right,arrowBounds.Top - pw2),
                        new Point(arrowBounds.Left + arrowBounds.Width / 2, arrowBounds.Bottom - pw2),
                        new Point(arrowBounds.Left,arrowBounds.Top - pw2),
                        new Point(l,arrowBounds.Top - pw2)
                    };

                    break;

                default:

                    background = new Point[]
                    {
                        new Point(l,t),
                        new Point(r,t),
                        new Point(r,b),
                        new Point(l,b)
                    };

                    break;
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

                g.Clear(this.TransparencyKey);

                if (background == null)
                    GenerateBounds(pw);

                brush.Color = this.BackColor;
                g.FillPolygon(brush, background);

                pen.Color = Color.Gray;
                pen.Width = pw;
                g.DrawPolygon(pen, background);

                if (message != null)
                {
                    TextRenderer.DrawText(g, message, this.Font, clientArea, Color.Black, this.BackColor, TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis | TextFormatFlags.EndEllipsis);
                }
            }

            buffer.Render(e.Graphics);
        }

        private void formAccountTooltip_Load(object sender, EventArgs e)
        {

        }
    }
}
