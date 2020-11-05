using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class IPEndPoint
    {
        public static System.Net.IPEndPoint Parse(string s, int defaultPort)
        {
            int i = s.LastIndexOf(':');
            if (i != -1)
            {
                var port = ushort.Parse(s.Substring(i + 1));

                s = s.Substring(0, i);
                defaultPort = port;
            }

            return new System.Net.IPEndPoint(System.Net.IPAddress.Parse(s), defaultPort);
        }

        public static bool TryParse(string s, int defaultPort, out System.Net.IPEndPoint ip)
        {
            try
            {
                ip = Parse(s, defaultPort);
                return true;
            }
            catch
            {
                ip = null;
                return false;
            }
        }
    }

    static class DnsEndPoint
    {
        public static System.Net.DnsEndPoint Parse(string s, int defaultPort)
        {
            int i = s.LastIndexOf(':');
            if (i != -1)
            {
                var port = ushort.Parse(s.Substring(i + 1));
                s = s.Substring(0, i);
                defaultPort = port;
            }

            return new System.Net.DnsEndPoint(s, defaultPort);
        }

        public static bool TryParse(string s, int defaultPort, out System.Net.DnsEndPoint ep)
        {
            try
            {
                ep = Parse(s, defaultPort);
                return true;
            }
            catch
            {
                ep = null;
                return false;
            }
        }
    }
}
