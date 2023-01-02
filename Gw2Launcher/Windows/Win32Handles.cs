using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class Win32Handles
    {
        private const byte THREADS = 4;
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
        }

        private class ObjectData : IObject
        {
            public UIntPtr pid;
            public ushort typeIndex;
            public HandleType type;
            public string name;

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
        public static IObjectHandle GetHandle(UIntPtr[] pids, HandleType type, Func<IObject, CallbackResponse> nameCallback, bool includeNulls = false)
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

                            _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = length));

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
                var data = new ObjectData[THREADS];

                buffers = new Buffer[THREADS];

                for (var j = 0; j < THREADS; j++)
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

                var loop = Util.Loop.For(0, handleCount, THREADS, 1000,
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

        public static IObjectHandle GetHandle(int processId, string objectName, MatchMode match)
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
                        return s.EndsWith(objectName, StringComparison.Ordinal);
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
                        return s.StartsWith(objectName, StringComparison.Ordinal);
                    };
                    break;
                default:
                    throw new NotSupportedException();
            }

            return GetHandle(processId, objectName, f);
        }

        public static IObjectHandle GetHandle(int processId, string objectName, Func<string, bool> objectNameCallback)
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

                            _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = length));

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

                buffers = new Buffer[THREADS];

                for (var j = 0; j < THREADS; j++)
                {
                    buffers[j] = new Buffer(256);
                }

                var canQuery = infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation && !Settings.IsRunningWine;

                //warning: NtQueryObject can cause a deadlock when querying an item that is waiting

                var loop = Util.Loop.For(0, handleCount, THREADS, 1000,
                    delegate(byte thread, long i, Util.Loop.IState state)
                    {
                        var handleInfo = Marshal.PtrToStructure((IntPtr)(_handle.GetValue() + i * infoSize), infoType);
                        
                        UIntPtr pid, h;

                        if (infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation)
                        {
                            var info = (SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)handleInfo;

                            pid = info.UniqueProcessId;
                            h = info.HandleValue;
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
                        }

                        if (processId > 0 && pid != _processId)
                            return;

                        var name = GetObjectName(buffers[thread], pid, h, canQuery);

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

        public static int FindType(int processId, UIntPtr handle)
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

                            _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = length));

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

                buffers = new Buffer[THREADS];

                for (var j = 0; j < THREADS; j++)
                {
                    buffers[j] = new Buffer(256);
                }

                var canQuery = infoClass == SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation && !Settings.IsRunningWine;

                //warning: NtQueryObject can cause a deadlock when querying an item that is waiting

                var loop = Util.Loop.For(0, handleCount, THREADS, 1000,
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

                            _info = Marshal.ReAllocHGlobal(_info, (IntPtr)(infoLength = length));

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

    }
}
