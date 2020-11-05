using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security;
using System.IO;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Security
{
    static class Credentials
    {
        private static readonly object _lock = new object();

        private const int FILE_HEADER = 1920161100;
        private const byte VERSION = 1;
        private const ushort FILE_HEADER_V0 = 942;
        private const string FILE = "users.dat";
        private const string KEY_SETTINGS = "@";

        private enum StorageState : byte
        {
            None = 0,
            Pending = 1,
            Initialized = 2,
        }

        private static StorageState state;

        static Credentials()
        {
            Settings.Encryption.ValueChanged += Encryption_ValueChanged;
        }

        static void Encryption_ValueChanged(object sender, EventArgs e)
        {
            Task.Run(
                delegate
                {
                    try
                    {
                        lock (_lock)
                        {
                            OnCryptoChanged();
                        }
                    }
                    catch { }
                });
        }

        private static Tools.DataCache<string> storage;
        private static Dictionary<string, SecureString> cache;

        private class DataItem : Tools.DataCache.ICacheItem<string>
        {
            public string ID
            {
                get;
                set;
            }

            public byte[] Data
            {
                get;
                set;
            }

            public void ReadFrom(BinaryReader reader, uint length)
            {
                this.Data = reader.ReadBytes((int)length);
            }

            public void WriteTo(BinaryWriter writer)
            {
                if (this.Data != null)
                    writer.Write(this.Data);
            }
        }

        private static bool Upgrade(int header, ushort version)
        {
            List<DataItem> items;

            using (BinaryReader reader = new BinaryReader(new BufferedStream(File.OpenRead(StoragePath), 1024)))
            {
                if (reader.ReadUInt16() != FILE_HEADER_V0)
                    return false;

                var key = new byte[] { 99, 12, 55, 17, 45, 97, 83, 64, 38 };
                var count = reader.ReadUInt16();
                var scope = Settings.EncryptionScope.CurrentUser;
                var o = Settings.Encryption.Value;
                if (o != null)
                    scope = o.Scope;

                items = new List<DataItem>(count + 1);

                using (var crypto = new Cryptography.Crypto())
                {
                    for (int i = 0; i < count; i++)
                    {
                        var name = reader.ReadString();
                        var length = reader.ReadUInt16();
                        var data = reader.ReadBytes(length);

                        data = ProtectedData.Unprotect(data, key, DataProtectionScope.CurrentUser);

                        try
                        {
                            items.Add(new DataItem()
                                {
                                    ID = name,
                                    Data = crypto.Compress(crypto.Encrypt(scope, data), Cryptography.Crypto.CryptoCompressionFlags.All),
                                });
                        }
                        finally
                        {
                            Array.Clear(data, 0, data.Length);
                        }
                    }
                }
            }

            if (items.Count == 0)
                return false;

            GetStorage().Write<DataItem>(items);

            return true;
        }

        private static void OnCryptoChanged()
        {
            var store = GetStorage();
            var items = new List<DataItem>();
            var o = Settings.Encryption.Value;
            var scope = o != null ? o.Scope : Settings.EncryptionScope.CurrentUser;

            using (var crypto = new Cryptography.Crypto())
            {
                try
                {
                    foreach (var item in store.ReadAll<DataItem>())
                    {
                        try
                        {
                            if (crypto.GetScope(item.Data) == scope)
                                continue;

                            var data = crypto.Decrypt(item.Data);

                            try
                            {
                                items.Add(new DataItem()
                                {
                                    ID = item.ID,
                                    Data = crypto.Compress(crypto.Encrypt(scope, data), Cryptography.Crypto.CryptoCompressionFlags.All),
                                });
                            }
                            finally
                            {
                                Array.Clear(data, 0, data.Length);
                            }
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                }
                catch { }
            }

            if (items.Count > 0)
                store.Write<DataItem>(items);
        }

        public static SecureString GetPassword(string username)
        {
            lock (_lock)
            {
                if (cache != null)
                {
                    SecureString s;
                    if (cache.TryGetValue(username, out s))
                        return s;
                }
                else
                {
                    cache = new Dictionary<string, SecureString>(StringComparer.OrdinalIgnoreCase);
                }

                if (state != StorageState.None)
                {
                    try
                    {
                        var i = GetStorage().Read<DataItem>(username);

                        using (var crypto = new Cryptography.Crypto())
                        {
                            SecureString s;

                            if (i.Data.Length > 0)
                            {
                                var data = crypto.Decrypt(i.Data);

                                try
                                {
                                    s = FromByteArray(data);
                                }
                                finally
                                {
                                    Array.Clear(data, 0, data.Length);
                                }
                            }
                            else
                            {
                                s = new SecureString();
                                s.MakeReadOnly();
                            }

                            cache.Add(username, s);

                            return s;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
            }

            return null;
        }

        public static void SetPassword(string username, SecureString password)
        {
            lock (_lock)
            {
                try
                {
                    if (cache == null)
                        cache = new Dictionary<string, SecureString>(StringComparer.OrdinalIgnoreCase);

                    var s = password.Copy();
                    s.MakeReadOnly();

                    cache[username] = s;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }

                if (state != StorageState.None)
                {
                    try
                    {
                        byte[] data;

                        if (password.Length > 0)
                        {
                            var buffer = ToByteArray(password);

                            try
                            {
                                var scope = Settings.EncryptionScope.CurrentUser;
                                var o = Settings.Encryption.Value;
                                if (o != null)
                                    scope = o.Scope;

                                using (var crypto = new Cryptography.Crypto())
                                {
                                    data = crypto.Compress(crypto.Encrypt(scope, buffer), Cryptography.Crypto.CryptoCompressionFlags.All);
                                }
                            }
                            finally
                            {
                                Array.Clear(buffer, 0, buffer.Length);
                            }
                        }
                        else
                        {
                            data = new byte[0];
                        }

                        var item = new DataItem()
                        {
                            ID = username,
                            Data = data,
                        };

                        GetStorage().Write<DataItem>(item);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
            }
        }

        private static Tools.DataCache<string> GetStorage()
        {
            var store = new Tools.DataCache<string>(FILE_HEADER, VERSION, StoragePath, Tools.DataCache.Utf8IgnoreCase);

            if (state == StorageState.Pending)
            {
                state = StorageState.Initialized;

                try
                {
                    try
                    {
                        store.Verify();
                    }
                    catch (Tools.DataCache.UnknownHeaderException e)
                    {
                        Upgrade(e.Header, e.Version);
                    }
                }
                catch 
                {
                    try
                    {
                        File.Delete(StoragePath);
                    }
                    catch { }
                }
            }

            return store;
        }

        public static void Clear()
        {
            lock (_lock)
            {
                if (cache != null)
                    cache.Clear();

                string path = Path.Combine(DataPath.AppData, FILE);
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        public static bool Compare(SecureString a, SecureString b)
        {
            if (a == null || b == null)
                return a == b;
            else if (object.ReferenceEquals(a, b))
                return true;

            int length = a.Length;
            if (b.Length != length)
                return false;

            IntPtr _a = IntPtr.Zero;
            IntPtr _b = IntPtr.Zero;
            try
            {
                _a = Marshal.SecureStringToBSTR(a);
                _b = Marshal.SecureStringToBSTR(b);

                length *= 2;

                for (int i = 0; i < length; i+=2)
                {
                    if (Marshal.ReadInt16(_a, i) != Marshal.ReadInt16(_b, i))
                        return false;
                }

                return true;
            }
            finally
            {
                if (_b != IntPtr.Zero) 
                    Marshal.ZeroFreeBSTR(_b);
                if (_a != IntPtr.Zero) 
                    Marshal.ZeroFreeBSTR(_a);
            }
        }

        private static string StoragePath
        {
            get
            {
                return Path.Combine(DataPath.AppData, FILE);
            }
        }

        public static bool StoreCredentials
        {
            get
            {
                return state != StorageState.None;
            }
            set
            {
                if (value)
                {
                    lock (_lock)
                    {
                        if (state == StorageState.None)
                            state = StorageState.Pending;
                    }
                }
                else
                {
                    try
                    {
                        lock (_lock)
                        {
                            state = StorageState.None;
                            string path = Path.Combine(DataPath.AppData, FILE);
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }

        public static SecureString FromString(ref string str)
        {
            var s = new SecureString();
            foreach (var c in str)
                s.AppendChar(c);
            s.MakeReadOnly();
            return s;
        }

        public static SecureString FromByteArray(byte[] data)
        {
            var s = new SecureString();

            for (int p = 0; p < data.Length; p += 2)
            {
                //data[p] + (data[p + 1] << 8);
                s.AppendChar(BitConverter.ToChar(data, p));
            }

            s.MakeReadOnly();

            return s;
        }

        public static SecureString FromCharArray(char[] data)
        {
            var s = new SecureString();

            foreach (var c in data)
            {
                s.AppendChar(c);
            }

            s.MakeReadOnly();

            return s;
        }

        public static byte[] ToByteArray(SecureString s)
        {
            var buffer = new byte[2 * s.Length];
            var ptr = Marshal.SecureStringToBSTR(s);

            try
            {
                Marshal.Copy(ptr, buffer, 0, buffer.Length);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }

            return buffer;
        }

        public static char[] ToCharArray(SecureString s)
        {
            var buffer = ToByteArray(s);

            try
            {
                var chars = new char[s.Length];
                var c = 0;

                for (int p = 0; p < buffer.Length; p += 2)
                {
                    chars[c++] = BitConverter.ToChar(buffer, p);
                }

                return chars;
            }
            finally
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }
}
