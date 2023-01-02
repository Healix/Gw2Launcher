using System;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class WaitingBounce : Base.BaseControl
    {
        private bool enabled;
        private int startTime;
        private SolidBrush brushF, brushB;
        private Rectangle ball;
        private Timer timer;
        private BufferedGraphics buffer;
        private bool clear;

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
            var h = this.Height - 3;
            var ms = Environment.TickCount - startTime;
            var x = (int)((this.Width - h) * (Math.Sin((ms / 500f) % 360) + 1) / 2);

            if (this.ball.X + 2 != x)
            {
                this.Invalidate();
            }
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
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
            clear = true;
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
            var e = this.Enabled && this.Visible;
            if (e == enabled)
                return;

            timer.Enabled = enabled = e;
            if (enabled)
                startTime = Environment.TickCount;

            this.Invalidate();
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

            Graphics g;

            if (buffer == null)
            {
                buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);
                g = buffer.Graphics;
                g.Clear(this.BackColor);
                clear = false;
            }
            else if (clear)
            {
                g = buffer.Graphics;
                g.Clear(this.BackColor);
                clear = false;
            }
            else
            {
                g = buffer.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.FillRectangle(brushB, ball);
            }

            if (enabled)
            {
                //var px = (int)(g.DpiX / 96f + 0.5f);
                var h = this.Height - 3;
                var ms = Environment.TickCount - startTime;
                var x = (int)((this.Width - h) * (Math.Sin((ms / 500f) % 360) + 1) / 2);

                ball = new Rectangle(x - 2, 0, h + 4, this.Height);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(brushF, x, 1, h, h);
            }

            buffer.Render(e.Graphics);
        }
    }
}
