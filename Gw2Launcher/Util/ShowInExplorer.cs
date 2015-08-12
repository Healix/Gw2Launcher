using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.IO;

namespace Gw2Launcher.Util
{
    public static class ShowInExplorer
    {
        [DllImport("shell32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, ref IntPtr ppidl, uint sfgao, ref uint psfgao);

        [DllImport("shell32.dll", EntryPoint = "SHOpenFolderAndSelectItems")]
        static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, int dwFlags);

        /// <summary>
        /// Opens an explorer window with the specified files selected
        /// </summary>
        /// <param name="folder">The path to the folder</param>
        /// <param name="filenames">The names of the files to be selected</param>
        /// <returns>True if successful</returns>
        public static bool SelectFiles(string folder, params string[] filenames)
        {
            IntPtr pidlFolder = IntPtr.Zero;
            uint psfgao = 0;

            try
            {
                if (SHParseDisplayName(folder, IntPtr.Zero, ref pidlFolder, 0, ref psfgao) != 0)
                    return false;

                IntPtr[] pidlFiles = new IntPtr[filenames.Length];

                try
                {
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        SHParseDisplayName(Path.Combine(folder, filenames[i]), IntPtr.Zero, ref pidlFiles[i], 0, ref psfgao);
                    }

                    if (SHOpenFolderAndSelectItems(pidlFolder, (uint)pidlFiles.Length, pidlFiles, 0) != 0)
                        return false;

                    return true;
                }
                finally
                {
                    foreach (IntPtr p in pidlFiles)
                    {
                        if (p != IntPtr.Zero)
                            Marshal.FreeCoTaskMem(p);
                    }
                }
            }
            finally
            {
                if (pidlFolder != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pidlFolder);
            }
        }
    }
}