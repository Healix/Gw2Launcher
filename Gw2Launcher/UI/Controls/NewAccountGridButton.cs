using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    public class NewAccountGridButton : AccountGridButton
    {
        protected static readonly Font FONT_REGULAR = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

        private const string TEXT_ADD_ACCOUNT = "add account";

        public NewAccountGridButton()
        {
        }

        protected override void ResizeLabels(Graphics g)
        {
            base.ResizeLabels(g);

            var size = TextRenderer.MeasureText(g, TEXT_ADD_ACCOUNT, FONT_REGULAR, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            rectName = new Rectangle(new Point(this.Width / 2 - size.Width / 2, this.Height / 2 - size.Height / 2), size);
        }

        protected override void OnSizeChanged()
        {
            rectName = new Rectangle(new Point(this.Width / 2 - rectName.Width / 2, this.Height / 2 - rectName.Height / 2), rectName.Size);
            redraw = true;
            this.Invalidate();
        }

        protected override void OnPrePaint(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                TextRenderer.DrawText(buffer.Graphics, TEXT_ADD_ACCOUNT, FONT_REGULAR, rectName, UiColors.GetColor(UiColors.Colors.Link), Color.Transparent, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }
        }
    }
}
