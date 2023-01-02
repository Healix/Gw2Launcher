using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Drawing.Design;

namespace Gw2Launcher.UI.Controls
{
    class ArrowButton : Base.BaseControl
    {
        public enum ArrowDirection
        {
            Right,
            Left
        }

        protected Color colorHighlight;
        protected PointF[] points;
        protected SolidBrush brush;
        protected Pen pen;
        protected bool highlighted;
        protected ArrowDirection direction;
        protected bool redraw;
        
        public ArrowButton()
        {
            points = new PointF[6];

            brush = new SolidBrush(this.ForeColor);
            pen = new Pen(brush);

            this.ForeColor = Color.FromArgb(150, 150, 150);
            colorHighlight = Color.FromArgb(20, 20, 20);

            this.Cursor = Cursors.Hand;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            OnColorChanged();
        }

        [DefaultValue(typeof(Color),"20,20,20")]
        public Color ForeColorHighlight
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

        protected UiColors.Colors _ForeColorHighlightName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors ForeColorHighlightName
        {
            get
            {
                return _ForeColorHighlightName;
            }
            set
            {
                if (_ForeColorHighlightName != value)
                {
                    _ForeColorHighlightName = value;
                    RefreshColors();
                }
            }
        }

        protected void OnColorChanged()
        {
            if (highlighted)
                pen.Color = brush.Color = colorHighlight;
            else
                pen.Color = brush.Color = this.ForeColor;
            this.Invalidate();
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
                pen.Color = brush.Color = this.ForeColor;
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
                using (var brush = new SolidBrush(Util.Color.Lighten(this.ForeColor, 0.5f)))
                {
                    g.FillClosedCurve(brush, points);
                    using (var pen = new Pen(brush))
                    {
                        g.DrawLines(pen, points);
                    }
                }
            }
        }

        public override void RefreshColors()
        {
            base.RefreshColors();

            if (_ForeColorHighlightName != UiColors.Colors.Custom)
                this.ForeColorHighlight = UiColors.GetColor(_ForeColorHighlightName);
        }
    }
}
