using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace Gw2Launcher.Security
{
    static class Impersonation
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, IntPtr lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;

        public interface IImpersonationToken : IDisposable
        {

        }

        private class ImpersonationToken : IImpersonationToken
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

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
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
        public static IImpersonationToken Impersonate(string username, SecureString password)
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
                return new ImpersonationToken(token);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                throw;
            }
        }
    }
}
