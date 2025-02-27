using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Gw2Launcher.UI.Controls
{
    class HotkeyContainerPanel : StackPanel
    {
        private Dictionary<Settings.Hotkey, Control> controls;
        private HotkeyGroupsPanel parent;

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

        public HotkeyContainerPanel(HotkeyGroupsPanel parent)
        {
            this.controls = new Dictionary<Settings.Hotkey, Control>();
            this.parent = parent;

            this.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;

            this.Controls.Add(new Label()
            {
                Font = new System.Drawing.Font("Segoe UI Semibold", parent.TemplateText.Font.SizeInPoints, System.Drawing.FontStyle.Bold),// parent.TemplateText.Font,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Margin = new Padding(parent.TemplateText.Margin.Left, parent.TemplateText.Margin.Top * 3, 0, parent.TemplateText.Margin.Top),
                Text = "",
                Visible = false,
                AutoSize = true,
                Padding = new Padding(0, 5, 0, 5),
            });
        }

        public ushort Group
        {
            get;
            set;
        }

        public bool Modified
        {
            get;
            set;
        }

        public Settings.Hotkey[] GetHotkeys()
        {
            if (controls.Count == 0)
                return null;
            return controls.Keys.ToArray();
        }

        public void Add(Settings.Hotkey hotkey, string text, Settings.IAccount account)
        {
            this.Modified = true;

            var p = new StackPanel()
            {
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                AutoSizeFill = AutoSizeFillMode.NoWrap,
                AutoSize = true,
                Margin = parent.TemplateText.Margin,
            };

            p.Tag = new HotkeyValue(hotkey, account);

            Controls.LinkLabel l;

            p.Controls.AddRange(new Control[]
                    {
                        l = new Controls.LinkLabel()
                        {
                            Font = parent.TemplateText.Font,
                            Anchor = AnchorStyles.Left,
                            AutoSize = true,
                            Margin = new Padding(),
                            Text = text,
                        },
                        new Label()
                        {
                            Font = parent.TemplateKey.Font,
                            ForeColor = SystemColors.GrayText,
                            Anchor = AnchorStyles.Left,
                            AutoSize = true,
                            Margin = new Padding(5,0,0,0),
                            Text = Windows.Hotkeys.ToString(hotkey.Keys),
                        },
                    });

            l.Click += l_Click;

            this.controls.Add(hotkey, p);
            this.Controls.Add(p);
        }

        void l_Click(object sender, EventArgs e)
        {
            parent.OnHotkeyClick((HotkeyValue)((Label)sender).Parent.Tag);
        }

        public void Update(Settings.Hotkey hotkey, Settings.Hotkey original, string text, Settings.IAccount account)
        {
            this.Modified = true;

            Control c;

            if (this.controls.TryGetValue(original, out c))
            {
                c.SuspendLayout();

                c.Tag = new HotkeyValue(hotkey, account);
                c.Controls[0].Text = text;
                c.Controls[1].Text = Windows.Hotkeys.ToString(hotkey.Keys);

                c.ResumeLayout();

                if (!object.ReferenceEquals(hotkey, original))
                {
                    this.controls.Remove(original);
                    this.controls.Add(hotkey, c);
                }
            }
            else
            {
                Add(hotkey, text, account);
            }
        }

        public void Remove(Settings.Hotkey hotkey)
        {
            Control c;

            if (this.controls.TryGetValue(hotkey, out c))
            {
                this.Modified = true;

                this.controls.Remove(hotkey);
                this.Controls.Remove(c);
                c.Dispose();
            }
        }

        public int Count
        {
            get
            {
                return this.controls.Count;
            }
        }

        public string Header
        {
            get
            {
                return this.Controls[0].Text;
            }
            set
            {
                this.Controls[0].Text = value;
                this.Controls[0].Visible = !string.IsNullOrEmpty(value);
            }
        }
    }
}
