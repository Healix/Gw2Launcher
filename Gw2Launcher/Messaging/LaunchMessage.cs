using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Gw2Launcher.Messaging
{
    class LaunchMessage
    {
        public LaunchMessage(int capacity)
        {
            accounts = new List<ushort>(capacity);
        }

        public List<ushort> accounts;
        public string args;

        public MappedMessage ToMap()
        {
            int id;
            using (var p = System.Diagnostics.Process.GetCurrentProcess())
                id = p.Id;

            byte[] _args;
            if (!string.IsNullOrEmpty(args))
                _args = System.Text.Encoding.UTF8.GetBytes(args);
            else
                _args = new byte[0];

            byte count;
            if (accounts.Count > byte.MaxValue)
                count = byte.MaxValue;
            else
                count = (byte)accounts.Count;

            MemoryMappedFile mmf = MemoryMappedFile.CreateNew(MappedMessage.BASE_ID + "L:" + id, 3 + count * 2 + _args.Length);
            using (var stream = new BinaryWriter(mmf.CreateViewStream()))
            {
                stream.Write(count);

                for (var i = 0; i < count; i++)
                    stream.Write(accounts[i]);

                stream.Write((ushort)_args.Length);
                stream.Write(_args);
            }

            return new MappedMessage(id, mmf);
        }

        public static LaunchMessage FromMap(int id)
        {
            using (var mmf = MemoryMappedFile.OpenExisting(MappedMessage.BASE_ID + "L:" + id))
            {
                using (var stream = new BinaryReader(mmf.CreateViewStream()))
                {
                    var count = stream.ReadByte();

                    LaunchMessage m = new LaunchMessage(count);
                    for (var i = 0; i < count; i++)
                        m.accounts.Add(stream.ReadUInt16());

                    var length = stream.ReadUInt16();
                    m.args = Encoding.UTF8.GetString(stream.ReadBytes(length));

                    return m;
                }
            }
        }

        public void Send()
        {
            if (accounts.Count > 0)
            {
                if (string.IsNullOrEmpty(args))
                {
                    foreach (var uid in accounts)
                        Messaging.Messager.Post(Messaging.Messager.MessageType.Launch, uid);
                }
                else
                {
                    using (var map = ToMap())
                        Messaging.Messager.Send(Messaging.Messager.MessageType.LaunchMap, map.ID);
                }
            }
        }
    }
}
