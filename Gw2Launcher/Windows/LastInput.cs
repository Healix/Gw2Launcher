using System;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class LastInput
    {
        public static uint TickCount
        {
            get
            {
                var li = new LASTINPUTINFO()
                {
                    cbSize = LASTINPUTINFO.SizeOf
                };
                if (!NativeMethods.GetLastInputInfo(ref li))
                    return 0;

                return (uint)Environment.TickCount - li.dwTime;
            }
        }

        /// <summary>
        /// Returns true if there was any input within the last 15 seconds
        /// </summary>
        public static bool IsActive
        {
            get
            {
                return TickCount < 15000;
            }
        }
    }
}
