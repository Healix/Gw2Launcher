using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class Accounts
    {
        public static IEnumerable<Settings.IAccount> GetAccounts()
        {
            foreach (var uid in Settings.Accounts.GetKeys())
            {
                var a = Settings.Accounts[uid];
                var account = a.Value;

                if (a.HasValue && account != null)
                {
                    yield return a.Value;
                }
            }
        }

        public static IEnumerable<Settings.IAccount> GetAccounts(Settings.AccountType type)
        {
            foreach (var uid in Settings.Accounts.GetKeys())
            {
                var a = Settings.Accounts[uid];
                var account = a.Value;

                if (a.HasValue && account != null && a.Value.Type == type)
                {
                    yield return a.Value;
                }
            }
        }

        public static IEnumerable<Settings.IGw2Account> GetGw2Accounts()
        {
            foreach (var gw2 in GetAccounts(Settings.AccountType.GuildWars2))
            {
                yield return (Settings.IGw2Account)gw2;
            }
        }

        public static IEnumerable<Settings.IGw1Account> GetGw1Accounts()
        {
            foreach (var gw1 in GetAccounts(Settings.AccountType.GuildWars1))
            {
                yield return (Settings.IGw1Account)gw1;
            }
        }
    }
}
