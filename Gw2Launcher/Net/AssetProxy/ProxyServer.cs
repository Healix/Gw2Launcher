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
                this.remoteEP = value;
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

                lock (this)
                {
                    if ((remoteEP == null || remoteEP is DnsEndPoint) && (ipPool == null || !ipPool.IsAlive()))
                    {
                        IPAddress[] ips;
                        try
                        {
                            string hostname;
                            if (remoteEP is DnsEndPoint)
                                hostname = ((DnsEndPoint)remoteEP).Host;
                            else
                                hostname = ProxyServer.PATCH_SERVER;
                            ips = Dns.GetHostAddresses(hostname);
                            if (ips.Length == 0)
                                throw new IndexOutOfRangeException();
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);

                            throw new Exception("DNS lookup failed for " + PATCH_SERVER);
                        }

                        ipPool = new IPPool(ips);
                    }
                }

                var client = new Client(accept, remoteEP, ipPool);

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
                clients.Remove(client);

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
                }
                else
                    return;
            }

            if (ServerStopped != null)
                ServerStopped(this, EventArgs.Empty);
        }
    }
}
