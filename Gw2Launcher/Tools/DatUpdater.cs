using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Gw2Launcher.Tools.Dat;
using Gw2Launcher.Tools.Dat.Compression;
using Gw2Launcher.Tools.Dat.Compression.IO;

namespace Gw2Launcher.Tools
{
    class DatUpdater
    {
        public const string CUSTOM_GW2CACHE_FOLDER_NAME = "gw2cache"; //CreateGw2Cache

        private const int ID_LOCAL = 16,
                          ID_CACHE = 12,
                          INDEX_ENTRIES = 2,
                          INDEX_IDS = 1,
                          INDEX_HEADER = 0;

        private event EventHandler<Settings.IDatFile> DatComplete;
        public event EventHandler Complete;

        private class Session
        {
            public Session(Session existing)
            {
                if (existing != null)
                {
                    compressor = existing.compressor;
                    if (compressor == null)
                        compressor = new Archive();
                }
                else
                    compressor = new Archive();
            }

            public Dictionary<int, int> snapshot;
            public HashSet<ushort> queued;
            public Queue<Settings.IDatFile> queue;
            public DatFile.MftEntry[] entries;
            public int entriesCount;
            public byte[] buffer1, buffer2;
            public int bufferLength;
            public bool abort;
            public int build;
            public Archive compressor;
            public Task task;
            public Settings.IDatFile master;

            public int BlockSize
            {
                get
                {
                    return BitConverter.ToInt32(buffer1, 12);
                }
            }

            public long MftOffset
            {
                get
                {
                    return BitConverter.ToInt64(buffer1, 24);
                }
                set
                {
                    var b = BitConverter.GetBytes(value);
                    Array.Copy(b, 0, buffer1, 24, b.Length);
                }
            }

            public int MftSize
            {
                get
                {
                    return BitConverter.ToInt32(buffer1, 32);
                }
                set
                {
                    var b = BitConverter.GetBytes(value);
                    Array.Copy(b, 0, buffer1, 32, b.Length);
                }
            }
        }

        private class CustomEntry
        {
            public int index;
            public DatFile.MftEntry entry;
            public byte[] data;
            public int length;
            public bool compress;

            public int WriteTo(Stream stream, Archive compressor)
            {
                if (compress)
                {
                    return compressor.Compress(data, 0, length, stream);
                }
                else
                {
                    stream.Write(data, 0, length);
                }

                return length;
            }

            public int WriteTo(byte[] buffer, int offset, Archive compressor)
            {
                if (compress)
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        ms.Position = offset;
                        return compressor.Compress(data, 0, length, ms);
                    }
                }
                else
                {
                    Array.Copy(data, 0, buffer, offset, length);
                }

