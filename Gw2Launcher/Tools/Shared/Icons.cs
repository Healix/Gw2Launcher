using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Gw2Launcher.Tools.Shared
{
    class Icons : Values<Icon>
    {
        public Icons(bool removeOnRelease = false)
            : base(removeOnRelease)
        {

        }
    }
}
