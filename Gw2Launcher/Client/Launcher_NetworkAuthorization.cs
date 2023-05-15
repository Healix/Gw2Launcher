using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        public class NetworkChangedEventArgs : EventArgs
        {
            public NetworkChangedEventArgs(System.Net.IPAddress ip)
            {
                this.Address = ip;
            }

            public System.Net.IPAddress Address
            {
                get;
                private set;
            }

            public bool Abort
            {
                get;
                set;
            }

            public void Remember(System.Net.IPAddress ip)
            {
                Network.Remember(ip);
            }
        }

        public static event EventHandler<NetworkChangedEventArgs> NetworkChanged;

        private static class Network
        {
            private static int timestamp;

            public static uint Elapsed
            {
                get
                {
                    return (uint)(Environment.TickCount - timestamp);
                }
            }

            public static bool Verify()
            {
                using (var t = Net.IP.GetPublicAddress())
                {
                    t.Wait();

                    var ip = t.Result;
                    var match = Net.IP.IsMatch(ip, Settings.IPAddresses.Value);

                    timestamp = Environment.TickCount;

                    if (!match)
                    {
                        if ((Settings.Network.Value & Settings.NetworkOptions.WarnOnChange) != 0 && NetworkChanged != null)
                        {
                            var e = new NetworkChangedEventArgs(ip);
                            NetworkChanged(null, e);
                            if (e.Abort)
                                return false;
                        }
                        else if ((Settings.Network.Value & Settings.NetworkOptions.Enabled) != 0 && ip != null)
                        {
                            Remember(ip);
                        }
                    }

                    return true;
                }
            }

            public static void Remember(System.Net.IPAddress ip)
            {
                var address = ip.GetAddressBytes();
                Net.IP.WildcardAddress.AddressType type;

                switch (ip.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:

                        type = Net.IP.WildcardAddress.AddressType.IPv4;
                        if ((Settings.Network.Value & Settings.NetworkOptions.Exact) == 0)
                            Array.Resize(ref address, address.Length - 1);

                        break;
                    default:
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:

                        type = Net.IP.WildcardAddress.AddressType.IPv6;

                        break;
                }

                var w = new Net.IP.WildcardAddress(type, address);
                var existing = Settings.IPAddresses.Value;

                if (existing == null || existing.Length == 0)
                    existing = new Net.IP.WildcardAddress[1];
                else
                    Array.Resize(ref existing, existing.Length + 1);
                existing[existing.Length - 1] = w;

                Settings.IPAddresses.Value = existing;
            }

        }


    }
}
