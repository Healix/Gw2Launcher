using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Gw2Launcher.Messaging
{
    class UpdateMessage
    {
        public UpdateMessage(int capacity)
        {
            files = new List<string>(capacity);
        }

        public List<string> files;

        public MappedMessage ToMap()
        {
            int id;
            using (var p = System.Diagnostics.Process.GetCurrentProcess())
                id = p.Id;

            byte count;
            if (files.Count > byte.MaxValue)
                count = byte.MaxValue;
            else
                count = (byte)files.Count;

            var length = 1;
            var _files = new byte[count][];
            for (var i = 0; i < count; i++)
            {
                byte[] b;
                _files[i] = b = Encoding.UTF8.GetBytes(files[i]);
                length += b.Length;
            }

            MemoryMappedFile mmf = MemoryMappedFile.CreateNew(MappedMessage.BASE_ID + "UpdateM:" + id, length + 2 * count);
            using (var stream = new BinaryWriter(mmf.CreateViewStream()))
            {
                stream.Write(count);

                for (var i = 0; i < count; i++)
                {
                    var b = _files[i];
                    stream.Write((ushort)b.Length);
                    stream.Write(b);
                }
            }

            return new MappedMessage(id, mmf);
        }

        public static UpdateMessage FromMap(int id)
        {
            using (var mmf = MemoryMappedFile.OpenExisting(MappedMessage.BASE_ID + "UpdateM:" + id))
            {
                using (var stream = new BinaryReader(mmf.CreateViewStream()))
                {
                    var count = stream.ReadByte();

                    UpdateMessage m = new UpdateMessage(count);
                    for (var i = 0; i < count; i++)
                    {
                        var length = stream.ReadUInt16();
                        m.files.Add(Encoding.UTF8.GetString(stream.ReadBytes(length)));
                    }

                    return m;
                }
            }
        }

        public void Send()
        {
            Send((IntPtr)Messaging.Messager.BROADCAST);
        }

        public void Send(IntPtr handle)
        {
            if (files.Count > 0)
            {
                using (var map = ToMap())
                    Messaging.Messager.Send(handle, Messaging.Messager.MessageType.UpdateMap, map.ID);
            }
        }
    }
}
