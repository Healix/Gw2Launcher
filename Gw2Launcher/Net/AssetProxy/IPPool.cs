using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Gw2Launcher.Net.AssetProxy
{
    class IPPool
    {
        public event EventHandler<RefreshAddressesEventArgs> RefreshAddresses;

        public class RefreshAddressesEventArgs : EventArgs
        {
            public bool Handled
            {
                get;
                set;
            }

            public IPEndPoint[] Addresses
            {
                get;
                set;
            }
        }

        public interface IAddress
        {
            IPEndPoint IP
            {
                get;
            }
            IPEndPoint GetAddress(int defaultPort);
            void Sample(double sample);
            void Error();
            void OK();
        }

        private class Address : IAddress
        {
            public double total;
            public int count;
            public double avg;
            public ushort errors;
            public int timestamp;

            public Address(IPEndPoint address)
            {
                this.IP = address;
            }

            public IPEndPoint IP
            {
                get;
                private set;
            }

            public IPEndPoint GetAddress(int port)
            {
                if (IP.Port != 0)
                    return IP;
                return new IPEndPoint(IP.Address, port);
            }

            public void Sample(double sample)
            {
                lock (this)
                {
                    if (double.MaxValue - total <= sample)
                        total = double.MaxValue;
                    else
                        total += sample;
                    ++count;
                    timestamp = Environment.TickCount;
                }
            }

            public void Error()
            {
                lock (this)
                {
                    ++errors;
                    timestamp = Environment.TickCount;
                }
            }

            public void OK()
            {
                lock (this)
                {
                    errors = 0;
                    timestamp = Environment.TickCount;
                }
            }

            public void Reset()
            {
                lock(this)
                {
                    avg = 0;
                    total = 0;
                    count = 0;
                }
            }

            public double Average
            {
                get
                {
                    return avg;
                }
            }

            public int Count
            {
                get
                {
                    return count;
                }
            }
        }

        private Address[] addresses;
        private int reset;
        private int lastRefresh;

        public IPPool()
        {

        }

        public IPPool(IPEndPoint[] addresses)
        {
            if (addresses != null)
            {
                SetAddresses(addresses);
            }
        }

        /// <summary>
        /// Addresses will be refreshed on the next request
        /// </summary>
        /// <param name="now">True to refresh immediately</param>
        public void Refresh(bool now = false)
        {
            addresses = null;

            if (now)
            {
                OnRefreshAddresses();
            }
        }

        private bool OnRefreshAddresses()
        {
            if (RefreshAddresses != null)
            {
                lastRefresh = Environment.TickCount;

                var e = new RefreshAddressesEventArgs();

                try
                {
                    RefreshAddresses(this, e);
                }
                catch { }

                if (e.Handled)
                {
                    SetAddresses(e.Addresses);

                    return true;
                }
            }

            return false;
        }

        private void SetAddresses(IPEndPoint[] addresses)
        {
            var _addresses = new Address[addresses.Length];
            var t = Environment.TickCount;

            for (var i = 0; i < addresses.Length; i++)
            {
                _addresses[i] = new Address(addresses[i])
                {
                    timestamp = t,
                };
            }

            reset = lastRefresh = t;
            this.addresses = _addresses;
        }

        public bool IsAlive()
        {
            return true;
        }

        public IPAddress[] GetAddresses()
        {
            lock (this)
            {
                if (addresses != null)
                {
                    var ips = new IPAddress[addresses.Length];

                    for (var i = 0; i < addresses.Length; i++)
                    {
                        ips[i] = addresses[i].IP.Address;
                    }

                    return ips;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the next address
        /// </summary>
        /// <param name="skip">Optional callback to skip addresses</param>
        public IAddress GetAddress(Func<IPAddress, bool> skip = null)
        {
            lock (this)
            {
                if (addresses == null)
                {
                    if ((lastRefresh == 0 || Environment.TickCount - lastRefresh > 10000) && OnRefreshAddresses())
                    {
                        //ok
                    }
                    else
                    {
                        throw new Exception("No addresses available");
                    }
                }

                var t = Environment.TickCount;
                var r = t - reset > 600000;

                var index = -1;
                var last = int.MinValue;
                var errors = 0;

                for (var i = 0; i < addresses.Length; i++)
                {
                    if (skip == null || !skip(addresses[i].IP.Address))
                    {
                        if (t - addresses[i].timestamp > last)
                        {
                            last = t - addresses[i].timestamp;
                            index = i;
                        }
                    }

                    if (r)
                        addresses[i].Reset();
                    if (addresses[i].errors > 0)
                        ++errors;
                }

                if (errors == addresses.Length)
                {
                    if (Environment.TickCount - lastRefresh > 60000)
                    {
                        Refresh(true);
                        return GetAddress();
                    }
                }

                if (index == -1)
                    return null;

                addresses[index].timestamp = t;
                return addresses[index];
            }
        }

        public int Count
        {
            get
            {
                if (addresses == null || !OnRefreshAddresses())
                {
                    return 0;
                }
                return addresses.Length;
            }
        }

        public IPPool Alternate
        {
            get;
            set;
        }
    }
}
