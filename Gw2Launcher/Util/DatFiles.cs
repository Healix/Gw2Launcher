using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class DatFiles
    {
        /// <summary>
        /// If stored in the settings, returns the dat file for the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Settings.IDatFile Get(string path)
        {
            foreach (ushort fid in Settings.DatFiles.GetKeys())
            {
                var dat = Settings.DatFiles[fid];
                if (dat.HasValue && path.Equals(dat.Value.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return dat.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Lists all accounts using the specified dat file
        /// </summary>
        /// <param name="dat"></param>
        /// <returns></returns>
        public static List<Settings.IAccount> GetAccounts(Settings.IDatFile dat)
        {
            List<Settings.IAccount> accounts = new List<Settings.IAccount>();

            foreach (ushort uid in Settings.Accounts.GetKeys())
            {
                var account = Settings.Accounts[uid];
                if (account.HasValue && account.Value.DatFile == dat)
                {
                    accounts.Add(account.Value);
                }
            }

            return accounts;
        }
    }
}
