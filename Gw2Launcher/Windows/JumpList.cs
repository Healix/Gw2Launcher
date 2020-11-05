using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    class JumpList
    {
        private enum KnownDestinationCategory
        {
            Frequent = 1,
            Recent
        }

        [ComImportAttribute()]
        [GuidAttribute("6332DEBF-87B5-4670-90C0-5E57B408A49E")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ICustomDestinationList
        {
            void SetAppID(
                [MarshalAs(UnmanagedType.LPWStr)] string pszAppID);
            [PreserveSig]
            int BeginList(
                out uint cMaxSlots,
                ref Guid riid,
                [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject);
            [PreserveSig]
            int AppendCategory(
                [MarshalAs(UnmanagedType.LPWStr)] string pszCategory,
                [MarshalAs(UnmanagedType.Interface)] IObjectArray poa);
            void AppendKnownCategory(
                [MarshalAs(UnmanagedType.I4)] KnownDestinationCategory category);
            [PreserveSig]
            int AddUserTasks(
                [MarshalAs(UnmanagedType.Interface)] IObjectArray poa);
            void CommitList();
            void GetRemovedDestinations(
                ref Guid riid,
                [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject);
            void DeleteList(
                [MarshalAs(UnmanagedType.LPWStr)] string pszAppID);
            void AbortList();
        }

        [GuidAttribute("77F10CF0-3DB5-4966-B520-B7C54FD35ED6")]
        [ClassInterfaceAttribute(ClassInterfaceType.None)]
        [ComImportAttribute()]
        private class CDestinationList { }

        [ComImportAttribute()]
        [GuidAttribute("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IObjectArray
        {
            void GetCount(out uint cObjects);
            void GetAt(
                uint iIndex,
                ref Guid riid,
                [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject);
        }

        [GuidAttribute("2D3468C1-36A7-43B6-AC24-D3F02FD9607A")]
        [ClassInterfaceAttribute(ClassInterfaceType.None)]
        [ComImportAttribute()]
        private class CEnumerableObjectCollection { }

        [ComImportAttribute()]
        [GuidAttribute("5632B1A4-E38A-400A-928A-D4CD63230295")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IObjectCollection
        {
            // IObjectArray
            [PreserveSig]
            void GetCount(out uint cObjects);
            [PreserveSig]
            void GetAt(
                uint iIndex,
                ref Guid riid,
                [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject);

            // IObjectCollection
            void AddObject(
                [MarshalAs(UnmanagedType.Interface)] object pvObject);
            void AddFromArray(
                [MarshalAs(UnmanagedType.Interface)] IObjectArray poaSource);
            void RemoveObject(uint uiIndex);
            void Clear();
        }

        public interface IJumpItem
        {
            string CustomCategory
            {
                get;
            }
        }

        public interface IJumpTask : IJumpItem
        {
            string ApplicationPath
            {
                get;
            }

            string Arguments
            {
                get;
            }

            string Description
            {
                get;
            }

            string IconResourcePath
            {
                get;
            }

            int IconResourceIndex
            {
                get;
            }

            string Title
            {
                get;
            }

            string WorkingDirectory
            {
                get;
            }
        }

        private class JumpItemTask : IDisposable
        {
            private static WindowProperties.PropertyKey PKEY_Title = new WindowProperties.PropertyKey(new Guid("{F29F85E0-4FF9-1068-AB91-08002B27B3D9}"), 2);

            private IJumpTask task;
            private Shortcut.IShellLink link;

            public JumpItemTask(IJumpTask task)
            {
                this.task = task;
            }

            ~JumpItemTask()
            {
                Dispose(false);
            }

            public static string GetTitle(Shortcut.IShellLink link)
            {
                try
                {
                    var ps = (WindowProperties.IPropertyStore)link;
                    using (var pv = new WindowProperties.PropVariant())
                    {
                        ps.GetValue(ref PKEY_Title, pv);
                        return (string)pv.GetValue();
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
                return "";
            }

            public Shortcut.IShellLink NativeShellLink
            {
                get
                {
                    if (link == null)
                    {
                        var l = link = (Shortcut.IShellLink)new Shortcut.ShellLink();
                        l.SetPath(task.ApplicationPath);
                        if (!string.IsNullOrEmpty(task.WorkingDirectory))
                            l.SetWorkingDirectory(System.IO.Path.GetDirectoryName(task.ApplicationPath));
                        else
                            l.SetWorkingDirectory(task.WorkingDirectory);
                        if (!string.IsNullOrEmpty(task.Arguments))
                            l.SetArguments(task.Arguments);
                        if (!string.IsNullOrEmpty(task.Description))
                            l.SetDescription(task.Description);
                        if (!string.IsNullOrEmpty(task.IconResourcePath))
                            l.SetIconLocation(task.IconResourcePath, task.IconResourceIndex);

                        if (!string.IsNullOrEmpty(task.Title))
                        {
                            var ps = (WindowProperties.IPropertyStore)l;

                            using (var pv = new WindowProperties.PropVariant(task.Title))
                            {
                                ps.SetValue(PKEY_Title, pv);
                            }

                            ps.Commit();
                        }
                    }

                    return link;
                }
            }

            public void Dispose(bool disposing)
            {
                if (link != null)
                {
                    Marshal.ReleaseComObject(link);
                    link = null;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        public class CustomCategoryException : Exception
        {

        }

        private static Guid GUID_IObjectArray = new Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9");
        private static Guid GUID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        public event EventHandler<IList<IJumpItem>> Removed;

        private List<JumpItemTask> active;
        private List<IJumpTask> tasks;
        private ICustomDestinationList list;

        public JumpList(IntPtr handle)
            : this(NativeMethods.GetCurrentProcessExplicitAppUserModelID(), handle)
        {

        }

        public JumpList(string appId, IntPtr handle)
        {
            tasks = new List<IJumpTask>();
            list = (ICustomDestinationList)new CDestinationList();
            if (!string.IsNullOrEmpty(appId))
            {
                //WindowProperties.SetAppUserModelID(handle, appId);
                list.SetAppID(appId);
            }
        }

        public List<IJumpTask> JumpItems
        {
            get
            {
                return tasks;
            }
        }

        private bool Compare(string a, string b)
        {
            if (a == null && b.Length == 0)
            {
                return true;
            }
            else
            {
                if (b.Length < 256)
                {
                    return a == b;
                }
                else
                {
                    return a.StartsWith(b, StringComparison.Ordinal);
                }
            }
        }

        private void BeginList()
        {
            uint slots;
            object o;

            var r = list.BeginList(out slots, ref GUID_IObjectArray, out o);

            if (r < 0)
            {
                throw Marshal.GetExceptionForHR(r);
            }

            var oa = (IObjectArray)o;
            uint count;

            oa.GetCount(out count);

            if (count > 0)
            {
                var removed = new List<IJumpItem>((int)count);
                var buffer = new StringBuilder(512);

                for (uint i = 0; i < count; ++i)
                {
                    oa.GetAt(i, ref GUID_IUnknown, out o);

                    if (o is Shortcut.IShellLink)
                    {
                        var link = (Shortcut.IShellLink)o;
                        var title = JumpItemTask.GetTitle(link);
                        string args = null;

                        for (var j = tasks.Count - 1; j >= 0; --j)
                        {
                            var t = tasks[j];

                            if (t.Title == title)
                            {
                                if (args == null)
                                {
                                    link.GetArguments(buffer, buffer.Capacity);
                                    args = buffer.ToString();
                                    buffer.Length = 0;
                                }

                                if (Compare(t.Arguments, args))
                                {
                                    tasks.RemoveAt(j);
                                    removed.Add(t);
                                    break;
                                }
                            }
                        }
                    }
                    //else if (o is Dialogs.FileDialogNative.IShellItem)
                    //{
                    //    var item = (Dialogs.FileDialogNative.IShellItem)o;
                    //}
                }

                if (removed.Count > 0)
                {
                    try
                    {
                        if (Removed != null)
                            Removed(this, removed);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }
            }
        }

        public void Apply()
        {
            BeginList();

            try
            {
                if (active == null)
                    active = new List<JumpItemTask>();
                if (active.Count > 0)
                {
                    foreach (var t in active)
                    {
                        t.Dispose();
                    }
                    active.Clear();
                }

                var count = tasks.Count;

                if (count > 0)
                {
                    IObjectCollection content = null;
                    var categories = new Dictionary<string, IObjectCollection>();

                    for (var i = 0; i < count; ++i)
                    {
                        var t = tasks[i];

                        if (i > 0)
                        {
                            if (tasks[i - 1].CustomCategory != t.CustomCategory)
                            {
                                content = null;
                            }
                        }

                        if (content == null)
                        {
                            var c = t.CustomCategory;
                            if (c == null)
                                c = "";
                            if (!categories.TryGetValue(c, out content))
                            {
                                content = categories[c] = (IObjectCollection)new CEnumerableObjectCollection();
                            }
                        }

                        var jit = new JumpItemTask(t);
                        active.Add(jit);
                        content.AddObject(jit.NativeShellLink);
                    }

                    foreach (var k in categories.Keys)
                    {
                        if (k.Length == 0)
                        {
                            list.AddUserTasks((IObjectArray)categories[k]);
                        }
                        else
                        {
                            var r = list.AppendCategory(k, (IObjectArray)categories[k]);
                            if (r < 0)
                            {
                                if (r == -2147024891) //denied (tracking of recent files is disabled)
                                    throw new CustomCategoryException();
                                throw Marshal.GetExceptionForHR(r);
                            }
                        }
                    }
                }
            }
            finally
            {
                list.CommitList();
            }
        }

        public static bool IsSupported
        {
            get
            {
                return Environment.OSVersion.Version >= new Version(6, 1);
            }
        }
    }
}
