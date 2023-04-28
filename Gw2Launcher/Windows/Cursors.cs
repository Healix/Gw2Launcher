using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class Cursors
    {
        private static Cursor _Hand;
        public static Cursor Hand
        {
            get
            {
#if DEBUG
                return System.Windows.Forms.Cursors.Hand;
#else
                if (_Hand == null)
                {
                    try
                    {
                        var h = NativeMethods.LoadCursor(IntPtr.Zero, 32649);
                        if (h == IntPtr.Zero)
                            _Hand = System.Windows.Forms.Cursors.Hand;
                        else
                            _Hand = new Cursor(h);
                    }
                    catch
                    {
                        _Hand = System.Windows.Forms.Cursors.Hand;
                    }
                }
                return _Hand;
#endif
            }
        }
    }
}
