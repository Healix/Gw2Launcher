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
        private static readonly byte[] KEY = new byte[] { 99, 12, 55, 17, 45, 97, 83, 64, 38 };

        private const ushort FILE_HEADER = 942;
        private const string FILE = "users.dat";

        private static bool storeCredentials;

        private static Dictionary<string, SecureString> cache;

        public static SecureString GetPassword(string username)
        {
            string path = Path.Combine(DataPath.AppData, FILE);
            SecureString password = null;

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

                if (storeCredentials)
                {
                    try
                    {
                        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                        {
                            if (reader.ReadUInt16() != FILE_HEADER)
                                return null;

                            ushort count = reader.ReadUInt16();

                            for (int i = 0; i < count; i++)
                            {
                                string name = reader.ReadString();
                                ushort length = reader.ReadUInt16();

                                if (cache.ContainsKey(name))
                                {
                                    reader.BaseStream.Position += length;
                                }
                                else
                                {
                                    if (length > 0)
                                    {
                                        byte[] data = reader.ReadBytes(length);

                                        try
                                        {
                                            data = ProtectedData.Unprotect(data, KEY, DataProtectionScope.CurrentUser);
                                            SecureString s = new SecureString();

                                            for (int p = 0; p < data.Length; p += 2)
                                            {
                                                //data[p] + (data[p + 1] << 8);
                                                s.AppendChar(BitConverter.ToChar(data, p));
                                            }

                                            s.MakeReadOnly();

                                            cache.Add(name, s);

                                            if (username.Equals(name, StringComparison.OrdinalIgnoreCase))
                                            {
                                                password = s;
                                            }
                                        }
                                        catch
                                        {
                                        }
                                        finally
                                        {
                                            Array.Clear(data, 0, data.Length);
                                        }
                                    }
                                    else
                                    {
                                        SecureString s = new SecureString();
                                        s.MakeReadOnly();

                                        cache.Add(name, s);

                                        if (username.Equals(name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            password = s;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return password;
        }

        public static void SetPassword(string username, SecureString password)
        {
            string path = Path.Combine(DataPath.AppData, FILE);

            byte[] passwordData;

            if (password.Length > 0)
            {
                byte[] buffer = new byte[2 * password.Length];
                IntPtr ptr = Marshal.SecureStringToBSTR(password);

                try
                {
                    Marshal.Copy(ptr, buffer, 0, buffer.Length);
                }
                finally
                {
                    Marshal.ZeroFreeBSTR(ptr);
                }

                try
                {
                    passwordData = ProtectedData.Protect(buffer, KEY, DataProtectionScope.CurrentUser);
                }
                finally
                {
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
            else
            {
                passwordData = new byte[0];
            }

            lock (_lock)
            {
                try
                {
                    if (cache == null)
                        cache = new Dictionary<string, SecureString>(StringComparer.OrdinalIgnoreCase);

                    SecureString s = password.Copy();
                    s.MakeReadOnly();

                    cache[username] = s;
                }
                catch { }

                if (storeCredentials)
                {
                    try
                    {
                        using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)))
                        {
                            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)))
                            {
                                bool hasHeader = false;

                                try
                                {
                                    hasHeader = reader.ReadUInt16() == FILE_HEADER;
                                    if (!hasHeader)
                                        reader.BaseStream.Position = 0;
                                }
                                catch { }

                                ushort count;

                                if (hasHeader)
                                {
                                    long position = reader.BaseStream.Position;
                                    bool hasUsername = false;
                                    count = reader.ReadUInt16();

                                    for (int i = 0; i < count; i++)
                                    {
                                        if (!hasUsername)
                                            writer.BaseStream.Position = reader.BaseStream.Position;

                                        string name = reader.ReadString();
                                        ushort length = reader.ReadUInt16();
                                        byte[] data = reader.ReadBytes(length);

                                        if (hasUsername)
                                        {
                                            writer.Write(name);
                                            writer.Write(length);
                                            writer.Write(data);
                                        }
                                        else if (name.Equals(username, StringComparison.OrdinalIgnoreCase))
                                        {
                                            //all users past this user will be shifted up
                                            //this user will be moved to the end of the file
                                            hasUsername = true;
                                        }
                                    }

                                    if (!hasUsername)
                                        writer.BaseStream.Position = reader.BaseStream.Position;

                                    writer.Write(username);
                                    writer.Write((ushort)passwordData.Length);
                                    writer.Write(passwordData);

                                    if (writer.BaseStream.Position < writer.BaseStream.Length)
                                        writer.BaseStream.SetLength(writer.BaseStream.Position);

                                    if (!hasUsername)
                                    {
                                        count++;

                                        writer.BaseStream.Position = position;
                                        writer.Write(count);
                                    }
                                }
                                else
                                {
                                    writer.Write(FILE_HEADER);
                                    writer.Write((ushort)1);

                                    writer.Write(username);
                                    writer.Write((ushort)passwordData.Length);
                                    writer.Write(passwordData);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
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

            int length = a.Length;
            if (b.Length != length)
                return false;

            IntPtr _a = IntPtr.Zero;
            IntPtr _b = IntPtr.Zero;
            try
            {
                _a = Marshal.SecureStringToBSTR(a);
                _b = Marshal.SecureStringToBSTR(b);

                for (int i = 0; i < length; i++)
                {
                    if (Marshal.ReadByte(_a, i) != Marshal.ReadByte(_b, i))
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

        public static bool StoreCredentials
        {
            get
            {
                return storeCredentials;
            }
            set
            {
                storeCredentials = value;

                if (!value)
                {
                    try
                    {
                        lock (_lock)
                        {
                            if (File.Exists(FILE))
                                File.Delete(FILE);
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
