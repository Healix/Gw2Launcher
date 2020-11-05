using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Security
{
    public static class Impersonation
    {
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;

        private static WindowsIdentity defaultIdentity;
        
        [DllImport(NativeMethods.ADVAPI32, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, IntPtr lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        public interface IIdentity : IDisposable
        {
            IDisposable Impersonate();
        }

        private class ImpersonationToken : IDisposable
        {
            public ImpersonationToken(SafeTokenHandle token)
            {
                try
                {
                    this.Token = token;
                    this.Identity = new WindowsIdentity(token.DangerousGetHandle());
                    this.Context = this.Identity.Impersonate();
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    this.Dispose();
                    throw;
                }
            }

            public ImpersonationToken(WindowsImpersonationContext context)
            {
                this.Context = context;
            }

            public SafeTokenHandle Token
            {
                get;
                private set;
            }

            public WindowsIdentity Identity
            {
                get;
                private set;
            }

            public WindowsImpersonationContext Context
            {
                get;
                private set;
            }

            public void Dispose()
            {
                if (Context != null)
                    Context.Dispose();
                if (Identity != null)
                    Identity.Dispose();
                if (Token != null)
                    Token.Dispose();
            }
        }

        private class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            [DllImport(NativeMethods.KERNEL32)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        private class ImpersonationIdentity : IIdentity
        {
            private WindowsIdentity identity;
            private SafeTokenHandle token;

            public ImpersonationIdentity(WindowsIdentity context)
            {
                this.identity = context;
            }

            public ImpersonationIdentity(SafeTokenHandle token)
            {
                try
                {
                    this.token = token;
                    this.identity = new WindowsIdentity(token.DangerousGetHandle());
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    this.Dispose();
                    throw;
                }
            }

            public IDisposable Impersonate()
            {
                return identity.Impersonate();
            }

            public void Dispose()
            {
                if (identity != null)
                {
                    identity.Dispose();
                    identity = null;
                }
                if (token != null)
                {
                    token.Dispose();
                    token = null;
                }
            }
        }

        public class BadUsernameOrPasswordException : Exception
        {

        }

        /// <summary>
        /// Impersonates the user until the token is disposed
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Token to cancel the impersonation</returns>
        public static IDisposable Impersonate(string username, SecureString password)
        {
            return new ImpersonationToken(GetToken(username, password));
        }

        public static IIdentity GetIdentity(string username, SecureString password)
        {
            return new ImpersonationIdentity(GetToken(username, password));
        }

        private static SafeTokenHandle GetToken(string username, SecureString password)
        {
            if (string.IsNullOrEmpty(username) || password == null || password.Length == 0)
            {
                throw new BadUsernameOrPasswordException();
            }

            SafeTokenHandle token;

            IntPtr ptr = Marshal.SecureStringToBSTR(password);
            try
            {
                if (!LogonUser(username, ".", ptr, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out token))
                {
                    int ret = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(ret);
                }
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                Util.Logging.Log(e);
                //1327: account restriction/blank password
                //1331: account is disabled
                if (((System.ComponentModel.Win32Exception)e).NativeErrorCode == 1326)
                    throw new BadUsernameOrPasswordException();
                else
                    throw;
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }

            try
            {
                return token;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Impersonates the original user
        /// </summary>
        /// <returns>Token to cancel the impersonation</returns>
        public static IDisposable Impersonate()
        {
            return defaultIdentity.Impersonate();
        }

        public static IDisposable Impersonate(IIdentity identity)
        {
            if (identity == null)
                return null;
            return identity.Impersonate();
        }

        public static void EnsureDefault()
        {
            if (defaultIdentity == null)
            {
                try
                {
                    defaultIdentity = WindowsIdentity.GetCurrent();
                }
                catch { }
            }
        }
    }
}
