using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;

namespace Gw2Launcher.Security.Cryptography
{
    public class Crypto : IDisposable
    {
        private const byte VERSION = 1;

        [Flags]
        public enum CryptoCompressionFlags
        {
            Data = 1,
            Key = 2,
            IV = 4,

            All = 7,
        }

        private const int IV_LENGTH = 16;
        private const int KEY_LENGTH = 32;

        public interface IData
        {
            byte[] Data
            {
                get;
            }

            Settings.EncryptionScope Scope
            {
                get;
            }
        }

        private class CryptoData : IData
        {
            public Settings.EncryptionScope scope;
            public byte[] data, key, iv;

            public CryptoData(Settings.EncryptionScope scope, byte[] data, byte[] key, byte[] iv)
            {
                this.scope = scope;
                this.data = data;
                this.key = key;
                this.iv = iv;
            }

            public byte[] Data
            {
                get
                {
                    return this.data;
                }
            }

            public Settings.EncryptionScope Scope
            {
                get
                {
                    return scope;
                }
            }
        }

        private Aes aes;
        private Random random;
        private byte[] key;
        private Settings.EncryptionScope scope;
        private CryptoCompressionFlags flags;

        public Crypto()
        {
            this.scope = Settings.EncryptionScope.CurrentUser;
            this.flags = CryptoCompressionFlags.All;
        }

        public Crypto(Settings.EncryptionScope scope, CryptoCompressionFlags flags, byte[] key)
        {
            this.scope = scope;
            this.flags = flags;
            this.key = key;
        }

        private Aes GetAes()
        {
            if (aes == null)
            {
                aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.FeedbackSize = 8;
            }

            return aes;
        }

        public static byte[] GenerateCryptoKey(int length = KEY_LENGTH)
        {
            return new Crypto().GenerateKey(length);
        }

        public byte[] GenerateKey(int length = KEY_LENGTH)
        {
            var buffer = new byte[length];
            if (random == null)
                random = new Random();
            random.NextBytes(buffer);
            return buffer;
        }

        public byte[] GenerateIV(int length = IV_LENGTH)
        {
            return GenerateKey(length);
        }

        public Settings.EncryptionScope GetScope(byte[] compressed)
        {
            if (compressed[0] == 0)
            {
                return (Settings.EncryptionScope)compressed[1];
            }
            else
            {
                var b = compressed[0] ^ compressed[compressed.Length - 3];
                return (Settings.EncryptionScope)(b >> 4);
            }
        }

