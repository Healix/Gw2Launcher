using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Design;

namespace Gw2Launcher.UI.Controls
{
    class FlatButton : Base.BaseControl
    {
        public event EventHandler SelectedChanged;

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

        [DefaultValue(false)]
        public bool Selected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    //if (value)
                    //    isHovered = false;
                    OnRedrawRequired();

                    if (SelectedChanged != null)
                        SelectedChanged(this, EventArgs.Empty);
                }
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
        [DefaultValue(typeof(Color), "")]
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

        protected UiColors.Colors _BorderColorName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors BorderColorName
        {
            get
            {
                return _BorderColorName;
            }
            set
            {
                if (_BorderColorName != value)
                {
                    _BorderColorName = value;
                    RefreshColors();
                }
            }
        }

        protected Color borderColorHovered;
        [DefaultValue(typeof(Color), "")]
        public Color BorderColorHovered
        {
            get
            {
                if (borderColorHovered.IsEmpty && !DesignMode)
                    return borderColor;
                return borderColorHovered;
            }
            set
            {
                if (borderColorHovered != value)
                {
                    borderColorHovered = value;
                    if (isHovered)
                        OnRedrawRequired();
                }
            }
        }

        protected UiColors.Colors _BorderColorHoveredName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors BorderColorHoveredName
        {
            get
            {
                return _BorderColorHoveredName;
            }
            set
            {
                if (_BorderColorHoveredName != value)
                {
                    _BorderColorHoveredName = value;
                    RefreshColors();
                }
            }
        }

        protected Color borderColorSelected;
        [DefaultValue(typeof(Color), "")]
        public Color BorderColorSelected
        {
            get
            {
                if (borderColorSelected.IsEmpty && !DesignMode)
                    return borderColor;
                return borderColorSelected;
            }
            set
            {
                if (borderColorSelected != value)
                {
                    borderColorSelected = value;
                    if (isSelected)
                        OnRedrawRequired();
                }
            }
        }

        protected UiColors.Colors _BorderColorSelectedName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors BorderColorSelectedName
        {
            get
            {
                return _BorderColorSelectedName;
            }
            set
            {
                if (_BorderColorSelectedName != value)
                {
                    _BorderColorSelectedName = value;
                    RefreshColors();
                }
            }
        }

        protected Color backColorHovered;
        [DefaultValue(typeof(Color), "")]
        public Color BackColorHovered
        {
            get
            {
                if (backColorHovered.IsEmpty && !DesignMode)
                    return this.BackColor;
                return backColorHovered;
            }
            set
            {
                if (backColorHovered != value)
                {
                    backColorHovered = value;
                    if (isHovered)
                        OnRedrawRequired();
                }
            }
        }

        protected UiColors.Colors _BackColorHoveredName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors BackColorHoveredName
        {
            get
            {
                return _BackColorHoveredName;
            }
            set
            {
                if (_BackColorHoveredName != value)
                {
                    _BackColorHoveredName = value;
                    RefreshColors();
                }
            }
        }

        protected Color foreColorHovered;
        [DefaultValue(typeof(Color), "")]
        public Color ForeColorHovered
        {
            get
            {
                if (foreColorHovered.IsEmpty && !DesignMode)
                    return this.ForeColor;
                return foreColorHovered;
            }
            set
            {
                if (foreColorHovered != value)
                {
                    foreColorHovered = value;
                    if (isHovered)
                        OnRedrawRequired();
                }
            }
        }

        protected UiColors.Colors _ForeColorHoveredName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
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

        protected Color backColorSelected;
        [DefaultValue(typeof(Color), "")]
        public Color BackColorSelected
        {
            get
            {
                if (backColorSelected.IsEmpty && !DesignMode)
                    return this.BackColor;
                return backColorSelected;
            }
            set
            {
                if (backColorSelected != value)
                {
                    backColorSelected = value;
                    if (isSelected)
                        OnRedrawRequired();
                }
            }
        }

        protected UiColors.Colors _BackColorSelectedName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors BackColorSelectedName
        {
            get
            {
                return _BackColorSelectedName;
            }
            set
            {
                if (_BackColorSelectedName != value)
                {
                    _BackColorSelectedName = value;
                    RefreshColors();
                }
            }
        }

