using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    public class FlatSlider : Control
    {
        public event EventHandler ValueChanged;

        private enum MouseWheelState : byte
        {
            Disabled,
            Entered,
            Enabled
        }

        private int sliderX, sliderY, sliderW, sliderH;
        private int barX, barY, barW, barH;
        private int originX;
        private Color colorBar, colorBarHighlight, colorSlider, colorSliderHighlight, colorDisabled;
        private SolidBrush brush;
        private Point lastMove;
        private bool highlighted, sliding;
        private MouseWheelState stateWheel; //prevent the wheel from being used until the mouse has moved within the control to prevent catching it while scrolling

        public FlatSlider()
        {
            brush = new SolidBrush(colorBar);

            barH = 4;
            sliderW = 10;
            sliderH = 20;
            barW = 100;

            colorBar = Color.FromArgb(214, 214, 214);
            colorBarHighlight = Color.FromArgb(150, 150, 150);
            colorSlider = Color.FromArgb(150, 150, 150);
            colorSliderHighlight = Color.FromArgb(75, 75, 75);
            colorDisabled = Color.FromArgb(204, 204, 204);

            this.Size = new Size(barW, sliderH);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, true);

            this.TabStop = false;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            var scale = factor.Width;

            if (scale != 1)
            {
                var v = Value;

                barH = (int)(4 * scale + 0.5f);
                sliderW = (int)(10 * scale + 0.5f);
                sliderH = (int)(20 * scale + 0.5f);
                barW = (int)(100 * scale + 0.5f);

                base.ScaleControl(factor, specified);

                Value = v;
            }
            else
                base.ScaleControl(factor, specified);
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

        public float Value
        {
            get
            {
                return (float)(sliderX - barX) / (barW - sliderW);
            }
            set
            {
                MoveSlider(barX + (int)(value * (barW - sliderW) + 0.5f));
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                brush.Dispose();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            barW = this.Width;
            barY = sliderY + sliderH / 2 - barH / 2;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SetClip(e.ClipRectangle);

            g.Clear(this.BackColor);

            var enabled = this.Enabled;

            if (!enabled)
                brush.Color = colorDisabled;

            if (sliderX > 0)
            {
                if (enabled)
                    brush.Color = colorBarHighlight;
                g.FillRectangle(brush, barX, barY, sliderX - barX, barH);
            }

            if (barX + sliderW < barW)
            {
                if (enabled)
                    brush.Color = colorBar;
                g.FillRectangle(brush, sliderX + sliderW, barY, barW - sliderX - sliderW, barH);
            }

            if (enabled)
            {
                if (highlighted)
                    brush.Color = colorSliderHighlight;
                else
                    brush.Color = colorSlider;
            }

            g.FillRectangle(brush, sliderX, sliderY, sliderW, sliderH);
        }

        private void MoveSlider(int x)
        {
            if (x < barX)
                x = barX;
            var max = barX + barW - sliderW;
            if (x > max)
                x = max;

            if (x != sliderX)
            {
                sliderX = x;
                this.Invalidate();

                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!this.Focused)
                this.Focus();

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                highlighted = true;
                sliding = true;

                if (e.X >= sliderX && e.X < sliderX + sliderW)
                    originX = e.X - sliderX;
                else
                    originX = sliderW / 2;

                MoveSlider(e.X - originX);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (sliding && e.Button == System.Windows.Forms.MouseButtons.Left)
                sliding = false;

            if (highlighted)
            {
                bool contains = (e.X >= sliderX && e.X < sliderX + sliderW && e.Y >= sliderY && e.Y < sliderY + sliderH);
                if (!contains)
                {
                    highlighted = false;
                    this.Invalidate();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (lastMove == e.Location)
                return;
            lastMove = e.Location;

            if (sliding)
                MoveSlider(e.X - originX);
            else
            {
                bool contains = (e.X >= sliderX && e.X < sliderX + sliderW && e.Y >= sliderY && e.Y < sliderY + sliderH);
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

            switch (stateWheel)
            {
                case MouseWheelState.Disabled:
                    stateWheel = MouseWheelState.Entered;
                    break;
                case MouseWheelState.Entered:
                    stateWheel = MouseWheelState.Enabled;
                    break;
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
            stateWheel = MouseWheelState.Disabled;
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
                        MoveSlider(sliderX - 1);
                        break;
                    case Keys.Down:
                    case Keys.Right:
                        MoveSlider(sliderX + 1);
                        break;
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (stateWheel == MouseWheelState.Enabled)
            {
                if (e.Delta > 0)
                {
                    this.Value += 0.03f;
                }
                else if (e.Delta < 0)
                {
                    this.Value -= 0.03f;
                }

                if (e is HandledMouseEventArgs)
                    ((HandledMouseEventArgs)e).Handled = true;
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
    }
}
