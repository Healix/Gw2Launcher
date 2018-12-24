using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.UI.Controls
{
    class ToolStripMenuItemStayOpenOnClick : System.Windows.Forms.ToolStripMenuItem
    {
        private bool disableClose;
        private bool hasDrop;

        public ToolStripMenuItemStayOpenOnClick()
            : base()
        {
        }

        public ToolStripMenuItemStayOpenOnClick(string text, System.Drawing.Image image, EventHandler onClick)
            : base(text, image, onClick)
        {
        }

        public bool StayOpenOnClick
        {
            get;
            set;
        }

        protected override void OnDropDownItemClicked(System.Windows.Forms.ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ToolStripMenuItemStayOpenOnClick)
                disableClose = ((ToolStripMenuItemStayOpenOnClick)e.ClickedItem).StayOpenOnClick;
            
            base.OnDropDownItemClicked(e);
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);
            
            if (!hasDrop)
            {
                hasDrop = true;
                this.DropDown.Closing += DropDown_Closing;
            }
        }

        void DropDown_Closing(object sender, System.Windows.Forms.ToolStripDropDownClosingEventArgs e)
        {
            if (disableClose)
            {
                disableClose = false;
                if (e.CloseReason == System.Windows.Forms.ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            }
        }
    }
}
