using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    class ProcessInfo : IDisposable
    {
        private int _size;
        private IntPtr _buffer;
        private IntPtr _process;
        private StringBuilder _sb;

        private int sizeofPBI;
        private int sizeofPEB;
        private int sizeofRTL;

        private PROCESS_BASIC_INFORMATION pbi;
        private bool hasPBI;

        public ProcessInfo()
        {
            sizeofPBI = Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION));
            sizeofPEB = Marshal.SizeOf(typeof(PEB));
            sizeofRTL = Marshal.SizeOf(typeof(RTL_USER_PROCESS_PARAMETERS));

            _size = sizeofRTL;
            _buffer = Marshal.AllocHGlobal(_size);
        }

        /// <summary>
        /// Opens a handle for the process
        /// </summary>
        /// <param name="pid">The ID of the process</param>
        /// <returns>True if a handle was opened and it could be accessed</returns>
        public bool Open(int pid)
        {
            if (_process != IntPtr.Zero)
                Close();

            _process = NativeMethods.OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead, false, pid);

            if (_process != IntPtr.Zero)
            {
                int pSize;
                int status = NativeMethods.NtQueryInformationProcess(_process, 0, ref pbi, sizeofPBI, out pSize);

                hasPBI = status == 0;
            }
            else
                hasPBI = false;

            return hasPBI;
        }

        /// <summary>
        /// Closes the handle to the currently opened process
        /// </summary>
        public void Close()
        {
            if (_process != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(_process);
                _process = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Returns the ID of the parent process for the currently opened process
        /// </summary>
        /// <returns></returns>
        public uint GetParent()
        {
            return GetParent(_process);
        }

        private uint GetParent(IntPtr handle)
        {
            int pSize;
            var pbi = new PROCESS_BASIC_INFORMATION();
            int status = NativeMethods.NtQueryInformationProcess(handle, 0, ref pbi, sizeofPBI, out pSize);
            if (status == 0)
                return (uint)pbi.InheritedFromUniqueProcessId;
            return 0;
        }

        /// <summary>
        /// Returns the command line for the currently opened process
        /// </summary>
        /// <returns></returns>
        public string GetCommandLine()
        {
            return GetCommandLine(_process, pbi.PebBaseAddress);
        }

        private string GetCommandLine(IntPtr handle, IntPtr address)
        {
            IntPtr read;
            if (NativeMethods.ReadProcessMemory(handle, address, _buffer, sizeofPEB, out read))
            {
                var peb = (PEB)Marshal.PtrToStructure(_buffer, typeof(PEB));
                if (NativeMethods.ReadProcessMemory(handle, peb.ProcessParameters, _buffer, sizeofRTL, out read))
                {
                    var rtl = (RTL_USER_PROCESS_PARAMETERS)Marshal.PtrToStructure(_buffer, typeof(RTL_USER_PROCESS_PARAMETERS));
                    if (rtl.CommandLine.Length > 0)
                    {
                        if (rtl.CommandLine.Length > _size)
                        {
                            _size = rtl.CommandLine.Length;
                            _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)_size);
                        }

                        if (NativeMethods.ReadProcessMemory(handle, rtl.CommandLine.Buffer, _buffer, rtl.CommandLine.Length, out read))
                            return Marshal.PtrToStringUni(_buffer, rtl.CommandLine.Length / 2);
                    }
                    else
                        return string.Empty;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the name of the user running the process
        /// </summary>
        /// <returns></returns>
        public string GetUsername()
        {
            return GetUsername(_process);
        }

        private string GetUsername(IntPtr handle)
        {
            IntPtr token;
            if (NativeMethods.OpenProcessToken(handle, ProcessTokenAccessFlags.Query, out token))
            {
                try
                {
                    var sid = GetTokenSid(token);
                    if (sid == IntPtr.Zero)
                        return null;
                    return GetAccountNameForSid(sid);
                }
                finally
                {
                    NativeMethods.CloseHandle(token);
                }
            }

            return null;
        }

        private IntPtr GetTokenSid(IntPtr token)
        {
            do
            {
                int _required = 0;
                if (NativeMethods.GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenUser, _buffer, _size, out _required))
                {
                    var tu = (TOKEN_USER)Marshal.PtrToStructure(_buffer, typeof(TOKEN_USER));

                    return tu.User.Sid;
                }
                else if (_required > _size)
                {
                    _size = _required;
                    _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)_size);
                }
                else
                {
                    break;
                }
            }
            while (true);

            return IntPtr.Zero;
        }

        private string GetAccountNameForSid(IntPtr sid)
        {
            int _name, _domain;
            bool retry = true;
            SID_NAME_USE sidUse;

            _name = _domain = this._size / 4; //size in unicode characters

            do
            {
                if (NativeMethods.LookupAccountSid(null, sid, _buffer, ref _name, (IntPtr)(_buffer + _name * 2), ref _domain, out sidUse))
                {
                    return Marshal.PtrToStringUni(_buffer);
                }
                else if (retry && _name > 0 && _domain > 0)
                {
                    var _size = (_name + _domain) * 2;

                    if (_size > this._size)
                    {
                        _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)_size);
                        this._size = _size;
                    }

                    retry = false;
                }
                else
                    break;
            }
            while (true);

            return null;
        }

        public string[] GetModules()
        {
            var sz = IntPtr.Size;
            var cb = _size;

            if (cb < 256)
            {
                _size = cb = sz * 100;
                _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)_size);
            }

            do
            {
                if (NativeMethods.EnumProcessModulesEx(_process, _buffer, cb, out cb, 3) != 1)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                else if (cb == 0)
                    return new string[0];

                if (cb <= _size)
                    break;

                _size = cb = cb + sz * 10;
                _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)_size);
            }
            while (true);

            if (_sb == null)
                _sb = new StringBuilder(256);
            else
                _sb.EnsureCapacity(256);

            var count = cb / sz;
            var modules = new string[count];
            var ofs = 0;

            for (var i = 0; i < count; i++)
            {
                var h = Marshal.ReadIntPtr(_buffer, ofs);
                ofs += sz;

                NativeMethods.GetModuleFileNameEx(_process, h, _sb, 256);

                modules[i] = _sb.ToString();
            }

            return modules;
        }

        public void Dispose()
        {
            if (_process != IntPtr.Zero)
                Close();

            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
            }
        }
    }
}
