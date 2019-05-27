using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Api
{
    class Account
    {
        public const ushort MAX_AP = 15000;

        private Account()
        {

        }

        public string Name
        {
            get;
            set;
        }

        public int World
        {
            get;
            set;
        }

        public int Age
        {
            get;
            set;
        }

        public DateTime Created
        {
            get;
            set;
        }

        public string[] Access
        {
            get;
            set;
        }

        public int DailyAP
        {
            get;
            set;
        }

        public int MonthlyAP
        {
            get;
            set;
        }

        public DateTime LastModified
        {
            get;
            set;
        }

        public static async Task<Account> GetAccountAsync(string key)
        {
            Dictionary<string, object> data;
            Net.ResponseData<string> response;

            try
            {
                response = await Net.DownloadStringAsync(Net.URL + "v2/account?access_token=" + key);
                data = (Dictionary<string, object>)Json.Decode(response.Data);
            }
            catch (System.Net.WebException e)
            {
                Api.Exceptions.Throw(e);
                throw;
            }

            object o;
            int dap, map;

            if (data.TryGetValue("daily_ap", out o))
            {
                dap = (int)o;
                map = (int)data["monthly_ap"];
            }
            else
            {
                dap = map = -1;
            }

            var _access = (List<object>)data["access"];
            var access = new string[_access.Count];

            for (var i = _access.Count - 1; i >= 0; i--)
            {
                access[i] = (string)_access[i];
            }

            var a = new Account()
            {
                Name = (string)data["name"],
                Age = (int)data["age"],
                World = (int)data["world"],
                Created = DateTime.Parse((string)data["created"], null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                DailyAP = dap,
                MonthlyAP = map,
                Access = access,
                LastModified = response.LastModifiedAdjusted,
            };

            return a;
        }
    }
}
