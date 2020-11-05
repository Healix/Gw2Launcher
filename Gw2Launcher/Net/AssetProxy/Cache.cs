using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Net.AssetProxy
{
    class Cache
    {
        public delegate void CacheStorageEventHandler(long bytes);

        public static event CacheStorageEventHandler CacheStorage;
        public static event EventHandler CachePurged;
        public static event EventHandler<PurgeProgressEventArgs> PurgeProgress;

        public class PurgeProgressEventArgs
        {
            public int total, purged;
        }

        private static bool isEnabled;
        private static Dictionary<string, CacheRecord> cache;
        public static readonly string PATH;
        private static Thread threadDelete;
        private static long totalBytes;

        static Cache()
        {
            PATH = Path.Combine(Path.GetTempPath(), "assetcache");
            cache = new Dictionary<string, CacheRecord>();

            try
            {
                var di = new DirectoryInfo(PATH);
                if (!di.Exists)
                    di.Create();
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            Util.ScheduledEvents.Register(DoPurge, 86400000);

            DoPurge();
        }

        public static bool Enabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
            }
        }

        public static long Bytes
        {
            get
            {
                return totalBytes;
            }
        }

        static int DoPurge()
        {
            if (threadDelete == null)
            {
                var t = threadDelete = new Thread(new ThreadStart(
                    delegate
                    {
                        Purge(false);
                        threadDelete = null;

                        if (CachePurged != null)
                            CachePurged(null, null);
                    }));
                t.IsBackground = true;
                t.Start();
            }

            return 86400000;
        }

        public static CacheStream GetCache(string request)
        {
            if (!isEnabled)
                return null;

            string filename;
            if (request.StartsWith("/program/101/1/", StringComparison.Ordinal))
                filename = request.Substring(14);
            else
                filename = request;
            filename = filename.Replace('/', '_').Replace('\\', '_').Trim('_');

            CacheRecord record;
            bool isNew;

            lock(cache)
            {
                if (isNew = !cache.TryGetValue(filename, out record))
                {
                    cache[filename] = record = new CacheRecord(filename);
                    record.initialized = false;
                }
            }

            if (isNew)
            {
                Monitor.Enter(record);

                try
                {
                    record.FileSizeChanged += record_FileSizeChanged;
                    record.Unused += record_Unused;

                    var fi = new FileInfo(Path.Combine(PATH, filename));
                    if (fi.Exists)
                    {
                        bool useExisting = false;

                        if (fi.Name.StartsWith("latest", StringComparison.Ordinal))
                        {
                            //latest files contain build info and shouldn't be cached. However, if another client
                            //is already running, the game can't update anyways, so using a cached build will allow
                            //it to login without patching
                            if (DateTime.UtcNow.Subtract(fi.LastWriteTimeUtc).TotalSeconds < 60 || Gw2Launcher.Client.Launcher.GetActiveStates().Count > 1)
                            {
                                useExisting = true;
                            }
                        }
                        else
                        {
                            useExisting = true;
                        }

                        if (useExisting)
                        {
                            record.stored = true;
                            record.length = fi.Length;
                        }
                        else
                        {
                            try
                            {
                                long length = fi.Length;

                                fi.Delete();

                                if (Monitor.TryEnter(cache, 10000))
                                {
                                    try
                                    {
                                        totalBytes -= length;
                                        if (totalBytes < 0)
                                            totalBytes = 0;
                                        if (CacheStorage != null)
                                            CacheStorage(totalBytes);
                                    }
                                    finally
                                    {
                                        Monitor.Exit(cache);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
                finally
                {
                    record.initialized = true;
                    Monitor.PulseAll(record);
                    Monitor.Exit(record);
                }
            }
            else if (!record.initialized)
            {
                Monitor.Enter(record);
                try
                {
                    while (!record.initialized)
                    {
                        Monitor.Wait(record);
                    }
                }
                finally
                {
                    Monitor.Exit(record);
                }
            }

            return new RecordStream(record);
        }

        static void record_Unused(object sender, EventArgs e)
        {
            CacheRecord r = (CacheRecord)sender;

            if (Monitor.TryEnter(cache, 10000))
            {
                try
                {
                    cache.Remove(r.fileId);
                }
                finally
                {
                    Monitor.Exit(cache);
                }
            }
        }

        static void record_FileSizeChanged(Cache.CacheRecord record, long difference)
        {
            if (Monitor.TryEnter(cache, 10000))
            {
                try
                {
                    totalBytes += difference;
                    if (totalBytes < 0)
                        totalBytes = 0;
                    if (CacheStorage != null)
                        CacheStorage(totalBytes);
                }
                finally
                {
                    Monitor.Exit(cache);
                }
            }
        }

        public static void Clear()
        {
            if (threadDelete != null)
                return;

            var t = threadDelete = new Thread(new ThreadStart(
                delegate
                {
                    Purge(true);
                    threadDelete = null;

                    if (CachePurged != null)
                        CachePurged(null, null);
                }));
            t.IsBackground = true;
            t.Start();
        }

        private static void Purge(bool all)
        {
            var d = DateTime.UtcNow.Subtract(new TimeSpan(3,0,0,0,0));
            long bytes;
            long total = 0;
            FileInfo[] files;

            try
            {
                files = new DirectoryInfo(PATH).GetFiles();
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return;
            }

            PurgeProgressEventArgs progress = new PurgeProgressEventArgs()
            {
                total = files.Length
            };

            //not handling files in cache, since it only contains files in use

            HashSet<string> keys;
            lock(cache)
            {
                bytes = totalBytes;
                keys = new HashSet<string>(cache.Keys);
            }

            foreach (var fi in files)
            {
                try
                {
                    if (!keys.Contains(fi.Name) && (all || fi.LastWriteTimeUtc < d))
                    {
                        fi.Delete();
                    }
                    else
                    {
                        total += fi.Length;
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                progress.purged++;

                if (PurgeProgress != null)
                    PurgeProgress(null, progress);
            }

            lock (cache)
            {
                total = total + totalBytes - bytes;
                if (totalBytes != total)
                {
                    totalBytes = total;
                    if (CacheStorage != null)
                        CacheStorage(totalBytes);
                }
            }
        }

        public abstract class CacheStream : Stream
        {
            public abstract bool HasData
            {
                get;
            }

            /// <summary>
            /// Saves the file, otherwise it be deleted once closed
            /// </summary>
            public abstract void Commit();

            /// <summary>
            /// Sets the position to the content, skipping the header
            /// </summary>
            public abstract void SetPositionToContent();
        }

        private class RecordStream : CacheStream
        {
            private CacheRecord record;
            private Stream stream;

            private bool canWrite;
            private bool inUse;
            private bool commit;

            public RecordStream(CacheRecord record)
            {
                this.record = record;

                lock (record)
                {
                    var isEmpty = record.length == 0;
                    canWrite = record.locks++ == 0 && isEmpty;

                    try
                    {
                        var path = record.Path;
                        if (canWrite)
                        {
                            stream = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                        }
                        else
                        {
                            if (isEmpty)
                            {
                                inUse = true;
                                record.Released += record_Released;
                            }
                            stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
            }

            private void record_Released(object sender, EventArgs e)
            {
                ((CacheRecord)sender).Released -= record_Released;
                inUse = false;
            }

            public override bool HasData
            {
                get 
                {
                    return stream != null && (record.length > 0 || inUse);
                }
            }

            public override void Commit()
            {
                commit = true;
            }

            public override void SetPositionToContent()
            {
                this.Position = 0;

                var buffer = new byte[1024];
                int read;
                int offset = 0;
                int newLine = 0;

                while ((read = Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (offset == 0)
                    {
                        while (read < 4)
                        {
                            var r = Read(buffer, read, buffer.Length - read);
                            if (r == 0)
                                break;
                            read += r;
                        }

                        if (read >= 4)
                        {
                            uint h = BitConverter.ToUInt32(buffer, 0);
                            if (h != 1347703880 && h != 1886680168)
                            {
                                //not a http header
                                this.Position = 0;
                                return;
                            }
                        }
                        else
                        {
                            //not enough data and end of file
                            this.Position = 0;
                            return;
                        }
                    }
                    else if (offset > 16384)
                    {
                        //header too large / not a header
                        this.Position = 0;
                        return;
                    }

                    for (var i = 0; i < read; i++)
                    {
                        switch (buffer[i])
                        {
                            case 13: //\r
                                break;
                            case 10: //\n
                                newLine++;

                                if (newLine == 2)
                                {
                                    this.Position = offset + i + 1;
                                    return;
                                }

                                break;
                            case 0:  //\0
                                newLine = 0;
                                break;
                            default:
                                newLine = 0;
                                break;
                        }
                    }

                    offset += read;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (stream != null)
                        stream.Dispose();

                    if (canWrite && !commit)
                    {
                        record.Delete();
                    }

                    record.Release(canWrite);
                }
            }

            public override bool CanRead
            {
                get 
                {
                    return stream != null && stream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return stream.CanSeek;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return canWrite && stream != null && stream.CanWrite;
                }
            }

            public override void Flush()
            {
                if (stream != null)
                    stream.Flush();
            }

            public override long Length
            {
                get 
                {
                    return stream.Length;
                }
            }

            public override long Position
            {
                get
                {
                    return stream.Position;
                }
                set
                {
                    if (value < stream.Length)
                        stream.Position = value;
                    else
                    {
                        if (!canWrite)
                        {
                            while (value > stream.Position && inUse)
                            {
                                System.Threading.Thread.Sleep(500);
                            }
                        }

                        stream.Position = value;
                    }
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int read = stream.Read(buffer, offset, count);

                if (!canWrite)
                {
                    while (read == 0 && inUse)
                    {
                        System.Threading.Thread.Sleep(500);

                        read = stream.Read(buffer, offset, count);
                    }
                }

                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                if (canWrite)
                    stream.SetLength(value);
                else
                    throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (canWrite)
                    stream.Write(buffer, offset, count);
                else
                    throw new NotSupportedException();
            }
        }

        private class CacheRecord
        {
            public delegate void FileSizeChangedEventHandler(CacheRecord record, long difference);

            public event EventHandler Released;
            public event FileSizeChangedEventHandler FileSizeChanged;
            public event EventHandler Unused;

            public string fileId;
            public ushort locks;
            public bool stored;
            public bool delete;
            public long length;
            public bool initialized;

            public CacheRecord(string id)
            {
                this.fileId = id;
            }

            public string Path
            {
                get
                {
                    if (stored)
                        return System.IO.Path.Combine(PATH, fileId);
                    else
                        return System.IO.Path.Combine(PATH, fileId) + ".tmp";
                }
            }

            public void Release(bool hasLock)
            {
                lock (this)
                {
                    if (hasLock)
                    {
                        try
                        {
                            var fi = new FileInfo(this.Path);
                            if (fi.Exists)
                            {
                                long l = length;
                                length = fi.Length;

                                if (l != length && FileSizeChanged != null)
                                    FileSizeChanged(this, length - l);
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }

                        if (Released != null)
                            Released(this, EventArgs.Empty);
                    }

                    if (--locks == 0)
                    {
                        if (delete || length == 0)
                        {
                            delete = false;

                            if (length != 0 && FileSizeChanged != null)
                                FileSizeChanged(this, -length);
                            length = 0;

                            try
                            {
                                var path = this.Path;
                                if (File.Exists(path))
                                    File.Delete(path);
                                stored = false;
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }
                        else if (!stored)
                        {
                            var retry = 2;
                            while (retry-- > 0)
                            {
                                try
                                {
                                    var from = this.Path;
                                    var to = from.Substring(0, from.Length - 4);

                                    File.Move(from, to);

                                    stored = true;
                                    break;
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);

                                    try
                                    {
                                        if (File.Exists(this.Path))
                                            File.Delete(this.Path);
                                    }
                                    catch { }
                                }
                            }
                        }

                        if (Unused != null)
                            Unused(this, EventArgs.Empty);
                    }
                }
            }

            public void Delete()
            {
                lock (this)
                {
                    if (locks == 0)
                    {
                        delete = false;

                        if (length != 0 && FileSizeChanged != null)
                            FileSizeChanged(this, -length);
                        length = 0;

                        try
                        {
                            var path = this.Path;
                            if (File.Exists(path))
                                File.Delete(path);
                            stored = false;
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                    else
                        delete = true;
                }
            }
        }
    }
}
