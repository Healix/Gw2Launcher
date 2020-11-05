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
    class ArrowButton : Control
    {
        public enum ArrowDirection
        {
            Right,
            Left
        }

        protected Color colorArrow, colorHighlight;
        protected PointF[] points;
        protected SolidBrush brush;
        protected Pen pen;
        protected bool highlighted;
        protected ArrowDirection direction;
        protected bool redraw;
        
        public ArrowButton()
        {
            points = new PointF[6];

            colorArrow = Color.FromArgb(150, 150, 150);
            colorHighlight = Color.FromArgb(20, 20, 20);
            brush = new SolidBrush(colorArrow);
            pen = new Pen(brush);

            this.Cursor = Cursors.Hand;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        public Color ColorArrow
        {
            get
            {
                return colorArrow;
            }
            set
            {
                colorArrow = value;
                OnColorChanged();
            }
        }

        public Color ColorHighlight
        {
            get
            {
                return colorHighlight;
            }
            set
            {
                colorHighlight = value;
                OnColorChanged();
            }
        }

        protected void OnColorChanged()
        {
            if (highlighted)
                pen.Color = brush.Color = colorHighlight;
            else
                pen.Color = brush.Color = colorArrow;
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

        public ArrowDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                redraw = true;
                this.Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                pen.Dispose();
                brush.Dispose();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            redraw = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!highlighted)
            {
                highlighted = true;
                pen.Color = brush.Color = colorHighlight;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (highlighted)
            {
                highlighted = false;
                pen.Color = brush.Color = colorArrow;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);

            var g = pevent.Graphics;

            if (redraw)
            {
                redraw = false;
                int w = this.Width - 1, h = this.Height - 1;

                if (direction == ArrowDirection.Right)
                {
                    points[0] = points[5] = new PointF(0, 0);
                    points[1] = points[2] = new PointF(0, h);
                    points[3] = points[4] = new PointF(w, h / 2);
                }
                else
                {
                    points[0] = points[5] = new PointF(w, 0);
                    points[1] = points[2] = new PointF(w, h);
                    points[3] = points[4] = new PointF(0, h / 2);
                }
            }

            if (this.Enabled)
            {
                g.FillClosedCurve(brush, points);
                g.DrawLines(pen, points);
            }
            else
            {
                using (var brush = new SolidBrush(Util.Color.Lighten(colorArrow, 0.5f)))
                {
                    g.FillClosedCurve(brush, points);
                    using (var pen = new Pen(brush))
                    {
                        g.DrawLines(pen, points);
                    }
                }
            }
        }
    }
}
