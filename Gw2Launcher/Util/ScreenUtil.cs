using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Gw2Launcher.Util
{
    static class ScreenUtil
    {
        /// <summary>
        /// Ensures that the rectangle (location, size) is at least partially visible on screen.
        /// </summary>
        public static Point Constrain(Point location, Size size)
        {
            var screen = Screen.FromPoint(location).Bounds;
            var bounds = new Rectangle(location, size);

            if (screen.Contains(bounds) || screen.IntersectsWith(bounds))
                return bounds.Location;

            return RectangleConstraint.Constrain(screen, bounds).Location;
        }

        public static Rectangle Constrain(Rectangle bounds)
        {
            var screen = Screen.FromPoint(bounds.Location).Bounds;

            if (screen.Contains(bounds) || screen.IntersectsWith(bounds))
                return bounds;

            return RectangleConstraint.Constrain(screen, bounds);
        }

        public static Point CenterScreen(Size size, Screen screen)
        {
            var bounds = screen.Bounds;
            var location = Point.Add(bounds.Location, new Size(bounds.Width / 2 - size.Width / 2, bounds.Height / 2 - size.Height / 2));
            if (location.Y < bounds.Y)
                location.Y = bounds.Y;
            if (location.X < bounds.X)
                location.X = bounds.X;
            return location;
        }
    }
}
