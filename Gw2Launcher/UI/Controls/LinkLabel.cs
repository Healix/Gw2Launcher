using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Design;

namespace Gw2Launcher.UI.Controls
{
    class LinkLabel : Base.BaseLabel
    {
        public const int DEFAULT_FORECOLOR = -13534734;

        protected Color foreColor;
        protected Color foreColorHighlight;
        protected Icon icon;

        public LinkLabel()
        {
            Cursor = Windows.Cursors.Hand;
            base.ForeColor = foreColor = Color.FromArgb(DEFAULT_FORECOLOR);
            foreColorHighlight = Color.FromArgb(34, 85, 169);
        }

        [DefaultValue(typeof(Color), "49, 121, 242")]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = foreColor = value;
            }
        }

        [DefaultValue(typeof(Color), "34, 85, 169")]
        public Color ForeColorHovered
        {
            get
            {
                return foreColorHighlight;
            }
            set
            {
                foreColorHighlight = value;
            }
        }

        protected UiColors.Colors _ForeColorHoveredName = UiColors.Colors.Custom;
        [UiPropertyColor()]
        [DefaultValue(UiColors.Colors.Custom)]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors ForeColorHoveredName
        {
            get
            {
                return _ForeColorHoveredName;
            }
            set
            {
                if (_ForeColorHoveredName != value)
                {
                    _ForeColorHoveredName = value;
                    RefreshColors();
                }
            }
        }

        public void ResetForeColor()
        {
            ForeColor = Color.FromArgb(DEFAULT_FORECOLOR);
        }

        [DefaultValue(typeof(Cursor), "Hand")]
        public override Cursor Cursor
        {
            get
            {
                return base.Cursor;
            }
            set
            {
                base.Cursor = value;
            }
        }

        public Icon Icon
        {
            get
            {
                return icon;
            }
            set
            {
                icon = value;
                this.Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();

            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            base.ForeColor = foreColorHighlight;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            base.ForeColor = foreColor;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (icon != null)
            {
                e.Graphics.DrawIcon(icon, 0, (this.Height - icon.Height) / 2);
            }
        }

        public override void RefreshColors()
        {
            if (_ForeColorHoveredName != UiColors.Colors.Custom)
                ForeColorHovered = UiColors.GetColor(_ForeColorHoveredName);

            base.RefreshColors();
        }
    }
}
