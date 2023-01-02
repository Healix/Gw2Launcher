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
            return Constrain(new Rectangle(location, size)).Location;
        }

        public static Rectangle Constrain(Rectangle bounds)
        {
            var screen = Screen.FromRectangle(bounds).Bounds;

            if (screen.Contains(bounds) || screen.IntersectsWith(bounds))
                return bounds;

            return RectangleConstraint.Constrain(screen, bounds);
        }

        public static Rectangle Constrain(Rectangle bounds, Rectangle screen)
        {
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

        public static bool IsFullScreen(Rectangle bounds)
        {
            //note that a true full screen window will hide the taskbar and always be equal to the size of the screen
            return bounds == Screen.FromRectangle(bounds).Bounds;
        }

        public static Rectangle FromDesktopBounds(Rectangle bounds)
        {
            var screen = Screen.FromRectangle(bounds);
            var l = screen.Bounds.Location;
            var w = screen.WorkingArea.Location;

            if (l == w)
                return bounds;

            return new Rectangle(bounds.X + w.X - l.X, bounds.Y + w.Y - l.Y, bounds.Width, bounds.Height);
        }

        /// <summary>
        /// Converts bounds to use desktop coordinates, which are based on the working area of the screen
        /// </summary>
        public static Rectangle ToDesktopBounds(Rectangle bounds)
        {
            var screen = Screen.FromRectangle(bounds);
            var l = screen.Bounds.Location;
            var w = screen.WorkingArea.Location;

            if (l == w)
                return bounds;

            return new Rectangle(bounds.X + l.X - w.X, bounds.Y + l.Y - w.Y, bounds.Width, bounds.Height);
        }

        public static Point ToDesktopLocation(Point location)
        {
            var screen = Screen.FromPoint(location);
            var l = screen.Bounds.Location;
            var w = screen.WorkingArea.Location;

            if (l == w)
                return location;

            return new Point(location.X + l.X - w.X, location.Y + l.Y - w.Y);
        }
    }
}
