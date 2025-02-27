using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Xml;

namespace Gw2Launcher.Tools.Backup
{
    class AccountExporter
    {
        public enum AccountType
        {
            Any,
            Gw1,
            Gw2,
        }

        [Flags]
        public enum FieldFlags
        {
            None = 0,
            Default = 1,
            Authenticated = 2,
            Hidden = 4,
        }

        public enum FileType
        {
            Auto = 0,
            Xml = 1,
            Csv = 2,
        }

        public enum EncodingType
        {
            None = 0,
            Text = 1,
            Encoded = 2,
        }

        public static class Tags
        {
            public const string UID = "id";
            public const string TYPE = "account_type";
            public const string NAME = "name";
            public const string EMAIL = "email";
            public const string PASSWORD = "password";
            public const string TOTP_KEY = "totp_key";
            public const string CREATED = "created_utc";
            public const string LAST_USED = "lastused_utc";
            public const string API_KEY = "api_key";
            public const string COLOR = "color";
            public const string DAT_PATH = "dat_path";
            public const string GFX_PATH = "gfx_path";
            public const string ARGS = "arguments";
        }

        public class ImportData
        {
            public ImportData(List<AccountData> accounts, FieldData[] fields, int unknownFields)
            {
                this.Accounts = accounts;
                this.Fields = fields;
                this.UnknownFields = unknownFields;
            }

            public List<AccountData> Accounts
            {
                get;
                private set;
            }

            public FieldData[] Fields
            {
                get;
                private set;
            }

            public int UnknownFields
            {
                get;
                private set;
            }
        }

        public class AccountData
        {
            public AccountData(List<FieldValue> values)
            {
                this.Values = values;
            }

            public List<FieldValue> Values
            {
                get;
                private set;
            }
        }

        public class FieldData
        {
            public class FieldOptions
            {
                public FieldOptions(Settings.IAccount a)
                {
                    this.Account = a;
                }

                public Settings.IAccount Account
                {
                    get;
                    private set;
                }

                public EncodingType Encoding
                {
                    get;
                    set;
                }
            }

            private Func<FieldOptions, string> getValue;
            private Action<Settings.IAccount, string> setValue;

            public FieldData(string name, string tag, AccountType type, FieldFlags flags, Func<FieldOptions, string> getValue, Action<Settings.IAccount, string> setValue)
            {
                this.Name = name;
                this.Tag = tag;
                this.Type = type;
                this.Flags = flags;
                this.getValue = getValue;
                this.setValue = setValue;
            }

            public FieldData(string tag)
            {
                this.Name = tag;
                this.Tag = tag;
            }

            public string Name
            {
                get;
                set;
            }

            public string Tag
            {
                get;
                set;
            }

            public AccountType Type
            {
                get;
                set;
            }

            public FieldFlags Flags
            {
                get;
                set;
            }

            public bool IsValid
            {
                get
                {
                    return getValue != null;
                }
            }

            public string GetValue(FieldOptions o)
            {
                return getValue(o);
            }

