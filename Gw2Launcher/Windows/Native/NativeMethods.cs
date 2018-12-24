using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Gw2Launcher.Windows.Native
{
    internal static class NativeMethods
    {
        internal static class DLL
        {
            public const string USER32 = "user32.dll";
            public const string KERNEL32 = "kernel32.dll";
            public const string NTDLL = "ntdll.dll";
            public const string URLMON = "urlmon.dll";
            public const string OLE32 = "ole32.dll";
            public const string GDI32 = "gdi32.dll";
            public const string SHELL32 = "shell32.dll";
            public const string ADVAPI32 = "advapi32.dll";
        }

        [DllImport(DLL.USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport(DLL.USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport(DLL.USER32, SetLastError = true)]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport(DLL.USER32, SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport(DLL.USER32, SetLastError = true)]
        internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport(DLL.USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport(DLL.USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindow(IntPtr hWnd);

        [DllImport(DLL.USER32, SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport(DLL.USER32)]
        internal static extern IntPtr BeginDeferWindowPos(int nNumWindows);

        [DllImport(DLL.USER32, SetLastError = true)]
        internal static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd,
            IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport(DLL.USER32)]
        internal static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

        [DllImport(DLL.NTDLL)]
        internal static extern NtStatus NtQueryObject(IntPtr ObjectHandle, int ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, ref int returnLength);

        [DllImport(DLL.NTDLL)]
        internal static extern NtStatus NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int returnLength);

        [DllImport(DLL.KERNEL32)]
        internal static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport(DLL.KERNEL32)]
        internal static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, UIntPtr dwProcessId);

        [DllImport(DLL.KERNEL32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, ushort hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);

        [DllImport(DLL.KERNEL32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, UIntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);

        [DllImport(DLL.KERNEL32)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport(DLL.USER32, SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport(DLL.USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport(DLL.USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport(DLL.USER32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport(DLL.USER32, SetLastError = true)]
        internal static extern int SendMessage(IntPtr hWnd, int msg, int Param, StringBuilder text);

        [DllImport(DLL.USER32)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport(DLL.USER32)]
        internal static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport(DLL.USER32)]
        internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport(DLL.URLMON)]
        internal static extern int CopyStgMedium(ref STGMEDIUM pcstgmedSrc, ref STGMEDIUM pstgmedDest);

        [DllImport(DLL.OLE32)]
        internal static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

        [DllImport(DLL.USER32)]
        internal static extern uint RegisterClipboardFormat(string lpszFormatName);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport(DLL.USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport(DLL.GDI32)]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport(DLL.SHELL32, CharSet = CharSet.Auto)]
        internal static extern IntPtr ILCreateFromPath(string path);

        [DllImport(DLL.SHELL32, CharSet = CharSet.None)]
        internal static extern void ILFree(IntPtr pidl);

        [DllImport(DLL.SHELL32, CharSet = CharSet.None)]
        internal static extern int ILGetSize(IntPtr pidl);

        [DllImport(DLL.SHELL32)]
        internal static extern int SHDoDragDrop([In] IntPtr hwnd, [In] IDataObject data, [In] IntPtr drop, [In] int dwEffect, out int pdwEffect);

        [DllImport(DLL.KERNEL32, SetLastError = false)]
        internal static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport(DLL.USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint RegisterWindowMessage(string lpString);

        [DllImport(DLL.USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport(DLL.USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SendMessageCallback(IntPtr hWnd, uint Msg, UIntPtr wParam,
            IntPtr lParam, SendMessageDelegate lpCallBack, UIntPtr dwData);

        [DllImport(DLL.USER32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PeekMessage(ref NativeMessage message, IntPtr handle, uint filterMin, uint filterMax, uint flags);

        [DllImport(DLL.USER32)]
        internal static extern IntPtr GetFocus();

        [DllImport(DLL.USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetWindowText(IntPtr hwnd, String lpString);

        [DllImport(DLL.USER32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMessage(ref NativeMessage message, IntPtr handle, uint filterMin, uint filterMax);

        [DllImport(DLL.USER32)]
        internal static extern IntPtr DispatchMessage([In] ref NativeMessage lpmsg);

        [DllImport(DLL.USER32)]
        internal static extern bool TranslateMessage([In] ref NativeMessage lpMsg);

        [DllImport(DLL.USER32)]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport(DLL.USER32)]
        internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport(DLL.KERNEL32, SetLastError = true)]
        internal static extern IntPtr CreateFile(string lpFileName, EFileAccess dwDesiredAccess, EFileShare dwShareMode, IntPtr lpSecurityAttributes, ECreationDisposition dwCreationDisposition, EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport(DLL.KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr InBuffer, int nInBufferSize, IntPtr OutBuffer, int nOutBufferSize, out int pBytesReturned, IntPtr lpOverlapped);

        [DllImport(DLL.KERNEL32)]
        internal static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport(DLL.KERNEL32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        [DllImport(DLL.KERNEL32, CharSet = CharSet.Auto)]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport(DLL.SHELL32, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, ref IntPtr ppidl, uint sfgao, ref uint psfgao);

        [DllImport(DLL.SHELL32)]
        internal static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, int dwFlags);

        [DllImport(DLL.SHELL32, SetLastError = false)]
        internal static extern int SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        [DllImport(DLL.USER32, SetLastError = true)]
        internal static extern int DestroyIcon(IntPtr hIcon);

        [DllImport(DLL.KERNEL32, SetLastError = true)]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport(DLL.NTDLL, SetLastError = true)]
        internal static extern int NtQueryInformationProcess(IntPtr hProcess, int pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);

        [DllImport(DLL.ADVAPI32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle, ProcessTokenAccessFlags DesiredAccess, out IntPtr TokenHandle);

        [DllImport(DLL.ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool LookupAccountSid(string lpSystemName, IntPtr Sid, IntPtr lpName, ref int cchName, IntPtr ReferencedDomainName, ref int cchReferencedDomainName, out SID_NAME_USE peUse);

        [DllImport(DLL.ADVAPI32, SetLastError = true)]
        internal static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);

        [DllImport(DLL.USER32)]
        internal static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    }
}
