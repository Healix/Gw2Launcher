using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Windows
{
    class Hosts
    {
        public static string GetPath()
        {
            return Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
        }

        public static bool Exists()
        {
            return File.Exists(GetPath());
        }

        public static void Add(string hostname, string address)
        {
            Modify(hostname, address, false);
        }

        public static void Remove(string hostname, string address = null)
        {
            Modify(hostname, address, true);
        }

        public static bool Contains(string hostname, string address = null)
        {
            using (var r = new StreamReader(File.Open(GetPath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8, false))
            {
                int hlen = hostname.Length;
                string line;

                while ((line = r.ReadLine()) != null)
                {
                    int i, j, iaddr, ihost;

                    i = 0;
                    iaddr = ParseSegment(line, i, out i);

                    if (iaddr != -1)
                    {
                        ihost = ParseSegment(line, i, out j);

                        if (ihost != -1 && j - ihost == hlen && line.Substring(ihost, j - ihost).Equals(hostname, StringComparison.OrdinalIgnoreCase))
                        {
                            if (address == null || line.Substring(iaddr, i - iaddr).Equals(address, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static void Modify(string hostname, string address, bool remove)
        {
            using (var f = File.Open(GetPath(), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var lines = new List<string>(50);
                var modified = !remove;

                using (var r = new StreamReader(f, Encoding.UTF8, false, 1024, true))
                {
                    int hlen = hostname.Length;
                    string line;

                    while ((line = r.ReadLine()) != null)
                    {
                        int i, j, iaddr, ihost;

                        i = 0;
                        iaddr = ParseSegment(line, i, out i);

                        if (iaddr != -1)
                        {
                            ihost = ParseSegment(line, i, out j);

                            if (ihost != -1 && j - ihost == hlen && line.Substring(ihost, j - ihost).Equals(hostname, StringComparison.OrdinalIgnoreCase))
                            {
                                if (remove)
                                {
                                    if (address == null || line.Substring(iaddr, i - iaddr).Equals(address, StringComparison.OrdinalIgnoreCase))
                                    {
                                        modified = true;
                                        continue;
                                    }
                                }
                                else if (line.Substring(iaddr, i - iaddr).Equals(address, StringComparison.OrdinalIgnoreCase))
                                {
                                    return; //address is already set
                                }
                                else
                                {
                                    modified = true;
                                    continue;
                                }
                            }
                        }

                        lines.Add(line);
                    }
                }

                if (!modified)
                    return;

                f.Position = 0;

                using (var w = new StreamWriter(f, Encoding.UTF8, 1024, true))
                {
                    foreach (var line in lines)
                    {
                        w.WriteLine(line);
                    }

                    if (!remove)
                    {
                        w.WriteLine(address + " " + hostname);
                    }
                }

                if (f.Length > f.Position)
                {
                    f.SetLength(f.Position);
                }
            }
        }

        private static int ParseSegment(string line, int startAt, out int endAt)
        {
            var length = line.Length;

            if (length == 0)
            {
                endAt = startAt;
                return -1;
            }

            do
            {
                switch (line[startAt])
                {
                    case '\t':
                    case ' ':

                        continue;

                    case '#':

                        endAt = startAt;
                        return -1;
                }

                break;
            }
            while (++startAt < length);

            if (startAt == length)
            {
                endAt = startAt;
                return -1;
            }

            var i = startAt;

            do
            {
                switch (line[startAt])
                {
                    case '\t':
                    case ' ':
                    case '#':

                        break;

                    default:

                        continue;
                }

                break;
            }
            while (++startAt < length);

            endAt = startAt;
            return i;
        }
    }
}
