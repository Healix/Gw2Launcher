using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Gw2Launcher.Tools
{
    static class Totp
    {
        /// <summary>
        /// The duration a generated code lasts for (30 seconds)
        /// </summary>
        public const long OTP_LIFESPAN_TICKS = 300000000;
        private const long UNIX_EPOCH = 621355968000000000; //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks

        public static bool IsValid(string key)
        {
            if (key.Length != 16)
                return false;

            foreach (var c in key)
            {
                if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '2' && c <= '7')
                {

                }
                else
                    return false;
            }

            return true;
        }

        public static byte[] Decode(string key)
        {
            //base32 A-Z 2-7, 5 bits per character

            var kl = key.Length;
            var bytes = new byte[kl * 5 / 8]; //not handling undersized keys
            sbyte offset = 8;
            int buffer = 0;

            for (var i = 0; i < kl; i++)
            {
                int val;
                var c = key[i];

                val = c - 'A';
                if (val <= 26)
                {
                    if (val < 0)
                        val += 41; //digit
                }
                else
                {
                    val -= 32; //lower case character
                }

                offset -= 5;

                if (offset < 0)
                {
                    if (val == 0)
                    {
                        bytes[(i * 5) / 8] = (byte)buffer;
                        offset += 8;
                        buffer = 0;
                    }
                    else
                    {
                        var v = val >> -offset;

                        bytes[(i * 5) / 8] = (byte)(buffer | v);
                        offset += 8;
                        buffer = (val << offset) & 255;
                    }

                }
                else if (offset == 0)
                {
                    bytes[(i * 5) / 8] = (byte)(buffer | val);
                    buffer = 0;
                    offset = 8;
                }
                else
                {
                    buffer |= val << offset;
                }
            }

            return bytes;
        }

        public static string Encode(byte[] key)
        {
            int kl = key.Length;
            var chars = new char[kl * 8 / 5];
            var ci = 0;
            byte buffered = 0;
            byte buffer = 0;

            for (var i = 0; i < kl; i++)
            {
                var b = key[i];
                int remaining = 8;

                do
                {
                    int val;

                    remaining -= (5 - buffered);

                    if (remaining < 0)
                    {
                        buffered = (byte)(5 + remaining);
                        buffer = (byte)(b << -remaining);
                    }
                    else
                    {
                        val = b >> remaining;
                        b &= (byte)(255 >> (8 - remaining));

                        if (buffered > 0)
                        {
                            val |= buffer;
                            buffered = 0;
                        }

                        char c;
                        if (val >= 26)
                            c = (char)(val + '2' - 26);
                        else
                            c = (char)(val + 'A');
                        chars[ci++] = c;
                    }
                }
                while (remaining > 0);
            }

            return new string(chars);
        }

        public static char[] Generate(byte[] key)
        {
            return Generate(key, DateTime.UtcNow.Ticks);
        }

        public static char[] Generate(byte[] key, long ticks)
        {
            return GetCode(key, (ticks - UNIX_EPOCH) / 10000000, 6);
        }

        private static char[] GetCode(byte[] key, long epoch, byte length)
        {
            var buffer = BitConverter.GetBytes(epoch / 30);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            using (var hmac = new HMACSHA1(key))
            {
                buffer = hmac.ComputeHash(buffer);
            }

            var offset = buffer[buffer.Length - 1] & 0xF;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer, offset, 4);

            var code = (BitConverter.ToInt32(buffer, offset) & 0x7FFFFFFF);
            var chars = new char[length];

            do
            {
                chars[--length] = (char)('0' + (code % 10));
                code /= 10;
            }
            while (length != 0);

            return chars;
        }
    }
}
