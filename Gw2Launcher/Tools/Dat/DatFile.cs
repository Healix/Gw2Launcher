using Gw2Launcher.Tools.Dat.Compression;
using System;
using System.IO;

namespace Gw2Launcher.Tools.Dat
{
    static class DatFile
    {
        public class MftIdEntry
        {
            public int baseId, fileId;
        }

        public class MftEntry : MftIdEntry
        {
            public long offset;
            public int size;
            public ushort compression;
            public ushort flags;
            public uint counter;
            public uint crc;

            public static MftEntry Read(BinaryReader reader)
            {
                var entry = new MftEntry()
                {
                    offset = reader.ReadInt64(),
                    size = reader.ReadInt32(),
                    compression = reader.ReadUInt16(),
                    flags = reader.ReadUInt16(),
                    counter = reader.ReadUInt32(),
                    crc = reader.ReadUInt32(),
                };

                return entry;
            }
        }

        public class Mft
        {
            public byte[] header;

            public int BlockSize
            {
                get
                {
                    return BitConverter.ToInt32(header, 12);
                }
            }

            public long MftOffset
            {
                get
                {
                    return BitConverter.ToInt64(header, 24);
                }
            }

            public int MftSize
            {
                get
                {
                    return BitConverter.ToInt32(header, 32);
                }
            }

            public MftEntry[] entries;
        }

        public static Mft ReadMft(BinaryReader r)
        {
            var mft = new Mft();

            mft.header = r.ReadBytes(40);

            //check version and header size
            var header = BitConverter.ToUInt32(mft.header, 0);
            if ((header != 441336215U //US
                && header != 441336225U) //CN
                || BitConverter.ToInt32(mft.header, 4) != 40)
                throw new IOException("Unknown header");

            r.BaseStream.Position = mft.MftOffset;

            if (r.ReadUInt32() != 443835981U)
                throw new IOException("Unknown MFT header");
            r.BaseStream.Position += 8; //junk

            var count = r.ReadUInt32() - 1; //this header was counted as an entry
            r.BaseStream.Position += 8; //0

            var entries = mft.entries = new MftEntry[count];
            for (var i = 0; i < count; i++)
            {
                entries[i] = MftEntry.Read(r);
            }

            var e = entries[1];
            if (e.compression != 0)
                throw new IOException("File is compressed");

            var size = e.size;
            r.BaseStream.Position = e.offset;

            while (size > 0)
            {
                var id = r.ReadInt32();
                var i = r.ReadInt32();

                if (i > 0)
                {
                    var entry = entries[i - 1];
                    if (entry.baseId == 0)
                    {
                        entry.baseId = id;
                        entry.fileId = id;
                    }
                    else if (id < entry.baseId)
                        entry.baseId = id;
                    else if (id > entry.fileId)
                        entry.fileId = id;
                }

                size -= 8;
            }

            return mft;
        }

        public static MftIdEntry[] Read(string path)
        {
            MftIdEntry[] entries;

            using (var r = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                int size, compression;
                long offset;

                if (r.ReadUInt32() != 441336215)
                    throw new NotSupportedException("Unknown header");
                
                r.BaseStream.Position = 24;
                
                offset = r.ReadInt64();
                size = r.ReadInt32();

                //earch entry is 24 bytes - skipping to the 3rd entry, which points to the ids
                r.BaseStream.Position = offset + 48;

                entries = new MftIdEntry[size / 24];

                offset = r.ReadInt64();
                size = r.ReadInt32();
                compression = r.ReadInt16();

                if (compression != 0)
                    throw new IOException("File is compressed");

                //each entry is 8 bytes (file id + index)
                r.BaseStream.Position = offset;

                while (size > 0)
                {
                    var id = r.ReadInt32();
                    var i = r.ReadInt32();

                    var entry = entries[i];
                    if (entry == null)
                    {
                        entries[i] = entry = new MftIdEntry()
                        {
                            baseId = id,
                            fileId = id
                        };
                    }
                    else
                    {
                        if (id < entry.baseId)
                            entry.baseId = id;
                        if (id > entry.fileId)
                            entry.fileId = id;
                    }

                    size -= 8;
                }
            }

            return entries;
        }

        /// <summary>
        /// Reads the build from Local.dat
        /// </summary>
        public static int ReadBuild(string path)
        {
            using (var r = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                var mft = ReadMft(r);

                foreach (var e in mft.entries)
                {
                    if (e.baseId == 16)
                    {
                        r.BaseStream.Position = e.offset;

                        var a = new Archive();
                        var buffer = a.Decompress(r.BaseStream, e.size);

                        using (var r2 = new BinaryReader(new MemoryStream(buffer)))
                        {
                            if (r2.ReadUInt16() != 18000) //PF
                                throw new IOException("Unknown header");
                            r2.BaseStream.Position += 4;
                            if (r2.ReadUInt16() != 12)
                                throw new IOException("Unknown version");
                            if (r2.ReadUInt32() != 1818455916U) //locl
                                throw new IOException("Unknown header");

                            while (r2.BaseStream.Position < buffer.Length)
                            {
                                var chunkType = r2.ReadUInt32();
                                var chunkSize = r2.ReadUInt32();
                                var chunkEnd = r2.BaseStream.Position + chunkSize;

                                if (chunkType == 1701998435U) //core
                                {
                                    var chunkVersion = r2.ReadUInt16();
                                    var chunkHeaderSize = r2.ReadUInt16();
                                    var chunkTableOffset = r2.ReadUInt32();

                                    if (chunkVersion != 0)
                                        throw new IOException("Unknown core version");

                                    return r2.ReadInt32();
                                }

                                r2.BaseStream.Position = chunkEnd;
                            }
                        }

                        break;
                    }
                }
            }

            throw new IOException("Build not found");
        }
    }
}
