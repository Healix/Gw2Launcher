using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.AccessControl;
using System.IO;
using System.DirectoryServices.AccountManagement;

namespace Gw2Launcher.Util
{
    static class FileUtil
    {
        public static bool AllowFileAccess(string path, FileSystemRights rights)
        {
            try
            {
                var security = new System.Security.AccessControl.FileSecurity();
                var usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                security.AddAccessRule(new FileSystemAccessRule(usersSid, rights, AccessControlType.Allow));
                File.SetAccessControl(path, security);
                return true;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return false;
            }
        }

        public static bool AllowFolderAccess(string path, FileSystemRights rights)
        {
            try
            {
                var security = new System.Security.AccessControl.DirectorySecurity();
                var usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                security.AddAccessRule(new FileSystemAccessRule(usersSid, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(path, security);
                return true;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return false;
            }
        }

        public static string GetTemporaryFileName(string folder)
        {
            int i = 0;
            Random r = new Random();
            string temp;
            do
            {
                temp = Path.Combine(folder, (i++ + r.Next(0x1000, 0xffff)).ToString("x") + ".tmp");
            }
            while (File.Exists(temp));
            return temp;
        }
    }
}
