using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Tools
{
    class DataStore : IDisposable
    {
        protected const uint MIN_BLOCK_SIZE = 10; //minimum acceptable size when searching for a free block

        protected class ItemEntry
        {
            public ushort id;
            public uint position;
            public uint length;
        }

        protected ItemEntry[] entries;
        protected bool hasFree;
        protected ushort nextId;
        protected Dictionary<ushort, ItemEntry> ids;

        protected Stream stream;
        protected bool modified;

        public DataStore(string path)
        {
            ids = new Dictionary<ushort, ItemEntry>();
            stream = new BufferedStream(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));

            try
            {
                stream.Position = stream.Length - sizeof(uint);

                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    var position = reader.ReadUInt32();
                    stream.Position = position;

                    var count = reader.ReadUInt16();

                    entries = new ItemEntry[count];

                    for (var i = 0; i < count; i++)
                    {
                        ItemEntry e;
                        entries[i] = e = new ItemEntry()
                        {
                            id = reader.ReadUInt16(),
                            position = reader.ReadUInt32(),
                            length = reader.ReadUInt32(),
                        };
                        ids[e.id] = e;
                        if (!hasFree && (i == 0 && entries[i].position > MIN_BLOCK_SIZE || i > 0 && entries[i].position - entries[i - 1].position - entries[i - 1].length > MIN_BLOCK_SIZE))
                            hasFree = true;
                    }
                }
            }
            catch
            {
                ids.Clear();
                entries = new ItemEntry[0];
                stream.SetLength(0);
            }
        }

        public ushort Add(byte[] data)
        {
            return Add(data, 0, data.Length);
        }

        public ushort Add(byte[] data, int offset, int length)
        {
            var _entries = new ItemEntry[entries.Length + 1];

            while (ids.ContainsKey(++nextId))
            {
                if (nextId == 0)
                    throw new ArgumentOutOfRangeException();
            }

            var entry = new ItemEntry()
            {
                id = nextId,
                length = (uint)length,
            };

            ids[entry.id] = entry;
            modified = true;

            if (hasFree)
            {
                int gaps = 0;
                uint smallest = uint.MaxValue;
                uint min = entry.length;
                if (min > MIN_BLOCK_SIZE)
                    min = MIN_BLOCK_SIZE;
                int insertBefore = -1;

                if (entries.Length > 0 && entries[0].position >= min)
                {
                    gaps++;
                    var size = entries[0].position;
                    if (size >= entry.length)
                    {
                        smallest = size;
                        insertBefore = 0;
                    }
                }

                for (int i = 0, l = entries.Length - 1; i < l; i++)
                {
                    var size = entries[i + 1].position - entries[i].position - entries[i].length;
                    if (size >= min)
                    {
                        gaps++;
                        if (size >= entry.length && size < smallest)
                        {
                            smallest = size;
                            insertBefore = i + 1;
                        }
                    }
                }

                if (insertBefore != -1)
                {
                    if (insertBefore > 0)
                        entry.position = entries[insertBefore - 1].position + entries[insertBefore - 1].length;
                    else
                        entry.position = 0;

                    Write(entry.position, data, offset, length);

                    Array.Copy(entries, 0, _entries, 0, insertBefore);
                    Array.Copy(entries, insertBefore, _entries, insertBefore + 1, entries.Length - insertBefore);

                    _entries[insertBefore] = entry;

                    entries = _entries;
                    hasFree = --gaps > 0 || smallest - entry.length >= min;

                    return entry.id;
                }
                else if (gaps == 0)
                    hasFree = false;
            }
            else if (entries.Length == 0)
            {
                _entries[0] = entry;
                entries = _entries;

                Write(0, data, offset, length);

                return entry.id;
            }

            var insert = entries.Length - 1;

            entry.position = entries[insert].position + entries[insert].length;

            Write(entry.position, data, offset, length);

            Array.Copy(entries, _entries, entries.Length);

            _entries[insert + 1] = entry;

            entries = _entries;

            return entry.id;
        }

        protected void Write(uint position, byte[] data, int offset, int length)
        {
            stream.Position = position;
            stream.Write(data, offset, length);
        }

        protected void WriteEntries()
        {
            if (entries.Length > 0)
            {
                var e = entries[entries.Length - 1];
                stream.Position = e.position + e.length;
            }
            else
            {
                stream.SetLength(0);
                return;
            }

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                uint position = (uint)stream.Position;

                writer.Write((ushort)entries.Length);

                foreach (var e in entries)
                {
                    writer.Write(e.id);
                    writer.Write(e.position);
                    writer.Write(e.length);
                }

                writer.Write(position);

                if (stream.Length > stream.Position)
                    stream.SetLength(stream.Position);
            }
        }

        public bool Remove(ushort id)
        {
            if (!ids.Remove(id))
                return false;

            for (int i = 0, l = entries.Length; i < l; i++)
            {
                var e = entries[i];
                if (e.id == id)
                {
                    var _entries = new ItemEntry[l - 1];

                    Array.Copy(entries, _entries, i);
                    Array.Copy(entries, i + 1, _entries, i, l - i - 1);

                    entries = _entries;

                    hasFree = l > 1;
                    var nid = id - 1;
                    if (nid >= 0 && nid < nextId)
                        nextId = (ushort)nid;

                    modified = true;

                    return true;
                }
            }

            return true;
        }

        public byte[] Get(ushort id)
        {
            ItemEntry entry;
            if (!ids.TryGetValue(id, out entry))
                return null;

            int length = (int)entry.length,
                offset = 0;

            var buffer = new byte[length];

            stream.Position = entry.position;
            do
            {
                offset += stream.Read(buffer, offset, length - offset);
            }
            while (offset < length);

            return buffer;
        }

        /// <summary>
        /// Removes all keys except those specified
        /// </summary>
        public void RemoveExcept(IEnumerable<ushort> keys)
        {
            var ids = new HashSet<ushort>(this.ids.Keys);
            foreach (var k in keys)
                ids.Remove(k);

            if (ids.Count > 0)
            {
                var _entries = new ItemEntry[this.ids.Count - ids.Count];
                var j = 0;

                for (int i = 0, l = entries.Length; i < l; i++)
                {
                    var e = entries[i];

                    if (!ids.Contains(e.id))
                    {
                        _entries[j++] = e;
                    }
                    else
                    {
                        this.ids.Remove(e.id);
                        var nid = e.id - 1;
                        if (nid >= 0 && nid < nextId)
                            nextId = (ushort)nid;
                    }
                }

                hasFree = true;
                modified = true;

                entries = _entries;
            }
        }

        public ICollection<ushort> Keys
        {
            get
            {
                return ids.Keys;
            }
        }

        public void Dispose()
        {
            if (stream != null)
            {
                if (modified)
                    WriteEntries();

                stream.Dispose();
                stream = null;
            }
        }
    }
}
