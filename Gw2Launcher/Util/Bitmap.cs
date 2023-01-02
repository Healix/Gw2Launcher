using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Util
{
    static class Bitmap
    {
        /// <summary>
        /// Replaces pixels with an alpha of 0 to the desired color
        /// </summary>
        /// <param name="transparencyKey">The color that will replace transparent pixels</param>
        public static void ReplaceTransparentPixels(System.Drawing.Bitmap image, System.Drawing.Color transparencyKey)
        {
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            try
            {
                int length = data.Height * data.Stride,
                    bypp = data.Stride / data.Width,
                    key = transparencyKey.ToArgb();
                if (bypp != 4) //32-bit
                    throw new BadImageFormatException("PixelFormat is not 32-bit");
                var scan0 = data.Scan0;
                var buffer = new int[length / bypp];

                Marshal.Copy(scan0, buffer, 0, buffer.Length);
                int i = 0;

                foreach (var px in buffer)
                {
                    if (px >> 24 == 0)
                        buffer[i] = key;
                    ++i;
                }

                Marshal.Copy(buffer, 0, scan0, buffer.Length);

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

            }
            finally
            {
                image.UnlockBits(data);
            }
        }

        /// <summary>
        /// Blends the desired color into the image
        /// </summary>
        /// <param name="color">The color that will be overlayed</param>
        /// <param name="grayscale">True if the input image is only using grayscale colors R=G=B</param>
        /// <param name="shadeShift">A positive value will lighten darks. Red overlayed on Black with a +100 shift will result in Dark Red for example, whereas +0 would be Black</param>
        public static void BlendColor(System.Drawing.Bitmap image, System.Drawing.Color color, bool grayscale, ushort shadeShift)
        {
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat);
            try
            {
                int length = data.Height * data.Stride,
                    bypp = data.Stride / data.Width;

                if (bypp != 4) //32-bit (BGRA)
                    throw new BadImageFormatException("PixelFormat is not 32-bit");
                var scan0 = data.Scan0;
                var buffer = new byte[length];

                Marshal.Copy(scan0, buffer, 0, length);

                byte cr = color.R,
                     cg = color.G,
                     cb = color.B;

                int ct = 255 + shadeShift;

                if (grayscale)
                {
                    for (var i = 0; i < length; i += bypp)
                    {
                        if (buffer[i + 3] != 0)
                        {
                            var b = buffer[i];

                            if (b == 255)
                            {
                                buffer[i] = cb;
                                buffer[i + 1] = cg;
                                buffer[i + 2] = cr;
                            }
                            else
                            {
                                var f = (float)(b + shadeShift) / ct;

                                buffer[i] = (byte)(cb * f);
                                buffer[i + 1] = (byte)(cg * f);
                                buffer[i + 2] = (byte)(cr * f);
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < length; i += bypp)
                    {
                        if (buffer[i + 3] != 0)
                        {
                            byte b = buffer[i],
                                 g = buffer[i+1],
                                 r = buffer[i+2];

                            if (b == 255)
                                buffer[i] = cb;
                            else
                                buffer[i] = (byte)(cb * (b + shadeShift) / ct);

                            if (g == 255)
                                buffer[i + 1] = cg;
                            else
                                buffer[i + 1] = (byte)(cg * (g + shadeShift) / ct);

                            if (r == 255)
                                buffer[i + 2] = cr;
                            else
                                buffer[i + 2] = (byte)(cr * (r + shadeShift) / ct);
                        }
                    }
                }

                Marshal.Copy(buffer, 0, scan0, length);
            }
            finally
            {
                image.UnlockBits(data);
            }
        }

        /// <summary>
        /// Creates a simple white rectangle with a red border and x
        /// </summary>
        public static Image CreateErrorImage(int width, int height)
        {
            var i = new System.Drawing.Bitmap(width, height);
            using (var g = Graphics.FromImage(i))
            {
                g.Clear(System.Drawing.Color.White);
                g.DrawRectangle(Pens.Red, 0, 0, width - 1, height - 1);
                g.DrawLine(Pens.Red, 0, 0, width - 1, height - 1);
                g.DrawLine(Pens.Red, 0, height - 1, width - 1, 0);
            }
            return i;
        }

        /// <summary>
        /// Resizes an convert an icon to an image
        /// </summary>
        /// <param name="icon">Icon to resize</param>
        /// <param name="iconSize">Optional icon size for multi-size icons</param>
        /// <param name="imageSize">Resulting image size</param>
        /// <returns>Resized icon as an image</returns>
        public static Image ResizeIcon(Icon icon, Size iconSize, Size imageSize)
        {
            Icon _icon;

            if (!iconSize.IsEmpty && (icon.Width != iconSize.Width || icon.Height != iconSize.Height))
                _icon = new Icon(icon, iconSize);
            else
                _icon = null;

            using (_icon)
            {
                var image = new System.Drawing.Bitmap(imageSize.Width, imageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (var g = Graphics.FromImage(image))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawIcon(_icon == null ? icon : _icon, new Rectangle(Point.Empty, imageSize));
                }

                return image;
            }
        }
    }
}
