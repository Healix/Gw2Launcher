using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class FlatProgressBar : Control
    {
        private long value, maximum;
        private int barX, _barX;
        private bool animated, animating, changed;
        private SolidBrush brush;

        public FlatProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            brush = new SolidBrush(this.ForeColor);
            maximum = 100;
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            brush.Color = this.ForeColor;
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

        public long Value
        {
            get
            {
                return value;
            }
            set
            {
                if (this.value != value)
                {
                    changed = true;
                    this.value = value;
                    OnValueChanged();
                }
            }
        }

        public long Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                if (maximum != value)
                {
                    changed = true;
                    maximum = value;
                    if (this.value > maximum)
                    {
                        this.value = maximum;
                        OnValueChanged();
                    }
                }
            }
        }

        public bool Animated
        {
            get
            {
                return animated;
            }
            set
            {
                animated = value;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            changed = true;
        }

        protected void SetX(int x)
        {
            var barX = this.barX;
            var d = x - barX;

            this.barX = x;
            x = barX;

            if (d == 0)
            {
                return;
            }
            else if (d < 0)
            {
                x += d;
                d = -d;
            }

            this.Invalidate(new Rectangle(x, 0, d, this.Height));
        }

        protected async void AnimateChange()
        {
            do
            {
                changed = false;

                var startX = barX;
                var endX = _barX;
                if (startX == endX)
                    break;

                var start = DateTime.UtcNow.Ticks;
                var t = 1000 / (endX - startX);
                if (t < 0)
                    t = -t;
                if (t > 100)
                    t = 100;
                else
                    ++t;

                do
                {
                    await Task.Delay(t);

                    var progress = (DateTime.UtcNow.Ticks - start) / 10000f / 1000f;

                    if (progress >= 1)
                    {
                        SetX(endX);
                        break;
                    }
                    else
                    {
                        SetX(startX + (int)((endX - startX) * progress));
                    }
                }
                while (!changed);
            }
            while (barX != _barX);

            animating = false;
        }

        protected void OnValueChanged()
        {
            _barX = (int)((double)this.value / this.maximum * this.Width);
            if (animating)
                return;
            if (animated)
            {
                animating = true;
                AnimateChange();
            }
            else
            {
                SetX(_barX);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(brush, 0, 0, barX, this.Height);

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (brush != null)
                    brush.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
