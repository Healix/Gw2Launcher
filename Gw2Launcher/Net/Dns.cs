using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Net.NetworkInformation;

namespace Gw2Launcher.Net
{
    static class Dns
    {
        //DNS is big endian

        //https://www.iana.org/assignments/dns-parameters/dns-parameters.xhtml

        private enum DnsQueryType
        {
            Request = 0,
            Response = 1
        }

        private enum DnsOperationCode
        {
            Query = 0,
            [Obsolete]
            InverseQuery = 1,
            Status = 2,
            Notify = 4,
            Update = 5,
        }

        private enum DnsResponseCode
        {
            NoError = 0,
            FormatError = 1,
            ServerFailure = 2,
            NXDomain = 3,
            NotImplemented = 4,
            Refused = 5,
            YXDomain = 6,
            YXRRSet = 7,
            NXRRSet = 8,
            NotAuth = 9,
            NotZone = 10,
            BadVersionOrSignature = 16,
            BadKey = 17,
            BadTime = 18,
            BadMode = 19,
            BadName = 20,
            BadAlgorithm = 21,
            BadTruncation = 22,
            BadCookie = 23,
        }

        private enum DnsRecordType
        {
            A = 1,
            NS = 2,
            CNAME = 5,
            SOA = 6,
            WKS = 11,
            PTR = 12,
            MX = 15,
            TXT = 16,
            AAAA = 28,
            SRV = 33,
            ANY = 255,
        }

        private enum DnsRecordClass
        {
            Internet = 1,
            None = 254,
            Any = 255,
        }

        private class Header
        {
            public const byte LENGTH = 12;

            private ushort id;
            private byte flag0;
            private byte flag1;
            private ushort questions;
            private ushort answers;
            private ushort authorityRecords;
            private ushort additionalRecords;

            public ushort Identification
            {
                get
                {
                    return id;
                }
                set
                {
                    id = value;
                }
            }

            public DnsQueryType QueryType
            {
                get
                {
                    return (DnsQueryType)Util.Bits.GetBits(flag0, 7, 1);
                }
                set
                {
                    flag0 = Util.Bits.SetBits(flag0, (byte)value, 7, 1);
                }
            }

            public DnsOperationCode OperationCode
            {
                get
                {
                    return (DnsOperationCode)Util.Bits.GetBits(flag0, 3, 4);
                }
                set
                {
                    flag0 = Util.Bits.SetBits(flag0, (byte)value, 3, 4);
                }
            }

            public bool AuthorativeAnswer
            {
                get
                {
                    return Util.Bits.GetBits(flag0, 2, 1) == 1;
                }
                set
                {
                    flag0 = Util.Bits.SetBits(flag0, (byte)(value ? 1 : 0), 2, 1);
                }
            }

            public bool Truncated
            {
                get
                {
                    return Util.Bits.GetBits(flag0, 1, 1) == 1;
                }
                set
                {
                    flag0 = Util.Bits.SetBits(flag0, (byte)(value ? 1 : 0), 1, 1);
                }
            }

            public bool RecursionDesired
            {
                get
                {
                    return Util.Bits.GetBits(flag0, 0, 1) == 1;
                }
                set
                {
                    flag0 = Util.Bits.SetBits(flag0, (byte)(value ? 1 : 0), 0, 1);
                }
            }

            public bool RecursionAvailable
            {
                get
                {
                    return Util.Bits.GetBits(flag1, 7, 1) == 1;
                }
                set
                {
                    flag1 = Util.Bits.SetBits(flag1, (byte)(value ? 1 : 0), 7, 1);
                }
            }

            private byte Z
            {
                get
                {
                    return Util.Bits.GetBits(flag1, 6, 1);
                }
                set
                {
                    flag1 = Util.Bits.SetBits(flag1, value, 6, 1);
                }
            }

            private byte AD
            {
                get
                {
                    return Util.Bits.GetBits(flag1, 5, 1);
                }
                set
                {
                    flag1 = Util.Bits.SetBits(flag1, value, 5, 1);
                }
            }

            private byte CD
            {
                get
                {
                    return Util.Bits.GetBits(flag1, 4, 1);
                }
                set
                {
                    flag1 = Util.Bits.SetBits(flag1, value, 4, 1);
                }
            }

            public DnsResponseCode ResponseCode
            {
                get
                {
                    return (DnsResponseCode)Util.Bits.GetBits(flag1, 0, 4);
                }
                set
                {
                    flag1 = Util.Bits.SetBits(flag1, (byte)value, 0, 4);
                }
            }

            public ushort TotalQuestions
            {
                get
                {
                    return questions;
                }
                set
                {
                    questions = value;
                }
            }

