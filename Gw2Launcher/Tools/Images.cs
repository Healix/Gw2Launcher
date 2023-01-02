using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Gw2Launcher.Tools
{
    public static class Images
    {
        /// <summary>
        /// Loads an image from a file and fits it within the specified width/height
        /// </summary>
        /// <param name="width">0 to use the image width</param>
        /// <param name="height">0 to use the image height</param>
        public static Image Load(string path, int width = 0, int height = 0)
        {
            using (var image = Bitmap.FromFile(path))
            {
                int w = image.Width,
                    h = image.Height;

                if (width == 0)
                    width = w;
                if (height == 0)
                    height = h;

                if (w == width && h == height)
                {
                    return (Image)image.Clone();
                }

                float rx = (float)width / w,
                      ry = (float)height / h,
                      r = rx < ry ? rx : ry;

                if (r < 1)
                {
                    w = (int)(w * r + 0.5f);
                    h = (int)(h * r + 0.5f);
                }

                var bmp = new Bitmap(width, height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.DrawImage(image, width / 2 - w / 2, height / 2 - h / 2, w, h);
                }
                return bmp;
            }
        }

        public static Task<Image> LoadAsync(string path, int width = 0, int height = 0)
        {
            return Task.Run<Image>(new Func<Image>(
                delegate
                {
                    return Load(path, width, height);
                }));
        }
    }
}
