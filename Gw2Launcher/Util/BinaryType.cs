using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Util
{
    static class BinaryType
    {
        public static bool Is64(string path)
        {
            try
            {
                using (var reader = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete), 1024)))
                {
                    if (reader.ReadInt16() == 23117) //DOS signature
                    {
                        reader.BaseStream.Position = 60; //PE pointer offset
                        reader.BaseStream.Position = reader.ReadInt32() + 4 + 20; //4-byte PE pointer + 4-byte PE signature + 20-byte COFF header

                        var signature = reader.ReadInt16();

                        switch (signature)
                        {
                            case 267: //32-bit
                                return false;
                            case 523: //64-bit
                                return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }
    }
}
