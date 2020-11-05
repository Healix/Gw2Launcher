using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class FlatButton : Control
    {
        protected bool 
            redraw, 
            isHovered, 
            isSelected;

        protected BufferedGraphics buffer;

        public FlatButton()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            alignment = HorizontalAlignment.Left;
        }

        protected void OnRedrawRequired()
        {
            redraw = true;
            this.Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
            OnRedrawRequired();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            OnRedrawRequired();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            isHovered = false;
            OnRedrawRequired();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            OnRedrawRequired();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            OnRedrawRequired();
        }

        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            base.OnBackgroundImageChanged(e);
            OnRedrawRequired();
        }

        public bool Selected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                //if (value)
                //    isHovered = false;
                OnRedrawRequired();
            }
        }

        public bool IsHovered
        {
            get
            {
                return isHovered;
            }
        }

        protected Color borderColor;
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
                    OnRedrawRequired();
                }
            }
        }

        protected Color backColorHovered;
        public Color BackColorHovered
        {
            get
            {
                if (backColorHovered.IsEmpty)
                    return this.BackColor;
                return backColorHovered;
            }
            set
            {
                backColorHovered = value;
            }
        }

        protected Color foreColorHovered;
        public Color ForeColorHovered
        {
            get
            {
                if (foreColorHovered.IsEmpty)
                    return this.ForeColor;
                return foreColorHovered;
            }
            set
            {
                foreColorHovered = value;
            }
        }

        protected Color backColorSelected;
        public Color BackColorSelected
        {
            get
            {
                if (backColorSelected.IsEmpty)
                    return this.BackColor;
                return backColorSelected;
            }
            set
            {
                backColorSelected = value;
            }
        }

        protected Color foreColorSelected;
        public Color ForeColorSelected
        {
            get
            {
                if (foreColorSelected.IsEmpty)
                    return this.ForeColor;
                return foreColorSelected;
            }
            set
            {
                foreColorSelected = value;
            }
        }

        public Color BackColorCurrent
        {
            get
            {
                if (isHovered)
                    return this.BackColorHovered;
                else if (isSelected)
                    return this.BackColorSelected;
                else
                    return this.BackColor;
            }
        }

        public Color ForeColorCurrent
        {
            get
            {
                if (isHovered)
                    return this.ForeColorHovered;
                else if (isSelected)
                    return this.ForeColorSelected;
                else
                    return this.ForeColor;
            }
        }

        protected AnchorStyles borderStyle;
        [System.ComponentModel.DefaultValue(AnchorStyles.None)]
        public AnchorStyles BorderStyle
        {
            get
            {
                return borderStyle;
            }
            set
            {
                if (borderStyle != value)
                {
                    borderStyle = value;
                    OnRedrawRequired();
                }
            }
        }

        protected HorizontalAlignment alignment;
        [System.ComponentModel.DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment Alignment
        {
            get
            {
                return alignment;
            }
            set
            {
                if (alignment != value)
                {
                    alignment = value;
                    OnRedrawRequired();
                }
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            OnRedrawRequired();
        }

        protected virtual BufferedGraphics AllocateBuffer(Graphics g)
        {
            return BufferedGraphicsManager.Current.Allocate(g, this.ClientRectangle);
        }

        protected virtual void DrawText(Graphics g, string text, int x, int y, int w, int h)
        {
            var f = TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;

            switch (alignment)
            {
                case HorizontalAlignment.Center:
                    f |= TextFormatFlags.HorizontalCenter;
                    break;
                case HorizontalAlignment.Right:
                    f |= TextFormatFlags.Right;
                    break;
            }

            TextRenderer.DrawText(g, text, this.Font, new Rectangle(x, y, w, h), ForeColorCurrent, BackColorCurrent, f);
        }

        protected virtual Size GetScaledDimensions(Image image, int maxW, int maxH)
        {
            int w = image.Width,
                h = image.Height;

            float rw = (float)maxW / w,
                  rh = (float)maxH / h,
                  r = rw < rh ? rw : rh;

            if (r < 1)
            {
                w = (int)(w * r + 0.5f);
                h = (int)(h * r + 0.5f);
            }
            
            return new Size(w, h);
        }

        protected void DrawImage(Graphics g, Image image, int x, int y)
        {
            try
            {
                int w = image.Width,
                    h = image.Height,
                    cw = this.Width,
                    ch = this.Height;

                float rx = (float)cw / w,
                      ry = (float)ch / h,
                      r = rx < ry ? rx : ry;

                if (r < 1)
                {
                    w = (int)(w * r + 0.5f);
                    h = (int)(w * r + 0.5f);
                }

                g.DrawImage(image, x, y, w, h);
            }
            catch { }
        }

        protected virtual void OnPaintBuffer(Graphics g)
        {
            int w = this.Width;
            int h = this.Height;
            int px;

            switch (alignment)
            {
                case HorizontalAlignment.Center:
                    px = 0;
                    break;
                default:
                    px = (int)(10 * g.DpiX / 96f + 0.5f);
                    break;
            }

            Image image;
            if ((image = this.BackgroundImage) != null)
            {
                try
                {
                    var sz = GetScaledDimensions(image, w, h);

                    if (this.Text != null)
                    {
                        g.DrawImage(image, this.Padding.Left, (h - sz.Height) / 2, sz.Width, sz.Height);
                        DrawText(g, this.Text, this.Padding.Left + px + sz.Width, this.Padding.Top, w - this.Padding.Horizontal - px * 2 - sz.Width, h - this.Padding.Vertical);
                    }
                    else
                    {
                        g.DrawImage(image, (w + sz.Width) / 2, (h - sz.Height) / 2, sz.Width, sz.Height);
                    }
                }
                catch { }
            }
            else
            {
                DrawText(g, this.Text, this.Padding.Left + px, this.Padding.Top, w - this.Padding.Horizontal - px * 2, h - this.Padding.Vertical);
            }
        }

        protected virtual void OnPaintBackgroundBuffer(Graphics g)
        {
            g.Clear(BackColorCurrent);

            if (borderStyle != AnchorStyles.None && borderColor.A > 0)
            {
                var pw = (int)(g.DpiX / 96f + 0.5f);

                using (var p = new Pen(borderColor, pw))
                {
                    int x = 0,
                        y = 0,
                        w = this.Width - 1,
                        h = this.Height - 1;

                    switch (borderStyle & (AnchorStyles.Left | AnchorStyles.Right))
                    {
                        case AnchorStyles.Left: //only left
                            w += pw;
                            break;
                        case AnchorStyles.Right: //only right
                            x -= pw;
                            w += pw;
                            break;
                        case AnchorStyles.None:
                            x -= pw;
                            w += pw * 2;
                            break;
                    }

                    switch (borderStyle & (AnchorStyles.Top | AnchorStyles.Bottom))
                    {
                        case AnchorStyles.Top: //only top
                            h += pw;
                            break;
                        case AnchorStyles.Bottom: //only bottom
                            y -= pw;
                            h += pw;
                            break;
                        case AnchorStyles.None:
                            y -= pw;
                            h += pw * 2;
                            break;
                    }

                    g.DrawRectangle(p, x, y, w, h);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (buffer == null)
                    buffer = AllocateBuffer(e.Graphics);

                OnPaintBackgroundBuffer(buffer.Graphics);
                OnPaintBuffer(buffer.Graphics);
            }

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            isHovered = true;
            OnRedrawRequired();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            isHovered = false;
            OnRedrawRequired();
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
