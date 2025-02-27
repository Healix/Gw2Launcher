using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.IO
{
    class BlockStream : Stream
    {
        private class BlockDisposable : IDisposable
        {
            private BlockStream parent;
            private bool write;

            public BlockDisposable(BlockStream parent, bool write)
            {
                this.parent = parent;
                this.write = write;
            }

            public void Dispose()
            {
                if (parent != null)
                {
                    if (write)
                        parent.EndWriteBlock();
                    else
                        parent.EndReadBlock();
                }
            }
        }

        private Stream inner;
        private MemoryStream ms;
        private long blockPosition;
        private uint blockLength;
        private bool leaveOpen;

        public BlockStream(Stream inner, bool leaveOpen)
        {
            this.inner = inner;
            this.leaveOpen = leaveOpen;
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

        public IDisposable BeginWriteBlock()
        {
            if (ms != null)
            {
                if (ms.Length > 0)
                {
                    EndWriteBlock();
                }
            }
            else
            {
                ms = new MemoryStream();
            }

            blockPosition = inner.Position;
            blockLength = 0;

            ms.Position = sizeof(uint); //length

            return new BlockDisposable(this, true);
        }

        public void EndWriteBlock()
        {
            ms.Position = 0;
            
            var bytes = BitConverter.GetBytes((uint)ms.Length - sizeof(uint));
            ms.Write(bytes, 0, bytes.Length);
            
            ms.Position = 0;
            ms.CopyTo(inner);
            ms.SetLength(0);
        }

        public IDisposable BeginReadBlock()
        {
            if (blockLength > 0)
            {
                EndReadBlock();
            }

            var buffer = new byte[sizeof(uint)];
            var i = 0;

            do
            {
                var r = inner.Read(buffer, 0, buffer.Length - i);

                if (r == 0)
                {
                    throw new IOException("Unable to read block length");
                }

                i += r;
            }
            while (i < buffer.Length);

            blockLength = BitConverter.ToUInt32(buffer, 0);
            blockPosition = inner.Position;

            return new BlockDisposable(this, false);
        }

        public void EndReadBlock()
        {
            inner.Position = blockPosition + blockLength;
            blockLength = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = blockLength - (inner.Position - blockPosition);

            if (count > remaining)
                throw new EndOfStreamException("Read beyond block length");

            return inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (ms != null && ms.Position > 0)
            {
                ms.SetLength(value);
            }
            else
            {
                inner.SetLength(value);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (ms != null && ms.Position > 0)
            {
                ms.Write(buffer, offset, count);
            }
            else
            {
                inner.Write(buffer, offset, count);
            }
        }

        public override void Flush()
        {
            inner.Flush();
        }

        public override void Close()
        {
            if (!leaveOpen)
            {
                base.Close();
            }
        }
    }
}
