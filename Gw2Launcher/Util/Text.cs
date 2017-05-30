using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
