using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class DailyCategoryBar : Control
    {
        public event EventHandler Collapsed;
        public event EventHandler Expanded;

        private BufferedGraphics buffer;
        private bool 
            redraw,
            collapsed,
            hovered;
        private Rectangle rectCollapse;

        public DailyCategoryBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        public bool IsCollapsed
        {
            get
            {
                return collapsed;
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

            rectCollapse = new Rectangle(this.Width - 50, 0, 50, this.Height);

            redraw = true;
            this.Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            redraw = true;
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hovered)
            {
                hovered = false;
                redraw = true;
                this.Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (rectCollapse.Contains(e.Location))
                {
                    if (collapsed)
                    {
                        collapsed = false;
                        if (Expanded != null)
                            Expanded(this, EventArgs.Empty);
                    }
                    else
                    {
                        collapsed = true;
                        if (Collapsed != null)
                            Collapsed(this, EventArgs.Empty);
                    }

                    redraw = true;
                    this.Invalidate(rectCollapse);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (rectCollapse.Contains(e.Location))
            {
                if (!hovered)
                {
                    hovered = true;
                    redraw = true;
                    this.Invalidate(rectCollapse);
                }
            }
            else if (hovered)
            {
                hovered = false;
                redraw = true;
                this.Invalidate(rectCollapse);
            }
        }

        public void SetState(bool collapsed)
        {
            if (collapsed != this.collapsed)
            {
                this.collapsed = collapsed;
                redraw = true;
                this.Invalidate(rectCollapse);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                var g = buffer.Graphics;

                TextRenderer.DrawText(g, this.Text, this.Font, new Rectangle(10, 0, rectCollapse.Left - 10, this.Height), this.ForeColor, TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis | TextFormatFlags.VerticalCenter);

                var c = Util.Color.Darken(this.BackColor, 0.5f);
                using (var brush = new SolidBrush(c))
                {
                    using (var pen = new Pen(brush))
                    {
                        if (hovered)
                        {
                            brush.Color = Util.Color.Darken(this.BackColor, 0.1f);
                            g.FillRectangle(brush, rectCollapse);

                            brush.Color = c;
                        }

                        PointF[] points;

                        float
                            w = 9,
                            h = 5,
                            x = rectCollapse.X + rectCollapse.Width / 2f - w / 2f,
                            y = rectCollapse.Y + rectCollapse.Height / 2f - h / 2f;

                        if (collapsed)
                        {
                            points = new PointF[]
                            {
                                new PointF(x, y),
                                new PointF(x + w, y),
                                new PointF(x + w / 2f, y + h),
                            };
                        }
                        else
                        {
                            points = new PointF[]
                            {
                                new PointF(x + w / 2f, y),
                                new PointF(x + w, y + h),
                                new PointF(x, y + h),
                            };
                        }

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        g.FillPolygon(brush, points);
                        g.DrawPolygon(pen, points);
                    }
                }
            }

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);

                buffer.Graphics.Clear(this.BackColor);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (buffer != null)
                {
                    buffer.Dispose();
                    buffer = null;
                }
            }
        }
    }
}
