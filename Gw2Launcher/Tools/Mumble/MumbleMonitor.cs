using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Tools.Mumble
{
    public class MumbleMonitor
    {
        private const int MUMBLE_DATA_LENGTH = 5460;
        private const int MUMBLE_DATA_LENGTH_BASIC = 1193;
        private const int MUMBLE_DATA_OFFSET_TICKS = 4;
        private const int MUMBLE_DATA_OFFSET_PROCESS_ID = 1188;
        private const string MUMBLE_DEFAULT_NAME = "MumbleLink";

        private enum LinkState
        {
            Unknown,
            Pending,
            Verified,
        }

        [Flags]
        public enum DataScope
        {
            None = 0,
            /// <summary>
            /// Excludes JSON data
            /// </summary>
            Basic = 1,
            /// <summary>
            /// Includes JSON data
            /// </summary>
            Extended = 2,
        }

        public interface IMumbleSubscriber : IDisposable
        {
            event EventHandler DataAvailable;
            bool GetData(out MumbleData.IdentificationData data);
            /// <summary>
            /// Returns cached data from a previous refresh
            /// </summary>
            /// <typeparam name="T">MumbleData structure</typeparam>
            /// <returns>True if data was available</returns>
            bool GetData<T>(out T data) where T : struct;
            Task<MumbleData.IdentificationData> GetData(int timeout);
            /// <summary>
            /// Refreshes and returns data
            /// </summary>
            /// <typeparam name="T">MumbleData structure</typeparam>
            /// <param name="timeout">Timeout in milliseconds</param>
            Task<T> GetData<T>(int timeout) where T : struct;
            Task<bool> Refresh(int timeout = -1);
            Settings.IAccount Account
            {
                get;
            }
        }

        public interface IMumbleProcess : IDisposable
        {
            /// <summary>
            /// Occurs when the link has been verfied
            /// </summary>
            event EventHandler Verified;
            /// <summary>
            /// Re-checks if the link is actively being updated
            /// </summary>
            void Verify();
            bool IsVerified
            {
                get;
            }
            /// <summary>
            /// An invalid link will not have any data
            /// </summary>
            bool IsValid
            {
                get;
            }
            Settings.IAccount Account
            {
                get;
            }
            IMumbleSubscriber Subscribe(DataScope scope);
            string LinkName
            {
                get;
            }
        }

        private class MumbleProcessSubscriber : IMumbleSubscriber
        {
            public event EventHandler DataAvailable;

            public MumbleProcessSubscriber(MumbleProcess parent, DataScope scope)
            {
                this.Parent = parent;
                this.Scope = scope;

                parent.DataAvailable += OnDataAvailable;
            }

            ~MumbleProcessSubscriber()
            {
                Dispose();
            }

            public bool IsValid
            {
                get
                {
                    return Parent.Parent.IsValid;
                }
            }

            public MumbleProcess Parent
            {
                get;
                private set;
            }

            public DataScope Scope
            {
                get;
                private set;
            }

            public int Age
            {
                get
                {
                    return Parent.Age;
                }
            }
            
            public Settings.IAccount Account
            {
                get
                {
                    return Parent.Account;
                }
            }

            public bool GetData(out MumbleData.IdentificationData data)
            {
                return Parent.GetData<MumbleData.IdentificationData>(DataScope.Basic, out data);
            }

            public Task<MumbleData.IdentificationData> GetData(int timeout)
            {
                return Parent.GetData<MumbleData.IdentificationData>(DataScope.Basic, timeout);
            }

            public bool GetData<T>(out T data) where T : struct
            {
                return Parent.GetData<T>(Scope, out data);
            }

            public Task<T> GetData<T>(int timeout) where T : struct
            {
                return Parent.GetData<T>(Scope, timeout);
            }

            public async Task<bool> Refresh(int timeout = -1)
            {
                var p = Parent;
                if (p == null)
                    return false;
                var t = Environment.TickCount;
                var tick = p.Tick;

                do
                {
                    if (await p.Query() && tick != p.Tick && p.DataScope >= Scope)
                    {
                        return true;
                    }
                    else if (Scope == DataScope.None)
                    {
                        break;
                    }
                    else if (timeout != -1)
                    {
                        var e = timeout - (Environment.TickCount - t);
                        if (e <= 0)
                            break;
                        else if (e > 100)
                            await Task.Delay(100);
                        else
                            await Task.Delay(e);
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
                while (true);

                return false;
            }

            void OnDataAvailable(object sender, EventArgs e)
            {
                if (DataAvailable != null)
                    DataAvailable(this, e);
            }

            public void Dispose()
            {
                if (Parent != null)
                {
                    Scope = DataScope.None;
                    Parent.DataAvailable -= OnDataAvailable;
                    Parent.Remove(this);
                    Parent = null;
                }
            }
        }

        private class MumbleProcess : IMumbleProcess
        {
            public event EventHandler<bool> ActiveStateChanged;
            public event EventHandler DataAvailable;
            public event EventHandler Verified;

            private MumbleProcessSubscriber[] subscribers;
            private bool refresh;
            private bool disposed;
            private bool verifying;

            public MumbleProcess(MumbleLink parent, int processId)
            {
                this.Parent = parent;
                this.Id = processId;
            }

            ~MumbleProcess()
            {
                Dispose();
            }

            public bool IsValid
            {
                get
                {
                    return Parent.IsValid;
                }
            }

            public MumbleLink Parent
            {
                get;
                private set;
            }

            public string LinkName
            {
                get
                {
                    return Parent.Name;
                }
            }

            public int Id
            {
                get;
                private set;
            }

            private uint _Tick;
            public uint Tick
            {
                get
                {
                    return _Tick;
                }
                private set
                {
                    if (_Tick != value)
                    {
                        _Tick = value;
                        LastUpdated = Environment.TickCount;
                        if (State != LinkState.Verified && ++State == LinkState.Verified)
                        {
                            if (Verified != null)
                                Verified(this, EventArgs.Empty);
                            //Verified.BeginInvoke(this, EventArgs.Empty, null, null);
                        }
                    }
                }
            }

            private DataScope scope;
            public DataScope Scope
            {
                get
                {
                    return scope;
                }
                set
                {
                    if (scope != value)
                    {
                        if (scope == MumbleMonitor.DataScope.None)
                        {
                            scope = value;
                            if (ActiveStateChanged != null)
                                ActiveStateChanged(this, true);
                        }
                        else if (value == MumbleMonitor.DataScope.None)
                        {
                            scope = value;
                            if (ActiveStateChanged != null)
                                ActiveStateChanged(this, false);
                        }
                        else
                        {
                            scope = value;
                        }
                    }
                }
            }

            public LinkState State
            {
                get;
                private set;
            }

            public ushort Subscribers
            {
                get;
                private set;
            }

            public int LastUpdated
            {
                get;
                private set;
            }

            public int Age
            {
                get
                {
                    if (!HasData)
                        return int.MaxValue;
                    return Environment.TickCount - LastUpdated;
                }
            }

            public bool HasData
            {
                get
                {
                    return DataScope != MumbleMonitor.DataScope.None;
                }
            }

            public byte[] Data
            {
                get;
                set;
            }

            public DataScope DataScope
            {
                get;
                private set;
            }

            public Settings.IAccount Account
            {
                get;
                set;
            }

            public bool IsVerified
            {
                get
                {
                    return State == LinkState.Verified;
                }
            }

            public void Verify()
            {
                lock (this)
                {
                    if (verifying)
                        return;
                    verifying = true;

                    State = LinkState.Unknown;
                }

                VerifyAsync();
            }

            private async void VerifyAsync()
            {
                while (!disposed)
                {
                    if (!await Query() && State == LinkState.Unknown)
                    {
                        if (!Parent.IsValid)
                            break;

                        //current link data isn't valid for this process; only need to check for 1 update
                        lock (this)
                        {
                            if (State == LinkState.Unknown)
                                State = LinkState.Pending;
                        }
                    }

                    if (IsVerified)
                        break;

                    //all accounts using this link will be overwriting each other

                    if (Parent.Processes > 1)
                    {
                        await Task.Delay(50);
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }

                verifying = false;
            }

            public IMumbleSubscriber Subscribe(DataScope scope)
            {
                return Add(scope);
            }

            public MumbleProcessSubscriber Add(DataScope scope)
            {
                lock (Parent)
                {
                    if (disposed)
                        return null;

                    var subscribers = this.subscribers;
                    MumbleProcessSubscriber s;

                    if (subscribers == null)
                    {
                        subscribers = new MumbleProcessSubscriber[]
                        {
                            s = new MumbleProcessSubscriber(this, scope),
                        };
                    }
                    else
                    {
                        var i = subscribers.Length;
                        if (i > 0)
                            Array.Resize<MumbleProcessSubscriber>(ref subscribers, i + 1);
                        else
                            subscribers = new MumbleProcessSubscriber[1];
                        subscribers[i] = s = new MumbleProcessSubscriber(this, scope);
                    }

                    this.subscribers = subscribers;
                    ++Subscribers;
                    Scope |= scope;

                    return s;
                }
            }

            public bool Remove(MumbleProcessSubscriber s)
            {
                lock (Parent)
                {
                    var subscribers = this.subscribers;
                    var i = IndexOf(subscribers, s);

                    if (i != -1)
                    {
                        --Subscribers;

                        if (subscribers.Length > 1)
                        {
                            subscribers = new MumbleProcessSubscriber[subscribers.Length - 1];
                            if (i > 0)
                                Array.Copy(this.subscribers, 0, subscribers, 0, i);
                            var l = this.subscribers.Length - i - 1;
                            if (l > 0)
                                Array.Copy(this.subscribers, i + 1, subscribers, i, l);
                            this.subscribers = subscribers;
                        }
                        else
                        {
                            this.subscribers = null;
                        }

                        refresh = true;
                        return true;
                    }

                    return false;
                }
            }

            private int IndexOf(MumbleProcessSubscriber[] array, MumbleProcessSubscriber s)
            {
                if (array != null)
                {
                    for (var i = array.Length - 1; i >= 0; --i)
                    {
                        if (object.ReferenceEquals(array[i], s))
                            return i;
                    }
                }
                return -1;
            }

            public void Push(DataBuffer buffer)
            {
                bool b;

                lock (this)
                {
                    b = _Tick != buffer.Ticks;

                    if (b || DataScope != buffer.Scope)
                    {
                        if (b)
                        {
                            _Tick = buffer.Ticks;
                            LastUpdated = Environment.TickCount;
                            b = State != LinkState.Verified && ++State == LinkState.Verified;
                        }

                        if (buffer.CanShare)
                        {
                            if (Data != null && Data.Length < buffer.Length)
                                Data = null;
                        }
                        else if (Data == null || Data.Length < buffer.Length)
                        {
                            Data = new byte[buffer.Length];
                        }

                        if (Data != null)
                        {
                            Array.Copy(buffer.Buffer, Data, buffer.Length);
                        }

                        DataScope = buffer.Scope;
                    }
                }

                if (b && Verified != null)
                    Verified(this, EventArgs.Empty);

                if (DataAvailable != null)
                    DataAvailable(this, EventArgs.Empty);
            }

            public async Task<T> GetData<T>(DataScope scope, int timeout) where T : struct
            {
                bool hasData = false;
                T data = default(T);

                EventHandler onData = delegate
                {
                    if (!hasData)
                    {
                        hasData = GetData<T>(scope, out data);
                    }
                };
                
                DataAvailable += onData;

                var t = Environment.TickCount;

                try
                {
                    do
                    {
                        await Query();

                        if (hasData)
                        {
                            break;
                        }
                        else if (timeout != -1 && Environment.TickCount - t > timeout || Scope < scope || disposed)
                        {
                            throw new TaskCanceledException();
                        }
                    }
                    while (!hasData);
                }
                finally
                {
                    DataAvailable -= onData;
                }

                return data;
            }

            public bool GetData<T>(DataScope scope, out T data) where T : struct
            {
                lock (this)
                {
                    if (Data != null)
                    {
                        if (DataScope < scope)
                        {
                            data = default(T);
                            return false;
                        }

                        var h = GCHandle.Alloc(Data, GCHandleType.Pinned);
                        try
                        {
                            data = (T)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(T));
                            return true;
                        }
                        finally
                        {
                            h.Free();
                        }
                    }
                }

                var buffer = Parent.Buffer;

                lock (buffer)
                {
                    if (buffer.Id == Id && buffer.Scope >= scope)
                    {
                        var h = GCHandle.Alloc(buffer.Buffer, GCHandleType.Pinned);
                        try
                        {
                            data = (T)Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof(T));
                            return true;
                        }
                        finally
                        {
                            h.Free();
                        }
                    }
                }

                data = default(T);
                return false;
            }

            public Task<bool> Query()
            {
                return Parent.Query(this.Id);
            }

            public DataScope Refresh()
            {
                lock (Parent)
                {
                    if (!refresh)
                        return Scope;
                    refresh = false;

                    var count = Subscribers;
                    var scope = MumbleMonitor.DataScope.None;

                    if (count == 0 || subscribers == null)
                    {
                        Scope = scope;
                        //this.active = null;

                        return scope;
                    }

                    var active = new MumbleProcessSubscriber[count];
                    //var j = 0;

                    for (var i = subscribers.Length - 1; i >= 0; --i)
                    {
                        var s = subscribers[i];

                        if (s.Scope > scope)
                        {
                            scope = s.Scope;
                            if (scope == MumbleMonitor.DataScope.Extended)
                                break;
                        }

                        //if (s.Scope != DataScope.None)
                        //{
                        //    active[j] = s;
                        //    scope |= s.Scope;

                        //    if (++j == count)
                        //    {
                        //        break;
                        //    }
                        //}
                    }

                    //if (j != count)
                    //    Array.Resize<MumbleProcessSubscriber>(ref active, j);

                    //this.active = active;
                    this.Scope = scope;

                    return scope;
                }
            }

            public void Dispose()
            {
                lock (Parent)
                {
                    if (!disposed)
                    {
                        disposed = true;

                        var subscribers = this.subscribers;

                        if (subscribers != null)
                        {
                            this.subscribers = null;

                            for (var i = subscribers.Length - 1; i >= 0; --i)
                            {
                                subscribers[i].Dispose();
                            }
                        }

                        Parent.Remove(this);
                    }
                }
            }
        }

        private class DataBuffer
        {
            public byte[] Buffer
            {
                get;
                set;
            }

            public DataScope Scope
            {
                get;
                set;
            }

            public ushort Length
            {
                get;
                set;
            }

            public bool CanShare
            {
                get;
                set;
            }

            public uint Ticks
            {
                get;
                set;
            }

            public uint Id
            {
                get;
                set;
            }
        }

        private class MumbleLink : IDisposable
        {
            private IntPtr _file;
            private IntPtr _view;
            private MumbleProcess[] processes;
            private DataBuffer buffer;
            private bool refresh;
            private Task<bool> task;

            public MumbleLink(MumbleMonitor parent, string name)
            {
                this.Parent = parent;
                this.Name = name;
                this.buffer = new DataBuffer();
                this.IsValid = name[0] != '0';
            }

            ~MumbleLink()
            {
                Dispose();
            }

            public bool IsValid
            {
                get;
                private set;
            }

            public MumbleMonitor Parent
            {
                get;
                private set;
            }

            public string Name
            {
                get;
                private set;
            }

            public DataScope Scope
            {
                get;
                private set;
            }

            /// <summary>
            /// Number of processes currently pulling data from the link
            /// </summary>
            public ushort Subscribers
            {
                get;
                private set;
            }

            /// <summary>
            /// Number of processes assigned to this link
            /// </summary>
            public ushort Processes
            {
                get;
                private set;
            }

            public DataBuffer Buffer
            {
                get
                {
                    return buffer;
                }
            }

            public bool IsOpened
            {
                get
                {
                    return _view != IntPtr.Zero;
                }
            }

            public bool Open()
            {
                if (IsOpened)
                    return true;
                if (!IsValid)
                    return false;

                _file = NativeMethods.OpenFileMapping(FileMapAccess.FileMapRead, false, this.Name);
                if (_file == IntPtr.Zero)
                {
                    _file = NativeMethods.CreateFileMapping(IntPtr.Zero, IntPtr.Zero, FileMapProtection.PageReadWrite, 0, MUMBLE_DATA_LENGTH, this.Name);
                    if (_file == IntPtr.Zero)
                    {
                        IsValid = false;
                        return false;
                    }
                }

                _view = NativeMethods.MapViewOfFile(_file, FileMapAccess.FileMapRead, 0, 0, MUMBLE_DATA_LENGTH);
                if (_view == IntPtr.Zero)
                {
                    Close();
                    return false;
                }

                return true;
            }

            public void Close()
            {
                if (_view != IntPtr.Zero)
                {
                    NativeMethods.UnmapViewOfFile(_view);
                    _view = IntPtr.Zero;
                }
                if (_file != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(_file);
                    _file = IntPtr.Zero;
                }
            }

            public MumbleProcess Add(int processId)
            {
                lock (this)
                {
                    var processes = this.processes;
                    MumbleProcess p;

                    if (processes == null)
                    {
                        processes = new MumbleProcess[]
                        {
                            p = new MumbleProcess(this, processId),
                        };
                    }
                    else
                    {
                        var i = IndexOf(processes, processId);

                        if (i == -1)
                        {
                            i = processes.Length;
                            if (i > 0)
                                Array.Resize<MumbleProcess>(ref processes, i + 1);
                            else
                                processes = new MumbleProcess[1];
                            processes[i] = p = new MumbleProcess(this, processId);
                        }
                        else
                        {
                            return processes[i];
                        }
                    }

                    p.ActiveStateChanged += process_ActiveStateChanged;

                    ++this.Processes;
                    this.processes = processes;
                    return p;
                }
            }

            public bool Remove(MumbleProcess p)
            {
                lock (this)
                {
                    var processes = this.processes;
                    var i = IndexOf(processes, p);

                    if (i != -1)
                    {
                        p.Scope = DataScope.None;
                        p.ActiveStateChanged -= process_ActiveStateChanged;

                        if (processes.Length > 1)
                        {
                            processes = new MumbleProcess[processes.Length - 1];
                            if (i > 0)
                                Array.Copy(this.processes, 0, processes, 0, i);
                            var l = this.processes.Length - i - 1;
                            if (l > 0)
                                Array.Copy(this.processes, i + 1, processes, i, l);
                            this.processes = processes;
                        }
                        else
                        {
                            Parent.Remove(this);
                        }

                        --this.Processes;

                        return true;
                    }

                    return false;
                }
            }

            void process_ActiveStateChanged(object sender, bool activated)
            {
                lock (this)
                {
                    if (activated)
                        ++Subscribers;
                    else
                        --Subscribers;

                    refresh = true;
                }
            }

            private int IndexOf(MumbleProcess[] array, int processId)
            {
                if (array != null)
                {
                    for (var i = array.Length - 1; i >= 0; --i)
                    {
                        if (array[i].Id == processId)
                            return i;
                    }
                }

                return -1;
            }

            private int IndexOf(MumbleProcess[] array, MumbleProcess p)
            {
                if (array != null)
                {
                    for (var i = array.Length - 1; i >= 0; --i)
                    {
                        if (object.ReferenceEquals(array[i], p))
                            return i;
                    }
                }

                return -1;
            }

            public DataScope Refresh()
            {
                lock (this)
                {
                    if (!refresh)
                        return Scope;
                    refresh = false;

                    var count = Subscribers;
                    var scope = MumbleMonitor.DataScope.None;

                    if (count == 0 || processes == null)
                    {
                        Scope = scope;
                        return scope;
                    }

                    //var active = new MumbleProcess[count];
                    //var j = 0;

                    for (var i = processes.Length - 1; i >= 0; --i)
                    {
                        //var p = processes[i];
                        var s = processes[i].Refresh();
                        if (s > scope)
                            scope = s;

                        //if (p.Refresh() != DataScope.None)
                        //{
                        //    active[j] = p;
                        //    scope |= p.Scope;

                        //    if (++j == count)
                        //    {
                        //        break;
                        //    }
                        //}
                    }

                    //if (j != count)
                    //    Array.Resize<MumbleProcess>(ref active, j);

                    this.Scope = scope;

                    return scope;
                }
            }

            public Task<bool> Query(int processId)
            {
                lock (this)
                {
                    if (task == null || task.IsCompleted)
                    {
                        task = Grab(processId);
                    }
                    return task;
                }
            }

            public async Task<bool> Grab(int processId)
            {
                if (Grab() != processId)
                {
                    await System.Threading.Tasks.Task.Delay(10);
                    return false;
                }
                return true;
            }

            public uint Grab()
            {
                MumbleProcess[] processes;
                int length;
                DataScope scope;

                lock (this)
                {
                    processes = this.processes;
                    if (processes == null)
                        return 0;

                    if (refresh)
                    {
                        Refresh();
                    }

                    scope = Scope;
                    if (scope == DataScope.Extended)
                        length = MUMBLE_DATA_LENGTH;
                    else
                        length = MUMBLE_DATA_LENGTH_BASIC;
                }

                if (Open())
                {
                    lock (buffer)
                    {
                        if (buffer.CanShare && Subscribers != 1)
                        {
                            for (var i = processes.Length - 1; i >= 0; --i)
                            {
                                var p = processes[i];
                                if (p.Id == buffer.Id)
                                {
                                    if (p.Data == null)
                                        p.Data = buffer.Buffer;
                                    break;
                                }
                            }
                        }

                        if (buffer.Length != length)
                        {
                            buffer.Length = (ushort)length;

                            if (buffer.Buffer == null || buffer.Buffer.Length < length)
                            {
                                buffer.Buffer = new byte[length];
                            }
                        }

                        Marshal.Copy(_view, buffer.Buffer, 0, length);

                        buffer.Ticks = BitConverter.ToUInt32(buffer.Buffer, MUMBLE_DATA_OFFSET_TICKS);
                        buffer.Id = BitConverter.ToUInt32(buffer.Buffer, MUMBLE_DATA_OFFSET_PROCESS_ID);
                        buffer.CanShare = processes.Length == 1;
                        buffer.Scope = scope;

                        for (var i = processes.Length - 1; i >= 0; --i)
                        {
                            var p = processes[i];
                            if (p.Id == buffer.Id)
                            {
                                p.Push(buffer);
                                break;
                            }
                        }

                        return buffer.Id;
                    }
                }

                return 0;
            }

            public void Dispose()
            {
                Close();
            }
        }

        private List<MumbleLink> links;

        public MumbleMonitor()
        {
            links = new List<MumbleLink>();
        }

        /// <summary>
        /// Adds the process to the mumble link with the specified name (multiple processes can share the same link)
        /// </summary>
        /// <param name="name">Name of the mumble link, or null to use the default name</param>
        /// <param name="pid">Process ID to match with the link data</param>
        public IMumbleProcess Add(string name, int pid, Settings.IAccount account)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(name))
                    name = MUMBLE_DEFAULT_NAME;
                else if (name.Length > 2 && name[0] == '"' && name[name.Length - 1] == '"')
                    name = name.Substring(1, name.Length - 2);
                var l = GetLink(name);
                var p = l.Add(pid);

                p.Account = account;

                return p;
            }
        }

        private bool Remove(MumbleLink link)
        {
            lock (this)
            {
                return links.Remove(link);
            }
        }

        private MumbleLink GetLink(string name)
        {
            for (var i = links.Count - 1; i >= 0; --i)
            {
                if (links[i].Name == name)
                {
                    return links[i];
                }
            }

            var l = new MumbleLink(this, name);
            links.Add(l);
            return l;
        }

    }
}
