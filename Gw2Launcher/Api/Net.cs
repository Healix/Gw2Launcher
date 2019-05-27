using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Gw2Launcher.Api
{
    static class Net
    {
        public const string URL = "https://api.guildwars2.com/";

        public class ResponseData<T>
        {
            private DateTime created, date, lastModified;
            private T data;

            public ResponseData(DateTime date, DateTime lastModified, T data)
            {
                this.created = DateTime.UtcNow;
                this.data = data;
                this.date = date;
                this.lastModified = lastModified;
            }

            public DateTime Date
            {
                get
                {
                    return date;
                }
            }

            public DateTime LastModified
            {
                get
                {
                    return lastModified;
                }
            }

            public DateTime LastModifiedAdjusted
            {
                get
                {
                    if (lastModified == DateTime.MinValue)
                        return DateTime.MinValue;
                    return created.Subtract(date.Subtract(lastModified));
                }
            }

            public T Data
            {
                get
                {
                    return data;
                }
            }
        }

        public static async Task<ResponseData<string>> DownloadStringAsync(string url)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.Timeout = 10000;
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = await request.GetResponseAsync())
            {
                using (var r = new StreamReader(response.GetResponseStream()))
                {
                    var date = response.Headers[HttpResponseHeader.Date];
                    DateTime d, lm;

                    if (string.IsNullOrEmpty(date) || !DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out d))
                        d = DateTime.MinValue;

                    date = response.Headers[HttpResponseHeader.LastModified];
                    if (string.IsNullOrEmpty(date) || !DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out lm))
                        lm = DateTime.MinValue;

                    return new ResponseData<string>(d, lm, await r.ReadToEndAsync());
                }
            }
        }

        public static async Task<ResponseData<byte[]>> DownloadBytesAsync(string url)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.Timeout = 10000;
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    var date = response.Headers[HttpResponseHeader.Date];
                    DateTime d, lm;

                    if (string.IsNullOrEmpty(date) || !DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out d))
                        d = DateTime.MinValue;

                    date = response.Headers[HttpResponseHeader.LastModified];
                    if (string.IsNullOrEmpty(date) || !DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out lm))
                        lm = DateTime.MinValue;

                    int contentLength = (int)response.ContentLength;
                    if (contentLength < 0)
                        contentLength = 0;
                    using (var ms = new MemoryStream(contentLength))
                    {
                        await stream.CopyToAsync(ms);

                        byte[] bytes;
                        if (ms.Capacity == contentLength)
                            bytes = ms.GetBuffer();
                        else
                            bytes = ms.ToArray();

                        return new ResponseData<byte[]>(d, lm, ms.ToArray());
                    }
                }
            }
        }
    }
}
