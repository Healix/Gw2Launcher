using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Base
{
    class PopupFlatBase : FlatBase
    {
        private bool noActivation;
        private PopupUtil popup;

        /// <summary>
        /// Basic popup form (owner will remain active)
        /// </summary>
        /// <param name="noActivation">Prevents activating the form</param>
        public PopupFlatBase(Form owner, bool noActivation)
        {
            this.noActivation = noActivation;
            this.Owner = owner;

            popup = new PopupUtil(owner, this);
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return noActivation;
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            if (value)
                popup.Enabled = true;
            base.SetVisibleCore(value);
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_MOUSEACTIVATE:

                    if (noActivation)
                    {
                        m.Result = (IntPtr)3; //MA_NOACTIVATE

                        return;
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                popup.Dispose();
            base.Dispose(disposing);
        }
    }
}
