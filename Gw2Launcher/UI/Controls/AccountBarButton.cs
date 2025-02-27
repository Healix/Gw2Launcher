using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace Gw2Launcher.UI.Controls
{
    class AccountBarButton : FlatButton
    {
        public event MouseEventHandler CloseClicked;
        public event MouseEventHandler BarClicked;

        public enum EdgeAlignment
        {
            None,
            Left,
            Top,
            Right,
            Bottom,
        }

        protected SolidBrush brush, brushKey;
        protected Pen penClose;

        protected EdgeAlignment alignmentColorKey;
        protected bool entered;
        protected Rectangle boundsClose, boundsText, boundsIcon;
        protected bool hoveredClose;
        protected byte opacityIcon;
        protected bool showClose, showText, showIcon, showColor;
        protected int minimumIconWidth;

        public AccountBarButton()
        {
            brush = new SolidBrush(Color.Transparent);
            brushKey = new SolidBrush(Color.Empty);

            alignmentColorKey = EdgeAlignment.None;
            Padding = new Padding(2, 0, 2, 0);

            penClose = new Pen(Color.White, 2)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Square,
                EndCap = System.Drawing.Drawing2D.LineCap.Square,
            };
        }

        public Settings.IAccount Account
        {
            get;
            set;
        }

        public Color ColorKey
        {
            get
            {
                return brushKey.Color;
            }
            set
            {
                if (brushKey.Color != value)
                {
                    brushKey.Color = value;
                    OnRedrawRequired();
                }
            }
        }

        public EdgeAlignment ColorKeyAlignment
        {
            get
            {
                return alignmentColorKey;
            }
            set
            {
                if (alignmentColorKey != value)
                {
                    alignmentColorKey = value;

                    SetColorKeyPadding();
                    OnRedrawRequired();
                }
            }
        }

        public bool ColorKeyVisible
        {
            get
            {
                return showColor;
            }
            set
            {
                if (showColor != value)
                {
                    showColor = value;

                    SetColorKeyPadding();
                    OnRedrawRequired();
                }
            }
        }

        protected void SetColorKeyPadding()
        {
            int l = 2,
                r = 2,
                t = 0,
                b = 0;

            if (showColor)
            {
                switch (alignmentColorKey)
                {
                    case EdgeAlignment.Bottom:
                        b = 4;
                        break;
                    case EdgeAlignment.Left:
                        l = 4;
                        break;
                    case EdgeAlignment.Right:
                        r = 4;
                        break;
                    case EdgeAlignment.Top:
                        t = 4;
                        break;
                }
            }

            Padding = new Padding(l, t, r, b);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            boundsClose = Rectangle.Empty;
            boundsText = Rectangle.Empty;
            boundsIcon = Rectangle.Empty;

            base.OnPaddingChanged(e);
        }

        public bool IconVisible
        {
            get
            {
                return showIcon;
            }
            set
            {
                if (showIcon != value)
                {
                    showIcon = value;
                    boundsIcon = Rectangle.Empty;
                    boundsText = Rectangle.Empty;
                    if (this.BackgroundImage != null)
                        OnRedrawRequired();
                }
            }
        }

        public int IconDefaultSize
        {
            get
            {
                return minimumIconWidth;
            }
            set
            {
                if (minimumIconWidth != value)
                {
                    minimumIconWidth = value;
                    if (showIcon && value > boundsIcon.Width)
                        OnRedrawRequired();
                }
            }
        }

        public bool TextVisible
        {
            get
            {
                return showText;
            }
            set
            {
                if (showText != value)
                {
                    showText = value;
                    boundsIcon = Rectangle.Empty;
                    boundsText = Rectangle.Empty;
                    OnRedrawRequired();
                }
            }
        }

        public bool CloseVisible
        {
            get
            {
                return showClose;
            }
            set
            {
                if (showClose != value)
                {
                    showClose = value;
                    hoveredClose = hoveredClose && value;
                    if (entered)
                        OnRedrawRequired();
                }
            }
        }

        public byte IconOpacity
        {
            get
            {
                return opacityIcon;
            }
            set
            {
                if (opacityIcon != value)
                {
                    opacityIcon = value;
                    OnRedrawRequired();
                }
            }
        }

        public bool IsTextClipped
        {
            get
            {
                if (showText)
                    return boundsText.Right + this.Padding.Right > this.Width;
                else
                    return !string.IsNullOrEmpty(this.Text);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            entered = true;

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            entered = false;

            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (showClose)
            {
                if (e.X >= boundsClose.Left && e.X <= boundsClose.Right)
                {
                    if (!hoveredClose)
                    {
                        hoveredClose = true;
                        OnRedrawRequired();
                    }
                }
                else if (hoveredClose)
                {
                    hoveredClose = false;
                    OnRedrawRequired();
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (hoveredClose)
            {
                if (CloseClicked != null)
                    CloseClicked(this, e);
            }
            else
            {
                if (BarClicked != null)
                    BarClicked(this, e);
            }

            base.OnMouseClick(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            boundsText = Rectangle.Empty;
            boundsIcon = Rectangle.Empty;

            base.OnTextChanged(e);
        }

        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            boundsText = Rectangle.Empty;
            boundsIcon = Rectangle.Empty;

            base.OnBackgroundImageChanged(e);
        }

        protected override void OnPaintBackgroundBuffer(Graphics g)
        {
            base.OnPaintBackgroundBuffer(g);
        }

        protected override void DrawText(Graphics g, string text, int x, int y, int w, int h)
        {
            TextRenderer.DrawText(g, text, this.Font, new Rectangle(x, y, w, h), ForeColorCurrent, BackColorCurrent, TextFormatFlags.VerticalCenter);
        }

        private void DrawImage(Graphics g, Image image, Rectangle bounds, byte opacity)
        {
            if (opacity > 0)
            {
                if (opacity < 255)
                {
                    using (var ia = new ImageAttributes())
                    {
                        ia.SetColorMatrix(new ColorMatrix()
                        {
                            Matrix33 = opacity / 255f,
                        });

                        g.DrawImage(image, bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
                    }
                }
                else
                {
                    g.DrawImage(image, bounds);
                }
            }
        }

        protected override void OnPaintBuffer(Graphics g)
        {
            int w = this.Width,
                h = this.Height;

            var scale = g.DpiX / 96f;

            Image image;
            if (showIcon && ((image = this.BackgroundImage) != null || minimumIconWidth > 0))
            {
                try
                {
                    if (showText && this.Text != null)
                    {
                        if (boundsIcon.IsEmpty)
                        {
                            Size sz;
                            if (image == null)
                                sz = new System.Drawing.Size(minimumIconWidth, minimumIconWidth);
                            else
                                sz = image.Size;
                            var pad = (int)(5 * scale + 0.5f);
                            sz = GetScaledDimensions(sz, w - this.Padding.Horizontal - pad, h - this.Padding.Vertical);
                            boundsIcon = new Rectangle(this.Padding.Left + pad, (h - sz.Height) / 2, sz.Width, sz.Height);
                        }
                        
                        if (boundsText.IsEmpty)
                        {
                            var pad = (int)(10 * scale + 0.5f);
                            var szText = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(this.Width - this.Padding.Horizontal - pad - boundsIcon.Width, this.Height - this.Padding.Vertical), TextFormatFlags.VerticalCenter);
                            boundsText = new Rectangle(this.Padding.Left + pad + boundsIcon.Width, 0, szText.Width, szText.Height);
                        }

                        if (image != null)
                        {
                            DrawImage(g, image, boundsIcon, opacityIcon);
                        }
                        DrawText(g, this.Text, boundsText.Left, boundsText.Top, w - boundsText.Left - Padding.Right, h); //boundsText.Top, boundsText.Width, boundsText.Height);// w - this.Padding.Horizontal - 10 - boundsIcon.Width, h - this.Padding.Vertical);
                    }
                    else
                    {
                        if (boundsIcon.IsEmpty)
                        {
                            int mw = w - this.Padding.Horizontal,
                                mh = h - this.Padding.Vertical,
                                x, 
                                y;
                            Size sz;

                            if (showColor)
                            {
                                switch (alignmentColorKey)
                                {
                                    case EdgeAlignment.Bottom:
                                    case EdgeAlignment.Top:
                                        //mh -= 2;
                                        break;
                                    default:
                                        //mw -= 2;
                                        break;
                                }
                            }

                            sz = GetScaledDimensions(image.Size, mw, mh);

                            x = w - sz.Width;
                            y = h - sz.Height;

                            if (sz.Width == mw)
                            {
                                x = (x - this.Padding.Horizontal) / 2 + this.Padding.Left;
                                y /= 2;
                            }
                            else if (sz.Height == mh)
                            {
                                y = (y - this.Padding.Vertical) / 2 + this.Padding.Top;
                                x /= 2;
                            }
                            else
                            {
                                x /= 2;
                                y /= 2;
                            }

                            //if (showColor)
                            //{
                            //    switch (alignmentColorKey)
                            //    {
                            //        case EdgeAlignment.Bottom:
                            //            if (sz.Height == mh)
                            //                y = 0;
                            //            break;
                            //        case EdgeAlignment.Top:
                            //            if (sz.Height == mh)
                            //                y = 4;
                            //            break;
                            //        case EdgeAlignment.Left:
                            //            if (sz.Width == mw)
                            //                x = 4;
                            //            break;
                            //        case EdgeAlignment.Right:
                            //            if (sz.Width == mw)
                            //                x = 0;
                            //            break;
                            //    }
                            //}

                            boundsIcon = new Rectangle(x, y, sz.Width, sz.Height);
                        }
                        DrawImage(g, image, boundsIcon, opacityIcon);
                    }
                }
                catch { }
            }
            else if (showText)
            {
                if (boundsText.IsEmpty)
                {
                    var szText = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(w - this.Padding.Horizontal, h - this.Padding.Vertical), TextFormatFlags.VerticalCenter);
                    boundsText = new Rectangle(this.Padding.Left, 0, szText.Width, szText.Height);
                }

                DrawText(g, this.Text, boundsText.Left, boundsText.Top, w - boundsText.Left - this.Padding.Right, h);
            }

            var fadeWidth = (int)(30 * scale + 0.5f);

            if (entered && showClose)
            {
                if (boundsClose.IsEmpty)
                {
                    var fh = this.Font.Height / 2;
                    int x = w - this.Padding.Right - fh - (int)(6 * scale + 0.5f),
                        y = (h - fh) / 2;
                    boundsClose = new Rectangle(x, y, fh, fh);
                }

                int br;
                if (showText)
                    br = boundsText.Right;
                else
                    br = boundsIcon.Right;

                if (br > boundsClose.Left - fadeWidth / 2)
                {
                    var fadePad = (int)(5 * scale + 0.5f);

                    using (var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(boundsClose.Left - fadeWidth - fadePad, 0), new Point(boundsClose.Left - fadePad, 0), Color.Transparent, this.BackColorCurrent))
                    {
                        g.FillRectangle(gradient, boundsClose.Left - fadePad - fadePad, this.Padding.Top, fadeWidth, h - this.Padding.Vertical);
                    }

                    brush.Color = this.BackColorCurrent;
                    g.FillRectangle(brush, boundsClose.Left - fadePad, this.Padding.Top, w - boundsClose.Left - Padding.Right + fadePad, h - this.Padding.Vertical);
                }

                var colorClose = hoveredClose ? Color.White : Color.FromArgb(170, 170, 170);
                penClose.Color = colorClose;

                var padding = 0;

                g.DrawLine(penClose, boundsClose.Left + padding, boundsClose.Top + padding, boundsClose.Right - padding - penClose.Width / 2, boundsClose.Bottom - padding - penClose.Width / 2);
                g.DrawLine(penClose, boundsClose.Left + padding, boundsClose.Bottom - padding - penClose.Width / 2, boundsClose.Right - padding - penClose.Width / 2, boundsClose.Top + padding);
            }
            else
            {
                int x = this.Width - this.Padding.Right;

                if (boundsText.Right > x)
                {
                    using (var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(x - fadeWidth, 0), new Point(x, 0), Color.Transparent, this.BackColorCurrent))
                    {
                        g.FillRectangle(gradient, x - fadeWidth, this.Padding.Top, fadeWidth, h - this.Padding.Vertical);
                    }
                }
            }

            if (showColor && alignmentColorKey != EdgeAlignment.None)
            {
                int x, y, w1, h1;
                float p;

                switch (alignmentColorKey)
                {
                    case EdgeAlignment.Left:
                    case EdgeAlignment.Right:

                        p = 0.85f;
                        w1 = (int)(2 * scale + 0.5f);
                        h1 = (int)(h * p);
                        y = (h - h1) / 2;

                        if (alignmentColorKey == EdgeAlignment.Right)
                            x = w - (int)(3 * scale + 0.5f);
                        else
                            x = (int)(1 * scale + 0.5f);

                        g.FillRectangle(brushKey, x, y, w1, h1);

                        break;
                    case EdgeAlignment.Top:
                    case EdgeAlignment.Bottom:

                        p = 0.95f;
                        w1 = (int)(w * p);
                        h1 = (int)(2 * scale + 0.5f);
                        x = (w - w1) / 2;

                        if (alignmentColorKey == EdgeAlignment.Top)
                            y = (int)(1 * scale + 0.5f);
                        else
                            y = h - (int)(3 * scale + 0.5f);

                        g.FillRectangle(brushKey, x, y, w1, h1);

                        break;
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            boundsText = Rectangle.Empty;
            boundsClose = Rectangle.Empty;
            boundsIcon = Rectangle.Empty;

            base.OnSizeChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                brushKey.Dispose();
                brush.Dispose();
                penClose.Dispose();
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            using (var g = this.CreateGraphics())
            {
                var sz = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(proposedSize.Width, proposedSize.Height));
                var w = sz.Width + this.Padding.Horizontal;
                var h = sz.Height + this.Padding.Vertical;

                if (this.MinimumSize.Width > w)
                {
                    w = this.MinimumSize.Width;
                }
                else if (this.MaximumSize.Width > 0 && this.MaximumSize.Width < w)
                {
                    w = this.MaximumSize.Width;
                }

                if (this.MinimumSize.Height > h)
                {
                    h = this.MinimumSize.Height;
                }
                else if (this.MaximumSize.Height > 0 && this.MaximumSize.Height < h)
                {
                    h = this.MaximumSize.Height;
                }

                return new Size(w, h);
            }
        }
    }
}
