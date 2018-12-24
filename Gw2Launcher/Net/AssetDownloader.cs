using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Gw2Launcher.Net
{
    class AssetDownloader : IDisposable
    {
        public delegate void RequestCompleteEventHandler(object sender, RequestCompleteEventArgs e);
        public delegate void ErrorEventHandler(object sender, Asset asset, Exception exception);

        public class RequestCompleteEventArgs : EventArgs
        {
            public string Location
            {
                get;
                set;
            }

            public HttpStatusCode StatusCode
            {
                get;
                set;
            }

            public long ContentBytes
            {
                get;
                set;
            }

            public Asset Asset
            {
                get;
                set;
            }
        }

        public event EventHandler Complete;
        public event ErrorEventHandler Error;
        public event EventHandler<uint> BytesDownloaded;
        public event EventHandler<uint> DownloadRate;
        public event RequestCompleteEventHandler RequestComplete;
        public event EventHandler Tick;
        private event EventHandler<Worker> WorkerAdded;

        protected const byte CONNECTION_TIMEOUT_SECONDS = 5;
        protected const byte CONNECTION_TRANSFER_TIMEOUT_SECONDS = 180;
        protected const byte RETRY_TIMEOUT_SECONDS = 5;
        protected const string ASSET_HOST = Settings.ASSET_HOST;
        protected const string COOKIE = Settings.ASSET_COOKIE;

        public class Asset
        {
            private const string URL_FILE_BASE = "/program/101/1/";
            private const string URL_PATH = URL_FILE_BASE + "{0}/{1}";
            private const string URL_PATH_COMPRESSED = URL_PATH + "/compressed";
            private const string URL_LATEST = "/latest{0}/101";

            public class CompleteEventArgs : EventArgs, IDisposable
            {
                private bool used;
                private Net.AssetProxy.Cache.CacheStream cache;
                private string request;
                private long contentLength;

                public CompleteEventArgs(string request, long contentLength)
                {
                    this.cache = Net.AssetProxy.Cache.GetCache(request);
                    this.request = request;
                    this.contentLength = contentLength;
                }

                public string Request
                {
                    get
                    {
                        return request;
                    }
                }

                public Net.AssetProxy.Cache.CacheStream GetCache()
                {
                    used = true;
                    return cache;
                }

                public long ContentLength
                {
                    get
                    {
                        return contentLength;
                    }
                }

                public void Dispose()
                {
                    if (!used)
                    {
                        using (cache) { }
                    }
                }
            }

            public class ProgressEventArgs
            {
                public long total, downloaded;
                public int processed, sizeChange;
            }

            public event EventHandler<CompleteEventArgs> Complete;
            public event EventHandler Cancelled;
            public event EventHandler<ProgressEventArgs> Progress;

            public enum AssetType
            {
                File,
                FileCompressed,
                Latest32,
                Latest64
            }

            public Asset(AssetType type)
            {
                this.type = type;
            }

            public Asset(int fileId, bool compressed, int size)
            {
                this.fileId = fileId;
                this.type = compressed ? AssetType.FileCompressed : AssetType.File;
                this.size = size;
            }

            public Asset(int baseId, int fileId, int size)
            {
                this.baseId = baseId;
                this.fileId = fileId;
                this.type = AssetType.File;
                this.size = size;
            }

            public int baseId, fileId, size;
            public AssetType type;

            public void OnComplete(CompleteEventArgs e)
            {
                if (Complete != null)
                    Complete(this, e);
            }

            public void OnCancelled()
            {
                if (Cancelled != null)
                    Cancelled(this, EventArgs.Empty);
            }

            public void OnProgress(ProgressEventArgs e)
            {
                if (Progress != null)
                    Progress(this, e);
            }

            public static string GetRequest(Asset asset)
            {
                if (asset.type == Asset.AssetType.File)
                    return string.Format(URL_PATH, asset.baseId, asset.fileId);
                else if (asset.type == Asset.AssetType.FileCompressed)
                    return string.Format(URL_PATH_COMPRESSED, asset.baseId, asset.fileId);
                else if (asset.type == Asset.AssetType.Latest32)
                    return string.Format(URL_LATEST, "");
                else if (asset.type == Asset.AssetType.Latest64)
                    return string.Format(URL_LATEST, "64");

                throw new NotSupportedException();
            }
        }

        private class SharedWork
        {
            public Queue<Asset> queue;
            public ManualResetEvent waiter;
            public bool abort;
            public bool keepalive;
            public byte threads;
            public bool cancel;
            public AssetDownloader owner;
            //public uint bps; //bytes per second limit
            public BpsLimiter limiter;
        }

        private class Worker : IDisposable
        {
            public event ErrorEventHandler Error;
            public event EventHandler Complete;
            public event EventHandler<int> BytesDownloaded;
            public event RequestCompleteEventHandler RequestComplete;
            public event EventHandler RequestBegin;

            public const int BUFFER_LENGTH = 16384;

            private TcpClient client, clientSwap;
            private AssetProxy.HttpStream stream;
            private IPEndPoint remoteEP;
            private SharedWork work;
            private Thread thread;
            private bool stop;

            public Worker(SharedWork work)
            {
                this.work = work;
            }

            private void WriteHeader(Stream stream, string host, string request)
            {
                string header = "GET " + request + " HTTP/1.1\r\nCookie: " + COOKIE + "\r\nHost: " + host + "\r\nConnection: keep-alive\r\nContent-Length: 0\r\n\r\n";
                byte[] buffer = Encoding.ASCII.GetBytes(header);
                stream.Write(buffer, 0, buffer.Length);
            }

            public IPEndPoint RemoteEP
            {
                get
                {
                    return remoteEP;
                }
            }

            /// <summary>
            /// The worker will be stopped once the current item is complete
            /// </summary>
            public void Stop()
            {
                stop = true;
                work.waiter.Set();
            }

            public void Close()
            {
                if (clientSwap != null)
                {
                    clientSwap.Close();
                    clientSwap = null;
                }
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
                if (client != null)
                {
                    client.Close();
                    client = null;
                }
            }

            private bool RetryNotFound(Asset asset)
            {
                //partial files fall back to full compressed, then uncompressed

                if (asset.type == Asset.AssetType.File && asset.baseId != 0 || asset.type == Asset.AssetType.FileCompressed)
                {
                    asset.type = asset.baseId != 0 ? Asset.AssetType.FileCompressed : Asset.AssetType.File;
                    asset.baseId = 0;

                    return true;
                }

                return false;
            }

            private void DoWork()
            {
                var work = this.work;

                byte[] buffer = new byte[BUFFER_LENGTH];
                bool doSwap = false;
                Task<IPAddress> taskSwap = null;
                int connectionTimeout = CONNECTION_TIMEOUT_SECONDS * 1000;
                int counter = 0;
                var timeout = DateTime.UtcNow.AddMilliseconds(connectionTimeout);
                Asset asset = null;
                BpsLimiter.BpsShare bpsShare = null;

                try
                {
                    while (!work.abort && !stop)
                    {
                        asset = null;

                        lock (work)
                        {
                            if (work.queue.Count > 0)
                            {
                                asset = work.queue.Dequeue();
                            }
                            else
                            {
                                work.waiter.Reset();
                                asset = null;
                            }
                        }

                        if (asset == null)
                        {
                            if (work.keepalive)
                            {
                                if (!work.waiter.WaitOne(30000))
                                {
                                    lock (work)
                                    {
                                        if (work.queue.Count > 0)
                                            continue;
                                    }
                                    return;
                                }

                                continue;
                            }
                            else
                                return;
                        }
                        else if (work.cancel)
                        {
                            asset.OnCancelled();
                            continue;
                        }

                        if (DateTime.UtcNow > timeout)
                            Close();

                        Asset.CompleteEventArgs complete = null;

                        long mismatchLength = -1;
                        byte retry = 10;
                        do
                        {
                            string request = Asset.GetRequest(asset);
                            var cache = AssetProxy.Cache.GetCache(request);

                            using (cache)
                            {
                                //skipping cache files that have already been written or are in use

                                if (cache == null)
                                {
                                    throw new Exception("Cache not available");
                                }
                                else if (cache.CanWrite)
                                {
                                    int port = Settings.PatchingUseHttps.Value ? 443 : 80;

                                    #region Remote connection

                                    if (doSwap)
                                    {
                                        if (taskSwap.IsCompleted)
                                        {
                                            if (taskSwap.Result != null)
                                            {
                                                work.owner.ipPool.AddSample(taskSwap.Result, double.MaxValue);
                                            }
                                            else if (clientSwap != null && clientSwap.Connected)
                                            {
                                                if (client != null)
                                                    client.Close();
                                                client = clientSwap;
                                                clientSwap = null;
                                                remoteEP = (IPEndPoint)client.Client.RemoteEndPoint;

                                                if (port == 443)
                                                    stream.SetBaseStream(client.GetStream(), true, ASSET_HOST, false);
                                                else
                                                    stream.SetBaseStream(client.GetStream(), true);
                                            }

                                            doSwap = false;
                                            counter = 0;
                                            taskSwap.Dispose();
                                        }
                                    }

                                    if (client == null || !client.Connected)
                                    {
                                        client = new TcpClient()
                                        {
                                            ReceiveTimeout = connectionTimeout,
                                            SendTimeout = connectionTimeout
                                        };

                                        try
                                        {
                                            remoteEP = new IPEndPoint(work.owner.ipPool.GetIP(), port);
                                            for (byte attempt = 10; attempt > 0; attempt--)
                                            {
                                                if (!client.ConnectAsync(remoteEP.Address, remoteEP.Port).Wait(connectionTimeout))
                                                {
                                                    client.Close();

                                                    if (work.abort || attempt == 1)
                                                        throw new TimeoutException("Unable to connect to host");

                                                    work.owner.ipPool.AddSample(remoteEP.Address, double.MaxValue);
                                                    Thread.Sleep(100);
                                                }
                                                else
                                                    break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Util.Logging.Log(ex);

                                            work.owner.ipPool.AddSample(remoteEP.Address, double.MaxValue);

                                            if (Error != null)
                                            {
                                                if (ex.InnerException != null)
                                                    Error(this, asset, ex.InnerException);
                                                else
                                                    Error(this, asset, ex);
                                            }

                                            return;
                                        }

                                        if (port == 443)
                                            stream = new AssetProxy.HttpStream(client.GetStream(), ASSET_HOST, false);
                                        else
                                            stream = new AssetProxy.HttpStream(client.GetStream());
                                    }

                                    #endregion

                                    #region Swap

                                    if (++counter == 10)
                                    {
                                        var ip = work.owner.ipPool.GetIP();

                                        if (ip == remoteEP.Address)
                                        {
                                            counter = 0;
                                        }
                                        else
                                        {
                                            doSwap = true;

                                            taskSwap = new Task<IPAddress>(
                                                delegate
                                                {
                                                    var clientSwap = this.clientSwap = new TcpClient();
                                                    clientSwap.ReceiveTimeout = clientSwap.SendTimeout = connectionTimeout;
                                                    try
                                                    {
                                                        if (!clientSwap.ConnectAsync(ip, port).Wait(connectionTimeout))
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

                                    #endregion

                                    if (RequestBegin != null)
                                        RequestBegin(this, EventArgs.Empty);

                                    if (work.limiter != null && bpsShare == null)
                                        bpsShare = work.limiter.GetShare();

                                    try
                                    {
                                        client.ReceiveTimeout = client.SendTimeout = connectionTimeout;

                                        WriteHeader(stream, ASSET_HOST, request);

                                        Net.AssetProxy.HttpStream.HttpHeader header;
                                        int read;

                                        var startTime = DateTime.UtcNow;
                                        long total = read = stream.ReadHeader(buffer, 0, out header);

                                        if (read <= 0)
                                            throw new EndOfStreamException();

                                        //extending the timeout once data has already been received
                                        client.ReceiveTimeout = client.SendTimeout = CONNECTION_TRANSFER_TIMEOUT_SECONDS * 1000;

                                        var response = (Net.AssetProxy.HttpStream.HttpResponseHeader)header;
                                        //uint bps = work.bps / work.threads;
                                        //int bpsSample = 0;
                                        //var bpsReset = DateTime.UtcNow.AddSeconds(1);
                                        //int length = BUFFER_LENGTH;

                                        var progress = new Asset.ProgressEventArgs()
                                        {
                                            total = response.ContentLength
                                        };

                                        while (read > 0)
                                        {
                                            if (BytesDownloaded != null)
                                                BytesDownloaded(this, read);

                                            if (progress.total > 0)
                                            {
                                                progress.downloaded = stream.ContentLengthProcessed;

                                                if (asset.size > 0)
                                                {
                                                    var processed = (int)((double)progress.downloaded / progress.total * asset.size);
                                                    progress.sizeChange = processed - progress.processed;
                                                    progress.processed = processed;
                                                }

                                                asset.OnProgress(progress);
                                            }

                                            cache.Write(buffer, 0, read);

                                            #region speed limit (old)

                                            //if (bps > 0 && read > 0)
                                            //{
                                            //    if (DateTime.UtcNow >= bpsReset)
                                            //    {
                                            //        bps = work.bps / work.threads;
                                            //        bpsSample = 0;
                                            //        bpsReset = DateTime.UtcNow.AddSeconds(1);
                                            //    }
                                            //    else
                                            //    {
                                            //        bpsSample += read;
                                            //        if (bpsSample >= bps)
                                            //        {
                                            //            var t = (int)(bpsReset.Subtract(DateTime.UtcNow).TotalMilliseconds + 0.5);
                                            //            if (t > 0)
                                            //                Thread.Sleep(t);
                                            //            bps = work.bps / work.threads;
                                            //            bpsSample = 0;
                                            //            bpsReset = DateTime.UtcNow.AddSeconds(1);
                                            //        }
                                            //        else
                                            //        {
                                            //            length = (int)(bps - bpsSample);
                                            //            if (length > BUFFER_LENGTH)
                                            //                length = BUFFER_LENGTH;
                                            //        }
                                            //    }
                                            //}

                                            #endregion

                                            if (bpsShare != null)
                                            {
                                                read = stream.Read(buffer, 0, bpsShare.GetLimit(BUFFER_LENGTH));
                                                bpsShare.Used(read);
                                            }
                                            else
                                                read = stream.Read(buffer, 0, BUFFER_LENGTH);


                                            total += read;
                                        }

                                        if (progress.total <= 0)
                                        {
                                            progress.total = progress.downloaded = stream.ContentLengthProcessed;
                                            progress.sizeChange = asset.size - progress.processed;
                                            progress.processed = asset.size;
                                            asset.OnProgress(progress);
                                        }
                                        else
                                            progress = null;

                                        var elapsed = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;

                                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
                                        {
                                            if (elapsed > 0)
                                                work.owner.ipPool.AddSample(remoteEP.Address, total / elapsed);
                                        }
                                        else
                                        {
                                            if (elapsed <= 0)
                                                elapsed = 1;
                                            work.owner.ipPool.AddSample(remoteEP.Address, total / elapsed * 2);
                                            throw new Exception("Server returned a bad response");
                                        }

                                        if (response.ContentLength > 0 && stream.ContentLengthProcessed != response.ContentLength)
                                        {
                                            work.owner.ipPool.AddSample(remoteEP.Address, double.MaxValue);
                                            if (mismatchLength == stream.ContentLengthProcessed)
                                                retry = 1;
                                            mismatchLength = stream.ContentLengthProcessed;
                                            throw new Exception("Content length doesn't match header");
                                        }

                                        if (response.StatusCode == HttpStatusCode.NotFound)
                                        {
                                            //some files will return a 404 response, which falls back from patches > compressed > uncompressed
                                            if (RetryNotFound(asset))
                                            {
                                                cache.Commit();
                                                continue;
                                            }
                                        }

                                        timeout = DateTime.UtcNow.AddMilliseconds(connectionTimeout);

                                        if (!response.KeepAlive.keepAlive)
                                            client.Close();

                                        retry = 1;
                                        cache.Commit();

                                        if (RequestComplete != null)
                                        {
                                            var e = new RequestCompleteEventArgs()
                                            {
                                                Location = request,
                                                ContentBytes = stream.ContentLengthProcessed,
                                                StatusCode = response.StatusCode,
                                                Asset = asset
                                            };
                                            RequestComplete(this, e);
                                        }

                                        complete = new Asset.CompleteEventArgs(request, stream.ContentLengthProcessed);

                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);

                                        client.Close();

                                        if (work.abort || --retry == 0)
                                        {
                                            asset.OnCancelled();
                                            if (Error != null)
                                                Error(this, asset, ex);
                                            break;
                                        }
                                        else
                                        {
                                            Thread.Sleep(RETRY_TIMEOUT_SECONDS * 1000);
                                        }
                                    }

                                }
                                else if (cache.HasData)
                                {
                                    using (var stream = new AssetProxy.HttpStream(cache))
                                    {
                                        AssetProxy.HttpStream.HttpHeader header;
                                        var read = stream.ReadHeader(buffer, 0, out header);
                                        if (read > 0)
                                        {
                                            var response = (Net.AssetProxy.HttpStream.HttpResponseHeader)header;
                                            if (response.StatusCode == HttpStatusCode.NotFound)
                                            {
                                                //some assets will not be available, like patches to older files
                                                //fallback: patch > compressed > uncompressed
                                                if (RetryNotFound(asset))
                                                    continue;
                                            }

                                            if (RequestComplete != null)
                                            {
                                                var e = new RequestCompleteEventArgs()
                                                {
                                                    Location = request,
                                                    ContentBytes = response.ContentLength,
                                                    StatusCode = response.StatusCode,
                                                    Asset = asset
                                                };
                                                RequestComplete(this, e);
                                            }

                                            complete = new Asset.CompleteEventArgs(request, response.ContentLength);
                                        }
                                        else
                                            complete = new Asset.CompleteEventArgs(request, 0);

                                        var progress = new Asset.ProgressEventArgs();
                                        progress.sizeChange = progress.processed = asset.size;
                                        asset.OnProgress(progress);
                                    }
                                    break;
                                }
                            }
                        }
                        while (true);

                        if (complete != null)
                        {
                            using (complete)
                            {
                                asset.OnComplete(complete);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);

                    if (asset != null)
                    {
                        asset.OnCancelled();
                        if (Error != null)
                            Error(this, asset, ex);
                    }
                }
                finally
                {
                    Close();

                    if (Complete != null)
                        Complete(this, EventArgs.Empty);
                }
            }

            public bool Start()
            {
                if (thread == null || !thread.IsAlive)
                {
                    thread = new Thread(new ThreadStart(DoWork));
                    thread.IsBackground = true;
                    thread.Start();

                    return true;
                }

                return false;
            }

            public void Abort()
            {
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                    thread = null;
                }
            }

            public void Dispose()
            {
                Close();

                try
                {

                    if (thread != null)
                    {
                        if (thread.IsAlive)
                            thread.Abort();
                        thread = null;
                    }
                }
                catch { }
            }
        }

        private bool isActive;
        private Worker[] workers;
        private SharedWork work;
        private AssetProxy.IPPool ipPool;
        private byte threads;

        private long totalBytesDownloaded;

        public AssetDownloader(byte threads)
            : this(threads, null)
        {

        }

        public AssetDownloader(byte threads, AssetProxy.IPPool ipPool)
        {
            SetIPPool(ipPool);

            var work = this.work = new SharedWork()
            {
                abort = false,
                cancel = false,
                waiter = new ManualResetEvent(true),
                queue = new Queue<Asset>(),
                threads = threads,
                keepalive = true,
                owner = this
            };

            workers = new Worker[this.threads = threads];
        }

        void worker_RequestComplete(object sender, Net.AssetDownloader.RequestCompleteEventArgs e)
        {
            if (RequestComplete != null)
                RequestComplete(this, e);
        }

        void worker_Error(object sender, Asset asset, Exception e)
        {
            if (Error != null)
                Error(this, asset, e);
            work.cancel = true;
        }

        void worker_Complete(object sender, EventArgs e)
        {
            lock (this)
            {
                work.threads--;
            }

            if (work.abort)
            {
                CancelQueue();
            }
        }

        void worker_BytesDownloaded(object sender, int e)
        {
            lock (this)
            {
                totalBytesDownloaded += e;
            }
        }

        public byte Threads
        {
            get
            {
                return this.threads;
            }
            set
            {
                if (this.threads == value)
                    return;

                lock (this)
                {
                    //when not already active, workers will be created on start
                    if (isActive)
                    {
                        if (this.threads > value)
                        {
                            //remove workers
                            for (var i = this.workers.Length - 1; i >= value; i--)
                            {
                                var worker = this.workers[i];
                                if (worker != null)
                                {
                                    worker.Stop();
                                    this.workers[i] = null;
                                }
                            }
                        }
                        else
                        {
                            //add workers
                            //note: not reusing workers that were stopped. Reducing and increasing the limit
                            //will temporarily create more workers beyond the limit while they finish downloading
                            var _workers = new Worker[value];
                            var existing = 0;
                            for (var i = 0; i < this.workers.Length; i++)
                            {
                                var w = this.workers[i];
                                if (w != null)
                                    _workers[existing++] = w;
                            }

                            for (var i = existing; i < value; i++)
                            {
                                Worker worker;
                                _workers[i] = worker = new Worker(work);
                                worker.BytesDownloaded += worker_BytesDownloaded;
                                worker.Complete += worker_Complete;
                                worker.Error += worker_Error;
                                worker.RequestComplete += worker_RequestComplete;

                                worker.Start();
                            }

                            lock (work)
                            {
                                work.threads += (byte)(value - existing);
                            }

                            this.workers = _workers;
                        }

                        this.threads = value;
                    }
                }
            }
        }

        public int BpsLimit
        {
            get
            {
                if (work.limiter == null)
                    return 0;
                return work.limiter.BpsLimit;
            }
            set
            {
                if (work.limiter == null)
                {
                    if (value != Int32.MaxValue)
                    {
                        work.limiter = new BpsLimiter()
                        {
                            BpsLimit = value
                        };
                    }
                }
                else
                {
                    work.limiter.BpsLimit = value;
                }
            }
        }

        public long TotalBytesDownloaded
        {
            get
            {
                return totalBytesDownloaded;
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }

        private async void DoMonitor()
        {
            long totalBytes = 0;
            long lastSample0 = 0;
            long lastSample1 = 0;
            bool reset = false;
            DateTime nextSample0 = DateTime.UtcNow;
            DateTime nextSample1 = DateTime.UtcNow;
            DateTime lastRequest = DateTime.MinValue;

            EventHandler requestBegin =
                delegate
                {
                    if (reset)
                    {
                        nextSample0 = nextSample1 = DateTime.UtcNow;
                        nextSample1 = nextSample0;
                        reset = false;
                    }
                };

            foreach (var worker in workers)
                worker.RequestBegin += requestBegin;

            EventHandler<Worker> workerAdded =
                delegate(object o, Worker w)
                {
                    w.RequestBegin += requestBegin;
                };

            this.WorkerAdded += workerAdded;

            do
            {
                await Task.Delay(1000);

                var l = totalBytesDownloaded;

                if (totalBytes != l)
                {
                    if (BytesDownloaded != null)
                        BytesDownloaded(this, (uint)(l - totalBytes));
                    totalBytes = l;
                }

                var now = DateTime.UtcNow;
                var elapsed = now.Subtract(nextSample0).TotalSeconds;

                if (elapsed > 0)
                {
                    if (l != lastSample0)
                    {
                        double rate = (l - lastSample0) / elapsed;
                        if (DownloadRate != null)
                            DownloadRate(this, (uint)(rate + 0.5));

                        nextSample1 = nextSample0;
                        lastSample1 = lastSample0;
                        nextSample0 = now;
                        lastSample0 = l;
                    }
                    else if (elapsed > 3)
                    {
                        if (!reset)
                        {
                            reset = true;

                            if (DownloadRate != null)
                                DownloadRate(this, 0);
                        }
                    }
                    else
                    {
                        elapsed = now.Subtract(nextSample1).TotalSeconds;
                        if (elapsed > 0)
                        {
                            double rate = (l - lastSample1) / elapsed;
                            if (DownloadRate != null)
                                DownloadRate(this, (uint)(rate + 0.5));
                        }
                    }
                }

                if (Tick != null)
                    Tick(this, EventArgs.Empty);
            }
            while (work.threads > 0 || work.keepalive && !work.abort);

            this.WorkerAdded -= workerAdded;

            lock (this)
            {
                var threads = (byte)workers.Length;
                for (var i = 0; i < threads; i++)
                {
                    var worker = workers[i];
                    if (worker != null)
                    {
                        using (worker)
                        {
                            worker.BytesDownloaded -= worker_BytesDownloaded;
                            worker.Complete -= worker_Complete;
                            worker.Error -= worker_Error;
                            worker.RequestComplete -= worker_RequestComplete;

                            worker.RequestBegin -= requestBegin;

                            workers[i] = null;
                        }
                    }
                }

                if (work.limiter != null && !work.limiter.Enabled)
                {
                    using (work.limiter)
                    {
                        work.limiter = null;
                    }
                }

                isActive = false;
            }

            if (Complete != null)
                Complete(this, EventArgs.Empty);
        }

        public void Start()
        {
            lock (this)
            {
                if (isActive)
                    return;
                isActive = true;
            }

            var threads = this.threads;

            if (workers.Length != threads)
            {
                foreach (var worker in workers)
                    using (worker) { }
                workers = new Worker[threads];
            }

            work.threads = threads;
            work.keepalive = true;
            work.cancel = false;
            work.abort = false;
            work.waiter.Set();

            for (var i = 0; i < threads; i++)
            {
                var worker = workers[i];
                if (worker == null)
                {
                    workers[i] = worker = new Worker(work);
                    worker.BytesDownloaded += worker_BytesDownloaded;
                    worker.Complete += worker_Complete;
                    worker.Error += worker_Error;
                    worker.RequestComplete += worker_RequestComplete;
                }
                worker.Start();
            }

            DoMonitor();
        }

        /// <summary>
        /// Sets the desired IPPool or leave null to auto populate
        /// </summary>
        public void SetIPPool(AssetProxy.IPPool ipPool)
        {
            if (ipPool == null)
            {
                try
                {
                    var ips = Dns.GetHostAddresses(ASSET_HOST);
                    if (ips.Length == 0)
                        throw new IndexOutOfRangeException();

                    var args = Settings.GW2Arguments.Value;
                    if (!string.IsNullOrEmpty(args))
                    {
                        var existing = Util.Args.GetValue(args, "assetsrv");
                        if (!string.IsNullOrEmpty(existing))
                        {
                            IPAddress assetsrv;
                            if (IPAddress.TryParse(existing, out assetsrv))
                            {
                                foreach (var ip in ips)
                                {
                                    if (ip == assetsrv)
                                    {
                                        assetsrv = null;
                                        break;
                                    }
                                }
                                if (assetsrv != null)
                                {
                                    var _ips = new IPAddress[ips.Length + 1];
                                    Array.Copy(ips, 0, _ips, 1, ips.Length);
                                    _ips[0] = assetsrv;
                                    ips = _ips;
                                }
                            }
                        }
                    }

                    ipPool = new AssetProxy.IPPool(ips);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);

                    throw new Exception("Unable to query DNS for " + ASSET_HOST);
                }
            }

            this.ipPool = ipPool;
        }

        /// <summary>
        /// Threads will stop once no more work is available
        /// </summary>
        public void StopWhenComplete()
        {
            if (work.keepalive)
            {
                work.keepalive = false;
                work.waiter.Set();
            }
        }

        /// <summary>
        /// Threads will be stopped after the current download is complete
        /// </summary>
        /// <param name="force">If true, downloads will be aborted</param>
        public void Abort(bool force)
        {
            work.abort = true;
            if (work.keepalive)
                work.waiter.Set();

            if (force)
            {
                foreach (var worker in workers)
                    worker.Abort();

                CancelQueue();
            }
        }

        private void CancelQueue()
        {
            while (work.queue.Count > 0)
            {
                Asset asset;
                lock (work)
                {
                    if (work.queue.Count > 0)
                        asset = work.queue.Dequeue();
                    else
                        asset = null;
                }
                if (asset != null)
                    asset.OnCancelled();
            }
        }

        public void Add(Asset asset)
        {
            lock (work)
            {
                work.queue.Enqueue(asset);
                work.waiter.Set();
            }

            OnAssetsAdded();
        }

        public void Add(IEnumerable<Asset> assets)
        {
            lock (work)
            {
                foreach (var asset in assets)
                {
                    work.queue.Enqueue(asset);
                }
                work.waiter.Set();
            }

            OnAssetsAdded();
        }

        private void OnAssetsAdded()
        {
            if (isActive)
            {
                lock (this)
                {
                    if (isActive && work.threads != workers.Length)
                    {
                        foreach (var worker in workers)
                        {
                            if (worker.Start())
                                work.threads++;
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            lock (work)
            {
                work.queue.Clear();
            }
        }

        public void Dispose()
        {
            work.abort = true;
            foreach (var worker in workers)
            {
                using (worker) { }
            }
        }

        public Task<AssetProxy.Cache.CacheStream> Download(Asset asset)
        {
            if (!isActive)
                Start();

            EventHandler<Net.AssetDownloader.Asset.CompleteEventArgs> onComplete = null;
            EventHandler onCancelled = null;
            ManualResetEvent waiter = new ManualResetEvent(false);
            AssetProxy.Cache.CacheStream cache = null;

            onComplete = delegate(object o, Net.AssetDownloader.Asset.CompleteEventArgs e)
            {
                cache = e.GetCache();
                onCancelled(o, EventArgs.Empty);
            };
            onCancelled = delegate
            {
                asset.Complete -= onComplete;
                asset.Cancelled -= onCancelled;
                waiter.Set();
            };

            asset.Complete += onComplete;
            asset.Cancelled += onCancelled;

            Add(asset);

            return Task.Run<AssetProxy.Cache.CacheStream>(new Func<AssetProxy.Cache.CacheStream>(
                delegate
                {
                    waiter.WaitOne();

                    return cache;
                }));
        }

        /// <summary>
        /// Downloads the asset without using the queue or cache
        /// </summary>
        public Task<string> DownloadString(Asset asset)
        {
            return Task.Run<string>(new Func<string>(
                delegate
                {
                    byte retry = 3;

                    do
                    {
                        IPAddress ip = null;

                        try
                        {
                            ip = ipPool.GetIP();

                            HttpWebRequest request = HttpWebRequest.CreateHttp(string.Format("http://{0}" + Asset.GetRequest(asset), ip));
                            request.Host = ASSET_HOST;
                            request.AllowAutoRedirect = false;
                            request.Proxy = null;
                            request.Timeout = CONNECTION_TIMEOUT_SECONDS * 1000;
                            request.Headers.Add(HttpRequestHeader.Cookie, COOKIE);

                            using (var response = (HttpWebResponse)request.GetResponse())
                            {
                                using (var r = new StreamReader(response.GetResponseStream()))
                                {
                                    return r.ReadToEnd();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (e is WebException)
                                using (((WebException)e).Response) { }

                            Util.Logging.Log(e);

                            if (ip == null)
                                return null;

                            ipPool.AddSample(ip, double.MaxValue);
                        }
                    }
                    while (--retry > 0);

                    return null;
                }));
        }
    }
}
