using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Gw2Launcher.Net
{
    static class IP
    {
        public static async Task<IPAddress> GetPublicAddress()
        {
            /* Alternatives:
             * https://www.cloudflare.com/cdn-cgi/trace
             * http://whatismyip.akamai.com/
             */

            var request = HttpWebRequest.CreateHttp("http://checkip.amazonaws.com");

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    using (var r = new StreamReader(response.GetResponseStream()))
                    {
                        var line = await r.ReadLineAsync();

                        IPAddress ip;
                        if (IPAddress.TryParse(line, out ip))
                        {
                            return ip;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                using (e.Response) { }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Returns true if the addresses match x.x.x.y
        /// </summary>
        public static bool IsSimilar(IPAddress a, IPAddress b)
        {
            var b1 = a.GetAddressBytes();
            var b2 = b.GetAddressBytes();
            
            if (b1.Length != b2.Length)
                return false;

            for (var i = b1.Length - 2; i >= 0; --i)
            {
                if (b1[i] != b2[i])
                    return false;
            }

            return true;
        }
    }
}
