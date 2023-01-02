using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace Gw2Launcher.Security.Cryptography
{
    public class ProtectedString : IDisposable
    {
        private SecureString value;
        private byte[] data;
        private Cryptography.Crypto crypto;

        public ProtectedString(SecureString s)
        {
            this.value = s;
        }

        public ProtectedString(Crypto crypto, byte[] data)
        {
            this.crypto = crypto;
            this.data = data;
        }

        /// <summary>
        /// Returns if the value is readily available
        /// </summary>
        public bool IsDecompressed
        {
            get
            {
                return value != null;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (value != null)
                {
                    return value.Length == 0;
                }
                else
                {
                    return data == null || data.Length == 0;
                }
            }
        }

        public SecureString ToSecureString()
        {
            if (value == null && data != null)
            {
                var buffer = crypto.Decrypt(data);
                try
                {
                    value = Credentials.FromByteArray(buffer);
                }
                finally
                {
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }

            return value;
        }

        /// <summary>
        /// Turns the protected string into a compressed encrpyted array
        /// </summary>
        /// <param name="crypto">Crypto source</param>
        /// <param name="apply">True to retain the crypto source and compressed array</param>
        public byte[] ToArray(Crypto crypto, bool apply = false)
        {
            byte[] buffer;

            if (this.data != null)
            {
                if (this.crypto.IsEqual(crypto))
                {
                    return this.data;
                }

                buffer = this.crypto.Decrypt(data);
            }
            else if (this.value != null)
            {
                buffer = Credentials.ToByteArray(this.value);
            }
            else
            {
                return new byte[0];
            }

            try
            {
                var b = crypto.Compress(crypto.Encrypt(buffer));

                if (apply)
                {
                    this.data = b;
                    this.crypto = crypto;
                }

                return b;
            }
            finally
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        public void Dispose()
        {
            if (value != null)
            {
                value.Dispose();
                value = null;
            }
        }
    }
}
