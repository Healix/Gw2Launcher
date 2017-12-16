using System.IO;

namespace Gw2Launcher.Tools.Dat
{
    static class DatFile
    {
        public class MftEntry
        {
            public int baseId, fileId;
        }

        public static MftEntry[] Read(string path)
        {
            MftEntry[] entries;

            using (var r = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                int id, size, compression;
                long offset;

                r.BaseStream.Position = 24;
                
                offset = r.ReadInt64();
                size = r.ReadInt32();

                //earch entry is 24 bytes - skipping to the 3rd entry, which points to the ids
                r.BaseStream.Position = offset + 48;

                entries = new MftEntry[size / 24];

                offset = r.ReadInt64();
                size = r.ReadInt32();
                compression = r.ReadInt16();

                if (compression != 0)
                    throw new IOException("File is compressed");

                //each entry is 8 bytes - file id + index
                r.BaseStream.Position = offset;

                do
                {
                    id = r.ReadInt32();
                    int i = r.ReadInt32();

                    MftEntry entry = entries[i];
                    if (entry == null)
                    {
                        entry = entries[i] = new MftEntry()
                        {
                            baseId = id,
                            fileId = id
                        };
                    }
                    else
                    {
                        if (id < entry.baseId)
                            entry.baseId = id;
                        if (id > entry.fileId)
                            entry.fileId = id;
                    }

                    size -= 8;
                }
                while (size > 0);
            }

            return entries;
        }
    }
}
