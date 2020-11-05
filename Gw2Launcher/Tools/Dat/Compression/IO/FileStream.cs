using System;
using System.IO;

namespace Gw2Launcher.Tools.Dat.Compression.IO
{
    /// <summary>
    /// Chunked data stream including a 4-byte CRC at the end of each block
    /// </summary>
    class FileStream : Stream
    {
        private Stream inner;

        private long startPosition;
        private int chunkLength;
        private Crc32 crc32;
        private bool keepOpen, computeCrc, autoWriteCrc;
        private long lastCrc;

        public FileStream(Stream inner, bool keepOpen, bool computeCrc, bool autoWriteCrc)
        {
            this.inner = inner;
            this.startPosition = inner.Position;
            this.chunkLength = 65536;
            this.keepOpen = keepOpen;
            this.autoWriteCrc = autoWriteCrc;
            if (this.computeCrc = computeCrc)
            {
                crc32 = new Crc32();
                lastCrc = inner.Position;
            }
        }

        /// <summary>
        /// Size of the data chunks, where the last 4 bytes is the crc, or 0 if chunks aren't used
        /// </summary>
        public int BlockSize
        {
            get
            {
                return chunkLength;
            }
            set
            {
                chunkLength = value;
            }
        }

        public override void Close()
        {
            if (CanWrite)
                Flush();
            if (!keepOpen)
                inner.Close();
            base.Close();
        }

        public uint CRC
        {
            get
            {
                return crc32.CRC;
            }
        }

        public bool ComputeCRC
        {
            get
            {
                return computeCrc;
            }
            set
            {
                computeCrc = value;
                if (value && crc32 == null)
                    crc32 = new Crc32();
            }
        }

        public bool AutoWriteCRC
        {
            get
            {
                return autoWriteCrc;
            }
            set
            {
                autoWriteCrc = value;
            }
        }

        public override bool CanRead
        {
            get
            {
                return inner.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return inner.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return inner.CanWrite;
            }
        }

        public override void Flush()
        {
            if (computeCrc && autoWriteCrc && lastCrc != inner.Position)
            {
                WriteCRC();
            }

            inner.Flush();
        }

        public override long Length
        {
            get
            {
                return inner.Length;
            }
        }

        public override long Position
        {
            get
            {
                return inner.Position;
            }
            set
            {
                inner.Position = value;
            }
        }

        private int ReadBytes(byte[] buffer, int offset, int count, bool compute)
        {
            var t = 0;

            do
            {
                var r = inner.Read(buffer, offset, count);

                if (r == 0)
                    break;

                if (compute)
                {
                    for (var i = 0; i < r; i++)
                    {
                        crc32.Add(buffer[offset + i]);
                    }
                }

                count -= r;
                t += r;
            }
            while (count > 0);

            return t;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (chunkLength > 0)
            {
                int total = 0;

                while (count > 0)
                {
                    var remaining = chunkLength - 4 - (int)((inner.Position - startPosition) % chunkLength);

                    if (remaining < count)
                    {
                        if (remaining > 0)
                        {
                            var r = ReadBytes(buffer, offset, remaining, computeCrc);

                            if (r < remaining)
                                return r + total;

                            total += r;
                            offset += r;
                            count -= r;
                        }

                        if (computeCrc)
                            ReadCRC(buffer, offset, count);
                        else
                            inner.Position += 4;
                    }
                    else
                    {
                        return ReadBytes(buffer, offset, count, computeCrc) + total;
                    }
                }

                return total;
            }
            else
            {
                return ReadBytes(buffer, offset, count, computeCrc);
            }
        }

        public void ResetCRC()
        {
            crc32.Reset();
            this.startPosition = inner.Position;
        }

        public void WriteCRC()
        {
            var crc = BitConverter.GetBytes(crc32.CRC);
            inner.Write(crc, 0, 4);
            lastCrc = inner.Position;
        }

        public void ReadCRC()
        {
            ReadCRC(new byte[4], 0, 4);
        }

        private void ReadCRC(byte[] buffer, int offset, int count)
        {
            if (count < 4)
            {
                offset = 0;
                count = 4;
                buffer = new byte[4];
            }

            var r = ReadBytes(buffer, offset, count, false);
            if (r < 4)
                throw new EndOfStreamException("Failed to read CRC");

            if (computeCrc)
            {
                var crc = BitConverter.ToUInt32(buffer, 0);
                if (crc != crc32.CRC)
                    throw new IOException("CRC doesn't match");
                crc32.Reset();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            inner.SetLength(value);
        }

        private void WriteBytes(byte[] buffer, int offset, int count, bool compute)
        {
            if (compute)
            {
                for (var i = 0; i < count; i++)
                {
                    crc32.Add(buffer[offset + i]);
                }
            }

            inner.Write(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (autoWriteCrc && chunkLength > 0)
            {
                while (count > 0)
                {
                    var remaining = chunkLength - 4 - (int)((inner.Position - startPosition) % chunkLength);

                    if (remaining < count)
                    {
                        if (remaining > 0)
                        {
                            WriteBytes(buffer, offset, remaining, computeCrc);

                            offset += remaining;
                            count -= remaining;
                        }

                        var crc = BitConverter.GetBytes(crc32.CRC);
                        inner.Write(crc, 0, 4);
                        crc32.Reset();

                        lastCrc = inner.Position;
                    }
                    else
                    {
                        WriteBytes(buffer, offset, count, computeCrc);

                        break;
                    }
                }
            }
            else
            {
                WriteBytes(buffer, offset, count, computeCrc);
            }
        }
    }
}
