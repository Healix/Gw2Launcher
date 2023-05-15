using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Gw2Launcher.Net
{
    public static class DnsServers
    {
        public struct DnsServer
        {
            public string name;
            public IPAddress[] IP;

            public override string ToString()
            {
                return name;
            }
        }

        private static readonly DnsServer[] servers;

        static DnsServers()
        {
            servers = new DnsServer[]
            {
                new DnsServer()
                {
                    name = "Default",
                    IP = new IPAddress[] 
                    { 
                        IPAddress.Loopback 
                    }
                },
                new DnsServer()
                {
                    name = "Cloudflare DNS",
                    IP = new IPAddress[] 
                    { 
                        new IPAddress(new byte[] { 1,1,1,1 }),
                        new IPAddress(new byte[] { 1,0,0,1 })
                    }
                },
                new DnsServer()
                {
                    name = "Comodo Secure DNS",
                    IP = new IPAddress[] 
                    { 
                        new IPAddress(new byte[] { 8,26,56,26 }),
                        new IPAddress(new byte[] { 8,20,247,20 })
                    }
                },
                new DnsServer()
                {
                    name = "DNS.Watch",
                    IP = new IPAddress[] 
                    { 
                        new IPAddress(new byte[] { 84,200,69,80 }),
                        new IPAddress(new byte[] { 84,200,70,40 })
                    }
                },
                new DnsServer()
                {
                    name = "Google DNS",
                    IP = new IPAddress[] 
                    { 
                        new IPAddress(new byte[] { 8, 8, 8, 8 }),
                        new IPAddress(new byte[] { 8, 8, 4, 4 })
                    }
                },
                new DnsServer()
                {
                    name = "Norton ConnectSafe",
                    IP = new IPAddress[] 
                    { 
                        new IPAddress(new byte[] { 199,85,126,10 }),
                        new IPAddress(new byte[] { 199,85,127,10 })
                    }
                },
                new DnsServer()
                {
                    name = "OpenDNS",
                    IP = new IPAddress[] 
                    { 
                        new IPAddress(new byte[] { 208,67,220,220 }),
                        new IPAddress(new byte[] { 208,67,222,222 })
                    }
                },
                new DnsServer()
                {
                    name = "VeriSign Public DNS",
                    IP = new IPAddress[] 
                    { 
                        new IPAddress(new byte[] { 64,6,64,6 }),
                        new IPAddress(new byte[] { 64,6,65,6 })
                    }
                },
            };
        }

        public static DnsServer[] Servers
        {
            get
            {
                return servers;
            }
        }

        public static List<IPAddress> GetIPs()
        {
            List<IPAddress> ips = new List<IPAddress>();
            foreach (var server in servers)
            {
                ips.AddRange(server.IP);
            }
            return ips;
        }

        public static IEnumerable<IPAddress> EnumerateIPs(bool includeLocal = true, bool includeAlternates = true)
        {
            for (var i = includeLocal ? 0 : 1; i < servers.Length; i++)
            {
                if (includeAlternates)
                {
                    for (var j = 0; j < servers[i].IP.Length; j++)
                    {
                        yield return servers[i].IP[j];
                    }
                }
                else
                {
                    yield return servers[i].IP[0];
                }
            }
        }
    }
}
