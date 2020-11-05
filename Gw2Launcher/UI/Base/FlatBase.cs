using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

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

            Windows.WindowShadow.Enable(this.Handle);
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

            if (visible)
            {
                using (var p = new Pen(Color.FromArgb(200, 200, 200), psize))
                {
                    var bounds = new Rectangle(psize, psize + (int)(10 * scale + 0.5f), this.ClientSize.Width - psize * 2, (int)(35 * scale + 0.5f));
                    int y;

                    using (var b = new SolidBrush(Color.FromArgb(235, 235, 235)))
                    {
                        g.FillRectangle(b, bounds);
                    }

                    TextRenderer.DrawText(g, this.Text, fontTitle, bounds, this.ForeColor, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                    y = bounds.Top + phalf - psize;
                    g.DrawLine(p, bounds.Left, y, bounds.Right, y);

                    y = bounds.Bottom + phalf;
                    g.DrawLine(p, bounds.Left, y, bounds.Right, y);
                }
            }

            using (var p = new Pen(Color.FromArgb(120, 120, 120), psize))
            {
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
    }
}
