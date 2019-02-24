using System;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Windows.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public SetWindowPosFlags flags;
    };

    [Flags]
    public enum SetWindowPosFlags
    {
        SWP_NOSIZE = 0x1,
        SWP_NOMOVE = 0x2,
        SWP_NOZORDER = 0x4,
        SWP_NOREDRAW = 0x8,
        SWP_NOACTIVATE = 0x10,
        SWP_FRAMECHANGED = 0x20,
        SWP_DRAWFRAME = SWP_FRAMECHANGED,
        SWP_SHOWWINDOW = 0x40,
        SWP_HIDEWINDOW = 0x80,
        SWP_NOCOPYBITS = 0x100,
        SWP_NOOWNERZORDER = 0x200,
        SWP_NOREPOSITION = SWP_NOOWNERZORDER,
        SWP_NOSENDCHANGING = 0x400,
        SWP_DEFERERASE = 0x2000,
        SWP_ASYNCWINDOWPOS = 0x4000,
    }

    public struct POINTAPI
    {
        public int x;
        public int y;
    }

    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public bool Equals(RECT rect)
        {
            return this.left == rect.left && this.top == rect.top && this.right == rect.right && this.bottom == rect.bottom;
        }

        public bool Equals(System.Drawing.Rectangle rect)
        {
            return this.left == rect.Left && this.top == rect.Top && this.right == rect.Right && this.bottom == rect.Bottom;
        }

        public System.Drawing.Rectangle ToRectangle()
        {
            return new System.Drawing.Rectangle(this.left, this.top, this.right - this.left, this.bottom - this.top);
        }

        public static RECT FromRectangle(System.Drawing.Rectangle rect)
        {
            RECT r = new RECT();
            r.left = rect.Left;
            r.right = rect.Right;
            r.top = rect.Top;
            r.bottom = rect.Bottom;
            return r;
        }
    }

    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public ShowWindowCommands showCmd;
        public POINTAPI ptMinPosition;
        public POINTAPI ptMaxPosition;
        public RECT rcNormalPosition;
    }

    [Flags]
    public enum DuplicateOptions : uint
    {
        DUPLICATE_CLOSE_SOURCE = (0x00000001),// Closes the source handle. This occurs regardless of any error status returned.
        DUPLICATE_SAME_ACCESS = (0x00000002), //Ignores the dwDesiredAccess parameter. The duplicate handle has the same access as the source handle.
    }

    public enum SYSTEM_INFORMATION_CLASS
    {
        SystemExtendedHandleInformation = 0x0040,
    }

    public enum NtStatus : uint
    {
        Success = 0x00000000,

        Warning = 0x80000000,
        NoMoreEntries = 0x8000001a,

        Error = 0xc0000000,
        InfoLengthMismatch = 0xc0000004,
    }

    public enum ObjectInformationClass : int
    {
        ObjectBasicInformation = 0,
        ObjectNameInformation = 1,
        ObjectTypeInformation = 2,
        ObjectAllTypesInformation = 3,
        ObjectHandleInformation = 4
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_BASIC_INFORMATION
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
    public struct OBJECT_NAME_INFORMATION
    { // Information Class 1
        public UNICODE_STRING Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UNICODE_STRING
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

    public enum ShowWindowCommands
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        Hide = 0,
        /// <summary>
        /// Activates and displays a window. If the window is minimized or
        /// maximized, the system restores it to its original size and position.
        /// An application should specify this flag when displaying the window
        /// for the first time.
        /// </summary>
        ShowNormal = 1,
        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        ShowMinimized = 2,
        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>      
        ShowMaximized = 3,
        /// <summary>
        /// Displays a window in its most recent size and position. This value
        /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
        /// the window is not activated.
        /// </summary>
        ShowNoActivate = 4,
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        Show = 5,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level
        /// window in the Z order.
        /// </summary>
        Minimize = 6,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to
        /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
        /// window is not activated.
        /// </summary>
        ShowMinNoActive = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is
        /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
        /// window is not activated.
        /// </summary>
        ShowNA = 8,
        /// <summary>
        /// Activates and displays the window. If the window is minimized or
        /// maximized, the system restores it to its original size and position.
        /// An application should specify this flag when restoring a minimized window.
        /// </summary>
        Restore = 9,
        /// <summary>
        /// Sets the show state based on the SW_* value specified in the
        /// STARTUPINFO structure passed to the CreateProcess function by the
        /// program that started the application.
        /// </summary>
        ShowDefault = 10,
        /// <summary>
        ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
        /// that owns the window is not responding. This flag should only be
        /// used when minimizing windows from a different thread.
        /// </summary>
        ForceMinimize = 11
    }

    public enum WindowMessages
    {
        WM_MOVING = 0x0216,
        WM_SIZING = 0x0214,
        WM_ENTERSIZEMOVE = 0x231,
        WM_EXITSIZEMOVE = 0x0232,
        WM_NCHITTEST = 0x84,
        WM_NCRBUTTONUP = 0x00A5,
        WM_GETTEXT = 0x0D,
        WM_GETTEXTLENGTH = 0x0E,
        WM_WINDOWPOSCHANGING = 0x46,
        WM_SHOW = 0x18,
        WM_MOVE = 0x0003,
        WM_MOUSEWHEEL = 0x20a,
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105,
        WM_SETFOCUS = 7,
        WM_PASTE = 0x302,
        WM_NCMOUSEMOVE = 160,
        WM_MOUSEMOVE = 512,
        WM_NCMOUSELEAVE = 674,
        WM_MOUSELEAVE = 675,
        WM_NCLBUTTONDOWN = 161,
        WM_NCRBUTTONDOWN = 164,
        WM_NCMBUTTONDOWN = 167,
        WM_NCXBUTTONDOWN = 171,
        WM_LBUTTONDOWN = 513,
        WM_RBUTTONDOWN = 516,
        WM_MBUTTONDOWN = 519,
        WM_XBUTTONDOWN = 523,
        WM_INVALIDATEDRAGIMAGE = 0x403,
        WM_NCLBUTTONDBLCLK = 0x00A3,
        WM_CANCELMODE = 0x001F,
    }

    public enum Sizing
    {
        Left = 1,
        Right = 2,
        Top = 3,
        TopLeft = 4,
        TopRight = 5,
        Bottom = 6,
        BottomLeft = 7,
        BottomRight = 8,
    }

    public enum HitTest
    {
        Error = -2,
        Transparent = -1,
        Nowhere = 0,
        Client = 1,
        Caption = 2,
        SysMenu = 3,
        GrowBox = 4,
        Menu = 5,
        HScroll = 6,
        VScroll = 7,
        MinButton = 8,
        MaxButton = 9,
        Left = 10,
        Right = 11,
        Top = 12,
        TopLeft = 13,
        TopRight = 14,
        Bottom = 15,
        BottomLeft = 16,
        BottomRight = 17,
        Border = 18,
        Close = 20,
        Help = 21,
    }

    [Flags]
    public enum WindowStyle
    {
        WS_EX_TOPMOST = 0x00000008,
        WS_EX_TOOLWINDOW = 0x80,
        WS_EX_COMPOSITED = 0x02000000,
        WS_EX_LAYERED = 0x00080000,
        WS_EX_TRANSPARENT = 0x00000020,
        WS_EX_NOACTIVATE = 0x08000000,
        
        WS_MAXIMIZEBOX = 0x00010000,
    }

    public enum WindowZOrder
    {
        HWND_TOP = 0,
        HWND_BOTTOM = 1,
        HWND_TOPMOST = -1,
        HWND_NOTOPMOST = -2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMessage
    {
        public IntPtr handle;
        public uint msg;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public System.Drawing.Point p;
    }

    public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    public delegate void SendMessageDelegate(IntPtr hWnd, uint uMsg, UIntPtr dwData, IntPtr lResult);

    public enum SymbolicLink
    {
        File = 0,
        Directory = 1
    }

    [Flags]
    public enum EFileAccess : uint
    {
        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000,
    }

    [Flags]
    public enum EFileShare : uint
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Enables subsequent open operations on an object to request read access. 
        /// Otherwise, other processes cannot open the object if they request read access. 
        /// If this flag is not specified, but the object has been opened for read access, the function fails.
        /// </summary>
        Read = 0x00000001,
        /// <summary>
        /// Enables subsequent open operations on an object to request write access. 
        /// Otherwise, other processes cannot open the object if they request write access. 
        /// If this flag is not specified, but the object has been opened for write access, the function fails.
        /// </summary>
        Write = 0x00000002,
        /// <summary>
        /// Enables subsequent open operations on an object to request delete access. 
        /// Otherwise, other processes cannot open the object if they request delete access.
        /// If this flag is not specified, but the object has been opened for delete access, the function fails.
        /// </summary>
        Delete = 0x00000004
    }

    [Flags]
    public enum EFileAttributes : uint
    {
        Readonly = 0x00000001,
        Hidden = 0x00000002,
        System = 0x00000004,
        Directory = 0x00000010,
        Archive = 0x00000020,
        Device = 0x00000040,
        Normal = 0x00000080,
        Temporary = 0x00000100,
        SparseFile = 0x00000200,
        ReparsePoint = 0x00000400,
        Compressed = 0x00000800,
        Offline = 0x00001000,
        NotContentIndexed = 0x00002000,
        Encrypted = 0x00004000,
        Write_Through = 0x80000000,
        Overlapped = 0x40000000,
        NoBuffering = 0x20000000,
        RandomAccess = 0x10000000,
        SequentialScan = 0x08000000,
        DeleteOnClose = 0x04000000,
        BackupSemantics = 0x02000000,
        PosixSemantics = 0x01000000,
        OpenReparsePoint = 0x00200000,
        OpenNoRecall = 0x00100000,
        FirstPipeInstance = 0x00080000
    }

    public enum ECreationDisposition : uint
    {
        /// <summary>
        /// Creates a new file. The function fails if a specified file exists.
        /// </summary>
        New = 1,
        /// <summary>
        /// Creates a new file, always. 
        /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
        /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
        /// </summary>
        CreateAlways = 2,
        /// <summary>
        /// Opens a file. The function fails if the file does not exist. 
        /// </summary>
        OpenExisting = 3,
        /// <summary>
        /// Opens a file, always. 
        /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
        /// </summary>
        OpenAlways = 4,
        /// <summary>
        /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
        /// The calling process must open the file with the GENERIC_WRITE access right. 
        /// </summary>
        TruncateExisting = 5
    }

    [Flags]
    public enum SHGSI : uint
    {
        SHGSI_ICONLOCATION = 0,
        SHGSI_ICON = 0x000000100,
        SHGSI_SYSICONINDEX = 0x000004000,
        SHGSI_LINKOVERLAY = 0x000008000,
        SHGSI_SELECTED = 0x000010000,
        SHGSI_LARGEICON = 0x000000000,
        SHGSI_SMALLICON = 0x000000001,
        SHGSI_SHELLICONSIZE = 0x000000004
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SHSTOCKICONINFO
    {
        public UInt32 cbSize;
        public IntPtr hIcon;
        public Int32 iSysIconIndex;
        public Int32 iIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260/*MAX_PATH*/)]
        public string szPath;
    }

    public enum SHSTOCKICONID : uint
    {
        SIID_HELP = 23,
        SIID_WARNING = 78,
        SIID_INFO = 79,
        SIID_ERROR = 80,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_BASIC_INFORMATION
    {
        public int ExitStatus;
        public IntPtr PebBaseAddress;
        public IntPtr AffinityMask;
        public int BasePriority;
        public IntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PEB
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] Reserved1;
        public byte BeingDebugged;
        public byte Reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] Reserved3;
        public IntPtr Ldr;
        public IntPtr ProcessParameters;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RTL_USER_PROCESS_PARAMETERS
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public IntPtr[] Reserved2;
        public UNICODE_STRING ImagePathName;
        public UNICODE_STRING CommandLine;
    }

    public struct TOKEN_USER
    {
        public SID_AND_ATTRIBUTES User;
    }

    public struct SID_AND_ATTRIBUTES
    {
        public IntPtr Sid;
        public int Attributes;
    }

    public enum SID_NAME_USE
    {
        SidTypeUser = 1,
        SidTypeGroup,
        SidTypeDomain,
        SidTypeAlias,
        SidTypeWellKnownGroup,
        SidTypeDeletedAccount,
        SidTypeInvalid,
        SidTypeUnknown,
        SidTypeComputer
    }

    public enum TOKEN_INFORMATION_CLASS
    {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics,
        TokenRestrictedSids,
        TokenSessionId
    }

    public enum ProcessTokenAccessFlags : uint
    {
        Query = 0x0008,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LASTINPUTINFO
    {
        public static readonly uint SizeOf = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dwTime;
    }

    public enum SystemMetric : int
    {
        /// <summary>
        /// The flags that specify how the system arranged minimized windows. For more information, see the Remarks section in this topic.
        /// </summary>
        SM_ARRANGE = 56,

        /// <summary>
        /// The value that specifies how the system is started:
        /// 0 Normal boot
        /// 1 Fail-safe boot
        /// 2 Fail-safe with network boot
        /// A fail-safe boot (also called SafeBoot, Safe Mode, or Clean Boot) bypasses the user startup files.
        /// </summary>
        SM_CLEANBOOT = 67,

        /// <summary>
        /// The number of display monitors on a desktop. For more information, see the Remarks section in this topic.
        /// </summary>
        SM_CMONITORS = 80,

        /// <summary>
        /// The number of buttons on a mouse, or zero if no mouse is installed.
        /// </summary>
        SM_CMOUSEBUTTONS = 43,

        /// <summary>
        /// The width of a window border, in pixels. This is equivalent to the SM_CXEDGE value for windows with the 3-D look.
        /// </summary>
        SM_CXBORDER = 5,

        /// <summary>
        /// The width of a cursor, in pixels. The system cannot create cursors of other sizes.
        /// </summary>
        SM_CXCURSOR = 13,

        /// <summary>
        /// This value is the same as SM_CXFIXEDFRAME.
        /// </summary>
        SM_CXDLGFRAME = 7,

        /// <summary>
        /// The width of the rectangle around the location of a first click in a double-click sequence, in pixels. ,
        /// The second click must occur within the rectangle that is defined by SM_CXDOUBLECLK and SM_CYDOUBLECLK for the system
        /// to consider the two clicks a double-click. The two clicks must also occur within a specified time.
        /// To set the width of the double-click rectangle, call SystemParametersInfo with SPI_SETDOUBLECLKWIDTH.
        /// </summary>
        SM_CXDOUBLECLK = 36,

        /// <summary>
        /// The number of pixels on either side of a mouse-down point that the mouse pointer can move before a drag operation begins.
        /// This allows the user to click and release the mouse button easily without unintentionally starting a drag operation.
        /// If this value is negative, it is subtracted from the left of the mouse-down point and added to the right of it.
        /// </summary>
        SM_CXDRAG = 68,

        /// <summary>
        /// The width of a 3-D border, in pixels. This metric is the 3-D counterpart of SM_CXBORDER.
        /// </summary>
        SM_CXEDGE = 45,

        /// <summary>
        /// The thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels.
        /// SM_CXFIXEDFRAME is the height of the horizontal border, and SM_CYFIXEDFRAME is the width of the vertical border.
        /// This value is the same as SM_CXDLGFRAME.
        /// </summary>
        SM_CXFIXEDFRAME = 7,

        /// <summary>
        /// The width of the left and right edges of the focus rectangle that the DrawFocusRectdraws.
        /// This value is in pixels.
        /// Windows 2000:  This value is not supported.
        /// </summary>
        SM_CXFOCUSBORDER = 83,

        /// <summary>
        /// This value is the same as SM_CXSIZEFRAME.
        /// </summary>
        SM_CXFRAME = 32,

        /// <summary>
        /// The width of the client area for a full-screen window on the primary display monitor, in pixels.
        /// To get the coordinates of the portion of the screen that is not obscured by the system taskbar or by application desktop toolbars,
        /// call the SystemParametersInfofunction with the SPI_GETWORKAREA value.
        /// </summary>
        SM_CXFULLSCREEN = 16,

        /// <summary>
        /// The width of the arrow bitmap on a horizontal scroll bar, in pixels.
        /// </summary>
        SM_CXHSCROLL = 21,

        /// <summary>
        /// The width of the thumb box in a horizontal scroll bar, in pixels.
        /// </summary>
        SM_CXHTHUMB = 10,

        /// <summary>
        /// The default width of an icon, in pixels. The LoadIcon function can load only icons with the dimensions
        /// that SM_CXICON and SM_CYICON specifies.
        /// </summary>
        SM_CXICON = 11,

        /// <summary>
        /// The width of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size
        /// SM_CXICONSPACING by SM_CYICONSPACING when arranged. This value is always greater than or equal to SM_CXICON.
        /// </summary>
        SM_CXICONSPACING = 38,

        /// <summary>
        /// The default width, in pixels, of a maximized top-level window on the primary display monitor.
        /// </summary>
        SM_CXMAXIMIZED = 61,

        /// <summary>
        /// The default maximum width of a window that has a caption and sizing borders, in pixels.
        /// This metric refers to the entire desktop. The user cannot drag the window frame to a size larger than these dimensions.
        /// A window can override this value by processing the WM_GETMINMAXINFO message.
        /// </summary>
        SM_CXMAXTRACK = 59,

        /// <summary>
        /// The width of the default menu check-mark bitmap, in pixels.
        /// </summary>
        SM_CXMENUCHECK = 71,

        /// <summary>
        /// The width of menu bar buttons, such as the child window close button that is used in the multiple document interface, in pixels.
        /// </summary>
        SM_CXMENUSIZE = 54,

        /// <summary>
        /// The minimum width of a window, in pixels.
        /// </summary>
        SM_CXMIN = 28,

        /// <summary>
        /// The width of a minimized window, in pixels.
        /// </summary>
        SM_CXMINIMIZED = 57,

        /// <summary>
        /// The width of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged.
        /// This value is always greater than or equal to SM_CXMINIMIZED.
        /// </summary>
        SM_CXMINSPACING = 47,

        /// <summary>
        /// The minimum tracking width of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions.
        /// A window can override this value by processing the WM_GETMINMAXINFO message.
        /// </summary>
        SM_CXMINTRACK = 34,

        /// <summary>
        /// The amount of border padding for captioned windows, in pixels. Windows XP/2000:  This value is not supported.
        /// </summary>
        SM_CXPADDEDBORDER = 92,

        /// <summary>
        /// The width of the screen of the primary display monitor, in pixels. This is the same value obtained by calling 
        /// GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, HORZRES).
        /// </summary>
        SM_CXSCREEN = 0,

        /// <summary>
        /// The width of a button in a window caption or title bar, in pixels.
        /// </summary>
        SM_CXSIZE = 30,

        /// <summary>
        /// The thickness of the sizing border around the perimeter of a window that can be resized, in pixels.
        /// SM_CXSIZEFRAME is the width of the horizontal border, and SM_CYSIZEFRAME is the height of the vertical border.
        /// This value is the same as SM_CXFRAME.
        /// </summary>
        SM_CXSIZEFRAME = 32,

        /// <summary>
        /// The recommended width of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
        /// </summary>
        SM_CXSMICON = 49,

        /// <summary>
        /// The width of small caption buttons, in pixels.
        /// </summary>
        SM_CXSMSIZE = 52,

        /// <summary>
        /// The width of the virtual screen, in pixels. The virtual screen is the bounding rectangle of all display monitors.
        /// The SM_XVIRTUALSCREEN metric is the coordinates for the left side of the virtual screen.
        /// </summary>
        SM_CXVIRTUALSCREEN = 78,

        /// <summary>
        /// The width of a vertical scroll bar, in pixels.
        /// </summary>
        SM_CXVSCROLL = 2,

        /// <summary>
        /// The height of a window border, in pixels. This is equivalent to the SM_CYEDGE value for windows with the 3-D look.
        /// </summary>
        SM_CYBORDER = 6,

        /// <summary>
        /// The height of a caption area, in pixels.
        /// </summary>
        SM_CYCAPTION = 4,

        /// <summary>
        /// The height of a cursor, in pixels. The system cannot create cursors of other sizes.
        /// </summary>
        SM_CYCURSOR = 14,

        /// <summary>
        /// This value is the same as SM_CYFIXEDFRAME.
        /// </summary>
        SM_CYDLGFRAME = 8,

        /// <summary>
        /// The height of the rectangle around the location of a first click in a double-click sequence, in pixels.
        /// The second click must occur within the rectangle defined by SM_CXDOUBLECLK and SM_CYDOUBLECLK for the system to consider
        /// the two clicks a double-click. The two clicks must also occur within a specified time. To set the height of the double-click
        /// rectangle, call SystemParametersInfo with SPI_SETDOUBLECLKHEIGHT.
        /// </summary>
        SM_CYDOUBLECLK = 37,

        /// <summary>
        /// The number of pixels above and below a mouse-down point that the mouse pointer can move before a drag operation begins.
        /// This allows the user to click and release the mouse button easily without unintentionally starting a drag operation.
        /// If this value is negative, it is subtracted from above the mouse-down point and added below it.
        /// </summary>
        SM_CYDRAG = 69,

        /// <summary>
        /// The height of a 3-D border, in pixels. This is the 3-D counterpart of SM_CYBORDER.
        /// </summary>
        SM_CYEDGE = 46,

        /// <summary>
        /// The thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels.
        /// SM_CXFIXEDFRAME is the height of the horizontal border, and SM_CYFIXEDFRAME is the width of the vertical border.
        /// This value is the same as SM_CYDLGFRAME.
        /// </summary>
        SM_CYFIXEDFRAME = 8,

        /// <summary>
        /// The height of the top and bottom edges of the focus rectangle drawn byDrawFocusRect.
        /// This value is in pixels.
        /// Windows 2000:  This value is not supported.
        /// </summary>
        SM_CYFOCUSBORDER = 84,

        /// <summary>
        /// This value is the same as SM_CYSIZEFRAME.
        /// </summary>
        SM_CYFRAME = 33,

        /// <summary>
        /// The height of the client area for a full-screen window on the primary display monitor, in pixels.
        /// To get the coordinates of the portion of the screen not obscured by the system taskbar or by application desktop toolbars,
        /// call the SystemParametersInfo function with the SPI_GETWORKAREA value.
        /// </summary>
        SM_CYFULLSCREEN = 17,

        /// <summary>
        /// The height of a horizontal scroll bar, in pixels.
        /// </summary>
        SM_CYHSCROLL = 3,

        /// <summary>
        /// The default height of an icon, in pixels. The LoadIcon function can load only icons with the dimensions SM_CXICON and SM_CYICON.
        /// </summary>
        SM_CYICON = 12,

        /// <summary>
        /// The height of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size
        /// SM_CXICONSPACING by SM_CYICONSPACING when arranged. This value is always greater than or equal to SM_CYICON.
        /// </summary>
        SM_CYICONSPACING = 39,

        /// <summary>
        /// For double byte character set versions of the system, this is the height of the Kanji window at the bottom of the screen, in pixels.
        /// </summary>
        SM_CYKANJIWINDOW = 18,

        /// <summary>
        /// The default height, in pixels, of a maximized top-level window on the primary display monitor.
        /// </summary>
        SM_CYMAXIMIZED = 62,

        /// <summary>
        /// The default maximum height of a window that has a caption and sizing borders, in pixels. This metric refers to the entire desktop.
        /// The user cannot drag the window frame to a size larger than these dimensions. A window can override this value by processing
        /// the WM_GETMINMAXINFO message.
        /// </summary>
        SM_CYMAXTRACK = 60,

        /// <summary>
        /// The height of a single-line menu bar, in pixels.
        /// </summary>
        SM_CYMENU = 15,

        /// <summary>
        /// The height of the default menu check-mark bitmap, in pixels.
        /// </summary>
        SM_CYMENUCHECK = 72,

        /// <summary>
        /// The height of menu bar buttons, such as the child window close button that is used in the multiple document interface, in pixels.
        /// </summary>
        SM_CYMENUSIZE = 55,

        /// <summary>
        /// The minimum height of a window, in pixels.
        /// </summary>
        SM_CYMIN = 29,

        /// <summary>
        /// The height of a minimized window, in pixels.
        /// </summary>
        SM_CYMINIMIZED = 58,

        /// <summary>
        /// The height of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged.
        /// This value is always greater than or equal to SM_CYMINIMIZED.
        /// </summary>
        SM_CYMINSPACING = 48,

        /// <summary>
        /// The minimum tracking height of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions.
        /// A window can override this value by processing the WM_GETMINMAXINFO message.
        /// </summary>
        SM_CYMINTRACK = 35,

        /// <summary>
        /// The height of the screen of the primary display monitor, in pixels. This is the same value obtained by calling 
        /// GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, VERTRES).
        /// </summary>
        SM_CYSCREEN = 1,

        /// <summary>
        /// The height of a button in a window caption or title bar, in pixels.
        /// </summary>
        SM_CYSIZE = 31,

        /// <summary>
        /// The thickness of the sizing border around the perimeter of a window that can be resized, in pixels.
        /// SM_CXSIZEFRAME is the width of the horizontal border, and SM_CYSIZEFRAME is the height of the vertical border.
        /// This value is the same as SM_CYFRAME.
        /// </summary>
        SM_CYSIZEFRAME = 33,

        /// <summary>
        /// The height of a small caption, in pixels.
        /// </summary>
        SM_CYSMCAPTION = 51,

        /// <summary>
        /// The recommended height of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
        /// </summary>
        SM_CYSMICON = 50,

        /// <summary>
        /// The height of small caption buttons, in pixels.
        /// </summary>
        SM_CYSMSIZE = 53,

        /// <summary>
        /// The height of the virtual screen, in pixels. The virtual screen is the bounding rectangle of all display monitors.
        /// The SM_YVIRTUALSCREEN metric is the coordinates for the top of the virtual screen.
        /// </summary>
        SM_CYVIRTUALSCREEN = 79,

        /// <summary>
        /// The height of the arrow bitmap on a vertical scroll bar, in pixels.
        /// </summary>
        SM_CYVSCROLL = 20,

        /// <summary>
        /// The height of the thumb box in a vertical scroll bar, in pixels.
        /// </summary>
        SM_CYVTHUMB = 9,

        /// <summary>
        /// Nonzero if User32.dll supports DBCS; otherwise, 0.
        /// </summary>
        SM_DBCSENABLED = 42,

        /// <summary>
        /// Nonzero if the debug version of User.exe is installed; otherwise, 0.
        /// </summary>
        SM_DEBUG = 22,

        /// <summary>
        /// Nonzero if the current operating system is Windows 7 or Windows Server 2008 R2 and the Tablet PC Input
        /// service is started; otherwise, 0. The return value is a bitmask that specifies the type of digitizer input supported by the device.
        /// For more information, see Remarks.
        /// Windows Server 2008, Windows Vista, and Windows XP/2000:  This value is not supported.
        /// </summary>
        SM_DIGITIZER = 94,

        /// <summary>
        /// Nonzero if Input Method Manager/Input Method Editor features are enabled; otherwise, 0.
        /// SM_IMMENABLED indicates whether the system is ready to use a Unicode-based IME on a Unicode application.
        /// To ensure that a language-dependent IME works, check SM_DBCSENABLED and the system ANSI code page.
        /// Otherwise the ANSI-to-Unicode conversion may not be performed correctly, or some components like fonts
        /// or registry settings may not be present.
        /// </summary>
        SM_IMMENABLED = 82,

        /// <summary>
        /// Nonzero if there are digitizers in the system; otherwise, 0. SM_MAXIMUMTOUCHES returns the aggregate maximum of the
        /// maximum number of contacts supported by every digitizer in the system. If the system has only single-touch digitizers,
        /// the return value is 1. If the system has multi-touch digitizers, the return value is the number of simultaneous contacts
        /// the hardware can provide. Windows Server 2008, Windows Vista, and Windows XP/2000:  This value is not supported.
        /// </summary>
        SM_MAXIMUMTOUCHES = 95,

        /// <summary>
        /// Nonzero if the current operating system is the Windows XP, Media Center Edition, 0 if not.
        /// </summary>
        SM_MEDIACENTER = 87,

        /// <summary>
        /// Nonzero if drop-down menus are right-aligned with the corresponding menu-bar item; 0 if the menus are left-aligned.
        /// </summary>
        SM_MENUDROPALIGNMENT = 40,

        /// <summary>
        /// Nonzero if the system is enabled for Hebrew and Arabic languages, 0 if not.
        /// </summary>
        SM_MIDEASTENABLED = 74,

        /// <summary>
        /// Nonzero if a mouse is installed; otherwise, 0. This value is rarely zero, because of support for virtual mice and because
        /// some systems detect the presence of the port instead of the presence of a mouse.
        /// </summary>
        SM_MOUSEPRESENT = 19,

        /// <summary>
        /// Nonzero if a mouse with a horizontal scroll wheel is installed; otherwise 0.
        /// </summary>
        SM_MOUSEHORIZONTALWHEELPRESENT = 91,

        /// <summary>
        /// Nonzero if a mouse with a vertical scroll wheel is installed; otherwise 0.
        /// </summary>
        SM_MOUSEWHEELPRESENT = 75,

        /// <summary>
        /// The least significant bit is set if a network is present; otherwise, it is cleared. The other bits are reserved for future use.
        /// </summary>
        SM_NETWORK = 63,

        /// <summary>
        /// Nonzero if the Microsoft Windows for Pen computing extensions are installed; zero otherwise.
        /// </summary>
        SM_PENWINDOWS = 41,

        /// <summary>
        /// This system metric is used in a Terminal Services environment to determine if the current Terminal Server session is
        /// being remotely controlled. Its value is nonzero if the current session is remotely controlled; otherwise, 0.
        /// You can use terminal services management tools such as Terminal Services Manager (tsadmin.msc) and shadow.exe to
        /// control a remote session. When a session is being remotely controlled, another user can view the contents of that session
        /// and potentially interact with it.
        /// </summary>
        SM_REMOTECONTROL = 0x2001,

        /// <summary>
        /// This system metric is used in a Terminal Services environment. If the calling process is associated with a Terminal Services
        /// client session, the return value is nonzero. If the calling process is associated with the Terminal Services console session,
        /// the return value is 0.
        /// Windows Server 2003 and Windows XP:  The console session is not necessarily the physical console.
        /// For more information, seeWTSGetActiveConsoleSessionId.
        /// </summary>
        SM_REMOTESESSION = 0x1000,

        /// <summary>
        /// Nonzero if all the display monitors have the same color format, otherwise, 0. Two displays can have the same bit depth,
        /// but different color formats. For example, the red, green, and blue pixels can be encoded with different numbers of bits,
        /// or those bits can be located in different places in a pixel color value.
        /// </summary>
        SM_SAMEDISPLAYFORMAT = 81,

        /// <summary>
        /// This system metric should be ignored; it always returns 0.
        /// </summary>
        SM_SECURE = 44,

        /// <summary>
        /// The build number if the system is Windows Server 2003 R2; otherwise, 0.
        /// </summary>
        SM_SERVERR2 = 89,

        /// <summary>
        /// Nonzero if the user requires an application to present information visually in situations where it would otherwise present
        /// the information only in audible form; otherwise, 0.
        /// </summary>
        SM_SHOWSOUNDS = 70,

        /// <summary>
        /// Nonzero if the current session is shutting down; otherwise, 0. Windows 2000:  This value is not supported.
        /// </summary>
        SM_SHUTTINGDOWN = 0x2000,

        /// <summary>
        /// Nonzero if the computer has a low-end (slow) processor; otherwise, 0.
        /// </summary>
        SM_SLOWMACHINE = 73,

        /// <summary>
        /// Nonzero if the current operating system is Windows 7 Starter Edition, Windows Vista Starter, or Windows XP Starter Edition; otherwise, 0.
        /// </summary>
        SM_STARTER = 88,

        /// <summary>
        /// Nonzero if the meanings of the left and right mouse buttons are swapped; otherwise, 0.
        /// </summary>
        SM_SWAPBUTTON = 23,

        /// <summary>
        /// Nonzero if the current operating system is the Windows XP Tablet PC edition or if the current operating system is Windows Vista
        /// or Windows 7 and the Tablet PC Input service is started; otherwise, 0. The SM_DIGITIZER setting indicates the type of digitizer
        /// input supported by a device running Windows 7 or Windows Server 2008 R2. For more information, see Remarks.
        /// </summary>
        SM_TABLETPC = 86,

        /// <summary>
        /// The coordinates for the left side of the virtual screen. The virtual screen is the bounding rectangle of all display monitors.
        /// The SM_CXVIRTUALSCREEN metric is the width of the virtual screen.
        /// </summary>
        SM_XVIRTUALSCREEN = 76,

        /// <summary>
        /// The coordinates for the top of the virtual screen. The virtual screen is the bounding rectangle of all display monitors.
        /// The SM_CYVIRTUALSCREEN metric is the height of the virtual screen.
        /// </summary>
        SM_YVIRTUALSCREEN = 77,
    }

    public enum GWL
    {
        GWL_WNDPROC = (-4),
        GWL_HINSTANCE = (-6),
        GWL_HWNDPARENT = (-8),
        GWL_STYLE = (-16),
        GWL_EXSTYLE = (-20),
        GWL_USERDATA = (-21),
        GWL_ID = (-12)
    }
}
