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
        private const string URL_LATEST = "http://" + Settings.ASSET_HOST + "/latest64/101";

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

        private static int GetBuildFromApi()
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(URL_BUILD_API);
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
                if (e is WebException)
                    using (((WebException)e).Response) { }
                Util.Logging.Log(e);
            }

            return -1;
        }

        private static int GetBuildFromLatest()
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(URL_LATEST);
                request.Headers.Add(HttpRequestHeader.Cookie, Settings.ASSET_COOKIE);
                request.Timeout = 5000;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        string data = reader.ReadToEnd();

                        int i = data.IndexOf(' ');
                        if (i != -1)
                        {
                            int b;
                            if (Int32.TryParse(data.Substring(0, i), out b))
                            {
                                return b;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e is WebException)
                    using (((WebException)e).Response) { }
                Util.Logging.Log(e);
            }

            return -1;
        }

        public static int GetBuild()
        {
            lock (_lock)
            {
                if (DateTime.UtcNow < nextCheck)
                    return build;

                try
                {
                    var b = GetBuildFromLatest();
                    if (b == -1)
                        b = GetBuildFromApi();

                    if (b != -1)
                    {
                        build = b;
                        nextCheck = DateTime.UtcNow.AddMilliseconds(BUILD_RECACHE_TIME);
                        return b;
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
