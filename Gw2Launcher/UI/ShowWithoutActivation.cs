using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    public class ShowWithoutActivationForm : Form
    {
        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (int)(WindowStyle.WS_EX_TOPMOST | WindowStyle.WS_EX_TOOLWINDOW | WindowStyle.WS_EX_NOACTIVATE);
                return createParams;
            }
        }
    }
}
