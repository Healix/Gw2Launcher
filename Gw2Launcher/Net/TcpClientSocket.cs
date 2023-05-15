using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Gw2Launcher.Net
{
    class TcpClientSocket : IDisposable
    {
        private TcpClient client;
        private int ticks;

        public TcpClientSocket(TcpClient client)
        {
            this.client = client;
        }

        public bool Connected
        {
            get
            {
                return client != null && client.Connected;
            }
        }

        public TcpClient Client
        {
            get
            {
                return client;
            }
        }

        public int LastUsed
        {
            get;
            set;
        }

        public int ReceiveTimeout
        {
            get
            {
                return client.ReceiveTimeout;
            }
            set
            {
                client.ReceiveTimeout = value;
            }
        }

        public int SendTimeout
        {
            get
            {
                return client.SendTimeout;
            }
            set
            {
                client.SendTimeout = value;
            }
        }

        public int SendReceiveTimeout
        {
            get
            {
                var t = client.SendTimeout;
                if (client.ReceiveTimeout > t)
                    t = client.ReceiveTimeout;
                return t;
            }
            set
            {
                client.SendTimeout = value;
                client.ReceiveTimeout = value;
            }
        }

        public IPEndPoint EndPoint
        {
            get;
            private set;
        }

        public AssetProxy.IPPool.IAddress Address
        {
            get;
            set;
        }

        public bool Connect(AssetProxy.IPPool.IAddress address, int port, int timeout)
        {
            this.Address = address;
            return Connect(address.GetAddress(port), timeout);
        }

        public bool Connect(IPEndPoint ip, int timeout)
        {
            this.EndPoint = ip;
            return client.ConnectAsync(ip.Address, ip.Port).Wait(timeout);
        }

        public void Close()
        {
            if (client != null)
            {
                client.Close();
            }
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Close();
                using (client.Client) { }
                client = null;
            }
        }
    }
}
