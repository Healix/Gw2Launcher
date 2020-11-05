using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class LabelButton : Label
    {
        public LabelButton()
            : base()
        {
            this.ForeColor = Color.FromArgb(49, 121, 242);
            this.Cursor = Cursors.Hand;
            this.Padding = new Padding(6, 0, 6, 0);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);

            var w = this.Width - 1;
            var h = this.Height - 1;

            var g = pevent.Graphics;

            var m = g.MeasureString("]", this.Font);
            var y = h / 2 - m.Height / 2;
            g.DrawString("[", this.Font, Brushes.Black, 0, y);
            g.DrawString("]", this.Font, Brushes.Black, w - m.Width, y);

            //pevent.Graphics.DrawLine(Pens.Black, 0, 0, 1, 0);
            //pevent.Graphics.DrawLine(Pens.Black, 0, h, 1, h);
            //pevent.Graphics.DrawLine(Pens.Black, 0, 1, 0, h - 1);

            //pevent.Graphics.DrawLine(Pens.Black, w - 1, 0, w, 0);
            //pevent.Graphics.DrawLine(Pens.Black, w - 1, h, w, h);
            //pevent.Graphics.DrawLine(Pens.Black, w, 1, w, h - 1);
        }
    }
}
