using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        public static event EventHandler<NetworkAuthorizationRequiredEventsArgs> NetworkAuthorizationRequired;

        public class NetworkAuthorizationRequiredEventsArgs : EventArgs
        {
            private IAuthorizationSession session;

            public NetworkAuthorizationRequiredEventsArgs(Settings.IAccount account, IAuthorizationSession session)
            {
                this.session = session;
                this.Account = account;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public Tools.ArenaAccount.AuthenticationType Authentication
            {
                get
                {
                    return session.Authentication;
                }
            }

            public Task<bool> Authenticate(string key)
            {
                return session.Authenticate(key);
            }

            public async Task<bool> Retry()
            {
                try
                {
                    return await session.Login();
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return false;
            }
        }

        public interface IAuthorizationSession
        {
            void Release();

            Tools.ArenaAccount.AuthenticationType Authentication
            {
                get;
            }

            Task<bool> Authenticate(string key);

            Task<bool> Login();
        }

        /// <summary>
        /// Drops any cached login sessions
        /// </summary>
        /// <param name="force">Includes logins currently being used</param>
        /// <param name="quick">Releases all sessions at once</param>
        public static Task ClearNetworkAuthorization(bool force, bool quick)
        {
            return NetworkAuthorization.Clear(force, quick);
        }

        public static bool HasPendingNetworkAuthorizationRequests
        {
            get
            {
                return NetworkAuthorization.IsActive;
            }
        }

        private static class NetworkAuthorization
        {
            public interface ISession : IAuthorizationSession
            {
                void Register(Account a);
            }

            private class Session : ISession
            {
                private ArenaSession session;
                private Account account;

                public Session(ArenaSession session)
                {
                    this.session = session;
                }

                ~Session()
                {
                    Release();
                }

                public ArenaSession ArenaSession
                {
                    get
                    {
                        return session;
                    }
                }

                public Tools.ArenaAccount.AuthenticationType Authentication
                {
                    get
                    {
                        return session.Session.Authentication;
                    }
                }

                public Task<bool> Authenticate(string key)
                {
                    return session.Session.Authenticate(key);
                }

                public Task<bool> Login()
                {
                    OnLoginRequest();
                    return session.Session.Login(session.Email, session.Password.ToSecureString());
                }

                public void Register(Account a)
                {
                    lock (sessions)
                    {
                        if (active.Add(a))
                        {
                            a.Process.Exited += NetworkAuthorization.Process_Exited;
                        }
                    }
                }

                public void Release()
                {
                    GC.SuppressFinalize(this);

                    ArenaSession s;

                    lock (this)
                    {
                        if (session != null)
                        {
                            s = session;
                            session = null;
                        }
                        else
                        {
                            return;
                        }
                    }

                    s.Release();
                }
            }

            private class ArenaSession : IDisposable
            {
                private class Lock : IDisposable
                {
                    private ArenaSession s;

                    public Lock(ArenaSession s)
                    {
                        this.s = s;
                    }

                    ~Lock()
                    {
                        Dispose();
                    }

                    public void Dispose()
                    {
                        lock (this)
                        {
                            if (s != null)
                            {
                                Monitor.Enter(s);

                                s._lock = null;

                                Monitor.Pulse(s);
                                Monitor.Exit(s);

                                s = null;
                            }
                        }
                    }
                }

                private Tools.ArenaAccount session;
                public byte _locks;
                public DateTime _released;
                private Lock _lock;

                public ArenaSession(string email)
                {
                    this.Email = email;
                }

                public bool HasSession
                {
                    get
                    {
                        return session != null && session.HasSession;
                    }
                }

                public bool IsAlive
                {
                    get
                    {
                        return session != null && session.HasSession && !session.IsExpired;
                    }
                }

                public string Email
                {
                    get;
                    private set;
                }

                public Settings.PasswordString Password
                {
                    get;
                    set;
                }

                public Tools.ArenaAccount Session
                {
                    get
                    {
                        return session;
                    }
                    set
                    {
                        session = value;
                    }
                }

                public void Release()
                {
                    if (!IsDisposed)
                        NetworkAuthorization.Release(this);
                }

                public IDisposable GetLock()
                {
                    Monitor.Enter(this);
                    try
                    {
                        while (true)
                        {
                            if (_lock == null)
                            {
                                return _lock = new Lock(this);
                            }
                            else
                            {
                                Monitor.Wait(_lock);
                            }
                        }
                    }
                    finally
                    {
                        if (Monitor.IsEntered(this))
                            Monitor.Exit(this);
                    }
                }

                public IDisposable TryGetLock()
                {
                    if (Monitor.TryEnter(this))
                    {
                        if (_lock == null)
                        {
                            return _lock = new Lock(this);
                        }

                        Monitor.Exit(this);
                    }

                    return null;
                }

                public bool IsDisposed
                {
                    get;
                    private set;
                }

                public void Dispose()
                {
                    IsDisposed = true;
                }
            }

            public enum VerifyResult
            {
                None,
                OK,
                Completed,
                Required
            }

            public static event EventHandler LoginRequest;
            public static event EventHandler<int> LoginComplete;

            private static Dictionary<string, ArenaSession> sessions;
            private static Queue<ArenaSession> releasing;
            private static HashSet<Account> active;
            private static DateTime nextCheck;

            static NetworkAuthorization()
            {
                sessions = new Dictionary<string, ArenaSession>(StringComparer.OrdinalIgnoreCase);
                active = new HashSet<Account>();
            }

            public static ISession Verify(Settings.IAccount account, Action onAuthenticationRequired)
            {
                ISession session;

                if (Verify(account, out session, false, onAuthenticationRequired) == VerifyResult.Required)
                {
                    try
                    {
                        Authenticate(account, session);
                    }
                    catch
                    {
                        if (session != null)
                            session.Release();
                        throw;
                    }
                }

                return session;
            }

            private static VerifyResult Verify(Settings.IAccount account, out ISession session, bool force, Action onAuthenticationRequired)
            {
                var doCheck = false;

                switch (account.NetworkAuthorizationState)
                {
                    case Settings.NetworkAuthorizationState.Unknown:

                        doCheck = true;

                        break;
                    case Settings.NetworkAuthorizationState.OK:

                        if (force || Settings.NetworkAuthorization.Value.HasFlag(Settings.NetworkAuthorizationFlags.RemoveAll))
                        {
                            doCheck = true;
                        }
                        else
                        {
                            switch (Settings.NetworkAuthorization.Value & Settings.NetworkAuthorizationFlags.VerificationModes)
                            {
                                case Settings.NetworkAuthorizationFlags.Automatic:

                                    //check if no accounts (with authorization enabled) are active

                                    lock (sessions)
                                    {
                                        doCheck = active.Count == 0;
                                    }

                                    if (doCheck && Settings.NetworkAuthorization.Value.HasFlag(Settings.NetworkAuthorizationFlags.VerifyIP))
                                    {
                                        try
                                        {
                                            if (DoVerifyIP())
                                                doCheck = false;
                                        }
                                        catch { }
                                    }

                                    if (!doCheck && account.HasCredentials)
                                    {
                                        session = GetSession(account.Email);
                                        return VerifyResult.OK;
                                    }

                                    break;
                                case Settings.NetworkAuthorizationFlags.Always:

                                    doCheck = true;

                                    break;
                            }
                        }

                        break;
                }

                if (!doCheck || !account.HasCredentials)
                {
                    session = null;
                    return VerifyResult.None;
                }

                if (onAuthenticationRequired != null)
                {
                    try
                    {
                        onAuthenticationRequired();
                    }
                    catch { }
                }

                var s = GetSession(account.Email);
                var l = s.ArenaSession.GetLock();

                session = null;

                try
                {
                    var r = DoVerify(s.ArenaSession, account);

                    if (r != VerifyResult.None)
                    {
                        session = s;
                    }

                    return r;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);

                    lock (sessions)
                    {
                        if (s.ArenaSession._locks == 0 && !s.ArenaSession.HasSession)
                        {
                            sessions.Remove(s.ArenaSession.Email);
                            s.ArenaSession.Dispose();
                            s = null;
                        }
                    }

                    //if (e.InnerException is Tools.ArenaAccount.UnexpectedResponseException)
                    //{
                    //    nextNetworkCheck = DateTime.UtcNow.AddMinutes(10);
                    //}

                    if (Settings.NetworkAuthorization.Value.HasFlag(Settings.NetworkAuthorizationFlags.AbortLaunchingOnFail))
                        throw new Exception("Unable to verify network authorization");

                    return VerifyResult.None;
                }
                finally
                {
                    l.Dispose();

                    if (session == null && s != null)
                        s.Release();
                }
            }

            private static void OnLoginRequest()
            {
                try
                {
                    if (LoginRequest != null)
                        LoginRequest(null, null);
                }
                catch { }
            }

            /// <summary>
            /// Returns true if the IP matches
            /// </summary>
            private static bool DoVerifyIP()
            {
                var match = false;

                using (var t = Net.IP.GetPublicAddress())
                {
                    t.Wait();

                    var ip = t.Result;

                    if (ip != null)
                    {
                        var current = ip.GetAddressBytes();
                        var existing = Settings.PublicIPAddress.Value;
                        
                        if (existing != null && current.Length == existing.Length)
                        {
                            match = true;

                            for (var i = current.Length - 2; i >= 0; --i)
                            {
                                if (current[i] != existing[i])
                                {
                                    match = false;
                                    break;
                                }
                            }

                            if (match && existing[existing.Length - 1] == current[current.Length - 1])
                                return true; //unchanged
                        }

                        Settings.PublicIPAddress.Value = current;
                    }
                }

                return match;
            }

            /// <summary>
            /// Logs into the account to check if authentication is required
            /// </summary>
            private static VerifyResult DoVerify(ArenaSession s, Settings.IAccount account)
            {
                using (var t = VerifySession(s, account.Password, true))
                {
                    t.Wait();
                    if (!t.Result)
                        throw new Exception("Login failed");
                }

                if (!s.Session.RequiresAuthentication)
                {
                    if (account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled)
                        account.NetworkAuthorizationState = Settings.NetworkAuthorization.Value.HasFlag(Settings.NetworkAuthorizationFlags.RemoveAll) ? Settings.NetworkAuthorizationState.Unknown : Settings.NetworkAuthorizationState.OK;

                    return VerifyResult.OK;
                }

                if (account.NetworkAuthorizationState == Settings.NetworkAuthorizationState.OK)
                {
                    //account was previously authorized, assuming IP was changed - flag all accounts
                    foreach (var _a in Util.Accounts.GetAccounts())
                    {
                        if (_a.NetworkAuthorizationState == Settings.NetworkAuthorizationState.OK)
                            _a.NetworkAuthorizationState = Settings.NetworkAuthorizationState.Unknown;
                    }
                }

                switch (s.Session.Authentication)
                {
                    case Tools.ArenaAccount.AuthenticationType.Email:
                    case Tools.ArenaAccount.AuthenticationType.SMS:
                        break;
                    case Tools.ArenaAccount.AuthenticationType.TOTP:

                        if (account.TotpKey != null)
                        {
                            var now = DateTime.UtcNow.Ticks;
                            byte retry = 0;

                            do
                            {
                                try
                                {
                                    if (retry > 0)
                                    {
                                        OnLoginRequest();

                                        //authorizing can only be attempted once per session
                                        using (var t = s.Session.Login(s.Email, account.Password.ToSecureString()))
                                        {
                                            t.Wait();
                                            if (t.Result)
                                            {
                                                s.Password = account.Password;
                                                if (!s.Session.RequiresAuthentication || s.Session.Authentication != Tools.ArenaAccount.AuthenticationType.TOTP)
                                                    break;
                                            }
                                            else
                                                break;
                                        }

                                        if (retry > 1)
                                            break;

                                        //retrying using the server's time
                                        now = s.Session.Date.Ticks;
                                        if (now <= 0)
                                            now = DateTime.UtcNow.Ticks;
                                    }

                                    var key = Tools.Totp.Generate(account.TotpKey, now);
                                    using (var t = s.Session.Authenticate(new string(key)))
                                    {
                                        t.Wait();
                                        if (t.Result)
                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);

                                    if (e.InnerException != null)
                                    {
                                        if (retry == 0 && (e.InnerException is Tools.ArenaAccount.AuthenticationException || e.InnerException is Tools.ArenaAccount.SessionExpiredException))
                                        {
                                            retry++;

                                            continue;
                                        }
                                    }

                                    retry += 2;
                                }

                                break;
                            }
                            while (true);
                        }

                        break;
                    default:
                        return VerifyResult.None;
                }

                if (s.Session.RequiresAuthentication)
                {
                    return VerifyResult.Required;
                }
                else
                {
                    if (account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled)
                        account.NetworkAuthorizationState = Settings.NetworkAuthorization.Value.HasFlag(Settings.NetworkAuthorizationFlags.RemoveAll) ? Settings.NetworkAuthorizationState.Unknown : Settings.NetworkAuthorizationState.OK;

                    return VerifyResult.Completed;
                }
            }

            private static Session GetSession(string email)
            {
                ArenaSession s;

                lock (sessions)
                {
                    if (!sessions.TryGetValue(email, out s))
                    {
                        sessions[email] = s = new ArenaSession(email);
                    }
                    ++s._locks;
                }

                return new Session(s);
            }

            private static async Task<bool> VerifySession(ArenaSession s, Settings.PasswordString password, bool ping)
            {
                var b = s.HasSession;

                if (b && ping)
                {
                    try
                    {
                        b = await s.Session.Ping();
                    }
                    catch 
                    {
                        b = false;
                    }
                }

                if (!b)
                {
                    if (password == null || password.Data.IsEmpty)
                    {
                        return false;
                    }

                    OnLoginRequest();

                    using (var t = Tools.ArenaAccount.LoginAccount(s.Email, password.ToSecureString()))
                    {
                        await t;

                        s.Session = t.Result;

                        if (s.Session != null)
                        {
                            s.Password = password;

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            /// <summary>
            /// Ends the session
            /// </summary>
            /// <param name="force">Forces logout on sessions in use</param>
            /// <param name="login">Login if required, otherwise it'll be skipped</param>
            /// <param name="quick">Only logout</param>
            private static async Task<bool> EndSession(ArenaSession s, bool force = false, bool login = true, bool quick = false)
            {
                if (!s.HasSession)
                    return true;

                var retry = true;

                do
                {
                    var v = Settings.NetworkAuthorization.Value;
                    var removeAll = v.HasFlag(Settings.NetworkAuthorizationFlags.RemoveAll);
                    var removeOthers = removeAll || v.HasFlag(Settings.NetworkAuthorizationFlags.RemovePreviouslyAuthorized);

                    var b = false;

                    try
                    {
                        if (removeOthers && !quick)
                        {
                            //warning: verifying could potentially take a minute
                            if (login && !await VerifySession(s, s.Password, false))
                                throw new Exception("Login failed");

                            if (!force)
                            {
                                lock (sessions)
                                {
                                    if (s._locks > 0)
                                        return false;
                                }
                            }

                            b = !s.Session.RequiresAuthentication;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);

                        b = false;
                    }

                    try
                    {
                        if (b)
                        {
                            if (removeOthers)
                            {
                                var networks = await s.Session.GetAuthorizedNetworks();
                                var count = networks.Length;

                                if (!removeAll)
                                    --count;

                                for (var i = 0; i < count; i++)
                                {
                                    await s.Session.Remove(networks[i]);
                                }
                            }
                        }

                        retry = false;

                        if (s.HasSession)
                        {
                            await s.Session.Logout();
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);

                        if (e is Tools.ArenaAccount.SessionExpiredException && retry && login)
                        {
                            retry = false;
                            continue;
                        }
                    }

                    break;
                }
                while (true);

                return true;
            }

            private static void Authenticate(Settings.IAccount account, ISession session)
            {
                var s = ((Session)session).ArenaSession.Session;

                if (s.RequiresAuthentication && NetworkAuthorizationRequired != null)
                {
                    try
                    {
                        NetworkAuthorizationRequired(null, new NetworkAuthorizationRequiredEventsArgs(account, session));
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                var v = Settings.NetworkAuthorization.Value;

                if (s.RequiresAuthentication)
                {
                    if (v.HasFlag(Settings.NetworkAuthorizationFlags.AbortLaunchingOnFail))
                        throw new Exception("Unable to authorize the current network");
                }
                else
                {
                    if (account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled)
                        account.NetworkAuthorizationState = v.HasFlag(Settings.NetworkAuthorizationFlags.RemoveAll) ? Settings.NetworkAuthorizationState.Unknown : Settings.NetworkAuthorizationState.OK;
                }
            }

            private static async void ReleaseAsync()
            {
                var delay = true;
                var last = DateTime.UtcNow;

                EventHandler onQueueComplete = delegate
                {
                    last = DateTime.UtcNow;
                };

                AllQueuedLaunchesComplete += onQueueComplete;

                while (true)
                {
                    if (delay)
                    {
                        await Task.Delay(20000);
                    }

                    ArenaSession s = null;

                    lock (sessions)
                    {
                        int count = releasing != null ? releasing.Count : 0;

                        if (count == 0)
                        {
                            releasing = null;
                            AllQueuedLaunchesComplete -= onQueueComplete;
                            return;
                        }

                        var now = DateTime.UtcNow;
                        var empty = GetPendingLaunchCount() == 0 && now.Subtract(last).TotalSeconds > 10;

                        while (count-- > 0)
                        {
                            var _s = releasing.Dequeue();

                            if (_s._locks == 0)
                            {
                                if (!_s.HasSession)
                                {
                                    if (_s.IsDisposed)
                                        continue;

                                    //session was never used and can simply be dropped
                                    var l = _s.TryGetLock();
                                    if (l != null)
                                    {
                                        try
                                        {
                                            sessions.Remove(_s.Email);
                                            _s.Dispose();
                                        }
                                        finally
                                        {
                                            l.Dispose();
                                        }

                                        continue;
                                    }
                                }
                                else if (empty)
                                {
                                    //sessions will be dropped after 30s when no accounts are queued
                                    if (now.Subtract(_s._released).TotalSeconds > 30)
                                    {
                                        s = _s;
                                        break;
                                    }
                                }
                                else if (now.Subtract(_s._released).TotalMinutes > 5)
                                {
                                    //assuming it isn't needed
                                    s = _s;
                                    break;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            releasing.Enqueue(_s);
                        }
                    }

                    if (s != null)
                    {
                        delay = false;
                        var removed = false;

                        try
                        {
                            removed = await OnRelease(s);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }

                        if (!removed)
                        {
                            lock (sessions)
                            {
                                if (s._locks == 0)
                                {
                                    releasing.Enqueue(s);
                                }
                            }
                        }
                    }
                    else
                    {
                        delay = true;
                    }
                }
            }

            private static void Release(ArenaSession s)
            {
                lock (sessions)
                {
                    var b = s._locks == 0;

                    if (!b && --s._locks == 0)
                    {
                        s._released = DateTime.UtcNow;
                        b = true;
                    }

                    if (b)
                    {
                        if (!s.HasSession)
                        {
                            var l = s.TryGetLock();
                            if (l != null)
                            {
                                try
                                {
                                    sessions.Remove(s.Email);
                                    s.Dispose();
                                    return;
                                }
                                finally
                                {
                                    l.Dispose();
                                }
                            }
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (releasing != null)
                    {
                        releasing.Enqueue(s);
                    }
                    else
                    {
                        releasing = new Queue<ArenaSession>();
                        releasing.Enqueue(s);
                        ReleaseAsync();
                    }
                }
            }

            private static async Task<bool> OnRelease(ArenaSession s)
            {
                var l = s.GetLock();

                try
                {
                    using (var t = EndSession(s))
                    {
                        if (await t)
                        {
                            lock (sessions)
                            {
                                if (s._locks == 0)
                                {
                                    s.Session = null;
                                    sessions.Remove(s.Email);
                                    s.Dispose();

                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
                finally
                {
                    l.Dispose();
                }

                return false;
            }

            /// <summary>
            /// Immediately releases any stored sessions
            /// </summary>
            /// <param name="force">True to include sessions currently being used</param>
            /// <param name="quick">Releases all at once</param>
            public static async Task Clear(bool force, bool quick)
            {
                ArenaSession[] _sessions;
                int count;

                lock (sessions)
                {
                    count = sessions.Count;
                    if (count == 0)
                        return;

                    _sessions = sessions.Values.ToArray();

                    if (force)
                    {
                        sessions.Clear();

                        for (var i = _sessions.Length - 1; i >= 0; --i)
                        {
                            _sessions[i].Dispose();
                        }
                    }
                    else
                    {
                        for (var i = _sessions.Length - 1; i >= 0; --i)
                        {
                            if (_sessions[i]._locks == 0)
                            {
                                sessions.Remove(_sessions[i].Email);
                                _sessions[i].Dispose();
                            }
                            else
                            {
                                _sessions[i] = null;
                                --count;
                            }
                        }
                    }

                    if (releasing != null)
                        releasing.Clear();
                }

                if (count == 0)
                    return;

                if (quick)
                {
                    var tasks = new Task[count];
                    var i = 0;

                    foreach (var s in _sessions)
                    {
                        if (s == null)
                            continue;
                        try
                        {
                            tasks[i++] = EndSession(s, true, false, force && s._locks > 0);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }

                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch { }

                    foreach (var s in _sessions)
                    {
                        if (s == null)
                            continue;
                        s.Session = null;
                    }
                }
                else
                {
                    foreach (var s in _sessions)
                    {
                        if (s == null)
                            continue;
                        try
                        {
                            await EndSession(s, true, false);
                            s.Session = null;
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                    }
                }
            }

            public static bool IsActive
            {
                get
                {
                    lock (sessions)
                    {
                        return sessions.Count > 0;
                    }
                }
            }

            static void Process_Exited(object sender, Account e)
            {
                lock (sessions)
                {
                    e.Process.Exited -= Process_Exited;
                    active.Remove(e);
                }
            }
        }
    }
}
