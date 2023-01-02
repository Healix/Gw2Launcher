using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Util
{
    static class Explorer
    {
        public static bool OpenFolderAndSelect(string path)
        {
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.FileName = "explorer.exe";
                    p.StartInfo.Arguments = "/select, \"" + path + '"';

                    if (p.Start())
                        return true;
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            return false;
        }

        public static bool OpenFolder(string path)
        {
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.FileName = "explorer.exe";
                    p.StartInfo.Arguments = '"' + path + '"';

                    if (p.Start())
                        return true;
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            return false;
        }

        /// <summary>
        /// Opens an explorer window with the specified files selected
        /// </summary>
        /// <param name="folder">The path to the folder</param>
        /// <param name="filenames">The names of the files to be selected</param>
        /// <returns>True if successful</returns>
        public static bool OpenFolderAndSelect(string folder, params string[] filenames)
        {
            IntPtr pidlFolder = IntPtr.Zero;
            uint psfgao = 0;

            try
            {
                if (NativeMethods.SHParseDisplayName(folder, IntPtr.Zero, ref pidlFolder, 0, ref psfgao) != 0)
                    return false;

                IntPtr[] pidlFiles = new IntPtr[filenames.Length];

                try
                {
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        NativeMethods.SHParseDisplayName(Path.Combine(folder, filenames[i]), IntPtr.Zero, ref pidlFiles[i], 0, ref psfgao);
                    }

                    if (NativeMethods.SHOpenFolderAndSelectItems(pidlFolder, (uint)pidlFiles.Length, pidlFiles, 0) != 0)
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

        public static bool OpenUrl(string url)
        {
            return OpenFolder(url);
        }
    }
}