            public bool SetValue(Settings.IAccount account, string value)
            {
                if (setValue == null)
                    return false;

                try
                {
                    setValue(account, value);

                    return true;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return false;
            }

            public bool CanSetValue
            {
                get
                {
                    return setValue != null;
                }
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        public class FieldValue
        {
            public FieldValue(FieldData field, string value)
            {
                this.Field = field;
                this.Value = value;
            }

            public FieldData Field
            {
                get;
                private set;
            }

            public string Value
            {
                get;
                private set;
            }
        }

        private readonly FieldData[] fields;
        private readonly Dictionary<string, FieldData> fieldsByTag;

        public AccountExporter()
        {
            fields = new FieldData[]
            {
                new FieldData("ID", Tags.UID, AccountType.Any, FieldFlags.Default, 
                    delegate(FieldData.FieldOptions o)
                    {
                        return o.Account.UID.ToString();
                    },
                    null),
                new FieldData("Account type", Tags.TYPE, AccountType.Any, FieldFlags.Default, 
                    delegate(FieldData.FieldOptions o)
                    {
                        return o.Account.Type.ToString();
                    },
                    null),
                new FieldData("Name", Tags.NAME, AccountType.Any, FieldFlags.Default, 
                    delegate(FieldData.FieldOptions o)
                    {
                        return o.Account.Name;
                    },
                    delegate(Settings.IAccount a, string v)
                    {
                        a.Name = v;
                    }),
                new FieldData("Email", Tags.EMAIL, AccountType.Any, FieldFlags.Default, 
                    delegate(FieldData.FieldOptions o)
                    {
                        return o.Account.Email;
                    },
                    delegate(Settings.IAccount a, string v)
                    {
                        a.Email = v;
                    }),
                new FieldData("Password", Tags.PASSWORD, AccountType.Any, FieldFlags.Authenticated, 
                    delegate(FieldData.FieldOptions o)
                    {
                        if (o.Account.Password == null)
                            return "";

                        switch (o.Encoding)
                        {
                            case EncodingType.Encoded:

                                using (var crypto = new Security.Cryptography.Crypto(Settings.EncryptionScope.Portable, Security.Cryptography.Crypto.CryptoCompressionFlags.All, Security.Cryptography.Crypto.GenerateCryptoKey()))
                                {
                                    return Encode(o.Account.Password.Data.ToArray(crypto, false), false);
                                }

                            case EncodingType.Text:

                                return new String(Security.Credentials.ToCharArray(o.Account.Password.ToSecureString()));

                            case EncodingType.None:
                            default:

                                return "";
                        }
                    },
                    delegate(Settings.IAccount a, string v)
                    {
                        System.Security.SecureString ss = null;
                        var data = Decode(v, false);

                        if (data != null)
                        {
                            using (var crypto = new Security.Cryptography.Crypto())
                            {
                                try
                                {
                                    data = crypto.Decrypt(data);
                                    ss = Security.Credentials.FromByteArray(data);
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);
                                }
                                finally
                                {
                                    Array.Clear(data, 0, data.Length);
                                }
                            }
                        }

                        if (ss == null && !string.IsNullOrEmpty(v))
                        {
                            ss = new System.Security.SecureString();
                            foreach (var c in v)
                            {
                                ss.AppendChar(c);
                            }
                            ss.MakeReadOnly();
                        }

                        if (ss != null)
                            a.Password = Settings.PasswordString.Create(ss);
                    }),
                new FieldData("Authenticator", Tags.TOTP_KEY, AccountType.Any, FieldFlags.Default, 
                    delegate(FieldData.FieldOptions o)
                    {
                        if (o.Account.TotpKey == null)
                            return "";
                        return Tools.Totp.Encode(o.Account.TotpKey);
                    },
                    delegate(Settings.IAccount a, string v)
                    {
                        a.TotpKey = Tools.Totp.Decode(v);
                    }),
                new FieldData("Created", Tags.CREATED, AccountType.Any, FieldFlags.None, 
                    delegate(FieldData.FieldOptions o)
                    {
                        return o.Account.CreatedUtc.ToString("s");
                    },
                    null),
                new FieldData("Last used", Tags.LAST_USED, AccountType.Any, FieldFlags.None, 
                    delegate(FieldData.FieldOptions o)
                    {
                        if (o.Account.LastUsedUtc.Ticks <= 1)
                            return "";
                        return o.Account.LastUsedUtc.ToString("s");
                    },
                    null),
                new FieldData("API key", Tags.API_KEY, AccountType.Gw2, FieldFlags.Default, 
                    delegate(FieldData.FieldOptions o)
                    {
                        var gw2 = o.Account as Settings.IGw2Account;
                        if (gw2 == null || gw2.Api == null)
                            return "";
                        return gw2.Api.Key;
                    },
                    delegate(Settings.IAccount a, string v)
                    {
                        var gw2 = a as Settings.IGw2Account;
                        if (gw2 == null)
                            return;
                        gw2.Api = Settings.ApiDataKey.Create(v);
                    }),
                new FieldData("Color", Tags.COLOR, AccountType.Any, FieldFlags.None, 
                    delegate(FieldData.FieldOptions o)
                    {
                        if (o.Account.ColorKeyIsDefault)
                            return "";
                        return ColorTranslator.ToHtml(o.Account.ColorKey);
                    },
                    delegate(Settings.IAccount a, string v)
                    {
                        if (v.Length > 0)
                        {
                            if (v[0] != '#' && v.IndexOf(',') != -1)
                            {
                                var parts = v.Split(',');
                                if (parts.Length == 3 || parts.Length == 4)
                                {
                                    //[r,g,b] or a,[r,g,b]
                                    var pi = parts.Length == 4 ? 1 : 0;
                                    a.ColorKey = Color.FromArgb(byte.Parse(parts[pi++].Trim()), byte.Parse(parts[pi++].Trim()), byte.Parse(parts[pi++].Trim()));
                                    return;
                                }
                            }

                            var c = ColorTranslator.FromHtml(v);
                            if (c.A != 255)
                                c = Color.FromArgb(255, c);
                            a.ColorKey = c;
                        }
                    }),
                new FieldData("Local.dat path", Tags.DAT_PATH, AccountType.Gw2, FieldFlags.None, 
                    delegate(FieldData.FieldOptions o)
                    {
                        var gw2 = o.Account as Settings.IGw2Account;
                        if (gw2 == null || gw2.DatFile == null)
                            return "";
                        return gw2.DatFile.Path;
                    },
                    null),
                new FieldData("GFXSettings.xml path", Tags.GFX_PATH, AccountType.Gw2, FieldFlags.None, 
                    delegate(FieldData.FieldOptions o)
                    {
                        var gw2 = o.Account as Settings.IGw2Account;
                        if (gw2 == null || gw2.GfxFile == null)
                            return "";
                        return gw2.GfxFile.Path;
                    },
                    null),
                new FieldData("Arguments", Tags.ARGS, AccountType.Any, FieldFlags.None, 
                    delegate(FieldData.FieldOptions o)
                    {
                        return o.Account.Arguments;
                    },
                    delegate(Settings.IAccount a, string v)
                    {
                        a.Arguments = v;
                    }),
            };

            fieldsByTag = new Dictionary<string, FieldData>(fields.Length);
            foreach (var f in fields)
            {
                fieldsByTag[f.Tag] = f;
            }
        }

        public FieldData[] Fields
        {
            get
            {
                return fields;
            }
        }

        public EncodingType CanWriteAuthorization
        {
            get;
            set;
        }

        public FieldData GetField(string tag)
        {
            FieldData f;
            fieldsByTag.TryGetValue(tag, out f);
            return f;
        }

        public ImportData Import(string path)
        {
            using (var stream = new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                FileType type;

                switch (Path.GetExtension(path).ToLower())
                {
                    case ".xml":
                        type = FileType.Xml;
                        break;
                    case ".csv":
                        type = FileType.Csv;
                        break;
                    default:
                        type = FileType.Auto;
                        break;
                }

                return Import(stream, type);
            }
        }

        public ImportData Import(Stream stream, FileType type)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true))
            {
                if (type == FileType.Auto)
                {
                    if (reader.Peek() == '<')
                        type = FileType.Xml;
                    else
                        type = FileType.Csv;
                }

                Dictionary<string, FieldData> fields;
                int unknown;
                List<AccountData> accounts;

                if (type == FileType.Xml)
                {
                    accounts = ReadXml(reader, out fields, out unknown);
                }
                else
                {
                    accounts = ReadCsv(reader, out fields, out unknown);
                }

                return new ImportData(accounts, fields.Values.ToArray(), unknown);
            }
        }

