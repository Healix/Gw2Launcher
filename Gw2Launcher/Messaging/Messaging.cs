using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gw2Launcher.Messaging
{
    static class Messager
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static readonly uint WM_GW2LAUNCHER = RegisterWindowMessage("Gw2Launcher_Message");
        private const int BROADCAST = 0xffff;

        public enum MessageType
        {
            None = 0,
            Show = 1,
            Launch = 2,
            LaunchMap = 3
        }

        public static bool Post(IntPtr hWnd, MessageType type, int value)
        {
            return PostMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, (IntPtr)value);
        }

        public static bool Post(IntPtr hWnd, MessageType type, IntPtr value)
        {
            return PostMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, value);
        }

        public static bool Post(MessageType type, int value)
        {
            return Post((IntPtr)BROADCAST, type, value);
        }

        public static bool Post(MessageType type, IntPtr value)
        {
            return Post((IntPtr)BROADCAST, type, value);
        }

        public static bool Send(IntPtr hWnd, MessageType type, int value)
        {
            return SendMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, (IntPtr)value);
        }

        public static bool Send(IntPtr hWnd, MessageType type, IntPtr value)
        {
            return SendMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, value);
        }

        public static bool Send(MessageType type, int value)
        {
            return Send((IntPtr)BROADCAST, type, value);
        }

        public static bool Send(MessageType type, IntPtr value)
        {
            return Send((IntPtr)BROADCAST, type, value);
        }
    }
}
