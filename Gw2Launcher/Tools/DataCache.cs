using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Tools
{
    abstract class DataCache
    {
        public interface ICacheItem<TID>
        {
            TID ID
            {
                get;
                set;
            }
            void ReadFrom(BinaryReader reader, uint length);
            void WriteTo(BinaryWriter writer);
        }

        public interface IFormat<T>
        {
            T Read(BinaryReader reader);
            void Write(BinaryWriter writer, T value);
            IEqualityComparer<T> Comparer
            {
                get;
            }
        }

        public class DictionarySet<TKey, TValue> : ISet<TKey>
        {
            private Dictionary<TKey, TValue> dictionary;

            public DictionarySet(Dictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }

            public bool Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public void ExceptWith(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public void IntersectWith(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public bool IsProperSubsetOf(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public bool IsProperSupersetOf(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public bool IsSubsetOf(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public bool IsSupersetOf(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public bool Overlaps(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public bool SetEquals(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public void SymmetricExceptWith(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            public void UnionWith(IEnumerable<TKey> other)
            {
                throw new NotSupportedException();
            }

            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TKey item)
            {
                return dictionary.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get
                {
                    return dictionary.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public bool Remove(TKey item)
            {
                return dictionary.Remove(item);
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return dictionary.Keys.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return dictionary.Keys.GetEnumerator();
            }
        }

        protected class FormatU32 : IFormat<uint>
        {
            public uint Read(BinaryReader reader)
            {
                return reader.ReadUInt32();
            }

            public void Write(BinaryWriter writer, uint value)
            {
                writer.Write(value);
            }

            public IEqualityComparer<uint> Comparer
            {
                get
                {
                    return null;
                }
            }
        }

        protected class FormatU16 : IFormat<ushort>
        {
            public ushort Read(BinaryReader reader)
            {
                return reader.ReadUInt16();
            }

            public void Write(BinaryWriter writer, ushort value)
            {
                writer.Write(value);
            }

            public IEqualityComparer<ushort> Comparer
            {
                get
                {
                    return null;
                }
            }
        }

        protected class FormatUtf8 : IFormat<string>
        {
            private IEqualityComparer<string> comparer;

            public FormatUtf8(IEqualityComparer<string> comparer)
            {
                this.comparer = comparer;
            }

            public string Read(BinaryReader reader)
            {
                return reader.ReadString();
            }

            public void Write(BinaryWriter writer, string value)
            {
                writer.Write(value);
            }

            public IEqualityComparer<string> Comparer
            {
                get
                {
                    return comparer;
                }
            }
        }


        public static IFormat<uint> U32
        {
            get
            {
                return new FormatU32();
            }
        }

        public static IFormat<ushort> U16
        {
            get
            {
                return new FormatU16();
            }
        }

        public static IFormat<string> Utf8
        {
            get
            {
                return new FormatUtf8(null);
            }
        }

        public static IFormat<string> Utf8IgnoreCase
        {
            get
            {
                return new FormatUtf8(StringComparer.OrdinalIgnoreCase);
            }
        }


        public class UnknownHeaderException : Exception
        {
            public UnknownHeaderException(int header, ushort version)
            {
                this.Header = header;
                this.Version = version;
            }

            public int Header
            {
                get;
                private set;
            }

            public ushort Version
            {
                get;
                private set;
            }
        }
    }

    class DataCache<TID> : DataCache
    {
        protected enum Format : byte
        {
            Unknown = 0,
            U16 = 1,
            U32 = 2,
        }

        protected readonly string path;
        protected readonly ushort version;
        protected readonly int header;
        protected readonly IFormat<TID> formatter;

        protected class DataEntry
        {
            public TID id;
            public uint offset, length;
        }

        /// <summary>
        /// Stores data to a file, with each entry retrievable by its ID. Intended for static
        /// files that don't change; data is appended
        /// </summary>
        /// <param name="header">The 4-byte header code to represent this instance</param>
        /// <param name="version">The version of this instance</param>
        /// <param name="path">The path to the file</param>
        /// <param name="formatter">The ID formatter for this instance</param>
        public DataCache(int header, ushort version, string path, IFormat<TID> formatter)
        {
            this.header = header;
            this.version = version;
            this.path = path;
            this.formatter = formatter;
        }

        /// <summary>
        /// If the file already exists and has data, verifies the header or throws a UnknownHeaderException
        /// </summary>
        public void Verify()
        {
            lock (this)
            {
                try
                {
                    using (var reader = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)), Encoding.UTF8))
                    {
                        var length = reader.BaseStream.Length;
                        ushort version;
                        int header;

                        if (length >= 6)
                        {
                            reader.BaseStream.Position = length - 6;
                            version = reader.ReadUInt16();
                            header = reader.ReadInt32();
                        }
                        else if (length == 0)
                        {
                            return;
                        }
                        else
                        {
                            version = 0;
                            header = 0;
                        }

                        if (header != this.header || version != this.version)
                        {
                            throw new UnknownHeaderException(header, version);
                        }
                    }
                }
                catch (UnknownHeaderException)
                {
                    throw;
                }
                catch { }
            }
        }

        protected DataEntry[] ReadEntries(BinaryReader reader, out Format format)
        {
            const byte HEADER_LENGTH = 11;

            var l = reader.BaseStream.Length;
            if (l < HEADER_LENGTH)
            {
                format = Format.Unknown;
                return null;
            }

            reader.BaseStream.Position = l - HEADER_LENGTH;

            uint position = reader.ReadUInt32();
            format = (Format)reader.ReadByte();
            ushort version = reader.ReadUInt16();
            int header = reader.ReadInt32();

            if (header != this.header || version != this.version)
            {
                throw new UnknownHeaderException(header, version);
            }

            reader.BaseStream.Position = position;

            uint count = reader.ReadUInt16();
            if (count == 0)
                return null;
            if (count == ushort.MaxValue)
                count = reader.ReadUInt32();
            var entries = new DataEntry[count];
            uint offset = 0;

            for (var i = 0; i < count; i++)
            {
                DataEntry e;

                e = new DataEntry()
                {
                    offset = offset,
                    id = formatter.Read(reader),
                };

                switch (format)
                {
                    case Format.U16:
                        e.length = reader.ReadUInt16();
                        break;
                    case Format.U32:
                    default:
                        e.length = reader.ReadUInt32();
                        break;
                }

                entries[i] = e;

                offset += e.length;
            }

            return entries;
        }

        public IEnumerable<TItem> ReadAll<TItem>()
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            lock (this)
            {
                using (var reader = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)), Encoding.UTF8))
                {
                    Format format;
                    var entries = ReadEntries(reader, out format);
                    if (entries != null)
                    {
                        foreach (var e in entries)
                        {
                            reader.BaseStream.Position = e.offset;

                            var item = new TItem()
                            {
                                ID = e.id,
                            };

                            item.ReadFrom(reader, e.length);

                            yield return item;
                        }
                    }
                }
            }
        }

        public void Read<TItem>(ISet<TID> ids, Dictionary<TID, TItem> output)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            Read(ids, output, null);
        }

        public Task ReadAsync<TItem>(ISet<TID> ids, Dictionary<TID, TItem> output)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            return Task.Run(new Action(
                delegate
                {
                    Read(ids, output, null);
                }));
        }

        public void Read<TItem>(ISet<TID> ids, Dictionary<TID, TItem> output, Action<TItem> onRead)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            int count;

            if (ids != null)
            {
                count = ids.Count;
                if (count == 0)
                    return;
            }
            else
            {
                count = int.MaxValue;
            }

            lock (this)
            {
                using (var reader = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)), Encoding.UTF8))
                {
                    Format format;
                    var entries = ReadEntries(reader, out format);
                    if (entries == null)
                        return;

                    foreach (var e in entries)
                    {
                        if (ids == null || ids.Remove(e.id))
                        {
                            reader.BaseStream.Position = e.offset;

                            TItem item;
                            if (!output.TryGetValue(e.id, out item))
                            {
                                output[e.id] = item = new TItem()
                                {
                                    ID = e.id,
                                };
                            }

                            item.ReadFrom(reader, e.length);

                            if (onRead != null)
                                onRead(item);

                            if (--count == 0)
                                return;
                        }
                    }
                }
            }
        }

        public TItem Read<TItem>(TID id)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            lock (this)
            {
                using (var reader = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)), Encoding.UTF8))
                {
                    var comparer = formatter.Comparer;
                    Format format;
                    var entries = ReadEntries(reader, out format);
                    if (entries == null)
                        throw new KeyNotFoundException();

                    foreach (var e in entries)
                    {
                        bool eq;
                        if (comparer == null)
                            eq = e.id.Equals(id);
                        else
                            eq = comparer.Equals(e.id, id);

                        if (eq)
                        {
                            reader.BaseStream.Position = e.offset;

                            TItem item = new TItem()
                            {
                                ID = e.id,
                            };

                            item.ReadFrom(reader, e.length);

                            return item;
                        }
                    }
                }
            }

            throw new KeyNotFoundException();
        }

        public Task ReadAsync<TItem>(ISet<TID> ids, Dictionary<TID, TItem> output, Action<TItem> onRead)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            return Task.Run(new Action(
                delegate
                {
                    Read(ids, output, onRead);
                }));
        }

        public void Delete(ISet<TID> ids)
        {
            lock (this)
            {
                using (var stream = new BufferedStream(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                        {
                            try
                            {
                                Delete(ids, stream, reader, writer);
                            }
                            catch (Exception e)
                            {
                                //data is corrupted
                                Util.Logging.Log(e);
                                stream.SetLength(0);
                            }
                        }
                    }
                }
            }
        }

        public Task DeleteAsync(ISet<TID> ids)
        {
            return Task.Run(new Action(
                delegate
                {
                    Delete(ids);
                }));
        }

        public void Clear()
        {
            lock (this)
            {
                using (var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    try
                    {
                        stream.SetLength(0);
                    }
                    catch (Exception e)
                    {
                        //data is corrupted
                        Util.Logging.Log(e);
                        stream.SetLength(0);
                    }
                }
            }
        }

        public Task ClearAsync()
        {
            return Task.Run(new Action(
                delegate
                {
                    Clear();
                }));
        }

        private void Delete(ISet<TID> ids, Stream stream, BinaryReader reader, BinaryWriter writer)
        {
            DataEntry[] entries;
            int count = 0;
            Format format;

            try
            {
                entries = ReadEntries(reader, out format);
                if (entries != null)
                    count = entries.Length;
                if (count == 0)
                    return;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return;
            }

            var last = entries[count - 1];
            var offsetNext = last.offset + last.length;

            writer.BaseStream.Position = offsetNext;
            var first = true;
            byte[] buffer = null;
            uint
                writeOffset = 0,
                readOffset = 0;

            int j = 0;
            for (int i = 0; i < count; i++)
            {
                var e = entries[i];
                if (ids.Contains(e.id))
                {
                    if (first)
                    {
                        first = false;
                        stream.SetLength(offsetNext);
                        buffer = new byte[32 * 1024];
                        writeOffset = e.offset;
                        readOffset = e.offset + e.length;
                    }
                    else if (readOffset == e.offset)
                    {
                        //consecutive entries
                        readOffset += e.length;
                    }
                    else
                    {
                        var length = e.offset - readOffset;
                        ShiftStreamUp(stream, buffer, readOffset, (int)length, writeOffset);
                        writeOffset += length;
                        readOffset = e.offset + e.length;
                    }
                }
                else
                {
                    if (j != i)
                        entries[j] = e;
                    j++;
                }
            }

            if (first)
            {
                //nothing changed
                return;
            }

            count = j;

            if (readOffset != offsetNext)
            {
                var length = offsetNext - readOffset;
                ShiftStreamUp(stream, buffer, readOffset, (int)length, writeOffset);
                writeOffset += length;
            }

            writer.BaseStream.Position = offsetNext = writeOffset;

            WriteEntries(writer, format, entries, count, null);
        }

        public void Write<TItem>(TItem item)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            Write<TItem>(new TItem[] { item });
        }

        public void Write<TItem>(ICollection<TItem> items)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            lock (this)
            {
                using (var stream = new BufferedStream(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                        {
                            try
                            {
                                Write(items, stream, reader, writer);
                            }
                            catch (Exception e)
                            {
                                //data is corrupted
                                Util.Logging.Log(e);
                                stream.SetLength(0);
                            }
                        }
                    }
                }
            }
        }
        
        public Task WriteAsync<TItem>(ICollection<TItem> items)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            return Task.Run(new Action(
                delegate
                {
                    Write(items);
                }));
        }

        private void Write<TItem>(ICollection<TItem> items, Stream stream, BinaryReader reader, BinaryWriter writer)
            where TItem : DataCache.ICacheItem<TID>, new()
        {
            DataEntry[] entries;
            int count = 0;
            Format format;

            try
            {
                entries = ReadEntries(reader, out format);
                if (entries != null)
                    count = entries.Length;
                if (count == 0)
                    format = Format.U16;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                entries = null;
                format = Format.U16;
            }

            var existing = new HashSet<TID>(formatter.Comparer);
            uint offsetNext = 0;

            if (count > 0)
            {
                foreach (var e in entries)
                    existing.Add(e.id);

                var last = entries[count - 1];
                offsetNext = last.offset + last.length;
            }

            var append = new List<DataEntry>(items.Count);
            Dictionary<TID, TItem> modified = null;

            writer.BaseStream.Position = offsetNext;
            var first = true;
            var appendStart = offsetNext;

            foreach (var item in items)
            {
                if (existing.Contains(item.ID))
                {
                    if (modified == null)
                        modified = new Dictionary<TID, TItem>(formatter.Comparer);
                    modified[item.ID] = item;
                    continue;
                }

                if (first)
                {
                    first = false;
                    if (offsetNext < writer.BaseStream.Length)
                        writer.BaseStream.SetLength(offsetNext);
                }

                item.WriteTo(writer);

                var p = (uint)writer.BaseStream.Position;
                var length = p - offsetNext;

                if (format != Format.U32 && length > ushort.MaxValue)
                    format = Format.U32;

                append.Add(new DataEntry()
                {
                    id = item.ID,
                    offset = offsetNext,
                    length = length,
                });

                offsetNext = p;
            }

            #region Handle overwriting existing entries

            if (modified != null)
            {
                if (first)
                {
                    if (offsetNext < writer.BaseStream.Length)
                        writer.BaseStream.SetLength(offsetNext);
                }
                else
                    first = true;

                uint
                    appendLength = offsetNext - appendStart,
                    writeOffset = 0,
                    readOffset = 0;
                byte[] buffer = null;

                int j = 0;
                for (int i = 0; i < count; i++)
                {
                    var e = entries[i];
                    if (modified.ContainsKey(e.id))
                    {
                        if (first)
                        {
                            first = false;
                            buffer = new byte[32 * 1024];
                            writeOffset = e.offset;
                            readOffset = e.offset + e.length;
                        }
                        else if (readOffset == e.offset)
                        {
                            //consecutive entries
                            readOffset += e.length;
                        }
                        else
                        {
                            var length = e.offset - readOffset;
                            ShiftStreamUp(stream, buffer, readOffset, (int)length, writeOffset);
                            writeOffset += length;
                            readOffset = e.offset + e.length;
                        }
                    }
                    else
                    {
                        if (j != i)
                            entries[j] = e;
                        j++;
                    }
                }

                count = j;

                if (readOffset != appendStart)
                {
                    var length = appendStart + appendLength - readOffset;
                    ShiftStreamUp(stream, buffer, readOffset, (int)length, writeOffset);
                    writeOffset += length;
                }
                else if (appendLength > 0)
                {
                    var length = appendStart + appendLength - readOffset;
                    ShiftStreamUp(stream, buffer, appendStart, (int)appendLength, writeOffset);
                    writeOffset += length;
                }

                writer.BaseStream.Position = offsetNext = writeOffset;

                foreach (var item in modified.Values)
                {
                    item.WriteTo(writer);

                    var p = (uint)writer.BaseStream.Position;
                    var length = p - offsetNext;

                    if (format != Format.U32 && length > ushort.MaxValue)
                        format = Format.U32;

                    append.Add(new DataEntry()
                    {
                        id = item.ID,
                        offset = offsetNext,
                        length = length,
                    });

                    offsetNext = p;
                }
            }

            #endregion

            if (append.Count > 0)
                WriteEntries(writer, format, entries, count, append);
        }

        private void WriteEntries(BinaryWriter writer, Format format, DataEntry[] entries, int count, ICollection<DataEntry> append)
        {
            var offset = (uint)writer.BaseStream.Position;
            uint c = (uint)count;
            if (append != null)
                c += (uint)append.Count;

            if (c >= ushort.MaxValue)
            {
                // this should never actually happen
                writer.Write(ushort.MaxValue);
                writer.Write(c);
            }
            else
                writer.Write((ushort)c);

            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var e = entries[i];
                    formatter.Write(writer, e.id);
                    switch (format)
                    {
                        case Format.U16:
                            writer.Write((ushort)e.length);
                            break;
                        case Format.U32:
                        default:
                            writer.Write(e.length);
                            break;
                    }
                }
            }

            if (append != null)
            {
                foreach (var e in append)
                {
                    formatter.Write(writer, e.id);
                    switch (format)
                    {
                        case Format.U16:
                            writer.Write((ushort)e.length);
                            break;
                        case Format.U32:
                        default:
                            writer.Write(e.length);
                            break;
                    }
                }
            }

            writer.Write(offset);
            writer.Write((byte)format);
            writer.Write(version);
            writer.Write(header);

            if (writer.BaseStream.Position < writer.BaseStream.Length)
            {
                writer.BaseStream.SetLength(writer.BaseStream.Position);
            }
        }

        private void ShiftStreamUp(Stream stream, byte[] buffer, long readFrom, int readLength, long writeAt)
        {
            int count = buffer.Length,
                read;

            do
            {
                if (readLength < count)
                    count = readLength;

                stream.Position = readFrom;
                read = stream.Read(buffer, 0, count);
                readLength -= read;
                readFrom += read;

                stream.Position = writeAt;
                stream.Write(buffer, 0, read);
                writeAt += read;
            }
            while (readLength > 0);
        }
    }
}
