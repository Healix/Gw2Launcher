using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gw2Launcher.Tools.Chromium
{
    public class CefSessionMonitor
    {
        public class SessionEventArgs : EventArgs
        {
            public enum EventType
            {
                VaultOpened,
                VaultClosed,
            }

            public SessionEventArgs(EventType type, Settings.IAccount account)
            {
                this.Type = type;
                this.Account = account;
            }

            public EventType Type
            {
                get;
                private set;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }
        }

        public interface IMonitor : IDisposable
        {
            Settings.IAccount Account
            {
                get;
            }

            string Path
            {
                get;
            }

            bool IsVaultOpen
            {
                get;
            }

            bool Enabled
            {
                get;
                set;
            }
        }

        private class Monitor : IMonitor
        {
            private FileSystemWatcher watcher;
            private string path;
            private FileStream stream;
            private bool pending;
            private Settings.IAccount account;
            private bool disposing;
            private bool enabled;
            private CefSessionMonitor owner;

            public Monitor(CefSessionMonitor owner, Settings.IAccount account, string path)
            {
                this.owner = owner;
                this.account = account;
                this.path = path;
                this.pending = true;

                this.enabled = !Settings.Tweaks.DisableVaultMonitor.Value && IsTrackingEnabled();

                if (account.Type == Settings.AccountType.GuildWars2)
                {
                    ((Settings.IGw2Account)account).ApiTrackingChanged += OnApiTrackingChanged;
                }
            }

            void OnApiTrackingChanged(object sender, EventArgs e)
            {
                Enabled = !Settings.Tweaks.DisableVaultMonitor.Value && IsTrackingEnabled();
            }

            public bool IsTrackingEnabled()
            {
                return account.Type == Settings.AccountType.GuildWars2 && (((Settings.IGw2Account)account).ApiTracking & (Settings.ApiTracking.Astral | Settings.ApiTracking.Daily | Settings.ApiTracking.Weekly)) != 0;
            }

            public string Path
            {
                get
                {
                    return path;
                }
            }

            public Settings.IAccount Account
            {
                get
                {
                    return account;
                }
            }

            public bool Pending
            {
                get
                {
                    return pending;
                }
            }

            public ushort Key
            {
                get;
                set;
            }

            public bool IsVaultOpen
            {
                get;
                set;
            }

            private bool Open()
            {
                var min = DateTime.MinValue;
                string path = null;

                foreach (var f in Directory.GetFiles(this.path, "0*.log"))
                {
                    if (path != null)
                    {
                        if (min == DateTime.MinValue)
                            min = File.GetLastWriteTimeUtc(path);
                        var d = File.GetLastWriteTimeUtc(f);
                        if (d > min)
                        {
                            min = d;
                            path = f;
                        }
                    }
                    else
                    {
                        path = f;
                    }
                }

                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }

                if (path != null)
                {
                    try
                    {
                        stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                        stream.Position = stream.Length;
                    }
                    catch { }
                }

                if (watcher == null)
                {
                    //note the current log file can change, but it's unlikely to happen
                    try
                    {
                        watcher = new FileSystemWatcher(this.path, "0*.log")
                        {
                            NotifyFilter = NotifyFilters.FileName,
                        };
                        watcher.Error += watcher_Error;
                        watcher.Created += watcher_Created;
                        watcher.EnableRaisingEvents = true;
                    }
                    catch
                    {
                        using (watcher)
                        {
                            watcher = null;
                        }
                    }
                }

                return stream != null;
            }

            void watcher_Created(object sender, FileSystemEventArgs e)
            {
                pending = true;
            }

            void watcher_Error(object sender, ErrorEventArgs e)
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.EnableRaisingEvents = true;
                }
                catch
                {
                    using (watcher)
                    {
                        watcher = null;
                    }
                    pending = true;
                }
            }

            public int Read(byte[] buffer, int offset, int length)
            {
                if (pending)
                {
                    pending = false;

                    if (!Open())
                    {
                        pending = true;
                        return 0;
                    }
                }

                if (stream != null)
                {
                    var l = stream.Length;
                    var p = stream.Position;

                    if (p > l)
                    {
                        stream.Position = p = l;
                    }

                    var remaining = l - p;

                    if (remaining != 0)
                    {
                        if (remaining > length)
                        {
                            remaining = length;
                        }

                        return stream.Read(buffer, offset, (int)remaining);
                    }
                }

                return 0;
            }

            public bool Enabled
            {
                get
                {
                    return enabled;
                }
                set
                {
                    if (enabled != value)
                    {
                        if (Util.Logging.Enabled)
                        {
                            if (value)
                                Util.Logging.LogEvent(account, "Enabling monitor for " + account.Name);
                            else
                                Util.Logging.LogEvent(account, "Disabling monitor for " + account.Name);
                        }
                        enabled = value;
                        OnMonitorChanged();
                    }
                }
            }

            private void OnMonitorChanged()
            {
                if (owner != null)
                {
                    owner.OnMonitorChanged(this);
                }
            }

            public bool Disposing
            {
                get
                {
                    return disposing;
                }
            }

            public void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.disposing = true;

                    if (owner != null)
                    {
                        owner = null;

                        if (account != null && account.Type == Settings.AccountType.GuildWars2)
                        {
                            ((Settings.IGw2Account)account).ApiTrackingChanged -= OnApiTrackingChanged;
                        }
                    }

                    using (watcher)
                    {
                        watcher = null;
                    }
                    using (stream)
                    {
                        stream = null;
                    }
                }
            }

            public void Dispose()
            {
                if (!disposing)
                {
                    disposing = true;
                    OnMonitorChanged();
                }
            }
        }

        public event EventHandler<SessionEventArgs> SessionEvent;

        private Monitor[] monitors;
        private int count;
        //private Dictionary<ushort, Monitor> accounts;
        //private Queue<Monitor> disposing;
        private bool refresh;
        private Task task;

        public CefSessionMonitor()
        {
            Settings.Tweaks.DisableVaultMonitor.ValueChanged += DisableVaultMonitor_ValueChanged;
            //accounts = new Dictionary<ushort, Monitor>();
        }

        void DisableVaultMonitor_ValueChanged(object sender, EventArgs e)
        {
            lock(this)
            {
                var b = !Settings.Tweaks.DisableVaultMonitor.Value;

                for (var i = 0; i < count; i++)
                {
                    monitors[i].Enabled = b && monitors[i].IsTrackingEnabled();
                }
            }
        }

        /// <summary>
        /// Adds an account to be monitored
        /// </summary>
        /// <param name="account">The account being monitored</param>
        /// <param name="path">The path to the user folder</param>
        public IMonitor Add(Settings.IAccount account, string path)
        {
            var m = new Monitor(this, account, Path.Combine(path, "Cache", "Session Storage"));

            lock (this)
            {
                if (monitors == null)
                {
                    monitors = new Monitor[3];
                }
                else if (count == monitors.Length)
                {
                    Array.Resize<Monitor>(ref monitors, count + 3);
                }

                monitors[count] = m;
                ++count;

                if (m.Enabled)
                {
                    StartMonitor();
                }
            }

            return m;
        }

        private void OnMonitorChanged(Monitor m)
        {
            StartMonitor();
        }

        private void StartMonitor()
        {
            lock (this)
            {
                refresh = true;

                if (task == null || task.IsCompleted)
                {
                    using (var t = Task.Run<Task>(new Func<Task>(DoMonitor)))
                    {
                        task = t.Result;
                    }
                }
            }
        }

        private async Task DoMonitor()
        {
            await Task.Delay(100);

            Monitor[] monitors = null;
            var buffer = new byte[1024];
            var search = Encoding.ASCII.GetBytes("//vault-");
            var length = 0;

            //opening and closing a page is logged and includes and session key
            //the session key changes whenever the vault/interface is opened

            while (true)
            {
                var enabled = false;

                if (refresh)
                {
                    Monitor[] disposing = null;
                    var di = 0;

                    lock(this)
                    {
                        monitors = this.monitors;
                        refresh = false;

                        for (var i = 0; i < count;)
                        {
                            if (monitors[i].Disposing)
                            {
                                if (di == 0)
                                {
                                    disposing = new Monitor[count - i];
                                }

                                disposing[di++] = monitors[i];

                                if (--count != i)
                                {
                                    monitors[i] = monitors[count];
                                }

                                monitors[count] = null;
                            }
                            else
                            {
                                if (!enabled)
                                {
                                    enabled = monitors[i].Enabled;
                                }
                                ++i;
                            }
                        }

                        length = count;

                        if (!enabled && di == 0)
                        {
                            task = null;
                            if (count == 0)
                            {
                                monitors = null;
                            }
                            return;
                        }
                    }

                    if (di > 0)
                    {
                        //VaultClosed won't occur if the account was exited with the vault open
                        for (var i = 0; i < di; i++)
                        {
                            var m = disposing[i];

                            if (m == null)
                            {
                                break;
                            }
                            else
                            {
                                if (m.IsVaultOpen)
                                {
                                    OnSessionEvent(SessionEventArgs.EventType.VaultClosed, m);
                                }

                                m.Dispose(true);
                            }
                        }
                    }
                }

                for (var i = 0; i < length; i++)
                {
                    if (!monitors[i].Enabled)
                    {
                        continue;
                    }
                    else
                    {
                        enabled = true;
                    }

                    int read;

                    try
                    {
                        read = monitors[i].Read(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        read = 0;
                    }

                    if (read > 0)
                    {
                        var j = 0;
                        var opened = false;
                        ushort _key = 0;

                        while (true)
                        {
                            j = Util.Array.IndexOf<byte>(buffer, search, j, read - j);

                            if (j == -1)
                                break;

                            //read a part of the session key to use as an identifier
                            if (j > 19)
                            {
                                ushort key = 0;

                                for (var k = j - 19; k < j; k++)
                                {
                                    key += buffer[k];
                                }

                                //avoiding pushing events when the vault is opened and closed within the same (5s) cycle

                                if (key != monitors[i].Key)
                                {
                                    monitors[i].Key = _key = key;
                                    opened = true;
                                }
                                else
                                {
                                    if (_key != key)
                                    {
                                        monitors[i].IsVaultOpen = false;
                                        OnSessionEvent(SessionEventArgs.EventType.VaultClosed, monitors[i]);
                                    }
                                    else
                                    {
                                        opened = false;
                                    }
                                }
                            }

                            j += 10;
                        }

                        if (opened)
                        {
                            monitors[i].IsVaultOpen = true;
                            OnSessionEvent(SessionEventArgs.EventType.VaultOpened, monitors[i]);
                        }
                    }
                }

                if (!enabled)
                {
                    refresh = true;
                }

                await Task.Delay(2000);
            }
        }

        private void OnSessionEvent(SessionEventArgs.EventType t, Monitor m)
        {
            if (SessionEvent != null)
            {
                try
                {
                    SessionEvent(this, new SessionEventArgs(t, m.Account));
                }
                catch { }
            }
        }
    }
}
