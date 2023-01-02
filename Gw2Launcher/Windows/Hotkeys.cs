using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.Windows
{
    public class Hotkeys : IDisposable
    {
        public class HotkeyException : Exception
        {
            public enum Reasons
            {
                InvalidKeys,
                RegisterError,
            }

            public HotkeyException(Reasons reason)
            {
                this.Reason = reason;
            }

            public HotkeyException(Reasons reason, string message, Exception inner)
                : base(message, inner)
            {
                this.Reason = reason;
            }

            public Reasons Reason
            {
                get;
                private set;
            }
        }

        public class HotkeyEventArgs : EventArgs
        {
            public bool SuppressHotkey
            {
                get;
                set;
            }
        }

        public interface IHotkey : IDisposable
        {
            event EventHandler<HotkeyEventArgs> KeyPress;
            object Data
            {
                get;
                set;
            }
        }

        private class Hotkey : IDisposable
        {
            public event EventHandler<HotkeyEventArgs> KeyPress;
            public event EventHandler Disposed;

            private class Subscriber : IHotkey
            {
                public event EventHandler<HotkeyEventArgs> KeyPress;

                private Hotkey hotkey;

                public Subscriber(Hotkey hotkey)
                {
                    this.hotkey = hotkey;
                    hotkey.KeyPress += OnKeyPress;
                }

                ~Subscriber()
                {
                    Dispose();
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);

                    lock (this)
                    {
                        if (hotkey == null)
                            return;

                        KeyPress = null;
                        hotkey.KeyPress -= OnKeyPress;
                        hotkey.Release();
                        hotkey = null;
                    }
                }

                public void OnKeyPress(object sender, HotkeyEventArgs e)
                {
                    if (KeyPress != null)
                    {
                        try
                        {
                            KeyPress(this, e);
                        }
                        catch { }
                    }
                }

                public object Data
                {
                    get;
                    set;
                }
            }

            public ushort id;
            public Keys keys;
            public byte subscribers;

            public Hotkey(IntPtr handle, ushort id, Keys keys)
            {
                this.id = id;
                this.keys = keys;
            }

            ~Hotkey()
            {
                Dispose();
            }

            public IHotkey Acquire()
            {
                lock (this)
                {
                    ++subscribers;
                    return new Subscriber(this);
                }
            }

            public void Release()
            {
                lock (this)
                {
                    if (--subscribers == 0)
                    {
                        Dispose();
                    }
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                if (keys != Keys.None)
                {
                    keys = Keys.None;
                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }

            public void OnKeyPress()
            {
                if (KeyPress != null)
                {
                    try
                    {
                        KeyPress(this, new HotkeyEventArgs());
                    }
                    catch { }
                }
            }

            public override string ToString()
            {
                return Hotkeys.ToString(keys);
            }
        }

        private Hotkey[] hotkeys;
        private IntPtr handle;
        private SynchronizationContext context;
        private int contextId;
        private int suppress;

        public Hotkeys(IntPtr handle)
        {
            context = SynchronizationContext.Current;
            contextId = Thread.CurrentThread.ManagedThreadId;

            this.handle = handle;
        }

        ~Hotkeys()
        {
            Dispose();
        }

        public bool IsSynchronized
        {
            get
            {
                return contextId == Thread.CurrentThread.ManagedThreadId;
            }
        }

        public void Invoke(Action a)
        {
            if (IsSynchronized)
            {
                try
                {
                    a();
                }
                catch { }
            }
            else
            {
                try
                {
                    context.Post(
                        delegate
                        {
                            try
                            {
                                a();
                            }
                            catch { }
                        }, null);

                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        private bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk)
        {
            if (contextId != Thread.CurrentThread.ManagedThreadId)
            {
                var b = false;

                try
                {
                    context.Send(
                        delegate
                        {
                            b = NativeMethods.RegisterHotKey(hWnd, id, fsModifiers, vk);
                        }, null);

                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return b;
            }
            else
            {
                return NativeMethods.RegisterHotKey(hWnd, id, fsModifiers, vk);
            }
        }

        private bool UnregisterHotKey(IntPtr hWnd, int id)
        {
            if (contextId != Thread.CurrentThread.ManagedThreadId)
            {
                var b = false;

                try
                {
                    context.Send(
                        delegate
                        {
                            b = NativeMethods.UnregisterHotKey(hWnd, id);
                        }, null);

                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return b;
            }
            else
            {
                return NativeMethods.UnregisterHotKey(hWnd, id);
            }
        }

        /// <summary>
        /// Returns true and the index of the hotkey if it was found, otherwise false and the index of a free space, or -1
        /// </summary>
        private bool GetHotkey(Keys keys, out int index)
        {
            index = -1;

            if (hotkeys != null)
            {
                for (var i = hotkeys.Length - 1; i >= 0; --i)
                {
                    if (hotkeys[i] != null)
                    {
                        if (hotkeys[i].keys == keys)
                        {
                            index = i;
                            return true;
                        }
                    }
                    else
                        index = i;
                }
            }

            return false;
        }

        public IHotkey Register(Keys keys)
        {
            keys = GetPrimaryKey(keys) | (keys & Keys.Modifiers);

            int i;

            if (!GetHotkey(keys, out i))
            {
                if (i == -1)
                {
                    if (hotkeys == null)
                    {
                        hotkeys = new Hotkey[1];
                        i = 0;
                    }
                    else
                    {
                        i = hotkeys.Length;
                        Array.Resize(ref hotkeys, i + 1);
                    }
                }

                var h = new Hotkey(handle, (ushort)i, keys);
                var modifier = KeyModifiers.None;

                if ((keys & Keys.Shift) != 0)
                {
                    modifier |= KeyModifiers.Shift;
                }
                if ((keys & Keys.Control) != 0)
                {
                    modifier |= KeyModifiers.Control;
                }
                if ((keys & Keys.Alt) != 0)
                {
                    modifier |= KeyModifiers.Alt;
                }

                keys = GetPrimaryKey(keys);

                if (keys == Keys.None)
                {
                    throw new HotkeyException(HotkeyException.Reasons.InvalidKeys);
                }

                if (!RegisterHotKey(handle, (ushort)i, modifier | KeyModifiers.NoRepeat, keys))
                {
                    throw new HotkeyException(HotkeyException.Reasons.RegisterError, "Unable to register hotkey", new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
                }

                hotkeys[i] = h;
                h.Disposed += OnHotkeyDisposed;
            }

            return hotkeys[i].Acquire();
        }

        void OnHotkeyDisposed(object sender, EventArgs e)
        {
            var h = (Hotkey)sender;

            h.Disposed -= OnHotkeyDisposed;
            hotkeys[h.id] = null;

            try
            {
                if (!UnregisterHotKey(handle, h.id))
                {
                    Util.Logging.Log(new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private bool Remove(Keys keys)
        {
            keys = GetPrimaryKey(keys) | (keys & Keys.Modifiers);

            int i;

            if (GetHotkey(keys, out i))
            {
                using (hotkeys[i])
                {
                    hotkeys[i] = null;
                }

                return true;
            }

            return false;
        }

        public void Clear()
        {
            if (hotkeys == null)
                return;

            for (var i = hotkeys.Length - 1; i >= 0; --i)
            {
                if (hotkeys[i] != null)
                {
                    hotkeys[i].Dispose();
                    hotkeys[i] = null;
                }
            }
        }

        public void Dispose()
        {
            Clear();
            hotkeys = null;
        }

        public void Process(ref Message m)
        {
            try
            {
                var h = hotkeys[(long)m.WParam];
                if (h != null)
                {
                    if (suppress != 0)
                    {
                        var elapsed = Environment.TickCount - suppress;
                        if (elapsed > 1000 || elapsed < 0)
                            suppress = 0;
                        else
                            return;
                    }

                    h.OnKeyPress();
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        /// <summary>
        /// Returns if the key combination is currently registered
        /// </summary>
        public bool IsRegistered(Keys keys)
        {
            int i;
            return GetHotkey(GetPrimaryKey(keys) | (keys & Keys.Modifiers), out i);
        }

        /// <summary>
        /// Sends a keypress that doesn't trigger hotkeys
        /// </summary>
        /// <param name="hotkey">The hotkey that was pressed to trigger this</param>
        /// <param name="keys">The keys to press</param>
        /// <returns>False if a hotkey was unregistered and failed to re-register</returns>
        public bool SendKeyPress(Keys hotkey, Keys keys)
        {
            Util.Logging.Log(hotkey.ToString() + ":" + keys.ToString());
            var hprimary = GetPrimaryKey(hotkey);
            var kprimary = GetPrimaryKey(keys);

            var modifiers = new Keys[]
            {
                Keys.ControlKey,
                Keys.ShiftKey,
                Keys.Menu,
            };

            Hotkey h;

            int i;
            if (GetHotkey(keys, out i))
                h = hotkeys[i];
            else
                h = null;

            var hisk = hprimary == kprimary;
            var hpressed = hprimary != Keys.None && Keyboard.GetKeyState(hprimary);
            var kpressed = hisk ? hpressed : Keyboard.GetKeyState(kprimary);
            var states = new bool[modifiers.Length];
            var requested = new bool[modifiers.Length];
            var unregistered = false;

            suppress = Environment.TickCount;

            //warning: pressing any key while a hotkey is being triggered will trigger the hotkey again

            if (hpressed)
                Keyboard.SetKeyState(hprimary, false);
            if (kpressed && !hisk)
                Keyboard.SetKeyState(kprimary, false);

            for (i = 0; i < modifiers.Length; i++)
            {
                states[i] = Keyboard.GetKeyState(modifiers[i]);
                requested[i] = (keys & modifiers[i]) != 0;

                if (states[i] != requested[i])
                {
                    Keyboard.SetKeyState(modifiers[i], requested[i]);
                }
            }

            if (h != null)
            {
                if (UnregisterHotKey(handle, h.id))
                {
                    unregistered = true;
                }
            }

            Keyboard.SetKeyState(kprimary, true);
            Keyboard.SetKeyState(kprimary, false);

            for (i = 0; i < modifiers.Length; i++)
            {
                if (states[i] != requested[i] && requested[i])
                {
                    Keyboard.SetKeyState(modifiers[i], false);
                }
            }

            if (unregistered)
            {
                var modifier = KeyModifiers.None;

                if ((keys & Keys.Shift) != 0)
                {
                    modifier |= KeyModifiers.Shift;
                }
                if ((keys & Keys.Control) != 0)
                {
                    modifier |= KeyModifiers.Control;
                }
                if ((keys & Keys.Alt) != 0)
                {
                    modifier |= KeyModifiers.Alt;
                }

                if (!RegisterHotKey(handle, h.id, modifier | KeyModifiers.NoRepeat, kprimary))
                {
                    //failed to reregister hotkeys
                    Remove(keys);

                    return false;
                }
                else
                {
                    if (kpressed && !hisk)
                    {
                        Keyboard.SetKeyState(kprimary, true);
                        SuppressUntil(kprimary);
                    }
                }
            }

            if (hpressed)
            {
                Keyboard.SetKeyState(hprimary, true);
                SuppressUntil(hprimary);
            }

            return true;
        }

        private async void SuppressUntil(Keys key)
        {
            while (suppress != 0)
            {
                if (!Windows.Keyboard.GetKeyState(key))
                {
                    suppress = 0;
                    return;
                }
                await Task.Delay(10);
            }
        }

        public static Keys GetPrimaryKey(Keys keys)
        {
            keys &= ~Keys.Modifiers;

            switch (keys)
            {
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.Menu:
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.LWin:
                case Keys.RWin:

                    return Keys.None;
            }

            return keys;
        }

        public static bool IsValid(Keys keys)
        {
            return GetPrimaryKey(keys) != Keys.None;
        }

        /// <summary>
        /// Checks if the hotkey can be bound
        /// </summary>
        public static bool IsAvailable(IntPtr handle, Keys keys)
        {
            var modifier = KeyModifiers.None;

            if ((keys & Keys.Shift) != 0)
            {
                modifier |= KeyModifiers.Shift;
            }
            if ((keys & Keys.Control) != 0)
            {
                modifier |= KeyModifiers.Control;
            }
            if ((keys & Keys.Alt) != 0)
            {
                modifier |= KeyModifiers.Alt;
            }

            keys = GetPrimaryKey(keys);

            try
            {
                if (NativeMethods.RegisterHotKey(handle, 0, modifier, keys))
                {
                    NativeMethods.UnregisterHotKey(handle, 0);

                    return true;
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            return false;
        }

        public static string ToString(Keys keys)
        {
            var sb = new StringBuilder(10);

            if ((keys & Keys.Control) != 0)
            {
                sb.Append("Ctrl+");
            }
            if ((keys & Keys.Alt) != 0)
            {
                sb.Append("Alt+");
            }
            if ((keys & Keys.Shift) != 0)
            {
                sb.Append("Shift+");
            }

            keys = GetPrimaryKey(keys);

            if (keys != Keys.None)
            {
                var c = NativeMethods.MapVirtualKeyEx((uint)keys, VkMapType.MAPVK_VK_TO_CHAR, IntPtr.Zero);

                if (c >= 33 && c <= 126)
                {
                    sb.Append((char)c);
                }
                else
                {
                    sb.Append(keys);
                }
            }
            else if (sb.Length > 0)
            {
                sb.Length--;
            }

            return sb.ToString();
        }
    }
}
