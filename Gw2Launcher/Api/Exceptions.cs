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
        public class ApiException : Exception
        {

        }

        /// <summary>
        /// Warning: the API can return this error for a valid key
        /// </summary>
        public class InvalidKeyException : ApiException
        {

        }

        public class PermissionRequiredException : ApiException
        {

        }

        public class ApiNotActiveException : ApiException
        {

        }

        public class IdsInvalidException : ApiException
        {

        }

        public class ServiceUnavailableException : ApiException
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

                    var i = data.IndexOf(':');

                    if (i != -1)
                    {
                        var l = data.Length;

                        ++i;

                        while (i < l && data[i] == ' ')
                        {
                            ++i;
                        }

                        int j;

                        if (i < l)
                        {
                            switch (data[i])
                            {
                                case '"':
                                case '\'':

                                    j = data.IndexOf(data[i], i + 1);
                                    ++i;

                                    break;
                                default:

                                    j = data.IndexOfAny(new char[] { ',', '\r', '\n' }, i);

                                    break;
                            }
                        }
                        else
                        {
                            j = -1;
                        }

                        if (j == -1)
                        {
                            j = data.Length;
                        }

                        var text = data.Substring(i, j - i);

                        switch (text)
                        {
                            case "invalid key":
                            case "Invalid access token":

                                throw new InvalidKeyException();

                            case "requires scope":

                                throw new PermissionRequiredException();

                            case "API not active":

                                throw new ApiNotActiveException();

                            case "all ids provided are invalid":

                                throw new IdsInvalidException();

                            default:

                                if (Util.Logging.Enabled)
                                {
                                    Util.Logging.Log("Unknown API exception [" + text + "]\r\n" + data);
                                }

                                break;
                        }
                    }

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        throw new ServiceUnavailableException();
                }
            }
        }
    }
}
