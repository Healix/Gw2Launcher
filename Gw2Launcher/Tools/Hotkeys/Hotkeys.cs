using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Gw2Launcher.Tools.Hotkeys
{
    public enum HotkeyAction : byte
    {
        LaunchFocus = 0,
        Focus = 1,
        Close = 2,

        LaunchAll = 3,
        CloseAll = 4,

        QuickLaunch = 5,
        Authenticator = 6,
        RunProgram = 7,

        ShowToFront = 8,
        WindowOptions = 9,
        RunAfter = 10,
        MinimizeRestoreAll = 11,
        WindowTemplates = 12,
        MinimizeRestore = 13,
        LaunchNext = 14,
        LaunchDaily = 15,
        AccountBar = 16,
        KeyPress = 17,
    }

    public enum HotkeyType : byte
    {
        /// <summary>
        /// For hotkeys used by the application
        /// </summary>
        Application = 0,
        /// <summary>
        /// For hotkeys used by accounts
        /// </summary>
        Account = 1,
    }

    public enum HotkeyFlags : byte
    {
        None = 0,
        OnlyWhenActive = 1,
        OnlyWhenInactive = 2,
    }

    public class RunProgramHotkey : Settings.Hotkey
    {
        public RunProgramHotkey(HotkeyAction action, Keys keys)
            : base(action, keys)
        {
        }

        public string Path
        {
            get;
            set;
        }

        public string Arguments
        {
            get;
            set;
        }

        public override void Read(BinaryReader r, ushort version)
        {
            Path = r.ReadString();
            Arguments = r.ReadString();
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(Path == null ? "" : Path);
            w.Write(Arguments == null ? "" : Arguments);
        }
    }

    public class KeyPressHotkey : Settings.Hotkey
    {
        public enum KeyPressMethod : byte
        {
            KeyEvent = 0,
            WindowMessage = 1,
        }

        public KeyPressHotkey(HotkeyAction action, Keys keys)
            : base(action, keys)
        {
        }

        public System.Windows.Forms.Keys KeyPress
        {
            get;
            set;
        }

        public KeyPressMethod Method
        {
            get;
            set;
        }

        public override void Read(BinaryReader r, ushort version)
        {
            base.Read(r, version);

            KeyPress = (System.Windows.Forms.Keys)r.ReadInt32();
            Method = (KeyPressMethod)r.ReadByte();
        }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);

            w.Write((int)KeyPress);
            w.Write((byte)Method);
        }
    }
}
