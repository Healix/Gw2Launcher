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
        private VerticalTagType tags;

        [Flags]
        public enum VerticalTagType : byte
        {
            None = 0,
            Top = 1,
            Bottom = 2,
        }

        public class TagData
        {
            public event EventHandler Changed;

            public TagData()
            {
            }

            private Color _ForeColor;
            public Color ForeColor
            {
                get
                {
                    return _ForeColor;
                }
                set
                {
                    if (_ForeColor != value)
                    {
                        _ForeColor = value;
                        if (_Visible)
                        {
                            OnChanged();
                        }
                    }
                }
            }

            private Color _BackColor;
            public Color BackColor
            {
                get
                {
                    return _BackColor;
                }
                set
                {
                    if (_BackColor != value)
                    {
                        _BackColor = value;
                        if (_Visible)
                        {
                            OnChanged();
                        }
                    }
                }
            }

            private string _Text;
            public string Text
            {
                get
                {
                    return _Text;
                }
                set
                {
                    if (_Text != value)
                    {
                        _Text = value;
                        if (_Visible)
                        {
                            OnChanged();
                        }
                    }
                }
            }

            private Font _Font;
            public Font Font
            {
                get
                {
                    return _Font;
                }
                set
                {
                    if (_Font != value)
                    {
                        _Font = value;
                        if (_Visible)
                        {
                            OnChanged();
                        }
                    }
                }
            }

            private bool _Visible;
            public bool Visible
            {
                get
                {
                    return _Visible;
                }
                set
                {
                    if (_Visible != value)
                    {
                        _Visible = value;
                        OnChanged();
                    }
                }
            }

            private void OnChanged()
            {
                if (Changed != null)
                {
                    Changed(this, EventArgs.Empty);
                }
            }
        }

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

        private TagData _TopTag;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TagData TopTag
        {
            get
            {
                return _TopTag;
            }
            set
            {
                if (_TopTag != value)
                {
                    var b = false;
                    if (_TopTag != null)
                    {
                        b = _TopTag.Visible;
                        _TopTag.Changed -= TagData_Changed;
                    }
                    if (value != null)
                    {
                        b = value.Visible || b;
                        value.Changed += TagData_Changed;
                    }
                    if (value != null && value.Visible)
                        tags |= VerticalTagType.Top;
                    else
                        tags &= ~VerticalTagType.Top;
                    _TopTag = value;
                    if (b)
                    {
                        OnRedrawRequired();
                    }
                }
            }
        }

        private TagData _BottomTag;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TagData BottomTag
        {
            get
            {
                return _BottomTag;
            }
            set
            {
                if (_BottomTag != value)
                {
                    var b = false;
                    if (_BottomTag != null)
                    {
                        b = _BottomTag.Visible;
                        _BottomTag.Changed -= TagData_Changed;
                    }
                    if (value != null)
                    {
                        b = value.Visible || b;
                        value.Changed += TagData_Changed;
                    }
                    if (value != null && value.Visible)
                        tags |= VerticalTagType.Bottom;
                    else
                        tags &= ~VerticalTagType.Bottom;
                    _BottomTag = value;
                    if (b)
                    {
                        OnRedrawRequired();
                    }
                }
            }
        }

        void TagData_Changed(object sender, EventArgs e)
        {
            var t = (TagData)sender;
            var ty = VerticalTagType.None;

            if (_TopTag == t)
            {
                ty = VerticalTagType.Top;
            }
            else if (_BottomTag == t)
            {
                ty = VerticalTagType.Bottom;
            }
            else
            {
                return;
            }

            if ((tags & ty) != 0)
            {
                if (!t.Visible)
                {
                    tags &= ~ty;
                }
            }
            else if (t.Visible)
            {
                tags |= ty;
            }

            OnRedrawRequired();
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

            if (tags != VerticalTagType.None)
            {
                using (var brush = new SolidBrush(Color.Transparent))
                {
                    if ((tags & VerticalTagType.Top) != 0)
                    {
                        brush.Color = _TopTag.BackColor;

                        var f = _TopTag.Font != null ? _TopTag.Font : this.Font;
                        var h = (int)(f.GetHeight(g) + 0.5f);
                        var r = new Rectangle(0, -1, this.Width, h);

                        g.FillRectangle(brush, 0, 0, r.Width, h - 1);
                        TextRenderer.DrawText(g, _TopTag.Text, f, r, _TopTag.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                    }

                    if ((tags & VerticalTagType.Bottom) != 0)
                    {
                        brush.Color = _BottomTag.BackColor;

                        var f = _BottomTag.Font != null ? _BottomTag.Font : this.Font;
                        var h = (int)(f.GetHeight(g) + 0.5f);
                        var r = new Rectangle(0, this.Height - h, this.Width, h);

                        g.FillRectangle(brush, 0, r.Y + 1, r.Width, h - 1);
                        TextRenderer.DrawText(g, _BottomTag.Text, f, r, _BottomTag.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                    }
                }
            }

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