        /// <summary>
        /// Decompresses the data
        /// </summary>
        /// <param name="buffer">Compressed data</param>
        /// <returns>Decompressed data</returns>
        public IData Decompress(byte[] buffer)
        {
            if (buffer[0] == 0)
            {
                var i = 1;
                var scope = (Settings.EncryptionScope)buffer[i++];
                int ivLength = buffer[i++],
                    keyLength = buffer[i++];
                byte[] data, key, iv;

                if (ivLength > 0)
                {
                    iv = new byte[ivLength];
                    Array.Copy(buffer, i, iv, 0, ivLength);
                    i += ivLength;
                }
                else
                {
                    iv = null;
                }

                if (keyLength > 0)
                {
                    key = new byte[keyLength];
                    Array.Copy(buffer, i, key, 0, keyLength);
                    i += keyLength;
                }
                else
                {
                    key = null;
                }

                var dataLength = buffer.Length - i;
                if (dataLength > 0)
                {
                    data = new byte[dataLength];
                    Array.Copy(buffer, i, data, 0, dataLength);
                }
                else
                {
                    data = null;
                }

                return new CryptoData(scope, data, key, iv);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Compresses the data into a single byte array
        /// </summary>
        /// <param name="scope">Encryption scope</param>
        /// <param name="data">Optional inclusion of the data</param>
        /// <param name="key">Optional inclusion of the key</param>
        /// <param name="iv">Optional inclusion of the IV</param>
        /// <returns>Byte array of compressed data</returns>
        private byte[] Compress(Settings.EncryptionScope scope, byte[] data, byte[] key, byte[] iv)
        {
            var ivLength = iv != null ? iv.Length : 0;
            var keyLength = key != null ? key.Length : 0;
            var dataLength = data != null ? data.Length : 0;
            var buffer = new byte[4 + dataLength + ivLength + keyLength];
            var i = 1;

            buffer[i++] = (byte)scope;
            buffer[i++] = (byte)ivLength;
            buffer[i++] = (byte)keyLength;

            foreach (var b in new byte[][] { iv, key, data })
            {
                if (b != null && b.Length > 0)
                {
                    Array.Copy(b, 0, buffer, i, b.Length);
                    i += b.Length;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Compress the data into a single byte array
        /// </summary>
        /// <param name="data">Data to compress</param>
        /// <param name="flags">Which parts of the data to include</param>
        /// <returns>Byte array of compressed data</returns>
        public byte[] Compress(IData data, CryptoCompressionFlags flags)
        {
            var d = (CryptoData)data;

            var _data = d.data;
            var key = d.key;
            var iv = d.iv;

            if ((flags & CryptoCompressionFlags.All) != CryptoCompressionFlags.All)
            {
                if ((flags & CryptoCompressionFlags.Data) != CryptoCompressionFlags.Data)
                    _data = null;
                if ((flags & CryptoCompressionFlags.IV) != CryptoCompressionFlags.IV)
                    iv = null;
                if ((flags & CryptoCompressionFlags.Key) != CryptoCompressionFlags.Key)
                    key = null;
            }

            return Compress(d.scope, _data, key, iv);
        }

        /// <summary>
        /// Compresses the data into a single byte array using options provided at initialization
        /// </summary>
        public byte[] Compress(IData data)
        {
            return Compress(data, flags);
        }

        /// <summary>
        /// Encrypts the data
        /// </summary>
        /// <param name="scope">Encryption scope</param>
        /// <param name="data">Data to encrypt</param>
        /// <param name="key">Optional key to use</param>
        /// <param name="iv">Optional IV to use</param>
        /// <returns>Encrypted data that may be further processed</returns>
        public IData Encrypt(Settings.EncryptionScope scope, byte[] data, byte[] key = null, byte[] iv = null)
        {
            byte[] buffer;

            switch (scope)
            {
                case Settings.EncryptionScope.CurrentUser:

                    if (iv == null)
                        iv = GenerateIV();
                    buffer = ProtectedData.Protect(data, iv, DataProtectionScope.CurrentUser);

                    break;
                case Settings.EncryptionScope.LocalMachine:

                    if (iv == null)
                        iv = GenerateIV();
                    buffer = ProtectedData.Protect(data, iv, DataProtectionScope.LocalMachine);

                    break;
                case Settings.EncryptionScope.Portable:

                    if (key == null)
                        key = GenerateKey();
                    if (iv == null)
                        iv = GenerateIV();
                    using (var aes = GetAes().CreateEncryptor(key, iv))
                    {
                        using (var ms = new MemoryStream((data.Length / aes.OutputBlockSize + 1) * aes.OutputBlockSize))
                        {
                            using (var cs = new CryptoStream(ms, aes, CryptoStreamMode.Write))
                            {
                                cs.Write(data, 0, data.Length);
                                cs.FlushFinalBlock();
                            }
                            buffer = ms.ToArray();
                        }
                    }

                    break;
                case Settings.EncryptionScope.Unencrypted:
                default:

                    buffer = data;

                    break;
            }

            return new CryptoData(scope, buffer, key, iv);
        }

        /// <summary>
        /// Encrypts the data using options provided at initialization
        /// </summary>
        public IData Encrypt(byte[] data)
        {
            return Encrypt(scope, data, key);
        }

        /// <summary>
        /// Decrypts the data
        /// </summary>
        /// <param name="scope">Encryption scope</param>
        /// <param name="data">Data to decrypt</param>
        /// <param name="key">Key used</param>
        /// <param name="iv">IV used</param>
        /// <returns>Decrypted data</returns>
        public byte[] Decrypt(Settings.EncryptionScope scope, byte[] data, byte[] key, byte[] iv)
        {
            switch (scope)
            {
                case Settings.EncryptionScope.CurrentUser:

                    return ProtectedData.Unprotect(data, iv, DataProtectionScope.CurrentUser);

                case Settings.EncryptionScope.LocalMachine:

                    return ProtectedData.Unprotect(data, iv, DataProtectionScope.LocalMachine);

                case Settings.EncryptionScope.Portable:

                    if (key == null)
                        key = this.key;
                    using (var aes = GetAes().CreateDecryptor(key, iv))
                    {
                        using (var cs = new CryptoStream(new MemoryStream(data), aes, CryptoStreamMode.Read))
                        {
                            var buffer = new byte[data.Length];
                            var length = 0;

                            do
                            {
                                var r = cs.Read(buffer, length, buffer.Length - length);
                                if (r == 0)
                                    break;
                                length += r;
                            }
                            while (true);

                            if (length != buffer.Length)
                            {
                                var buffer2 = new byte[length];
                                Array.Copy(buffer, buffer2, length);
                                Array.Clear(buffer, 0, length);
                                buffer = buffer2;
                            }

                            return buffer;
                        }
                    }

                case Settings.EncryptionScope.Unencrypted:
                default:

                    return data;
            }
        }

        /// <summary>
        /// Decrypts the data
        /// </summary>
        /// <param name="data">Data to decrypt</param>
        /// <param name="key">Optional key</param>
        /// <param name="iv">Optional IV</param>
        /// <returns>Decrypted data</returns>
        public byte[] Decrypt(IData data, byte[] key = null, byte[] iv = null)
        {
            var d = (CryptoData)data;
            return Decrypt(d.scope, d.data, d.key, d.iv);
        }

        /// <summary>
        /// Decrypts the data
        /// </summary>
        /// <param name="data">Data to decrypt</param>
        /// <param name="key">Key used</param>
        /// <param name="iv">IV used</param>
        /// <returns>Decrypted data</returns>
        public byte[] Decrypt(IData data, IData key, IData iv)
        {
            var d = (CryptoData)data;

            byte[] _key = d.key,
                   _iv = d.iv;

            if (key != null)
                _key = ((CryptoData)key).key;

            if (iv != null)
                _iv = ((CryptoData)iv).iv;

            return Decrypt(d.scope, d.data, _key, _iv);
        }

        /// <summary>
        /// Decrypts data that has been compressed into a single byte array
        /// </summary>
        /// <param name="compressed">Compressed data</param>
        /// <returns>Decrypted data</returns>
        public byte[] Decrypt(byte[] compressed)
        {
            return Decrypt(Decompress(compressed));
        }

        /// <summary>
        /// Returns if both instance are using the same options
        /// </summary>
        public bool IsEqual(Crypto crypto)
        {
            return this.scope == crypto.scope && this.flags == crypto.flags && this.key == crypto.key;
        }

        public void Dispose()
        {
            if (aes != null)
            {
                aes.Dispose();
                aes = null;
            }
        }
    }
}
