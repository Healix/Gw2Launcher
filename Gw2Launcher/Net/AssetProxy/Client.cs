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
    class Client
    {
        private const int BUFFER_LENGTH = 65536;
        private const int TIMEOUT = 5000;

        private static ushort nextId;
        private static readonly object _lock;

        private TcpClient clientIn, clientOut, clientSwap;
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
                remote = new IPEndPoint(ipPool.GetIP(), 80);
            }

            remoteEP = remote;

            clientIn = client;
            clientIn.ReceiveTimeout = TIMEOUT;
            clientIn.SendTimeout = TIMEOUT;
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
            Task.Factory.StartNew(DoClient, TaskCreationOptions.LongRunning);
        }

        private void DoClient()
        {
            int count = 0;

            try
            {
                var buffer = new byte[BUFFER_LENGTH];
                int read;

                HttpStream httpIn = new HttpStream(clientIn.GetStream());
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
                                    httpOut.BaseStream = clientOut.GetStream();
                                }

                                doSwap = false;
                                count = 0;
                                taskSwap.Dispose();
                            }
                        }

                        if (clientOut == null || !clientOut.Connected)
                        {
                            clientOut = new TcpClient();
                            clientOut.ReceiveTimeout = TIMEOUT;
                            clientOut.SendTimeout = TIMEOUT;

                            try
                            {
                                if (!clientOut.ConnectAsync(remoteEP.Address, remoteEP.Port).Wait(TIMEOUT))
                                {
                                    throw new TimeoutException("Unable to connect to " + remoteEP.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                if (doIPPool)
                                    ipPool.AddSample(remoteEP.Address, double.MaxValue);

                                throw ex;
                            }

                            httpOut = new HttpStream(clientOut.GetStream());
                        }

                        #endregion

                        if (read > 0 && RequestDataReceived != null)
                            RequestDataReceived(this, new ArraySegment<byte>(buffer, 0, read));

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

                        DateTime startTime = DateTime.UtcNow;
                        long bytes = 0;

                        try
                        {
                            do
                            {
                                if (writeCache)
                                    cache.Write(buffer, 0, read);

                                httpIn.Write(buffer, 0, read);
                                read = httpOut.Read(buffer, 0, BUFFER_LENGTH);
                                bytes += read;

                                if (read > 0 && ResponseDataReceived != null)
                                    ResponseDataReceived(this, new ArraySegment<byte>(buffer, 0, read));
                            }
                            while (read > 0);
                        }
                        catch (Exception ex)
                        {
                            if (doIPPool)
                                ipPool.AddSample(remoteEP.Address, double.MaxValue);

                            throw ex;
                        }

                        var response = (HttpStream.HttpResponseHeader)header;

                        if (writeCache && response.StatusCode == HttpStatusCode.OK)
                        {
                            if (request.Location.StartsWith("/latest", StringComparison.OrdinalIgnoreCase))
                                cache.Expires = DateTime.UtcNow.AddMinutes(1);
                            else
                                cache.Expires = DateTime.UtcNow.AddHours(12);

                            cache.Commit();
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

                                    taskSwap = Task.Run<IPAddress>(
                                        delegate
                                        {
                                            var clientSwap = this.clientSwap = new TcpClient();
                                            clientSwap.ReceiveTimeout = clientSwap.SendTimeout = TIMEOUT;
                                            try
                                            {
                                                if (!clientSwap.ConnectAsync(ip, 80).Wait(TIMEOUT))
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
