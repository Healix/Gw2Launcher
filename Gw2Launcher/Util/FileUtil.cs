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
                var security = Directory.GetAccessControl(path, AccessControlSections.Access);
                var usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                var rules = security.GetAccessRules(true, false, typeof(SecurityIdentifier));
                var inheritance = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

                foreach (FileSystemAccessRule rule in rules)
                {
                    if (rule.IdentityReference == usersSid)
                    {
                        if (rule.AccessControlType == AccessControlType.Allow)
                        {
                            if ((rule.FileSystemRights & rights) == rights && (rule.InheritanceFlags & inheritance) == inheritance)
                                return true;
                            rights |= rule.FileSystemRights;
                        }
                        break;
                    }
                }

                security = new System.Security.AccessControl.DirectorySecurity();
                security.AddAccessRule(new FileSystemAccessRule(usersSid, rights, inheritance, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(path, security);

                return true;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return false;
            }
        }

        public static bool HasFolderPermissions(string path, FileSystemRights rights)
        {
            var security = Directory.GetAccessControl(path, AccessControlSections.Access);
            var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));
            var usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            var inheritance = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference == usersSid)
                {
                    if (rule.AccessControlType == AccessControlType.Allow)
                    {
                        if ((rule.FileSystemRights & rights) == rights && (rule.InheritanceFlags & inheritance) == inheritance)
                            return true;
                    }
                    break;
                }
            }

            return false;
        }
        
        public static string GetTemporaryFileName(string folder)
        {
            return GetTemporaryFileName(folder, "{0}.tmp");
        }

        /// <summary>
        /// Returns a file name that doesn't exist
        /// </summary>
        /// <param name="folder">Folder where the file should be</param>
        /// <param name="format">Name format (example-{0}.tmp)</param>
        /// <returns>Path or null if all names were taken</returns>
        public static string GetTemporaryFileName(string folder, string format)
        {
            int i = 0;
            Random r = new Random();
            string temp;
            do
            {
                temp = Path.Combine(folder, string.Format(format, (i++ + r.Next(0x1000, 0xffff)).ToString("x")));
            }
            while (File.Exists(temp) && i < 100);
            if (i == 100 && File.Exists(temp))
            {
                try
                {
                    File.Delete(temp);
                }
                catch
                {
                    return null;
                }
            }
            return temp;
        }

        public static string GetTemporaryFolderName(string folder)
        {
            return GetTemporaryFolderName(folder, "{0}");
        }

        public static string GetTemporaryFolderName(string folder, string format)
        {
            int i = 0;
            Random r = new Random();
            string temp;
            do
            {
                temp = Path.Combine(folder, string.Format(format, (i++ + r.Next(0x1000, 0xffff)).ToString("x")));
            }
            while (Directory.Exists(temp) && i < 100);
            if (i == 100 && Directory.Exists(temp))
            {
                try
                {
                    Directory.Delete(temp);
                }
                catch
                {
                    return null;
                }
            }
            return temp;
        }

        public static string ReplaceInvalidFileNameChars(string filename, char replaceWith)
        {
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            StringBuilder sb = new StringBuilder(filename);
            foreach (var c in invalid)
                sb.Replace(c, replaceWith);
            return sb.ToString();
        }

        /// <summary>
        /// Removes directory separator characters from the end of the path
        /// </summary>
        public static string GetTrimmedDirectoryPath(string path)
        {
            try
            {
                path = Path.GetFullPath(path);
            }
            catch
            {
                return null;
            }

            var l = path.Length;
            if (l > 1 && (path[l - 1] == Path.DirectorySeparatorChar || path[l - 1] == Path.AltDirectorySeparatorChar) && path[l - 2] != Path.VolumeSeparatorChar)
                return path.Substring(0, l - 1);
            return path;
        }

        public static byte GetExecutableBits(string path)
        {
            try
            {
                using (var reader = new BinaryReader(new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete), 1024)))
                {
                    if (reader.ReadInt16() == 23117) //DOS signature
                    {
                        reader.BaseStream.Position = 60; //PE pointer offset
                        reader.BaseStream.Position = reader.ReadInt32() + 4 + 20; //4-byte PE pointer + 4-byte PE signature + 20-byte COFF header

                        var signature = reader.ReadInt16();

                        switch (signature)
                        {
                            case 267: //32-bit
                                return 32;
                            case 523: //64-bit
                                return 64;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return 0;
        }

        public static bool Is32BitExecutable(string path)
        {
            return GetExecutableBits(path) == 32;
        }

        public static bool Is64BitExecutable(string path)
        {
            return GetExecutableBits(path) == 64;
        }

        /// <summary>
        /// Deletes a directory and its content; does not crawl subdirectory links
        /// </summary>
        public static void DeleteDirectory(string path)
        {
            DeleteDirectory(path, false, false);
        }

        /// <summary>
        /// Deletes a directory and its content; does not crawl subdirectory links
        /// </summary>
        /// <param name="checkInitialPath">If the initial path is a link, it'll be deleted instead of crawled</param>
        public static void DeleteDirectory(string path, bool checkInitialPath)
        {
            DeleteDirectory(path, checkInitialPath, false);
        }

        /// <summary>
        /// Deletes a directory and its content; does not crawl subdirectory links
        /// </summary>
        /// <param name="checkInitialPath">If the initial path is a link, it'll be deleted instead of crawled</param>
        /// <param name="deleteReadOnlySystem">Allows deletion of read-only/system directories</param>
        public static void DeleteDirectory(string path, bool checkInitialPath, bool deleteReadOnlySystem)
        {
            if (checkInitialPath)
            {
                var a = File.GetAttributes(path);

                if (deleteReadOnlySystem)
                {
                    var a2 = a & ~(FileAttributes.ReadOnly | FileAttributes.System);
                    if (a != a2)
                    {
                        File.SetAttributes(path, a2);
                    }
                }

                if ((a & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {
                    Directory.Delete(path);
                    return;
                }
            }

            var q = new Stack<string>();
            q.Push(path);

            do
            {
                foreach (var d in Directory.GetDirectories(q.Pop()))
                {
                    var a = File.GetAttributes(d);

                    if (deleteReadOnlySystem)
                    {
                        var a2 = a & ~(FileAttributes.ReadOnly | FileAttributes.System);
                        if (a != a2)
                        {
                            File.SetAttributes(d, a2);
                        }
                    }

                    if ((a & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {
                        Directory.Delete(d);
                    }
                    else
                    {
                        q.Push(d);
                    }
                }
            }
            while (q.Count > 0);

            Directory.Delete(path, true);
        }

        public static bool CanWriteToFolder(string path)
        {
            try
            {
                var n = GetTemporaryFolderName(path);

                Directory.CreateDirectory(n);
                Directory.Delete(n);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the file
        /// </summary>
        /// <param name="inheritPermissions">True to inherit permissions from the destination directory</param>
        public static void MoveFile(string from, string to, bool overwrite, bool inheritPermissions)
        {
            if (overwrite && File.Exists(to))
                File.Delete(to);

            File.Move(from, to);

            if (inheritPermissions)
            {
                var ac = File.GetAccessControl(to);
                ac.SetAccessRuleProtection(false, false);
                File.SetAccessControl(to, ac);
            }
        }
    }
}
