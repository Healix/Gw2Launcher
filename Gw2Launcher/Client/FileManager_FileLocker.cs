using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Gw2Launcher.Client
{
    public static partial class FileManager
    {
        public static class FileLocker
        {
            public interface ISharedFile : IDisposable
            {
                string Path
                {
                    get;
                }

                Settings.IFile File
                {
                    get;
                }

                ISharedFile Copy();
            }

            private class SharedDisposable : ISharedFile
            {
                private SharedFile file;

                public SharedDisposable(SharedFile s)
                {
                    file = s;
                }

                public string Path
                {
                    get
                    {
                        lock (this)
                        {
                            if (file != null)
                                return file.Path;
                            return null;
                        }
                    }
                }

                public Settings.IFile File
                {
                    get;
                    set;
                }

                public ISharedFile Copy()
                {
                    lock (files)
                    {
                        if (file.IsAlive)
                        {
                            file.Lock();
                            return new SharedDisposable(file);
                        }
                    }

                    throw new ObjectDisposedException("SharedFile");
                }

                public void Dispose()
                {
                    lock (this)
                    {
                        if (file != null)
                        {
                            file.Release();
                            file = null;
                        }
                    }
                }
            }

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
                        if (locks > 0 && --locks == 0)
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

                public bool IsAlive
                {
                    get
                    {
                        return locks > 0;
                    }
                }
            }

            private static Dictionary<string, SharedFile> files;

            static FileLocker()
            {
                files = new Dictionary<string, SharedFile>();
            }

            /// <summary>
            /// Locks the file
            /// </summary>
            /// <param name="timeout">The amount of time to try to acquire a lock</param>
            public static ISharedFile Lock(string path, int timeout)
            {
                SharedFile sf;

                lock (files)
                {
                    bool b;
                    if (b = !files.TryGetValue(path, out sf))
                        files[path] = sf = new SharedFile(path);

                    sf.Lock();

                    if (!b)
                        return new SharedDisposable(sf);
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

                return new SharedDisposable(sf);
            }

            /// <summary>
            /// Locks the file
            /// </summary>
            /// <param name="timeout">The amount of time to try to acquire a lock</param>
            /// <param name="force">Forces a file with only 1 reference to lock</param>
            public static ISharedFile Lock(Settings.IFile file, int timeout, bool force = false)
            {
                if (file == null || !force && !file.IsLocked && file.References <= 1)
                    return null;

                var l = Lock(file.Path, timeout);
                ((SharedDisposable)l).File = file;

                return l;
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

            public static async void Release(ISharedFile file, int delay)
            {
                await Task.Delay(delay);

                file.Dispose();
            }
        }
    }
}
