using System;
using System.IO;
using System.Collections.Generic;

namespace Gw2Launcher.Tools.Dat.Compression.IO
{
    /// <summary>
    /// Reads/writes bits in int32 chunks
    /// </summary>
    class BitStream : Stream
    {
        private Stream inner;

        private const byte CAPACITY = 32;

        private uint bufferIn, bufferOut;
        private byte bufferInLength, bufferOutLength;
        private byte[] buffer;

        public BitStream(Stream inner)
        {
            this.inner = inner;
            this.buffer = new byte[4];
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
            if (bufferOutLength > 0)
            {
                bufferOut <<= CAPACITY - bufferOutLength;
                bufferOutLength = CAPACITY;
                WriteBuffer();
            }

            inner.Flush();
        }

        public override void Close()
        {
            if (CanWrite)
                Flush();
            inner.Close();

            base.Close();
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
            return inner.Read(buffer, offset, count);
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
        }

        /// <summary>
        /// Writes the specified number of bits
        /// </summary>
        public void WriteBits(uint value, byte bits)
        {
            if (bits != CAPACITY)
                value &= (1U << bits) - 1;

            if (bufferOutLength + bits > CAPACITY)
            {
                var _bits = (byte)(CAPACITY - bufferOutLength);
                bufferOut = bufferOut << _bits | value >> bits - _bits;
                bufferOutLength = CAPACITY;
                WriteBuffer();

                bits -= _bits;
                value &= (uint)((1 << bits) - 1);
            }

            if (bits >= CAPACITY)
                bufferOut = value;
            else
                bufferOut = bufferOut << bits | value;
            bufferOutLength += bits;
            WriteBuffer();
        }

        public void WriteBit(bool value)
        {
            bufferOut <<= 1;
            if (value)
                bufferOut |= 1;
            ++bufferOutLength;
            WriteBuffer();
        }

        public void WriteBits(IEnumerable<bool> values)
        {
            foreach (var v in values)
                WriteBit(v);
        }

        private void WriteBuffer()
        {
            if (bufferOutLength == CAPACITY)
            {
                for (var i = 0; i < 4; i++)
                {
                    buffer[i] = (byte)(bufferOut >> 8 * i);
                }

                inner.Write(buffer, 0, 4);
                bufferOutLength = 0;
            }
        }

        public void Clear()
        {
            bufferInLength = 0;
            bufferOutLength = 0;
        }

        private void ReadBuffer()
        {
            var r = inner.Read(buffer, 0, 4);
            if (r == 0)
                throw new EndOfStreamException();

            uint value = 0;

            for (var i = 0; i < r; i++)
            {
                value |= (uint)buffer[i] << i * 8;
            }

            bufferIn = value;
            bufferInLength = CAPACITY;
        }

        public uint ReadBits(byte bits)
        {
            if (bufferInLength == 0)
                ReadBuffer();

            if (bits <= bufferInLength)
            {
                if (bits == CAPACITY)
                {
                    bufferInLength = 0;
                    return bufferIn;
                }
                else
                {
                    bufferInLength -= bits;
                    return bufferIn >> bufferInLength & (uint)((1 << bits) - 1);
                }
            }
            else
            {
                var v = (bufferIn & (uint)((1 << bufferInLength) - 1)) << bits - bufferInLength;
                bits -= bufferInLength;
                bufferInLength = 0;
                ReadBuffer();
                bufferInLength -= bits;
                return bufferIn >> bufferInLength & (uint)((1 << bits) - 1) | v;
            }

            //uint v = 0;
            //for (var i = 0; i < bits; i++)
            //{
            //    if (ReadBit())
            //        v |= 1U << bits - i - 1;
            //}
            //return v;
        }

        public bool ReadBit()
        {
            if (bufferInLength == 0)
            {
                ReadBuffer();
            }

            --bufferInLength;
            return (bufferIn >> bufferInLength & 1) == 1;
        }
    }
}
