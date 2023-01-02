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

            string name;
            int dap, map;
            int age, world;

            if (!Json.GetValue<int>(data, "daily_ap", out dap))
                dap = -1;
            if (!Json.GetValue<int>(data, "monthly_ap", out map))
                map = -1;
            if (!Json.GetValue<int>(data, "age", out age))
                age = 0;
            if (!Json.GetValue<int>(data, "world", out world))
                world = 0;

            var _access = Json.GetValue<List<object>>(data, "access");
            string[] access;

            if (_access == null)
            {
                access = new string[0];
            }
            else
            {
                access = new string[_access.Count];
                for (var i = _access.Count - 1; i >= 0; i--)
                {
                    access[i] = (string)_access[i];
                }
            }

            if (!Json.GetValue<string>(data, "name", out name))
                name = "";

            var a = new Account()
            {
                Name = name,
                Age = age,
                World = world,
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
