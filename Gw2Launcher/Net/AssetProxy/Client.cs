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
        private const int BUFFER_LENGTH = 1048576;
        private const string ASSET_HOST = Settings.ASSET_HOST;
        protected const int CONNECTION_TIMEOUT_MS = 10 * 1000;
        protected const int CONNECTION_TRANSFER_TIMEOUT_MS = 180 * 1000;
        protected const int CONNECTION_KEEPALIVE_TIMEOUT_MS = 120 * 1000;
        protected const int CONNECTION_REMOTE_KEEPALIVE_TIMEOUT_MS = 60 * 1000;
        protected const int CONNECT_RETRY_COUNT = 2;
        protected const int CORRUPT_RETRY_ALTERNATE = 2; //number of attempts until alternatives are used
        protected const int CORRUPT_RETRY_MAX = 5; //maximum attempts until the corrupt file is used

        private static ushort nextId;
        private static readonly object _lock;
        private static BpsLimiter bpsLimiter;
        private static CorruptedRequests corrupted;

        private TcpClient clientIn;
        private TcpClientSocket clientOut, clientSwap;
        private Task task;
        private IPPool ipPool;
        private ushort id;
        private IPPool.IAddress address;

        private class CorruptedRequests
        {
            public class RequestInfo
            {
                public int ticks;
                public byte counter;
                public HashSet<IPAddress> addresses;

                public RequestInfo()
                {
                    addresses = new HashSet<IPAddress>();
                }

                public int Age
                {
                    get
                    {
                        return Environment.TickCount - ticks;
                    }
                }

                public bool IsCorrupted
                {
                    get
                    {
                        return counter > 0 && Age < 300000;
                    }
                }

                public void Add(IPAddress address)
                {
                    lock (this)
                    {
                        addresses.Add(address);
                    }
                }

                public bool Contains(IPAddress address)
                {
                    lock(this)
                    {
                        return addresses.Contains(address);
                    }
                }
            }

            private Dictionary<string, RequestInfo> requests;

            public CorruptedRequests()
            {
            }

            public bool Get(string request, out RequestInfo r)
            {
                if (request != null)
                {
                    lock (this)
                    {
                        if (requests.TryGetValue(request, out r))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    r = null;
                }

                return false;
            }

            public void Remove(string request)
            {
                if (requests != null)
                {
                    lock (this)
                    {
                        requests.Remove(request);
                    }
                }
            }

            public RequestInfo Add(string request, IPAddress address)
            {
                lock (this)
                {
                    RequestInfo r;

                    if (requests == null)
                    {
                        requests = new Dictionary<string, RequestInfo>();
                        r = null;
                    }
                    else
                    {
                        requests.TryGetValue(request, out r);
                    }

                    if (r == null)
                    {
                        requests[request] = r = new RequestInfo();
                    }

                    r.Add(address);
                    r.ticks = Environment.TickCount;
                    ++r.counter;

                    return r;
                }
            }

            public int Count
            {
                get
                {
                    if (requests != null)
                        return requests.Count;
                    return 0;
                }
            }

            public void Clear()
            {
                lock (this)
                {
                    if (requests != null)
                    {
                        requests.Clear();
                        requests = null;
                    }
                }
            }
        }

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
            corrupted = null;
        }

        /// <summary>
        /// Accepts the client
        /// </summary>
        /// <param name="ipPool">Addresses to use for remote connections</param>
        /// <param name="address">Option address to use</param>
        public Client(TcpClient client, IPPool ipPool, IPPool.IAddress address = null)
        {
            lock (_lock)
            {
                id = nextId++;
            }

            this.ipPool = ipPool;
            this.address = address;
            
            clientIn = client;
            clientIn.ReceiveTimeout = CONNECTION_KEEPALIVE_TIMEOUT_MS;
            clientIn.SendTimeout = CONNECTION_TIMEOUT_MS;
            clientIn.SendBufferSize = BUFFER_LENGTH;
            clientIn.NoDelay = true;
        }

        public void Dispose()
        {
            Close();

            if (task != null && task.IsCompleted)
                task.Dispose();
        }

        public IPEndPoint RemoteEP
        {
            get
            {
                return address != null ? address.IP : null;
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
            {
                clientIn.Close();
                using (clientIn.Client) { }
                clientIn = null;
            }

            if (clientSwap != null)
            {
                clientSwap.Dispose();
                clientSwap = null;
            }
            if (clientOut != null)
            {
                clientOut.Dispose();
                clientOut = null;
            }
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
            var alternate = false;
            CorruptedRequests.RequestInfo ri = null;
            BpsLimiter.BpsShare bpsShare = null;
            IPEndPoint remoteEP;

            try
            {
                remoteEP = address != null ? address.GetAddress(Settings.PatchingOptions.Value.HasFlag(Settings.PatchingFlags.UseHttps) ? 443 : 80) : null;

                if (bpsLimiter != null)
                    bpsShare = bpsLimiter.GetShare();

                var buffer = new byte[BUFFER_LENGTH];
                int read;

                Stream stream;
                HttpStream httpIn = new HttpStream(stream = clientIn.GetStream());
                HttpStream httpOut = null;

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
                            var readOnly = Settings.PatchingOptions.Value.HasFlag(Settings.PatchingFlags.DisableCaching);

                            cache = Cache.GetCache(request.Location, readOnly);

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
                                else if (readOnly)
                                {
                                    cache.Dispose();
                                    cache = null;
                                }
                                else
                                {
                                    writeCache = cache.CanWrite;
                                }
                            }
                        }

                        #endregion

                        #region Corrupted requests

                        if (corrupted != null && corrupted.Get(request.Location, out ri) && ri.IsCorrupted)
                        {
                            //file was previously corrupted, try alternatives

                            Func<IPAddress, bool> skip = delegate(IPAddress ip)
                            {
                                return ri.Contains(ip);
                            };

                            address = ipPool.Alternate.GetAddress(skip);

                            if (address == null)
                            {
                                //all available IPs are corrupted
                                //origincdn is restricted (as of may 2023)

                                address = ipPool.GetAddress(skip);

                                if (address == null)
                                {
                                    if (Util.Logging.Enabled)
                                    {
                                        Util.Logging.LogEvent(null, "All addresses ("+ri.addresses.Count+") were corrupt for [" + request.Location + "]");
                                    }

                                    address = ipPool.GetAddress();
                                }
                            }

                            var ep = address.GetAddress(Settings.PatchingOptions.Value.HasFlag(Settings.PatchingFlags.UseHttps) ? 443 : 80);

                            if (remoteEP != ep)
                            {
                                remoteEP = ep;

                                if (!alternate)
                                {
                                    alternate = true;

                                    if (clientOut != null)
                                    {
                                        clientSwap = clientOut;
                                        clientOut = null;
                                    }
                                }
                                else if (clientOut != null)
                                {
                                    clientOut.Dispose();
                                    clientOut = null;
                                }
                            }
                        }

                        #endregion

                        if (read > 0 && RequestDataReceived != null)
                            RequestDataReceived(this, new ArraySegment<byte>(buffer, 0, read));

                        var ofs = read;
                        var attempt = 0;

                        do
                        {
                            //read any remaining request data (not normally used)

                            read = httpIn.Read(buffer, ofs, BUFFER_LENGTH - ofs);

                            if (read > 0)
                            {
                                ofs += read;
                                if (RequestDataReceived != null)
                                    RequestDataReceived(this, new ArraySegment<byte>(buffer, ofs, read));
                            }
                            else
                            {
                                break;
                            }
                        }
                        while (true);

                        HttpStream.HttpResponseHeader response;

                        do
                        {
                            #region Remote connection

                            if (clientOut == null || !clientOut.Connected || Environment.TickCount - clientOut.LastUsed > CONNECTION_REMOTE_KEEPALIVE_TIMEOUT_MS)
                            {
                                var t = Environment.TickCount;

                                for (var i = 0; ; ++i)
                                {
                                    try
                                    {
                                        using (clientOut) { }

                                        clientOut = new TcpClientSocket(new TcpClient()
                                        {
                                            ReceiveTimeout = CONNECTION_TIMEOUT_MS,
                                            SendTimeout = CONNECTION_TIMEOUT_MS,
                                            ReceiveBufferSize = BUFFER_LENGTH,
                                        });

                                        if (i > 0 || remoteEP == null)
                                        {
                                            address = alternate ? ipPool.Alternate.GetAddress() : ipPool.GetAddress();
                                            remoteEP = address.GetAddress(Settings.PatchingOptions.Value.HasFlag(Settings.PatchingFlags.UseHttps) ? 443 : 80);
                                        }

                                        if (!clientOut.Connect(remoteEP, CONNECTION_TIMEOUT_MS))
                                        {
                                            //retry a few times, but GW2 will drop the connection if it takes too long

                                            clientOut.Dispose();

                                            if (address != null)
                                            {
                                                address.Error();
                                                address = null;
                                            }

                                            if (i == CONNECT_RETRY_COUNT || Environment.TickCount - t > 30000 || !clientIn.Connected)
                                            {
                                                throw new TimeoutException("Unable to connect to " + remoteEP.ToString());
                                            }
                                        }
                                        else
                                        {
                                            clientOut.Address = address;

                                            break;
                                        }
                                    }
                                    catch
                                    {
                                        if (address != null)
                                            address.Error();
                                        else
                                            throw;

                                        if (i <= 0)
                                            throw;
                                    }
                                }

                                if (httpOut == null)
                                {
                                    if (remoteEP.Port == 443)
                                        httpOut = new HttpStream(clientOut.Client.GetStream(), ASSET_HOST, false);
                                    else
                                        httpOut = new HttpStream(clientOut.Client.GetStream());
                                }
                                else
                                {
                                    if (remoteEP.Port == 443)
                                        httpOut.SetBaseStream(clientOut.Client.GetStream(), true, ASSET_HOST, false);
                                    else
                                        httpOut.SetBaseStream(clientOut.Client.GetStream(), true);
                                }
                            }
                            else
                            {
                                clientOut.SendReceiveTimeout = CONNECTION_TIMEOUT_MS;
                            }

                            #endregion

                            //keeping request in the buffer in case a new request is needed
                            httpOut.Write(buffer, 0, ofs);

                            if ((read = httpOut.ReadHeader(buffer, ofs, out header)) == 0)
                            {
                                var t = Environment.TickCount;

                                do
                                {
                                    try
                                    {
                                        if (!clientOut.Connected || (read = httpOut.ReadHeader(buffer, ofs, out header, true)) == 0)
                                        {
                                            //connection closed
                                            return;
                                        }
                                    }
                                    catch
                                    {
                                        //receive timeout
                                    }

                                    if (Environment.TickCount - t > 30000)
                                        return;
                                }
                                while (true);
                            }

                            if (ResponseHeaderReceived != null)
                                ResponseHeaderReceived(this, (HttpStream.HttpResponseHeader)header);

                            if (read > 0 && ResponseDataReceived != null)
                                ResponseDataReceived(this, new ArraySegment<byte>(buffer, ofs, read));

                            response = (HttpStream.HttpResponseHeader)header;

                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    
                                    int size;
                                    var contentLength = response.ContentLength;

                                    if (contentLength > 0 && int.TryParse(response.Headers["X-Arena-Checksum-Length"], out size) && size > 0 && size != contentLength)
                                    {
                                        //file is corrupted, retry instead of passing it to GW2 (GW2 will give up after too many failures)

                                        if (Util.Logging.Enabled)
                                        {
                                            Util.Logging.LogEvent(null, "Checksum length doesn't match header for [" + request.Location + "] from [" + remoteEP.ToString() + "] (length: " + response.ContentLength + ", expected: " + size + ")");
                                        }

                                        if (++attempt <= CORRUPT_RETRY_MAX)
                                        {
                                            IPEndPoint ep;

                                            if (corrupted == null)
                                            {
                                                lock (_lock)
                                                {
                                                    if (corrupted == null)
                                                        corrupted = new CorruptedRequests();
                                                }
                                            }

                                            ri = corrupted.Add(request.Location, remoteEP.Address);

                                            Func<IPAddress, bool> skip = delegate(IPAddress ip)
                                            {
                                                return ri.Contains(ip);
                                            };

                                            bool alt;

                                            if (alt = (attempt > CORRUPT_RETRY_ALTERNATE || (address = ipPool.GetAddress(skip)) == null))
                                            {
                                                address = ipPool.Alternate.GetAddress(skip);
                                            }

                                            if (address != null)
                                            {
                                                ep = address.GetAddress(Settings.PatchingOptions.Value.HasFlag(Settings.PatchingFlags.UseHttps) ? 443 : 80);

                                                if (alt && !alternate)
                                                {
                                                    alternate = true;

                                                    if (clientOut != null)
                                                    {
                                                        clientSwap = clientOut;
                                                        clientOut = null;
                                                    }
                                                }
                                                else
                                                {
                                                    using (clientOut) { }
                                                    clientOut = null;
                                                }

                                                alternate = alt;
                                                remoteEP = ep;

                                                continue;
                                            }
                                            else
                                            {
                                                if (Util.Logging.Enabled)
                                                {
                                                    Util.Logging.LogEvent(null, "All addresses (" + (ri.addresses.Count) + ") were corrupt for [" + request.Location + "]");
                                                }
                                            }
                                        }
                                    }
                                    else if (address != null)
                                    {
                                        address.OK();
                                    }

                                    break;
                                case HttpStatusCode.NotFound:
                                    
                                    //delta patches for old files may not be available
                                    if (address != null)
                                        address.OK();

                                    break;
                            }

                            break;
                        }
                        while (true);

                        clientOut.SendReceiveTimeout = CONNECTION_TRANSFER_TIMEOUT_MS;

                        var connected = true;
                        var startTime = Environment.TickCount;
                        long bytes = 0;

                        try
                        {
                            //cache will continue to be written if the client disconnects

                            if (read > 0)
                            {
                                if (writeCache)
                                    cache.Write(buffer, ofs, read);

                                if (connected)
                                {
                                    try
                                    {
                                        httpIn.Write(buffer, ofs, read);
                                    }
                                    catch
                                    {
                                        connected = false;
                                        if (!writeCache)
                                            return;
                                    }
                                }

                                read = 0;
                                ofs = 0;
                            }

                            do
                            {
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

                                if (read > 0)
                                {
                                    if (writeCache)
                                        cache.Write(buffer, 0, read);

                                    if (connected)
                                    {
                                        try
                                        {
                                            httpIn.Write(buffer, 0, read);
                                        }
                                        catch
                                        {
                                            connected = false;
                                            if (!writeCache)
                                                return;
                                        }
                                    }
                                }
                                else
                                    break;
                            }
                            while (true);

                            if (!httpOut.EndOfContent)
                            {
                                //was disconnected before finishing
                                return;
                            }
                        }
                        catch (IOException)
                        {
                            throw;
                        }
                        catch
                        {
                            if (connected && address != null)
                            {
                                address.Error();
                            }

                            throw;
                        }

                        clientOut.LastUsed = Environment.TickCount;

                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:

                                if (address != null)
                                {
                                    var ms = Environment.TickCount - startTime;
                                    if (ms > 0)
                                    {
                                        address.Sample((double)bytes / ms);
                                    }
                                }
                                
                                var ok = true;
                                int size;

                                if (int.TryParse(response.Headers["X-Arena-Checksum-Length"], out size) && size > 0)
                                {
                                    if (httpOut.ContentLengthProcessed != size)
                                    {
                                        ok = false;

                                        //file is corrupted
                                        //origincdn can throw errors or drop the connection while downloading, causing the CDN to cache error pages or incomplete files
                                        //note behind each assetcdn IP is multiple servers (30+), some of which may not be corrupted, but most usually are

                                        if (corrupted == null)
                                        {
                                            lock (_lock)
                                            {
                                                if (corrupted == null)
                                                    corrupted = new CorruptedRequests();
                                            }
                                        }

                                        corrupted.Add(request.Location, remoteEP.Address);

                                        if (Util.Logging.Enabled)
                                        {
                                            Util.Logging.LogEvent(null, "Checksum length doesn't match content for [" + request.Location + "] from [" + remoteEP.ToString() + "] (length: " + response.ContentLength + ", expected: " + size + ")");
                                        }
                                    }
                                    else
                                    {
                                        if (ri != null)
                                        {
                                            corrupted.Remove(request.Location);
                                            ri = null;
                                        }

                                        //should always occur unless corrupt
                                        if (alternate)
                                        {
                                            if (clientSwap != null)
                                            {
                                                remoteEP = clientSwap.EndPoint;
                                                address = clientSwap.Address;

                                                clientOut.Dispose();
                                                clientOut = clientSwap;
                                                clientSwap = null;
                                            }
                                            else
                                            {
                                                clientOut.Close();
                                            }

                                            alternate = false;
                                        }
                                    }
                                }

                                if (writeCache && ok)
                                {
                                    cache.Commit();
                                }

                                break;
                            case HttpStatusCode.NotFound:

                                //not found should only happen for delta patches
                                var n = Path.GetFileName(request.Location);

                                if (n[0] != '0' || !char.IsDigit(n[0]))
                                {
                                    if (address != null)
                                        address.Error();
                                }

                                break;
                            case HttpStatusCode.Forbidden:

                                //IP is probably outdated and no longer hosting assetcdn

                            default:

                                if (address != null)
                                    address.Error();

                                if (alternate)
                                    using (clientOut) { }

                                break;
                        }

                        if (!connected)
                            return;

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
                            if (clientOut != null)
                                clientOut.ReceiveTimeout = keepAlive.timeout * 1000;
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

                Close();

                if (Closed != null)
                    Closed(this, EventArgs.Empty);
            }
        }
    }
}
