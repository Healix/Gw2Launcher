using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Messaging
{
    static class Messager
    {
        public delegate bool SendCallbackEventHandler(IntPtr hWnd, uint Msg, IntPtr lResult);

        public static readonly uint WM_GW2LAUNCHER = NativeMethods.RegisterWindowMessage("Gw2Launcher_Message");
        public const int BROADCAST = 0xffff;

        private static event SendMessageDelegate MessageCallback;
        private static SendMessageDelegate messageCallback;

        public enum MessageType
        {
            None = 0,
            Show = 1,
            Launch = 2,
            LaunchMap = 3,
            UpdateMap = 4,
            QuickLaunch = 5,
            TotpCode = 6,
        }

        public static bool Post(IntPtr hWnd, MessageType type, int value)
        {
            return NativeMethods.PostMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, (IntPtr)value);
        }

        public static bool Post(IntPtr hWnd, MessageType type, IntPtr value)
        {
            return NativeMethods.PostMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, value);
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
            return NativeMethods.SendMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, (IntPtr)value);
        }

        public static IntPtr Send(IntPtr hWnd, MessageType type, IntPtr value)
        {
            return NativeMethods.SendMessage(hWnd, WM_GW2LAUNCHER, (IntPtr)type, value);
        }

        public static IntPtr Send(MessageType type, int value)
        {
            return Send((IntPtr)BROADCAST, type, value);
        }

        public static IntPtr Send(MessageType type, IntPtr value)
        {
            return Send((IntPtr)BROADCAST, type, value);
        }

        private static void OnSendMessageCallback(IntPtr hWnd, uint uMsg, UIntPtr dwData, IntPtr lResult)
        {
            if (MessageCallback != null)
            {
                MessageCallback(hWnd, uMsg, dwData, lResult);
            }
        }

        public static bool SendCallback(MessageType type, int value, int timeout, SendCallbackEventHandler callback)
        {
            bool handled = false;

            if (messageCallback == null)
            {
                messageCallback = new SendMessageDelegate(OnSendMessageCallback);
            }

            SendMessageDelegate _callback = delegate(IntPtr hWnd, uint uMsg, UIntPtr dwData, IntPtr lResult)
            {
                if (!handled && callback(hWnd, uMsg, lResult))
                    handled = true;
            };

            MessageCallback += _callback;

            //warning: callbacks can occur whenever, so the callback must be kept alive until the program terminates, otherwise it'll crash

            NativeMethods.SendMessageCallback((IntPtr)BROADCAST, WM_GW2LAUNCHER, (UIntPtr)type, (IntPtr)value, messageCallback, UIntPtr.Zero);

            var message = new NativeMessage();
            var limit = DateTime.UtcNow.AddMilliseconds(timeout);
            do
            {
                System.Threading.Thread.Sleep(5);
                NativeMethods.PeekMessage(ref message, IntPtr.Zero, 0, 0, 0);
            }
            while (DateTime.UtcNow < limit && !handled);

            MessageCallback -= _callback;

            return handled;
        }
    }
}
