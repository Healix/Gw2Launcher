using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Tools.Dat.Compression
{
    class Crc32
    {
        public static WeakReference<uint[]> _table;

        private uint sum;
        private uint[] table;

        static Crc32()
        {
            _table = new WeakReference<uint[]>(BuildTable());
        }

        private static uint[] BuildTable()
        {
            var table = new uint[256];
            var h = 0x82F63B78;

            for (uint i = 0; i < 256; i++)
            {
                var v = i;

                for (byte j = 0; j < 8; j++)
                {
                    var b = v & 1;
                    v >>= 1;
                    if (b == 1)
                        v = h ^ v;
                }

                table[i] = v;
            }

            return table;
        }

        public Crc32()
        {
            lock (_table)
            {
                if (!_table.TryGetTarget(out table))
                    _table.SetTarget(table = BuildTable());
            }

            Reset();
        }

        public void Add(byte b)
        {
            sum = table[(byte)sum ^ b] ^ (sum >> 8);
        }

        public void Reset()
        {
            sum = 0xFFFFFFFF;
        }

        public uint CRC
        {
            get
            {
                return sum ^ 0xFFFFFFFF;
            }
        }

        public uint Compute(byte[] data, int offset, int count)
        {
            var crc = 0xFFFFFFFF;

            count += offset;

            for (; offset < count; ++offset)
            {
                crc = table[(byte)crc ^ data[offset]] ^ (crc >> 8);
            }

            crc ^= 0xFFFFFFFF;

            return crc;
        }

        public uint Compute(byte[] data)
        {
            return Compute(data, 0, data.Length);
        }
    }
}
