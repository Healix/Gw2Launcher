using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Gw2Launcher.Net.AssetProxy
{
    class Cache
    {
        public delegate void CacheStorageEventHandler(long bytes);

        public static event CacheStorageEventHandler CacheStorage;

        private static bool isEnabled;
        private static Dictionary<string, CacheRecord> cache;
        private static int counter;
        public static readonly string PATH;
        private static Thread threadDelete;
        private static long totalBytes;

        static Cache()
        {
            PATH = Path.Combine(Path.GetTempPath(), "assetcache");
            cache = new Dictionary<string, CacheRecord>();

            var t = threadDelete = new Thread(new ThreadStart(Purge));
            t.Start();
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

        public static CacheStream GetCache(string request)
        {
            if (!isEnabled)
                return null;

            RecordStream stream;

            lock(cache)
            {
                var now = DateTime.UtcNow;

                if (threadDelete != null)
                {
                    if (threadDelete.IsAlive)
                    {
                        try
                        {
                            threadDelete.Abort();
                            threadDelete = null;
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                    else
                        threadDelete = null;
                }

                CacheRecord record;
                if (!cache.TryGetValue(request, out record) || now > record.Expires || !File.Exists(record.path))
                {
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

                    if (record != null)
                        record.Delete();

                    byte retry = 10;
                    string path;
                    do
                    {
                        path = Path.Combine(PATH, counter++.ToString());
                        if (File.Exists(path))
                        {
                            try
                            {
                                File.Delete(path);

                                break;
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                                if (--retry == 0)
                                    break;
                            }
                        }
                        else
                            break;
                    }
                    while (true);

                    cache[request] = record = new CacheRecord(path);
                    record.Expires = now.AddMinutes(30);

                    record.FileSizeChanged += record_FileSizeChanged;
                }

                stream = new RecordStream(record);
            }

            return stream;
        }

        static void record_FileSizeChanged(Cache.CacheRecord record, long difference)
        {
            lock (cache)
            {
                totalBytes += difference;
                if (totalBytes < 0)
                    totalBytes = 0; //out of sync
                if (CacheStorage != null)
                    CacheStorage(totalBytes);
            }
        }

        static void stream_Committed(object sender, EventArgs e)
        {
            RecordStream stream = (RecordStream)sender;
            var length = stream.Length;
        }

        public static int Count
        {
            get
            {
                return cache.Count;
            }
        }

        public static void Clear()
        {
            lock (cache)
            {
                if (cache.Count == 0)
                    return;

                var deleted = cache.Values.ToArray();

                cache.Clear();

                var t = new Thread(new ThreadStart(
                    delegate
                    {
                        Purge(deleted);
                    }));
                t.Start();
            }
        }

        private static void Purge()
        {
            try
            {
                new DirectoryInfo(PATH).Delete(true);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            lock (cache)
            {
                if (totalBytes == 0)
                    return;
                totalBytes = 0;
                if (CacheStorage != null)
                    CacheStorage(totalBytes);
            }
        }

        private static void Purge(IEnumerable<CacheRecord> records)
        {
            long deleted = 0;
            foreach (var r in records)
            {
                deleted += r.length;
                try
                {
                    File.Delete(r.path);
                }
                catch { }
            }

            //try
            //{
            //    var di = new DirectoryInfo(PATH);
            //    if (di.Exists)
            //    {
            //        foreach (var f in di.GetFiles())
            //        {
            //            try
            //            {
            //                int i;
            //                if (Int32.TryParse(f.Name, out i) && i < limit)
            //                {
            //                    var length = f.Length;
            //                    f.Delete();
            //                    deleted += length;
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                Util.Logging.Log(ex);
            //            }
            //        }
            //    }
            //    else
            //        return;
            //}
            //catch (Exception e)
            //{
            //    Util.Logging.Log(e);
            //}

            if (deleted > 0)
            {
                lock (cache)
                {
                    totalBytes -= deleted;
                    if (totalBytes < 0)
                        totalBytes = 0; //out of sync

                    if (CacheStorage != null)
                        CacheStorage(totalBytes);
                }
            }
        }

        public abstract class CacheStream : Stream
        {
            public abstract DateTime Expires
            {
                get;
                set;
            }

            public abstract bool HasData
            {
                get;
            }

            public abstract void Commit();
        }

        private class RecordStream : CacheStream
        {
            private CacheRecord record;
            private Stream stream;
            private bool hasLock;
            private bool inUse;
            private bool written;
            private bool commit;

            public RecordStream(CacheRecord record)
            {
                this.record = record;

                lock (record)
                {
                    hasLock = record.owner == null;
                    record.locks++;

                    try
                    {
                        if (hasLock)
                        {
                            record.owner = this;
                            stream = File.Open(record.path, record.hasData ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                        }
                        else
                        {
                            record.Released += record_Released;
                            stream = File.Open(record.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                            inUse = true;
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
                    return stream != null && (record.hasData || inUse);
                }
            }

            public override DateTime Expires
            {
                get
                {
                    return record.Expires;
                }
                set
                {
                    record.Expires = value;
                }
            }

            public override void Commit()
            {
                commit = true;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (stream != null)
                        stream.Dispose();

                    if (hasLock)
                    {
                        if (written)
                        {
                            if (!commit)
                                record.Delete();
                        }
                    }

                    record.Release(hasLock);
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
                    return hasLock && stream != null && stream.CanWrite;
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
                    stream.Position = value;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int read = stream.Read(buffer, offset, count);

                while (!hasLock && read == 0 && inUse)
                {
                    System.Threading.Thread.Sleep(500);

                    read = stream.Read(buffer, offset, count);
                }

                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                if (hasLock)
                    stream.SetLength(value);
                else
                    throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (hasLock)
                {
                    stream.Write(buffer, offset, count);

                    record.hasData = true;
                    written = true;
                }
                else
                    throw new NotSupportedException();
            }
        }

        private class CacheRecord
        {
            public delegate void FileSizeChangedEventHandler(CacheRecord record, long difference);

            public event EventHandler Released;
            public event FileSizeChangedEventHandler FileSizeChanged;

            public string path;
            public CacheStream owner;
            public ushort locks;
            public bool delete;
            public long length;
            public bool hasData;

            public CacheRecord(string path)
            {
                this.path = path;
            }

            public DateTime Expires
            {
                get;
                set;
            }

            public void Release(bool hasLock)
            {
                if (hasLock)
                {
                    this.owner = null;

                    if (Released != null)
                        Released(this, EventArgs.Empty);

                    var fi = new FileInfo(path);
                    if (fi.Exists)
                    {
                        long l = length;
                        try
                        {
                            length = fi.Length;
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                            length = 0;
                        }

                        if (l != length && FileSizeChanged != null)
                            FileSizeChanged(this, length - l);
                    }
                }

                lock(this)
                {
                    if (--locks == 0 && (delete || !hasData))
                    {
                        hasData = false;
                        delete = false;
                        long l = length;
                        length = 0;

                        if (l != length && FileSizeChanged != null)
                            FileSizeChanged(this, length - l);

                        try
                        {
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                }
            }

            public void Delete()
            {
                lock (this)
                {
                    if (locks == 0)
                    {
                        hasData = false;
                        delete = false;
                        long l = length;
                        length = 0;

                        if (l != length && FileSizeChanged != null)
                            FileSizeChanged(this, length - l);

                        try
                        {
                            if (File.Exists(path))
                                File.Delete(path);
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
