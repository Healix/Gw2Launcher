using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    static class Keyboard
    {
        public enum KeyMessage
        {
            /// <summary>
            /// Key down message
            /// </summary>
            Down = 1,
            /// <summary>
            /// Key up message
            /// </summary>
            Up = 2,
            /// <summary>
            /// Key down and up message
            /// </summary>
            Press = 3,
        }
        
        /// <summary>
        /// Returns true if the key is currently pressed
        /// </summary>
        public static bool GetKeyState(short key)
        {
            return (NativeMethods.GetAsyncKeyState(key) & 0x8000) == 0x8000;
        }

        /// <summary>
        /// Returns true if the key is currently pressed
        /// </summary>
        public static bool GetKeyState(Keys key)
        {
            return GetKeyState((short)key);
        }

        /// <summary>
        /// Sets the key state
        /// </summary>
        public static void SetKeyState(Keys key, bool pressed)
        {
            var scan = NativeMethods.MapVirtualKeyEx((uint)key, VkMapType.MAPVK_VK_TO_VSC, IntPtr.Zero);
            //scan = scan << 16 | 1;
            //if (!pressed)
               // scan = scan | 0xC0000001;

            NativeMethods.keybd_event((byte)key, (byte)scan, pressed ? 0 : 2, 0);
        }
        
        /// <summary>
        /// Sends a key message
        /// </summary>
        public static bool SendKey(IntPtr window, Keys key, bool state, bool post)
        {
            return SendKey(window,key, state ? KeyMessage.Down : KeyMessage.Up, post);
        }

        /// <summary>
        /// Sends a key message
        /// </summary>
        /// <param name="post">True to use PostMessage</param>
        /// <param name="count">Number of messages to send</param>
        public static bool SendKey(IntPtr window, Keys key, KeyMessage state, bool post, int count = 1)
        {
            var scan = NativeMethods.MapVirtualKeyEx((uint)key, VkMapType.MAPVK_VK_TO_VSC, IntPtr.Zero);
            var b = true;

             scan = scan << 16 | 1;

             for (var i = 0; i < count; i++)
             {
                 if ((state & KeyMessage.Down) != 0)
                 {
                     //scan = scan << 16 | (uint)1;
                     b = SendMessage(window, WindowMessages.WM_KEYDOWN, (IntPtr)key, (IntPtr)scan, post);
                 }

                 if ((state & KeyMessage.Up) != 0)
                 {
                     //scan = scan << 16 | 1 << 31 | 1 << 30 | 1
                     b = SendMessage(window, WindowMessages.WM_KEYUP, (IntPtr)key, (IntPtr)(scan | 0xC0000001), post);
                 }
             }

            return b;
        }

        private static bool SendMessage(IntPtr window, WindowMessages m, IntPtr key, IntPtr scan, bool post)
        {
            if (post)
            {
                return NativeMethods.PostMessage(window, m, key, scan);
            }
            else
            {
                return NativeMethods.SendMessage(window, m, key, scan) == IntPtr.Zero;
            }
        }

        /// <summary>
        /// Sends a keypress (including modifiers) to the window via messages
        /// </summary>
        public static void SendKeyPress(IntPtr window, Keys keys, bool messages = true)
        {
            var primary = Windows.Hotkeys.GetPrimaryKey(keys);

            if (primary == Keys.None)
                return;

            var modifiers = new Keys[]
            {
                Keys.Control,
                Keys.Shift,
                Keys.Alt,
            }; ;

            var states = new bool[modifiers.Length];
            var requested = new bool[modifiers.Length];
            bool primaryDown = false;

            if (!messages && GetKeyState(primary))
            {
                primaryDown = true;
                SetKeyState(primary, false);
            }

            for (var i = 0; i < modifiers.Length; i++)
            {
                states[i] = GetKeyState(modifiers[i]);
                requested[i] = (keys & modifiers[i]) != 0;

                if (states[i] != requested[i])
                {
                    SetKeyState(modifiers[i], requested[i]);
                }
            }

            if (messages)
            {
                SendKey(window, primary, KeyMessage.Press, false);
            }
            else
            {
                SetKeyState(primary, true);
                SetKeyState(primary, false);
            }

            for (var i = 0; i < states.Length; i++)
            {
                //only releasing keys that were pressed, since any held key could have already been released
                if (states[i] != requested[i] && requested[i])
                {
                    SetKeyState(modifiers[i], false);
                }
            }
        }
    }
}
