using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class Win32Handles
    {
        private const byte THREADS = 2;
        private const int HANDLE_TYPES_LENGTH = 3;

        public enum MatchMode : byte
        {
            Contains = 0,
            Exact = 1,
            StartsWith = 2,
            EndsWith = 3,
        }

        public enum HandleType : short
        {
            Any = -1,
            Mutex = 0,
            File = 1,
            Section = 2,
        }

        public enum CallbackResponse : byte
        {
            Continue,
            Return,
            Abort,
        }

        private static short[] _handleTypes;

        public struct TypeInfo
        {
            public TypeInfo(int index, string name, OBJECT_TYPE_INFORMATION info)
            {
                this.Index = index;
                this.Name = name;
                this.Info = info;
            }

            public int Index;
            public string Name;
            public OBJECT_TYPE_INFORMATION Info;
        }

        public interface IObjectHandle
        {
            void Kill();

            int PID
            {
                get;
            }
        }

        public interface IObject
        {
            string Name
            {
                get;
            }

            UIntPtr PID
            {
                get;
            }

            HandleType Type
            {
                get;
            }

            UIntPtr Handle
            {
                get;
            }
        }

        private class ObjectData : IObject
        {
            public UIntPtr pid;
            public ushort typeIndex;
            public HandleType type;
            public string name;
            public UIntPtr handle;

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public UIntPtr PID
            {
                get
                {
                    return pid;
                }
            }

            public HandleType Type
            {
                get
                {
                    if (type == HandleType.Any)
                        return GetHandleType(typeIndex);
                    else
                        return type;
                }
            }

            public UIntPtr Handle
            {
                get
                {
                    return handle;
                }
            }
        }

        private class ObjectHandle : IObjectHandle
        {
            protected UIntPtr processId, handle;

            public ObjectHandle(UIntPtr processId, UIntPtr handle)
            {
                this.processId = processId;
                this.handle = handle;
            }

            public void Kill()
            {
                using (var p = Process.GetProcessById((int)processId.GetValue()))
                {
                    IntPtr h = IntPtr.Zero;
                    try
                    {
                        if (!NativeMethods.DuplicateHandle(p.Handle, handle, NativeMethods.GetCurrentProcess(), out h, 0, false, DuplicateOptions.DUPLICATE_CLOSE_SOURCE))
                            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                    }
                    finally
                    {
                        if (h != IntPtr.Zero)
                            NativeMethods.CloseHandle(h);
                    }
                }
            }

            public int PID
            {
                get
                {
                    return (int)processId.GetValue();
                }
            }
        }

        private class Buffer : IDisposable
        {
            public Buffer(int length)
            {
                handle = Marshal.AllocHGlobal(this.length = length);
            }

            public IntPtr handle;
            public int length;

            public void ReAlloc(int length)
            {
                handle = Marshal.ReAllocHGlobal(handle, (IntPtr)(this.length = length));
            }

            public void Dispose()
            {
                if (handle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(handle);
                    handle = IntPtr.Zero;
                }
            }
        }

        private static string GetFileName(Buffer buffer, UIntPtr processId, UIntPtr handle)
        {
            var _processHandle = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, processId);
            var _handle = IntPtr.Zero;

            try
            {
                if (!NativeMethods.DuplicateHandle(_processHandle, handle, NativeMethods.GetCurrentProcess(), out _handle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS))
                    return null;

                //more specifically, avoid pipes, which can block until closed
                if (NativeMethods.GetFileType(_handle) != FileType.FileTypeDisk)
                    return null;

                int length;
                var l = buffer.length / 2;

                while (true)
                {
                    length = NativeMethods.GetFinalPathNameByHandle(_handle, buffer.handle, l, 0);
                    if (length > l)
                    {
                        l = length;
                        buffer.ReAlloc(length * 2);
                    }
                    else
                        break;
                }

                if (length > 0 && length < buffer.length)
                {
                    return Marshal.PtrToStringUni(buffer.handle, length);
                }

                return null;
            }
            finally
            {
                if (_handle != IntPtr.Zero)
                    NativeMethods.CloseHandle(_handle);
                if (_processHandle != IntPtr.Zero)
                    NativeMethods.CloseHandle(_processHandle);
            }
        }

        private static string GetObjectName(Buffer buffer, UIntPtr processId, UIntPtr handle, bool queryInfo)
        {
            var _processHandle = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, processId);
            var _handle = IntPtr.Zero;

            try
            {
                if (!NativeMethods.DuplicateHandle(_processHandle, handle, NativeMethods.GetCurrentProcess(), out _handle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS))
                    return null;

                int nameLength;

                if (queryInfo)
                {
                    OBJECT_BASIC_INFORMATION basicInfo;
                    nameLength = 0;

                    //warning: not fully implemented in Wine and will always return a 0 length

                    if (NativeMethods.NtQueryObject(_handle, ObjectInformationClass.ObjectBasicInformation, out basicInfo, Marshal.SizeOf(typeof(OBJECT_BASIC_INFORMATION)), ref nameLength) != NtStatus.Success)
                        return null;

                    nameLength = basicInfo.NameInformationLength;

                    //var _basic = IntPtr.Zero;
                    //OBJECT_BASIC_INFORMATION basicInfo = new OBJECT_BASIC_INFORMATION();
                    //nameLength = 0;

                    //try
                    //{
                    //    _basic = Marshal.AllocHGlobal(Marshal.SizeOf(basicInfo));

                    //    if (NativeMethods.NtQueryObject(_handle, ObjectInformationClass.ObjectBasicInformation, _basic, Marshal.SizeOf(basicInfo), ref nameLength) != NtStatus.Success)
                    //        return null;

                    //    basicInfo = (OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(_basic, basicInfo.GetType());
                    //    nameLength = basicInfo.NameInformationLength;
                    //}
                    //finally
                    //{
                    //    if (_basic != IntPtr.Zero)
                    //        Marshal.FreeHGlobal(_basic);
                    //}

                    if (nameLength > buffer.length)
                    {
                        buffer.ReAlloc(nameLength);
                    }
                    else if (nameLength == 0)
                    {
                        return null;
                    }
                }
                else
                {
                    nameLength = buffer.length;
                }

                var querying = true;

                do
                {
                    var r = NativeMethods.NtQueryObject(_handle, ObjectInformationClass.ObjectNameInformation, buffer.handle, buffer.length, ref nameLength);

                    switch (r)
                    {
                        case NtStatus.Success:

                            querying = false;

                            break;
                        case NtStatus.InvalidInfoClass:
                        case NtStatus.BufferOverflow:

                            if (nameLength > buffer.length)
                                buffer.ReAlloc(nameLength);
                            else
                                return null;

                            break;
                        default:

                            return null;
                    }
                }
                while (querying);

                var nameInfo = (OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(buffer.handle, typeof(OBJECT_NAME_INFORMATION));

                try
                {
                    if (nameInfo.Name.Length == 0)
                        return null;
                    return nameInfo.Name.ToString();
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return null;
            }
            finally
            {
                if (_handle != IntPtr.Zero)
                    NativeMethods.CloseHandle(_handle);
                if (_processHandle != IntPtr.Zero)
                    NativeMethods.CloseHandle(_processHandle);
            }
        }

        private static short GetHandleTypeIndex(HandleType t)
        {
            switch (t)
            {
                case HandleType.Any:

                    return 0;

                case HandleType.File:
                case HandleType.Mutex:
                    
                    if (_handleTypes == null)
                        _handleTypes = GetHandleTypes();
                    return _handleTypes[(short)t];
            }

            return -1;
        }

        private static HandleType GetHandleType(ushort index)
        {
            if (_handleTypes == null)
                _handleTypes = GetHandleTypes();

            for (var i = 0; i < _handleTypes.Length; i++)
            {
                if (_handleTypes[i] == index)
                    return (HandleType)i;
            }

            return HandleType.Any;
        }

        /// <summary>
        /// Searches for a handle
        /// </summary>
        /// <param name="pids">Process IDs to include or null for any</param>
        /// <param name="type">Handle type</param>
        /// <param name="nameCallback">Callback function</param>
        /// <param name="includeNulls">Includes null names in callback</param>
        /// <returns>A handle, if found</returns>
        public static IObjectHandle GetHandle(UIntPtr[] pids, HandleType type, Func<IObject, CallbackResponse> nameCallback, bool includeNulls = false, byte threads = THREADS)
        {
            var infoClass = SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation;
            int infoLength = 0x10000;

            Buffer[] buffers = null;

            var _info = Marshal.AllocHGlobal(infoLength);

            try
            {
                //CNST_SYSTEM_HANDLE_INFORMATION is limited to 16-bit process IDs
                var querying = true;
                var length = 0;

                do
                {
                    var r = NativeMethods.NtQuerySystemInformation(infoClass, _info, infoLength, ref length);

                    switch (r)
                    {
                        case NtStatus.Success:

                            querying = false;

                            break;
                        case NtStatus.InfoLengthMismatch:

                            _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = length + 0x1000));

                            break;
                        case NtStatus.InvalidInfoClass:

                            if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                            {
                                infoClass = SYSTEM_INFORMATION_CLASS.SystemHandleInformation;
                            }
                            else
                            {
                                throw new NotSupportedException("SystemHandleInformation not supported");
                            }

                            break;
                        default:

                            throw new Exception(r.ToString("x"));
                    }
                }
                while (querying);

                IntPtr _handle;
                long handleCount;
                Type infoType;

                if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                {
#if x86
                    handleCount = Marshal.ReadInt32(_info);
#elif x64
                    handleCount = Marshal.ReadInt64(_info);
#else
                    if (IntPtr.Size == 4)
                        handleCount = Marshal.ReadInt32(_info);
                    else
                        handleCount = Marshal.ReadInt64(_info);
#endif
                    _handle = _info + IntPtr.Size * 2;
                    infoType = typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX);
                }
                else
                {
                    //SystemHandleInformation count is 32bit
                    handleCount = Marshal.ReadInt32(_info);

                    _handle = _info + IntPtr.Size;
                    infoType = typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO);
                }

                var infoSize = Marshal.SizeOf(infoType);
                var data = new ObjectData[threads];

                buffers = new Buffer[threads];

                for (var j = 0; j < threads; j++)
                {
                    buffers[j] = new Buffer(512);
                    data[j] = new ObjectData()
                        {
                            type = type,
                        };
                }

                var canQuery = infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation && !Settings.IsRunningWine;
                var ti = GetHandleTypeIndex(type);

                //warning: NtQueryObject can cause a deadlock when querying an item that is waiting

                var loop = Util.Loop.For(0, handleCount, threads, 1000,
                    delegate(byte thread, long i, Util.Loop.IState state)
                    {
                        var handleInfo = Marshal.PtrToStructure((IntPtr)(_handle.GetValue() + i * infoSize), infoType);

                        UIntPtr pid, h;
                        var bpid = pids == null;
                        ushort t;

                        if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                        {
                            var info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)handleInfo;

                            pid = info.UniqueProcessId;
                            h = info.HandleValue;
                            t = info.ObjectTypeIndex;

                            if (!bpid)
                            {
                                for (var j = 0; j < pids.Length; ++j)
                                {
                                    if (pid == pids[j])
                                    {
                                        bpid = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO)handleInfo;

                            //try to recover 32-bit processId from 16-bit info
                            pid = (UIntPtr)info.UniqueProcessId;
                            h = (UIntPtr)info.HandleValue;
                            t = info.ObjectTypeIndex;

                            if (!bpid)
                            {
                                for (var j = 0; j < pids.Length; ++j)
                                {
                                    if (info.UniqueProcessId == (ushort)pids[j])
                                    {
                                        pid = pids[j];
                                        bpid = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!bpid || ti > 0 && t != ti)
                            return;

                        var d = data[thread];

                        switch (type)
                        {
                            case HandleType.File:

                                d.name = GetFileName(buffers[thread], pid, h);

                                break;
                            default:

                                d.name = GetObjectName(buffers[thread], pid, h, canQuery);

                                break;
                        }

                        if (d.name != null || includeNulls)
                        {
                            d.pid = pid;
                            d.typeIndex = t;
                            d.handle = h;

                            switch (nameCallback(d))
                            {
                                case CallbackResponse.Abort:

                                    state.Abort();

                                    break;
                                case CallbackResponse.Return:

                                    state.Return(new ObjectHandle(pid, h));

                                    break;
                            }
                        }
                    });

                loop.Wait();

                return (ObjectHandle)loop.Result;
            }
            finally
            {
                Marshal.FreeHGlobal(_info);
                if (buffers != null)
                {
                    foreach (var b in buffers)
                    {
                        using (b) { }
                    }
                }
            }
        }

        public static IObjectHandle GetHandle(int processId, HandleType type, string objectName, MatchMode match)
        {
            Func<string, bool> f;

            switch (match)
            {
                case MatchMode.Contains:
                    f = delegate(string s)
                    {
                        return s.IndexOf(objectName) != -1;
                    };
                    break;
                case MatchMode.EndsWith:
                    f = delegate(string s)
                    {
                        if (s.Length < objectName.Length)
                            return false;

                        var b = true;
                        var j = s.Length - objectName.Length;

                        for (var i = objectName.Length - 1; i >= 0; --i)
                        {
                            if (s[j+i] != objectName[i])
                            {
                                b = false;
                                break;
                            }
                        }

                        return b;
                    };
                    break;
                case MatchMode.Exact:
                    f = delegate(string s)
                    {
                        return s.Equals(objectName);
                    };
                    break;
                case MatchMode.StartsWith:
                    f = delegate(string s)
                    {
                        if (s.Length < objectName.Length)
                            return false;

                        var b = true;

                        for (var i = 0; i < objectName.Length; ++i)
                        {
                            if (s[i] != objectName[i])
                            {
                                b = false;
                                break;
                            }
                        }

                        return b;
                    };
                    break;
                default:
                    throw new NotSupportedException();
            }

            return GetHandle(processId, type, objectName, f);
        }

        public static IObjectHandle GetHandle(int processId, HandleType type, string objectName, Func<string, bool> objectNameCallback, byte threads = THREADS)
        {
            var infoClass = SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation;
            int infoLength = 0x10000;
            var _processId = (UIntPtr)processId;
            var _type = GetHandleTypeIndex(type);
            Buffer[] buffers = null;

            var _info = Marshal.AllocHGlobal(infoLength);

            try
            {
                //CNST_SYSTEM_HANDLE_INFORMATION is limited to 16-bit process IDs
                var querying = true;
                var length = 0;

                do
                {
                    var r = NativeMethods.NtQuerySystemInformation(infoClass, _info, infoLength, ref length);

                    switch (r)
                    {
                        case NtStatus.Success:

                            querying = false;

                            break;
                        case NtStatus.InfoLengthMismatch:

                            _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = length + 0x1000));

                            break;
                        case NtStatus.InvalidInfoClass:

                            if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                            {
                                infoClass = SYSTEM_INFORMATION_CLASS.SystemHandleInformation;
                            }
                            else
                            {
                                throw new NotSupportedException("SystemHandleInformation not supported");
                            }

                            break;
                        default:

                            throw new Exception(r.ToString("x"));
                    }
                }
                while (querying);

                IntPtr _handle;
                long handleCount;
                Type infoType;

                if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                {
#if x86
                    handleCount = Marshal.ReadInt32(_info);
#elif x64
                    handleCount = Marshal.ReadInt64(_info);
#else
                    if (IntPtr.Size == 4)
                        handleCount = Marshal.ReadInt32(_info);
                    else
                        handleCount = Marshal.ReadInt64(_info);
#endif
                    _handle = _info + IntPtr.Size * 2;
                    infoType = typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX);
                }
                else
                {
                    //SystemHandleInformation count is 32bit
                    handleCount = Marshal.ReadInt32(_info);

                    _handle = _info + IntPtr.Size;
                    infoType = typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO);
                }

                var infoSize = Marshal.SizeOf(infoType);

                buffers = new Buffer[threads];

                for (var j = 0; j < threads; j++)
                {
                    buffers[j] = new Buffer(256);
                }

                var canQuery = infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation && !Settings.IsRunningWine;

                //warning: NtQueryObject can cause a deadlock when querying an item that is waiting

                var loop = Util.Loop.For(0, handleCount, threads, 1000,
                    delegate(byte thread, long i, Util.Loop.IState state)
                    {
                        var handleInfo = Marshal.PtrToStructure((IntPtr)(_handle.GetValue() + i * infoSize), infoType);
                        
                        UIntPtr pid, h;
                        ushort t;

                        if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                        {
                            var info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)handleInfo;

                            pid = info.UniqueProcessId;
                            h = info.HandleValue;
                            t = info.ObjectTypeIndex;
                        }
                        else
                        {
                            var info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO)handleInfo;

                            //try to recover 32-bit processId from 16-bit info
                            if (info.UniqueProcessId == (ushort)processId)
                                pid = (UIntPtr)processId;
                            else
                                pid = (UIntPtr)info.UniqueProcessId;
                            h = (UIntPtr)info.HandleValue;
                            t = info.ObjectTypeIndex;
                        }

                        if (_type > 0 && _type != t)
                            return;
                        if (processId > 0 && pid != _processId)
                            return;

                        string name;

                        switch (type)
                        {
                            case HandleType.File:

                                name = GetFileName(buffers[thread], pid, h);

                                break;
                            default:

                                name = GetObjectName(buffers[thread], pid, h, canQuery);

                                break;
                        }

                        if (name != null && objectNameCallback(name))
                        {
                            state.Return(new ObjectHandle(pid, h));
                        }
                    });

                loop.Wait();

                return (ObjectHandle)loop.Result;
            }
            finally
            {
                Marshal.FreeHGlobal(_info);
                if (buffers != null)
                {
                    foreach (var b in buffers)
                    {
                        using (b) { }
                    }
                }
            }
        }

        public static int FindType(int processId, UIntPtr handle, byte threads = THREADS)
        {
            var infoClass = SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation;
            int infoLength = 0x10000;
            var _processId = (UIntPtr)processId;
            Buffer[] buffers = null;

            var _info = Marshal.AllocHGlobal(infoLength);

            try
            {
                //CNST_SYSTEM_HANDLE_INFORMATION is limited to 16-bit process IDs
                var querying = true;
                var length = 0;

                do
                {
                    var r = NativeMethods.NtQuerySystemInformation(infoClass, _info, infoLength, ref length);

                    switch (r)
                    {
                        case NtStatus.Success:

                            querying = false;

                            break;
                        case NtStatus.InfoLengthMismatch:

                            _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = length + 0x1000));

                            break;
                        case NtStatus.InvalidInfoClass:

                            if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                            {
                                infoClass = SYSTEM_INFORMATION_CLASS.SystemHandleInformation;
                            }
                            else
                            {
                                throw new NotSupportedException("SystemHandleInformation not supported");
                            }

                            break;
                        default:

                            throw new Exception(r.ToString("x"));
                    }
                }
                while (querying);

                IntPtr _handle;
                long handleCount;
                Type infoType;

                if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                {
#if x86
                    handleCount = Marshal.ReadInt32(_info);
#elif x64
                    handleCount = Marshal.ReadInt64(_info);
#else
                    if (IntPtr.Size == 4)
                        handleCount = Marshal.ReadInt32(_info);
                    else
                        handleCount = Marshal.ReadInt64(_info);
#endif
                    _handle = _info + IntPtr.Size * 2;
                    infoType = typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX);
                }
                else
                {
                    //SystemHandleInformation count is 32bit
                    handleCount = Marshal.ReadInt32(_info);

                    _handle = _info + IntPtr.Size;
                    infoType = typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO);
                }

                var infoSize = Marshal.SizeOf(infoType);

                buffers = new Buffer[threads];

                for (var j = 0; j < threads; j++)
                {
                    buffers[j] = new Buffer(256);
                }

                var canQuery = infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation && !Settings.IsRunningWine;

                //warning: NtQueryObject can cause a deadlock when querying an item that is waiting

                var loop = Util.Loop.For(0, handleCount, threads, 1000,
                    delegate(byte thread, long i, Util.Loop.IState state)
                    {
                        var handleInfo = Marshal.PtrToStructure((IntPtr)(_handle.GetValue() + i * infoSize), infoType);

                        UIntPtr pid, h;
                        ushort t;

                        if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                        {
                            var info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)handleInfo;

                            pid = info.UniqueProcessId;
                            h = info.HandleValue;
                            t = info.ObjectTypeIndex;
                        }
                        else
                        {
                            var info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO)handleInfo;

                            //try to recover 32-bit processId from 16-bit info
                            if (info.UniqueProcessId == (ushort)processId)
                                pid = (UIntPtr)processId;
                            else
                                pid = (UIntPtr)info.UniqueProcessId;
                            h = (UIntPtr)info.HandleValue;
                            t = info.ObjectTypeIndex;
                        }

                        if (processId > 0 && pid != _processId)
                            return;

                        if (h == handle)
                        {
                            state.Return(t);
                        }
                    });

                loop.Wait();

                return loop.Result is ushort ? (ushort)loop.Result : -1;
            }
            finally
            {
                Marshal.FreeHGlobal(_info);
                if (buffers != null)
                {
                    foreach (var b in buffers)
                    {
                        using (b) { }
                    }
                }
            }
        }

        private static HandleType GetHandleType(string name)
        {
            switch (name)
            {
                case "File":

                    return HandleType.File;

                case "Mutant":

                    return HandleType.Mutex;

                case "Section":

                    return HandleType.Section;
            }

            return HandleType.Any;
        }

        /// <summary>
        /// Queries handle type indexes
        /// </summary>
        /// <param name="types">Optional output for all types</param>
        /// <returns>Handle types</returns>
        private static short[] GetHandleTypes(List<TypeInfo> types = null)
        {
            int infoLength = 0x3000;
            var _info = Marshal.AllocHGlobal(infoLength);
            var handleTypes = new short[HANDLE_TYPES_LENGTH];

            try
            {
                var querying = true;
                var length = 0;

                do
                {
                    var r = NativeMethods.NtQueryObject(IntPtr.Zero, ObjectInformationClass.ObjectAllTypesInformation, _info, infoLength, ref length);

                    switch (r)
                    {
                        case NtStatus.Success:

                            querying = false;

                            break;
                        case NtStatus.InfoLengthMismatch:

                            _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = length + 0x1000));

                            break;
                        default:

                            throw new Exception(r.ToString("x"));
                    }
                }
                while (querying);

                var infoType = typeof(OBJECT_TYPE_INFORMATION);
                var infoSize = Marshal.SizeOf(infoType);
                IntPtr _handle;
                long handleCount;

                var b = types != null;

                handleCount = Marshal.ReadInt32(_info);
                _handle = _info + IntPtr.Size;

                var offset = 0;
                const short INDEX_OFFSET = 2;

                for (var i = 0; i < handleCount; i++)
                {
                    var t = (OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure((IntPtr)(_handle.GetValue() + offset), infoType);
                    var s = t.TypeName.ToString();
                    var ht = GetHandleType(s);

                    if (ht != HandleType.Any)
                    {
                        handleTypes[(short)ht] = (short)(i + INDEX_OFFSET);
                    }

                    if (b)
                        types.Add(new TypeInfo(i + INDEX_OFFSET, s, t));

                    //offset includes padding for byte alignment
                    offset += infoSize + (t.TypeName.MaximumLength + IntPtr.Size - 1) & ~(IntPtr.Size - 1);
                }

                return handleTypes;
            }
            finally
            {
                Marshal.FreeHGlobal(_info);
            }
        }


        public static class HandleMonitor
        {
            /// <summary>
            /// Occurs after scanning for handles
            /// </summary>
            public static event EventHandler Tick;

            private static Queue<QueuedItem> queue;
            private static bool isActive;

            public static bool IsActive
            {
                get
                {
                    return isActive;
                }
            }

            public interface IHandle
            {
                IntPtr Handle
                {
                    get;
                }

                /// <summary>
                /// Returns the name associated with the handle
                /// </summary>
                /// <param name="identity">Optional identity required when the process is running under a different user</param>
                string GetName(Security.Impersonation.IIdentity identity = null);

                HandleType Type
                {
                    get;
                }
            }

            private class HandleInfo : IHandle
            {
                public string name;
                public Func<string> getName;
                public IntPtr handle;
                public short type;

                public IntPtr Handle
                {
                    get
                    {
                        return handle;
                    }
                }

                public string GetName(Security.Impersonation.IIdentity identity = null)
                {
                    if (getName != null)
                    {
                        try
                        {
                            if (identity != null)
                            {
                                using (identity.Impersonate())
                                {
                                    name = getName();
                                }
                            }
                            else
                            {
                                name = getName();
                            }
                        }
                        catch
                        {
                            name = null;
                        }
                        getName = null;
                    }

                    return name;
                }

                public HandleType Type
                {
                    get
                    {
                        return GetHandleType((ushort)type);
                    }
                }
            }

            public interface IMonitor : IDisposable
            {
                event EventHandler<IHandle> HandleAdded;

                void Start();

                /// <summary>
                /// Type of handle being watched
                /// </summary>
                HandleType Type
                {
                    get;
                }

                /// <summary>
                /// ID of process being watched
                /// </summary>
                int PID
                {
                    get;
                }

                /// <summary>
                /// Number of times handles for the process was found
                /// </summary>
                ushort Cycles
                {
                    get;
                }

                /// <summary>
                /// Waits for one cycle of the handle monitor to complete
                /// </summary>
                /// <param name="timeout">-1 to wait indefinitely</param>
                /// <returns>False if no cycle occured</returns>
                bool Wait(int timeout = -1);
            }

            static HandleMonitor()
            {
                queue = new Queue<QueuedItem>();
            }

            private class QueuedItem
            {
                public MonitorClient client;
                public bool remove;
            }

            private class Monitor
            {
                public MonitorProcess[] monitors;

                public Monitor()
                {
                    monitors = new MonitorProcess[0];
                }

                public int IndexOf(int pid)
                {
                    for (var i = 0; i < monitors.Length; ++i)
                    {
                        if (monitors[i].pid == pid)
                            return i;
                    }
                    return -1;
                }

                public int Add(MonitorClient m)
                {
                    var i = IndexOf(m.pid);

                    m.typeIndex = GetHandleTypeIndex(m.Type);

                    if (i == -1)
                    {
                        i = monitors.Length;
                        Array.Resize<MonitorProcess>(ref monitors, i + 1);
                        monitors[i] = new MonitorProcess()
                        {
                            pid = m.pid,
                            clients = new MonitorClient[] { m },
                            types = m.typeIndex != 0 ? new short[] { m.typeIndex } : new short[0],
                            any = m.typeIndex == 0,
                            skip = m.action == MonitorClient.Action.Skip,
                        };
                    }
                    else
                    {
                        monitors[i].Add(m);
                        monitors[i].Add(m.typeIndex);

                        if (m.action != MonitorClient.Action.Skip)
                        {
                            //monitor already existed, flag to push all handles
                            monitors[i].all = true;
                        }
                        else
                        {
                            monitors[i].skip = true;
                        }
                    }

                    return i;
                }

                public void Remove(MonitorClient m)
                {
                    var i = IndexOf(m.pid);

                    if (i != -1)
                    {
                        if (monitors[i].Remove(m))
                        {
                            if (monitors[i].clients.Length == 0)
                            {
                                if (i < monitors.Length - 1)
                                {
                                    Array.Copy(monitors, i + 1, monitors, i, monitors.Length - i - 1);
                                }

                                Array.Resize(ref monitors, monitors.Length - 1);
                            }
                            else
                            {
                                var shared = false;

                                for (var j = 0; j < monitors[i].clients.Length; j++)
                                {
                                    if (monitors[i].clients[j].typeIndex == m.typeIndex)
                                    {
                                        shared = true;
                                        break;
                                    }
                                }

                                if (!shared)
                                {
                                    monitors[i].Remove(m.typeIndex);
                                }
                            }
                        }
                    }
                }

                public void Reset()
                {
                    for (var i = 0; i < monitors.Length; i++)
                    {
                        monitors[i].Reset();
                    }
                }
            }

            private class LinkedNode
            {
                public uint handle;
                public LinkedNode previous, next;
            }

            private class MonitorProcess
            {
                public int pid;
                public short[] types;
                public bool any;
                public MonitorClient[] clients;
                public LinkedNode handles;
                public bool all;
                public bool skip;

                public MonitorProcess()
                {
                    types = new short[0];
                    clients = new MonitorClient[0];
                    handles = new LinkedNode();
                }

                public int IndexOf(short type)
                {
                    for (var i = 0; i < types.Length; ++i)
                    {
                        if (types[i] == type)
                            return i;
                    }
                    return -1;
                }

                public bool Contains(short type)
                {
                    if (any)
                    {
                        return true;
                    }
                    else
                    {
                        return IndexOf(type) != -1;
                    }
                }

                public void Add(short type)
                {
                    if (type == 0)
                    {
                        any = true;
                        return;
                    }

                    var i = IndexOf(type);

                    if (i == -1)
                    {
                        i = types.Length;
                        Array.Resize<short>(ref types, i + 1);
                        types[i] = type;
                    }
                }

                public void Remove(short type)
                {
                    if (type == 0)
                    {
                        any = false;
                        return;
                    }

                    var i = IndexOf(type);

                    if (i != -1)
                    {
                        if (i < types.Length - 1)
                        {
                            Array.Copy(types, i + 1, types, i, types.Length - i - 1);
                        }

                        Array.Resize(ref types, types.Length - 1);
                    }
                }

                public void Add(MonitorClient m)
                {
                    var i = clients.Length;
                    Array.Resize<MonitorClient>(ref clients, i + 1);
                    clients[i] = m;
                }

                public bool Remove(MonitorClient m)
                {
                    for (var i = 0; i < clients.Length; i++)
                    {
                        if (object.ReferenceEquals(clients[i], m))
                        {
                            if (i < clients.Length - 1)
                            {
                                Array.Copy(clients, i + 1, clients, i, clients.Length - i - 1);
                            }

                            Array.Resize(ref clients, clients.Length - 1);

                            return true;
                        }
                    }
                    return false;
                }

                public void Push(short type, ulong handle, HandleInfo h, bool added)
                {
                    for (var i = 0; i < clients.Length; i++)
                    {
                        if ((added || clients[i].action == MonitorClient.Action.All) && clients[i].action != MonitorClient.Action.Skip)
                        {
                            if (clients[i].typeIndex == type || clients[i].typeIndex == 0)
                            {
                                clients[i].Push(h);
                            }
                        }
                    }
                }

                public void Reset()
                {
                    if (all || skip)
                    {
                        all = false;
                        skip = false;

                        for (var i = 0; i < clients.Length; i++)
                        {
                            clients[i].action = MonitorClient.Action.None;
                        }
                    }
                }
            }

            private class MonitorClient : IMonitor
            {
                public event EventHandler<IHandle> HandleAdded;

                public enum Action : byte
                {
                    None = 0,
                    All,
                    Skip
                }

                public int pid;
                public short typeIndex;
                public bool disposed;
                public bool started;
                public Action action;
                public ushort cycles;

                public MonitorClient(HandleType type, int pid, bool ignoreExisting)
                {
                    this.Type = type;
                    this.pid = pid;

                    if (ignoreExisting)
                        action = Action.Skip;
                    else
                        action = Action.All;
                }

                public int PID
                {
                    get
                    {
                        return (int)pid;
                    }
                }

                public ushort Cycles
                {
                    get
                    {
                        return cycles;
                    }
                }

                public HandleType Type
                {
                    get;
                    private set;
                }

                public void Start()
                {
                    if (started || disposed)
                        return;
                    started = true;

                    lock (queue)
                    {
                        queue.Enqueue(new QueuedItem()
                        {
                            client = this,
                        });
                        StartMonitor();
                    }
                }

                public void Push(HandleInfo h)
                {
                    if (HandleAdded != null)
                    {
                        try
                        {
                            HandleAdded(this, h);
                        }
                        catch { }
                    }
                }

                public bool Wait(int timeout = -1)
                {
                    var ok = false;
                    var t = timeout != -1 ? Environment.TickCount : 0;

                    EventHandler onTick = delegate
                    {
                        ok = true;
                    };

                    Tick += onTick;

                    try
                    {
                        while (!ok)
                        {
                            System.Threading.Thread.Sleep(100);

                            if (timeout != -1 && Environment.TickCount - t > timeout || !isActive)
                                break;
                        }
                    }
                    finally
                    {
                        Tick -= onTick;
                    }

                    return ok;
                }

                public void Dispose()
                {
                    if (disposed)
                        return;
                    disposed = true;

                    HandleAdded = null;

                    if (started)
                    {
                        lock (queue)
                        {
                            if (isActive)
                            {
                                queue.Enqueue(new QueuedItem()
                                {
                                    client = this,
                                    remove = true,
                                });
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Creates a monitor
            /// </summary>
            /// <param name="type">Type of handles to watch for</param>
            /// <param name="pid">Process to monitor</param>
            /// <param name="ignoreExisting">True to not report handles that were already created prior to the monitor starting</param>
            public static IMonitor Create(HandleType type, int pid, bool ignoreExisting = true)
            {
                var t = GetHandleTypeIndex(type);

                if (t == -1)
                {
                    throw new NotSupportedException("Handle type \"" + type + "\" was not available");
                }

                var m = new MonitorClient(type, pid, ignoreExisting)
                {
                    typeIndex = t,
                };

                return m;
            }

            private static void StartMonitor()
            {
                if (!isActive)
                {
                    isActive = true;
                    Task.Run(new Action(DoMonitor));
                }
            }

            private static async void DoMonitor()
            {
                await Task.Delay(100);

                var infoClass = SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation;
                int infoLength = 0x10000;
                var _info = IntPtr.Zero;
                var infoSize = Marshal.SizeOf(typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));
                var extended = true;

                var monitor = new Monitor();
                var data = new HandleInfo();
                Buffer buffer = null;
                var hasAll = false;
                byte empty = 0;

                try
                {
                    _info = Marshal.AllocHGlobal(infoLength);
                    buffer = new Buffer(512);

                    while (true)
                    {
                        #region Queue

                        lock (queue)
                        {
                            if (queue.Count > 0)
                            {
                                do
                                {
                                    var q = queue.Dequeue();

                                    if (q.remove)
                                    {
                                        monitor.Remove(q.client);
                                    }
                                    else if (!q.client.disposed)
                                    {
                                        var m = monitor.Add(q.client);

                                        if (monitor.monitors[m].all || q.client.action == MonitorClient.Action.Skip)
                                            hasAll = true;
                                    }
                                }
                                while (queue.Count > 0);

                                if (monitor.monitors.Length == 0)
                                {
                                    empty = 1;
                                }
                                else
                                {
                                    empty = 0;
                                }
                            }
                        }

                        if (empty != 0)
                        {
                            if (++empty > 5)
                            {
                                lock (queue)
                                {
                                    if (queue.Count == 0)
                                    {
                                        isActive = false;
                                        return;
                                    }
                                }
                            }

                            await Task.Delay(500);

                            continue;
                        }

                        #endregion

                        //CNST_SYSTEM_HANDLE_INFORMATION is limited to 16-bit process IDs
                        var querying = true;
                        var length = 0;

                        do
                        {
                            var r = NativeMethods.NtQuerySystemInformation(infoClass, _info, infoLength, ref length);

                            switch (r)
                            {
                                case NtStatus.Success:

                                    querying = false;

                                    break;
                                case NtStatus.InfoLengthMismatch:

                                    _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = (int)(length * 1.01)));

                                    break;
                                case NtStatus.InvalidInfoClass:

                                    if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                                    {
                                        infoClass = SYSTEM_INFORMATION_CLASS.SystemHandleInformation;
                                        infoSize = Marshal.SizeOf(typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO));
                                        extended = false;
                                    }
                                    else
                                    {
                                        throw new NotSupportedException("SystemHandleInformation not supported");
                                    }

                                    break;
                                default:

                                    throw new Exception(r.ToString("x"));
                            }
                        }
                        while (querying);

                        IntPtr _handle;
                        long handleCount;
                        //Type infoType;

                        if (extended)
                        {
#if x86
                            handleCount = Marshal.ReadInt32(_info);
#elif x64
                            handleCount = Marshal.ReadInt64(_info);
#else
                            if (IntPtr.Size == 4)
                                handleCount = Marshal.ReadInt32(_info);
                            else
                                handleCount = Marshal.ReadInt64(_info);
#endif
                            _handle = _info + IntPtr.Size * 2;
                            //infoType = typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX);
                        }
                        else
                        {
                            //SystemHandleInformation count is 32bit
                            handleCount = Marshal.ReadInt32(_info);

                            _handle = _info + IntPtr.Size;
                            //infoType = typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO);
                        }

                        var _pid = 0;
                        var pcount = monitor.monitors.Length;
                        MonitorProcess mi = null;
                        LinkedNode n = null;
                        ulong prev = 0;

                        //handles are listed in numeric order and group by process. processes are not listed in order.

                        for (var i = 0; i < handleCount; i++)
                        {
                            //var handleInfo = Marshal.PtrToStructure((IntPtr)(_handle.GetValue() + i * infoSize), infoType);
                            var ofs = i * infoSize + IntPtr.Size;
                            int pid;

                            if (extended)
                            {
                                //info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)handleInfo;
                                pid = (int)Marshal.ReadIntPtr(_handle, ofs); //info.UniqueProcessId
                            }
                            else
                            {
                                //info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO)handleInfo;
                                pid = Marshal.ReadInt16(_handle, ofs); //info.UniqueProcessId
                            }

                            if (pid != _pid)
                            {
                                if (mi != null)
                                {
                                    for (var j = 0; j < mi.clients.Length; j++)
                                    {
                                        ++mi.clients[j].cycles;
                                    }

                                    if (--pcount == 0)
                                    {
                                        //all monitored processes found
                                        mi = null;
                                        break;
                                    }
                                }

                                _pid = pid;

                                var k = monitor.IndexOf(pid);
                                if (k != -1)
                                {
                                    mi = monitor.monitors[k];
                                    n = mi.handles;
                                }
                                else
                                {
                                    mi = null;
                                }

                                prev = 0;
                            }

                            if (mi != null)
                            {
                                IntPtr h;
                                short t;

                                if (extended)
                                {
                                    h = Marshal.ReadIntPtr(_handle, ofs + IntPtr.Size); //info.HandleValue
                                    t = Marshal.ReadInt16(_handle, ofs + IntPtr.Size * 2 + 6); //info.ObjectTypeIndex
                                }
                                else
                                {
                                    h = (IntPtr)Marshal.ReadInt16(_handle, ofs + 6); //info.HandleValue
                                    t = Marshal.ReadByte(_handle, ofs + 4); //info.ObjectTypeIndex
                                }

                                var handle = (uint)h;

                                #region linked nodes

                                //note a handle could be closed and immediately reused - these won't be detected as removed/added handles

                                var next = n.next;
                                var add = false;

                                if (next != null)
                                {
                                    if (next.handle < handle)
                                    {
                                        do
                                        {
                                            //next.handle was removed

                                            next = next.next;

                                            if (next == null)
                                            {
                                                add = true;
                                                n.next = null;
                                                break;
                                            }
                                        }
                                        while (next.handle < handle);

                                        n.next = next;
                                        if (next != null)
                                            next.previous = n;
                                    }

                                    if (!add)
                                    {
                                        add = next.handle != handle;
                                    }
                                }
                                else
                                {
                                    add = true;
                                }

                                if (add)
                                {
                                    var n2 = new LinkedNode()
                                    {
                                        handle = handle,
                                        previous = n,
                                        next = next,
                                    };
                                    n.next = n2;
                                    if (next != null)
                                        next.previous = n2;
                                    n = n2;

                                    //n.handle was added
                                }
                                else
                                {
                                    n = next;
                                }

                                #endregion

                                if (mi.Contains(t))
                                {
                                    if (add || mi.all)
                                    {
                                        data.handle = h;
                                        data.type = t;
                                        data.getName = delegate
                                        {
                                            if (t == GetHandleTypeIndex(HandleType.File))
                                            {
                                                return GetFileName(buffer, (UIntPtr)pid, (UIntPtr)h.GetValue());
                                            }
                                            else
                                            {
                                                return GetObjectName(buffer, (UIntPtr)pid, (UIntPtr)h.GetValue(), extended);
                                            }
                                        };

                                        mi.Push(t, handle, data, add);
                                    }
                                }

                                prev = handle;
                            }
                        }

                        if (mi != null)
                        {
                            for (var j = 0; j < mi.clients.Length; j++)
                            {
                                ++mi.clients[j].cycles;
                            }
                        }

                        if (hasAll)
                        {
                            hasAll = false;
                            monitor.Reset();
                        }

                        if (Tick != null)
                        {
                            try
                            {
                                Tick(null, EventArgs.Empty);
                            }
                            catch { }
                        }

                        await Task.Delay(500);
                    }
                }
                catch (Exception e)
                {
                    isActive = false;
                    Util.Logging.Log(e);
                }
                finally
                {
                    if (_info != IntPtr.Zero)
                        Marshal.FreeHGlobal(_info);

                    using (buffer) { }
                }
            }

        }
    }
}
