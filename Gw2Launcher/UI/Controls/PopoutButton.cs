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
    class PopoutButton : Control
    {
        protected Color colorArrow, colorHighlight, colorBorder;
        protected PointF[] points, border;
        protected SolidBrush brush;
        protected Pen pen;
        protected bool highlighted;
        protected bool redraw;

        public PopoutButton()
        {
            points = new PointF[3];
            border = new PointF[5];

            colorArrow = Color.FromArgb(128, 128, 128);
            colorHighlight = Color.FromArgb(20, 20, 20);
            colorBorder = Color.FromArgb(90, 90, 90);

            brush = new SolidBrush(colorArrow);
            pen = new Pen(colorBorder, 1);

            this.Cursor = Cursors.Hand;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
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
                brush.Color = colorHighlight;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (highlighted)
            {
                highlighted = false;
                brush.Color = colorArrow;
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

                int w = this.Width, h = this.Height;
                int aw = (int)(w * 0.5f + 0.5f);

                points[0] = new PointF(w - aw, 0);
                points[1] = new PointF(w, 0);
                points[2] = new PointF(w, aw);

                int pad = 2;
                int gap = 3;

                border[0] = new PointF(w - (aw - pad) - gap - 1, pad);
                border[1] = new PointF(pad, pad);
                border[2] = new PointF(pad, h - pad - 1);
                border[3] = new PointF(w - pad - 1, h - pad - 1);
                border[4] = new PointF(w - pad - 1, aw - pad + gap);
            }

            g.FillPolygon(brush, points);
            g.DrawLines(pen, border);
        }
    }
}
