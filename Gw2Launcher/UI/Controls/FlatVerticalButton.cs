using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class FlatVerticalButton : FlatButton
    {
        protected Bitmap bufferText;

        public FlatVerticalButton()
            : base()
        {
        }

        protected override void OnPaintBuffer(Graphics g)
        {
            var space = (int)(10 * g.DpiX / 96f + 0.5f);
            var size = TextRenderer.MeasureText(g, this.Text, this.Font, new Size(this.Height - this.Padding.Vertical, this.Width - this.Padding.Horizontal - space), TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

            if (bufferText == null || bufferText.Width != size.Width || bufferText.Width != size.Height)
            {
                if (bufferText != null)
                    bufferText.Dispose();
                bufferText = new Bitmap(size.Width, size.Height, g);
            }

            using (var gb = Graphics.FromImage(bufferText))
            {
                TextRenderer.DrawText(gb, this.Text, this.Font, new Rectangle(Point.Empty, size), ForeColorCurrent, BackColorCurrent, TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
            }

            //g.Clear(this.BackColor);
            g.TranslateTransform((this.Width + size.Height) / 2 + Padding.Left, space + Padding.Top);
            g.RotateTransform(90);
            g.DrawImage(bufferText, new Point(0, 0));
            g.ResetTransform();
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
