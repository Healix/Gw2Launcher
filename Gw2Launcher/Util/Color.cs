using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class Color
    {
        public static System.Drawing.Color Invert(System.Drawing.Color c)
        {
            return System.Drawing.Color.FromArgb(c.A, 255 - c.R, 255 - c.G, 255 - c.B);
        }

        public static System.Drawing.Color Darken(System.Drawing.Color c, float percent)
        {
            percent = 1 - percent;

            return From(c.R * percent, c.G * percent, c.B * percent);
        }

        public static System.Drawing.Color Lighten(System.Drawing.Color c, float percent)
        {
            var a = byte.MaxValue * percent;
            percent = 1 - percent;

            return From(c.R * percent + a, c.G * percent + a, c.B * percent + a);
        }

        public static System.Drawing.Color Gradient(System.Drawing.Color ca, System.Drawing.Color cb, float percent)
        {
            float r, g, b;

            r = cb.R * percent;
            g = cb.G * percent;
            b = cb.B * percent;
                   
            percent = 1 - percent;

            return From(ca.R * percent + r, ca.G * percent + g, ca.B * percent + b);
        }

        private static System.Drawing.Color From(float r, float g, float b)
        {
            int _r, _g, _b;
            if (r > byte.MaxValue)
                _r = byte.MaxValue;
            else
                _r = (int)(r + 0.5f);
            if (g > byte.MaxValue)
                _g = byte.MaxValue;
            else
                _g = (int)(g + 0.5f);
            if (b > byte.MaxValue)
                _b = byte.MaxValue;
            else
                _b = (int)(b + 0.5f);

            return System.Drawing.Color.FromArgb(_r, _g, _b);
        }

        /// <summary>
        /// Converts the HSL values to RGB
        /// </summary>
        /// <param name="h">Hue between 0 and 240</param>
        /// <param name="s">Saturation between 0 and 240</param>
        /// <param name="l">Lightness between 0 and 240</param>
        /// <returns></returns>
        public static System.Drawing.Color FromHSL(int h, int s, int l)
        {
            try
            {
                var hls = Windows.Native.NativeMethods.ColorHLSToRGB(h, l, s);

                return System.Drawing.Color.FromArgb(255 << 24 | (hls & 0xff) << 16 | (hls >> 8 & 0xff) << 8 | (hls >> 16) & 0xff);
            }
            catch
            {
                return System.Drawing.Color.Empty;
            }
        }

        public static System.Drawing.Color FromUID(ushort uid)
        {
            var i = uid - 1;
            var h = i * 36 % 240;
            var s = 120 + (i / 7) * 35 % 160;
            if (s >= 240)
                s -= 160;
            var l = 120 + i / 28 * 30 % 80;
            if (l >= 180)
                l -= 100;

            try
            {
                return System.Drawing.Color.FromArgb(Windows.Native.NativeMethods.ColorHLSToRGB(h, l, s) | -16777216);
            }
            catch
            {
                return System.Drawing.Color.Empty;
            }
        }
    }
}
