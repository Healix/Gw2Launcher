using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Gw2Launcher.Tools.Hotkeys;

namespace Gw2Launcher.UI.Controls
{
    class HotkeyGroupsPanel : StackPanel
    {
        public event EventHandler HotkeyClick;

        public class ModifiedHotkeys
        {
            public ModifiedHotkeys(Settings.Hotkey[] hotkeys, Settings.IAccount account = null)
            {
                this.Hotkeys = hotkeys;
                this.Account = account;
            }

            public Settings.Hotkey[] Hotkeys
            {
                get;
                private set;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }
        }

        public class HotkeyValue
        {
            public HotkeyValue(Settings.Hotkey hotkey, Settings.IAccount account)
            {
                this.Hotkey = hotkey;
                this.Account = account;
            }

            public Settings.Hotkey Hotkey
            {
                get;
                private set;
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }
        }

        private Dictionary<Settings.Hotkey, HotkeyContainerPanel> hotkeys;
        private Dictionary<ushort, HotkeyContainerPanel> groups;

        public HotkeyGroupsPanel()
        {
            this.hotkeys = new Dictionary<Settings.Hotkey, HotkeyContainerPanel>();
            this.groups = new Dictionary<ushort, HotkeyContainerPanel>();
            this.EnableGrouping = true;
        }

        public Label TemplateHeader
        {
            get;
            set;
        }

        public Controls.LinkLabel TemplateText
        {
            get;
            set;
        }

        public Label TemplateKey
        {
            get;
            set;
        }

        public bool EnableGrouping
        {
            get;
            set;
        }

        public bool Modified
        {
            get;
            set;
        }

        public int Count
        {
            get
            {
                return hotkeys.Count;
            }
        }

        public string GetText(Settings.Hotkey h)
        {
            var info = HotkeyInfo.GetInfo(h.Action);

            try
            {
                switch (h.Action)
                {
                    case HotkeyAction.RunProgram:

                        return "Run " + Path.GetFileName(((RunProgramHotkey)h).Path);

                    case HotkeyAction.KeyPress:

                        return "Press " + Windows.Hotkeys.ToString(((KeyPressHotkey)h).KeyPress);

                }
            }
            catch { }

            return info.Name;
        }

        public void Add(Settings.Hotkey hotkey, string text, Settings.IAccount account)
        {
            HotkeyContainerPanel p;

            Modified = true;

            if (hotkeys.TryGetValue(hotkey, out p))
            {
                Update(hotkey, hotkey, text, account);
            }
            else
            {
                this.SuspendLayout();

                p = GetPanel(GetGroup(account));
                p.Add(hotkey, text, account);
                p.Visible = true;

                hotkeys.Add(hotkey, p);

                this.ResumeLayout();
                this.PerformLayout();
            }
        }

        public void Update(Settings.Hotkey hotkey, Settings.Hotkey original, string text, Settings.IAccount account)
        {
            HotkeyContainerPanel p;

            Modified = true;

            if (hotkeys.TryGetValue(original, out p))
            {
                var group = GetGroup(account);

                this.SuspendLayout();

                if (p.Group != group)
                {
                    p.Remove(original);
                    if (p.Count == 0)
                        p.Visible = false;

                    p = GetPanel(group);
                    p.Add(hotkey, text, account);
                    p.Visible = true;
                }
                else
                {
                    p.Update(hotkey, original, text, account);
                }

                this.ResumeLayout();

                if (!object.ReferenceEquals(hotkey, original))
                {
                    hotkeys.Remove(original);
                    hotkeys.Add(hotkey, p);
                }
            }
            else
            {
                Add(hotkey, text, account);
            }
        }

        public void Remove(Settings.Hotkey hotkey)
        {
            HotkeyContainerPanel p;

            Modified = true;

            if (hotkeys.TryGetValue(hotkey, out p))
            {
                p.Remove(hotkey);
                hotkeys.Remove(hotkey);

                if (p.Count == 0)
                    p.Visible = false;
            }
        }

        private ushort GetGroup(Settings.IAccount account)
        {
            if (EnableGrouping && account != null)
            {
                return account.UID;
            }

            return 0;
        }

        private HotkeyContainerPanel GetPanel(ushort uid)
        {
            HotkeyContainerPanel p;

            if (!groups.TryGetValue(uid, out p))
            {
                if (uid != 0 && groups.Count == 0)
                {
                    GetPanel(0);
                }

                string header = null;

                if (uid > 0)
                {
                    var a = Settings.Accounts[uid];
                    if (a.HasValue)
                    {
                        header = a.Value.Name;
                    }
                }

                p = new HotkeyContainerPanel(this)
                {
                    Margin = new Padding(),
                    FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                    Group = uid,
                    AutoSize = true,
                };

                if (header != null)
                    p.Header = header;

                this.groups.Add(uid, p);
                this.Controls.Add(p);
            }

            return p;
        }

        public void OnHotkeyClick(HotkeyContainerPanel.HotkeyValue hotkey)
        {
            if (HotkeyClick != null)
                HotkeyClick(hotkey, EventArgs.Empty);
        }

        public void ResetModified()
        {
            this.Modified = false;

            foreach (HotkeyContainerPanel p in groups.Values)
            {
                p.Modified = false;
            }
        }

        public IEnumerable<ModifiedHotkeys> GetModified()
        {
            foreach (HotkeyContainerPanel p in groups.Values)
            {
                if (!p.Modified)
                    continue;

                Settings.IAccount a = null;

                if (p.Group > 0)
                {
                    var v = Settings.Accounts[p.Group];
                    if (v.HasValue)
                        a = v.Value;
                }

                yield return new ModifiedHotkeys(p.GetHotkeys(), a);
            }
        }
    }
}
