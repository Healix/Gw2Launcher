using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class AutoScrollContainerPanel : Panel
    {
        public event EventHandler<bool> PreVisiblePropertyChanged;

        private bool visible;

        public AutoScrollContainerPanel(Panel panel)
            : base()
        {
            this.Bounds = panel.Bounds;
            this.Anchor = panel.Anchor;
            this.AutoScroll = panel.AutoScroll;
            this.Visible = panel.Visible;

            panel.Anchor = panel.Anchor & ~AnchorStyles.Bottom;
            panel.AutoScroll = false;
            panel.AutoScrollMargin = Size.Empty;
            panel.Bounds = new Rectangle(0, 0, 0, 0);
            panel.AutoSize = true;
            panel.Visible = true;

            if (panel.Height > this.Height)
                this.AutoScrollMargin = panel.AutoScrollMargin;

            this.Controls.Add(panel);
        }

        protected override void SetVisibleCore(bool value)
        {
            if (visible != value)
            {
                visible = value;
                if (PreVisiblePropertyChanged != null)
                    PreVisiblePropertyChanged(this, value);
            }

            base.SetVisibleCore(value);
        }

        public new bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
