using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Gw2Launcher.Tools;

namespace Gw2Launcher.Api
{
    class DailyAchievements
    {
        private const int HEADER = 1751335244;
        private const ushort VERSION = 1;
        private const long TICKS_PER_DAY = 864000000000;
        private const string FILE_NAME = "achievements.dat";

        private enum CacheType : byte
        {
            Unknown = 0,
            Achievement = 1,
            Icon = 2
        }

        private class ItemID : IEquatable<ItemID>
        {
            public CacheType type;
            public uint id;

            public ItemID(CacheType type, uint id)
            {
                this.type = type;
                this.id = id;
            }

            public override int GetHashCode()
            {
                return (int)id + ((int)type << 24);
            }

            public override bool Equals(object obj)
            {
                if (obj is ItemID)
                    return this.Equals((ItemID)obj);
                return base.Equals(obj);
            }

            public override string ToString()
            {
                return type.ToString() + ":" + id.ToString();
            }

            public bool Equals(ItemID other)
            {
                if (id == other.id)
                    return type == other.type;
                return false;
            }
        }

        public class Daily
        {
            [Flags]
            public enum AccessCondition
            {
                None = 0,

                HasAccess = 1,

                HeartOfThorns = 2,
                PathOfFire = 4,
                EndOfDragons = 8,

                Unknown = 16, //placeholder for future expansions
            }

            public uint ID
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public string Requirement
            {
                get;
                set;
            }

            public int MinLevel
            {
                get;
                set;
            }

            public int MaxLevel
            {
                get;
                set;
            }

            public System.Drawing.Image Icon
            {
                get;
                set;
            }

            public AccessCondition Access
            {
                get;
                set;
            }

            public string UnknownName
            {
                get;
                set;
            }
        }

        public class Category
        {
            public string Name
            {
                get;
                set;
            }

            public Daily[] Dailies
            {
                get;
                set;
            }
        }

        public class Dailies : IDisposable
        {
            public DateTime Date
            {
                get;
                set;
            }

            public Category[] Categories
            {
                get;
                set;
            }

            public System.Drawing.Image[] Icons
            {
                get;
                set;
            }

            public int Count
            {
                get;
                set;
            }

            public int LowlevelCount
            {
                get;
                set;
            }

            public void Dispose()
            {
                foreach (var icon in Icons)
                {
                    icon.Dispose();
                }
            }
        }

        private class Icon : DataCache.ICacheItem<ItemID>
        {
            public ItemID id;
            public byte[] bytes;
            public string url;

