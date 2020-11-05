using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    class ShellIcons
    {
        public enum IconSize
        {
            Large = 0,
            Small = 1,
        }

        private IImageList[] iil;
        private Size[] sizes;

        public ShellIcons()
        {
            iil = new IImageList[2];
            sizes = new Size[2];
        }

        public Size GetSize(IconSize size)
        {
            var i = (int)size;

            if (!sizes[i].IsEmpty)
                return sizes[i];

            lock (iil)
            {
                if (!sizes[i].IsEmpty)
                    return sizes[i];

                switch (size)
                {
                    case IconSize.Small:

                        return sizes[i] = new Size(NativeMethods.GetSystemMetrics(SystemMetric.SM_CXSMICON), NativeMethods.GetSystemMetrics(SystemMetric.SM_CYSMICON));

                    case IconSize.Large:

                        return sizes[i] = new Size(NativeMethods.GetSystemMetrics(SystemMetric.SM_CXICON), NativeMethods.GetSystemMetrics(SystemMetric.SM_CYICON));

                    default:

                        return Size.Empty;
                }
            }
        }

        public Icon GetIcon(string path, IconSize size)
        {
            var i = (int)size;
            var guid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            var iml = iil[i];

            if (iml == null)
            {
                lock (iil)
                {
                    iml = iil[i];

                    if (iml == null)
                    {
                        if (NativeMethods.SHGetImageList(i, ref guid, out iml) != 0)
                            return null;
                        iil[i] = iml;
                    }
                }
            }

            var h = IntPtr.Zero;
            var shfi = new SHFILEINFO();

            const uint SHGFI_SYSICONINDEX = 0x000004000;
            const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
            const int ILD_TRANSPARENT = 0x00000001;
            const int ILD_IMAGE = 0x00000020;

            NativeMethods.SHGetFileInfo(path, 0, ref shfi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi), SHGFI_SYSICONINDEX | SHGFI_USEFILEATTRIBUTES);

            iml.GetIcon(shfi.iIcon, ILD_TRANSPARENT | ILD_IMAGE, ref h);

            if (h != IntPtr.Zero)
            {
                try
                {
                    return (Icon)Icon.FromHandle(h).Clone();
                }
                finally
                {
                    NativeMethods.DestroyIcon(h);
                }
            }

            return null;
        }
    }
}
