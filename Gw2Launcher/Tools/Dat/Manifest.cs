using System.IO;

namespace Gw2Launcher.Tools.Dat
{
    class Manifest
    {
        public class ManifestRecord
        {
            public int baseId, fileId, size;
        }

        private Manifest()
        {

        }

        private static Manifest ParseRoot(BinaryReader r, long fileOffset)
        {
            var records = r.ReadInt32();
            var offset = 36 + r.ReadInt32();

            Manifest m = new Manifest()
            {
                records = new ManifestRecord[records]
            };

            r.BaseStream.Position = fileOffset + offset;

            for (var i = 0; i < records; i++)
            {
                var re = m.records[i] = new ManifestRecord();

                re.baseId = r.ReadInt32();
                re.fileId = r.ReadInt32();
                re.size = r.ReadInt32();

                if (re.baseId <= 0 || re.fileId <= 0 || re.size < 0)
                    throw new InvalidDataException();

                r.BaseStream.Position += 12;
            }

            return m;
        }

        private static Manifest ParseAsset(BinaryReader r, long fileOffset)
        {
            r.BaseStream.Position += 8;

            var records = r.ReadInt32();
            var offset = 44 + r.ReadInt32();

            Manifest m = new Manifest()
            {
                records = new ManifestRecord[records]
            };

            r.BaseStream.Position = fileOffset + offset;

            for (var i = 0; i < records; i++)
            {
                var re = m.records[i] = new ManifestRecord();

                re.baseId = r.ReadInt32();
                re.fileId = r.ReadInt32();
                re.size = r.ReadInt32();

                if (re.baseId <= 0 || re.fileId <= 0 || re.size < 0)
                    throw new InvalidDataException();

                r.BaseStream.Position += 4;
            }

            return m;
        }

        public static Manifest Parse(Stream stream)
        {
            using (var r = new BinaryReader(new BufferedStream(stream, 8192)))
            {
                var fileOffset = r.BaseStream.Position;

                r.BaseStream.Position += 12;

                var chunkId = r.ReadInt32();

                r.BaseStream.Position += 12;

                var build = r.ReadInt32();

                Manifest m;

                if (chunkId == 1414743629)
                    m = ParseAsset(r, fileOffset);
                else if (chunkId == 1179472449)
                    m = ParseRoot(r, fileOffset);
                else
                    throw new IOException("Unknown chunk header");

                m.build = build;

                return m;
            }
        }

        public int build;
        public ManifestRecord[] records;
    }
}