                return length;
            }
        }

        private Session session;

        private DatUpdater()
        {

        }

        public bool IsActive
        {
            get
            {
                lock (this)
                {
                    if (session != null && session.task != null)
                        return !session.task.IsCompleted;
                    return false;
                }
            }
        }

        public bool CanUpdate
        {
            get
            {
                return session != null && session.master != null;
            }
        }

        /// <summary>
        /// Creates a new instance to be used for updating dat files
        /// </summary>
        /// <param name="master">The already updated file to be used as a master</param>
        public static DatUpdater Create(Settings.IDatFile master)
        {
            var du = new DatUpdater()
            {
                session = new Session(null)
                {
                    master = master,
                },
            };

            du.Initialize(du.session);

            return du;
        }

        /// <summary>
        /// Creates a new instance to be used for updating cache files
        /// </summary>
        public static DatUpdater Create()
        {
            var du = new DatUpdater()
            {
                session = new Session(null),
            };

            return du;
        }

        public static int GetBuild(Settings.IDatFile dat)
        {
            try
            {
                using (var r = new BinaryReader(new BufferedStream(File.Open(dat.Path, FileMode.Open, FileAccess.Read, FileShare.Read))))
                {
                    var mft = Dat.DatFile.ReadMft(r);

                    foreach (var entry in mft.entries)
                    {
                        switch (entry.baseId)
                        {
                            case ID_LOCAL:

                                return ReadBuild(new Archive(), r.BaseStream, entry);
                        }
                    }
                }
            }
            catch { }

            return 0;
        }

        private void Initialize(Session s)
        {
            const byte RESERVED = 15; //first 15 indexes are reserved

            var i = 0;

#warning !BitConverter.IsLittleEndian
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException();

            var hs = new HashSet<ushort>();
            var queue = new Queue<Settings.IDatFile>();

            s.queue = queue;
            s.queued = hs;

            var modifyGw2Cache = Settings.GuildWars2.UseCustomGw2Cache.Value;

            using (var r = new BinaryReader(new BufferedStream(File.Open(s.master.Path, FileMode.Open, modifyGw2Cache ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read))))
            {
                var mft = DatFile.ReadMft(r);
                var blockSize = mft.BlockSize;
                var custom = new Dictionary<int, CustomEntry>();
                var entries = new DatFile.MftEntry[mft.entries.Length + 50];
                var snapshot = new Dictionary<int, int>(entries.Length);
                var changed = s.snapshot == null;
                var count = 0;
                var ofs = 0;

                for (i = 0; i < mft.entries.Length; i++)
                {
                    var entry = mft.entries[i];
                    var size = entry.size;

                    if (!changed && entry.baseId > 0)
                    {
                        int fileId;
                        if (!s.snapshot.TryGetValue(entry.baseId, out fileId) || fileId != entry.fileId)
                            changed = true;
                    }

                    if (i < RESERVED)
                    {
                        //lower indexes are reserved

                        switch (i)
                        {
                            case INDEX_IDS:
                            case INDEX_ENTRIES:

                                size = 0;

                                break;
                        }
                    }
                    else if (entry.baseId == entry.fileId && entry.baseId < 100)
                    {
                        //lower IDs are core files and specific to the account

                        switch (entry.baseId)
                        {
                            case ID_LOCAL:

                                s.build = ReadBuild(s.compressor, r.BaseStream, entry);

                                int previous;
                                if (!changed && s.build > 0 && s.snapshot.TryGetValue(0, out previous) && previous != s.build)
                                    changed = true;

                                break;
                            case ID_CACHE:

                                custom[entry.baseId] = new CustomEntry()
                                {
                                    entry = entry,
                                    index = i,
                                };

                                break;
                        }

                        continue;
                    }

                    entries[count++] = entry;

                    if (entry.baseId > 0)
                        snapshot[entry.baseId] = entry.fileId;

                    if (size > 0)
                    {
                        ofs += (size / blockSize + 1) * blockSize;
                    }
                }

                //if (!changed)
                //    return;

                if (modifyGw2Cache && s.build > 0)
                {
                    CustomEntry c;
                    if (!custom.TryGetValue(ID_CACHE, out c))
                    {
                        custom[ID_CACHE] = c = new CustomEntry()
                        {
                            entry = new DatFile.MftEntry()
                            {
                                compression = 8,
                                baseId = ID_CACHE,
                                fileId = ID_CACHE,
                                crc = 1214729159,
                                flags = 3,
                            }
                        };
                    }

                    var e = c.entry;
                    if (e.compression == 8 || e.compression == 0)
                    {
                        var data = CreateGw2Cache(s.compressor, s.build, e.compression == 8);

                        var entry = new DatFile.MftEntry()
                        {
                            baseId = e.baseId,
                            fileId = e.fileId,
                            compression = e.compression,
                            counter = e.counter,
                            crc = e.crc,
                            flags = e.flags,
                            offset = long.MaxValue,
                            size = data.Length,
                        };

                        entries[count++] = entry;
                        snapshot[entry.baseId] = entry.fileId;

                        c.data = data;
                        c.length = data.Length;

                        ofs += (entry.size / blockSize + 1) * blockSize;
                    }
                }

                Array.Sort(entries, RESERVED, count - RESERVED, Comparer<DatFile.MftEntry>.Create(
                    delegate(DatFile.MftEntry a, DatFile.MftEntry b)
                    {
                        return a.offset.CompareTo(b.offset);
                    }));

                var buffer = new byte[ofs];
                ofs = blockSize;

                Array.Copy(mft.header, buffer, mft.header.Length);

                for (i = INDEX_ENTRIES + 1; i < count; i++)
                {
                    var e = entries[i];

                    if (e.size == 0)
                        continue;

                    if (e.offset != long.MaxValue)
                    {
                        r.BaseStream.Position = e.offset;
                        ReadBytes(r, buffer, ofs, e.size);
                    }
                    else
                    {
                        CustomEntry c;
                        if (!custom.TryGetValue(e.baseId, out c))
                            continue;
                        e.size = c.WriteTo(buffer, ofs, s.compressor);
                    }

                    e.offset = ofs;
                    ofs += (e.size / blockSize + 1) * blockSize;
                }

                s.entries = entries;
                s.entriesCount = count;
                s.buffer1 = buffer;
                s.snapshot = snapshot;

                if (modifyGw2Cache)
                {
                    CustomEntry c;
                    if (custom.TryGetValue(ID_CACHE, out c) && c.data != null)
                    {
                        if (c.index > 0)
                        {
                            UpdateCache(s, r, mft, c);
                        }
                        else
                        {
                            r.BaseStream.Position = 0;
                            UpdateCache(r, s.build);
                        }
                    }
                }
            }
        }

        private DatFile.MftEntry FindEntry(DatFile.MftEntry[] entries, int length, int baseId)
        {
            for (var i = length - 1; i >= 0; --i)
            {
                var e = entries[i];
                if (e.baseId == baseId)
                    return e;
            }
            return null;
        }

        /// <summary>
        /// Updates the gw2cache location
        /// </summary>
        /// <param name="build">Optionally supply the build, otherwise it'll be found within the dat or looked up</param>
        public bool UpdateCache(Settings.IDatFile dat, int build = 0)
        {
            using (var bs = new BufferedStream(File.Open(dat.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)))
            {
                var applied = false;

                if (bs.Length > 0)
                {
                    using (var r = new BinaryReader(bs))
                    {
                        applied = UpdateCache(r, build);
                    }
                }
                else
                {
                    if (build == 0)
                        build = Tools.Gw2Build.Build;
                    var buffer = CreateNew(build);
                    bs.Write(buffer, 0, buffer.Length);
                    applied = build != 0;
                }

                return applied;
            }
        }

        /// <summary>
        /// Updates the dat file using the master
        /// </summary>
        public static bool Create(Settings.IDatFile master, Settings.IDatFile dat)
        {
            return Create(master).Update(dat);
        }

        /// <summary>
        /// Creates a bare Local.dat file
        /// </summary>
        /// <param name="build">If > 0, sets the gw2cache location</param>
        /// <returns>Local.dat file in bytes</returns>
        public static byte[] CreateNew(int build = 0)
        {
            int blockSize = 512;

            using (var ms = new MemoryStream(blockSize * (build > 0 ? 4 : 2))) //4 blocks: header, entries, cache, ids (cache + ids only needed if build is set)
            {
                using (var fs = new Dat.Compression.IO.FileStream(ms, true, false, false))
                {
                    using (var w = new BinaryWriter(fs))
                    {
                        DatFile.MftEntry e;
                        var entries = new DatFile.MftEntry[build > 0 ? 16 : 15]; //first 15 entries are reserved
                        long ofs = 0;

                        entries[INDEX_HEADER] = new DatFile.MftEntry()
                        {
                            flags = 3,
                            size = 40,
                        };
                        ofs += blockSize;

                        if (build > 0)
                        {
                            var indexCache = entries.Length - 1;

                            e = entries[indexCache] = new DatFile.MftEntry()
                            {
                                offset = ofs,
                                compression = 8,
                                flags = 3,
                                crc = 1214729159, //note the crc is always the same because the file ends with a crc, which cancels out when crc'd again
                            };

                            w.BaseStream.Position = ofs;
                            w.Write(CreateGw2Cache(new Archive(), build, true));
                            e.size = (int)(w.BaseStream.Position - ofs);
                            ofs += (e.size / blockSize + 1) * blockSize;

                            e = entries[INDEX_IDS] = new DatFile.MftEntry()
                            {
                                offset = ofs,
                                flags = 3,
                            };

                            w.BaseStream.Position = ofs;
                            fs.ComputeCRC = true;

                            w.Write(ID_CACHE);
                            w.Write(indexCache + 1);

                            e.size = (int)(w.BaseStream.Position - ofs);
                            e.crc = fs.CRC;
                            fs.ComputeCRC = false;
                            fs.ResetCRC();
                            ofs += blockSize;
                        }

                        e = entries[INDEX_ENTRIES] = new DatFile.MftEntry()
                        {
                            offset = ofs,
                            flags = 3,
                        };

                        w.BaseStream.Position = e.offset;
                        fs.ComputeCRC = true;

                        w.Write(443835981U);
                        w.Write(0L);
                        w.Write(entries.Length + 1); //this header is included in the count
                        w.Write(0L);

                        for (var i = 0; i < INDEX_ENTRIES; i++)
                        {
                            WriteEntry(w, entries[i]);
                        }

                        w.BaseStream.Position += 24; //INDEX_ENTRIES is written last

                        for (var i = INDEX_ENTRIES + 1; i < entries.Length; i++)
                        {
                            WriteEntry(w, entries[i]);
                        }

                        e.size = (int)(w.BaseStream.Position - e.offset);
                        e.crc = fs.CRC;
                        fs.ComputeCRC = false;
                        fs.ResetCRC();
                        ofs += blockSize;

                        w.BaseStream.Position = e.offset + (INDEX_ENTRIES + 1) * 24;

                        WriteEntry(w, entries[INDEX_ENTRIES]);

                        w.BaseStream.Position = 0;

                        w.Write(441336215U);
                        w.Write(entries[INDEX_HEADER].size);
                        w.Write(3401187329U);
                        w.Write(blockSize);
                        w.Write(2396038944U);
                        w.Write(0);
                        w.Write(entries[INDEX_ENTRIES].offset);
                        w.Write(entries[INDEX_ENTRIES].size);
                        w.Write(0);
                    }
                }

                return ms.ToArray();
            }
        }

        private bool UpdateCache(BinaryReader r, int build = 0)
        {
            var mft = DatFile.ReadMft(r);
            var s = session;
            CustomEntry c = null;
            var counter = 2;

            for (var i = mft.entries.Length - 1; i >= 0; --i)
            {
                var e = mft.entries[i];

                switch (e.baseId)
                {
                    case ID_CACHE:

                        c = new CustomEntry()
                        {
                            index = i,
                            entry = e,
                        };
                        if (--counter == 0)
                            i = 1;

                        break;
                    case ID_LOCAL:

                        if (build == 0)
                        {
                            build = ReadBuild(s.compressor, r.BaseStream, e);
                        }
                        if (--counter == 0)
                            i = 1;

                        break;
                }
            }

            if (build > 0)
            {
                if (c == null)
                {
                    //entry will need to be created, which will require updating the entries and ids

                    var blockSize = mft.BlockSize;
                    var ofs = 0L;
                    DatFile.MftEntry entry;

                    c = new CustomEntry()
                    {
                        index = mft.entries.Length,
                    };

                    //find the next position to write data (not considering free space)
                    for (var i = mft.entries.Length - 1; i >= 0; --i)
                    {
                        var e = mft.entries[i];
                        var l = e.offset + e.size;

                        if (l > ofs)
                            ofs = l;
                    }

                    ofs = (ofs / blockSize + 1) * blockSize;

                    using (var w = new BinaryWriter(r.BaseStream, Encoding.ASCII, true))
                    {
                        var crc32 = new Crc32();
                        byte[] buffer;

                        entry = mft.entries[INDEX_IDS];

                        if (!HasFreeSpace(entry, blockSize, 8))
                        {
                            r.BaseStream.Position = entry.offset;
                            buffer = r.ReadBytes(entry.size);

                            w.BaseStream.Position = ofs;
                            w.Write(buffer);

                            entry.offset = ofs;

                            ofs += ((entry.size + 8) / blockSize + 1) * blockSize;
                        }

                        //adding id
                        w.BaseStream.Position = entry.offset + entry.size;
                        w.Write(ID_CACHE);
                        w.Write(c.index + 1);

                        entry.size += 8;

                        //update size
                        w.BaseStream.Position = mft.entries[INDEX_ENTRIES].offset + (INDEX_IDS + 1) * 24;
                        w.Write(entry.offset);
                        w.Write(entry.size);

                        //recalculate crc
                        r.BaseStream.Position = entry.offset;
                        buffer = r.ReadBytes(entry.size);
                        foreach (var b in buffer)
                        {
                            crc32.Add(b);
                        }

                        //update crc
                        w.BaseStream.Position = mft.entries[INDEX_ENTRIES].offset + (INDEX_IDS + 1) * 24 + 20;
                        w.Write(crc32.CRC);

                        crc32.Reset();

                        entry = mft.entries[INDEX_ENTRIES];

                        if (!HasFreeSpace(entry, blockSize, 24))
                        {
                            r.BaseStream.Position = entry.offset;
                            buffer = r.ReadBytes(entry.size);

                            w.BaseStream.Position = ofs;
                            w.Write(buffer);

                            entry.offset = ofs;

                            ofs += ((entry.size + 24) / blockSize + 1) * blockSize;
                        }

                        c.entry = new DatFile.MftEntry()
                        {
                            compression = 8,
                            baseId = ID_CACHE,
                            fileId = ID_CACHE,
                            crc = 1214729159,
                            flags = 3,
                            offset = ofs,
                        };
                        c.data = CreateGw2Cache(s.compressor, build, c.entry.compression == 8);
                        c.length = c.data.Length;

                        var entries = new DatFile.MftEntry[c.index + 1];
                        Array.Copy(mft.entries, entries, mft.entries.Length);
                        entries[c.index] = c.entry;
                        mft.entries = entries;

                        //adding entry
                        w.BaseStream.Position = entry.offset + entry.size;
                        WriteEntry(w, c.entry);

                        entry.size += 24;

                        //update count
                        w.BaseStream.Position = entry.offset + 12;
                        w.Write(entries.Length + 1);

                        //update size
                        w.BaseStream.Position = entry.offset + (INDEX_ENTRIES + 1) * 24;
                        w.Write(entry.offset);
                        w.Write(entry.size);

                        //entries crc is updated when writing data

                        //update header
                        w.BaseStream.Position = 24;
                        w.Write(entry.offset);
                        w.Write(entry.size);
                    }
                }
                else
                {
                    c.data = CreateGw2Cache(s.compressor, build, c.entry.compression == 8);
                    c.length = c.data.Length;
                }

                UpdateCache(s, r, mft, c);

                return true;
            }

            return false;
        }

        private void UpdateCache(Session s, BinaryReader r, DatFile.Mft mft, CustomEntry c)
        {
            using (var w = new BinaryWriter(r.BaseStream))
            {
                var entry = mft.entries[INDEX_ENTRIES];

                //write the data
                w.BaseStream.Position = mft.entries[c.index].offset;
                var size = c.WriteTo(w.BaseStream, s.compressor);

                if (size != c.entry.size)
                {
                    //rewrite the size of the entry
                    w.BaseStream.Position = entry.offset + (c.index + 1) * 24 + 8;
                    w.Write(size);

                    //note that although the data for this entry has changed, the crc hasn't due to how it's calculated

                    //calculate the crc of the main entry
                    r.BaseStream.Position = entry.offset;
                    var buffer = r.ReadBytes(entry.size);

                    var crc32 = new Crc32();

                    for (var i = 0; i < buffer.Length; i++)
                    {
                        if (i < 72 || i >= 96) //skip 3rd entry
                            crc32.Add(buffer[i]);
                    }

                    //rewrite the crc
                    w.BaseStream.Position = entry.offset + (INDEX_ENTRIES + 1) * 24 + 20;
                    w.Write(crc32.CRC);
                }
            }
        }

        private bool HasFreeSpace(DatFile.MftEntry e, int blockSize, int space)
        {
            var l = e.offset + e.size;
            var r = (int)(l % blockSize);
            if (r > 0)
                r = blockSize - r;
            return r >= space;
        }

        /// <summary>
        /// Updates the Local.dat file based on the supplied master file
        /// </summary>
        public bool Update(Settings.IDatFile dat)
        {
            try
            {
                Update(session, dat);

                dat.IsPending = false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Snapshot(Settings.IDatFile master)
        {
            Session s;

            lock (this)
            {
                s = new Session(session)
                {
                    master = master,
                };

                if (session != null)
                {
                    session.abort = true;
                }

                session = s;
            }

            try
            {
                using (var r = new BinaryReader(new BufferedStream(File.Open(master.Path, FileMode.Open, FileAccess.Read, FileShare.Read))))
                {
                    var mft = DatFile.ReadMft(r);
                    s.snapshot = new Dictionary<int, int>(mft.entries.Length);

                    foreach (var entry in mft.entries)
                    {
                        if (entry.baseId > 0)
                        {
                            s.snapshot[entry.baseId] = entry.fileId;

                            if (entry.baseId == ID_LOCAL)
                            {
                                r.BaseStream.Position = entry.offset;
                                s.snapshot[0] = ReadBuild(s.compressor, r.BaseStream, entry);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public void Abort()
        {
            lock (this)
            {
                if (session != null)
                {
                    session.abort = true;
                    session = null;
                }
            }
        }

        /// <summary>
        /// Updates all dat files in the background
        /// </summary>
        public bool Update()
        {
            try
            {
                Session s;

                lock (this)
                {
                    if (session == null)
                        return false;
                    s = session;
                }

                Update(s);

                return true;
            }
            catch
            {
                session = null;
                return false;
            }
        }

        private void Update(Session s)
        {
            var ids = Settings.DatFiles.GetKeys();
            var dats = new Settings.IDatFile[ids.Length];
            var i = 0;

            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException();

            var hs = new HashSet<ushort>();
            var queue = new Queue<Settings.IDatFile>(ids.Length);

            foreach (var uid in ids)
            {
                if (s.master.UID == uid)
                    continue;

                var dat = Settings.DatFiles[uid].Value;
                if (dat != null && dat.References > 0 && dat.IsPending)
                {
                    dats[i++] = dat;

                    hs.Add(dat.UID);
                    queue.Enqueue(dat);
                }
            }

            if (i > 0)
            {
                Initialize(s);

                s.task = new Task(DoQueue, TaskCreationOptions.LongRunning);
                s.task.Start();
            }
        }

        private static void WriteEntry(BinaryWriter w, DatFile.MftEntry e)
        {
            if (e == null)
            {
                w.Write(0L);
                w.Write(0L);
                w.Write(0L);
            }
            else
            {
                w.Write(e.offset);
                w.Write(e.size);
                w.Write(e.compression);
                w.Write(e.flags);
                w.Write(e.counter);
                w.Write(e.crc);
            }
        }

        private void DoQueue()
        {
            Session s;

            lock (this)
            {
                s = this.session;
                if (s == null || s.abort)
                    return;
            }

            var q = s.queue;
            Settings.IDatFile dat = null;

            while (true)
            {
                lock (this)
                {
                    if (dat != null)
                    {
                        s.queued.Remove(dat.UID);
                        if (DatComplete != null)
                            DatComplete(this, dat);
                    }

                    if (s.abort || q.Count == 0)
                    {
                        if (!s.abort)
                        {
                            session = null;
                            if (Complete != null)
                                Complete(this, EventArgs.Empty);
                        }
                        return;
                    }

                    dat = q.Dequeue();
                }

                try
                {
                    Update(s, dat);

                    lock (this)
                    {
                        if (!s.abort)
                        {
                            dat.IsPending = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        private void Update(Session s, Settings.IDatFile dat)
        {
            using (var r = new BinaryReader(new BufferedStream(System.IO.File.Open(dat.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))))
            {
                DatFile.Mft mft;
                int mftEntries;

                if (r.BaseStream.Length > 0)
                {
                    mft = DatFile.ReadMft(r);
                    mftEntries = mft.entries.Length;
                }
                else
                {
                    mft = null;
                    mftEntries = 0;
                }
                var blockSize = s.BlockSize;
                var custom = new Dictionary<int, CustomEntry>();
                var entries = s.entries;
                var count = s.entriesCount;
                var remaining = s.snapshot.Count;
                var changed = false;
                var size = 0;

                for (var i = 0; i < mftEntries; i++)
                {
                    var entry = mft.entries[i];

                    if (entry.baseId > 0)
                    {
                        if (!changed)
                        {
                            int fileId;
                            if (s.snapshot.TryGetValue(entry.baseId, out fileId))
                            {
                                --remaining;

                                if (fileId != entry.fileId)
                                {
                                    changed = true;
                                }
                                else
                                {
                                    if (entry.baseId == ID_CACHE)
                                    {
                                        var e = FindEntry(s.entries, s.entriesCount, entry.baseId);
                                        if (e != null && e.size != entry.size)
                                            changed = true;
                                    }
                                }

                                continue;
                            }
                        }

                        if (entry.baseId == entry.fileId && entry.baseId < 100 && entry.size > 0)
                        {
                            if (changed && s.snapshot.ContainsKey(entry.baseId))
                                continue;

                            switch (entry.baseId)
                            {
                                case ID_LOCAL:

                                    byte[] data;
                                    int position;

                                    var build = ReadBuild(s.compressor, r.BaseStream, entry, out data, out position);

                                    if (s.build > build)
                                    {
                                        changed = true;

                                        //updating the build
                                        //note that this isn't required

                                        var b = BitConverter.GetBytes(s.build);
                                        Array.Copy(b, 0, data, position, 4);

                                        var c = new CustomEntry()
                                        {
                                            compress = true,
                                            data = data,
                                            entry = entry,
                                            index = i,
                                            length = data.Length,
                                        };

                                        custom[entry.baseId] = c;

                                        entry.offset = long.MaxValue;
                                        entry.size = data.Length;
                                    }

                                    break;
                            }

                            entries[count++] = entry;
                            size += (entry.size / blockSize + 1) * blockSize;
                        }
                    }
                }

                if (!changed && remaining == 0)
                    return;

                size += ((s.entriesCount + count) * 40 / blockSize + 1) * blockSize; //add padding for entries and IDs (24 bytes per entry, 8-16 bytes per id)

                Array.Sort(entries, s.entriesCount, count - s.entriesCount, Comparer<DatFile.MftEntry>.Create(
                    delegate(DatFile.MftEntry a, DatFile.MftEntry b)
                    {
                        return a.offset.CompareTo(b.offset);
                    }));

                var buffer = s.buffer2;
                if (buffer == null || buffer.Length < size)
                    s.buffer2 = buffer = new byte[size + 1024 * 1024];

                var ofs = 0;
                var fofs = s.buffer1.Length;

                for (var i = s.entriesCount; i < count; i++)
                {
                    var e = entries[i];

                    if (e.size == 0)
                    {
                        e.offset = 0;
                        continue;
                    }

                    if (e.offset != long.MaxValue)
                    {
                        r.BaseStream.Position = e.offset;
                        ReadBytes(r, buffer, ofs, e.size);
                    }
                    else
                    {
                        CustomEntry c;
                        if (custom.TryGetValue(e.baseId, out c))
                            e.size = c.WriteTo(buffer, ofs, s.compressor);
                    }

                    e.offset = ofs + fofs;
                    ofs += (e.size / blockSize + 1) * blockSize;
                }

                using (var fs = new Dat.Compression.IO.FileStream(new MemoryStream(buffer), false, true, false))
                {
                    using (var w = new BinaryWriter(fs))
                    {
                        w.BaseStream.Position = ofs;

                        //write ids
                        for (var i = 1; i <= count; i++)
                        {
                            var entry = entries[i - 1];

                            if (entry.baseId > 0)
                            {
                                w.Write(entry.baseId);
                                w.Write(i);

                                if (entry.baseId != entry.fileId)
                                {
                                    w.Write(entry.fileId);
                                    w.Write(i);
                                }
                            }
                        }

                        var e = entries[INDEX_IDS];

                        e.offset = ofs + fofs;
                        e.size = (int)(w.BaseStream.Position - ofs);
                        e.crc = fs.CRC;
                        ofs += (e.size / blockSize + 1) * blockSize;

                        e = entries[INDEX_ENTRIES];
                        w.BaseStream.Position = ofs;
                        fs.ResetCRC();

                        //write entries header
                        w.Write(443835981U);
                        w.Write(0L);
                        w.Write(count + 1);
                        w.Write(0L);

                        //write first set of entries
                        for (var i = 0; i < INDEX_ENTRIES; i++)
                        {
                            WriteEntry(w, entries[i]);
                        }

                        var p0 = w.BaseStream.Position;
                        w.BaseStream.Position += 24; //entry is written last

                        //write remaining entries
                        for (var i = INDEX_ENTRIES + 1; i < count; i++)
                        {
                            WriteEntry(w, entries[i]);
                        }

                        e.offset = ofs + fofs;
                        e.size = (int)(w.BaseStream.Position - ofs);
                        e.crc = fs.CRC;
                        ofs += (e.size / blockSize + 1) * blockSize;

                        s.MftOffset = e.offset;
                        s.MftSize = e.size;

                        w.BaseStream.Position = p0;
                        WriteEntry(w, entries[INDEX_ENTRIES]);
                    }
                }

                var stream = r.BaseStream;

                stream.Position = 0;
                stream.Write(s.buffer1, 0, s.buffer1.Length);
                stream.Write(buffer, 0, ofs);

                stream.SetLength(stream.Position);
            }
        }

        private void ReadBytes(BinaryReader r, byte[] buffer, int offset, int count)
        {
            do
            {
                var read = r.Read(buffer, offset, count);

                if (read == count)
                    return;
                else if (read == 0)
                    throw new EndOfStreamException();

                count -= read;
                offset += read;
            }
            while (count > 0);
        }

        private static byte[] CreateGw2Cache(Dat.Compression.Archive compressor, int build, bool compress)
        {
            var buffer = new byte[532];
            var b = BitConverter.GetBytes(build);

            buffer[0] = (byte)'A';
            buffer[1] = (byte)'R';
            buffer[2] = (byte)'A';
            buffer[3] = (byte)'P';

            buffer[4] = b[0];
            buffer[5] = b[1];

            //null terminated unicode string

            //int i = 6;
            //foreach (char c in CUSTOM_GW2CACHE_FOLDER_NAME)
            //{
            //    buffer[i] = (byte)c;
            //    i += 2;
            //}

            buffer[6] = (byte)'g';
            buffer[8] = (byte)'w';
            buffer[10] = (byte)'2';
            buffer[12] = (byte)'c';
            buffer[14] = (byte)'a';
            buffer[16] = (byte)'c';
            buffer[18] = (byte)'h';
            buffer[20] = (byte)'e';

            buffer[528] = (byte)'D';
            buffer[529] = (byte)'I';
            buffer[530] = (byte)'O';
            buffer[531] = (byte)'N';

            if (compress)
            {
                using (var ms = new MemoryStream(buffer.Length))
                {
                    var size = compressor.Compress(buffer, 0, buffer.Length, ms);

                    return ms.ToArray();
                }
            }
            else
            {
                return buffer;
            }
        }

        private static int ReadBuild(Archive compressor, Stream stream, DatFile.MftEntry e)
        {
            byte[] data;
            int position;
            return ReadBuild(compressor, stream, e, out data, out position);
        }

        private static int ReadBuild(Archive compressor, Stream stream, DatFile.MftEntry e, out byte[] data, out int position)
        {
            stream.Position = e.offset;

            data = compressor.Decompress(stream, e.size);

            using (var r = new BinaryReader(new MemoryStream(data)))
            {
                if (r.ReadUInt16() != 18000) //PF
                    throw new IOException("Unknown header");
                r.BaseStream.Position += 4;
                if (r.ReadUInt16() != 12)
                    throw new IOException("Unknown version");
                if (r.ReadUInt32() != 1818455916U) //locl
                    throw new IOException("Unknown header");

                while (r.BaseStream.Position < data.Length)
                {
                    var chunkType = r.ReadUInt32();
                    var chunkSize = r.ReadUInt32();
                    var chunkEnd = r.BaseStream.Position + chunkSize;

                    if (chunkType == 1701998435U) //core
                    {
                        var chunkVersion = r.ReadUInt16();
                        var chunkHeaderSize = r.ReadUInt16();
                        var chunkTableOffset = r.ReadUInt32();

                        if (chunkVersion != 0)
                            throw new IOException("Unknown core version");

                        position = (int)r.BaseStream.Position;
                        return r.ReadInt32();
                    }

                    r.BaseStream.Position = chunkEnd;
                }
            }

            throw new FileNotFoundException();
        }

        /// <summary>
        /// Waits for a dat file being updated in the background to complete
        /// </summary>
        public void Wait(Settings.IDatFile dat)
        {
            ManualResetEvent waiter = null;
            EventHandler<Settings.IDatFile> onComplete = null;

            while (true)
            {
                var found = false;

                lock (this)
                {
                    if (session != null && session.queued != null && session.queued.Contains(dat.UID))
                    {
                        for (var i = session.queue.Count - 1; i >= 0; --i)
                        {
                            if (session.queue.Peek() == dat)
                            {
                                if (waiter == null)
                                {
                                    waiter = new ManualResetEvent(false);
                                    onComplete = delegate(object o, Settings.IDatFile d)
                                    {
                                        if (d == dat)
                                        {
                                            DatComplete -= onComplete;
                                            waiter.Set();
                                        }
                                    };
                                    DatComplete += onComplete;
                                }
                                found = true;

                                break;
                            }
                            session.queue.Enqueue(session.queue.Dequeue());
                        }
                    }
                }

                if (waiter != null)
                {
                    if (found && waiter.WaitOne(60000, false))
                    {
                        waiter.Dispose();
                        return;
                    }
                    else
                    {
                        DatComplete -= onComplete;
                        if (!found)
                        {
                            waiter.Dispose();
                            return;
                        }
                    }
                }
                else
                    return;
            }
        }
    }
}
