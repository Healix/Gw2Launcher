using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    class FileCopy
    {
        /// <summary>
        /// Progress callback
        /// </summary>
        /// <param name="total">Total bytes</param>
        /// <param name="transferred">Bytes transferred</param>
        /// <returns>True to continue, False to cancel</returns>
        public delegate bool ProgressEventHandler(long total, long transferred);

        private ProgressEventHandler progress;

        private FileCopy(ProgressEventHandler progress)
        {
            this.progress = progress;
        }

        public bool Copy(string from, string to)
        {
            int pbCancel = 0;

            if (!NativeMethods.CopyFileEx(from, to, new NativeMethods.CopyProgressRoutine(CopyProgress), IntPtr.Zero, ref pbCancel, CopyFileFlags.COPY_FILE_RESTARTABLE))
            {
                if (pbCancel == 0)
                    throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());

                return false;
            }

            return true;
        }

        private CopyProgressResult CopyProgress(long total, long transferred, long streamSize, long StreamByteTrans, uint dwStreamNumber, CopyProgressCallbackReason reason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData)
        {
            if (this.progress(total, transferred))
            {
                return CopyProgressResult.PROGRESS_CONTINUE;
            }
            else
            {
                return CopyProgressResult.PROGRESS_CANCEL;
            }
        }

        public static bool Copy(string from, string to, ProgressEventHandler progress)
        {
            return new FileCopy(progress).Copy(from, to);
        }
    }
}
