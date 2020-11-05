using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class Bits
    {
        public static byte GetBits(byte source, byte offset)
        {
            return (byte)(source >> offset & 1);
        }

        public static byte GetBits(byte source, byte offset, byte length)
        {
            return (byte)(source >> offset & ~(255 << length));
        }

        public static byte SetBits(byte source, byte value, byte offset, byte length)
        {
            return (byte)(value << offset | source & ~(~(255 << length) << offset));
        }

        public static string ToString(byte source)
        {
            char[] c = new char[8];

            for (var i = 0; i < 8; i++)
            {
                c[7 - i] = (source >> i & 1) == 1 ? '1' : '0';
            }

            return new string(c);
        }
    }
}
