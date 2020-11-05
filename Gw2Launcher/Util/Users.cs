using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;

namespace Gw2Launcher.Util
{
    static class Users
    {
        public static readonly string UserName;

        static Users()
        {
            UserName = Environment.UserName;
        }

        public static string GetUserName(string user)
        {
            if (!string.IsNullOrEmpty(user))
                return user;
            return UserName;
        }

        public static bool IsCurrentUser(string user)
        {
            if (string.IsNullOrEmpty(user))
                return true;

            return user.Equals(UserName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCurrentEnvironmentUser(string user = null)
        {
            if (string.IsNullOrEmpty(user))
                return UserName.Equals(Environment.UserName, StringComparison.Ordinal);

            return user.Equals(Environment.UserName, StringComparison.OrdinalIgnoreCase);
        }

        public static Principal GetPrincipal(string username)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Machine);
            Principal p = Principal.FindByIdentity(ctx, IdentityType.Name, username);

            return p;
        }

        public static void Activate(bool activate)
        {
            foreach (string user in Settings.HiddenUserAccounts.GetKeys())
            {
                if (Settings.HiddenUserAccounts[user].Value)
                {
                    Util.ProcessUtil.RunTask(activate ? "gw2launcher-users-active-yes" : "gw2launcher-users-active-no");
                    break;
                }
            }
        }
    }
}
