using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Gw2Launcher.UI.Controls
{
    class FlatVScrollBar : Control
    {
        public event EventHandler<int> ValueChanged;

        private int value, maximum;

        private int sliderX, sliderY, sliderW, sliderH;
        private int barX, barY, barW, barH;
        private int originY;
        private Color colorBar, colorSlider, colorSliderHighlight;
        private SolidBrush brush;
        private bool highlighted, sliding;

        public FlatVScrollBar()
            : base()
        {
            brush = new SolidBrush(colorBar);

            value = 0;
            maximum = 100;

            sliderY = 0;
            barX = 0;
            barY = 0;
            barH = 100;
            sliderW = 6;
            sliderH = 80;
            barW = 6;

            colorBar = Color.FromArgb(214, 214, 214);
            colorSlider = Color.FromArgb(150, 150, 150);
            colorSliderHighlight = Color.FromArgb(90, 90, 90);

            this.Size = new Size(barW + 2, barH);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                brush.Dispose();
            }
            base.Dispose(disposing);
        }

        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value > maximum)
                    value = maximum;
                else if (value < 0)
                    value = 0;

                if (this.value != value)
                {
                    this.value = value;
                    OnValueChanged();
                }
            }
        }

        public int Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                if (value < 0)
                    value = 0;

                if (maximum != value)
                {
                    maximum = value;
                    if (value > maximum)
                        value = maximum;
                    OnValueChanged();
                }
            }
        }

        protected void OnValueChanged()
        {
            sliderY = (barH - sliderH) * value / maximum;

            this.Invalidate();

            if (ValueChanged != null)
                ValueChanged(this, value);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            var w = this.Width - 1;

            barX = w / 2 - barW / 2;
            barH = this.Height - 1;
            sliderY = (barH - sliderH) * value / maximum;
            sliderX = barX;

            this.Invalidate();
        }

        private void MoveSlider(int y)
        {
            if (y < barY)
                y = barY;
            var max = barY + barH - sliderH;
            if (y > max)
                y = max;

            if (y != sliderY)
            {
                sliderY = y;

                this.Invalidate();

                var value = (int)((float)(sliderY - barY) / (barH - sliderH) * maximum + 0.5f);
                if (this.value != value)
                {
                    this.value = value;
                    if (ValueChanged != null)
                        ValueChanged(this, this.Value);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                sliding = true;

                if (e.Y >= sliderY && e.Y < sliderY + sliderH)
                    originY = e.Y - sliderY;
                else
                    originY = sliderH / 2;

                MoveSlider(e.Y - originY);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (sliding)
            {
                MoveSlider(e.Y - originY);
            }
            else
            {
                bool contains = (e.Y >= sliderY && e.Y < sliderY + sliderH); //e.X >= sliderX && e.X < sliderX + sliderW && 
                if (highlighted)
                {
                    if (!contains)
                    {
                        highlighted = false;
                        this.Invalidate();
                    }
                }
                else if (contains)
                {
                    highlighted = true;
                    this.Invalidate();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (sliding && e.Button == System.Windows.Forms.MouseButtons.Left)
                sliding = false;

            if (highlighted)
            {
                bool contains = (e.Y >= sliderY && e.Y < sliderY + sliderH); //e.X >= sliderX && e.X < sliderX + sliderW && 
                if (!contains)
                {
                    highlighted = false;
                    this.Invalidate();
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!sliding && highlighted)
            {
                highlighted = false;
                this.Invalidate();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!sliding)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                    case Keys.Left:
                        this.Value -= barH / 10;
                        break;
                    case Keys.Down:
                    case Keys.Right:
                        this.Value += barH / 10;
                        break;
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta > 0)
            {
                this.Value -= barH / 10;
            }
            else if (e.Delta < 0)
            {
                this.Value += barH / 10;
            }

            ((HandledMouseEventArgs)e).Handled = true;
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    e.IsInputKey = true;
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SetClip(e.ClipRectangle);

            g.Clear(this.BackColor);

            brush.Color = colorBar;

            g.FillRectangle(brush, barX, barY, barW, barH);

            if (highlighted)
                brush.Color = colorSliderHighlight;
            else
                brush.Color = colorSlider;

            g.FillRectangle(brush, sliderX, sliderY, sliderW, sliderH);
        }
    }
}
