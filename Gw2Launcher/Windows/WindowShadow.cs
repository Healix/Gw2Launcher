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
        public static bool Enable(IntPtr handle, bool enabled = true)
        {
            if (!NativeMethods.IsDwmCompositionEnabled())
                return false;

            try
            {
                var value = (int)(enabled ? DWMNCRENDERINGPOLICY.DWMNCRP_ENABLED : DWMNCRENDERINGPOLICY.DWMNCRP_DISABLED);
                if (NativeMethods.DwmSetWindowAttribute(handle, DWMWINDOWATTRIBUTE.NCRenderingPolicy, ref value, sizeof(DWMNCRENDERINGPOLICY)) != 0)
                    return false;

                if (enabled)
                {
                    var m = new MARGINS()
                    {
                        topHeight = 1,
                    };

                    return NativeMethods.DwmExtendFrameIntoClientArea(handle, ref m) == 0;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