        protected Color foreColorSelected;
        [DefaultValue(typeof(Color), "")]
        public Color ForeColorSelected
        {
            get
            {
                if (foreColorSelected.IsEmpty && !DesignMode)
                    return this.ForeColor;
                return foreColorSelected;
            }
            set
            {
                if (foreColorSelected != value)
                {
                    foreColorSelected = value;
                    if (isSelected)
                        OnRedrawRequired();
                }
            }
        }

        protected UiColors.Colors _ForeColorSelectedName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors ForeColorSelectedName
        {
            get
            {
                return _ForeColorSelectedName;
            }
            set
            {
                if (_ForeColorSelectedName != value)
                {
                    _ForeColorSelectedName = value;
                    RefreshColors();
                }
            }
        }

        public Color BackColorCurrent
        {
            get
            {
                if (isHovered)
                {
                    if (isSelected && PrioritizeSelectedColoring)
                        return this.BackColorSelected;
                    return this.BackColorHovered;
                }
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
                {
                    if (isSelected && PrioritizeSelectedColoring)
                        return this.ForeColorSelected;
                    return this.ForeColorHovered;
                }
                else if (isSelected)
                    return this.ForeColorSelected;
                else
                    return this.ForeColor;
            }
        }

        public Color BorderColorCurrent
        {
            get
            {
                if (isHovered)
                {
                    if (isSelected && PrioritizeSelectedColoring)
                        return this.BorderColorSelected;
                    return this.BorderColorHovered;
                }
                else if (isSelected)
                    return this.BorderColorSelected;
                else
                    return this.BorderColor;
            }
        }

        /// <summary>
        /// By default, hovering will take priority over being selected. If true, selected will take priority.
        /// </summary>
        [DefaultValue(false)]
        public bool PrioritizeSelectedColoring
        {
            get;
            set;
        }

        protected AnchorStyles borderStyle;
        [DefaultValue(AnchorStyles.None)]
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

        protected HorizontalAlignment alignment = HorizontalAlignment.Left;
        [DefaultValue(HorizontalAlignment.Left)]
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

        private bool _NoWrap;
        [DefaultValue(false)]
        public bool NoWrap
        {
            get
            {
                return _NoWrap;
            }
            set
            {
                if (_NoWrap != value)
                {
                    _NoWrap = value;
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

            if (_NoWrap)
                f |= TextFormatFlags.SingleLine;

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
                    h = (int)(h * r + 0.5f);
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

            if (borderStyle != AnchorStyles.None)
            {
                var borderColor = this.BorderColorCurrent;
                if (borderColor.A > 0)
                {
                    var pw = (int)(g.DpiX / 96f + 0.5f);

                    using (var p = new Pen(borderColor, pw))
                    {
                        int x = pw / 2,
                            y = pw / 2,
                            w = this.Width - pw,
                            h = this.Height - pw;

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

        public override void RefreshColors()
        {
            base.RefreshColors();

            if (_ForeColorSelectedName != UiColors.Colors.Custom)
                this.ForeColorSelected = UiColors.GetColor(_ForeColorSelectedName);
            if (_BackColorSelectedName != UiColors.Colors.Custom)
                this.BackColorSelected = UiColors.GetColor(_BackColorSelectedName);
            if (_ForeColorHoveredName != UiColors.Colors.Custom)
                this.ForeColorHovered = UiColors.GetColor(_ForeColorHoveredName);
            if (_BackColorHoveredName != UiColors.Colors.Custom)
                this.BackColorHovered = UiColors.GetColor(_BackColorHoveredName);
            if (_BorderColorSelectedName != UiColors.Colors.Custom)
                this.BorderColorSelected = UiColors.GetColor(_BorderColorSelectedName);
            if (_BorderColorHoveredName != UiColors.Colors.Custom)
                this.BorderColorHovered = UiColors.GetColor(_BorderColorHoveredName);
            if (_BorderColorName != UiColors.Colors.Custom)
                this.BorderColor = UiColors.GetColor(_BorderColorName);
        }
    }
}
