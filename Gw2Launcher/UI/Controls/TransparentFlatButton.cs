using Gw2Launcher.Windows.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    class TransparentFlatButton : FlatButton
    {
        [System.ComponentModel.DefaultValue(false)]
        public bool Transparent
        {
            get;
            set;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch ((Windows.Native.WindowMessages)m.Msg)
            {
                case Windows.Native.WindowMessages.WM_NCHITTEST:

                    if (Transparent)
                        m.Result = (IntPtr)HitTest.Transparent;

                    break;
            }
        }
    }
}
