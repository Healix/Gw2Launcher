using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gw2Launcher.Net.AssetProxy;

namespace Gw2Launcher.Tools
{
    public class BackgroundPatcher : IDisposable
    {
        public delegate void ErrorEventHandler(object sender, string message, Exception exception);

        public event EventHandler<float> ProgressChanged;
        public event EventHandler<DownloadProgressEventArgs> DownloadProgress;
        public event EventHandler<DownloadProgressEventArgs> DownloadManifestsComplete;
        public event ErrorEventHandler Error;
        public event EventHandler Complete;
        public event EventHandler Starting;
        public event EventHandler StateChanged;
        public event EventHandler<PatchEventArgs> PatchBeginning;
        public event EventHandler<PatchEventArgs> PatchReady;

        public class DownloadProgressEventArgs : EventArgs
        {
            public int filesTotal, filesDownloaded, manifestsTotal, manifestsDownloaded;
            public uint downloadRate;
            public long contentBytesTotal, estimatedBytesRemaining, bytesDownloaded, estimatedTotalBytes, estimatedBytesDownloaded;
            public long contentBytesCore; //exe and manifests
            public bool errored;
            public DateTime startTime;
            public int build;
            public bool rescan;
        }

        public class PatchEventArgs : EventArgs
        {
            public int Build
            {
                get;
                set;
            }

            public TimeSpan Elapsed
            {
                get;
                set;
            }

            public int Files
            {
                get;
                set;
            }

            public long Size
            {
                get;
                set;
            }
        }

        private const ushort MANIFEST_BASE_ID = 4101;
        private const float AVG_COMPRESSION = 0.575152368070167f; //0.565204517123359f; //0.627114534f; //average compression ratio of files in Gw2.dat
        private const float AVG_PATCH_COMPRESSION = 0.185207611983736f; //0.227189296144413f; //0.0724870563f; //average difference between patches and full uncompressed files

        private Net.AssetDownloader downloader;
        private Dictionary<int, int> baseIds;
        private bool isActive;
        private DownloadProgressEventArgs progress;
        private DateTime initializationTime;
        private int progressValue;
        private Dat.Compression.Archive archive;

        private struct LatestData
        {
            public int buildId, exeId, exeSize, manifestId, manifestSize;
        }

        public BackgroundPatcher()
        {
            Settings.BackgroundPatchingMaximumThreads.ValueChanged += BackgroundPatchingMaximumThreads_ValueChanged;
            Settings.PatchingSpeedLimit.ValueChanged += BackgroundPatchingSpeedLimit_ValueChanged;
        }

        static BackgroundPatcher()
        {
            _instance = new BackgroundPatcher();
        }

        private static BackgroundPatcher _instance;
        public static BackgroundPatcher Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Dispose()
        {
            Settings.BackgroundPatchingMaximumThreads.ValueChanged -= BackgroundPatchingMaximumThreads_ValueChanged;
            Settings.PatchingSpeedLimit.ValueChanged -= BackgroundPatchingSpeedLimit_ValueChanged;
        }

        void BackgroundPatchingMaximumThreads_ValueChanged(object sender, EventArgs e)
        {
            if (downloader != null)
            {
                var v = (Settings.ISettingValue<byte>)sender;
                if (v.HasValue)
                    downloader.Threads = v.Value;
                else
                    downloader.Threads = 10;
            }
        }

        void BackgroundPatchingSpeedLimit_ValueChanged(object sender, EventArgs e)
        {
            if (downloader != null)
            {
                var v = (Settings.ISettingValue<int>)sender;
                if (v.HasValue)
                    downloader.BpsLimit = v.Value;
                else
                    downloader.BpsLimit = Int32.MaxValue;
            }
        }

        void downloader_Tick(object sender, EventArgs e)
        {
            if (DownloadProgress != null)
                DownloadProgress(this, progress);
        }

        void downloader_RequestComplete(object sender, Net.AssetDownloader.RequestCompleteEventArgs e)
        {
            lock (this)
            {
                //progress.estimatedBytesRemaining -= e.Asset.size;
                progress.contentBytesTotal += e.ContentBytes;
            }
        }

