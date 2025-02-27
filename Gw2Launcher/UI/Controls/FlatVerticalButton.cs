using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class FlatVerticalButton : FlatButton
    {
        protected Bitmap bufferText;
        private Size sizeText;

        public FlatVerticalButton()
            : base()
        {
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        private bool _ShowNotification;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowNotification
        {
            get
            {
                return _ShowNotification;
            }
            set
            {
                if (_ShowNotification != value)
                {
                    _ShowNotification = value;
                    OnRedrawRequired();
                }
            }
        }

        private void OnRedrawTextRequired()
        {
            if (sizeText.Height != 0)
            {
                sizeText = Size.Empty;
                OnRedrawRequired();
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);

            OnRedrawTextRequired();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            OnRedrawTextRequired();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            OnRedrawTextRequired();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            OnRedrawTextRequired();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            using (var g = this.CreateGraphics())
            {
                var space = (int)(10 * g.DpiX / 96f + 0.5f);
                var sz = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(proposedSize.Height - this.Padding.Vertical - space * 2, proposedSize.Width - this.Padding.Horizontal), TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

                sz.Width += space * 2;

                switch (alignment)
                {
                    case HorizontalAlignment.Center:

                        break;
                    case HorizontalAlignment.Left:
                    case HorizontalAlignment.Right:

                        break;
                }

                var w = sz.Height + this.Padding.Horizontal;
                var h = sz.Width + this.Padding.Vertical;

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

        protected override void OnPaintBuffer(Graphics g)
        {
            var space = (int)(10 * g.DpiX / 96f + 0.5f);

            if (sizeText.Height == 0)
            {
                sizeText = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(this.Height - this.Padding.Vertical - space * 2, this.Width - this.Padding.Horizontal), TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
            }

            if (bufferText == null || bufferText.Width != sizeText.Width || bufferText.Height != sizeText.Height)
            {
                if (bufferText != null)
                    bufferText.Dispose();
                bufferText = new Bitmap(sizeText.Width, sizeText.Height, g);
            }

            using (var gb = Graphics.FromImage(bufferText))
            {
                TextRenderer.DrawText(gb, this.Text, this.Font, new Rectangle(Point.Empty, sizeText), ForeColorCurrent, BackColorCurrent, TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
            }

            int x = (this.Width + sizeText.Height) / 2 + Padding.Left,
                y;

            switch (alignment)
            {
                case HorizontalAlignment.Center:

                    y = (this.Height - sizeText.Width) / 2;

                    break;
                case HorizontalAlignment.Right:

                    y = this.Height - sizeText.Width - this.Padding.Bottom - space;

                    break;
                case HorizontalAlignment.Left:
                default:

                    y = this.Padding.Top + space;

                    break;
            }

            g.TranslateTransform(x, y);
            g.RotateTransform(90);
            g.DrawImage(bufferText, new Point(0, 0));
            g.ResetTransform();

            if (_ShowNotification)
            {
                var sz = (int)(13 * g.DpiX / 96f + 0.5f);

                y = (int)(2 * g.DpiX / 96f + 0.5f);
                x = this.Width - sz - y;

                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                using (var b = new SolidBrush(Color.FromArgb(200, 18, 18)))
                {
                    g.FillEllipse(b, x, y, sz - 1, sz - 1);
                }

                using (var p = new Pen(Color.White, sz * 0.15f))
                {
                    p.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

                    var gap = sz * 0.2f;
                    var x1 = x + sz / 2f - 0.5f;
                    var y1 = y + gap;
                    var h1 = sz - gap * 2 - 1;
                    var h2 = h1 * 0.25f;
                    var y2 = y1 + h1 - h2 - (sz * 0.1f);

                    g.DrawLine(p, x1, y1, x1, y2);
                    g.DrawLine(p, x1, y1 + h1 - h2, x1, y1 + h1);
                }

                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (bufferText != null)
                {
                    bufferText.Dispose();
                    bufferText = null;
                }
            }
        }
    }
}
