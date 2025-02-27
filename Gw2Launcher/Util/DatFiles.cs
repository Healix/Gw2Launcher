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
            foreach (var dat in Settings.DatFiles.GetValues())
            {
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
        public static List<Settings.IGw2Account> GetAccounts(Settings.IDatFile dat)
        {
            var accounts = new List<Settings.IGw2Account>();

            foreach (var a in Util.Accounts.GetGw2Accounts())
            {
                if (a.DatFile == dat)
                {
                    accounts.Add(a);
                }
            }

            return accounts;
        }
    }
}
