using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Gw2Launcher.UI.Controls
{
    class ApplyAllCheckBox : Control
    {
        private BufferedGraphics buffer;
        private bool redraw;
        private ToolTip tooltip;
        private bool hovered;

        public ApplyAllCheckBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            redraw = true;

            tooltip = new ToolTip();
            tooltip.InitialDelay = 100;
            tooltip.SetToolTip(this, "Apply to all accounts");

            DefaultState = true;

            this.Cursor = Windows.Cursors.Hand;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        private void OnRedrawRequired()
        {
            if (redraw)
                return;
            redraw = true;
            this.Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            OnRedrawRequired();
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

        private bool _Checked;
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                if (_Checked != value)
                {
                    _Checked = value;
                    OnRedrawRequired();
                }
            }
        }

        private bool _Clicked;
        [Browsable(false)]
        [DefaultValue(false)]
        public bool Clicked
        {
            get
            {
                return _Clicked;
            }
            set
            {
                _Clicked = value;
            }
        }

        [DefaultValue(true)]
        public bool DefaultState
        {
            get;
            set;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            hovered = true;
            OnRedrawRequired();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hovered = false;
            OnRedrawRequired();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            this.Checked = !_Checked;
            _Clicked = true;
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            OnRedrawRequired();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                var g = buffer.Graphics;
                var sz = (this.Height - this.Padding.Vertical) * 3 / 4;
                var color = this.Enabled ? this.ForeColor : SystemColors.GrayText;
                Color c = hovered ? SystemColors.Highlight : Color.FromArgb(90, 90, 90);
                var c1 = Color.FromArgb(60, 60, 60);

                g.Clear(this.BackColor);

                using (var brush = new SolidBrush(this.Enabled ? c : SystemColors.GrayText))
                {
                    var scale = g.DpiX / 96f;
                    var pw = 1;

                    using (var pen = new Pen(this.Enabled ? c : SystemColors.GrayText, pw))
                    {
                        var x = Padding.Left;
                        var y = Padding.Top;
                        var sz2 = sz / 2;

                        for (var i = 0; i < 2; i++)
                        {
                            g.FillRectangle(Brushes.White, x, y, sz, sz);
                            g.DrawRectangle(pen, pw / 2 + x, pw / 2 + y, sz - pw, sz - pw);
                            if (_Checked)
                            {
                                var b = this.Enabled & !hovered;
                                if (b)
                                    brush.Color = c1;
                                var gap = pw * 2;
                                g.FillRectangle(brush, x + gap, y + gap, sz - gap * 2, sz - gap * 2);
                                if (b)
                                    brush.Color = c;
                            }
                            x += sz2;
                            y += sz2;
                        }
                    }
                }
            }

            buffer.Render(e.Graphics);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (buffer != null)
                    buffer.Dispose();
            }
        }
    }
}
