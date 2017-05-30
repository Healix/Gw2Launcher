using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Gw2Launcher.Tools
{
    static class Statistics
    {
        private const ushort WRITE_DELAY = 10000;
        private const string FILE_NAME = "statistics.dat";

        public enum RecordType
        {
            Launched,
            Exited
        }

        private struct QueuedRecord
        {
            public QueuedRecord(RecordType type, ushort uid, DateTime time)
            {
                this.type = type;
                this.uid = uid;
                this.time = time;
            }

            public RecordType type;
            public ushort uid;
            public DateTime time;
        }

        private static Queue<QueuedRecord> queue;
        private static Task task;
        private static DateTime lastQueue;
        private static CancellationTokenSource cancelToken;
        private static object _writer = new object();

        static Statistics()
        {
            queue = new Queue<QueuedRecord>();
            cancelToken = new CancellationTokenSource();
        }

        public static void Record(RecordType type, ushort uid)
        {
            var now = lastQueue = DateTime.UtcNow;
            lock(queue)
            {
                queue.Enqueue(new QueuedRecord(type, uid, now));
                if (task == null || task.IsCompleted)
                {
                    var cancel = cancelToken.Token;
                    task = Task.Factory.StartNew(
                        new Action(
                            delegate
                            {
                                DoQueue(cancel);
                            }));
                }
            }
        }

        private static void DoQueue(CancellationToken cancel)
        {
            while (true)
            {
                while (true)
                {
                    int remaining = WRITE_DELAY - (int)DateTime.UtcNow.Subtract(lastQueue).TotalMilliseconds;
                    if (remaining > 0)
                    {
                        if (cancel.WaitHandle.WaitOne(remaining))
                            break;
                    }
                    else
                    {
                        break;
                    }
                }

                Write();

                if (cancel.IsCancellationRequested)
                    return;

                lock(queue)
                {
                    if (queue.Count == 0)
                    {
                        task = null;
                        return;
                    }
                }
            }
        }

        private static void Write()
        {
            QueuedRecord[] items;

            lock (queue)
            {
                int count = queue.Count;
                items = new QueuedRecord[count];

                for (int i = 0; i < count; i++)
                {
                    items[i] = queue.Dequeue();
                }
            }

            if (items.Length > 0)
            {
                try
                {
                    Write(items);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        private static void Write(IEnumerable<QueuedRecord> items)
        {
            lock (_writer)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine(DataPath.AppData, FILE_NAME), FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                    foreach (var q in items)
                    {
                        writer.Write(q.uid);
                        writer.Write((byte)q.type);
                        writer.Write(q.time.ToBinary());
                    }
                }
            }
        }

        public static void Save()
        {
            Task t;
            lock(queue)
            {
                if (task == null)
                    return;
                t = task;
                cancelToken.Cancel();
            }

            t.Wait();
        }

        public static string Export(Settings.IAccount account)
        {
            List<QueuedRecord> l = new List<QueuedRecord>();
            ushort uid = account.UID;
            DateTime programExit = DateTime.MinValue;
            DateTime launch = DateTime.MinValue;

            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(Path.Combine(DataPath.AppData, FILE_NAME), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (true)
                    {
                        ushort id = reader.ReadUInt16();
                        RecordType type = (RecordType)reader.ReadByte();
                        DateTime time = DateTime.FromBinary(reader.ReadInt64());

                        if (id == uid && account.CreatedUtc <= time)
                        {
                            if (type == RecordType.Launched)
                            {
                                if (launch != DateTime.MinValue && programExit > launch)
                                    l.Add(new QueuedRecord(RecordType.Exited, 0, programExit));

                                l.Add(new QueuedRecord(type, id, time));

                                launch = time;
                                programExit = DateTime.MinValue;
                            }
                            else if (type == RecordType.Exited && launch != DateTime.MinValue)
                            {
                                l.Add(new QueuedRecord(type, id, time));
                                launch = DateTime.MinValue;
                            }
                        }

                        if (id == 0)
                        {
                            if (type == RecordType.Exited && time > launch && programExit == DateTime.MinValue)
                                programExit = time;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            if (launch != DateTime.MinValue && programExit > launch)
                l.Add(new QueuedRecord(RecordType.Exited, 0, programExit));

            if (l.Count == 0)
                return null;

            string path = Path.GetTempFileName();

            try
            {
                using (StreamWriter writer = new StreamWriter(File.Create(path)))
                {
                    writer.WriteLine("# " + account.Name);
                    writer.WriteLine("# event_type, UTC date (yyyy-MM-ddTHH:mm:ss)");
                    writer.WriteLine();

                    foreach (var q in l)
                    {
                        string type;
                        if (q.uid == 0)
                        {
                            switch (q.type)
                            {
                                case RecordType.Exited:
                                    type = "app_exit";
                                    break;
                                case RecordType.Launched:
                                    type = "app_start";
                                    break;
                                default:
                                    type = "";
                                    break;
                            }
                        }
                        else
                        {
                            switch (q.type)
                            {
                                case RecordType.Exited:
                                    type = "gw2_exit";
                                    break;
                                case RecordType.Launched:
                                    type = "gw2_launch";
                                    break;
                                default:
                                    type = "";
                                    break;
                            }
                        }

                        writer.WriteLine(type + ", " + q.time.ToString("s"));
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);

                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
                return null;
            }

            return path;
        }
    }
}
