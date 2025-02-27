using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Api
{
    public class Wallet
    {
        private int[] values;

        private Wallet(int[] values)
        {
            this.values = values;
        }

        public int GetValue(int id)
        {
            if (id > values.Length)
            {
                return 0;
            }

            return values[id - 1];
        }

        /// <summary>
        /// ID 1
        /// </summary>
        public int Coins
        {
            get
            {
                return GetValue(1);
            }
        }

        /// <summary>
        /// ID 2
        /// </summary>
        public int Karma
        {
            get
            {
                return GetValue(2);
            }
        }

        /// <summary>
        /// ID 3
        /// </summary>
        public int Laurels
        {
            get
            {
                return GetValue(3);
            }
        }

        /// <summary>
        /// ID 63
        /// </summary>
        public int Astral
        {
            get
            {
                return GetValue(63);
            }
        }

        public static async Task<Wallet> GetWalletAsync(string key)
        {
            List<object> data;
            Net.ResponseData<string> response;

            try
            {
                response = await Net.DownloadStringAsync(Net.URL + "v2/account/wallet?access_token=" + key);
                data = (List<object>)Json.Decode(response.Data);
            }
            catch (System.Net.WebException e)
            {
                Api.Exceptions.Throw(e);
                throw;
            }

            int last;

            if (!Json.GetValue<int>((Dictionary<string, object>)data[data.Count - 1], "id", out last))
            {
                throw new NotSupportedException("id not available");
            }

            //using the id as an index; ids start from 1, so subtract 1

            var values = new int[last];

            for (var i = data.Count - 1; i >= 0; --i)
            {
                var json = (Dictionary<string, object>)data[i];
                var id = Json.GetValue<int>(json, "id");

                if (id > last)
                {
                    Array.Resize<int>(ref values, last = id);
                }

                values[id - 1] = Json.GetValue<int>(json, "value");
            }

            return new Wallet(values);
        }
    }
}
