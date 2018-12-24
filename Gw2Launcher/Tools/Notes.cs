using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Tools
{
    class Notes : IDisposable
    {
        private class Instance
        {
            private DataStore store;
            private byte subscribers;

            public void Register()
            {
                lock (this)
                {
                    subscribers++;
                }
            }

            public void Release()
            {
                lock (this)
                {
                    if (--subscribers == 0 && store != null)
                    {
                        store.Dispose();
                        store = null;
                    }
                }
            }

            public void Open()
            {
                lock (this)
                {
                    if (store == null)
                        store = new DataStore(Path.Combine(DataPath.AppData, "notes.dat"));
                }
            }

            public string Get(ushort id)
            {
                lock (this)
                {
                    try
                    {
                        Open();
                        return Encoding.UTF8.GetString(store.Get(id));
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            public bool Remove(ushort id)
            {
                lock (this)
                {
                    try
                    {
                        Open();
                    }
                    catch
                    {
                        return false;
                    }

                    return store.Remove(id);
                }
            }

            public ushort Add(string value)
            {
                lock (this)
                {
                    Open();

                    return store.Add(Encoding.UTF8.GetBytes(value));
                }
            }

            public ushort Replace(ushort id, string value)
            {
                lock (this)
                {
                    Open();

                    store.Remove(id);
                    return store.Add(Encoding.UTF8.GetBytes(value));
                }
            }

            public void RemoveExcept(IEnumerable<ushort> keys)
            {
                lock (this)
                {
                    Open();

                    store.RemoveExcept(keys);
                }
            }
        }

        private static Instance instance;

        private bool registered;

        static Notes()
        {
            instance = new Instance();
        }

        public Notes()
        {
            instance.Register();
            registered = true;
        }

        ~Notes()
        {
            if (registered)
            {
                instance.Release();
                registered = false;
            }
        }

        public void Dispose()
        {
            if (registered)
            {
                instance.Release();
                registered = false;
                GC.SuppressFinalize(this);
            }
        }

        public string Get(ushort id)
        {
            return instance.Get(id);
        }

        public ushort Add(string value)
        {
            return instance.Add(value);
        }

        public bool Remove(ushort id)
        {
            return instance.Remove(id);
        }

        public ushort Replace(ushort id, string value)
        {
            return instance.Replace(id, value);
        }

        /// <summary>
        /// Removes all keys except for those specified
        /// </summary>
        public void RemoveExcept(IEnumerable<ushort> keys)
        {
            instance.RemoveExcept(keys);
        }

        public Task<string> GetAsync(ushort id)
        {
            return Task.Run<string>(
                delegate
                {
                    return instance.Get(id);
                });
        }

        public Task<ushort> AddAsync(string value)
        {
            return Task.Run<ushort>(
                delegate
                {
                    return instance.Add(value);
                });
        }

        public Task<bool> RemoveAsync(ushort id)
        {
            return Task.Run<bool>(
                delegate
                {
                    return instance.Remove(id);
                });
        }

        public Task<ushort> ReplaceAsync(ushort id, string value)
        {
            return Task.Run<ushort>(
                delegate
                {
                    return instance.Replace(id, value);
                });
        }

        public Task OpenAsync()
        {
            return Task.Run(
                delegate
                {
                    instance.Open();
                });
        }

        public Task CloseAsync()
        {
            return Task.Run(
                delegate
                {
                    Dispose();
                });
        }

        public static Task<bool[]> RemoveRange(params ushort[] ids)
        {
            return Task.Run<bool[]>(
                delegate
                {
                    var results = new bool[ids.Length];
                    var i = 0;

                    using (var store = new Notes())
                    {
                        foreach (var id in ids)
                        {
                            results[i++] = store.Remove(id);
                        }
                    }
                    return results;
                });
        }

        public static Task<string[]> GetRange(params ushort[] ids)
        {
            return Task.Run<string[]>(
                delegate
                {
                    var results = new string[ids.Length];
                    var i = 0;

                    using (var store = new Notes())
                    {
                        foreach (var id in ids)
                        {
                            results[i++] = store.Get(id);
                        }
                    }
                    return results;
                });
        }

        public static Task<ushort[]> AddRange(params string[] values)
        {
            return Task.Run<ushort[]>(
                delegate
                {
                    var results = new ushort[values.Length];
                    var i = 0;

                    using (var store = new Notes())
                    {
                        foreach (var value in values)
                        {
                            results[i++] = store.Add(value);
                        }
                    }
                    return results;
                });
        }
    }
}
