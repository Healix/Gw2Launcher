using System;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class WaitingBounce : Control
    {
        private bool enabled;
        private int startTime;
        private SolidBrush brushF, brushB;
        private Rectangle ball;
        private Timer timer;
        private BufferedGraphics buffer;

        public WaitingBounce()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            brushF = new SolidBrush(this.ForeColor);
            brushB = new SolidBrush(this.BackColor);
            timer = new Timer()
            {
                Interval = 25,
                Enabled = false,
            };
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        [System.ComponentModel.Browsable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            OnStateChanged();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            OnStateChanged();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            brushF.Color = this.ForeColor;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            brushB.Color = this.BackColor;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
        }

        protected void OnStateChanged()
        {
            timer.Enabled = enabled = this.Enabled && this.Visible;
            if (enabled)
                startTime = Environment.TickCount;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
            }

            base.Dispose(disposing);

            if (disposing)
            {
                brushF.Dispose();
                brushB.Dispose();
                if (buffer != null)
                    buffer.Dispose();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (enabled)
            {
                if (buffer == null)
                {
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);
                    buffer.Graphics.Clear(this.BackColor);
                }

                var g = buffer.Graphics;

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.FillRectangle(brushB, ball);

                var px = (int)(g.DpiX / 96f + 0.5f);
                var h = this.Height - px * 3;
                var ms = Environment.TickCount - startTime;
                var x = (int)((this.Width - h) * (Math.Sin((ms / 500f) % 360) + 1) / 2);

                ball = new Rectangle(x - px * 2, 0, h + px * 4, this.Height);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(brushF, x, px, h, h);

                buffer.Render(e.Graphics);
            }
        }
    }
}