        public void Export(string path, FileType type, IList<FieldData> fields, IEnumerable<Settings.IAccount> accounts)
        {
            using (var stream = new BufferedStream(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                Export(stream, type, fields, accounts);
            }
        }

        public void Export(Stream stream, FileType type, IList<FieldData> fields, IEnumerable<Settings.IAccount> accounts)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                if (type == AccountExporter.FileType.Csv)
                {
                    WriteCsv(writer, fields, accounts);
                }
                else
                {
                    WriteXml(writer, fields, accounts);
                }
            }
        }

        private List<AccountData> ReadXml(StreamReader stream, out Dictionary<string, FieldData> fields, out int unknownCount)
        {
            var settings = new XmlReaderSettings()
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
            };

            var accounts = new List<AccountData>();
            var fcount = 0;

            fields = new Dictionary<string, FieldData>();
            unknownCount = 0;

            using (var xml = XmlReader.Create(stream, settings))
            {
                xml.ReadStartElement();

                if (xml.Name != "account")
                    throw new IOException("Invalid XML format");

                while (xml.NodeType == XmlNodeType.Element)
                {
                    xml.ReadStartElement();

                    var values = new List<FieldValue>(fcount > 0 ? fcount : 10);

                    while (xml.NodeType == XmlNodeType.Element)
                    {
                        var key = xml.Name;
                        var value = xml.ReadElementContentAsString();

                        FieldData field;
                        if (!fields.TryGetValue(key, out field))
                        {
                            if (!fieldsByTag.TryGetValue(key, out field))
                            {
                                field = new FieldData(key);
                                ++unknownCount;
                            }
                            fields[key] = field;
                        }

                        values.Add(new FieldValue(field, value));
                    }

                    accounts.Add(new AccountData(values));
                    if (values.Count > fcount)
                        fcount = values.Count;

                    xml.ReadEndElement();
                }
            }

            return accounts;
        }

        private List<AccountData> ReadCsv(StreamReader stream, out Dictionary<string, FieldData> fields, out int unknownCount)
        {
            var first = true;
            FieldData[] headers = null;
            string line;
            var headerLength = 10;
            var accounts = new List<AccountData>();

            fields = new Dictionary<string, FieldData>();
            unknownCount = 0;

            while ((line = stream.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var l = SplitCsv(line, headerLength);

                #region Detect header

                if (first)
                {
                    first = false;

                    //detect header

                    headerLength = l.Length;
                    headers = new FieldData[headerLength];

                    if (line[0] == '#')
                    {
                        for (var i = 0; i < headerLength; i++)
                        {
                            var key = l[i];
                            if (i == 0)
                                key = key.Substring(1);

                            FieldData field;
                            if (!fields.TryGetValue(key, out field))
                            {
                                if (!fieldsByTag.TryGetValue(key, out field))
                                {
                                    if (key.Length == 0)
                                        key = "column" + (i + 1);
                                    field = new FieldData(key);
                                    ++unknownCount;
                                }
                                fields[key] = field;
                            }

                            headers[i] = field;
                        }

                        //valid header
                        continue;
                    }
                    else
                    {
                        var hasHeader = 0;

                        for (var i = 0; i < headerLength; i++)
                        {
                            var key = l[i];

                            FieldData field;
                            if (fieldsByTag.TryGetValue(key, out field))
                            {
                                hasHeader++;
                                headers[i] = field;
                                fields[key] = field;
                            }
                        }

                        if (hasHeader != headerLength)
                        {
                            for (var i = 0; i < headerLength; i++)
                            {
                                if (headers[i] == null)
                                {
                                    var key = "column" + (i + 1);
                                    var field = new FieldData(key);
                                    headers[i] = field;
                                    fields[key] = field;
                                    ++unknownCount;
                                }
                            }
                        }

                        if (hasHeader > headerLength / 2)
                        {
                            //valid header
                            continue;
                        }
                        else
                        {
                            //invalid header (probably), treating header as an account
                        }
                    }
                }

                #endregion

                if (l.Length > headerLength)
                    throw new IOException("Invalid CSV format");

                var count = headerLength < l.Length ? headerLength : l.Length;
                var values = new List<FieldValue>(count);

                for (var j = 0; j < count; j++)
                {
                    values.Add(new FieldValue(headers[j], l[j]));
                }

                accounts.Add(new AccountData(values));
            }

            return accounts;
        }

        /// <summary>
        /// Splits a line into parts
        /// </summary>
        /// <param name="line">CSV format: [0,"1","""2""", 3] = [0,1,"2",3]</param>
        /// <param name="count">Optional (estimated) field count</param>
        private string[] SplitCsv(string line, int count = 10)
        {
            var parts = new string[count];
            var i = 0;
            var pi = 0;
            var l = line.Length;
            var last = true;

            while (i < l)
            {
                var j = i;

                if (char.IsWhiteSpace(line[i]))
                {
                    while (++i < l && char.IsWhiteSpace(line[i]))
                    {
                    }
                    if (i == l)
                        break;
                }

                bool q;

                if (q = line[i] == '"')
                {
                    ++j;

                    for (; j < l; ++j)
                    {
                        if (line[j] == '"')
                        {
                            if (j + 1 == l || line[j + 1] != '"')
                                break;
                            else
                                ++j;
                        }
                    }

                    if (j == l || line[j] != '"')
                    {
                        j = i;
                        q = false;
                    }
                }

                j = line.IndexOf(',', j);
                if (j == -1)
                {
                    last = false;
                    j = l;
                }

                if (pi >= count)
                {
                    count += 5;
                    Array.Resize<string>(ref parts, count);
                }

                if (q && line[j - 1] == line[i])
                    parts[pi++] = line.Substring(i + 1, j - i - 2).Replace("\"\"", "\"");
                else
                    parts[pi++] = line.Substring(i, j - i);

                i = j + 1;
            }

            if (last)
            {
                if (pi >= count)
                {
                    Array.Resize<string>(ref parts, pi + 1);
                }
                parts[pi++] = "";
            }

            if (pi != parts.Length)
            {
                Array.Resize<string>(ref parts, pi);
            }

            return parts;
        }

        private string GetValue(FieldData f, Settings.IAccount a, FileType format)
        {
            try
            {
                var isAuthenticated = (f.Flags & FieldFlags.Authenticated) != 0;

                if (!isAuthenticated || CanWriteAuthorization != EncodingType.None)
                {
                    var o = new FieldData.FieldOptions(a);
                    if (isAuthenticated)
                        o.Encoding = CanWriteAuthorization;
                    var v = f.GetValue(o);

                    if (v == null)
                    {
                        v = "";
                    }

                    if (format == FileType.Csv)
                    {
                        if (v.Length > 0 && (v.IndexOf(',') != -1 || v[0] == '"'))
                        {
                            v = '"' + v.Replace("\"", "\"\"") + '"';
                        }
                    }

                    return v;
                }
            }
            catch { }

            return "";
        }
        
        private string Encode(byte[] data, bool includeSum = false)
        {
            if (includeSum)
            {
                ushort sum = 0;

                for (var i = 0; i < data.Length; i++)
                {
                    sum += data[i];
                }

                var buffer = new byte[data.Length + 2];
                var c = BitConverter.GetBytes(sum);

                Array.Copy(data, buffer, data.Length);
                Array.Copy(c, 0, buffer, data.Length, c.Length);

                data = buffer;
            }

            return ":" + System.Convert.ToBase64String(data).TrimEnd('=');
        }

        private byte[] Decode(string b64, bool includesSum = false)
        {
            try
            {
                if (!b64.StartsWith(":"))
                    return null;

                var l = b64.Length - 1;
                var buffer = System.Convert.FromBase64String(b64.Substring(1).PadRight(4 - l % 4, '='));

                if (includesSum)
                {
                    l = buffer.Length - 2;
                    var sum = BitConverter.ToInt16(buffer, l);

                    for (var i = 0; i < l; i++)
                    {
                        sum -= buffer[i];
                    }

                    if (sum != 0)
                        return null;

                    Array.Resize<byte>(ref buffer, l);
                }

                return buffer;
            }
            catch
            {
                return null;
            }
        }

        private void WriteCsv(StreamWriter stream, IList<FieldData> fields, IEnumerable<Settings.IAccount> accounts)
        {
            var sb = new StringBuilder(fields.Count * 20);
            var count = fields.Count;
            var isEmpty = true;

            sb.Append('#');

            foreach (var fi in fields)
            {
                sb.Append(fi.Tag);
                sb.Append(',');
            }

            sb.Length -= 1;
            stream.WriteLine(sb.ToString());
            sb.Length = 0;

            foreach (var a in accounts)
            {
                for (var i = 0; i < count; i++)
                {
                    var v = GetValue(fields[i], a, FileType.Csv);
                    if (isEmpty)
                        isEmpty = v.Length == 0;
                    sb.Append(v);
                    sb.Append(',');
                }

                if (!isEmpty)
                {
                    sb.Length -= 1;
                    stream.WriteLine(sb.ToString());
                    sb.Length = 0;

                    isEmpty = true;
                }

                sb.Length = 0;
            }
        }

        private void WriteXml(StreamWriter stream, IList<FieldData> fields, IEnumerable<Settings.IAccount> accounts)
        {
            var settings = new System.Xml.XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
            };

            using (var xml = System.Xml.XmlWriter.Create(stream, settings))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("accounts");

                var values = new string[fields.Count];
                var isEmpty = true;

                foreach (var a in accounts)
                {
                    xml.WriteStartElement("account");

                    for (var i = 0; i < values.Length; i++ )
                    {
                        values[i] = GetValue(fields[i], a, FileType.Xml);
                        if (isEmpty)
                            isEmpty = values[i].Length == 0;
                    }

                    if (!isEmpty)
                    {
                        for (var i = 0; i < values.Length; i++)
                        {
                            xml.WriteStartElement(fields[i].Tag);
                            xml.WriteValue(values[i]);
                            xml.WriteEndElement();
                        }

                        isEmpty = true;
                    }

                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }
    }
}
