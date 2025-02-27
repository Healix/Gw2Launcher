using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Design;

namespace Gw2Launcher.UI.Base
{
    public class FlatBase : StackFormBase
    {
        private BufferedGraphics buffer;
        private bool redraw;
        private Font fontTitle;

        public FlatBase()
        {
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(248, 248, 248);
            ShowInTaskbar = false;
            Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            fontTitle = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);

            redraw = true;
        }

        private void OnRedrawRequired()
        {
            if (redraw)
                return;
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

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode)
            {
                this.Opacity = 0;
                if (!Settings.StyleDisableWindowShadows.Value)
                    Windows.WindowShadow.Enable(this.Handle);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!DesignMode)
            {
                this.Refresh();
                this.Opacity = 1;
            }
        }

        /// <returns>True if handled</returns>
        protected virtual bool OnEscapePressed()
        {
            if (CloseOnEscape)
            {
                this.Close();
                return true;
            }

            return false;
        }

        protected virtual bool CloseOnEscape
        {
            get
            {
                return false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                if (OnEscapePressed())
                {
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected virtual void OnPaintBackground(Graphics g)
        {
            var text = this.Text;
            var visible = !string.IsNullOrEmpty(text);

            g.Clear(this.BackColor);

            var scale = GetScaling();
            var psize = (int)(scale + 0.5f);
            var phalf = psize / 2;

            using (var p = new Pen(visible ? this.TitleBorderColor : this.BorderColor, psize))
            {
                if (visible)
                {
                    var bounds = new Rectangle(psize, psize + (int)(10 * scale + 0.5f), this.ClientSize.Width - psize * 2, (int)(35 * scale + 0.5f));
                    int y;

                    using (var b = new SolidBrush(this.TitleBackColor))
                    {
                        g.FillRectangle(b, bounds);
                    }

                    TextRenderer.DrawText(g, this.Text, fontTitle, bounds, this.TitleForeColor, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                    y = bounds.Top + phalf - psize;
                    g.DrawLine(p, bounds.Left, y, bounds.Right, y);

                    y = bounds.Bottom + phalf;
                    g.DrawLine(p, bounds.Left, y, bounds.Right, y);

                    //border last
                    p.Color = this.BorderColor;
                }

                g.DrawRectangle(p, phalf, phalf, this.ClientSize.Width - psize, this.ClientSize.Height - psize);
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (buffer != null)
                    buffer.Dispose();
            }
        }

        protected Color _TitleForeColor = Color.Black;
        [DefaultValue(typeof(Color), "Black")]
        public virtual Color TitleForeColor
        {
            get
            {
                return _TitleForeColor;
            }
            set
            {
                if (_TitleForeColor != value)
                {
                    _TitleForeColor = value;
                    this.Invalidate();
                }
            }
        }

        protected UiColors.Colors _TitleForeColorName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors TitleForeColorName
        {
            get
            {
                return _TitleForeColorName;
            }
            set
            {
                if (_TitleForeColorName != value)
                {
                    _TitleForeColorName = value;
                    RefreshColors();
                }
            }
        }

        protected Color _TitleBackColor = Color.FromArgb(235, 235, 235);
        [DefaultValue(typeof(Color), "235,235,235")]
        public virtual Color TitleBackColor
        {
            get
            {
                return _TitleBackColor;
            }
            set
            {
                if (_TitleBackColor != value)
                {
                    _TitleBackColor = value;
                    this.Invalidate();
                }
            }
        }

        protected UiColors.Colors _TitleBackColorName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors TitleBackColorName
        {
            get
            {
                return _TitleBackColorName;
            }
            set
            {
                if (_TitleBackColorName != value)
                {
                    _TitleBackColorName = value;
                    RefreshColors();
                }
            }
        }

        protected Color _TitleBorderColor = Color.FromArgb(200, 200, 200);
        [DefaultValue(typeof(Color), "200,200,200")]
        public virtual Color TitleBorderColor
        {
            get
            {
                return _TitleBorderColor;
            }
            set
            {
                if (_TitleBorderColor != value)
                {
                    _TitleBorderColor = value;
                    this.Invalidate();
                }
            }
        }

        protected UiColors.Colors _TitleBorderColorName = UiColors.Colors.Custom;
        [DefaultValue(UiColors.Colors.Custom)]
        [UiPropertyColor()]
        [TypeConverter(typeof(UiColorTypeConverter))]
        [Editor(typeof(UiColorTypeEditor), typeof(UITypeEditor))]
        public UiColors.Colors TitleBorderColorName
        {
            get
            {
                return _TitleBorderColorName;
            }
            set
            {
                if (_TitleBorderColorName != value)
                {
                    _TitleBorderColorName = value;
                    RefreshColors();
                }
            }
        }

        protected Color _BorderColor = Color.FromArgb(120, 120, 120);
        [DefaultValue(typeof(Color), "120,120,120")]
        public virtual Color BorderColor
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

        public override void RefreshColors()
        {
            if (_BorderColorName != UiColors.Colors.Custom)
                this.BorderColor = UiColors.GetColor(_BorderColorName);
            if (_TitleBackColorName != UiColors.Colors.Custom)
                this.TitleBackColor = UiColors.GetColor(_TitleBackColorName);
            if (_TitleBorderColorName != UiColors.Colors.Custom)
                this.TitleBorderColor = UiColors.GetColor(_TitleBorderColorName);
            if (_TitleForeColorName != UiColors.Colors.Custom)
                this.TitleForeColor = UiColors.GetColor(_TitleForeColorName);

            base.RefreshColors();
        }
    }
}
