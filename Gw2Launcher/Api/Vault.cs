using Gw2Launcher.Api.Cache;
using Gw2Launcher.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Api
{
    public class Vault
    {
        private class VaultData : DataCache.ICacheItem<AchievementCache.ItemID>
        {
            public AchievementCache.ItemID id;
            public Objective data;

            public AchievementCache.ItemID ID
            {
                get
                {
                    return id;
                }
                set
                {
                    this.id = value;
                }
            }

            public string Title
            {
                get
                {
                    if (data != null)
                    {
                        return data.Title;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                set
                {
                    if (data != null)
                    {
                        data.Title = value;
                    }
                }
            }

            public void ReadFrom(BinaryReader reader, uint length)
            {
                if (id.type == AchievementCache.CacheType.Vault)
                {
                    this.Title = reader.ReadString();
                }
            }

            public void WriteTo(BinaryWriter writer)
            {
                writer.Write(this.Title);
            }
        }

        public enum VaultType
        {
            Daily,
            Weekly,
            Special,
        }

        public enum ObjectiveType
        {
            Unknown,
            PvE,
            PvP,
            WvW,
        }

        public class Objective
        {
            public ushort ID
            {
                get;
                set;
            }

            public string Title
            {
                get;
                set;
            }

            public bool Claimed
            {
                get;
                set;
            }

            public ushort ProgressCurrent
            {
                get;
                set;
            }

            public ushort ProgressComplete
            {
                get;
                set;
            }

            public ObjectiveType Type
            {
                get;
                set;
            }
        }

        public VaultType Type
        {
            get;
            private set;
        }

        public byte ProgressCurrent
        {
            get;
            private set;
        }

        public byte ProgressComplete
        {
            get;
            private set;
        }

        public bool Claimed
        {
            get;
            private set;
        }

        public Objective[] Objectives
        {
            get;
            private set;
        }
        
        public static async Task<Vault> GetVaultAsync(VaultType type, string key)
        {
            return await GetVaultAsync(type, key, Settings.ShowDailiesLanguage.Value);
        }

        public static async Task<Vault> GetVaultAsync(VaultType type, string key, Settings.Language lang)
        {
            Dictionary<string, object> data;
            Net.ResponseData<string> response;

            try
            {
                string t;

                switch (type)
                {
                    case VaultType.Daily:

                        t = "daily";

                        break;
                    case VaultType.Weekly:

                        t = "weekly";

                        break;
                    case VaultType.Special:

                        t = "special";

                        break;
                    default:

                        throw new NotSupportedException(type + " is not supported");
                }

                string l;

                if (lang != Settings.Language.EN)
                {
                    l = "&lang=" + lang.ToLanguageCode();
                }
                else
                {
                    l = "";
                }

                response = await Net.DownloadStringAsync(Net.URL + "v2/account/wizardsvault/" + t + "?access_token=" + key + l);
                data = (Dictionary<string, object>)Json.Decode(response.Data);
            }
            catch (System.Net.WebException e)
            {
                Api.Exceptions.Throw(e);
                throw;
            }

            List<object> objectives;
            Dictionary<AchievementCache.ItemID, VaultData> dobjectives = null;

            if (!Json.GetValue<List<object>>(data, "objectives", out objectives))
            {
                throw new NotSupportedException("objectives not available");
            }

            var _objectives = new Objective[objectives.Count];

            if (lang != Settings.Language.EN)
            {
                dobjectives = new Dictionary<AchievementCache.ItemID, VaultData>(_objectives.Length);
            }

            var vault = new Vault()
            {
                Type = type,
                ProgressCurrent = (byte)Json.GetValue<int>(data, "meta_progress_current"),
                ProgressComplete = (byte)Json.GetValue<int>(data, "meta_progress_complete"),
                Claimed = Json.GetValue<bool>(data, "meta_reward_claimed"),
                Objectives = _objectives,
            };

            for (var i = 0; i < _objectives.Length; i++)
            {
                var o = (Dictionary<string, object>)objectives[i];
                ObjectiveType t;

                switch (Json.GetValue<string>(o, "track"))
                {
                    case "PvE":

                        t = ObjectiveType.PvE;

                        break;
                    case "PvP":

                        t = ObjectiveType.PvP;

                        break;
                    case "WvW":

                        t = ObjectiveType.WvW;

                        break;
                    default:

                        t = ObjectiveType.Unknown;

                        break;
                }

                _objectives[i] = new Objective()
                {
                    ID = (ushort)Json.GetValue<int>(o, "id"),
                    Title = Json.GetValue<string>(o, "title"),
                    Type = t,
                    ProgressCurrent = (ushort)Json.GetValue<int>(o, "progress_current"),
                    ProgressComplete = (ushort)Json.GetValue<int>(o, "progress_complete"),
                    Claimed = Json.GetValue<bool>(o, "claimed"),
                };

                if (lang != Settings.Language.EN)
                {
                    var id = new AchievementCache.ItemID(AchievementCache.CacheType.Vault, _objectives[i].ID);

                    dobjectives[id] = new VaultData()
                    {
                        id = id,
                        data = _objectives[i],
                    };
                }
            }

            if (lang != Settings.Language.EN)
            {
                await PopulateObjectives(dobjectives, lang);
            }

            return vault;
        }

        private static async Task<Net.ResponseData<string>> GetObjectivesAsync(ICollection<AchievementCache.ItemID> ids, Settings.Language lang)
        {
            try
            {
                var sb = new StringBuilder((ids != null ? ids.Count * 5 : 3) + (lang != Settings.Language.EN ? 8 : 0) + 4);

                if (lang != Settings.Language.EN)
                {
                    sb.Append("lang=");
                    sb.Append(lang.ToLanguageCode());
                    sb.Append('&');
                }

                sb.Append("ids=");

                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        sb.Append(id.id);
                        sb.Append(',');
                    }
                    sb.Length--;
                }
                else
                {
                    sb.Append("all");
                }

                return await Net.DownloadStringAsync(Net.URL + "v2/wizardsvault/objectives?" + sb.ToString());
            }
            catch (System.Net.WebException e)
            {
                Api.Exceptions.Throw(e);
                throw;
            }
        }

        private static VaultData[] ParseObjectives(Net.ResponseData<string> r, Dictionary<AchievementCache.ItemID, VaultData> objectives)
        {
            var data = (List<object>)Json.Decode(r.Data);
            var items = new VaultData[data.Count];
            var index = 0;

            foreach (Dictionary<string, object> d in data)
            {
                var id = new AchievementCache.ItemID(AchievementCache.CacheType.Vault, Json.GetValue<int>(d, "id"));

                VaultData v;
                if (objectives.TryGetValue(id, out v))
                {
                    v.Title = Json.GetString(d, "title");

                    items[index++] = v;
                }
                else
                {
                    Util.Logging.LogEvent("Unknown objective " + id);
                }
            }

            return items;
        }

        private static async Task PopulateObjectives(Dictionary<AchievementCache.ItemID, VaultData> objectives, Settings.Language lang)
        {
            var ids = new HashSet<AchievementCache.ItemID>(objectives.Keys);

            try
            {
                await AchievementCache.Storage.ReadAsync<VaultData>(ids, objectives, null);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (ids.Count > 0)
            {
                try
                {
                    var r = await GetObjectivesAsync(ids, lang);
                    var added = await Task.Run<VaultData[]>(new Func<VaultData[]>(
                        delegate
                        {
                            return ParseObjectives(r, objectives);
                        }));

                    try
                    {
                        if (added.Length > 0)
                        {
                            await AchievementCache.Storage.WriteAsync<VaultData>(added);
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
                catch (Exceptions.IdsInvalidException)
                { }
            }
        }
    }
}
