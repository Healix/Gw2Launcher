using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Gw2Launcher.Client
{
    static partial class Launcher
    {
        static class FileLocker
        {
            public class SharedFile
            {
                private ushort locks;

                public SharedFile(string path)
                {
                    this.Path = path;
                }

                public string Path
                {
                    get;
                    private set;
                }

                public Stream Stream
                {
                    get;
                    set;
                }

                public void Release()
                {
                    lock(files)
                    {
                        if (--locks == 0)
                        {
                            Remove(this);
                        }
                    }
                }

                public void Lock()
                {
                    lock(files)
                    {
                        ++locks;
                    }
                }
            }

            private static Dictionary<string, SharedFile> files;

            static FileLocker()
            {
                files = new Dictionary<string, SharedFile>();
            }

            public static SharedFile Lock(Account account, string path, int timeout)
            {
                SharedFile sf;

                lock (files)
                {
                    bool b;
                    if (b = !files.TryGetValue(path, out sf))
                        files[path] = sf = new SharedFile(path);

                    sf.Lock();

                    if (!b)
                        return sf;
                }

                if (File.Exists(path))
                {
                    var limit = DateTime.UtcNow.AddMilliseconds(timeout);

                    do
                    {
                        try
                        {
                            sf.Stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                            break;
                        }
                        catch (FileNotFoundException)
                        {
                            break;
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }

                        if (DateTime.UtcNow > limit)
                            break;

                        Thread.Sleep(100);
                    }
                    while (true);
                }

                if (sf.Stream == null)
                {
                    lock (files)
                    {
                        files.Remove(sf.Path);
                        return null;
                    }
                }

                return sf;
            }

            public static SharedFile Lock(Account account, Settings.IFile file, int timeout)
            {
                if (file.References <= 1)
                    return null;

                return Lock(account, file.Path, timeout);
            }

            private static void Remove(SharedFile sf)
            {
                lock (files)
                {
                    using (sf.Stream) { }
                    files.Remove(sf.Path);
                }
            }

            public static void Clear()
            {
                lock (files)
                {
                    foreach (var sf in files.Values)
                    {
                        using (sf.Stream) { }
                    }
                    files.Clear();
                }
            }
        }
    }
}
