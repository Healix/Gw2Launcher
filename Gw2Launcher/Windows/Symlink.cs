using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class Symlink
    {
        private const string VIRTUAL_NTFS_PATH_PREFIX = @"\??\";
        private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
        private const int FSCTL_SET_REPARSE_POINT = 0x000900A4;

        [StructLayout(LayoutKind.Sequential)]
        private struct REPARSE_DATA_BUFFER 
        {
            /// <summary>
            /// Reparse point tag. Must be a Microsoft reparse point tag.
            /// </summary>
            public uint ReparseTag;

            /// <summary>
            /// Size, in bytes, of the data after the Reserved member. This can be calculated by:
            /// (4 * sizeof(ushort)) + SubstituteNameLength + PrintNameLength + 
            /// (namesAreNullTerminated ? 2 * sizeof(char) : 0);
            /// </summary>
            public ushort ReparseDataLength;

            /// <summary>
            /// Reserved; do not use. 
            /// </summary>
            public ushort Reserved;

            /// <summary>
            /// Offset, in bytes, of the substitute name string in the PathBuffer array.
            /// </summary>
            public ushort SubstituteNameOffset;

            /// <summary>
            /// Length, in bytes, of the substitute name string. If this string is null-terminated,
            /// SubstituteNameLength does not include space for the null character.
            /// </summary>
            public ushort SubstituteNameLength;

            /// <summary>
            /// Offset, in bytes, of the print name string in the PathBuffer array.
            /// </summary>
            public ushort PrintNameOffset;

            /// <summary>
            /// Length, in bytes, of the print name string. If this string is null-terminated,
            /// PrintNameLength does not include space for the null character. 
            /// </summary>
            public ushort PrintNameLength;

            /// <summary>
            /// A buffer containing the unicode-encoded path string. The path string contains
            /// the substitute name string and print name string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
        }
        
        private static SafeFileHandle OpenReparsePoint(string reparsePoint, EFileAccess accessMode) 
        {
            SafeFileHandle reparsePointHandle = new SafeFileHandle(NativeMethods.CreateFile(reparsePoint, accessMode,
                EFileShare.Read | EFileShare.Write | EFileShare.Delete,
                IntPtr.Zero, ECreationDisposition.OpenExisting,
                EFileAttributes.BackupSemantics | EFileAttributes.OpenReparsePoint, IntPtr.Zero), true);

            if (Marshal.GetLastWin32Error() != 0)
                throw new Exception("Failed to open reparse point", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

            return reparsePointHandle;
        }

        public static void CreateHardLink(string link, string target)
        {
            if (!NativeMethods.CreateHardLink(link, target, IntPtr.Zero))
                throw new IOException("Failed to create link");
        }

        public static void CreateJunction(string link, string target)
        {
            var di = new DirectoryInfo(link);
            if (di.Exists)
            {
                if (di.GetFiles().Length > 0 || di.GetDirectories().Length > 0)
                    throw new IOException("Directory already exists and is not empty");
            }
            else
                di.Create();

            using (SafeFileHandle handle = OpenReparsePoint(link, EFileAccess.GenericWrite))
            {
                byte[] linkbytes = Encoding.Unicode.GetBytes(VIRTUAL_NTFS_PATH_PREFIX + Path.GetFullPath(target));

                REPARSE_DATA_BUFFER buffer = new REPARSE_DATA_BUFFER();
                buffer.ReparseTag = IO_REPARSE_TAG_MOUNT_POINT;
                buffer.ReparseDataLength = (ushort)(linkbytes.Length + 12);
                buffer.SubstituteNameOffset = 0;
                buffer.SubstituteNameLength = (ushort)linkbytes.Length;
                buffer.PrintNameOffset = (ushort)(linkbytes.Length + 2);
                buffer.PrintNameLength = 0;
                buffer.PathBuffer = new byte[0x3ff0];
                Array.Copy(linkbytes, buffer.PathBuffer, linkbytes.Length);

                int _size = Marshal.SizeOf(buffer);
                IntPtr _buffer = Marshal.AllocHGlobal(_size);

                try 
                {
                    Marshal.StructureToPtr(buffer, _buffer, false);

                    int bytesReturned;
                    bool result = NativeMethods.DeviceIoControl(handle.DangerousGetHandle(), FSCTL_SET_REPARSE_POINT,
                        _buffer, linkbytes.Length + 20, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);

                    if (!result)
                        throw new IOException("Failed to create junction", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                } 
                finally 
                {
                    Marshal.FreeHGlobal(_buffer);
                }
            }
        }
    }
}