            public ushort TotalAnswers
            {
                get
                {
                    return answers;
                }
                set
                {
                    answers = value;
                }
            }

            public ushort TotalAuthorityResourceRecords
            {
                get
                {
                    return authorityRecords;
                }
                set
                {
                    authorityRecords = value;
                }
            }

            public ushort TotalAdditionalResourceRecords
            {
                get
                {
                    return additionalRecords;
                }
                set
                {
                    additionalRecords = value;
                }
            }

            public void ToArray(byte[] array, int offset, out int offsetOut)
            {
                if (BitConverter.IsLittleEndian)
                {
                    array[offset++] = (byte)(id >> 8 & 255);
                    array[offset++] = (byte)(id & 255);
                    array[offset++] = flag0;
                    array[offset++] = flag1;
                    array[offset++] = (byte)(questions >> 8 & 255);
                    array[offset++] = (byte)(questions & 255);
                    array[offset++] = (byte)(answers >> 8 & 255);
                    array[offset++] = (byte)(answers & 255);
                    array[offset++] = (byte)(authorityRecords >> 8 & 255);
                    array[offset++] = (byte)(authorityRecords & 255);
                    array[offset++] = (byte)(additionalRecords >> 8 & 255);
                    array[offset++] = (byte)(additionalRecords & 255);
                }
                else
                {
                    array[offset++] = (byte)(id & 255);
                    array[offset++] = (byte)(id >> 8 & 255);
                    array[offset++] = flag0;
                    array[offset++] = flag1;
                    array[offset++] = (byte)(questions & 255);
                    array[offset++] = (byte)(questions >> 8 & 255);
                    array[offset++] = (byte)(answers & 255);
                    array[offset++] = (byte)(answers >> 8 & 255);
                    array[offset++] = (byte)(authorityRecords & 255);
                    array[offset++] = (byte)(authorityRecords >> 8 & 255);
                    array[offset++] = (byte)(additionalRecords & 255);
                    array[offset++] = (byte)(additionalRecords >> 8 & 255);
                }

                offsetOut = offset;
            }

            public static Header FromArray(byte[] array, int offset, out int offsetOut)
            {
                if (array.Length < LENGTH)
                    throw new IOException("Header must be at least 12 bytes");

                Header h = new Header();

                if (BitConverter.IsLittleEndian)
                {
                    h.id = (ushort)(array[offset++] << 8 | array[offset++]);
                    h.flag0 = array[offset++];
                    h.flag1 = array[offset++];
                    h.questions = (ushort)(array[offset++] << 8 | array[offset++]);
                    h.answers = (ushort)(array[offset++] << 8 | array[offset++]);
                    h.authorityRecords = (ushort)(array[offset++] << 8 | array[offset++]);
                    h.additionalRecords = (ushort)(array[offset++] << 8 | array[offset++]);
                }
                else
                {
                    h.id = (ushort)(array[offset++] | array[offset++] << 8);
                    h.flag0 = array[offset++];
                    h.flag1 = array[offset++];
                    h.questions = (ushort)(array[offset++] | array[offset++] << 8);
                    h.answers = (ushort)(array[offset++] | array[offset++] << 8);
                    h.authorityRecords = (ushort)(array[offset++] | array[offset++] << 8);
                    h.additionalRecords = (ushort)(array[offset++] | array[offset++] << 8);
                }

                offsetOut = offset;
                return h;
            }
        }

        private class Question
        {
            private string hostname;
            private DnsRecordType recordType;
            private DnsRecordClass recordClass;

            public Question(string hostname, DnsRecordType recordType, DnsRecordClass recordClass)
            {
                this.hostname = hostname;
                this.recordType = recordType;
                this.recordClass = recordClass;
            }

            public int Length
            {
                get
                {
                    //[(byte)string length][(string)string][(byte)0][(ushort)type][(ushort)class]
                    return hostname.Length + 2 + 4;
                }
            }

            public void ToArray(byte[] array, int offset, out int offsetOut)
            {
                Hostname.ToArray(this.hostname, array, offset, out offset);

                array[offset++] = 0;

                if (BitConverter.IsLittleEndian)
                {
                    array[offset++] = (byte)((ushort)recordType >> 8 & 255);
                    array[offset++] = (byte)((ushort)recordType & 255);
                    array[offset++] = (byte)((ushort)recordClass >> 8 & 255);
                    array[offset++] = (byte)((ushort)recordClass & 255);
                }
                else
                {
                    array[offset++] = (byte)((ushort)recordType & 255);
                    array[offset++] = (byte)((ushort)recordType >> 8 & 255);
                    array[offset++] = (byte)((ushort)recordClass & 255);
                    array[offset++] = (byte)((ushort)recordClass >> 8 & 255);
                }

                offsetOut = offset;
            }

