using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    class EnabledEvent
    {
        public event EventHandler<bool> EnabledChanged;

        private bool enabled;

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    if (EnabledChanged != null)
                        EnabledChanged(this, value);
                }
            }
        }
    }
}
