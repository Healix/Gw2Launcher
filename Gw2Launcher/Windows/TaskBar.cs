using System;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Windows
{
    public class Taskbar
    {
        public enum TaskbarStates
        {
            NoProgress = 0,
            Indeterminate = 0x1,
            Normal = 0x2,
            Error = 0x4,
            Paused = 0x8
        }

        [ComImport()]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig]
            void HrInit();
            [PreserveSig]
            void AddTab(IntPtr hwnd);
            [PreserveSig]
            void DeleteTab(IntPtr hwnd);
            [PreserveSig]
            void ActivateTab(IntPtr hwnd);
            [PreserveSig]
            void SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            [PreserveSig]
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            // ITaskbarList3
            [PreserveSig]
            void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
            [PreserveSig]
            void SetProgressState(IntPtr hwnd, TaskbarStates state);
        }

        [ComImport()]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance
        {
        }

        private ITaskbarList3 instance;
        private static bool isSupported = Environment.OSVersion.Version >= new Version(6, 1);

        public Taskbar()
        {
            instance = (ITaskbarList3)new TaskbarInstance();
        }

        public void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
        {
            if (isSupported)
            {
                try
                {
                    instance.SetProgressState(windowHandle, taskbarState);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        public void SetValue(IntPtr windowHandle, ulong progressValue, ulong progressMax)
        {
            if (isSupported)
            {
                try
                {
                    instance.SetProgressValue(windowHandle, progressValue, progressMax);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        public bool IsSupported
        {
            get
            {
                return isSupported;
            }
        }
    }
}
