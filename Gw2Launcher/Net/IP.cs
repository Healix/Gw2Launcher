using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Gw2Launcher.Net
{
    public static class IP
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

        public static bool IsMatch(IPAddress a, WildcardAddress b)
        {
            switch (a.AddressFamily)
            {
                case System.Net.Sockets.AddressFamily.InterNetworkV6:
                    if (b.Type != WildcardAddress.AddressType.IPv6)
                        return false;
                    break;
                case System.Net.Sockets.AddressFamily.InterNetwork:
                    if (b.Type != WildcardAddress.AddressType.IPv4)
                        return false;
                    break;
                default:
                    return false;
            }

            var b1 = a.GetAddressBytes();
            var b2 = b.Address;

            if (b2.Length > b1.Length)
                return false;

            for (var i = b2.Length - 1; i >= 0; --i)
            {
                if (b1[i] != b2[i])
                    return false;
            }

            return true;
        }

        public static bool IsMatch(IPAddress a, WildcardAddress[] b)
        {
            if (b == null)
                return false;

            var b1 = a != null ? a.GetAddressBytes() : new byte[4];

            for (var j = 0; j < b.Length; j++)
            {
                var b2 = b[j].Address;
                var match = true;

                if (b2.Length > b1.Length)
                    continue;

                for (var i = b2.Length - 1; i >= 0; --i)
                {
                    if (b1[i] != b2[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// An IP address with wildcard matching (127.0.*)
        /// </summary>
        public class WildcardAddress
        {
            public enum AddressType : byte
            {
                IPv4 = 4,
                IPv6 = 16
            }

            public WildcardAddress(AddressType type, byte[] address)
            {
                Type = type;
                Address = address;
            }

            /// <summary>
            /// Parses an IP that can have wildcards. The first wildcard will be the end of the address (127.*.0.1 will become 127.*)
            /// </summary>
            public static WildcardAddress Parse(string ip)
            {
                var i = ip.IndexOfAny(new char[] { '.', ':' });

                if (i == -1)
                    throw new FormatException();

                var j = 0;
                var c = ip[i];
                var v6 = c == ':';
                var at = v6 ? AddressType.IPv6 : AddressType.IPv4;
                var bytes = new byte[(byte)at];
                var length = ip.Length;
                var count = 0;

                while (true)
                {
                    var l = i - j;

                    if (l == 1 && ip[j] == '*')
                    {
                        break;
                    }

                    if (v6)
                    {
                        if (l > 0)
                        {
                            var v = Convert.ToUInt16(ip.Substring(j, l), 16);
                            bytes[count++] = (byte)(v >> 8);
                            bytes[count++] = (byte)(v & 255);
                        }
                        else
                        {
                            count += 2;
                        }
                    }
                    else
                    {
                        var v = byte.Parse(ip.Substring(j, l));
                        bytes[count++] = v;
                    }

                    j = i + 1;

                    if (count == bytes.Length || i == length)
                        break;

                    i = ip.IndexOf(c, j);
                    if (i == -1)
                        i = ip.Length;
                }

                if (count != bytes.Length)
                    Array.Resize<byte>(ref bytes, count);

                return new WildcardAddress(at, bytes);
            }

            public AddressType Type
            {
                get;
                private set;
            }

            public byte[] Address
            {
                get;
                private set;
            }

            public void ToString(StringBuilder sb)
            {
                if (Address.Length == 0)
                    sb.Append('*');

                if (Type == AddressType.IPv6)
                {
                    sb.EnsureCapacity(sb.Length + Address.Length / 2 * 5);

                    for (var i = 0; i < Address.Length; i += 2)
                    {
                        sb.Append((Address[i] << 8 | Address[i + 1]).ToString("x"));
                        sb.Append(':');
                    }
                }
                else
                {
                    sb.EnsureCapacity(sb.Length + Address.Length * 4);

                    for (var i = 0; i < Address.Length; i++)
                    {
                        sb.Append(Address[i]);
                        sb.Append('.');
                    }
                }

                if (Address.Length == (byte)Type)
                    sb.Length--;
                else
                    sb.Append('*');
            }

            public override string ToString()
            {
                System.Text.StringBuilder sb;

                if (Type == AddressType.IPv6)
                    sb = new StringBuilder(Address.Length / 2 * 5);
                else
                    sb = new StringBuilder(Address.Length * 4);

                ToString(sb);

                return sb.ToString();
            }
        }
    }
}
