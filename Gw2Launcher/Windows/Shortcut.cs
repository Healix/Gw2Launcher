using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace Gw2Launcher.Windows
{
    class Shortcut
    {
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        public class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010c-0000-0000-C000-000000000046")]
        private interface IPersist
        {
            void GetClassID(out Guid pClassID);
        };

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("00000109-0000-0000-C000-000000000046")]
        private interface IPersistStream
        {
            void GetClassID(out Guid pClassID);

            [PreserveSig]
            int IsDirty();
            void Load([In] System.Runtime.InteropServices.ComTypes.IStream pStm);
            void Save([In] System.Runtime.InteropServices.ComTypes.IStream pStm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
            void GetSizeMax(out long pcbSize);
        };

        private class IStreamContainer : System.Runtime.InteropServices.ComTypes.IStream
        {
            protected Stream stream;

            public IStreamContainer(Stream stream)
            {
                this.stream = stream;
            }

            public void Clone(out System.Runtime.InteropServices.ComTypes.IStream ppstm)
            {
                IStreamContainer clone = new IStreamContainer(new MemoryStream((int)this.stream.Length));
                var p = this.stream.Position;
                this.stream.Position = 0;
                this.stream.CopyTo(clone.stream);
                clone.stream.Position = this.stream.Position = p;
                ppstm = clone;
            }

            public void Commit(int grfCommitFlags)
            {
            }

            public void CopyTo(System.Runtime.InteropServices.ComTypes.IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
            {
                long count = this.stream.Length - this.stream.Position;
                if (cb < count)
                    count = cb;
                byte[] buffer = new byte[count];
                int read = this.stream.Read(buffer, 0, (int)count);

                pstm.Write(buffer, read, pcbWritten);

                if (pcbRead != IntPtr.Zero)
                    Marshal.WriteInt64(pcbRead, read);
            }

            public void LockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            public void Read(byte[] pv, int cb, IntPtr pcbRead)
            {
                int read = this.stream.Read(pv, 0, cb);
                if (pcbRead != IntPtr.Zero)
                    Marshal.WriteInt64(pcbRead, read);
            }

            public void Revert()
            {
            }

            public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
            {
                long p = this.stream.Seek(dlibMove, (SeekOrigin)dwOrigin);
                if (plibNewPosition != IntPtr.Zero)
                    Marshal.WriteInt64(plibNewPosition, p);
            }

            public void SetSize(long libNewSize)
            {
                this.stream.SetLength(libNewSize);
            }

            public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
            {
                pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG()
                {
                    cbSize = this.stream.Length,
                    type = 2,
                };
            }

            public void UnlockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            public void Write(byte[] pv, int cb, IntPtr pcbWritten)
            {
                this.stream.Write(pv, 0, cb);
                if (pcbWritten != IntPtr.Zero)
                    Marshal.WriteInt64(pcbWritten, cb);
            }

            public Stream BaseStream
            {
                get
                {
                    return this.stream;
                }
            }
        }

        public Shortcut(string target, string args)
        {
            this.Target = target;
            this.Arguments = args;
            this.WorkingDirectory = Path.GetDirectoryName(target);
        }

        public Shortcut(string target, string args, string comment)
            : this(target, args)
        {
            this.Description = comment;
        }

        public string Target
        {
            get;
            set;
        }

        public string Arguments
        {
            get;
            set;
        }

        public string WorkingDirectory
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string Icon
        {
            get;
            set;
        }

        public string AppUserModelID
        {
            get;
            set;
        }

        public bool PreventPinning
        {
            get;
            set;
        }

        private IShellLink Create()
        {
            var l = Create(this.Target, this.Arguments, this.Description, this.Icon);

            try
            {
                var ps = (WindowProperties.IPropertyStore)l;

                if (this.PreventPinning)
                {
                    using (var pv = new WindowProperties.PropVariant(true))
                    {
                        var pk = WindowProperties.PropertyKey.AppUserModel_PreventPinning;
                        ps.SetValue(ref pk, pv);
                    }
                }

                if (!string.IsNullOrEmpty(this.AppUserModelID))
                {
                    using (var pv = new WindowProperties.PropVariant(this.AppUserModelID))
                    {
                        var pk = WindowProperties.PropertyKey.AppUserModel_ID;
                        ps.SetValue(ref pk, pv);
                    }
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return l;
        }

        public void Save(Stream output)
        {
            var stream = (IPersistStream)Create();
            try
            {
                stream.Save(new IStreamContainer(output), true);
            }
            finally
            {
                Marshal.ReleaseComObject(stream);
            }
        }

        public void Save(string output)
        {
            var file = (System.Runtime.InteropServices.ComTypes.IPersistFile)Create();
            try
            {
                file.Save(output, false);
            }
            finally
            {
                Marshal.ReleaseComObject(file);
            }
        }

        private static IShellLink Create(string target, string args, string comment, string icon)
        {
            IShellLink l = (IShellLink)new ShellLink();
            l.SetPath(target);
            l.SetWorkingDirectory(Path.GetDirectoryName(target));
            l.SetArguments(args);
            if (!string.IsNullOrEmpty(comment))
                l.SetDescription(comment);
            if (!string.IsNullOrEmpty(icon))
                l.SetIconLocation(icon, 0);

            return l;
        }

        public static void Create(string output, string target, string args, string comment, string icon)
        {
            var file = (System.Runtime.InteropServices.ComTypes.IPersistFile)Create(target, args, comment, icon);
            try
            {
                file.Save(output, false);
            }
            finally
            {
                Marshal.ReleaseComObject(file);
            }
        }

        public static void Create(Stream output, string target, string args, string comment, string icon)
        {
            var stream = (IPersistStream)Create(target, args, comment, icon);

            try
            {
                stream.Save(new IStreamContainer(output), true);
            }
            finally
            {
                Marshal.ReleaseComObject(stream);
            }
        }
    }
}
