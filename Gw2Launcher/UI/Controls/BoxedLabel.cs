using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Controls
{
    class BoxedLabel : Control
    {
        private const int EP_EDITBORDER_NOSCROLL = 6;
        private const int EBS_NORMAL = 1;
        private const int EBS_DISABLED = 4;
        private const int TMT_BORDERSIZE = 2403;

        private BufferedGraphics buffer;
        private bool redraw;
        private Rectangle rectContent;

        public BoxedLabel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, true);

            _BorderStyle = BorderStyle.Fixed3D;
            _TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            UseVisualStyleBackColor = true;

            BackColor = SystemColors.Window;
            ForeColor = SystemColors.WindowText;

            redraw = true;
        }

        private BorderStyle _BorderStyle;
        [DefaultValue(BorderStyle.Fixed3D)]
        public BorderStyle BorderStyle
        {
            get
            {
                return _BorderStyle;
            }
            set
            {
                if (_BorderStyle != value)
                {
                    _BorderStyle = value;
                    OnRedrawRequired();
                }
            }
        }

        private System.Drawing.ContentAlignment _TextAlign;
        [DefaultValue(System.Drawing.ContentAlignment.MiddleLeft)]
        public System.Drawing.ContentAlignment TextAlign
        {
            get
            {
                return _TextAlign;
            }
            set
            {
                if (_TextAlign != value)
                {
                    _TextAlign = value;
                    OnRedrawRequired();
                }
            }
        }

        [DefaultValue(true)]
        public bool UseVisualStyleBackColor
        {
            get;
            set;
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
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

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            OnRedrawRequired();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            OnRedrawRequired();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            OnRedrawRequired();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            OnRedrawRequired();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            OnRedrawRequired();
        }

        protected void OnRedrawRequired()
        {
            if (!redraw)
            {
                redraw = true;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle);

                OnPaintBackground(buffer.Graphics);
            }

            buffer.Render(e.Graphics);
        }

        protected virtual void OnPaintBackground(Graphics g)
        {
            int border;

            if (Application.RenderWithVisualStyles && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsSupported)
            {
                IntPtr theme,
                       hdc = IntPtr.Zero;

                try
                {
                    theme = NativeMethods.OpenThemeData(this.Handle, VisualStyleElement.TextBox.TextEdit.Normal.ClassName);
                }
                catch
                {
                    theme = IntPtr.Zero;
                }

                if (theme != IntPtr.Zero)
                {
                    try
                    {
                        hdc = g.GetHdc();

                        var r = new RECT()
                        {
                            right = this.Width,
                            bottom = this.Height,
                        };

                        var state = this.Enabled ? EBS_NORMAL : EBS_DISABLED;

                        NativeMethods.DrawThemeBackground(theme, hdc, EP_EDITBORDER_NOSCROLL, state, ref r, IntPtr.Zero);
                        NativeMethods.GetThemeMetric(theme, hdc, EP_EDITBORDER_NOSCROLL, state, TMT_BORDERSIZE, out border);
                    }
                    finally
                    {
                        if (theme != IntPtr.Zero)
                            NativeMethods.CloseThemeData(theme);
                        if (hdc != IntPtr.Zero)
                            g.ReleaseHdc(hdc);
                    }

                    if (!UseVisualStyleBackColor || !this.Enabled)
                    {
                        using (var b = new SolidBrush(this.Enabled ? this.BackColor : SystemColors.Control))
                        {
                            g.FillRectangle(b, border, border, this.Width - border * 2, this.Height - border * 2);
                        }
                    }
                }
                else
                {
                    border = 0;
                    g.Clear(this.BackColor);
                }
            }
            else
            {
                g.Clear(this.BackColor);

                switch (BorderStyle)
                {
                    case BorderStyle.Fixed3D:

                        ControlPaint.DrawBorder3D(g, this.DisplayRectangle, Border3DStyle.Sunken);
                        border = NativeMethods.GetSystemMetrics(Gw2Launcher.Windows.Native.SystemMetric.SM_CXEDGE);

                        break;
                    case System.Windows.Forms.BorderStyle.FixedSingle:

                        ControlPaint.DrawBorder3D(g, this.DisplayRectangle, Border3DStyle.Flat);
                        border = NativeMethods.GetSystemMetrics(Gw2Launcher.Windows.Native.SystemMetric.SM_CXBORDER);

                        break;
                    default:

                        border = 0;

                        break;
                }
            }

            rectContent = new Rectangle(this.Padding.Left + border, this.Padding.Top + border, this.Width - this.Padding.Horizontal - border * 2, this.Height - this.Padding.Vertical - border * 2);

            var flags = Util.Text.GetAlignmentFlags(_TextAlign) | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;

            TextRenderer.DrawText(g, this.Text, this.Font, rectContent, this.Enabled ? this.ForeColor : Color.FromArgb(160, 160, 160), Color.Transparent, flags);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
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
