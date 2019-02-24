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
        public event EventHandler<HttpStream.HttpResponseHeader> ResponseHeaderReceived;
        public event EventHandler<HttpStream.HttpRequestHeader> RequestHeaderReceived;
        public event EventHandler<ArraySegment<byte>> ResponseDataReceived;
        public event EventHandler<ArraySegment<byte>> RequestDataReceived;

        private TcpListener listener;
        private Task taskListener;
        private IPEndPoint remoteEP;
        private bool isActive;
        private int port;
        private CancellationTokenSource cancelToken;
        private bool dynamicEP;
        private IPPool ipPool;
        //private int clients;
        private HashSet<Client> clients;

        public ProxyServer()
        {
            dynamicEP = true;
            clients = new HashSet<Client>();

            Gw2Launcher.Client.Launcher.ActiveProcessCountChanged += Launcher_ActiveProcessCountChanged;
        }

        public void Dispose()
        {
            Gw2Launcher.Client.Launcher.ActiveProcessCountChanged -= Launcher_ActiveProcessCountChanged;

            Stop();
        }

        void Launcher_ActiveProcessCountChanged(object sender, int e)
        {
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

        public IPEndPoint RemoteEP
        {
            get
            {
                return remoteEP;
            }
            set
            {
                dynamicEP = value == null;
                this.remoteEP = value;
            }
        }

        public int Port
        {
            get
            {
                if (!isActive)
                    return 0;
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
        }
        
        public void Start()
        {
            Start(0);
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
                    listener.Start();
                    this.port = Port;

                    break;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    if (retry == 0)
                    {
                        isActive = false;
                        return;
                    }
                    else
                    {
                        port = 0;
                    }
                }
            }
            while (retry-- > 0);

            if (dynamicEP)
                remoteEP = null;

            //cancelToken = new CancellationTokenSource();
            //AutoStop(cancelToken.Token);

            if (ServerStarted != null)
                ServerStarted(this, EventArgs.Empty);

            if (taskListener != null && taskListener.IsCompleted)
                taskListener.Dispose();
            taskListener = Task.Factory.StartNew(DoListener, TaskCreationOptions.LongRunning);
        }

        private async void DoListener()
        {
            while (isActive)
            {
                TcpClient accept = null;

                try
                {
                    accept = await listener.AcceptTcpClientAsync();

                    if (remoteEP == null && ipPool == null)
                    {
                        IPAddress[] ips;
                        try
                        {
                            ips = Dns.GetHostAddresses(ProxyServer.PATCH_SERVER);
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
                        accept.Close();

                    var server = listener.Server;
                    if (server == null || !server.IsBound)
                    {
                        if (server != null)
                            server.Dispose();
                        return;
                    }
                }
            }
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

                    listener.Stop();

                    foreach (var client in clients)
                    {
                        client.Close();
                    }

                    if (taskListener != null && taskListener.IsCompleted)
                        taskListener.Dispose();

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
