using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class RoundedLabelButton : Label
    {
        protected bool isHovered;
        protected Region regionBackground, regionBorder;

        public RoundedLabelButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, true);

            base.Padding = new Padding(10, 2, 10, 2);
            this.Cursor = Windows.Cursors.Hand;

            base.BackColor = backColor = SystemColors.ControlLight;
            base.ForeColor = foreColor = SystemColors.ControlText;

            backColorHovered = Util.Color.Darken(backColor, 0.05f);

            borderColor = backColorHovered;
            borderColorHovered = Util.Color.Darken(borderColor, 0.05f);

            roundedCorners = new Point(2, 2);
        }

        protected byte borderSize;
        public byte BorderSize
        {
            get
            {
                return borderSize;
            }
            set
            {
                if (borderSize != value)
                {
                    borderSize = value;
                    OnRedrawRequired();
                    this.Invalidate();
                }
            }
        }

        protected Point roundedCorners;
        [DefaultValue(typeof(Point), "2, 2")]
        public Point RoundedCorners
        {
            get
            {
                return roundedCorners;
            }
            set
            {
                if (roundedCorners != value)
                {
                    roundedCorners = value;
                    OnRedrawRequired();
                    this.Invalidate();
                }
            }
        }

        protected Color borderColor;
        [DefaultValue(typeof(Color), "216, 216, 216")]
        public Color BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    if (!isHovered)
                        this.Invalidate();
                }
            }
        }

        protected Color borderColorHovered;
        [DefaultValue(typeof(Color), "205, 205, 205")]
        public Color BorderColorHovered
        {
            get
            {
                if (borderColorHovered.IsEmpty)
                    return borderColor;
                return borderColorHovered;
            }
            set
            {
                if (borderColorHovered != value)
                {
                    borderColorHovered = value;
                    if (isHovered)
                        this.Invalidate();
                }
            }
        }

        protected Color backColorHovered;
        [DefaultValue(typeof(Color), "216, 216, 216")]
        public Color BackColorHovered
        {
            get
            {
                if (backColorHovered.IsEmpty)
                    return backColor;
                return backColorHovered;
            }
            set
            {
                if (backColorHovered != value)
                {
                    backColorHovered = value;
                    if (isHovered)
                        this.Invalidate();
                }
            }
        }

        protected Color foreColorHovered;
        [DefaultValue(typeof(Color), "ControlText")]
        public Color ForeColorHovered
        {
            get
            {
                if (foreColorHovered.IsEmpty)
                    return foreColor;
                return foreColorHovered;
            }
            set
            {
                if (foreColorHovered != value)
                {
                    foreColorHovered = value;
                    if (isHovered)
                        this.Invalidate();
                }
            }
        }

        public Color BorderColorCurrent
        {
            get
            {
                if (isHovered)
                    return BorderColorHovered;
                else
                    return borderColor;
            }
        }

        public Color BackColorCurrent
        {
            get
            {
                if (isHovered)
                    return BackColorHovered;
                else
                    return backColor;
            }
        }

        public Color ForeColorCurrent
        {
            get
            {
                if (isHovered)
                    return ForeColorHovered;
                else
                    return foreColor;
            }
        }

        private Color backColor;
        [DefaultValue(typeof(Color), "ControlLight")]
        public override Color BackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                if (backColor != value)
                {
                    base.BackColor = backColor = value;
                    if (!isHovered)
                        this.Invalidate();
                }
            }
        }

        private Color foreColor;
        [DefaultValue(typeof(Color), "ControlText")]
        public override Color ForeColor
        {
            get
            {
                return foreColor;
            }
            set
            {
                if (foreColor != value)
                {
                    base.ForeColor = foreColor = value;
                    if (!isHovered)
                        this.Invalidate();
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Focus();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            isHovered = true;
            base.BackColor = BackColorCurrent;
            base.ForeColor = ForeColorCurrent;
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            isHovered = false;
            base.BackColor = BackColorCurrent;
            base.ForeColor = ForeColorCurrent;
            this.Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            OnRedrawRequired();
        }

        protected void OnRedrawRequired()
        {
            if (regionBackground != null)
            {
                regionBackground.Dispose();
                regionBackground = null;
            }
            if (regionBorder != null)
            {
                regionBorder.Dispose();
                regionBorder = null;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);

            var g = e.Graphics;

            if (roundedCorners.IsEmpty)
            {
                g.Clear(this.BackColorCurrent);

                if (borderSize > 0 && this.BorderColorCurrent.A > 0)
                {
                    using (var pen = new Pen(this.BorderColorCurrent, borderSize))
                    {
                        g.DrawRectangle(pen, borderSize / 2, borderSize / 2, this.Width - borderSize, this.Height - borderSize);
                    }
                }
            }
            else
            {
                if (this.Parent != null)
                {
                    g.Clear(this.Parent.BackColor);
                }
                else
                {
                    g.Clear(this.BackColorCurrent);
                }

                using (var brush = new SolidBrush(this.BorderColorCurrent))
                {
                    var scale = g.DpiX / 96f;
                    var rx = (int)(roundedCorners.X * scale + 0.5f);
                    var ry = (int)(roundedCorners.Y * scale + 0.5f);

                    if (this.borderSize > 0 && this.BorderColorCurrent.A > 0)
                    {
                        if (regionBorder == null)
                        {
                            var ptr = Windows.Native.NativeMethods.CreateRoundRectRgn(0, 0, this.Width, this.Height, rx, ry);

                            if (ptr != IntPtr.Zero)
                            {
                                try
                                {
                                    regionBorder = Region.FromHrgn(ptr);
                                }
                                finally
                                {
                                    Windows.Native.NativeMethods.DeleteObject(ptr);
                                }
                            }
                            else
                            {
                                regionBorder = new Region();
                            }
                        }

                        g.FillRegion(brush, regionBorder);
                    }

                    if (regionBackground == null)
                    {
                        var ptr = Windows.Native.NativeMethods.CreateRoundRectRgn(borderSize, borderSize, this.Width - borderSize, this.Height - borderSize, rx, ry);

                        if (ptr != IntPtr.Zero)
                        {
                            try
                            {
                                regionBackground = Region.FromHrgn(ptr);
                            }
                            finally
                            {
                                Windows.Native.NativeMethods.DeleteObject(ptr);
                            }
                        }
                        else
                        {
                            regionBackground = new Region();
                        }
                    }

                    brush.Color = this.BackColorCurrent;
                    g.FillRegion(brush, regionBackground);
                }
            }
        }
    }
}
