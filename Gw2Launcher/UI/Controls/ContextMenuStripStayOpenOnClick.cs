using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    class ContextMenuStripStayOpenOnClick : ContextMenuStrip
    {
        private bool disableClose;

        public ContextMenuStripStayOpenOnClick()
            : base()
        {

        }

        public ContextMenuStripStayOpenOnClick(System.ComponentModel.IContainer container)
            : base(container)
        {

        }

        public bool StayOpenOnClick
        {
            get;
            set;
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            if (disableClose)
            {
                disableClose = false;
                if (e.CloseReason == System.Windows.Forms.ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            }

            base.OnClosing(e);
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ToolStripMenuItemStayOpenOnClick)
                disableClose = ((ToolStripMenuItemStayOpenOnClick)e.ClickedItem).StayOpenOnClick;

            base.OnItemClicked(e);
        }
    }
}
