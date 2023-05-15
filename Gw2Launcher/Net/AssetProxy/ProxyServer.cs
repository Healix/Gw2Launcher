using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Gw2Launcher.Net.AssetProxy
{
    class ProxyServer : IDisposable
    {
        public const string PATCH_SERVER = Settings.ASSET_HOST; //direct: origincdn.101.arenanetworks.com

        public event EventHandler<Client> ClientConnected;
        public event EventHandler<Exception> ClientError;
        public event EventHandler<Client> ClientClosed;
        public event EventHandler<int> ClientsChanged;
        public event EventHandler ServerStarted;
        public event EventHandler ServerStopped;
        public event EventHandler ServerRestarted;
        public event EventHandler<Exception> ServerError;
        public event EventHandler<HttpStream.HttpResponseHeader> ResponseHeaderReceived;
        public event EventHandler<HttpStream.HttpRequestHeader> RequestHeaderReceived;
        public event EventHandler<ArraySegment<byte>> ResponseDataReceived;
        public event EventHandler<ArraySegment<byte>> RequestDataReceived;

        private TcpListener listener;
        private EndPoint remoteEP;
        private bool isActive;
        private int port;
        private CancellationTokenSource cancelToken;
        private IPPool ipPool;
        //private int clients;
        private HashSet<Client> clients;
        private ushort defaultPort;

        public ProxyServer()
        {
            clients = new HashSet<Client>();

            Gw2Launcher.Client.Launcher.ActiveProcessCountChanged += Launcher_ActiveProcessCountChanged;
        }

        public void Dispose()
        {
            Gw2Launcher.Client.Launcher.ActiveProcessCountChanged -= Launcher_ActiveProcessCountChanged;

            Stop();
        }

        void Launcher_ActiveProcessCountChanged(Gw2Launcher.Client.Launcher.AccountType type, ushort e)
        {
            if (type != Gw2Launcher.Client.Launcher.AccountType.GuildWars2)
                return;

            if (e == 0)
            {
                lock (this)
                {
                    if (cancelToken == null)
                    {
                        cancelToken = new CancellationTokenSource();
                        AutoStop(cancelToken.Token);
                    }
                }
            }
            else if (cancelToken != null)
            {
                lock (this)
                {
                    if (cancelToken != null)
                    {
                        using (cancelToken)
                        {
                            cancelToken.Cancel();
                            cancelToken = null;
                        }
                    }
                }
            }
        }

        public bool IsListening
        {
            get
            {
                return isActive;
            }
        }

        public EndPoint RemoteEP
        {
            get
            {
                return remoteEP;
            }
            set
            {
                lock (this)
                {
                    if (this.remoteEP != value)
                    {
                        this.remoteEP = value;
                        if (this.ipPool != null)
                            this.ipPool.Refresh();
                    }
                }
            }
        }

        public int CurrentPort
        {
            get
            {
                if (!isActive)
                    return 0;
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
        }

        public ushort DefaultPort
        {
            get
            {
                return defaultPort;
            }
            set
            {
                if (defaultPort != value)
                {
                    defaultPort = value;

                    if (isActive && value > 0)
                    {
                        Restart();
                    }
                }
            }
        }
        
        public void Start()
        {
            Start(defaultPort);
        }

        public void Start(int port)
        {
            lock (this)
            {
                if (isActive)
                    return;
                else
                    isActive = true;
            }

            Client.Reset();
            Cache.Enabled = true;

            byte retry;

            if (port == 0 && this.listener != null)
            {
                retry = 1;
                port = this.port;
            }
            else
                retry = 0;

            ipPool = CreateDefaultIPPool();

            do
            {
                var listener = this.listener = new TcpListener(IPAddress.Loopback, port);

                try
                {
                    //prevent child processes from inheriting the socket and keeping it alive
                    Windows.Native.NativeMethods.SetHandleInformation(listener.Server.Handle, 1, 0);

                    listener.Start();
                    this.port = CurrentPort;

                    listener.BeginAcceptTcpClient(OnBeginAcceptTcpClient, listener);

                    break;
                }
                catch (Exception e)
                {
                    if (listener != null)
                    {
                        listener.Stop();
                        using (listener.Server) { }
                        this.listener = null;
                    }

                    Util.Logging.Log(e);
                    if (retry == 0)
                    {
                        isActive = false;
                        if (ServerError != null)
                            ServerError(this, e);
                        return;
                    }
                    else
                    {
                        port = 0;
                    }
                }
            }
            while (retry-- > 0);

            //cancelToken = new CancellationTokenSource();
            //AutoStop(cancelToken.Token);

            if (ServerStarted != null)
                ServerStarted(this, EventArgs.Empty);
        }

        private void Restart()
        {
            lock (this)
            {
                if (!isActive)
                    return;
                
                try
                {
                    this.listener.Stop();

                    var listener = this.listener = new TcpListener(IPAddress.Loopback, defaultPort);
                    listener.Start();

                    this.port = CurrentPort;

                    listener.BeginAcceptTcpClient(OnBeginAcceptTcpClient, listener);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);

                    Stop();

                    return;
                }
            }

            if (ServerRestarted != null)
                ServerRestarted(this, EventArgs.Empty);
        }

        private void OnBeginAcceptTcpClient(IAsyncResult r)
        {
            var l = (TcpListener)r.AsyncState;
            TcpClient accept = null;

            try
            {
                accept = l.EndAcceptTcpClient(r);

                var client = new Client(accept, ipPool);
                lock (this)
                {
                    //clients++;
                    clients.Add(client);

                    if (cancelToken != null)
                    {
                        using (cancelToken)
                        {
                            cancelToken.Cancel();
                            cancelToken = null;
                        }
                    }

                    if (ClientsChanged != null)
                        ClientsChanged(this, clients.Count);
                }

                if (ClientConnected != null)
                    ClientConnected(this, client);

                client.Closed += client_Closed;

                if (ResponseHeaderReceived != null)
                    client.ResponseHeaderReceived += client_ResponseHeaderReceived;
                if (RequestHeaderReceived != null)
                    client.RequestHeaderReceived += client_RequestHeaderReceived;
                if (ResponseDataReceived != null)
                    client.ResponseDataReceived += client_ResponseDataReceived;
                if (RequestDataReceived != null)
                    client.RequestDataReceived += client_RequestDataReceived;
                if (ClientError != null)
                    client.Error += client_Error;

                client.Start();
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);

                if (accept != null)
                {
                    accept.Close();
                    using (accept.Client) { }
                }

                var server = l.Server;
                if (server == null || !server.IsBound)
                {
                    using (server) { }
                    return;
                }
            }

            try
            {
                l.BeginAcceptTcpClient(OnBeginAcceptTcpClient, l);
            }
            catch { }
        }
        
        private async void AutoStop(CancellationToken cancel)
        {
            try
            {
                await Task.Delay(5 * 60000, cancel);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            lock (this)
            {
                if (!cancel.IsCancellationRequested)
                {
                    Stop();

                    //Cache.Clear();
                }
            }
        }

        void client_Error(object sender, Exception e)
        {
            if (ClientError != null)
                ClientError(sender, e);
        }

        void client_ResponseHeaderReceived(object sender, HttpStream.HttpResponseHeader e)
        {
            if (ResponseHeaderReceived != null)
                ResponseHeaderReceived(sender, e);
        }

        void client_RequestHeaderReceived(object sender, HttpStream.HttpRequestHeader e)
        {
            if (RequestHeaderReceived != null)
                RequestHeaderReceived(sender, e);
        }

        void client_ResponseDataReceived(object sender, ArraySegment<byte> e)
        {
            if (ResponseDataReceived != null)
                ResponseDataReceived(sender, e);
        }

        void client_RequestDataReceived(object sender, ArraySegment<byte> e)
        {
            if (RequestDataReceived != null)
                RequestDataReceived(sender, e);
        }

        void client_Closed(object sender, EventArgs e)
        {
            var client = (Client)sender;

            lock (this)
            {
                //clients--;
                if (clients.Remove(client))
                {
                    if (ClientsChanged != null)
                        ClientsChanged(this, clients.Count);
                }

                //if (clients == 0 && cancelToken == null)
                //{
                //    cancelToken = new CancellationTokenSource();
                //    AutoStop(cancelToken.Token);
                //}
            }

            if (ClientClosed != null)
                ClientClosed(this, client);

            client.Dispose();
        }

        public void Stop()
        {
            lock (this)
            {
                if (isActive)
                {
                    isActive = false;

                    try
                    {
                        listener.Stop();
                        using (listener.Server) { }
                        listener = null;
                    }
                    catch { }

                    foreach (var client in clients)
                    {
                        client.Dispose();
                    }

                    if (cancelToken != null)
                    {
                        using (cancelToken)
                        {
                            cancelToken.Cancel();
                            cancelToken = null;
                        }
                    }

                    ipPool = null;
                    Client.Reset();
                }
                else
                    return;
            }

            if (ServerStopped != null)
                ServerStopped(this, EventArgs.Empty);
        }

        public static IPPool CreateDefaultIPPool()
        {
            var pool = new IPPool();

            pool.Alternate = new IPPool()
            {
                Alternate = pool,
            };

            pool.RefreshAddresses += pool_RefreshAddresses;
            pool.Alternate.RefreshAddresses += alternate_RefreshAddresses;

            return pool;
        }

        private static void alternate_RefreshAddresses(object sender, IPPool.RefreshAddressesEventArgs e)
        {
            var ips = Dns.GetHostAddresses(Settings.ASSET_HOST, DnsServers.EnumerateIPs(false, true));
            var alt = ((IPPool)sender).Alternate;
            var index = 0;
            byte[][] addresses = null;

            if (ips.Count > 0)
            {
                Func<byte[], bool> contains = delegate(byte[] ip)
                {
                    for (var i = 0; i < addresses.Length; i++)
                    {
                        if (addresses[i] == null)
                            break;

                        if (addresses[i].Length == ip.Length)
                        {
                            var match = true;

                            for (var j = ip.Length / 2 - 1; j >= 0; --j)
                            {
                                if (ip[j] != addresses[i][j])
                                {
                                    match = false;
                                    break;
                                }
                            }

                            if (match)
                                return true;
                        }
                    }

                    return false;
                };

                if (alt != null)
                {
                    var ips2 = alt.GetAddresses();

                    if (ips2 != null)
                    {
                        addresses = new byte[ips.Count + ips2.Length][];

                        for (var i = 0; i < ips2.Length; i++)
                        {
                            addresses[index++] = ips2[i].GetAddressBytes();
                        }
                    }
                }

                if (addresses == null)
                {
                    addresses = new byte[ips.Count][];
                }

                var count = 0;
                var result = new IPEndPoint[ips.Count];

                foreach (var ip in ips)
                {
                    var bytes = ip.GetAddressBytes();

                    if (!contains(bytes))
                    {
                        addresses[index++] = bytes;
                        result[count++] = new IPEndPoint(ip, 80);
                    }
                }

                if (count > 0)
                {
                    Array.Resize(ref result, count);

                    e.Addresses = result;
                    e.Handled = true;
                }
                else if (ips.Count > 0)
                {
                    foreach (var ip in ips)
                    {
                        result[count++] = new IPEndPoint(ip, 80);
                    }

                    e.Addresses = result;
                    e.Handled = true;
                }
            }
        }

        private static void pool_RefreshAddresses(object sender, IPPool.RefreshAddressesEventArgs e)
        {
            IPEndPoint[] addresses = null;
            var assetsrv = Util.Args.GetValue(Settings.GuildWars2.Arguments.Value, "assetsrv");

            if (!string.IsNullOrEmpty(assetsrv))
            {
                IPEndPoint ipEp;
                if (Util.IPEndPoint.TryParse(assetsrv, 0, out ipEp))
                {
                    addresses = new IPEndPoint[] { ipEp };
                }
                else
                {
                    DnsEndPoint dnsEp;
                    if (Util.DnsEndPoint.TryParse(assetsrv, 0, out dnsEp))
                    {
                        var ips = Dns.GetHostAddresses(dnsEp.Host);
                        if (ips.Length == 0)
                            return;
                        addresses = new IPEndPoint[ips.Length];

                        for (var i = 0; i < ips.Length; i++)
                        {
                            addresses[i] = new IPEndPoint(ips[i], dnsEp.Port);
                        }
                    }
                }
            }

            if (addresses == null)
            {
                var ips = Dns.GetHostAddresses(Settings.ASSET_HOST);
                if (ips.Length == 0)
                    return;
                addresses = new IPEndPoint[ips.Length];

                for (var i = 0; i < ips.Length; i++)
                {
                    addresses[i] = new IPEndPoint(ips[i], 0);
                }
            }

            e.Handled = true;
            e.Addresses = addresses;
        }

        public int Clients
        {
            get
            {
                lock (this)
                {
                    return clients.Count;
                }
            }
        }
    }
}
