using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    class FlatTextBox : Base.BaseControl
    {
        public FlatTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.TextBox = CreateTextBox();

            this.Cursor = Cursors.IBeam;
            this._BorderColor = Color.Black;

            this.Controls.Add(this.TextBox);

            this.TextBox.GotFocus += OnFocusChanged;
            this.TextBox.LostFocus += OnFocusChanged;
            this.TextBox.TextChanged += TextBox_TextChanged;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            SetTextBoxBounds();
        }

        void TextBox_TextChanged(object sender, EventArgs e)
        {
            OnTextChanged(e);
        }

        protected virtual TextBox CreateTextBox()
        {
            return new TextBox()
            {
                BackColor = this.BackColor,
                ForeColor = this.ForeColor,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(3),
            };
        }

        public TextBox TextBox
        {
            get;
            private set;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(this.TextBox.Width + this.TextBox.Margin.Horizontal, this.TextBox.Height + this.TextBox.Margin.Vertical);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            this.TextBox.BackColor = this.BackColor;
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            this.TextBox.ForeColor = this.ForeColor;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (IsHandleCreated)
            {
                SetTextBoxBounds();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (IsHandleCreated)
            {
                SetTextBoxBounds();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            this.TextBox.Focus();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            this.TextBox.Focus();
        }

        private void SetTextBoxBounds()
        {
            var t = this.TextBox;

            if (t.Multiline)
            {
                t.Bounds = new Rectangle(t.Margin.Left, t.Margin.Top, this.Width - t.Margin.Horizontal, this.Height - t.Margin.Vertical);
            }
            else
            {
                var b = new Rectangle(t.Margin.Left, (this.Height - t.Height) / 2, this.Width - t.Margin.Horizontal, t.Height);

                t.Bounds = b;

                this.Height = t.Height + t.Margin.Vertical;
            }
        }

        [DefaultValue(typeof(Cursor), "IBeam")]
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

        [DefaultValue("")]
        public override string Text
        {
            get
            {
                return this.TextBox.Text;
            }
            set
            {
                this.TextBox.Text = value;
            }
        }

        public int TextLength
        {
            get
            {
                return this.TextBox.TextLength;
            }
        }

        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign
        {
            get
            {
                return this.TextBox.TextAlign;
            }
            set
            {
                this.TextBox.TextAlign = value;
            }
        }

        public void Select(int start, int length)
        {
            this.TextBox.Select(start, length);
        }

        public Color BorderColorCurrent
        {
            get
            {
                if (TextBox.Focused && !BorderColorFocused.IsEmpty)
                    return BorderColorFocused;
                return BorderColor;
            }
        }

        private Color _BorderColor;
        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColor
        {
            get
            {
                return _BorderColor;
            }
            set
            {
                if (_BorderColor != value)
                {
                    _BorderColor = value;
                    this.Invalidate();
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

        private Color _BorderColorFocused;
        [DefaultValue(typeof(Color), "")]
        public Color BorderColorFocused
        {
            get
            {
                return _BorderColorFocused;
            }
            set
            {
                if (_BorderColorFocused != value)
                {
                    _BorderColorFocused = value;
                    this.Invalidate();
                }
            }
        }
        protected UiColors.Colors _BorderColorFocusedName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors BorderColorFocusedName
        {
            get
            {
                return _BorderColorFocusedName;
            }
            set
            {
                if (_BorderColorFocusedName != value)
                {
                    _BorderColorFocusedName = value;
                    RefreshColors();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var pw = (int)(e.Graphics.DpiX / 96f + 0.5f);
            var phalf = pw / 2;

            using (var p = new Pen(BorderColorCurrent, pw))
            {
                e.Graphics.DrawRectangle(p, phalf, phalf, this.Width - pw, this.Height - pw);
            }
        }

        void OnFocusChanged(object sender, EventArgs e)
        {
            if (_BorderColorFocused.A > 0 && _BorderColorFocused != _BorderColor)
                this.Invalidate();
        }

        public override void RefreshColors()
        {
            if (_BorderColorName != UiColors.Colors.Custom)
                BorderColor = UiColors.GetColor(_BorderColorName);
            if (_BorderColorFocusedName != UiColors.Colors.Custom)
                BorderColorFocused = UiColors.GetColor(_BorderColorFocusedName);

            base.RefreshColors();
        }
    }
}
