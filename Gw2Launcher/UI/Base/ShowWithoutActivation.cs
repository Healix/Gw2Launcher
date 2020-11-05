using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI.Base
{
    public class ShowWithoutActivationForm : Base.BaseForm
    {
        public ShowWithoutActivationForm()
        {
            InitializeComponents();
        }

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

        protected override bool AutoScalingEnabled
        {
            get
            {
                return false;
            }
        }
    }
}
