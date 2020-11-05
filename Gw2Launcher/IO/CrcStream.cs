using System;
using System.IO;

namespace Gw2Launcher.IO
{
    /// <summary>
    /// Calculates a CRC while reading/writing data
    /// </summary>
    class CrcStream : Stream
    {
        private Stream inner;

        private bool keepOpen;
        private ushort crcsum;

        public CrcStream(Stream inner, bool keepOpen)
        {
            this.inner = inner;
            this.keepOpen = keepOpen;
        }

        public override void Close()
        {
            if (!keepOpen)
                inner.Close();
        }

        public ushort CRC
        {
            get
            {
                return crcsum;
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            var r = inner.Read(buffer, offset, count);

            count = offset + r;
            while (offset < count)
            {
                crcsum += buffer[offset++];
            }

            return r;
        }

        public void ResetCRC()
        {
            crcsum = 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            inner.Write(buffer, offset, count);

            count += offset;
            while (offset < count)
            {
                crcsum += buffer[offset++];
            }
        }
    }
}
