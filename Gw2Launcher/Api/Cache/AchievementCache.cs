using Gw2Launcher.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Api.Cache
{
    class AchievementCache
    {
        private const int HEADER = 1751335244;
        private const ushort VERSION = 2;
        private const string FILE_NAME = "achievements.dat";

        public enum CacheType : byte
        {
            Unknown = 0,
            Achievement = 1,
            Icon = 2,
            Vault = 3,
        }

        public class ItemID : IEquatable<ItemID>
        {
            public CacheType type;
            public int id;

            public ItemID(CacheType type, int id)
            {
                this.type = type;
                this.id = id;
            }

            public override int GetHashCode()
            {
                return id + ((int)type << 24);
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

        private class FormatCacheID : DataCache.IFormat<ItemID>
        {
            public ItemID Read(BinaryReader reader)
            {
                var type = (CacheType)reader.ReadByte();
                int id;

                switch (type)
                {
                    case CacheType.Achievement:
                    case CacheType.Vault:
                        id = reader.ReadUInt16();
                        break;
                    case CacheType.Icon:
                        id = reader.ReadInt32();
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
                    case CacheType.Vault:
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

        private static DataCache<ItemID> storage;

        static AchievementCache()
        {
            storage = new DataCache<ItemID>(HEADER, VERSION, Path.Combine(DataPath.AppData, FILE_NAME), new FormatCacheID());
        }

        public static DataCache<ItemID> Storage
        {
            get
            {
                return storage;
            }
        }
    }
}