        void downloader_Complete(object sender, EventArgs e)
        {
            if (DownloadProgress != null)
            {
                progress.bytesDownloaded = downloader.TotalBytesDownloaded;
                progress.downloadRate = 0;
                DownloadProgress(this, progress);
            }

            archive = null;

            if (Complete != null)
                Complete(this, e);

            if (progress.manifestsDownloaded + progress.filesDownloaded > 0 && progress.manifestsDownloaded == progress.manifestsTotal && progress.filesDownloaded == progress.filesTotal)
            {
                if (!progress.rescan || progress.filesDownloaded > 0)
                {
                    if (PatchReady != null)
                    {
                        var pr = new PatchEventArgs()
                        {
                            Build = progress.build,
                            Elapsed = DateTime.UtcNow.Subtract(progress.startTime),
                            Files = progress.filesTotal + progress.manifestsTotal,
                            Size = progress.contentBytesTotal - progress.contentBytesCore
                        };
                        PatchReady(this, pr);
                    }
                }
            }
        }

        void downloader_Error(object sender, Net.AssetDownloader.Asset asset, Exception exception)
        {
            Util.Logging.Log(exception);

            lock (this)
            {
                if (progress.errored)
                    return;
                progress.errored = true;
            }

            if (Error != null)
                Error(this, "Failed to download files", exception);
        }

        void downloader_DownloadRate(object sender, uint e)
        {
            progress.downloadRate = e;
            progress.bytesDownloaded = downloader.TotalBytesDownloaded;
        }

        public bool IsReady
        {
            get
            {
                return !isActive;
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive || downloader != null && downloader.IsActive;
            }
        }

        public void Stop(bool force)
        {
            if (downloader != null)
                downloader.Abort(force);
        }

        public void Start()
        {
            Start(false);
        }

        public async void Start(bool rescan)
        {
            lock (this)
            {
                if (IsActive)
                    return;
                isActive = true;
            }

            Cache.Enabled = true;

            int lastBuild;
            if (!rescan && progress != null && progress.filesTotal > 0 && progress.filesDownloaded == progress.filesTotal)
                lastBuild = progress.build;
            else
                lastBuild = 0;

            progress = new DownloadProgressEventArgs();
            progress.startTime = DateTime.UtcNow;
            progress.rescan = rescan;

            if (Starting != null)
                Starting(this, EventArgs.Empty);

            baseIds = await GetBaseIDs();

            if (baseIds.Count > 0)
            {
                #region Initialize downloader

                try
                {
                    if (downloader == null)
                    {
                        byte threads;
                        if (Settings.BackgroundPatchingMaximumThreads.HasValue)
                            threads = Settings.BackgroundPatchingMaximumThreads.Value;
                        else
                            threads = 10;

                        downloader = new Net.AssetDownloader(threads);

                        downloader.DownloadRate += downloader_DownloadRate;
                        downloader.Error += downloader_Error;
                        downloader.Complete += downloader_Complete;
                        downloader.RequestComplete += downloader_RequestComplete;
                        downloader.Tick += downloader_Tick;

                        var v = Settings.PatchingSpeedLimit;
                        if (v.HasValue)
                            downloader.BpsLimit = v.Value;
                        else
                            downloader.BpsLimit = Int32.MaxValue;

                        initializationTime = DateTime.UtcNow;
                    }
                    else if (DateTime.UtcNow.Subtract(initializationTime).TotalDays > 1)
                    {
                        downloader.SetIPPool(null);

                        initializationTime = DateTime.UtcNow;
                    }
                }
                catch (Exception e)
                {
                    progress.errored = true;
                    if (Error != null)
                        Error(this, "Failed to initialize", e);

                    isActive = false;
                    if (StateChanged != null)
                        StateChanged(this, EventArgs.Empty);
                    return;
                }

                #endregion

                var latest = await GetLatest();

                if (lastBuild > 0 && latest.buildId == lastBuild)
                {
                    //nochange
                    progress.build = latest.buildId;
                    progress.filesTotal = progress.filesDownloaded = 1;

                    if (Complete != null)
                        Complete(this, EventArgs.Empty);
                }
                else if (latest.buildId != 0)
                {
                    progress.build = latest.buildId;

                    downloader.Start();

                    if (await GetManifests(latest.manifestId, latest.manifestSize, baseIds, rescan))
                    {
                        if (progress.manifestsTotal > 0 || latest.buildId != Settings.LastKnownBuild.Value)
                        {
                            if (!progress.rescan)
                            {
                                if (PatchBeginning != null)
                                    PatchBeginning(null, new PatchEventArgs()
                                    {
                                        Build = latest.buildId
                                    });

                                lock (baseIds)
                                    progress.filesTotal++;

                                var asset = new Net.AssetDownloader.Asset(latest.exeId, true, (int)(latest.exeSize * AVG_COMPRESSION + 0.5f));

                                lock (this)
                                {
                                    progress.estimatedBytesRemaining += asset.size;
                                    progress.estimatedTotalBytes += asset.size;
                                }

                                asset.Cancelled += file_Cancelled;
                                asset.Complete += file_Complete;
                                asset.Progress += asset_Progress;

                                asset.Complete += delegate(object o, Net.AssetDownloader.Asset.CompleteEventArgs c)
                                {
                                    lock (this)
                                    {
                                        progress.contentBytesCore += c.ContentLength;
                                    }
                                };

                                downloader.Add(asset);
                            }
                        }
                    }
                    else
                    {
                        progress.errored = true;
                        if (Error != null)
                            Error(this, "Failed to retieve manifests", null);
                    }

                    downloader.StopWhenComplete();
                }
                else
                {
                    progress.errored = true;
                    if (Error != null)
                        Error(this, "Failed to retieve latest build", null);
                }
            }
            else
            {
                progress.errored = true;
                if (Error != null)
                    Error(this, "Failed to read Gw2.dat", null);
            }

            isActive = false;
            if (StateChanged != null)
                StateChanged(this, EventArgs.Empty);
        }

