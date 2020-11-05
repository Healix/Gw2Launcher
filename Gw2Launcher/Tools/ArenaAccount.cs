using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Net;
using System.IO;

namespace Gw2Launcher.Tools
{
    class ArenaAccount
    {
        private const string URL_BASE = "https://account.arena.net/";

        /// <summary>
        /// The login or authentication code failed
        /// </summary>
        public class AuthenticationException : Exception
        {

        }

        /// <summary>
        /// Attempting to access the site failed; it was likely changed
        /// </summary>
        public class UnexpectedResponseException : Exception
        {

        }

        /// <summary>
        /// Session is not valid; login required
        /// </summary>
        public class SessionExpiredException : Exception
        {

        }

        public enum AuthenticationType
        {
            Unknown,
            Email,
            SMS,
            TOTP
        }

        public static event EventHandler<int> LoginResponseDuration;

        public class AuthorizedNetwork
        {
            public AuthorizedNetwork(string ip, int prefix)
            {
                this.Address = ip;
                this.NetworkPrefix = prefix;
            }

            public string Address
            {
                get;
                private set;
            }

            public int NetworkPrefix
            {
                get;
                private set;
            }
        }

        private CookieContainer cookies;
        private DateTime dateSet, dateServer;
        private string session;

        private ArenaAccount()
        {
        }

        /// <summary>
        /// True if an authentication code is required to complete the login
        /// </summary>
        public bool RequiresAuthentication
        {
            get;
            private set;
        }

        /// <summary>
        /// Type of authentication the login is requiring
        /// </summary>
        public AuthenticationType Authentication
        {
            get;
            private set;
        }

        /// <summary>
        /// The estimated current date/time of the server in UTC
        /// </summary>
        public DateTime Date
        {
            get
            {
                if (dateSet != DateTime.MinValue)
                    return dateServer.Add(DateTime.UtcNow.Subtract(dateSet));
                return DateTime.MinValue;
            }
            private set
            {
                dateServer = value;
                dateSet = DateTime.UtcNow;
            }
        }

        public bool HasSession
        {
            get
            {
                return cookies != null;
            }
        }

        public bool IsExpired
        {
            get
            {
                if (dateServer != DateTime.MinValue)
                {
                    return DateTime.UtcNow.Subtract(dateSet).TotalMinutes > 10 || cookies == null;
                }

                return false;
            }
        }

        private string GetRedirect(string data)
        {
            var i = data.IndexOf("\"redirect\":\"");

            if (i != -1)
            {
                i += 12;
                var j = data.IndexOf('"', i);
                var redirect = data.Substring(i, j - i);

                return redirect;
            }

            return null;
        }

        private bool OnHeadersReceived(WebHeaderCollection headers)
        {
            var cookie = headers[HttpResponseHeader.SetCookie];
            if (cookie != null && cookie.StartsWith("s="))
            {
                var key = cookie.Substring(2, cookie.IndexOf(';') - 2);

                if (session.Equals(key, StringComparison.Ordinal))
                {
                    return true;
                }
                else
                {
                    this.cookies = null;
                    throw new SessionExpiredException();
                }
            }
            return false;
        }

        private bool IsLoginRequired(string data)
        {
            var redirect = GetRedirect(data);

            return redirect != null && redirect.StartsWith("/login");
        }

