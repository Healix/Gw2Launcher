using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Gw2Launcher.Api
{
    static class Exceptions
    {
        public class InvalidKeyException : Exception
        {

        }

        public class PermissionRequiredException : Exception
        {

        }

        public static void Throw(WebException e)
        {
            using (var response = (HttpWebResponse)e.Response)
            {
                if (response != null)
                {
                    string data;
                    using (var r = new StreamReader(response.GetResponseStream()))
                    {
                        data = r.ReadToEnd();
                    }

                    if (data.IndexOf("invalid key") != -1)
                        throw new InvalidKeyException();
                    if (data.IndexOf("requires scope") != -1)
                        throw new PermissionRequiredException();
                }
            }
        }
    }
}