            public static List<Question> FromArray(byte[] array, int count, int offset, out int offsetOut)
            {
                List<Question> questions = new List<Question>(count);

                for (int i = 0; i < count; i++)
                {
                    string hostname = Hostname.FromArray(array, offset, out offset);

                    if (BitConverter.IsLittleEndian)
                    {
                        ushort recordType = (ushort)(array[offset++] << 8 | array[offset++]);
                        ushort recordClass = (ushort)(array[offset++] << 8 | array[offset++]);

                        questions.Add(new Question(hostname, (DnsRecordType)recordType, (DnsRecordClass)recordClass));
                    }
                    else
                    {
                        ushort recordType = (ushort)(array[offset++] | array[offset++] << 8);
                        ushort recordClass = (ushort)(array[offset++] | array[offset++] << 8);

                        questions.Add(new Question(hostname, (DnsRecordType)recordType, (DnsRecordClass)recordClass));
                    }

                }

                offsetOut = offset;
                return questions;
            }
        }

        private class Record
        {
            protected string hostname;
            protected DnsRecordType recordType;
            protected DnsRecordClass recordClass;
            protected uint ttl;
            protected byte[] data;

            public static List<Record> FromArray(byte[] array, int count, int offset, out int offsetOut)
            {
                List<Record> records = new List<Record>(count);

                for (int i = 0; i < count; i++)
                {
                    string hostname = Hostname.FromArray(array, offset, out offset);
                    DnsRecordType recordType;
                    Record record;
                    ushort dataLength;

                    if (BitConverter.IsLittleEndian)
                        recordType = (DnsRecordType)(array[offset++] << 8 | array[offset++]);
                    else
                        recordType = (DnsRecordType)(array[offset++] | array[offset++] << 8);

                    switch (recordType)
                    {
                        case DnsRecordType.A:
                        case DnsRecordType.AAAA:
                            record = new IPRecord();
                            break;
                        case  DnsRecordType.CNAME:
                            record = new CNameRecord();
                            break;
                        default:
                            record = new Record();
                            break;
                    }

                    record.hostname = hostname;
                    record.recordType = recordType;

                    if (BitConverter.IsLittleEndian)
                    {
                        record.recordClass = (DnsRecordClass)(array[offset++] << 8 | array[offset++]);
                        record.ttl = (uint)(array[offset++] << 24 | array[offset++] << 16 | array[offset++] << 8 | array[offset++]);
                        dataLength = (ushort)(array[offset++] << 8 | array[offset++]);
                    }
                    else
                    {
                        record.recordClass = (DnsRecordClass)(array[offset++] | array[offset++] << 8);
                        record.ttl = (uint)(array[offset++] | array[offset++] << 8 | array[offset++] << 16 | array[offset++] << 24);
                        dataLength = (ushort)(array[offset++] | array[offset++] << 8);
                    }

                    record.data = new byte[dataLength];
                    Array.Copy(array, offset, record.data, 0, record.data.Length);

                    records.Add(record);

                    offset += dataLength;
                }

                offsetOut = offset;
                return records;
            }
        }

        private class IPRecord : Record
        {
            public IPAddress IP
            {
                get
                {
                    return new IPAddress(base.data);
                }
            }
        }

        private class CNameRecord : Record
        {
            public string CanonicalName
            {
                get
                {
                    int o;
                    return Hostname.FromArray(base.data, 0, out o);
                }
            }
        }

        private class Hostname
        {
            public static void ToArray(string hostname, byte[] array, int offset, out int offsetOut)
            {
                int length = hostname.Length;
                int index = offset++;

                for (var i = 0; i < length; i++)
                {
                    char c = hostname[i];
                    if (c == '.')
                    {
                        array[index] = (byte)(offset - index - 1);
                        index = offset++;
                    }
                    else
                        array[offset++] = (byte)c;
                }

                array[index] = (byte)(offset - index - 1);

                offsetOut = offset;
            }

            public static string FromArray(byte[] array, int offset, out int offsetOut)
            {
                offsetOut = 0;

                StringBuilder hostname = new StringBuilder(50);
                byte length;

                while ((length = array[offset++]) > 0)
                {
                    //first two bits define the type, where 00 = length, 11 = pointer
                    var b = Util.Bits.GetBits(length, 6, 2);
                    if (b != 0)
                    {
                        if (b == 3)
                        {
                            offsetOut = offset + 1;
                            
                            ushort pointer = Util.Bits.GetBits(length, 0, 6);
                            offset = (pointer << 8) | array[offset];
                            
                            continue;
                        }

                        throw new NotSupportedException("Unknown length format");
                    }

                    if (hostname.Length > 0)
                        hostname.Append('.');
                    hostname.Append(Encoding.ASCII.GetString(array, offset, length));

                    offset += length;
                }

                if (offsetOut == 0)
                    offsetOut = offset;

                return hostname.ToString();
            }
        }

