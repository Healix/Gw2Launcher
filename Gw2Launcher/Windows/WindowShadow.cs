using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class WindowShadow
    {
        public static bool Enable(IntPtr handle)
        {
            if (!NativeMethods.IsDwmCompositionEnabled())
                return false;

            var value = (int)DWMNCRENDERINGPOLICY.DWMNCRP_ENABLED;
            if (NativeMethods.DwmSetWindowAttribute(handle, DWMWINDOWATTRIBUTE.NCRenderingPolicy, ref value, sizeof(DWMNCRENDERINGPOLICY)) != 0)
                return false;

            var m = new MARGINS()
            {
                bottomHeight = 1,
            };

            return NativeMethods.DwmExtendFrameIntoClientArea(handle, ref m) == 0;
        }
    }
}
