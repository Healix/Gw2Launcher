using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Gw2Launcher.Util
{
    static class Bitmap
    {
        public static void ReplaceTransparentPixels(System.Drawing.Bitmap image, Color transparencyKey)
        {
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            
            int length = data.Height * data.Stride,
                bypp = data.Stride / data.Width,
                key = transparencyKey.ToArgb();
            if (bypp != 4) //32-bit
                throw new BadImageFormatException("PixelFormat is not 32-bit");
            var scan0 = data.Scan0;

            for (var ofs = 0; ofs < length; ofs += bypp)
            {
                var px = System.Runtime.InteropServices.Marshal.ReadInt32(scan0, ofs);
                if ((px & 0xFF000000L) == 0L)
                    System.Runtime.InteropServices.Marshal.WriteInt32(scan0, ofs, key);
            }

            #region Unsafe version

            //unsafe
            //{
            //    int length = bd.Height * bd.Stride,
            //        bypp = bd.Stride / bd.Width;
            //    int* scan0 = (int*)bd.Scan0.ToPointer();

            //    for (var ofs = 0; ofs < length; ofs += bypp)
            //    {
            //        if ((*scan0 & 0xFF000000L) == 0L)
            //            *scan0 = key;

            //        scan0++;
            //    }
            //}

            #endregion

            image.UnlockBits(data);
        }
    }
}
