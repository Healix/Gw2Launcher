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
                    int l = 2,
                        r = 2,
                        t = 0,
                        b = 0;

                    if (showColor)
                    {
                        switch (value)
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
                    alignmentColorKey = value;
                    
                    OnRedrawRequired();
                }
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            boundsClose = Rectangle.Empty;
            boundsText = Rectangle.Empty;
            boundsIcon = Rectangle.Empty;

            base.OnPaddingChanged(e);
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
                    int l = 2,
                        r = 2,
                        t = 0,
                        b = 0;

                    if (value)
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

                    showColor = value;
                    OnRedrawRequired();
                }
            }
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

            Image image;
            if (showIcon && (image = this.BackgroundImage) != null)
            {
                try
                {
                    if (showText && this.Text != null)
                    {
                        if (boundsIcon.IsEmpty)
                        {
                            var sz = GetScaledDimensions(image, w - this.Padding.Horizontal - 5, h - this.Padding.Vertical);
                            boundsIcon = new Rectangle(this.Padding.Left + 5, (h - sz.Height) / 2, sz.Width, sz.Height);
                        }

                        if (boundsText.IsEmpty)
                        {
                            var szText = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(this.Width - this.Padding.Horizontal - 10 - boundsIcon.Width, this.Height - this.Padding.Vertical), TextFormatFlags.VerticalCenter);
                            boundsText = new Rectangle(this.Padding.Left + 10 + boundsIcon.Width, 0, szText.Width, szText.Height);
                        }

                        DrawImage(g, image, boundsIcon, opacityIcon);
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

                            sz = GetScaledDimensions(image, mw, mh);

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

            if (entered && showClose)
            {
                if (boundsClose.IsEmpty)
                {
                    var fh = this.Font.Height / 2;
                    int x = w - this.Padding.Right - fh - 6,
                        y = (h - fh) / 2;
                    boundsClose = new Rectangle(x, y, fh, fh);
                }

                int br;
                if (showText)
                    br = boundsText.Right;
                else
                    br = boundsIcon.Right;

                if (br > boundsClose.Left - 15)
                {
                    using (var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(boundsClose.Left - 35, 0), new Point(boundsClose.Left - 5, 0), Color.Transparent, this.BackColorCurrent))
                    {
                        g.FillRectangle(gradient, boundsClose.Left - 35, this.Padding.Top, 30, h - this.Padding.Vertical);
                    }

                    brush.Color = this.BackColorCurrent;
                    g.FillRectangle(brush, boundsClose.Left - 5, this.Padding.Top, w - boundsClose.Left - Padding.Right + 5, h - this.Padding.Vertical);
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
                    using (var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(x - 30, 0), new Point(x, 0), Color.Transparent, this.BackColorCurrent))
                    {
                        g.FillRectangle(gradient, x - 30, this.Padding.Top, 30, h - this.Padding.Vertical);
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
                        w1 = 2;
                        h1 = (int)(h * p);
                        y = (h - h1) / 2;

                        if (alignmentColorKey == EdgeAlignment.Right)
                            x = w - 3;
                        else
                            x = 1;

                        g.FillRectangle(brushKey, x, y, w1, h1);

                        break;
                    case EdgeAlignment.Top:
                    case EdgeAlignment.Bottom:

                        p = 0.95f;
                        w1 = (int)(w * p);
                        h1 = 2;
                        x = (w - w1) / 2;

                        if (alignmentColorKey == EdgeAlignment.Top)
                            y = 1;
                        else
                            y = h - 3;

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
                return new Size(sz.Width + this.Padding.Horizontal, sz.Height + this.Padding.Vertical);
            }
        }
    }
}
