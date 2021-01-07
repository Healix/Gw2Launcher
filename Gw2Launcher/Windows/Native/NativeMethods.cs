using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Gw2Launcher.Windows.Native
{
    internal static class NativeMethods
    {
        public const string USER32 = "user32.dll";
        public const string KERNEL32 = "kernel32.dll";
        public const string NTDLL = "ntdll";
        public const string URLMON = "urlmon.dll";
        public const string OLE32 = "ole32.dll";
        public const string GDI32 = "gdi32.dll";
        public const string SHELL32 = "shell32.dll";
        public const string ADVAPI32 = "advapi32.dll";
        public const string SHLWAPI = "shlwapi.dll";
        public const string DWMAPI = "dwmapi.dll";
        public const string UXTHEME = "uxtheme.dll";
        public const string PSAPI = "psapi.dll";

        [DllImport(USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport(USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport(USER32, EntryPoint = "SetWindowLong")]
        internal static extern int SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport(USER32, EntryPoint = "SetWindowLongPtr")]
        internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
#if x86
            return (IntPtr)SetWindowLongPtr32(hWnd, nIndex, (int)dwNewLong);
#elif x64
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
#else
            if (IntPtr.Size == 4)
                return (IntPtr)SetWindowLongPtr32(hWnd, nIndex, (int)dwNewLong);
            else
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
#endif
        }

        [DllImport(USER32, EntryPoint = "GetWindowLong")]
        internal static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport(USER32, EntryPoint = "GetWindowLongPtr")]
        internal static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        internal static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
#if x86
            return GetWindowLongPtr32(hWnd, nIndex);
#elif x64
            return GetWindowLongPtr64(hWnd, nIndex);
#else
            if (IntPtr.Size == 4)
                return GetWindowLongPtr32(hWnd, nIndex);
            else
                return GetWindowLongPtr64(hWnd, nIndex);
#endif
        }

        [DllImport(USER32, SetLastError = true)]
        internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport(USER32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport(USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindow(IntPtr hWnd);

        [DllImport(USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport(USER32, SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport(USER32)]
        internal static extern IntPtr BeginDeferWindowPos(int nNumWindows);

        [DllImport(USER32, SetLastError = true)]
        internal static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd,
            IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport(USER32)]
        internal static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

        [DllImport(NTDLL)]
        internal static extern NtStatus NtQueryObject(IntPtr ObjectHandle, ObjectInformationClass ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, ref int returnLength);

        [DllImport(NTDLL)]
        internal static extern NtStatus NtQueryObject(IntPtr ObjectHandle, ObjectInformationClass ObjectInformationClass, out OBJECT_BASIC_INFORMATION ObjectInformation, int ObjectInformationLength, ref int returnLength);

        [DllImport(NTDLL)]
        internal static extern NtStatus NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int returnLength);

        [DllImport(KERNEL32)]
        internal static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport(KERNEL32)]
        internal static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, UIntPtr dwProcessId);

        [DllImport(KERNEL32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, ushort hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);

        [DllImport(KERNEL32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, UIntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);

        [DllImport(KERNEL32)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport(USER32, SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetClassName(IntPtr hWnd, char[] lpClassName, int nMaxCount);

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport(USER32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport(USER32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int SendMessage(IntPtr hWnd, int msg, int Param, StringBuilder text);

        [DllImport(USER32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int SendMessage(IntPtr hWnd, WindowMessages msg, int Param, StringBuilder text);

        [DllImport(USER32)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport(USER32)]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport(USER32)]
        internal static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport(USER32)]
        internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport(USER32)]
        internal static extern bool ShowWindowAsync(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport(URLMON)]
        internal static extern int CopyStgMedium(ref STGMEDIUM pcstgmedSrc, ref STGMEDIUM pstgmedDest);

        [DllImport(OLE32)]
        internal static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

        [DllImport(USER32)]
        internal static extern uint RegisterClipboardFormat(string lpszFormatName);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, WindowMessages Msg, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport(GDI32)]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport(SHELL32, CharSet = CharSet.Auto)]
        internal static extern IntPtr ILCreateFromPath(string path);

        [DllImport(SHELL32, CharSet = CharSet.None)]
        internal static extern void ILFree(IntPtr pidl);

        [DllImport(SHELL32, CharSet = CharSet.None)]
        internal static extern int ILGetSize(IntPtr pidl);

        [DllImport(SHELL32)]
        internal static extern int SHDoDragDrop([In] IntPtr hwnd, [In] IDataObject data, [In] IntPtr drop, [In] int dwEffect, out int pdwEffect);

        [DllImport(KERNEL32, SetLastError = false)]
        internal static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint RegisterWindowMessage(string lpString);

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, WindowMessages Msg, IntPtr wParam, IntPtr lParam);

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SendMessageCallback(IntPtr hWnd, uint Msg, UIntPtr wParam,
            IntPtr lParam, SendMessageDelegate lpCallBack, UIntPtr dwData);

        [DllImport(USER32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PeekMessage(ref NativeMessage message, IntPtr handle, uint filterMin, uint filterMax, uint flags);

        [DllImport(USER32)]
        internal static extern IntPtr GetFocus();

        [DllImport(USER32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetWindowText(IntPtr hwnd, String lpString);

        [DllImport(USER32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMessage(ref NativeMessage message, IntPtr handle, uint filterMin, uint filterMax);

        [DllImport(USER32)]
        internal static extern IntPtr DispatchMessage([In] ref NativeMessage lpmsg);

        [DllImport(USER32)]
        internal static extern bool TranslateMessage([In] ref NativeMessage lpMsg);

        [DllImport(USER32)]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport(USER32, SetLastError = true)]
        internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern IntPtr CreateFile(string lpFileName, EFileAccess dwDesiredAccess, EFileShare dwShareMode, IntPtr lpSecurityAttributes, ECreationDisposition dwCreationDisposition, EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr InBuffer, int nInBufferSize, IntPtr OutBuffer, int nOutBufferSize, out int pBytesReturned, IntPtr lpOverlapped);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        [DllImport(KERNEL32, CharSet = CharSet.Auto)]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport(SHELL32, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, ref IntPtr ppidl, uint sfgao, ref uint psfgao);

        [DllImport(SHELL32)]
        internal static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, int dwFlags);

        [DllImport(SHELL32, SetLastError = false)]
        internal static extern int SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        [DllImport(USER32, SetLastError = true)]
        internal static extern int DestroyIcon(IntPtr hIcon);

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport(NTDLL, SetLastError = true)]
        internal static extern int NtQueryInformationProcess(IntPtr hProcess, int pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);

        [DllImport(ADVAPI32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle, ProcessTokenAccessFlags DesiredAccess, out IntPtr TokenHandle);

        [DllImport(ADVAPI32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool LookupAccountSid(string lpSystemName, IntPtr Sid, IntPtr lpName, ref int cchName, IntPtr ReferencedDomainName, ref int cchReferencedDomainName, out SID_NAME_USE peUse);

        [DllImport(ADVAPI32, SetLastError = true)]
        internal static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);

        [DllImport(USER32)]
        internal static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport(USER32)]
        internal static extern bool ReleaseCapture();

        [DllImport(SHLWAPI)]
        internal static extern int ColorHLSToRGB(int H, int L, int S);

        [DllImport(USER32)]
        internal static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport(USER32)]
        internal static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport(USER32)]
        private static extern uint GetWindowThreadProcessId(IntPtr handle, IntPtr pid);

        internal static uint GetWindowThreadId(IntPtr handle)
        {
            return GetWindowThreadProcessId(handle, IntPtr.Zero);
        }

        [DllImport(USER32, SetLastError = true)]
        internal static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport(USER32)]
        internal static extern IntPtr WindowFromPoint(System.Drawing.Point p);

        [DllImport(DWMAPI)]
        internal static extern int DwmIsCompositionEnabled(out bool enabled);

        internal static bool IsDwmCompositionEnabled()
        {
            bool isEnabled;
            try
            {
                DwmIsCompositionEnabled(out isEnabled);
            }
            catch
            {
                isEnabled = false;
            }
            return isEnabled;
        }

        [DllImport(SHELL32)]
        internal static extern void GetCurrentProcessExplicitAppUserModelID([Out(), MarshalAs(UnmanagedType.LPWStr)] out string AppID);

        internal static string GetCurrentProcessExplicitAppUserModelID()
        {
            try
            {
                string id;
                GetCurrentProcessExplicitAppUserModelID(out id);
                return id;
            }
            catch
            {
                return null;
            }
        }

        [DllImport(KERNEL32, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
        
        [DllImport(KERNEL32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        internal static TITLEBARINFOEX GetTitleBarInfoEx(IntPtr hwnd)
        {
            var tbi = new TITLEBARINFOEX();
            tbi.cbSize = Marshal.SizeOf(typeof(TITLEBARINFOEX));

            var ptr = Marshal.AllocHGlobal(tbi.cbSize);
            try
            {
                Marshal.StructureToPtr(tbi, ptr, false);
                if (SendMessage(hwnd, (int)WindowMessages.WM_GETTITLEBARINFOEX, IntPtr.Zero, ptr) == IntPtr.Zero)
                    throw new NotSupportedException();
                return (TITLEBARINFOEX)Marshal.PtrToStructure(ptr, tbi.GetType());
            }
            finally
            {
                Marshal.DestroyStructure(ptr, typeof(TITLEBARINFOEX));
                Marshal.FreeHGlobal(ptr);
            }
        }

        [DllImport(DWMAPI, PreserveSig = true)]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

        [DllImport(DWMAPI)]
        internal static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport(DWMAPI)]
        internal static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref int[] margins);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel, CopyFileFlags dwCopyFlags);

        public delegate CopyProgressResult CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

        [DllImport(GDI32)]
        internal static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern bool GetFileInformationByHandleEx(IntPtr hFile, FILE_INFO_BY_HANDLE_CLASS infoClass, IntPtr info, uint dwBufferSize);

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern bool GetFileInformationByHandleEx(IntPtr hFile, FILE_INFO_BY_HANDLE_CLASS infoClass, out FILE_STANDARD_INFO info, uint dwBufferSize);

        [DllImport(NTDLL, ExactSpelling = true)]
        internal static extern uint NtOpenFile(out Microsoft.Win32.SafeHandles.SafeFileHandle handle, UInt32 access, ref OBJECT_ATTRIBUTES objectAttributes, out IO_STATUS_BLOCK ioStatus, System.IO.FileShare share, uint openOptions);

        [DllImport(UXTHEME, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal extern static int DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, IntPtr pClipRect);

        [DllImport(UXTHEME, ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenThemeData(IntPtr hWnd, String classList);

        [DllImport(UXTHEME, ExactSpelling = true)]
        internal static extern Int32 CloseThemeData(IntPtr hTheme);

        [DllImport(UXTHEME, ExactSpelling = true)]
        internal extern static Int32 GetThemeMetric(IntPtr hTheme, IntPtr hDC, int iPartId, int iStateId, int iPropId, out int piVal);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport(USER32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern bool SetHandleInformation(IntPtr hObject, uint dwMask, uint dwFlags);

        [DllImport(SHELL32, CharSet = CharSet.Auto)]
        internal static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport(SHELL32)]
        internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport(SHELL32)]
        internal static extern int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

        [DllImport(PSAPI, SetLastError = true)]
        internal static extern int EnumProcessModulesEx(IntPtr hProcess, IntPtr lphModule, int cb, out int lpcbNeeded, uint dwFilterFlag);

        [DllImport(PSAPI)]
        internal static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
    }
}
