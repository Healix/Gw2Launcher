using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI
{
    class formProgressBar : Base.FlatBase
    {
        public formProgressBar()
        {
            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            this.Text = "";
            this.StartPosition = FormStartPosition.Manual;

            this.ProgressBar = new Controls.FlatProgressBar()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                Animated = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.LightSteelBlue,
                Margin = new Padding(5),
            };

            this.Controls.Add(this.ProgressBar);
            this.Size = new System.Drawing.Size(300, 50);
        }

        public Controls.FlatProgressBar ProgressBar
        {
            get;
            private set;
        }

        public long Maximum
        {
            get
            {
                return ProgressBar.Maximum;
            }
            set
            {
                ProgressBar.Maximum = value;
            }
        }

        public long Value
        {
            get
            {
                return ProgressBar.Value;
            }
            set
            {
                ProgressBar.Value = value;
            }
        }

        public void CenterAt(Control c)
        {
            var l = c.PointToScreen(Point.Empty);
            this.Location = new Point(l.X + c.Width / 2 - this.Width / 2, l.Y + c.Height / 2 - this.Height / 2);
        }
    }
}
