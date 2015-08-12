using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace Gw2Launcher.Tools
{
    static class Gw2Build
    {
        private const int BUILD_RECACHE_TIME = 60000 * 10;
        private const string URL_BUILD_API = "https://api.guildwars2.com/v2/build";

        private static readonly object _lock;
        private static int build;
        private static int time;

        static Gw2Build()
        {
            _lock = new object();
            time = -BUILD_RECACHE_TIME;
        }

        public static Task<int> GetBuildAsync()
        {
            return Task.Factory.StartNew<int>(new Func<int>(GetBuild));
        }

        public static int GetBuild()
        {
            lock (_lock)
            {
                if (Environment.TickCount < time + BUILD_RECACHE_TIME)
                    return build;

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string data = client.DownloadString(URL_BUILD_API);
                        int i = data.IndexOf("id\":");
                        if (i != -1)
                        {
                            i += 4;

                            int b;
                            for (int j = i, l = data.Length; j < l; j++)
                            {
                                if (!char.IsDigit(data[j]))
                                {
                                    if (Int32.TryParse(data.Substring(i, j - i), out b))
                                    {
                                        build = b;
                                        time = Environment.TickCount;
                                        return b;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return -1;
        }

        public static int Build
        {
            get
            {
                if (Environment.TickCount > time + BUILD_RECACHE_TIME)
                {
                    if (Monitor.TryEnter(_lock, 5000))
                    {
                        try
                        {
                            GetBuild();
                        }
                        finally
                        {
                            Monitor.Exit(_lock);
                        }
                    }
                }

                return build;
            }
        }
    }
}
