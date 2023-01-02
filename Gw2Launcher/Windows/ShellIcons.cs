using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Gw2Launcher.Windows.Native;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Windows
{
    static class ShellIcons
    {
        const uint SHGFI_SYSICONINDEX = 0x000004000;
        const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        const int ILD_TRANSPARENT = 0x00000001;
        const int ILD_IMAGE = 0x00000020;

        public enum IconSize
        {
            /// <summary>
            /// 32x32
            /// </summary>
            Large = 0,
            /// <summary>
            /// 16x16
            /// </summary>
            Small = 1,
            /// <summary>
            /// 48x48
            /// </summary>
            ExtraLarge = 2,
            /// <summary>
            /// Caption icon
            /// </summary>
            SystemSmall = 3,
            /// <summary>
            /// 256x256
            /// </summary>
            Jumbo = 4,
        }

        private static short[] sizes;

        //getting icons is limited to 1 thread at a time

        static ShellIcons()
        {
            sizes = new short[5];
        }

        public static Size GetSize(IconSize size)
        {
            lock (sizes)
            {
                var i = (int)size;

                if (sizes[i] > 0)
                {
                    return new Size(sizes[i], sizes[i]);
                }
                else if (sizes[i] == 0)
                {
                    try
                    {
                        var guid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
                        IImageList iml;

                        if (NativeMethods.SHGetImageList(i, ref guid, out iml) == 0)
                        {
                            try
                            {
                                int x = 0,
                                    y = 0;

                                iml.GetIconSize(ref x, ref y);
                                sizes[i] = (short)x;

                                return new Size(x, x);
                            }
                            finally
                            {
                                Marshal.FinalReleaseComObject(iml);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                return Size.Empty;
            }
        }

        public static Icon GetIcon(string path, IconSize size)
        {
            lock (sizes)
            {
                try
                {
                    var shfi = new SHFILEINFO();
                    var iml = NativeMethods.SHGetFileInfo(path, 0, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_SYSICONINDEX | SHGFI_USEFILEATTRIBUTES | (uint)size);

                    if (iml != null)
                    {
                        var h = IntPtr.Zero;

                        try
                        {
                            iml.GetIcon(shfi.iIcon, ILD_TRANSPARENT | ILD_IMAGE, ref h);

                            if (h != IntPtr.Zero)
                            {
                                return (Icon)Icon.FromHandle(h).Clone();
                            }
                        }
                        finally
                        {
                            if (h != IntPtr.Zero)
                                NativeMethods.DestroyIcon(h);
                            Marshal.FinalReleaseComObject(iml);
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            return null;
        }
    }
}
