using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class SidebarPanel : Panel
    {
        private Color colorBorder;

        public SidebarPanel():base()
        {
            colorBorder = SystemColors.WindowFrame;
        }

        public Color BorderColor
        {
            get
            {
                return colorBorder;
            }
            set
            {
                colorBorder = value;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;
            g.DrawLine(new Pen(new SolidBrush(colorBorder)), this.Width - 1, 0, this.Width - 1, this.Height - 1);
        }
    }
}
