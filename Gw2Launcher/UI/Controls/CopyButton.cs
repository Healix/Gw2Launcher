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
    class CopyButton : Control
    {
        protected Color colorBorder, colorHighlight;
        protected Rectangle[] rects;
        protected Pen pen;
        protected bool highlighted;
        protected ToolTip tooltip;

        public CopyButton()
        {
            tooltip = new ToolTip();
            rects = new Rectangle[2];

            colorBorder = Color.FromArgb(150, 150, 150);
            colorHighlight = Color.FromArgb(20, 20, 20);
            pen = new Pen(colorBorder);

            this.Cursor = Windows.Cursors.Hand;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                pen.Dispose();
                tooltip.Dispose();
            }
        }

        public string Tooltip
        {
            get
            {
                return tooltip.GetToolTip(this);
            }
            set
            {
                tooltip.SetToolTip(this, value);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            int w = this.Width - 1, h = this.Height - 1;

            var h1 = h * 5 / 8;
            var w1 = h1 * 4 / 5;
            var y = h / 2 - h1 * 3 / 4;
            rects[0] = new Rectangle(1, y + h1 / 2, w1, h1);
            rects[1] = new Rectangle(1 + w1 - w1 / 2, y, w1, h1);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!highlighted)
            {
                //if (!string.IsNullOrEmpty(this.Tooltip))
                //    tooltip.Show(this.Tooltip, this, this.Width, 0);
                highlighted = true;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (highlighted)
            {
                //tooltip.Hide(this);
                highlighted = false;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);

            var g = pevent.Graphics;

            var bg = rects[1];
            var fg = rects[0];
            Brush brushBg;

            if (highlighted)
            {
                pen.Color = colorHighlight;
                brushBg = Brushes.White;
            }
            else
            {
                pen.Color = colorBorder;
                brushBg = Brushes.WhiteSmoke;
            }

            g.FillRectangle(brushBg, bg);
            g.DrawRectangle(pen, bg);

            g.FillRectangle(brushBg, fg);
            g.DrawRectangle(pen, fg);

            pen.Color = this.BackColor;
            g.DrawLine(pen, fg.X + fg.Width / 2, fg.Y - 1, fg.Right + 1, fg.Y - 1);
            g.DrawLine(pen, fg.Right + 1, fg.Y - 1, fg.Right + 1, fg.Y + fg.Height / 2);
        }
    }
}
