using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Tools.Hotkeys
{
    class HotkeyInfo
    {
        private static HotkeyInfo[] hotkeys;

        public HotkeyInfo(HotkeyAction action, HotkeyType type, string name, string description, HotkeyFlags flags)
        {
            this.Action = action;
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.Flags = flags;
        }

        public HotkeyAction Action
        {
            get;
            private set;
        }

        public HotkeyType Type
        {
            get;
            private set;
        }

        public HotkeyFlags Flags
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        private static void Initialize()
        {
            hotkeys = new HotkeyInfo[Enum.GetValues(typeof(HotkeyAction)).Length];

            Register(HotkeyAction.LaunchFocus, HotkeyType.Account, "Launch or focus", "Launches the account or focuses the game if it was already launched", HotkeyFlags.None);
            Register(HotkeyAction.Focus, HotkeyType.Account, "Focus", "Focuses the game", HotkeyFlags.OnlyWhenActive);
            Register(HotkeyAction.Close, HotkeyType.Account, "Close", "Closes the game", HotkeyFlags.OnlyWhenActive);
            Register(HotkeyAction.LaunchAll, HotkeyType.Application, "Launch all", "Launches all visible accounts", HotkeyFlags.None);
            Register(HotkeyAction.CloseAll, HotkeyType.Application, "Close all", "Closes all active accounts", HotkeyFlags.None);
            Register(HotkeyAction.QuickLaunch, HotkeyType.Application, "Show quick launch", "Shows a popup menu to launch accounts", HotkeyFlags.None);
            Register(HotkeyAction.Authenticator, HotkeyType.Application, "Paste authenticator", "Pastes the authenticator code to the current window", HotkeyFlags.None);
            Register(HotkeyAction.RunProgram, HotkeyType.Application, "Run program", "Starts a program", HotkeyFlags.None);
            Register(HotkeyAction.ShowToFront, HotkeyType.Application, "Show Gw2Launcher", "Focuses and brings Gw2Launcher to the front", HotkeyFlags.None);
            Register(HotkeyAction.WindowOptions, HotkeyType.Application, "Show window options", "Shows window and process options for the current account", HotkeyFlags.OnlyWhenActive);
            Register(HotkeyAction.RunAfter, HotkeyType.Application, "Show run after", "Shows run after options for the current account", HotkeyFlags.OnlyWhenActive);
            Register(HotkeyAction.MinimizeRestoreAll, HotkeyType.Application, "Minimize or restore all", "Minimizes or restores all visible accounts", HotkeyFlags.OnlyWhenActive);
            Register(HotkeyAction.WindowTemplates, HotkeyType.Application, "Show window templates", "Shows the compact view for window templates", HotkeyFlags.None);
            Register(HotkeyAction.MinimizeRestore, HotkeyType.Account, "Minimize or restore", "Minimizes or restores the game", HotkeyFlags.OnlyWhenActive);
            Register(HotkeyAction.LaunchNext, HotkeyType.Application, "Launch next", "Launches the next account in order of last used", HotkeyFlags.None);
            Register(HotkeyAction.LaunchDaily, HotkeyType.Application, "Launch daily", "Launches the first account showing the daily login indicator", HotkeyFlags.None);
            Register(HotkeyAction.AccountBar, HotkeyType.Application, "Show account bar", "Shows or hides the account bar", HotkeyFlags.None);
            Register(HotkeyAction.KeyPress, HotkeyType.Application, "Send key press", "Sends a keypress to the current window", HotkeyFlags.None);
        }

        private static void Register(HotkeyAction action, HotkeyType type, string name, string description = null, HotkeyFlags flags = HotkeyFlags.None)
        {
            hotkeys[(ushort)action] = new HotkeyInfo(action, type, name, description, flags);
        }

        public static void Register(HotkeyInfo hotkey)
        {
            if (hotkeys == null)
                Initialize();
            var i = (ushort)hotkey.Action;
            if (i > hotkeys.Length)
            {
                Array.Resize(ref hotkeys, i + 1);
            }
            hotkeys[i] = hotkey;
        }

        public static HotkeyInfo[] Hotkeys
        {
            get
            {
                if (hotkeys == null)
                    Initialize();
                return hotkeys;
            }
        }

        public static HotkeyInfo GetInfo(HotkeyAction action)
        {
            return Hotkeys[(ushort)action];
        }
    }
}
