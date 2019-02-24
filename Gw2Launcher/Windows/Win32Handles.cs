using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class Win32Handles
    {
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
                        if (!NativeMethods.DuplicateHandle(p.Handle, handleInfo.HandleValue, NativeMethods.GetCurrentProcess(), out handle, 0, false, DuplicateOptions.DUPLICATE_CLOSE_SOURCE))
                            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                    }
                    finally
                    {
                        if (handle != IntPtr.Zero)
                            NativeMethods.CloseHandle(handle);
                    }
                }
            }
        }

        static Win32Handles()
        {
            IS_64 = Marshal.SizeOf(typeof(IntPtr)) == 8;
        }

        public static string GetObjectName(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handle)
        {
            IntPtr _processHandle = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, handle.UniqueProcessId);
            IntPtr _handle = IntPtr.Zero;

            try
            {
                if (!NativeMethods.DuplicateHandle(_processHandle, handle.HandleValue, NativeMethods.GetCurrentProcess(), out _handle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS))
                    return null;

                IntPtr _basic = IntPtr.Zero;
                int nameLength = 0;

                try
                {
                    OBJECT_BASIC_INFORMATION basicInfo = new OBJECT_BASIC_INFORMATION();
                    _basic = Marshal.AllocHGlobal(Marshal.SizeOf(basicInfo));

                    NativeMethods.NtQueryObject(_handle, (int)ObjectInformationClass.ObjectBasicInformation, _basic, Marshal.SizeOf(basicInfo), ref nameLength);
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
                    while (NativeMethods.NtQueryObject(_handle, (int)ObjectInformationClass.ObjectNameInformation, _objectName, nameLength, ref nameLength) == NtStatus.InfoLengthMismatch)
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
                    NativeMethods.CloseHandle(_handle);
                if (_processHandle != IntPtr.Zero)
                    NativeMethods.CloseHandle(_processHandle);
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
                while (NativeMethods.NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation, _info, infoLength, ref length) == NtStatus.InfoLengthMismatch)
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

                var handleInfo = new SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX();
                var infoSize = Marshal.SizeOf(handleInfo);
                var infoType = handleInfo.GetType();

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
                        if (!name.Equals(objectName, StringComparison.Ordinal))
                            continue;
                    }
                    else if (name.IndexOf(objectName, StringComparison.Ordinal) == -1)
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
