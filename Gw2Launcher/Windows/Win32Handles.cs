using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Gw2Launcher.Windows
{
    static class Win32Handles
    {
        #region Native

        private const int DUPLICATE_CLOSE_SOURCE = 0x1;
        private const int DUPLICATE_SAME_ACCESS = 0x2;

        private const uint STATUS_INFO_LENGTH_MISMATCH = 0xc0000004;
        private const int CNST_SYSTEM_EXTENDED_HANDLE_INFORMATION = 64;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryObject(IntPtr ObjectHandle, int ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, ref int returnLength);

        [DllImport("ntdll.dll")]
        private static extern uint NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int returnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, UIntPtr dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, ushort hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, UIntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        private enum ObjectInformationClass : int
        {
            ObjectBasicInformation = 0,
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2,
            ObjectAllTypesInformation = 3,
            ObjectHandleInformation = 4
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_BASIC_INFORMATION
        { // Information Class 0
            public int Attributes;
            public int GrantedAccess;
            public int HandleCount;
            public int PointerCount;
            public int PagedPoolUsage;
            public int NonPagedPoolUsage;
            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public int NameInformationLength;
            public int TypeInformationLength;
            public int SecurityDescriptorLength;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_NAME_INFORMATION
        { // Information Class 1
            public UNICODE_STRING Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
        {
            public IntPtr Object;
            public UIntPtr UniqueProcessId;
            public UIntPtr HandleValue;
            public uint GrantedAccess;
            public ushort CreatorBackTraceIndex;
            public ushort ObjectTypeIndex;
            public uint HandleAttributes;
            public uint Reserved;
        }

        #endregion

        private static readonly bool IS_64;

        public interface IObjectHandle
        {
            void Kill();
        }

        private class ObjectHandle : IObjectHandle
        {
            protected SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo;

            public ObjectHandle(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX info)
            {
                this.handleInfo = info;
            }

            public SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Info
            {
                get
                {
                    return this.handleInfo;
                }
            }

            public void Kill()
            {
                using (var p = Process.GetProcessById((int)handleInfo.UniqueProcessId.ToUInt32()))
                {
                    IntPtr handle = IntPtr.Zero;
                    try
                    {
                        if (!DuplicateHandle(p.Handle, handleInfo.HandleValue, GetCurrentProcess(), out handle, 0, false, DUPLICATE_CLOSE_SOURCE))
                            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                    }
                    finally
                    {
                        if (handle != IntPtr.Zero)
                            CloseHandle(handle);
                    }
                }
            }
        }

        static Win32Handles()
        {
            IS_64 = Marshal.SizeOf(typeof(IntPtr)) == 8 ? true : false;
        }

        public static string GetObjectName(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handle)
        {
            IntPtr _processHandle = OpenProcess(ProcessAccessFlags.All, false, handle.UniqueProcessId);
            IntPtr _handle = IntPtr.Zero;

            try
            {
                if (!DuplicateHandle(_processHandle, handle.HandleValue, GetCurrentProcess(), out _handle, 0, false, DUPLICATE_SAME_ACCESS))
                    return null;

                IntPtr _basic = IntPtr.Zero;
                int nameLength = 0;

                try
                {
                    OBJECT_BASIC_INFORMATION basicInfo = new OBJECT_BASIC_INFORMATION();
                    _basic = Marshal.AllocHGlobal(Marshal.SizeOf(basicInfo));

                    NtQueryObject(_handle, (int)ObjectInformationClass.ObjectBasicInformation, _basic, Marshal.SizeOf(basicInfo), ref nameLength);
                    basicInfo = (OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(_basic, basicInfo.GetType());
                    nameLength = basicInfo.NameInformationLength;
                }
                finally
                {
                    if (_basic != IntPtr.Zero)
                        Marshal.FreeHGlobal(_basic);
                }

                if (nameLength == 0)
                {
                    return null;
                }

                OBJECT_NAME_INFORMATION nameInfo = new OBJECT_NAME_INFORMATION();
                IntPtr _objectName = Marshal.AllocHGlobal(nameLength);

                try
                {
                    while ((uint)(NtQueryObject(_handle, (int)ObjectInformationClass.ObjectNameInformation, _objectName, nameLength, ref nameLength)) == STATUS_INFO_LENGTH_MISMATCH)
                    {
                        Marshal.FreeHGlobal(_objectName);
                        _objectName = Marshal.AllocHGlobal(nameLength);
                    }
                    nameInfo = (OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(_objectName, nameInfo.GetType());
                }
                finally
                {
                    Marshal.FreeHGlobal(_objectName);
                }

                try
                {
                    return Marshal.PtrToStringUni(nameInfo.Name.Buffer, nameInfo.Name.Length >> 1);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return null;
            }
            finally
            {
                if (_handle != IntPtr.Zero) //moved from Marshal.FreeHGlobal(_objectName);
                    CloseHandle(_handle);
                if (_processHandle != IntPtr.Zero)
                    CloseHandle(_processHandle);
            }
        }

        public static IObjectHandle GetHandle(int processId, string objectName, bool exactMatch)
        {
            int infoLength = 0x10000;
            int length = 0;
            IntPtr _info = Marshal.AllocHGlobal(infoLength);
            IntPtr _handle = IntPtr.Zero;
            long handleCount = 0;

            try
            {
                //CNST_SYSTEM_HANDLE_INFORMATION is limited to 16-bit process IDs
                while ((NtQuerySystemInformation(CNST_SYSTEM_EXTENDED_HANDLE_INFORMATION, _info, infoLength, ref length)) == STATUS_INFO_LENGTH_MISMATCH)
                {
                    infoLength = length;
                    Marshal.FreeHGlobal(_info);
                    _info = Marshal.AllocHGlobal(infoLength);
                }

                if (IS_64)
                {
                    handleCount = Marshal.ReadInt64(_info);
                    _handle = new IntPtr(_info.ToInt64() + 16);
                }
                else
                {
                    handleCount = Marshal.ReadInt32(_info);
                    _handle = new IntPtr(_info.ToInt32() + 8);
                }

                SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo;
                List<SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX> handles = new List<SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>();

                handleInfo = new SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX();
                int infoSize = Marshal.SizeOf(handleInfo);
                Type infoType = handleInfo.GetType();

                for (long i = 0; i < handleCount; i++)
                {
                    if (IS_64)
                    {
                        handleInfo = (SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)Marshal.PtrToStructure(_handle, infoType);
                        _handle = new IntPtr(_handle.ToInt64() + infoSize);
                    }
                    else
                    {
                        handleInfo = (SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)Marshal.PtrToStructure(_handle, infoType);
                        _handle = new IntPtr(_handle.ToInt32() + infoSize);
                    }

                    if (processId > 0 && handleInfo.UniqueProcessId.ToUInt32() != processId)
                        continue;

                    string name = GetObjectName(handleInfo);
                    if (name == null)
                        continue;

                    if (exactMatch)
                    {
                        if (!name.Equals(objectName))
                            continue;
                    }
                    else if (!name.Contains(objectName))
                        continue;

                    return new ObjectHandle(handleInfo);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(_info);
            }

            return null;
        }

        public static bool Is64Bit()
        {
            return IS_64;
        }
    }
}
