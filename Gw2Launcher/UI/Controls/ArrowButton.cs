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
        protected bool highlighted;
        protected ArrowDirection direction;
        protected bool redraw;

        public ArrowButton()
        {
            points = new PointF[3];

            brush = new SolidBrush(this.ForeColor);

            this.ForeColor = Color.FromArgb(150, 150, 150);
            colorHighlight = Color.FromArgb(20, 20, 20);

            this.Cursor = Cursors.Hand;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            OnColorChanged();
        }

        [DefaultValue(typeof(Color), "20,20,20")]
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
            if (this.Enabled)
            {
                if (highlighted)
                    brush.Color = colorHighlight;
                else
                    brush.Color = this.ForeColor;
            }
            else
            {
                brush.Color = Util.Color.Lighten(this.ForeColor, 0.5f);
            }
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
                brush.Dispose();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            redraw = true;
            var w = this.Height / 2 + 1;
            if (this.Width != w)
                this.Width = w;
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
                if (this.Enabled)
                {
                    brush.Color = this.ForeColor;
                    this.Invalidate();
                }
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            OnColorChanged();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);

            var g = pevent.Graphics;

            if (redraw)
            {
                redraw = false;
                var h = this.Height;

                if (direction == ArrowDirection.Right)
                {
                    var w = h / 2;

                    points[0] = new PointF(0, 0);
                    points[1] = new PointF(w, h / 2f);
                    points[2] = new PointF(0, h);
                }
                else
                {
                    var w = (int)(h / 2f + 0.5f);

                    points[0] = new PointF(w, 0);
                    points[1] = new PointF(0, h / 2f);
                    points[2] = new PointF(w, h);
                }
            }

            g.FillPolygon(brush, points);
        }

        public override void RefreshColors()
        {
            base.RefreshColors();

            if (_ForeColorHighlightName != UiColors.Colors.Custom)
                this.ForeColorHighlight = UiColors.GetColor(_ForeColorHighlightName);
        }
    }
}