            private System.Drawing.Image image;
            public System.Drawing.Image GetImage()
            {
                if (image != null)
                    return image;
                if (bytes == null)
                    return null;

                try
                {
                    using (var stream = new MemoryStream(bytes, false))
                    {
                        return image = System.Drawing.Bitmap.FromStream(stream);
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                bytes = null;
                return null;
            }

            public ItemID ID
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

            public void ReadFrom(BinaryReader reader, uint length)
            {
                var hasData = reader.ReadBoolean();
                if (hasData)
                {
                    bytes = reader.ReadBytes((int)length - 1);
                }
                else
                {
                    url = reader.ReadString();
                }
            }

            public void WriteTo(BinaryWriter writer)
            {
                if (bytes != null)
                {
                    writer.Write(true);
                    writer.Write(bytes);
                }
                else
                {
                    writer.Write(false);
                    writer.Write(url);
                }
            }
        }

        private class Achievement : DataCache.ICacheItem<ItemID>
        {
            public ItemID id;
            public string
                name,
                description,
                requirement;
            public Icon icon;
            public uint iconId;

            public ItemID ID
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

            public void ReadFrom(BinaryReader reader, uint length)
            {
                name = reader.ReadString();
                description = reader.ReadString();
                requirement = reader.ReadString();
                iconId = reader.ReadUInt32();
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteString(writer, name);
                WriteString(writer, description);
                WriteString(writer, requirement);
                writer.Write(iconId);
            }

            private void WriteString(BinaryWriter writer, string s)
            {
                if (s == null)
                    writer.Write(string.Empty);
                else
                    writer.Write(s);
            }
        }

        private class FormatCacheID : DataCache.IFormat<ItemID>
        {
            public ItemID Read(BinaryReader reader)
            {
                var type = (CacheType)reader.ReadByte();
                uint id;

                switch (type)
                {
                    case CacheType.Achievement:
                        id = reader.ReadUInt16();
                        break;
                    case CacheType.Icon:
                        id = reader.ReadUInt32();
                        break;
                    default:
                        throw new IOException("Invalid ID");
                }

                return new ItemID(type, id);
            }

            public void Write(BinaryWriter writer, ItemID value)
            {
                writer.Write((byte)value.type);
                switch (value.type)
                {
                    case CacheType.Achievement:
                        writer.Write((ushort)value.id);
                        break;
                    case CacheType.Icon:
                        writer.Write(value.id);
                        break;
                    default:
                        throw new IOException();
                }
            }

            public IEqualityComparer<ItemID> Comparer
            {
                get 
                {
                    return null;
                }
            }
        }

        private Dailies today, tomorrow;
        private DateTime nextUpdate;

        private DataCache<ItemID> cache;
        private bool clearCache;

        public DailyAchievements()
        {
            cache = new DataCache<ItemID>(HEADER, VERSION, Path.Combine(DataPath.AppData, FILE_NAME), new FormatCacheID());
        }

        public Settings.Language Language
        {
            get
            {
                return Settings.ShowDailiesLanguage.Value;
            }
        }

        public async Task<Dailies> GetToday(Action onDownloadBegin)
        {
            var now = DateTime.UtcNow;

            if (today != null)
            {
                if (today.Date.Day == now.Day || now < today.Date && today.Date.Subtract(now).TotalMinutes < 10)
                {
                    return today;
                }
                if (tomorrow != null && tomorrow.Date.Day == now.Day)
                {
                    today = tomorrow;
                    tomorrow = null;
                    return today;
                }
            }

            if (now < nextUpdate)
                return today;

            if (onDownloadBegin != null)
                onDownloadBegin();

            var d = await TryGetDailies(Api.Net.URL + "v2/achievements/daily?v=latest");

            if (d != null)
            {
                if (today != null && Equals(d, today))
                {
                    //the daily hasn't changed

                    var t = d.Date.Ticks;
                    var minutesAfter = (t - (t / TICKS_PER_DAY * TICKS_PER_DAY)) / 600000000;

                    if (minutesAfter < 15)
                    {
                        //less than 15 minutes after the daily, allow rechecking
                        nextUpdate = DateTime.UtcNow.AddMinutes(1);

                        d.Dispose();
                        return null;
                    }
                }
            }

            today = d;

            return d;
        }

        public async Task<Dailies> GetTomorrow(Action onDownloadBegin)
        {
            var now = DateTime.UtcNow;

            if (tomorrow != null)
            {
                var next = now.AddDays(1);
                if (tomorrow.Date.Day == next.Day || next < tomorrow.Date && tomorrow.Date.Subtract(next).TotalMinutes < 10)
                {
                    return tomorrow;
                }
            }

            if (now < nextUpdate)
                return tomorrow;

            if (onDownloadBegin != null)
                onDownloadBegin();

            var d = await TryGetDailies(Api.Net.URL + "v2/achievements/daily/tomorrow?v=latest");

            if (d != null)
            {
                bool b;
                if (b = (today != null && Equals(d, today)) || tomorrow != null && Equals(d, tomorrow))
                {
                    //the daily (tomorrow) hasn't changed

                    var t = d.Date.Ticks;
                    var minutesAfter = (t - (t / TICKS_PER_DAY * TICKS_PER_DAY)) / 600000000;

                    if (minutesAfter < 15)
                    {
                        //less than 15 minutes after the daily, allow rechecking
                        nextUpdate = DateTime.UtcNow.AddMinutes(1);

                        d.Dispose();
                        return null;
                    }
                }

                d.Date = d.Date.AddDays(1);
            }

            tomorrow = d;

            return d;
        }

        public async Task Reset(bool clearCache)
        {
            nextUpdate = DateTime.MinValue;
            if (today != null)
                today.Date = DateTime.MinValue;
            if (tomorrow != null)
                tomorrow.Date = DateTime.MinValue;
            if (clearCache)
                await cache.ClearAsync();
        }

        private bool Equals(Dailies a, Dailies b)
        {
            if (a.Categories.Length != b.Categories.Length)
                return false;

            for (int i = 0, l = a.Categories.Length; i < l; i++)
            {
                var da = a.Categories[i].Dailies;
                var db = b.Categories[i].Dailies;

                if (da.Length != db.Length)
                    return false;

                for (int j = 0, dl = da.Length; j < dl; j++)
                {
                    if (da[j].ID != db[j].ID)
                        return false;
                }
            }

            return true;
        }

        private async Task<Dailies> TryGetDailies(string url)
        {
            try
            {
                return await GetDailiesAsync(url);
            }
            catch (System.Net.WebException e)
            {
                using (e.Response) { }
                Util.Logging.Log(e);
                nextUpdate = DateTime.UtcNow.AddMinutes(1);
            }
            catch (Exception e)
            {
                //decoding error
                Util.Logging.Log(e);
                nextUpdate = DateTime.UtcNow.AddMinutes(1);
            }

            return null;
        }

        private async Task<Dailies> GetDailiesAsync(string url)
        {
            var response = await Api.Net.DownloadStringAsync(url);
            var date = response.Date;
            var now = DateTime.UtcNow;

            var dailies = new Dailies()
            {
                Date = now
            };

            if (date != DateTime.MinValue)
            {
                if (date.Day != now.Day)
                {
                    var offset = now.Subtract(date).TotalMinutes;
                    if (offset > 0)
                    {
                        //server is behind

                        var secondsToNextDay = (int)(((date.Ticks / TICKS_PER_DAY + 1) * TICKS_PER_DAY - date.Ticks) / 10000) / 1000;
                        if (secondsToNextDay < 300)
                        {
                            nextUpdate = now.AddSeconds(secondsToNextDay + 1);
                            dailies.Date = date;
                        }
                        else
                        {
                            //there's more than 5 minutes until the next day
                            // -- can't trust the local or server clock.
                            nextUpdate = now.AddMinutes(5);
                            dailies.Date = date;
                        }
                    }
                    else
                    {
                        //server is ahead

                        if (offset > -10)
                        {
                            dailies.Date = date;
                        }
                        else
                        {
                            //server is more than 10 minutes ahead
                            // -- local clock must be wrong
                        }
                    }
                }
                else
                    dailies.Date = date;
            }

            var ids = ParseDailies(response.Data, dailies);

            var achievements = new Dictionary<ItemID, Achievement>(ids.Count);
            var icons = new Dictionary<ItemID, Icon>();

            if (clearCache)
            {
                clearCache = false;
                await cache.ClearAsync();
            }

            await PopulateAchievements(ids, achievements, icons, this.Language);

            var images = new System.Drawing.Image[icons.Count];
            int i = 0;
            foreach (var icon in icons.Values)
            {
                var image = icon.GetImage();
                if (image == null)
                    continue;
                images[i++] = image;
            }
            if (i != images.Length)
            {
                var _images = new System.Drawing.Image[i];
                if (i > 0)
                    Array.Copy(images, _images, i);
                images = _images;
            }
            dailies.Icons = images;

            int count = 0,
                cii = 0,
                lowcount = 0;

            for (int ci = 0, cl = dailies.Categories.Length; ci < cl; ci++)
            {
                var c = dailies.Categories[ci];
                var items = c.Dailies;
                i = 0;

                for (int j = 0, l = items.Length; j < l; j++)
                {
                    var d = items[j];
                    Achievement a;
                    if (achievements.TryGetValue(new ItemID(CacheType.Achievement, d.ID), out a))
                    {
                        d.Name = a.name;
                        d.Description = a.description;
                        d.Requirement = a.requirement;

                        if (a.icon != null || a.iconId != 0 && icons.TryGetValue(new ItemID(CacheType.Icon, a.iconId), out a.icon) && a.icon != null)
                            d.Icon = a.icon.GetImage();

                        if (i != j)
                            items[i] = d;
                        i++;

                        if (d.MaxLevel < 80)
                            lowcount++;
                    }
                    else
                    {
                        //daily has no data
                    }
                }

                if (i > 0)
                {
                    if (i != items.Length)
                    {
                        c.Dailies = new Daily[i];
                        if (i > 0)
                            Array.Copy(items, c.Dailies, i);
                    }

                    count += i;

                    if (cii != ci)
                        dailies.Categories[cii] = c;
                    cii++;
                }
            }

            if (cii != dailies.Categories.Length)
            {
                var c = dailies.Categories;
                dailies.Categories = new Category[cii];
                if (cii > 0)
                    Array.Copy(c, dailies.Categories, cii);
            }

            dailies.Count = count;
            dailies.LowlevelCount = lowcount;

            return dailies;
        }

        private Daily.AccessCondition ParseAccessCondition(string product, string condition)
        {
            var c = Daily.AccessCondition.None;

            switch (product)
            {
                case "HeartOfThorns":
                    c = Daily.AccessCondition.HeartOfThorns;
                    break;
                case "PathOfFire":
                    c = Daily.AccessCondition.PathOfFire;
                    break;
                case "EndOfDragons":
                    c = Daily.AccessCondition.EndOfDragons;
                    break;
                default:
                    c = Daily.AccessCondition.Unknown;
                    Util.Logging.Log("Unknown product: " + product);
                    break;
            }

            switch (condition)
            {
                case "HasAccess":
                    c |= Daily.AccessCondition.HasAccess;
                    break;
                case "NoAccess":
                    break;
                default:
                    throw new NotSupportedException("Unknown condition \"" + condition + "\"");
            }

            return c;
        }

        /// <summary>
        /// Parses dailies from json data
        /// </summary>
        /// <param name="json">Dailies json data</param>
        /// <param name="output">Output for the parsed dailies</param>
        /// <returns>A set of achievement IDs from the parsed dailies</returns>
        private HashSet<ItemID> ParseDailies(string json, Dailies output)
        {
            var data = Api.Json.Decode(json) as Dictionary<string, object>;
            if (data == null)
                throw new IOException();

            output.Categories = new Category[data.Count];

            var ids = new HashSet<ItemID>();
            var ci = 0;

            foreach (var key in data.Keys)
            {
                var cdata = (List<object>)data[key];

                Category c;
                Daily[] dailies;

                output.Categories[ci++] = c = new Category()
                {
                    Name = GetCategoryName(key),
                    Dailies = dailies = new Daily[cdata.Count]
                };

                for (int di = 0, dl = cdata.Count; di < dl; di++)
                {
                    var ddata = (Dictionary<string, object>)cdata[di];
                    var ldata = (Dictionary<string, object>)ddata["level"];
                    Daily d;

                    dailies[di] = d = new Daily()
                    {
                        ID = (uint)(int)ddata["id"],
                        MinLevel = (int)ldata["min"],
                        MaxLevel = (int)ldata["max"],
                    };

                    try
                    {
                        object o;
                        if (ddata.TryGetValue("required_access", out o))
                        {
                            if (o is Dictionary<string, object>)
                            {
                                var rdata = (Dictionary<string, object>)o;
                                var product = (string)rdata["product"];

                                d.Access = ParseAccessCondition(product, (string)rdata["condition"]);

                                if ((d.Access & Daily.AccessCondition.Unknown) != 0)
                                {
                                    d.UnknownName = CreateUnknownName(product);
                                }
                            }
                            else if (o is List<object>)
                            {
                                throw new Exception("Unknown required_access (array) format");
                            }
                            else
                            {
                                throw new Exception("Unknown required_access format");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }

                    ids.Add(new ItemID(CacheType.Achievement, d.ID));
                }
            }

            return ids;
        }

        private string CreateUnknownName(string n)
        {
            if (string.IsNullOrEmpty(n))
                return null;

            var l = n.Length;
            var chars = new char[l];
            var count = 0;

            for (var i = 0; i < l; i++)
            {
                if (char.IsUpper(n, i))
                {
                    chars[count++] = n[i];
                }
            }

            if (count <= 1)
                return n.Substring(0, l > 3 ? 3 : 0);
            return new string(chars, 0, count < 6 ? count : 6);
        }

        private async Task PopulateAchievements(HashSet<ItemID> achievementIds, Dictionary<ItemID, Achievement> achievements, Dictionary<ItemID, Icon> icons, Settings.Language l)
        {
            try
            {
                await cache.ReadAsync<Achievement>(achievementIds, achievements,
                    delegate(Achievement a)
                    {
                        if (a.iconId == 0)
                            return;

                        var id = new ItemID(CacheType.Icon, a.iconId);
                        if (!icons.TryGetValue(id, out a.icon))
                        {
                            icons[id] = a.icon = new Icon()
                            {
                                id = id
                            };
                        }
                    });
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (achievementIds.Count == 0)
            {
                await PopulateIcons(icons);
                return;
            }

            var sb = new StringBuilder(achievementIds.Count * 5 + 30);
            sb.Append("v2/achievements?ids=");

            foreach (var id in achievementIds)
            {
                sb.Append(id.id);
                sb.Append(',');
            }

            sb.Length--;

            if (l != Settings.Language.EN)
            {
                sb.Append("&lang=");
                sb.Append(Settings.GetLanguageCode(l));
            }

            Achievement[] achievs;

            try
            {
                var response = await Api.Net.DownloadStringAsync(Api.Net.URL + sb.ToString());
                achievs = await ParseAchievements(response.Data, achievements, icons);
            }
            catch (System.Net.WebException e)
            {
                var fail = true;
                using (var response = e.Response as System.Net.HttpWebResponse)
                {
                    if (response != null)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            using (var r = new StreamReader(response.GetResponseStream()))
                            {
                                var text = r.ReadToEnd();
                                if (text.IndexOf("all ids provided are invalid") != -1)
                                {
                                    //none of the requested achievements had data
                                    fail = false;
                                }
                            }
                        }
                    }
                }

                if (fail)
                    throw;
                else
                    achievs = null;
            }

            try
            {
                if (achievs != null && achievs.Length > 0)
                    await cache.WriteAsync<Achievement>(achievs);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        private async Task PopulateIcons(Dictionary<ItemID, Icon> icons)
        {
            var ids = new HashSet<ItemID>(icons.Keys);

            try
            {
                await cache.ReadAsync<Icon>(ids, icons,
                    delegate(Icon icon)
                    {
                        if (icon.bytes == null && icon.url != null)
                            ids.Add(icon.id);
                    });
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (ids.Count == 0)
                return;

            var l = new List<Icon>(ids.Count);
            HashSet<ItemID> invalid = null;

            foreach (var id in ids)
            {
                var icon = icons[id];
                try
                {
                    if (icon.url == null)
                        continue;
                    var response = await Api.Net.DownloadBytesAsync(icon.url);
                    icon.bytes = response.Data;
                    l.Add(icon);
                }
                catch (System.Net.WebException e)
                {
                    Util.Logging.Log(e);

                    using (var response = e.Response as System.Net.HttpWebResponse)
                    {
                        if (response != null)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                using (var r = new StreamReader(response.GetResponseStream()))
                                {
                                    var text = r.ReadToEnd();
                                    if (text.IndexOf("invalid signature") != -1)
                                    {
                                        //icon doesn't exist
                                        if (invalid == null)
                                            invalid = new HashSet<ItemID>();
                                        invalid.Add(icon.id);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            if (invalid != null)
                await cache.DeleteAsync(invalid);

            if (l.Count > 0)
                await cache.WriteAsync<Icon>(l);
        }

        /// <summary>
        /// Parses achievements from json data
        /// </summary>
        /// <param name="json">Achievements json data</param>
        /// <param name="achievements">Output for the parsed achievements</param>
        /// <param name="icons">Output for icon data</param>
        /// <returns>An array of parsed achievements</returns>
        private async Task<Achievement[]> ParseAchievements(string json, Dictionary<ItemID, Achievement> achievements, Dictionary<ItemID, Icon> icons)
        {
            var data = Api.Json.Decode(json) as List<object>;
            if (data == null)
                throw new IOException();

            var achievs = new Achievement[data.Count];
            var i = 0;

            foreach (var o in data)
            {
                var adata = (Dictionary<string, object>)o;

                //some strings contain double spaces
                var a = new Achievement()
                {
                    id = new ItemID(CacheType.Achievement, (uint)(int)adata["id"]),
                    name = ((string)adata["name"]).Replace("  ", " "),
                    description = ((string)adata["description"]).Replace("  ", " "),
                    requirement = ((string)adata["requirement"]).Replace("  ", " "),
                };

                var iconUrl = Api.Json.GetValue<string>(adata, "icon");
                if ((a.iconId = GetIconIDFromUrl(iconUrl)) > 0)
                {
                    var id = new ItemID(CacheType.Icon, a.iconId);
                    if (!icons.TryGetValue(id, out a.icon))
                    {
                        Icon icon;
                        icons[id] = icon = new Icon()
                        {
                            id = id,
                            url = iconUrl,
                        };
                    }
                }

                achievements[a.id] = a;
                achievs[i++] = a;
            }

            await PopulateIcons(icons);

            return achievs;
        }

        private uint GetIconIDFromUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var j = url.LastIndexOf('/');
                if (j != -1)
                {
                    j++;
                    var k = url.IndexOf('.', j);
                    if (k != -1)
                    {
                        uint iconid;
                        if (uint.TryParse(url.Substring(j, k - j), out iconid))
                        {
                            return iconid;
                        }
                    }
                }
            }

            return 0;
        }

        private string GetCategoryName(string key)
        {
            switch (key)
            {
                case "pve":
                    return "PvE";
                case "wvw":
                    return "WvW";
                case "pvp":
                    return "PvP";
            }

            if (key.Length > 1)
                return char.ToUpper(key[0]) + key.Substring(1);

            return key;
        }
    }
}
