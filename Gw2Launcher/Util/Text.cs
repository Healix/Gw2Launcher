using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.Util
{
    static class Text
    {
        public static string FormatBytes(long bytes)
        {
            if (bytes > 858993459) //0.8 GB
            {
                return string.Format("{0:0.##} GB", bytes / 1073741824d);
            }
            else if (bytes > 838860) //0.8 MB
            {
                return string.Format("{0:0.##} MB", bytes / 1048576d);
            }
            else if (bytes > 819) //0.8 KB
            {
                return string.Format("{0:0.##} KB", bytes / 1024d);
            }
            else
            {
                return bytes + " bytes";
            }
        }

        public static TextFormatFlags GetAlignmentFlags(System.Drawing.ContentAlignment alignment)
        {
            switch (alignment)
            {
                case System.Drawing.ContentAlignment.BottomCenter:
                    return TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom;
                case System.Drawing.ContentAlignment.BottomLeft:
                    return TextFormatFlags.Left | TextFormatFlags.Bottom;
                case System.Drawing.ContentAlignment.BottomRight:
                    return TextFormatFlags.Right | TextFormatFlags.Bottom;
                case System.Drawing.ContentAlignment.MiddleCenter:
                    return TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                case System.Drawing.ContentAlignment.MiddleLeft:
                    return TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
                case System.Drawing.ContentAlignment.MiddleRight:
                    return TextFormatFlags.Right | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                case System.Drawing.ContentAlignment.TopCenter:
                    return TextFormatFlags.HorizontalCenter | TextFormatFlags.Top;
                case System.Drawing.ContentAlignment.TopLeft:
                    return TextFormatFlags.Left | TextFormatFlags.Top;
                case System.Drawing.ContentAlignment.TopRight:
                    return TextFormatFlags.Right | TextFormatFlags.Top;
            }
            return TextFormatFlags.Default;
        }
    }
}
