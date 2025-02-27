using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using Gw2Launcher.Api;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class DailyAchievement : Base.BaseControl
    {
        private BufferedGraphics buffer;
        private bool redraw, resize;
        private bool hovered;

        public interface IDataSource
        {
            ushort ID
            {
                get;
            }

            string Name
            {
                get;
            }

            string Description
            {
                get;
            }

            Image Icon
            {
                get;
            }

            bool IsUnknown
            {
                get;
            }

            Settings.TaggedType Tagged
            {
                get;
                set;
            }

            bool Favorite
            {
                get;
                set;
            }
        }

        public enum FavoriteAlignment
        {
            Right,
            Left,
        }

        private class Control
        {
            public Rectangle bounds;
            public bool visible;
        }

        private class Label : Control
        {
            public string value;
            public Font font;
            public Color foreColor;

            public Size Measure(Graphics g, Size proposedSize)
            {
                if (!visible || string.IsNullOrEmpty(value))
                    return Size.Empty;
                return TextRenderer.MeasureText(g, value, font, proposedSize, TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis);
            }

            public void Draw(Graphics g)
            {
                if (visible && bounds.Height > 0)
                    TextRenderer.DrawText(g, value, font, bounds, foreColor, TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis);
            }
        }

        private class Favorite : Control
        {
            public FavoriteVisibility visibility;
            public bool enabled;
            public bool selected;
            public FavoriteAlignment alignment;

            public void Draw(Graphics g)
            {
                var x = bounds.X;
                var y = bounds.Y;
                var w = bounds.Width;
                var h = bounds.Height;

                var points = new PointF[]
                {
                    new PointF(x + w * 0.5f, y + h * 0.1765f),
                    new PointF(x + w * 0.65f, y),
                    new PointF(x + w * 0.85f, y),
                    new PointF(x + w, y + h * 0.1765f),
                    new PointF(x + w, y + h * 0.4118f),
                    new PointF(x + w * 0.55f, y + h * 0.9412f),
                    new PointF(x + w * 0.45f, y + h * 0.9412f),
                    new PointF(x + w * 0.0f, y + h * 0.4118f),
                    new PointF(x, y + h * 0.1765f),
                    new PointF(x + w * 0.15f, y),
                    new PointF(x + w * 0.35f, y),
                };

                Color fillColor, borderColor;

                if (selected)
                {
                    fillColor = Color.FromArgb(230, 60, 70); //UiColors.GetColor(UiColors.Colors.DailiesTextLight);
                    borderColor = Color.FromArgb(218, 29, 37); //UiColors.GetColor(UiColors.Colors.DailiesTextLight);
                }
                else
                {
                    fillColor = Color.Transparent;
                    borderColor = UiColors.GetColor(UiColors.Colors.DailiesTextLight);
                }

                using (var b = new SolidBrush(borderColor))
                {
                    using (var p = new Pen(b))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        if (fillColor.A != 0)
                        {
                            b.Color = fillColor;
                            g.FillPolygon(b, points);
                        }

                        g.DrawPolygon(p, points);

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                    }
                }
            }
        }

        [Flags]
        public enum FavoriteVisibility
        {
            Never = 0,
            Always = 1,
            Hovered = 2,
            Selected = 4,
        }

        private Label labelName, labelDescription, labelLevel;
        private Control icon;
        private Favorite favorite;
        private Image image;
        private IDataSource source;

        public DailyAchievement()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            labelName = new Label()
            {
                foreColor = this.ForeColor,
                font = this.Font,
            };
            labelDescription = new Label()
            {
                foreColor = this.ForeColor,
                font = this.Font,
            };
            labelLevel = new Label()
            {
                foreColor = SystemColors.GrayText,
                font = this.Font,
            };
            icon = new Control();
            favorite = new Favorite();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            labelName.foreColor = this.ForeColor;
            labelDescription.foreColor = this.ForeColor;
            OnRedrawRequired(false);
        }

        private void OnRedrawRequired(bool resize)
        {
            if (resize)
            {
                this.resize = true;
            }

            if (!redraw)
            {
                redraw = true;
                this.Invalidate();
            }
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

        public Daily.Category Category
        {
            get;
            set;
        }

        public IDataSource DataSource
        {
            get
            {
                return source;
            }
            set
            {
                if (value == null)
                {
                    source = null;
                }
                else if (!object.ReferenceEquals(source, value))
                {
                    source = value;
                    labelName.value = value.Name;
                    labelDescription.value = value.Description;
                    //labelLevel.value = "Level " + daily.MinLevel + " to " + daily.MaxLevel;
                    image = value.Icon;
                    if (favorite.selected != value.Favorite)
                    {
                        favorite.selected = value.Favorite;
                        OnFavStateChanged();
                    }
                    OnRedrawRequired(true);
                }
            }
        }

        public string NameValue
        {
            get
            {
                return labelName.value;
            }
            set
            {
                labelName.value = value;
                OnRedrawRequired(true);
            }
        }

        public bool NameVisible
        {
            get
            {
                return labelName.visible;
            }
            set
            {
                if (labelName.visible != value)
                {
                    labelName.visible = value;
                    OnRedrawRequired(true);
                }
            }
        }

        public Font NameFont
        {
            get
            {
                return labelName.font;
            }
            set
            {
                labelName.font = value;
                OnRedrawRequired(true);
            }
        }

        public string DescriptionValue
        {
            get
            {
                return labelDescription.value;
            }
            set
            {
                labelDescription.value = value;
                OnRedrawRequired(true);
            }
        }

        public bool DescriptionVisible
        {
            get
            {
                return labelDescription.visible;
            }
            set
            {
                if (labelDescription.visible != value)
                {
                    labelDescription.visible = value;
                    OnRedrawRequired(true);
                }
            }
        }

        public Font DescriptionFont
        {
            get
            {
                return labelDescription.font;
            }
            set
            {
                labelDescription.font = value;
                OnRedrawRequired(true);
            }
        }

        public string LevelValue
        {
            get
            {
                return labelLevel.value;
            }
            set
            {
                labelLevel.value = value;
                OnRedrawRequired(true);
            }
        }

        public bool LevelVisible
        {
            get
            {
                return labelLevel.visible;
            }
            set
            {
                if (labelLevel.visible != value)
                {
                    labelLevel.visible = value;
                    OnRedrawRequired(true);
                }
            }
        }

        public Font LevelFont
        {
            get
            {
                return labelLevel.font;
            }
            set
            {
                labelLevel.font = value;
                OnRedrawRequired(true);
            }
        }

        public Image IconValue
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                if (icon.visible)
                {
                    if (image != null && icon.bounds.Size.IsEmpty)
                    {
                        icon.bounds.Size = image.Size;
                        OnRedrawRequired(true);
                    }
                    else
                    {
                        OnRedrawRequired(false);
                    }
                }
            }
        }

        public Size IconSize
        {
            get
            {
                return icon.bounds.Size;
            }
            set
            {
                icon.bounds.Size = value;
                if (icon.visible)
                {
                    OnRedrawRequired(true);
                }
            }
        }

        public bool IconVisible
        {
            get
            {
                return icon.visible;
            }
            set
            {
                if (icon.visible != value)
                {
                    icon.visible = value;
                    OnRedrawRequired(true);
                }
            }
        }

        public bool ProgressVisible
        {
            get
            {
                return _ProgressValue != 0;
            }
            set
            {
                if (value)
                {
                    if (_ProgressValue == 0)
                    {
                        _ProgressValue = 1;
                    }

                    if (_ProgressDisplayedVisible)
                    {
                        OnRedrawRequired(true);
                    }
                }
                else if (_ProgressValue != 0)
                {
                    OnRedrawRequired(_ProgressValue == 255 || _ProgressDisplayedVisible);
                    _ProgressValue = 0;
                }
            }
        }

        public bool ProgressClaimed
        {
            get
            {
                return _ProgressValue == 255;
            }
            set
            {
                if (_ProgressValue != 0)
                {
                    if (value)
                    {
                        if (_ProgressValue != 255)
                        {
                            _ProgressValue = 255;
                            OnRedrawRequired(true);
                        }
                    }
                    else if (_ProgressValue == 255)
                    {
                        _ProgressValue = 254;
                        OnRedrawRequired(true);
                    }
                }
            }
        }

        private byte _ProgressValue; //0 = disabled, 1-254 = %, 255 = claimed
        public float ProgressValue
        {
            get
            {
                switch (_ProgressValue)
                {
                    case 0:
                    case 1:

                        return 0;

                    case 254:
                    case 255:

                        return 1;

                    default:

                        return (_ProgressValue - 1) / 253f;
                }
            }
            set
            {
                if (_ProgressValue != 0)
                {
                    var v = (int)(value * 253) + 1;
                    if (v < 1)
                    {
                        v = 1;
                    }
                    else if (v > 254)
                    {
                        v = 254;
                    }
                    if (v != _ProgressValue)
                    {
                        OnRedrawRequired(_ProgressValue == 255);
                        _ProgressValue = (byte)v;
                    }
                }
            }
        }

        private bool _ProgressDisplayedVisible;
        public bool ProgressDisplayedVisible
        {
            get
            {
                return _ProgressDisplayedVisible;
            }
            set
            {
                if (_ProgressDisplayedVisible != value)
                {
                    _ProgressDisplayedVisible = value;

                    if (ProgressVisible)
                    {
                        OnRedrawRequired(_ProgressValue != 255);
                    }
                }
            }
        }

        private ushort _ProgressDisplayedValue;
        public ushort ProgressDisplayedValue
        {
            get
            {
                return _ProgressDisplayedValue;
            }
            set
            {
                if (_ProgressDisplayedValue != value)
                {
                    _ProgressDisplayedValue = value;

                    if (_ProgressDisplayedVisible && ProgressVisible)
                    {
                        OnRedrawRequired(false);
                    }
                }
            }
        }

        private ushort _ProgressDisplayedTotal;
        /// <summary>
        /// Optional total; 0 will not be shown
        /// </summary>
        public ushort ProgressDisplayedTotal
        {
            get
            {
                return _ProgressDisplayedTotal;
            }
            set
            {
                if (_ProgressDisplayedTotal != value)
                {
                    _ProgressDisplayedTotal = value;

                    if (_ProgressDisplayedVisible && ProgressVisible)
                    {
                        OnRedrawRequired(false);
                    }
                }
            }
        }

        private bool _ShowNotification;
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
                    OnRedrawRequired(false);
                }
            }
        }

        public Size FavSize
        {
            get
            {
                return favorite.bounds.Size;
            }
            set
            {
                favorite.bounds.Size = value;
                if (favorite.visible)
                {
                    OnRedrawRequired(true);
                }
            }
        }

        public FavoriteVisibility FavVisibility
        {
            get
            {
                return favorite.visibility;
            }
            set
            {
                if (favorite.visibility != value)
                {
                    if (value == FavoriteVisibility.Never || favorite.visibility == FavoriteVisibility.Never)
                    {
                        OnRedrawRequired(true);
                    }

                    favorite.visibility = value;
                    OnFavStateChanged();
                }
            }
        }

        /// <summary>
        /// Fav icon can be interacted with
        /// </summary>
        public FavoriteAlignment FavAlignment
        {
            get
            {
                return favorite.alignment;
            }
            set
            {
                if (favorite.alignment != value)
                {
                    favorite.alignment = value;
                    if (favorite.visible)
                    {
                        OnRedrawRequired(true);
                    }
                }
            }
        }

        /// <summary>
        /// Fav icon can be interacted with
        /// </summary>
        public bool FavEnabled
        {
            get
            {
                return favorite.enabled;
            }
            set
            {
                favorite.enabled = value;
            }
        }

        /// <summary>
        /// Fav icon is filled
        /// </summary>
        public bool FavSelected
        {
            get
            {
                return favorite.selected;
            }
            set
            {
                if (favorite.selected != value)
                {
                    favorite.selected = value;
                    OnFavStateChanged();
                    if (favorite.visible)
                    {
                        OnRedrawRequired(false);
                    }
                }
            }
        }

        private void OnFavStateChanged()
        {
            var v = FavoriteVisibility.Always;

            if (favorite.selected)
            {
                v |= FavoriteVisibility.Selected;
            }

            if (hovered)
            {
                v |= FavoriteVisibility.Hovered;
            }

            var b = (favorite.visibility & v) != 0;

            if (favorite.visible != b)
            {
                favorite.visible = b;
                OnRedrawRequired(false);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            OnRedrawRequired(true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            hovered = true;

            if (_ProgressDisplayedVisible && _ProgressValue != 0 && _ProgressValue != 255)
            {
                OnRedrawRequired(false);
            }

            if ((favorite.visibility & FavoriteVisibility.Hovered) != 0)
            {
                OnFavStateChanged();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            hovered = false;

            if (_ProgressDisplayedVisible && _ProgressValue != 0 && _ProgressValue != 255)
            {
                OnRedrawRequired(false);
            }

            if ((favorite.visibility & FavoriteVisibility.Hovered) != 0)
            {
                OnFavStateChanged();
            }
        }

        protected void PerformLayout(Graphics g)
        {
            int w = this.Width,
                h = this.Height,
                lw, lx;

            if (icon.visible)
                lx = icon.bounds.Width + 20;
            else
                lx = 20;
            lw = w - lx - 10;

            if (favorite.alignment == FavoriteAlignment.Right && (favorite.visibility != FavoriteVisibility.Never || _ProgressValue == 255 || _ProgressDisplayedVisible))
            {
                lw -= favorite.bounds.Width + 5;
                favorite.bounds.Location = new Point(lx + lw + 5, h / 2 - favorite.bounds.Height / 2);
            }

            var sizeName = labelName.Measure(g, new Size(lw, h));

            if (labelDescription.visible)
            {
                labelName.bounds = new Rectangle(new Point(lx, 10), sizeName);
                icon.bounds.Location = new Point(10, 10);

                var gap = (int)(labelName.font.GetHeight(g) / 4 + 0.5f);
                var max = h - sizeName.Height - gap - 20;
                var ly = labelName.bounds.Bottom;

                if (labelLevel.visible)
                {
                    var sizeLevel = labelLevel.Measure(g, new Size(lw, max));
                    labelLevel.bounds = new Rectangle(icon.bounds.X + icon.bounds.Width / 2 - sizeLevel.Width / 2, icon.bounds.Bottom - sizeLevel.Height / 2, sizeLevel.Width, sizeLevel.Height);
                    //labelLevel.bounds = new Rectangle(new Point(lx, ly + gap / 2), sizeLevel);

                    //max -= gap / 2 + sizeLevel.Height;
                    //ly = labelLevel.bounds.Bottom;
                }

                var sizeDescription = labelDescription.Measure(g, new Size(lw, max));
                if (sizeDescription.Height > max)
                    sizeDescription.Height = max;
                labelDescription.bounds = new Rectangle(new Point(lx, ly + gap), sizeDescription);
            }
            else
            {
                labelName.bounds = new Rectangle(new Point(lx, h / 2 - sizeName.Height / 2), sizeName);
                labelDescription.bounds = Rectangle.Empty;
                icon.bounds.Location = new Point(10, h / 2 - icon.bounds.Height / 2);
            }

            if (favorite.alignment == FavoriteAlignment.Left && favorite.visibility != FavoriteVisibility.Never)
            {
                favorite.bounds.Location = new Point(w - favorite.bounds.Width - 5, labelName.bounds.Y);//icon.bounds.X + icon.bounds.Width / 2 - favorite.bounds.Width / 2, icon.bounds.Bottom - favorite.bounds.Height / 2);
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (proposedSize.Width > 0)
            {
                int w = proposedSize.Width,
                    h = proposedSize.Height;
                if (h == 0)
                    h = int.MaxValue;

                using (var g = this.CreateGraphics())
                {
                    var lx = icon.bounds.Width + 20;
                    var lw = w - lx - 10;

                    if (favorite.visibility != FavoriteVisibility.Never)
                    {
                        lw -= favorite.bounds.Width + 5;
                    }

                    var sizeName = labelName.Measure(g, new Size(lw, h));

                    if (labelDescription.visible)
                    {
                        var gap = (int)(labelName.font.GetHeight(g) / 4 + 0.5f);
                        var max = h - sizeName.Height - gap - 20;

                        //if (labelLevel.visible)
                        //{
                        //    var sizeLevel = labelLevel.Measure(g, new Size(lw, max));
                        //    max -= gap / 2 + sizeLevel.Height;
                        //}

                        var sizeDescription = labelDescription.Measure(g, new Size(lw, max));
                        if (sizeDescription.Height > max)
                            sizeDescription.Height = max;

                        h = sizeDescription.Height + sizeName.Height + gap;
                    }
                    else
                    {
                        h = sizeName.Height;
                    }

                    if (icon.bounds.Height > h)
                        h = icon.bounds.Height;

                    h += 20;

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

            return base.GetPreferredSize(proposedSize);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                var g = buffer.Graphics;

                if (resize)
                {
                    resize = false;
                    PerformLayout(g);
                }

                if (_ShowNotification)
                {
                    var sz = (int)(11 * g.DpiX / 96f + 0.5f);

                    var y = (int)(3 * g.DpiX / 96f + 0.5f);
                    var x = y;

                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    using (var b = new SolidBrush(Color.FromArgb(230, 0, 0)))
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

                if (_ProgressValue != 0)
                {
                    var w = this.Width;
                    var h = this.Height;
                    var pw = (int)(w * ProgressValue + 0.5f);

                    if (pw > 0)
                    {
                        if (_ProgressValue == 255)
                        {
                            //claimed - a checkmark is shown instead of progress

                            var scale = g.DpiX / 96f;
                            var px = (int)(scale + 0.5f);
                            var bounds = Rectangle.Inflate(favorite.bounds, px, px);
                            var colorCheck = Color.White;
                            var colorBackground = Color.FromArgb(59, 140, 39);

                            var x = bounds.X + px;
                            var y = bounds.Y - px;
                            w = bounds.Width;
                            h = bounds.Height;

                            var points = new PointF[]
                            {
                                new PointF(x + w * 0.933f, y),
                                new PointF(x + w, y + h * 0.167f),
                                new PointF(x + w * 0.467f, y + h),
                                new PointF(x, y + h * 0.583f),
                                new PointF(x + w * 0.2f, y + h * 0.417f),
                                new PointF(x + w * 0.4f, y + h * 0.667f),
                            };

                            using (var b = new SolidBrush(colorBackground))
                            {
                                using (var p = new Pen(b, px))
                                {
                                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                    var sz = bounds.Width >= bounds.Height ? bounds.Width : bounds.Height;

                                    g.FillEllipse(b, bounds.X, bounds.Y, sz, sz);

                                    if (colorCheck.A != 0)
                                    {

                                        b.Color = colorCheck;
                                        g.FillPolygon(b, points);
                                    }

                                    p.Color = Color.FromArgb(128, 0, 0, 0);
                                    g.DrawPolygon(p, points);

                                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                                }
                            }
                        }
                        else
                        {
                            //not claimed - show progress

                            using (var b = new SolidBrush(Color.FromArgb(15, 0, 255, 0)))
                            {
                                var scale = g.DpiX / 96f;
                                var bw = (int)(scale * 2 + 0.5f);

                                pw -= bw;

                                var padding = (int)(scale * 1 + 0.5f);

                                if (pw > 0)
                                {
                                    g.FillRectangle(b, 0, padding + bw, pw, h - padding * 2 - bw * 2); //background
                                }

                                b.Color = Color.FromArgb(25, 0, 255, 0);

                                g.FillRectangle(b, pw, padding, bw, h - padding * 2); //right
                                g.FillRectangle(b, 0, padding, pw, bw); //top
                                g.FillRectangle(b, 0, h - padding - bw, pw, bw); //bottom

                            }
                        }
                    }

                    if (hovered && _ProgressDisplayedVisible && _ProgressValue != 255)
                    {
                        using (var f1 = new Font("Segoe UI Semibold", 8f, FontStyle.Bold, GraphicsUnit.Point))
                        {
                            const TextFormatFlags tf = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

                            var s1 = _ProgressDisplayedValue.ToString();
                            var sz1 = TextRenderer.MeasureText(g, s1, f1, Size.Empty, tf);

                            if (_ProgressDisplayedTotal != 0)
                            {
                                using (var f2 = new Font("Segoe UI", 6f, FontStyle.Regular, GraphicsUnit.Point))
                                {
                                    var s2 = _ProgressDisplayedTotal.ToString();
                                    var sz2 = TextRenderer.MeasureText(g, s2, f2, Size.Empty, tf);
                                    var r1 = new Rectangle(favorite.bounds.Right - sz1.Width, (h - sz1.Height - sz2.Height / 2) / 2, sz1.Width, sz1.Height);
                                    var r2 = new Rectangle(favorite.bounds.Right - sz2.Width, r1.Bottom, sz2.Width, sz2.Height);

                                    TextRenderer.DrawText(g, s1, f1, r1, this.ForeColor, Color.Transparent, tf);
                                    TextRenderer.DrawText(g, s2, f2, r2, Util.Color.Gradient(this.ForeColor, this.BackColor, 0.4f), Color.Transparent, tf);
                                }
                            }
                            else
                            {
                                var r1 = new Rectangle(favorite.bounds.Right - sz1.Width, (h - sz1.Height) / 2, sz1.Width, sz1.Height);

                                TextRenderer.DrawText(g, s1, f1, r1, this.ForeColor, Color.Transparent, tf);
                            }
                        }
                    }
                }

                if (icon.visible)
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Undefined)
                    {
                        g.DrawImage(image, icon.bounds);
                    }
                }

                if (favorite.visible)
                {
                    favorite.Draw(g);
                }

                labelName.Draw(g);
                labelDescription.Draw(g);
                labelLevel.Draw(g);
            }

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);

                var g = buffer.Graphics;

                g.Clear(this.BackColor);

            }
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

        public override void RefreshColors()
        {
            labelLevel.foreColor = UiColors.GetColor(UiColors.Colors.DailiesTextLight);

            OnRedrawRequired(false);

            base.RefreshColors();
        }
    }
}
