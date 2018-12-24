using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class Color
    {
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
    }
}
