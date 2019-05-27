using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Gw2Launcher.Net.AssetProxy
{
    class Client : IDisposable
    {
        private const int BUFFER_LENGTH = 102400; //16384
        private const string ASSET_HOST = Settings.ASSET_HOST;
        protected const byte CONNECTION_TIMEOUT_SECONDS = 5;
        protected const byte CONNECTION_TRANSFER_TIMEOUT_SECONDS = 180;
        protected const byte CONNECTION_KEEPALIVE_TIMEOUT_SECONDS = 120;

        private static ushort nextId;
        private static readonly object _lock;
        private static BpsLimiter bpsLimiter;

        private TcpClient clientIn, clientOut, clientSwap;
        private Task task;
        private IPEndPoint remoteEP;
        private IPPool ipPool;
        private bool doIPPool;
        private ushort id;

        /// <summary>
        /// Headers will be processed before raising data events
        /// </summary>
        public event EventHandler<HttpStream.HttpRequestHeader> RequestHeaderReceived;
        /// <summary>
        /// Headers will be processed before raising data events
        /// </summary>
        public event EventHandler<HttpStream.HttpResponseHeader> ResponseHeaderReceived;
        /// <summary>
        /// Raw byte stream including headers and chunks
        /// </summary>
        public event EventHandler<ArraySegment<byte>> ResponseDataReceived;
        /// <summary>
        /// Raw byte stream including headers and chunks
        /// </summary>
        public event EventHandler<ArraySegment<byte>> RequestDataReceived;
        public event EventHandler<Exception> Error;
        public event EventHandler Closed;

        static Client()
        {
            nextId = 1;
            _lock = new object();

            PatchingSpeedLimit_ValueChanged(Settings.PatchingSpeedLimit, null);
            Settings.PatchingSpeedLimit.ValueChanged += PatchingSpeedLimit_ValueChanged;
        }

        static void PatchingSpeedLimit_ValueChanged(object sender, EventArgs e)
        {
            var v = (Settings.ISettingValue<int>)sender;
            if (v.HasValue)
            {
                if (bpsLimiter == null)
                    bpsLimiter = new BpsLimiter();
                bpsLimiter.BpsLimit = v.Value;
            }
            else if (bpsLimiter != null)
            {
                bpsLimiter.Enabled = false;
            }
        }

        public static void Reset()
        {
            nextId = 1;
        }

        public Client(TcpClient client, IPEndPoint remote, IPPool ipPool)
        {
            lock (_lock)
            {
                id = nextId++;
            }

            if (remote == null)
            {
                doIPPool = true;
                this.ipPool = ipPool;
                remote = new IPEndPoint(ipPool.GetIP(), Settings.PatchingUseHttps.Value ? 443 : 80);
            }

            remoteEP = remote;

            clientIn = client;
            clientIn.ReceiveTimeout = CONNECTION_KEEPALIVE_TIMEOUT_SECONDS * 1000;
            clientIn.SendTimeout = CONNECTION_TIMEOUT_SECONDS * 1000;
            clientIn.SendBufferSize = BUFFER_LENGTH;
            clientIn.NoDelay = true;
        }

        public void Dispose()
        {
            if (clientOut != null)
                clientOut.Close();

            Close();

            if (task != null && task.IsCompleted)
                task.Dispose();
        }

        public IPEndPoint RemoteEP
        {
            get
            {
                return remoteEP;
            }
        }

        public ushort ID
        {
            get
            {
                return id;
            }
        }

        public void Close()
        {
            if (clientIn != null)
                clientIn.Close();
            if (clientSwap != null)
                clientSwap.Close();
        }

        public void Start()
        {
            if (task != null)
            {
                if (!task.IsCompleted)
                    return;
                task.Dispose();
            }
            task = Task.Factory.StartNew(DoClient, TaskCreationOptions.LongRunning);
        }

        private void DoClient()
        {
            int count = 0;

            BpsLimiter.BpsShare bpsShare;
            if (bpsLimiter != null)
                bpsShare = bpsLimiter.GetShare();
            else
                bpsShare = null;

            try
            {
                var buffer = new byte[BUFFER_LENGTH];
                int read;

                Stream stream;
                HttpStream httpIn = new HttpStream(stream = clientIn.GetStream());
                HttpStream httpOut = null;
                bool doSwap = false;
                Task<IPAddress> taskSwap = null;

                HttpStream.HttpHeader header;

                while (clientIn.Connected)
                {
                    Cache.CacheStream cache = null;
                    bool writeCache = false;

                    if ((read = httpIn.ReadHeader(buffer, 0, out header)) == 0)
                        return;

                    var request = (HttpStream.HttpRequestHeader)header;
                    if (RequestHeaderReceived != null)
                        RequestHeaderReceived(this, request);

                    try
                    {
                        #region Cached responses

                        if (Cache.Enabled)
                        {
                            cache = Cache.GetCache(request.Location);

                            if (cache != null)
                            {
                                if (cache.HasData)
                                {
                                    do
                                    {
                                        read = httpIn.Read(buffer, 0, BUFFER_LENGTH);
                                    }
                                    while (read > 0);

                                    if (ResponseHeaderReceived != null)
                                        ResponseHeaderReceived(this, HttpStream.HttpResponseHeader.Cached());

                                    while ((read = cache.Read(buffer, 0, BUFFER_LENGTH)) > 0)
                                    {
                                        httpIn.Write(buffer, 0, read);
                                    }

                                    continue;
                                }

                                writeCache = cache.CanWrite;
                            }
                        }

                        #endregion

                        #region Remote connection

                        if (doSwap)
                        {
                            if (taskSwap.IsCompleted)
                            {
                                if (taskSwap.Result != null)
                                {
                                    ipPool.AddSample(taskSwap.Result, double.MaxValue);
                                }
                                else if (clientSwap != null && clientSwap.Connected)
                                {
                                    if (clientOut != null)
                                        clientOut.Close();
                                    clientOut = clientSwap;
                                    clientSwap = null;
                                    remoteEP = (IPEndPoint)clientOut.Client.RemoteEndPoint;

                                    if (remoteEP.Port == 443)
                                        httpOut.SetBaseStream(clientOut.GetStream(), true, ASSET_HOST, false);
                                    else
                                        httpOut.SetBaseStream(clientOut.GetStream(), true);
                                }

                                doSwap = false;
                                count = 0;
                                taskSwap.Dispose();
                            }
                        }

                        if (clientOut == null || !clientOut.Connected)
                        {
                            clientOut = new TcpClient()
                            {
                                ReceiveTimeout = CONNECTION_TIMEOUT_SECONDS * 1000,
                                SendTimeout = CONNECTION_TIMEOUT_SECONDS * 1000,
                                ReceiveBufferSize = BUFFER_LENGTH * 2,
                            };

                            try
                            {
                                if (!clientOut.ConnectAsync(remoteEP.Address, remoteEP.Port).Wait(CONNECTION_TIMEOUT_SECONDS * 1000))
                                {
                                    throw new TimeoutException("Unable to connect to " + remoteEP.ToString());
                                }
                            }
                            catch
                            {
                                if (doIPPool)
                                    ipPool.AddSample(remoteEP.Address, double.MaxValue);

                                throw;
                            }

                            if (remoteEP.Port == 443)
                                httpOut = new HttpStream(clientOut.GetStream(), ASSET_HOST, false);
                            else
                                httpOut = new HttpStream(clientOut.GetStream());
                        }

                        #endregion

                        if (read > 0 && RequestDataReceived != null)
                            RequestDataReceived(this, new ArraySegment<byte>(buffer, 0, read));

                        clientOut.ReceiveTimeout = clientOut.SendTimeout = CONNECTION_TIMEOUT_SECONDS * 1000;

                        do
                        {
                            httpOut.Write(buffer, 0, read);
                            read = httpIn.Read(buffer, 0, BUFFER_LENGTH);

                            if (read > 0 && RequestDataReceived != null)
                                RequestDataReceived(this, new ArraySegment<byte>(buffer, 0, read));
                        }
                        while (read > 0);

                        if ((read = httpOut.ReadHeader(buffer, 0, out header)) == 0)
                            return;

                        if (ResponseHeaderReceived != null)
                            ResponseHeaderReceived(this, (HttpStream.HttpResponseHeader)header);

                        if (read > 0 && ResponseDataReceived != null)
                            ResponseDataReceived(this, new ArraySegment<byte>(buffer, 0, read));

                        clientOut.ReceiveTimeout = clientOut.SendTimeout = CONNECTION_TRANSFER_TIMEOUT_SECONDS * 1000;

                        DateTime startTime = DateTime.UtcNow;
                        long bytes = 0;
                        
                        try
                        {
                            do
                            {
                                if (writeCache)
                                    cache.Write(buffer, 0, read);

                                httpIn.Write(buffer, 0, read);

                                if (bpsShare != null)
                                {
                                    read = httpOut.Read(buffer, 0, bpsShare.GetLimit(BUFFER_LENGTH));
                                    bpsShare.Used(read);
                                }
                                else
                                    read = httpOut.Read(buffer, 0, BUFFER_LENGTH);

                                bytes += read;

                                if (read > 0 && ResponseDataReceived != null)
                                    ResponseDataReceived(this, new ArraySegment<byte>(buffer, 0, read));
                            }
                            while (read > 0);
                        }
                        catch
                        {
                            if (doIPPool)
                                ipPool.AddSample(remoteEP.Address, double.MaxValue);

                            throw;
                        }

                        var response = (HttpStream.HttpResponseHeader)header;

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            if (writeCache)
                            {
                                cache.Commit();
                            }
                        }
                        else if (doIPPool)
                        {
                            if (request.Location.IndexOf("latest") != -1)
                            {
                                //the latest index should always be good, this IP is bad
                                ipPool.AddSample(remoteEP.Address, double.MaxValue);
                                if (!doSwap)
                                    count = 9; //force triggering swap
                            }
                        }

                        if (doIPPool)
                        {
                            double ms = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;
                            if (ms > 0)
                            {
                                if (response.StatusCode == HttpStatusCode.OK)
                                    ipPool.AddSample(remoteEP.Address, bytes / ms);
                                else
                                    ipPool.AddSample(remoteEP.Address, bytes / ms * 2);
                            }

                            if (++count == 10)
                            {
                                var ip = ipPool.GetIP();

                                if (ip == remoteEP.Address)
                                {
                                    count = 0;
                                }
                                else
                                {
                                    doSwap = true;

                                    taskSwap = new Task<IPAddress>(
                                        delegate
                                        {
                                            var clientSwap = this.clientSwap = new TcpClient()
                                            {
                                                ReceiveTimeout = CONNECTION_TIMEOUT_SECONDS * 1000,
                                                SendTimeout = CONNECTION_TIMEOUT_SECONDS * 1000,
                                                ReceiveBufferSize = BUFFER_LENGTH * 2,
                                            };
                                            try
                                            {
                                                if (!clientSwap.ConnectAsync(ip, Settings.PatchingUseHttps.Value ? 443 : 80).Wait(CONNECTION_TIMEOUT_SECONDS * 1000))
                                                {
                                                    throw new TimeoutException();
                                                }

                                                return null;
                                            }
                                            catch (Exception e)
                                            {
                                                Util.Logging.Log(e);
                                                clientSwap.Close();

                                                return ip;
                                            }
                                        });
                                    taskSwap.Start();
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (cache != null)
                            cache.Dispose();
                    }

                    var keepAlive = ((HttpStream.HttpResponseHeader)header).KeepAlive;
                    if (keepAlive.keepAlive)
                    {
                        if (keepAlive.timeout > 0)
                        {
                            clientIn.Client.ReceiveTimeout = keepAlive.timeout * 1000;
                            clientOut.Client.ReceiveTimeout = keepAlive.timeout * 1000;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);

                if (Error != null)
                    Error(this, e);
            }
            finally
            {
                if (bpsShare != null)
                    bpsShare.Dispose();

                if (clientIn != null)
                    clientIn.Close();
                if (clientOut != null)
                    clientOut.Close();
                if (clientSwap != null)
                    clientSwap.Close();

                if (Closed != null)
                    Closed(this, EventArgs.Empty);
            }
        }
    }
}