        private void ThrowForWebException(WebException e)
        {
            if (e.Response == null || !(e.Response is HttpWebResponse))
                return;

            using (var response = (HttpWebResponse)e.Response)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new AuthenticationException();
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.Redirect:
                    case HttpStatusCode.MovedPermanently:
                    case HttpStatusCode.TemporaryRedirect:
                    case HttpStatusCode.Forbidden:
                        throw new UnexpectedResponseException();
                }
            }
        }

        public async Task<bool> Authenticate(string key)
        {
            if (!this.RequiresAuthentication)
                throw new InvalidOperationException("Session is already authenticated");

            string name;

            switch (this.Authentication)
            {
                case AuthenticationType.Email:
                    name = "email";
                    break;
                case AuthenticationType.SMS:
                    name = "sms";
                    break;
                case AuthenticationType.TOTP:
                    name = "totp";
                    break;
                default:
                    throw new InvalidOperationException("Unknown authentication type");
            }

            var url = URL_BASE + "login/wait/" + name;

            var request = CreateHttpJsonPost(url + ".json", url + "?", cookies);
            var post = string.Format("{{\"_formName\":\"wait{0}\",\"otp\":\"{1}\",\"whitelist\":\"on\"}}", char.ToUpper(name[0]) + name.Substring(1), key);

            using (var rs = request.GetRequestStream())
            {
                var buffer = Encoding.UTF8.GetBytes(post);
                rs.Write(buffer, 0, buffer.Length);
            }

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    OnHeadersReceived(response.Headers);

                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var data = await reader.ReadToEndAsync();
                        var redirect = GetRedirect(data);

                        if (redirect != null)
                        {
                            if (redirect.StartsWith("/overview", StringComparison.OrdinalIgnoreCase))
                            {
                                this.RequiresAuthentication = false;

                                return true;
                            }
                            else
                            {
                                //unknown/failed
                            }
                        }
                    }
                }

                await Logout();
            }
            catch (WebException e)
            {
                ThrowForWebException(e);
                throw;
            }

            return false;
        }

        public async Task<AuthorizedNetwork[]> GetAuthorizedNetworks()
        {
            var request = CreateHttp(URL_BASE + "security/settings.json", URL_BASE + "overview", this.cookies);

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    OnHeadersReceived(response.Headers);

                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var data = await reader.ReadToEndAsync();
                        var json = (Dictionary<string, object>)Api.Json.Decode(data);
                        var whitelist = (List<object>)json["whitelist"];
                        var networks = new AuthorizedNetwork[whitelist.Count];
                        int i = 0;

                        foreach (Dictionary<string, object> entry in whitelist)
                        {
                            networks[i++] = new AuthorizedNetwork((string)entry["address"], int.Parse((string)entry["network_prefix"]));
                        }

                        return networks;
                    }
                }
            }
            catch (WebException e)
            {
                ThrowForWebException(e);
                throw;
            }
        }

        public async Task Remove(AuthorizedNetwork address)
        {
            var request = CreateHttpJsonPost(URL_BASE + "security/whitelist/remove.json", URL_BASE + "security/settings", cookies);
            var post = string.Format("{{\"ip\":\"{0}\",\"prefix\":\"{1}\",\"location\":\"undefined\"}}", address.Address, address.NetworkPrefix);

            using (var rs = request.GetRequestStream())
            {
                var buffer = Encoding.UTF8.GetBytes(post);
                rs.Write(buffer, 0, buffer.Length);
            }

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    OnHeadersReceived(response.Headers);

                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var data = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (WebException e)
            {
                ThrowForWebException(e);
                throw;
            }
        }

        /// <summary>
        /// Checks if the session is still valid
        /// </summary>
        public async Task<bool> Ping()
        {
            if (this.cookies == null)
                return false;

            var request = CreateHttp(URL_BASE + "login.json", URL_BASE + "overview", this.cookies);

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    OnHeadersReceived(response.Headers);

                    var date = response.Headers[HttpResponseHeader.Date];
                    if (!string.IsNullOrEmpty(date))
                    {
                        try
                        {
                            this.Date = DateTime.Parse(date, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }

                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var data = await reader.ReadToEndAsync();
                        var redirect = GetRedirect(data);

                        if (redirect != null && redirect.StartsWith("/overview", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                using (e.Response) { }
            }
            catch { }

            return false;
        }

        public async Task<bool> Login(string email, SecureString password)
        {
            var cookies = new CookieContainer(1);
            var request = CreateHttp(URL_BASE + "login.json", URL_BASE + "login", cookies);

            var start = Environment.TickCount;

            this.cookies = null;

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    var cookie = response.Headers[HttpResponseHeader.SetCookie];
                    if (cookie.StartsWith("s="))
                    {
                        var key = session = cookie.Substring(2, cookie.IndexOf(';') - 2);
                        cookies.Add(new Cookie("s", key, "/", "arena.net"));
                    }
                }
            }
            catch (WebException e)
            {
                ThrowForWebException(e);
                throw;
            }

            request = CreateHttpJsonPost(URL_BASE + "login.json", URL_BASE + "login", cookies);

            using (var rs = request.GetRequestStream())
            {
                byte[] buffer, post;

                post = Encoding.UTF8.GetBytes(string.Format("{{\"_formName\":\"login\",\"email\":\"{0}\",\"password\":\"\"}}", email));
                rs.Write(post, 0, post.Length - 2);

                var chars = Security.Credentials.ToCharArray(password);
                int last = 0,
                    i = -1;

                //escape quotes
                while ((i = Array.IndexOf<char>(chars, '"', i + 1)) != -1)
                {
                    buffer = Encoding.UTF8.GetBytes(chars, last, i - last);
                    rs.Write(buffer, 0, buffer.Length);
                    rs.WriteByte((byte)'\\');
                    Array.Clear(buffer, 0, buffer.Length);
                    last = i;
                }

                buffer = Encoding.UTF8.GetBytes(chars, last, chars.Length - last);
                rs.Write(buffer, 0, buffer.Length);

                Array.Clear(chars, 0, buffer.Length);
                Array.Clear(buffer, 0, buffer.Length);

                rs.Write(post, post.Length - 2, 2);
            }

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var date = response.Headers[HttpResponseHeader.Date];

                        if (!string.IsNullOrEmpty(date))
                        {
                            try
                            {
                                this.Date = DateTime.Parse(date, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                            }
                        }

                        if (LoginResponseDuration != null)
                        {
                            try
                            {
                                LoginResponseDuration(null, Environment.TickCount - start);
                            }
                            catch { }
                        }

                        var data = await reader.ReadToEndAsync();
                        var redirect = GetRedirect(data);

                        var cookie = response.Headers[HttpResponseHeader.SetCookie];
                        if (cookie.StartsWith("s="))
                        {
                            var key = session = cookie.Substring(2, cookie.IndexOf(';') - 2);
                            cookies.Add(new Cookie("s", key, "/", "arena.net"));
                        }

                        if (redirect != null)
                        {
                            if (redirect.StartsWith("/overview", StringComparison.OrdinalIgnoreCase))
                            {
                                //authenticated

                                this.RequiresAuthentication = false;
                                this.Authentication = AuthenticationType.Unknown;
                            }
                            else if (redirect.StartsWith("/login/wait/", StringComparison.OrdinalIgnoreCase))
                            {
                                var j = 12;
                                var l = redirect.IndexOf('?', j);
                                if (l == -1)
                                    l = redirect.Length - j;

                                var type = AuthenticationType.Unknown;

                                switch (redirect.Substring(j, l - j))
                                {
                                    case "email":
                                        type = AuthenticationType.Email;
                                        break;
                                    case "sms":
                                        type = AuthenticationType.SMS;
                                        break;
                                    case "totp":
                                        type = AuthenticationType.TOTP;
                                        break;
                                }

                                this.RequiresAuthentication = true;
                                this.Authentication = type;
                            }
                            else
                            {
                                //unknown
                                return false;
                            }

                            this.cookies = cookies;

                            return true;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                ThrowForWebException(e);
                throw;
            }

            return false;
        }

        public async Task Logout()
        {
            if (cookies == null)
                return;

            var request = CreateHttp(URL_BASE + "logout.json", URL_BASE + "overview", cookies);

            cookies = null;

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var data = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (WebException e)
            {
                using (e.Response) { }
            }
            catch
            {
            }
        }

        public static async Task<ArenaAccount> LoginAccount(string email, SecureString password)
        {
            var account = new ArenaAccount();

            if (await account.Login(email, password))
                return account;

            return null;
        }

        private static HttpWebRequest CreateHttpJsonPost(string url, string referer, CookieContainer cookies)
        {
            var request = CreateHttp(url, referer, cookies);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            return request;
        }

        private static HttpWebRequest CreateHttp(string url, string referer, CookieContainer cookies)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.AllowAutoRedirect = false;
            //request.Timeout = 60000;
            request.Referer = referer;
            request.CookieContainer = cookies;
            request.ServicePoint.Expect100Continue = false;

            return request;
        }
    }
}
