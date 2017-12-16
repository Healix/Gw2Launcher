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
        private Pen penBorder;

        public SidebarPanel():base()
        {
            colorBorder = SystemColors.WindowFrame;
            penBorder = new Pen(Color.Black);

            base.Disposed += SidebarPanel_Disposed;
        }

        void SidebarPanel_Disposed(object sender, EventArgs e)
        {
            penBorder.Dispose();
        }

        public Color BorderColor
        {
            get
            {
                return penBorder.Color;
            }
            set
            {
                penBorder.Color = value;
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;
            g.DrawLine(penBorder, this.Width - 1, 0, this.Width - 1, this.Height - 1);
        }
    }
}
