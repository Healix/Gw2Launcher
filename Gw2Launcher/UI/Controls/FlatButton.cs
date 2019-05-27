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
                if (value)
                    isHovered = false;
                OnRedrawRequired();
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
                    this.Invalidate();
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

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            OnRedrawRequired();
        }

        protected virtual BufferedGraphics AllocateBuffer(Graphics g)
        {
            return BufferedGraphicsManager.Current.Allocate(g, this.DisplayRectangle);
        }

        protected virtual void DrawText(Graphics g, string text, int x, int y, int w, int h)
        {
            TextRenderer.DrawText(g, text, this.Font, new Rectangle(x, y, w, h), ForeColorCurrent, BackColorCurrent, TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
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
            Image image;
            if ((image = this.BackgroundImage) != null)
            {
                try
                {
                    int w = this.Width,
                        h = this.Height;
                    var sz = GetScaledDimensions(image, w, h);

                    if (this.Text != null)
                    {
                        g.DrawImage(image, this.Padding.Left, (h - sz.Height) / 2, sz.Width, sz.Height);
                        DrawText(g, this.Text, this.Padding.Left + 10 + sz.Width, this.Padding.Top, this.Width - this.Padding.Horizontal - 10 - sz.Width, this.Height - this.Padding.Vertical);
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
                DrawText(g, this.Text, this.Padding.Left + 10, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical);
            }
        }

        protected virtual void OnPaintBackgroundBuffer(Graphics g)
        {
            g.Clear(BackColorCurrent);

            if (borderColor.A > 0)
            {
                using (var p = new Pen(borderColor,1))
                {
                    g.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (redraw)
            {
                redraw = false;

                OnPaintBuffer(buffer.Graphics);
            }

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                if (buffer == null)
                    buffer = AllocateBuffer(e.Graphics);

                OnPaintBackgroundBuffer(buffer.Graphics);
            }
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
            if (disposing)
            {
                if (buffer != null)
                {
                    buffer.Dispose();
                    buffer = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
