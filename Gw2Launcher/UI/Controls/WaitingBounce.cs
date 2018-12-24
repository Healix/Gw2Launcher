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
        private long startTicks;
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
                Interval = 50,
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
                startTicks = DateTime.UtcNow.Ticks;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                brushF.Dispose();
                brushB.Dispose();
                timer.Dispose();
                if (buffer != null)
                    buffer.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (enabled)
            {
                if (buffer == null)
                {
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);
                    buffer.Graphics.Clear(this.BackColor);
                }

                var g = buffer.Graphics;

                g.FillRectangle(brushB, ball);

                int h = this.Height - 3;
                float ms = (DateTime.UtcNow.Ticks - startTicks) / 10000f;
                int x = (int)((this.Width - h) * (Math.Sin((ms / 500) % 360) + 1) / 2);
                ball = new Rectangle(x - 2, 0, h + 4, h + 3);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(brushF, x, 1, h, h);

                buffer.Render(e.Graphics);
            }

            base.OnPaint(e);
        }
    }
}
