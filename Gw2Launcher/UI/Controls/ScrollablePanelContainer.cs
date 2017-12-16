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
    public partial class ScrollablePanelContainer : UserControl
    {
        private Panel content;

        public ScrollablePanelContainer()
        {
            InitializeComponent();

            panelContainer.MouseWheel += panel_MouseWheel;
        }

        public void SetContent(Panel panel)
        {
            this.content = panel;

            panel.MouseWheel += panel_MouseWheel;
            panel.Location = new Point(0, 0);
            panelContainer.Controls.Add(panel);

            int h = panel.Height - panelContainer.Height;
            if (h <= 0)
            {
                scrollV.Visible = false;
            }
            else
            {
                scrollV.Visible = true;
                scrollV.Maximum = h;
            }
        }

        void panel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                scrollV.Value -= this.Height / 10;
            }
            else if (e.Delta < 0)
            {
                scrollV.Value += this.Height / 10;
            }

            ((HandledMouseEventArgs)e).Handled = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.DrawRectangle(SystemPens.WindowFrame, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void scrollV_ValueChanged(object sender, int e)
        {
            this.content.Location = new Point(0, -e);
        }
    }
}
