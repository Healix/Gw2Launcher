using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        public static event EventHandler<NetworkAuthorizationRequiredEventsArgs> NetworkAuthorizationRequired;

        public class NetworkAuthorizationRequiredEventsArgs : EventArgs
        {
            private Tools.ArenaAccount account;

            public NetworkAuthorizationRequiredEventsArgs(Settings.IAccount account, Tools.ArenaAccount arena)
            {
                this.account = arena;
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
                    return account.Authentication;
                }
            }

            public Task<bool> Authenticate(string key)
            {
                return account.Authenticate(key);
            }

            public async Task<bool> Retry()
            {
                if (this.Account.HasCredentials)
                {
                    try
                    {
                        return await account.Login(this.Account.Email, this.Account.Password);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                return false;
            }
        }

        private static class NetworkAuthorization
        {
            public enum VerifyResult
            {
                None,
                OK,
                Completed,
                Required
            }

            private static DateTime nextNetworkCheck;
            
            public static void Verify(Settings.IAccount account, Tools.ArenaAccount session)
            {
                if (Verify(account, false, session, out session) == VerifyResult.Required)
                    Authenticate(account, session);
            }

            public static VerifyResult Verify(Settings.IAccount account, bool force, Tools.ArenaAccount sessionExisting, out Tools.ArenaAccount session)
            {
                var v = Settings.NetworkAuthorization.Value;
                bool doCheck, update = false;
                session = sessionExisting;

                switch (account.NetworkAuthorizationState)
                {
                    case Settings.NetworkAuthorizationState.Unknown:

                        doCheck = true;

                        break;
                    case Settings.NetworkAuthorizationState.OK:

                        if (force)
                        {
                            doCheck = true;
                            break;
                        }

                        switch (v & Settings.NetworkAuthorizationFlags.VerificationModes)
                        {
                            case Settings.NetworkAuthorizationFlags.Automatic:

                                var now = DateTime.UtcNow;
                                if (doCheck = now > nextNetworkCheck && Launcher.GetActiveProcessCount() == 0)
                                    update = true;

                                break;
                            case Settings.NetworkAuthorizationFlags.Always:

                                doCheck = true;

                                break;
                            default:
                                return VerifyResult.None;
                        }

                        break;
                    case Settings.NetworkAuthorizationState.Disabled:
                    default:
                        return VerifyResult.None;
                }

                if (!doCheck || !account.HasCredentials)
                    return VerifyResult.None;

                try
                {
                    if (session == null)
                    {
                        using (var t = Tools.ArenaAccount.LoginAccount(account.Email, account.Password))
                        {
                            t.Wait();
                            session = t.Result;
                        }
                        if (session == null)
                            return VerifyResult.None;
                        if (update)
                            nextNetworkCheck = DateTime.UtcNow.AddMinutes(10);
                    }
                    if (!session.RequiresAuthentication)
                    {
                        if (account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled)
                            account.NetworkAuthorizationState = Settings.NetworkAuthorizationState.OK;

                        var t = Complete(session, false);

                        return VerifyResult.OK;
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    session = null;

                    if (e.InnerException is Tools.ArenaAccount.UnexpectedResponseException)
                    {
                        if (update)
                            nextNetworkCheck = DateTime.UtcNow.AddMinutes(10);
                    }
                }

                if (session == null)
                    return VerifyResult.None;

                if (account.NetworkAuthorizationState == Settings.NetworkAuthorizationState.OK)
                {
                    //account was previously authorized, assuming IP was changed - flag all accounts
                    foreach (var uid in Settings.Accounts.GetKeys())
                    {
                        var _a = Settings.Accounts[uid];
                        if (_a.HasValue && _a.Value.NetworkAuthorizationState == Settings.NetworkAuthorizationState.OK)
                            _a.Value.NetworkAuthorizationState = Settings.NetworkAuthorizationState.Unknown;
                    }
                }

                switch (session.Authentication)
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
                                        //authorizing can only be attempted once per session
                                        using (var t = session.Login(account.Email, account.Password))
                                        {
                                            t.Wait();
                                            if (t.Result)
                                            {
                                                if (!session.RequiresAuthentication || session.Authentication != Tools.ArenaAccount.AuthenticationType.TOTP)
                                                    break;
                                            }
                                            else
                                                break;
                                        }

                                        if (retry > 1)
                                            break;

                                        //retrying using the server's time
                                        now = session.Date.Ticks;
                                        if (now <= 0)
                                            now = DateTime.UtcNow.Ticks;
                                    }

                                    var key = Tools.Totp.Generate(account.TotpKey, now);
                                    using (var t = session.Authenticate(new string(key)))
                                    {
                                        t.Wait();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Util.Logging.Log(e);

                                    if (e.InnerException != null)
                                    {
                                        if (retry == 0 && e.InnerException is Tools.ArenaAccount.AuthenticationException)
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

                if (session.RequiresAuthentication)
                {
                    return VerifyResult.Required;
                }
                else
                {
                    var t = Complete(session, v.HasFlag(Settings.NetworkAuthorizationFlags.RemovePreviouslyAuthorized));

                    if (account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled)
                        account.NetworkAuthorizationState = Settings.NetworkAuthorizationState.OK;

                    return VerifyResult.Completed;
                }
            }

            public static void Authenticate(Settings.IAccount account, Tools.ArenaAccount session)
            {
                if (session.RequiresAuthentication && NetworkAuthorizationRequired != null)
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

                if (session.RequiresAuthentication)
                {
                    var t = Complete(session, false);

                    if (v.HasFlag(Settings.NetworkAuthorizationFlags.AbortLaunchingOnFail))
                        throw new Exception("Unable to authorize the current network");
                }
                else
                {
                    var t = Complete(session, v.HasFlag(Settings.NetworkAuthorizationFlags.RemovePreviouslyAuthorized));

                    if (account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled)
                        account.NetworkAuthorizationState = Settings.NetworkAuthorizationState.OK;
                }
            }

            private static async Task Complete(Tools.ArenaAccount session, bool removeOtherNetworks)
            {
                try
                {
                    if (removeOtherNetworks)
                    {
                        var networks = await session.GetAuthorizedNetworks();
                        for (var i = 0; i < networks.Length - 1; i++)
                        {
                            await session.Remove(networks[i]);
                        }
                    }
                    await session.Logout();
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }
    }
}
