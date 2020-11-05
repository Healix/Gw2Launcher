using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Api
{
    static class TokenInfo
    {
        public enum Permissions
        {
            Unknown,
            Account,
            Inventories,
            Characters,
            Tradingpost,
            Wallet,
            Unlocks,
            Pvp,
            Builds,
            Progression,
            Guilds
        }

        public static async Task<Permissions[]> GetPermissionsAsync(string key)
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

            var result = new Permissions[permissions.Count];

            for (var i = result.Length - 1; i >= 0; i--)
            {
                switch ((string)permissions[i])
                {
                    case "account":
                        result[i] = Permissions.Account;
                        break;
                    case "inventories":
                        result[i] = Permissions.Inventories;
                        break;
                    case "characters":
                        result[i] = Permissions.Characters;
                        break;
                    case "tradingpost":
                        result[i] = Permissions.Tradingpost;
                        break;
                    case "wallet":
                        result[i] = Permissions.Wallet;
                        break;
                    case "unlocks":
                        result[i] = Permissions.Unlocks;
                        break;
                    case "pvp":
                        result[i] = Permissions.Pvp;
                        break;
                    case "builds":
                        result[i] = Permissions.Builds;
                        break;
                    case "progression":
                        result[i] = Permissions.Progression;
                        break;
                    case "guilds":
                        result[i] = Permissions.Guilds;
                        break;
                    default:
                        result[i] = Permissions.Unknown;
                        break;
                }
            }

            return result;
        }
    }
}
