using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Gw2Launcher.Tools;
using Gw2Launcher.Api.Cache;

namespace Gw2Launcher.Api
{
    public class Daily
    {
        private class IconData : DataCache.ICacheItem<AchievementCache.ItemID>, IImage
        {
            public AchievementCache.ItemID id;
            public byte[] bytes;
            public string url;

            public bool Empty
            {
                get
                {
                    return url == null && bytes == null;
                }
            }

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

            public int GetID()
            {
                return id.id;
            }

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

            public void Dispose()
            {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                bytes = null;
            }
        }

        private class AchievementData : DataCache.ICacheItem<AchievementCache.ItemID>
        {
            public AchievementCache.ItemID id;
            public Achievement data;
            public int icon;

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

            public void ReadFrom(BinaryReader reader, uint length)
            {
                if (data == null)
                {
                    data = new Achievement();
                }

                if (id.type == AchievementCache.CacheType.Achievement)
                {
                    data.Name = reader.ReadString();
                    data.Requirement = reader.ReadString();
                    icon = reader.ReadInt32();
                }
            }

            public void WriteTo(BinaryWriter writer)
            {
                WriteString(writer, data.Name);
                WriteString(writer, data.Requirement);
                writer.Write(icon);
            }

            private void WriteString(BinaryWriter writer, string s)
            {
                if (s == null)
                    writer.Write(string.Empty);
                else
                    writer.Write(s);
            }
        }

        public interface IImage : IDisposable
        {
            System.Drawing.Image GetImage();
            int GetID();
            bool Empty
            {
                get;
            }
        }

        public interface IAchievement
        {
            string Name
            {
                get;
            }

            string Requirement
            {
                get;
            }

            int Icon
            {
                get;
            }
        }

        public class Category : IComparable<Category>, IEquatable<Category>
        {
            public ushort Index
            {
                get;
                set;
            }

            public ushort ID
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public IImage Icon
            {
                get;
                set;
            }

            public System.Drawing.Image GetIcon()
            {
                if (Icon != null)
                {
                    return Icon.GetImage();
                }
                return null;
            }

            public Achievement[] Today
            {
                get;
                set;
            }

            public Achievement[] Tomorrow
            {
                get;
                set;
            }

            public int CompareTo(Category a)
            {
                if (this.ID == a.ID)
                {
                    return 0;
                }
                else if (a != null)
                {
                    return string.Compare(this.Name, a.Name, StringComparison.Ordinal);
                }

                return 1;
            }

            public bool Equals(Category c)
            {
                return this.ID == c.ID && Equals(this.Today, c.Today) && Equals(this.Tomorrow, c.Tomorrow);
            }