        void asset_Progress(object sender, Net.AssetDownloader.Asset.ProgressEventArgs e)
        {
            lock (this)
            {
                progress.estimatedBytesRemaining -= e.sizeChange;
                progress.estimatedBytesDownloaded += e.sizeChange;

                var v = (int)((double)progress.estimatedBytesDownloaded / progress.estimatedTotalBytes * 10000);
                if (v != progressValue)
                {
                    progressValue = v;
                    if (ProgressChanged != null)
                        ProgressChanged(this, v / 10000f);
                }
            }
        }

        private async Task<Dictionary<int, int>> GetBaseIDs()
        {
            return await Task.Run<Dictionary<int, int>>(new Func<Dictionary<int, int>>(
                delegate
                {
                    Dictionary<int, int> ids = new Dictionary<int, int>();

                    if (!string.IsNullOrEmpty(Settings.GW2Path.Value))
                    {
                        try
                        {
                            var path = Path.Combine(Path.GetDirectoryName(Settings.GW2Path.Value), "Gw2.dat");
                            if (File.Exists(path))
                            {
                                var entries = Dat.DatFile.Read(path);
                                foreach (var entry in entries)
                                {
                                    if (entry != null)
                                        ids[entry.baseId] = entry.fileId;
                                }
                            }

                            //additionally check Local.dat -- manifests are not always updated

                            foreach (var fid in Settings.DatFiles.GetKeys())
                            {
                                var dat = Settings.DatFiles[fid];
                                if (dat != null && dat.Value != null && !string.IsNullOrEmpty(dat.Value.Path) && File.Exists(dat.Value.Path))
                                {
                                    var entries = Dat.DatFile.Read(dat.Value.Path);
                                    foreach (var entry in entries)
                                    {
                                        if (entry != null)
                                        {
                                            int existing;
                                            if (!ids.TryGetValue(entry.baseId, out existing) || entry.fileId < existing)
                                                ids[entry.baseId] = entry.fileId;
                                        }
                                    }

                                    //only checking one of the Local.dat files, which should all be the same version
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }

                    return ids;
                }));
        }

        private bool Is64()
        {
            bool is64;

            if (is64 = Environment.Is64BitOperatingSystem)
            {
                //check if the 32-bit executable is being used with the -32 option
                if (!string.IsNullOrEmpty(Settings.GW2Arguments.Value) && Settings.GW2Arguments.Value.IndexOf("-32") != -1 && File.Exists(Settings.GW2Path.Value) && Util.FileUtil.Is32BitExecutable(Settings.GW2Path.Value))
                    is64 = false;
            }

            return is64;
        }
        
        private void AddFilesFromManifest(byte[] data, Dictionary<int, int> baseIds)
        {
            using (var stream = new MemoryStream(data,false))
            {
                AddFilesFromManifest(stream, baseIds);
            }
        }

        private void AddFilesFromManifest(Stream stream, Dictionary<int, int> baseIds)
        {
            List<Net.AssetDownloader.Asset> assets = new List<Net.AssetDownloader.Asset>();
            var manifest = Dat.Manifest.Parse(stream);
            long size = 0;

            lock (baseIds)
            {
                foreach (var record in manifest.records)
                {
                    int existing;
                    if (!baseIds.TryGetValue(record.baseId, out existing))
                        existing = 0;

                    if (existing != record.fileId)
                    {
                        baseIds[record.baseId] = record.fileId;

                        Net.AssetDownloader.Asset asset;
                        if (existing == 0)
                            asset = new Net.AssetDownloader.Asset(record.fileId, true, (int)(record.size * AVG_COMPRESSION + 0.5f));
                        else
                            asset = new Net.AssetDownloader.Asset(existing, record.fileId, (int)(record.size * AVG_PATCH_COMPRESSION + 0.5f));

                        asset.Complete += file_Complete;
                        asset.Cancelled += file_Cancelled;
                        asset.Progress += asset_Progress;

                        assets.Add(asset);

                        size += asset.size;
                    }
                }

                progress.filesTotal += assets.Count;
            }

            if (assets.Count > 0)
            {
                lock (this)
                {
                    progress.estimatedBytesRemaining += size;
                    progress.estimatedTotalBytes += size;
                }
                downloader.Add(assets);
            }
        }

        void file_Cancelled(object sender, EventArgs e)
        {
            lock (this)
            {
                if (progress.errored)
                    return;
                progress.errored = true;
            }

            if (Error != null)
                Error(this, "Failed to retieve files", null);
        }

        void file_Complete(object sender, Net.AssetDownloader.Asset.CompleteEventArgs e)
        {
            lock (this)
                progress.filesDownloaded++;
        }

        void manifest_Cancelled(object sender, EventArgs e)
        {
            lock (this)
            {
                if (progress.errored)
                    return;
                progress.errored = true;
            }

            if (Error != null)
                Error(this, "Failed to retieve manifests", null);
        }

        void manifest_Complete(object sender, Net.AssetDownloader.Asset.CompleteEventArgs e, bool isUsed)
        {
            var asset = (Net.AssetDownloader.Asset)sender;
            var c = e.GetCache();

            using (c)
            {
                if (c != null && c.HasData)
                {
                    if (isUsed)
                    {
                        if (asset.type == Net.AssetDownloader.Asset.AssetType.File)
                        {
                            try
                            {
                                c.SetPositionToContent();
                                AddFilesFromManifest(c, baseIds);

                                Create404Manifest(asset.fileId);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }
                        }
                        else if (asset.type == Net.AssetDownloader.Asset.AssetType.FileCompressed)
                        {
                            try
                            {
                                c.SetPositionToContent();
                                AddFilesFromManifest(archive.DecompressRaw(c, -1), baseIds);
                            }
                            catch (Exception ex)
                            {
                                Util.Logging.Log(ex);
                            }
                        }
                    }

                    bool isComplete;
                    lock (this)
                    {
                        progress.manifestsDownloaded++;
                        progress.contentBytesCore += e.ContentLength;
                        isComplete = progress.manifestsDownloaded == progress.manifestsTotal;
                    }

                    if (isComplete)
                        OnManifestsComplete();
                }
                else
                {
                    lock (this)
                    {
                        if (progress.errored)
                            return;
                        progress.errored = true;
                    }

                    if (Error != null)
                        Error(this, "Failed to retieve manifests", null);
                }
            }
        }

        void manifestUsed_Complete(object sender, Net.AssetDownloader.Asset.CompleteEventArgs e)
        {
            manifest_Complete(sender, e, true);
        }

        void manifestUnused_Complete(object sender, Net.AssetDownloader.Asset.CompleteEventArgs e)
        {
            manifest_Complete(sender, e, false);
        }

        private void OnManifestsComplete()
        {
            if (!progress.rescan && DownloadManifestsComplete != null)
                DownloadManifestsComplete(this, progress);
        }

        private void Create404Manifest(int manifestId)
        {
            //the uncompressed manifests were used to retrieve the patch files
            //a 404 response for the compressed manifests will cause the cached uncompressed manifests to be requested
            var cache = Cache.GetCache(Net.AssetDownloader.Asset.GetRequest(new Net.AssetDownloader.Asset(manifestId, true, 0)));
            using (cache)
            {
                if (cache != null && cache.CanWrite)
                {
                    using (var w = new StreamWriter(cache, Encoding.ASCII))
                    {
                        w.WriteLine("HTTP/1.1 404 Not Found");
                        w.WriteLine("Content-Length: 0");
                        w.WriteLine("Connection: keep-alive");
                        w.WriteLine("");

                        cache.Commit();
                    }
                }
            }
        }

        private async Task<bool> GetManifests(int manifestId, int manifestSize, Dictionary<int, int> baseIds, bool rescan)
        {
            int existing;
            if (baseIds.TryGetValue(MANIFEST_BASE_ID, out existing) && existing == manifestId)
            {
                if (!rescan)
                    return true; //the manifest is already up to date
            }
            else if (rescan)
                progress.rescan = false;

            progress.manifestsTotal++;
            progress.estimatedBytesRemaining += manifestSize;
            progress.estimatedTotalBytes += manifestSize;

            int language;
            switch (Settings.BackgroundPatchingLang.Value)
            {
                case 1: //DE
                    language = 296042;
                    break;
                case 3: //FR
                    language = 296043;
                    break;
                case 2: //ES (n/a)
                case 0: //EN
                default:
                    language = 296040;
                    break;
            }

#warning useCompression = BitConverter.IsLittleEndian
            var useCompression = BitConverter.IsLittleEndian;

            if (useCompression && archive == null)
                archive = new Dat.Compression.Archive();

            //var asset = new Net.AssetDownloader.Asset(manifestId, false, manifestSize);
            var asset = new Net.AssetDownloader.Asset(manifestId, useCompression, useCompression ? (int)(manifestSize * AVG_COMPRESSION + 0.5f) : manifestSize);
            asset.Complete += delegate(object o, Net.AssetDownloader.Asset.CompleteEventArgs c)
            {
                progress.contentBytesCore += c.ContentLength;
            };
            asset.Progress += asset_Progress;

            var cache = await downloader.Download(asset);
            using (cache)
            {
                if (cache != null && cache.HasData)
                {
                    cache.SetPositionToContent();
                    var p = cache.Position;
                    Dat.Manifest manifest;

                    if (useCompression)
                    {
                        using (var ms = new MemoryStream(archive.DecompressRaw(cache, manifestSize)))
                        {
                            manifest = Tools.Dat.Manifest.Parse(ms);
                        }
                    }
                    else
                    {
                        manifest = Tools.Dat.Manifest.Parse(cache);
                        Create404Manifest(manifestId);
                    }
                    
                    List<Net.AssetDownloader.Asset> assets = new List<Net.AssetDownloader.Asset>(manifest.records.Length);
                    long size = 0;

                    foreach (var record in manifest.records)
                    {
                        bool isUsed = true;

                        switch (record.baseId)
                        {
                            case 724786:    //Launcher
                                break;
                            case 1283391:   //Launcher64

                                if (!Is64())
                                    isUsed = false;

                                break;
                            case 1475411:   //LauncherOSX

                                isUsed = false;

                                break;
                            case 622855:    //ClientContent86
                                break;
                            case 1283393:   //ClientContent64
                                break;
                            case 296040:    //English
                            case 296042:    //German
                            case 296043:    //French
                            case 1051220:   //Chinese

                                isUsed = record.baseId == language;

                                break;
                        }

                        //asset = new Net.AssetDownloader.Asset(record.fileId, !isUsed, !isUsed ? (int)(record.size * AVG_COMPRESSION + 0.5f) : record.size);
                        asset = new Net.AssetDownloader.Asset(record.fileId, useCompression, useCompression ? (int)(record.size * AVG_COMPRESSION + 0.5f) : record.size);
                        if (isUsed)
                            asset.Complete += manifestUsed_Complete;
                        else
                            asset.Complete += manifestUnused_Complete;
                        asset.Cancelled += manifest_Cancelled;
                        asset.Progress += asset_Progress;

                        assets.Add(asset);

                        size += asset.size;
                    }

                    progress.manifestsTotal += assets.Count;
                    progress.manifestsDownloaded++;
                    progress.estimatedBytesRemaining += size;
                    progress.estimatedTotalBytes += size;

                    downloader.Add(assets);

                    return true;
                }
            }

            return false;
        }

        private async Task<LatestData> GetLatest()
        {
            var latest = await downloader.DownloadString(new Net.AssetDownloader.Asset(Is64() ? Net.AssetDownloader.Asset.AssetType.Latest64 : Net.AssetDownloader.Asset.AssetType.Latest32));

            if (!string.IsNullOrEmpty(latest))
            {
                var data = latest.Split(' ');

                try
                {
                    return new LatestData()
                    {
                        buildId = Int32.Parse(data[0]),
                        exeId = Int32.Parse(data[1]),
                        exeSize = Int32.Parse(data[2]),
                        manifestId = Int32.Parse(data[3]),
                        manifestSize = Int32.Parse(data[4])
                    };
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            return new LatestData();
        }
    }
}