        private static Random random;

        static Dns()
        {
            random = new Random();
        }

        public static IPAddress[] GetHostAddresses(string host)
        {
            var ips = System.Net.Dns.GetHostAddresses(host);

            if (ips.Length == 1 && IPAddress.IsLoopback(ips[0])) //probably being overriden
            {
                var h = GetHostAddresses(host, GetDefaultDnsAddresses(true));
                if (h.Count > 0)
                    ips = h.ToArray<IPAddress>();
            }

            return ips;
        }

        public static IPAddress[] GetHostAddresses(string host, string serverIp)
        {
            return GetHostAddresses(host, IPAddress.Parse(serverIp));
        }

        public static IPAddress[] GetHostAddresses(string host, IPAddress serverIp)
        {
            return GetHostAddresses(host, new IPEndPoint(serverIp, 53));
        }

        public static IPAddress[] GetHostAddresses(string host, IPEndPoint server)
        {
            Header header = new Header();
            header.Identification = (ushort)random.Next(ushort.MaxValue);
            header.OperationCode = DnsOperationCode.Query;
            header.QueryType = DnsQueryType.Request;
            header.RecursionDesired = true;
            header.TotalQuestions = 1;

            var question = new Question(host, DnsRecordType.A, DnsRecordClass.Internet);

            byte[] buffer = new byte[Header.LENGTH + question.Length];
            int offset = 0;

            header.ToArray(buffer, offset, out offset);
            question.ToArray(buffer, offset, out offset);

            using(UdpClient udp = new UdpClient())
            {
                udp.Client.ReceiveTimeout = 3000;

                udp.Send(buffer, buffer.Length, server);
                
                IPEndPoint ep = null;
                buffer = udp.Receive(ref ep);

                if (!server.Equals(ep))
                    throw new IOException("Unexpected DNS response endpoint");
            }

            header = Header.FromArray(buffer, offset = 0, out offset);
            var questions = Question.FromArray(buffer, header.TotalQuestions, offset, out offset);
            var answers = Record.FromArray(buffer, header.TotalAnswers, offset, out offset);
            //var records = Record.FromArray(buffer, header.TotalAuthorityResourceRecords, offset, out offset);
            //var arecords = Record.FromArray(buffer, header.TotalAdditionalResourceRecords, offset, out offset);

            List<IPAddress> ips = new List<IPAddress>(answers.Count);

            foreach (var record in answers)
            {
                if (record is IPRecord)
                {
                    ips.Add(((IPRecord)record).IP);
                }
            }

            return ips.ToArray();
        }

        public static HashSet<IPAddress> GetHostAddresses(string host, IEnumerable<IPAddress> servers)
        {
            HashSet<IPAddress> ips = new HashSet<IPAddress>();
            ParallelOptions po = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 3
            };

            Parallel.ForEach<IPAddress>(servers, po,
                delegate(IPAddress ip)
                {
                    try
                    {
                        IPAddress[] _ips;
                        if (IPAddress.IsLoopback(ip))
                            _ips = GetHostAddresses(host);
                        else
                            _ips = GetHostAddresses(host, ip);

                        lock (ips)
                        {
                            foreach (var _ip in _ips)
                                ips.Add(_ip);
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                });

            return ips;
        }

        /// <summary>
        /// Returns system DNS addresses
        /// </summary>
        /// <param name="fallback">If true and no servers were found, 8.8.8.8 and 1.1.1.1 will be used</param>
        public static IEnumerable<IPAddress> GetDefaultDnsAddresses(bool fallback = false)
        {
            var count = 0;

            foreach (var i in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (i.NetworkInterfaceType != NetworkInterfaceType.Loopback && i.OperationalStatus == OperationalStatus.Up && !i.IsReceiveOnly)
                {
                    foreach (var ip in i.GetIPProperties().DnsAddresses)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (!IPAddress.IsLoopback(ip))
                            {
                                ++count;
                                yield return ip;
                            }
                        }
                    }
                }
            }

            if (fallback && count == 0)
            {
                yield return new IPAddress(new byte[] { 8, 8, 8, 8 });
                yield return new IPAddress(new byte[] { 1, 1, 1, 1 });
            }
        }
    }
}
