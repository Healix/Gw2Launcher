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

        const byte DEFAULT_SLIDER_H = 80;

        private int value, maximum;

        private int sliderX, sliderY, sliderW, sliderH;
        private int barX, barY, barW, barH;
        private int originY;
        private Color colorBar, colorSlider, colorSliderHighlight;
        private SolidBrush brush;
        private bool highlighted, sliding, hovered;

        public FlatVScrollBar()
            : base()
        {
            brush = new SolidBrush(colorBar);

            value = 0;
            maximum = 100;

            barH = 100;
            sliderW = 6;
            sliderH = DEFAULT_SLIDER_H;
            barW = sliderW;

            colorBar = Color.FromArgb(214, 214, 214);
            colorSlider = Color.FromArgb(150, 150, 150);
            colorSliderHighlight = Color.FromArgb(90, 90, 90);

            this.Size = new Size(barW + 2, barH);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, true);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            var scale = factor.Width;

            if (scale != 1)
            {
                var v = Value;

                barH = (int)(100 * scale + 0.5f);
                sliderW = (int)(6 * scale + 0.5f);
                sliderH = (int)(DEFAULT_SLIDER_H * scale + 0.5f);
                barW = sliderW;

                base.ScaleControl(factor, specified);

                Value = v;
            }
            else
                base.ScaleControl(factor, specified);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                brush.Dispose();
            }
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

                    if (ValueChanged != null)
                        ValueChanged(this, value);
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
                    if (this.value > maximum)
                        this.Value = maximum;
                    else
                        OnValueChanged();
                }
            }
        }

        protected void OnValueChanged()
        {
            if (maximum > 0)
                sliderY = (barH - sliderH) * value / maximum;
            else
                sliderY = 0;

            this.Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            var w = this.Width;

            barX = w / 2 - barW / 2;
            barH = this.Height;

            if (barH < DEFAULT_SLIDER_H / 0.75f)
                sliderH = (int)(barH * 0.75f);
            else
                sliderH = DEFAULT_SLIDER_H;

            if (maximum > 0)
                sliderY = (barH - sliderH) * value / maximum;
            else
                sliderY = 0;
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

            hovered = false;
            if (!sliding && highlighted)
            {
                highlighted = false;
                this.Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            hovered = true;
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
                        this.Value -= barH / 3;
                        break;
                    case Keys.Down:
                    case Keys.Right:
                        this.Value += barH / 3;
                        break;
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (hovered)
            {
                if (e is HandledMouseEventArgs)
                    ((HandledMouseEventArgs)e).Handled = true;

                DoMouseWheel(e);
            }
        }

        public void DoMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                this.Value -= barH / 3;
            }
            else if (e.Delta < 0)
            {
                this.Value += barH / 3;
            }
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

            if (maximum > 0)
            {
                if (highlighted)
                    brush.Color = colorSliderHighlight;
                else
                    brush.Color = colorSlider;

                g.FillRectangle(brush, sliderX, sliderY, sliderW, sliderH);
            }
        }
    }
}
