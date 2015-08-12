using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Gw2Launcher.Util
{
    static class RectangleConstraint
    {
        public static Rectangle Constrain(Rectangle constraint, Rectangle bounds)
        {
            if (bounds.X < constraint.X)
                bounds.X = constraint.X;
            else if (bounds.Right > constraint.Right)
                bounds.X = constraint.Right - bounds.Width;

            if (bounds.Bottom > constraint.Bottom)
                bounds.Y = constraint.Bottom - bounds.Height;
            else if (bounds.Top < constraint.Top)
                bounds.Y = constraint.Top;

            return bounds;
        }

        public static Rectangle ConstainToScreen(Control from, Rectangle bounds)
        {
            return Constrain(Screen.FromControl(from).Bounds, bounds);
        }

        public static Rectangle ConstainToScreen(Control from, Point location, Size size)
        {
            return Constrain(Screen.FromControl(from).Bounds, new Rectangle(location, size));
        }

        public static Rectangle ConstrainToScreen(Rectangle bounds)
        {
            Screen screen = Screen.FromPoint(bounds.Location);
            return Constrain(screen.Bounds, bounds);
        }

        public static Rectangle ConstrainToScreen(Point location, Size size)
        {
            return ConstrainToScreen(new Rectangle(location, size));
        }
    }
}
