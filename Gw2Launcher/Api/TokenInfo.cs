using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Api
{
    public static class TokenInfo
    {
        [Flags]
        public enum Permissions : ushort
        {
            None = 0,
            Unknown = 1,
            Account = 2,
            Inventories = 4,
            Characters = 8,
            Tradingpost = 16,
            Wallet = 32,
            Unlocks = 64,
            Pvp = 128,
            Builds = 256,
            Progression = 512,
            Guilds = 1024,
            Wvw = 2048,
        }

        public static async Task<Permissions> GetPermissionsAsync(string key)
        {
            Dictionary<string, object> data;
            try
            {
                data = (Dictionary<string, object>)Json.Decode((await Net.DownloadStringAsync(Net.URL + "v2/tokeninfo?access_token=" + key)).Data);
            }
            catch (System.Net.WebException e)
            {
                Api.Exceptions.Throw(e);
                throw;
            }

            var permissions = (List<object>)data["permissions"];
            var p = Permissions.None;

            for (var i = permissions.Count - 1; i >= 0; i--)
            {
                switch ((string)permissions[i])
                {
                    case "account":
                        p |= Permissions.Account;
                        break;
                    case "inventories":
                        p |= Permissions.Inventories;
                        break;
                    case "characters":
                        p |= Permissions.Characters;
                        break;
                    case "tradingpost":
                        p |= Permissions.Tradingpost;
                        break;
                    case "wallet":
                        p |= Permissions.Wallet;
                        break;
                    case "unlocks":
                        p |= Permissions.Unlocks;
                        break;
                    case "pvp":
                        p |= Permissions.Pvp;
                        break;
                    case "builds":
                        p |= Permissions.Builds;
                        break;
                    case "progression":
                        p |= Permissions.Progression;
                        break;
                    case "guilds":
                        p |= Permissions.Guilds;
                        break;
                    case "wvw":
                        p |= Permissions.Wvw;
                        break;
                    default:
                        p |= Permissions.Unknown;
                        break;
                }
            }

            return p;
        }
    }
}
