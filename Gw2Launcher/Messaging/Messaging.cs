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

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendMessageCallback(IntPtr hWnd, uint Msg, UIntPtr wParam,
            IntPtr lParam, SendMessageDelegate lpCallBack, UIntPtr dwData);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PeekMessage(ref NativeMessage message, IntPtr handle, uint filterMin, uint filterMax, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        delegate void SendMessageDelegate(IntPtr hWnd, uint uMsg, UIntPtr dwData, IntPtr lResult);
        public delegate bool SendCallbackEventHandler(IntPtr hWnd, uint Msg, IntPtr lResult);

        public static readonly uint WM_GW2LAUNCHER = RegisterWindowMessage("Gw2Launcher_Message");
        private const int BROADCAST = 0xffff;

        public enum MessageType
        {
            None = 0,
            Show = 1,
            Launch = 2,
            LaunchMap = 3,
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

        public static IntPtr Send(IntPtr hWnd, MessageType type, int value)
        {
            return SendMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, (IntPtr)value);
        }

        public static IntPtr Send(IntPtr hWnd, MessageType type, IntPtr value)
        {
            return SendMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, value);
        }

        public static IntPtr Send(MessageType type, int value)
        {
            return Send((IntPtr)BROADCAST, type, value);
        }

        public static IntPtr Send(MessageType type, IntPtr value)
        {
            return Send((IntPtr)BROADCAST, type, value);
        }

        public static bool SendCallback(MessageType type, int value, int timeout, SendCallbackEventHandler callback)
        {
            bool handled = false;
            SendMessageDelegate _callback = delegate(IntPtr hWnd, uint uMsg, UIntPtr dwData, IntPtr lResult)
            {
                if (!handled && callback(hWnd, uMsg, lResult))
                    handled = true;
            };

            SendMessageCallback((IntPtr)BROADCAST, WM_GW2LAUNCHER, (UIntPtr)type, (IntPtr)value, _callback, UIntPtr.Zero);

            var message = new NativeMessage();
            var limit = DateTime.UtcNow.AddMilliseconds(timeout);
            do
            {
                System.Threading.Thread.Sleep(5);
                PeekMessage(ref message, IntPtr.Zero, 0, 0, 0);
            }
            while (DateTime.UtcNow < limit && !handled);

            return handled;
        }
    }
}
