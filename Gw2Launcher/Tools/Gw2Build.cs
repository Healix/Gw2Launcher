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
        private const int BUILD_RECACHE_TIME = 60000;
        private const string URL_BUILD_API = "https://api.guildwars2.com/v2/build";

        private static readonly object _lock;
        private static int build;
        private static DateTime nextCheck;

        static Gw2Build()
        {
            _lock = new object();
        }

        public static Task<int> GetBuildAsync()
        {
            return Task.Factory.StartNew<int>(new Func<int>(GetBuild));
        }

        public static int GetBuild()
        {
            lock (_lock)
            {
                if (DateTime.UtcNow < nextCheck)
                    return build;

                try
                {
                    HttpWebRequest request = HttpWebRequest.CreateHttp(URL_BUILD_API);
                    request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    request.Timeout = 5000;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream()))
                        {
                            string data = reader.ReadToEnd();
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
                                            nextCheck = DateTime.UtcNow.AddMilliseconds(BUILD_RECACHE_TIME);
                                            return b;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            return -1;
        }

        public static int Build
        {
            get
            {
                if (DateTime.UtcNow > nextCheck)
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