            public static bool Equals(Achievement[] a, Achievement[] b)
            {
                if (a != null && b != null && a.Length == b.Length)
                {
                    int sum1 = 0, sum2 = 0;

                    for (var i = 0; i < a.Length; i++)
                    {
                        sum1 += a[i].ID;
                        sum2 += b[i].ID;

                        //if (a[i].ID != b[i].ID)
                        //{
                        //    //check if they're the same, but in a different order (shouldn't be needed; IDs are in order)

                        //    var k = false;

                        //    for (var j = 0; j < b.Length; j++)
                        //    {
                        //        if (a[i].ID == b[j].ID)
                        //        {
                        //            k = true;
                        //            break;
                        //        }
                        //    }

                        //    if (!k)
                        //    {
                        //        return false;
                        //    }
                        //}
                    }

                    return sum1 == sum2;
                }

                return a == b;
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        public class Achievement : IComparable<Achievement>
        {
            public ushort ID
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public IImage Icon
            {
                get;
                set;
            }

            /// <summary>
            /// API data was not available
            /// </summary>
            public bool IsUnknown
            {
                get
                {
                    return Name == null;
                }
            }

            public System.Drawing.Image GetIcon()
            {
                if (Icon != null)
                {
                    return Icon.GetImage();
                }
                return null;
            }

            public string Requirement
            {
                get;
                set;
            }

            public int CompareTo(Achievement a)
            {
                if (this.ID == a.ID)
                {
                    return 0;
                }
                else if (a != null)
                {
                    return string.Compare(this.Name, a.Name, StringComparison.Ordinal);
                }

                return 1;
            }

            public override string ToString()
            {
                return this.Name;
            }
        }
                
        public class Achievements : IDisposable
        {
            public enum GroupType : byte
            {
                Today,
                Tomorrow
            }

            private AchievementsGroup[] groups;
            private Category[] categories;
            private IImage[] icons;

            public Achievements()
            {
                groups = new AchievementsGroup[2];
            }

            public Achievements(Category[] categories, AchievementsGroup[] groups, IImage[] icons)
            {
                this.categories=categories;
                this.groups=groups;
                this.icons=icons;
            }

            public AchievementsGroup this[GroupType type]
            {
                get
                {
                    return groups[(byte)type];
                }
                set
                {
                    groups[(byte)type] = value;
                }
            }

            public AchievementsGroup[] Groups
            {
                get
                {
                    return groups;
                }
            }

            public AchievementsGroup Today
            {
                get
                {
                    return groups[0];
                }
            }

            public AchievementsGroup Tomorrow
            {
                get
                {
                    return groups[1];
                }
            }

            public Category[] Categories
            {
                get
                {
                    return categories;
                }
            }

            public AchievementsGroup GetGroup(GroupType type)
            {
                return groups[(byte)type];
            }

            public Category GetCategory(ushort id)
            {
                for (var i = 0; i < categories.Length; i++)
                {
                    if (categories[i].ID == id)
                    {
                        return categories[i];
                    }
                }
                return null;
            }

            public Achievement[] GetAchievements(GroupType type, ushort category)
            {
                var g = GetGroup(type);

                if (g != null)
                {
                    return g.Categories[category].Achievements;
                }

                return null;
            }

            /// <summary>
            /// Date requested
            /// </summary>
            public DateTime Date
            {
                get;
                set;
            }

            /// <summary>
            /// Number of days since the dailies started
            /// </summary>
            public int Age
            {
                get
                {
                    return DateTime.UtcNow.Subtract(Date.Date).Days;
                }
            }

            /// <summary>
            /// Summary of achievement IDs for comparison
            /// </summary>
            public int Summary
            {
                get;
                set;
            }

            /// <summary>
            /// Total number of unique achievements
            /// </summary>
            public int Count
            {
                get;
                set;
            }

            /// <summary>
            /// Data can be considered accurate
            /// </summary>
            public bool Verified
            {
                get;
                set;
            }

            public void Dispose()
            {
                if (icons != null)
                {
                    for (var i = 0; i < icons.Length; i++)
                    {
                        if (icons[i] != null)
                        {
                            icons[i].Dispose();
                        }
                    }
                }
            }
        }

        public class CategoryAchievements : IEquatable<CategoryAchievements>
        {
            public CategoryAchievements(Category category, Achievement[] achievements)
            {
                this.Category = category;
                this.Achievements = achievements;
            }

            public Category Category
            {
                get;
                private set;
            }

            public Achievement[] Achievements
            {
                get;
                private set;
            }

            public int Count
            {
                get
                {
                    if (Achievements == null)
                        return 0;
                    return Achievements.Length;
                }
            }

            public bool Equals(CategoryAchievements c)
            {
                var a = this.Achievements;
                var b = c.Achievements;

                if (a != null && b != null && a.Length == b.Length)
                {
                    var sum1 = 0;
                    var sum2 = 0;

                    for (var i = 0; i < a.Length; i++)
                    {
                        sum1 += a[i].ID;
                        sum2 += b[i].ID;

                    }

                    return sum1 == sum2;
                }

                return a == b;
            }
        }

        public class AchievementsGroup : IEquatable<AchievementsGroup>
        {
            private CategoryAchievements[] categories;

            public AchievementsGroup(CategoryAchievements[] categories)
            {
                this.categories = categories;
            }

            public CategoryAchievements[] Categories
            {
                get
                {
                    return categories;
                }
            }

            /// <summary>
            /// Summary of achievement IDs for comparison
            /// </summary>
            public int Summary
            {
                get;
                set;
            }

            public bool Equals(AchievementsGroup g)
            {
                if (this.Summary == g.Summary && categories.Length == g.categories.Length)
                {
                    for (var i = 0; i < categories.Length; i++)
                    {
                        if (!categories[i].Equals(g.categories[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        public class DailyNotModifiedException : Exception
        {
            public DateTime Date
            {
                get;
                set;
            }
        }

        private Achievements cache;

        public Daily()
        {
        }

        public bool IsCached
        {
            get
            {
                if (cache != null)
                {
                    var now = DateTime.UtcNow;

                    if (cache.Date.Date == now.Date || now < cache.Date && cache.Date.Subtract(now).TotalMinutes < 10)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static int GetIconIDFromUrl(string url)
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
                        int iconid;
                        if (int.TryParse(url.Substring(j, k - j), out iconid))
                        {
                            return iconid;
                        }
                    }
                }
            }

            return 0;
        }

        public static ushort[] GetDefaultCategories()
        {
            return new ushort[] { 238, 243, 330, 321, 88, 250 };
        }

        private Settings.Language Language
        {
            get
            {
                return Settings.ShowDailiesLanguage.Value;
            }
        }

        public Achievements Current
        {
            get
            {
                return cache;
            }
        }

        public async Task Reset(bool clear)
        {
            if (cache != null)
            {
                if (clear)
                {
                    cache.Dispose();
                    cache = null;
                }
            }

            if (clear)
            {
                await AchievementCache.Storage.ClearAsync();
            }
        }

        public async Task<Category[]> GetCategories(ICollection<ushort> ids)
        {
            var r = await GetCategoriesAsync(ids, Language);
            var icons = new Dictionary<AchievementCache.ItemID, IconData>();
            AchievementsGroup[] groups;
            int summary;
            var categories = await Task.Run<Category[]>(new Func<Category[]>(
                delegate
                {
                    return ParseCategories(r, icons, null, out summary, out groups);
                }));
            var images = await PopulateIcons(icons);
            return categories;
        }

        public async Task<IImage[]> GetIcons(ICollection<int> ids)
        {
            var icons = new Dictionary<AchievementCache.ItemID, IconData>(ids.Count);
            foreach (var i in ids)
            {
                var id = new AchievementCache.ItemID(AchievementCache.CacheType.Icon, i);
                icons[id] = new IconData()
                {
                    id = id,
                };
            }
            return await PopulateIcons(icons);
        }

        public async Task<Achievements> GetDailies(ICollection<ushort> ids)
        {
            var date = DateTime.UtcNow;
            var r = await GetCategoriesAsync(ids, Language);

            const byte TODAY = 0,
                       TOMORROW = 1;

            int summary = 0;
            AchievementsGroup[] groups = null;
            var achievements = new Dictionary<AchievementCache.ItemID, AchievementData>();
            var icons = new Dictionary<AchievementCache.ItemID, IconData>();
            var categories = await Task.Run<Category[]>(new Func<Category[]>(
                delegate
                {
                    return ParseCategories(r, icons, achievements, out summary, out groups);
                }));

            var minutes = date.Subtract(date.Date).TotalMinutes;
            var verified = minutes >= 60;

            if (cache != null)
            {
                if (cache.Summary == summary && cache.Age == 0 && cache.Today.Equals(groups[cache.Tomorrow == null ? TOMORROW : TODAY]))
                {
                    cache.Date = date;

                    throw new DailyNotModifiedException()
                    {
                        Date = date,
                    };
                }
                else if (!verified)
                {
                    //api usually updates within a few minutes after reset, but can take longer (worst seen: ~45m)

                    #region Find an existing category that can be matched to the new data

                    var ciCache = -1;
                    var ciGroup = 0;

                    for (; ciGroup < categories.Length; ciGroup++)
                    {
                        if (cache.Categories.Length > ciGroup && cache.Categories[ciGroup].ID == categories[ciGroup].ID)
                        {
                            ciCache = ciGroup;
                        }
                        else
                        {
                            var c = cache.GetCategory(categories[ciGroup].ID);

                            if (c != null)
                            {
                                ciCache = c.Index;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        //ensure the category doesn't have the same achievements every day
                        if (groups[TODAY].Categories[ciGroup].Count > 0 && groups[TOMORROW].Categories[ciGroup].Count > 0 && !groups[TODAY].Categories[ciGroup].Equals(groups[TOMORROW].Categories[ciGroup]))
                        {
                            break;
                        }
                        else
                        {
                            ciCache = -1;
                        }
                    }

                    #endregion

                    if (ciCache != -1)
                    {
                        var b = false;

                        switch (cache.Age)
                        {
                            case 0: //cache is for today

                                if (cache.Tomorrow == null)
                                {
                                    //tomorrow was previously swapped with today
                                    //if cache.today is still api.tomorrow, the api hasn't updated
                                    b = cache.Today.Categories[ciCache].Equals(groups[TOMORROW].Categories[ciGroup]);
                                    verified = !b;
                                }
                                else if (!cache.Today.Categories[ciCache].Equals(groups[TODAY].Categories[ciGroup]))
                                {
                                    //today has updated
                                    verified = true;
                                }

                                break;
                            case 1: //cache is 1 day old (cache.tomorrow should be api.today)

                                //if cache.today is still api.today, the api hasn't updated
                                b = cache.Today.Categories[ciCache].Equals(groups[TODAY].Categories[ciGroup]);
                                verified = !b;

                                break;
                            case 2: //cache is 2 days old (cache.tomorrow should not be api.today)

                                //if cache.tomorrow is still api.today, the api hasn't updated
                                if (cache.Tomorrow != null)
                                {
                                    b = cache.Tomorrow.Categories[ciCache].Equals(groups[TODAY].Categories[ciGroup]);
                                    verified = !b;
                                }

                                break;
                            default:

                                //anything older can't be compared

                                break;
                        }

                        if (b)
                        {
                            //daily hasn't been updated yet, tomorrow is today
                            groups[TODAY] = groups[TOMORROW];
                            groups[TOMORROW] = null;
                            verified = cache.Verified;
                        }
                    }
                    else
                    {
                        //categories have changed, there is nothing to compare to
                    }
                }
            }

            //if (!verified)
            //{
            //    verified = minutes >= 60;
            //}


            //if (current != null && current.Summary == summary)
            //{
            //    if (date.Subtract(date.Date).TotalMinutes < 15)
            //    {

            //    }

            //    var b = false;

            //    if (!current[Achievements.GroupType.Today].Equals(current[Achievements.GroupType.Tomorrow]))
            //    {
                    
            //    }

            //    if (current[Achievements.GroupType.Tomorrow] != null)
            //    {
            //        for (var i = 0; i < current.Categories.Length; i++)
            //        {
            //            //find a category that doesn't have the same dailies every day
            //            if (current[Achievements.GroupType.Tomorrow].Categories[i] != null && !current[Achievements.GroupType.Today].Categories[i].Equals(current[Achievements.GroupType.Tomorrow].Categories[i]))
            //            {
            //                if (current[0].Equals(groups[0]))
            //                {
            //                    b=true;
            //                    break;
            //                }
            //            }
            //        }
            //    }


            //    foreach (var c1 in current.Categories.Values)
            //    {
            //        //find a category that doesn't have the same dailies every day
            //        if (c1.Tomorrow != null && !Category.Equals(c1.Tomorrow, c1.Today))
            //        {
            //            //confirm the daily hasn't been updated yet by checking if what was previously tomorrow is now today
            //            Category c2;
            //            if (categories.TryGetValue(c1.ID, out c2) && Category.Equals(c1.Today, c2.Today))
            //            {
            //                b = true;
            //                break;
            //            }
            //        }
            //    }

            //    if (b)
            //    {
            //        throw new DailyNotModifiedException()
            //            {
            //                Date = date,
            await PopulateAchievements(achievements, icons, this.Language);
            var images = await PopulateIcons(icons);

            //var images = new IImage[icons.Count];
            //int i = 0;

            //foreach (var icon in icons.Values)
            //{
            //    images[i++] = icon;
            //    //var image = icon.GetImage();

            //    //if (image != null)
            //    //{
            //    //    images[i++] = image;
            //    //}
            //}

            for (var g = 0; g < groups.Length; g++)
            {
                if (groups[g] != null)
                {
                    for (var j = 0; j < categories.Length; j++)
                    {
                        var a = groups[g].Categories[j].Achievements;

                        if (a != null)
                        {
                            Array.Sort(a);
                        }
                    }
                }
            }

            //foreach (var c in categories.Values)
            //{
            //    if (c.Today != null)
            //    {
            //        Array.Sort(c.Today);
            //    }

            //    if (c.Tomorrow != null)
            //    {
            //        Array.Sort(c.Tomorrow);
            //    }
            //}

            //if (i != images.Length)
            //{
            //    Array.Resize<System.Drawing.Image>(ref images, i);
            //}

            //dailies.Icons = images;


            


            //var d = await TryGetDailies(Api.Net.URL + "v2/achievements/daily?v=latest");

            //if (d != null)
            //{
            //    if (today != null && Equals(d, today))
            //    {
            //        //the daily hasn't changed

            //        var t = d.Date.Ticks;
            //        var minutesAfter = (t - (t / TICKS_PER_DAY * TICKS_PER_DAY)) / 600000000;

            //        if (minutesAfter < 15)
            //        {
            //            //less than 15 minutes after the daily, allow rechecking
            //            nextUpdate = DateTime.UtcNow.AddMinutes(1);

            //            d.Dispose();
            //            return null;
            //        }
            //    }
            //}

            //today = d;


            var dailies = new Achievements(categories, groups, images)
            {
                Date = date,
                Summary = summary,
                Count = achievements.Count,
                Verified = verified,
            };

            if (cache != null)
            {
                cache.Dispose();
            }

            cache = dailies;

            return dailies;
        }

        private static Category[] ParseCategories(Net.ResponseData<string> r, Dictionary<AchievementCache.ItemID, IconData> icons, Dictionary<AchievementCache.ItemID, AchievementData> achievements, out int summary, out AchievementsGroup[] groups)
        {
            var data = (List<object>)Json.Decode(r.Data);
            var count = data.Count;
            if (count > ushort.MaxValue)
                count = ushort.MaxValue;
            var categories = new Category[count];

            if (achievements != null)
            {
                groups = new AchievementsGroup[]
                {
                    new AchievementsGroup(new CategoryAchievements[count]),
                    new AchievementsGroup(new CategoryAchievements[count]),
                };
            }
            else
            {
                groups = null;
            }

            var sum = 0;

            for (var index = 0; index < count; index++)
            {
                var d = (Dictionary<string, object>)data[index];

                var iconUrl = Json.GetValue<string>(d, "icon");
                var iconId = new AchievementCache.ItemID(AchievementCache.CacheType.Icon, GetIconIDFromUrl(iconUrl));
                IconData icon;

                if (!icons.TryGetValue(iconId, out icon) && iconId.id > 0)
                {
                    icons[iconId] = icon = new IconData()
                    {
                        id = iconId,
                        url = iconUrl,
                    };
                }

                var c = new Category()
                {
                    Index = (ushort)index,
                    ID = (ushort)Json.GetValue<int>(d, "id"),
                    Name = Json.GetString(d, "name"),
                    Icon = icon,
                };

                //categories[c.ID] = c;
                categories[index] = c;

                if (achievements != null)
                {
                    for (var j = 0; j < 2; j++)
                    {
                        var l = Json.GetValue<List<object>>(d, j == 0 ? "achievements" : "tomorrow");
                        Achievement[] daily;

                        if (l != null)
                        {
                            daily = new Achievement[l.Count];
                            groups[j].Categories[index] = new CategoryAchievements(c, daily);

                            //daily = new Achievement[l.Count];
                            //groups[j].Categories[index] = new CategoryAchievements(c,daily);

                            if (j == 0)
                            {
                                c.Today = daily;
                            }
                            else
                            {
                                c.Tomorrow = daily;
                            }
                        }
                        else
                        {
                            groups[j].Categories[index] = new CategoryAchievements(c, null);

                            continue;
                        }

                        for (var i = 0; i < daily.Length; i++)
                        {
                            var id = new AchievementCache.ItemID(AchievementCache.CacheType.Achievement, Json.GetValue<int>((Dictionary<string, object>)l[i], "id"));
                            AchievementData a;

                            if (!achievements.TryGetValue(id, out a))
                            {
                                achievements[id] = a = new AchievementData()
                                {
                                    id = id,
                                    data = new Achievement()
                                    {
                                        ID = (ushort)id.id,
                                    },
                                };
                            }

                            daily[i] = a.data;
                            sum += id.id;

                            groups[j].Summary += id.id;
                        }
                    }
                }
            }

            summary = sum;
            return categories;
        }

        private AchievementData[] ParseAchievements(Net.ResponseData<string> r, Dictionary<AchievementCache.ItemID, IconData> icons, Dictionary<AchievementCache.ItemID, AchievementData> achievements)
        {
            var data = (List<object>)Json.Decode(r.Data);
            var items = new AchievementData[data.Count];
            var index = 0;

            foreach (Dictionary<string, object> d in data)
            {
                var id = new AchievementCache.ItemID(AchievementCache.CacheType.Achievement, Json.GetValue<int>(d, "id"));
                AchievementData a;

                if (!achievements.TryGetValue(id, out a))
                {
                    continue;
                    //achievements[id] = a = new AchievementData()
                    //{
                    //    data = new Achievement()
                    //    {
                    //        ID = id.id,
                    //    },
                    //};
                }

                var iconUrl = Json.GetValue<string>(d, "icon");
                var iconId = new AchievementCache.ItemID(AchievementCache.CacheType.Icon, GetIconIDFromUrl(iconUrl));
                IconData icon;

                if (!icons.TryGetValue(iconId, out icon))
                {
                    if (iconId.id > 0)
                    {
                        icons[iconId] = icon = new IconData()
                        {
                            id = iconId,
                            url = iconUrl,
                        };
                    }
                }
                else if (icon.Empty)
                {
                    icon.url = iconUrl;
                }

                a.data.Name = Json.GetString(d, "name");
                a.data.Requirement = Json.GetString(d, "requirement");
                a.data.Icon = icon;
                a.icon = iconId.id;

                //var item = new AchievementData()
                //{
                //    id = new Storage.AchievementsCache.ItemID(CacheType.Achievement, Json.GetValue<int>(d, "id")),
                //    name = Json.GetString(d, "name"),
                //    requirement = Json.GetString(d, "requirement"),
                //    iconId = iconId.id,
                //    icon = icon,
                //};

                //achievements[item.id] = item;
                items[index++] = a;
            }

            return items;
        }

        private async Task PopulateAchievements(Dictionary<AchievementCache.ItemID, AchievementData> achievements, Dictionary<AchievementCache.ItemID, IconData> icons, Settings.Language lang)
        {
            var ids = new HashSet<AchievementCache.ItemID>(achievements.Keys);

            try
            {
                await AchievementCache.Storage.ReadAsync<AchievementData>(ids, achievements,
                    delegate(AchievementData a)
                    {
                        if (a.icon == 0)
                            return;

                        var iconId = new AchievementCache.ItemID(AchievementCache.CacheType.Icon, a.icon);
                        IconData icon;

                        if (!icons.TryGetValue(iconId, out icon))
                        {
                            icons[iconId] = icon = new IconData()
                            {
                                id = iconId
                            };
                        }

                        a.data.Icon = icon;
                    });
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (ids.Count > 0)
            {
                try
                {
                    var r = await GetAchievementsAsync(ids, lang);
                    var added = await Task.Run<AchievementData[]>(new Func<AchievementData[]>(
                        delegate
                        {
                            return ParseAchievements(r, icons, achievements);
                        }));

                    try
                    {
                        if (added.Length > 0)
                        {
                            await AchievementCache.Storage.WriteAsync<AchievementData>(added);
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

        private async Task<IImage[]> PopulateIcons(Dictionary<AchievementCache.ItemID, IconData> icons)
        {
            var ids = new HashSet<AchievementCache.ItemID>(icons.Keys);
            var images = new IImage[ids.Count];
            var index = 0;

            try
            {
                await AchievementCache.Storage.ReadAsync<IconData>(ids, icons,
                    delegate(IconData icon)
                    {
                        if (icon.bytes == null && icon.url != null)
                        {
                            ids.Add(icon.id);
                        }
                        else
                        {
                            images[index++] = icon;
                        }
                    });
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            if (ids.Count == 0)
            {
                return images;
            }

            var added = new List<IconData>(ids.Count);
            HashSet<AchievementCache.ItemID> invalid = null;

            foreach (var id in ids)
            {
                var icon = icons[id];

                images[index++] = icon;

                try
                {
                    if (icon.url == null)
                    {
                        continue;
                    }

                    var response = await Api.Net.DownloadBytesAsync(icon.url);

                    icon.bytes = response.Data;
                    added.Add(icon);
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
                                            invalid = new HashSet<AchievementCache.ItemID>();
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
            {
                await AchievementCache.Storage.DeleteAsync(invalid);
            }

            if (added.Count > 0)
            {
                await AchievementCache.Storage.WriteAsync<IconData>(added);
            }

            return images;
        }

        private async Task<Net.ResponseData<string>> GetAchievementsAsync(ICollection<AchievementCache.ItemID> ids, Settings.Language lang)
        {
            try
            {
                var sb = new StringBuilder((ids != null ? ids.Count * 5 : 3) + (lang != Settings.Language.EN ? 8 : 0));

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

                if (lang != Settings.Language.EN)
                {
                    sb.Append("&lang=");
                    sb.Append(lang.ToLanguageCode());
                }

                return await Net.DownloadStringAsync(Net.URL + "v2/achievements?ids=" + sb.ToString());
            }
            catch (System.Net.WebException e)
            {
                Api.Exceptions.Throw(e);
                throw;
            }
        }

        public static async Task<Category[]> GetCategoriesAsync(Settings.Language lang, params ushort[] ids)
        {
            var r = await GetCategoriesAsync(ids, lang);
            var achievements = new Dictionary<AchievementCache.ItemID, AchievementData>();
            var icons = new Dictionary<AchievementCache.ItemID, IconData>();
            var categories = await Task.Run<Category[]>(new Func<Category[]>(
                delegate
                {
                    int summary;
                    AchievementsGroup[] groups;
                    return ParseCategories(r, icons, achievements, out summary, out groups);
                }));
            return categories;
        }

        private static async Task<Net.ResponseData<string>> GetCategoriesAsync(ICollection<ushort> ids, Settings.Language lang)
        {
            try
            {
                var count = ids != null ? ids.Count : 0;
                var sb = new StringBuilder(count * 4 + 3 + (lang != Settings.Language.EN ? 8 : 0));

                if (count > 0)
                {
                    foreach (var id in ids)
                    {
                        sb.Append(id);
                        sb.Append(',');
                    }
                    sb.Length--;
                }
                else
                {
                    sb.Append("all");
                }

                if (lang != Settings.Language.EN)
                {
                    sb.Append("&lang=");
                    sb.Append(lang.ToLanguageCode());
                }

                return await Net.DownloadStringAsync(Net.URL + "v2/achievements/categories?v=latest&ids=" + sb.ToString());
            }
            catch (System.Net.WebException e)
            {
                Api.Exceptions.Throw(e);
                throw;
            }
        }

        public async Task<Achievement[]> GetStoredAchievements()
        {
            try
            {
                var data = new Dictionary<AchievementCache.ItemID, AchievementData>();

                await AchievementCache.Storage.ReadAsync<AchievementData>(null, data, null,
                    delegate(AchievementCache.ItemID id)
                    {
                        return id.type == AchievementCache.CacheType.Achievement;
                    });

                var result = new Achievement[data.Count];
                var i = 0;

                foreach (var d in data.Values)
                {
                    result[i++] = d.data;

                    d.data.ID = (ushort)d.id.id;
                }

                if (i != result.Length)
                {
                    Array.Resize<Achievement>(ref result, i);
                }
                
                return result;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                throw;
            }
        }
    }
}
