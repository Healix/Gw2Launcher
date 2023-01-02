using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls
{
    class AffinityAccountsPanel : StackPanel
    {
        private class ItemData
        {
            public AffinityDisplay Display;
            public Settings.IAccount Account;
            public bool Modified;
            public bool Hidden;
        }

        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem menuSave, menuLoad, menuOnlyShowEnabled, menuSelectAccounts;
        private Label labelNoAccounts;

        public AffinityAccountsPanel(IEnumerable<Settings.IAccount> accounts)
        {
            this.SuspendLayout();

            contextMenu = new ContextMenuStrip();
            contextMenu.Items.AddRange(new ToolStripItem[]
                {
                    menuSave = new ToolStripMenuItem("Save", null, menuSave_Click),
                    menuLoad = new ToolStripMenuItem("Load", null, menuLoad_Click),
                    new ToolStripSeparator(),
                    menuOnlyShowEnabled = new ToolStripMenuItem("Only show enabled", null, menuOnlyShowEnabled_Click),
                    //menuSelectAccounts = new ToolStripMenuItem("Select accounts", null, menuSelectAccounts_Click),
                });

            labelNoAccounts = new Label()
            {
                Margin = new Padding(1, 8, 0, 0),
                Text = "No accounts found",
                AutoSize = true,
                Visible = false,
                ForeColor = System.Drawing.SystemColors.GrayText,
            };
            labelNoAccounts.MouseDown += OnMouseDown;
            this.Controls.Add(labelNoAccounts);

            var count = 0;

            foreach (var a in accounts)
            {

                var label = new Label()
                {
                    Margin = new Padding(1, 0, 0, 8),
                    Text = a.Name,
                    AutoSize = true,
                };

                var content = new StackPanel()
                {
                    Margin = Padding.Empty,
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                };

                var check = new CheckBox()
                {
                    Margin = new Padding(8, 0, 6, 0),
                    Anchor = AnchorStyles.Left,
                    AutoSize = true,
                    Checked = a.ProcessAffinity > 0,
                };

                var display = new AffinityDisplay()
                {
                    Margin = Padding.Empty,
                    Affinity = a.ProcessAffinity,
                    Enabled = check.Checked,
                    AutoSize = true,
                    Tag = a,
                };

                var container = new StackPanel()
                {
                    Margin = new Padding(0, 8, 0, 0),
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                };

                var id = new ItemData()
                {
                    Account = a,
                    Display = display,
                };

                container.Tag = id;
                display.Tag = id;

                check.CheckedChanged += checkEnable_CheckedChanged;
                display.MouseDown += OnMouseDown;
                display.AffinityChanged += display_AffinityChanged;
                label.MouseDown += OnMouseDown;

                content.Controls.AddRange(new Control[] { check, display });
                container.Controls.AddRange(new Control[] { label, content });
                this.Controls.Add(container);

                ++count;
            }

            labelNoAccounts.Visible = count == 0;

            this.ResumeLayout();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            OnMouseDown(this, e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                contextMenu.Dispose();
            }
        }

        void display_AffinityChanged(object sender, EventArgs e)
        {
            var d = (AffinityDisplay)sender;
            ((ItemData)d.Parent.Parent.Tag).Modified = true;
            d.AffinityChanged -= display_AffinityChanged;
        }

        void checkEnable_CheckedChanged(object sender, EventArgs e)
        {
            var check = (CheckBox)sender;
            var d = ((ItemData)check.Parent.Parent.Tag);
            d.Modified = true;
            d.Display.Enabled = check.Checked;
            if (!check.Checked && menuOnlyShowEnabled.Checked)
            {
                //check.Parent.Parent.Visible = false;
                DoFilter();
            }
        }

        private IEnumerable<KeyValuePair<Settings.IAccount, bool>> EnumerateAccounts()
        {
            foreach (Control container in this.Controls)
            {
                if (container.Tag != null)
                    yield return new KeyValuePair<Settings.IAccount, bool>(((ItemData)container.Tag).Account, container.Visible);
            }
        }

        private void DoFilter()
        {
            this.SuspendLayout();

            var count = 0;

            foreach (Control container in this.Controls)
            {
                if (container.Tag != null)
                {
                    var b = IsFilterVisible((ItemData)container.Tag);
                    container.Visible = b;
                    if (b)
                        ++count;
                }
            }

            labelNoAccounts.Visible = count == 0;

            this.ResumeLayout();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ShowMenu(sender);
            }
        }

        private void ShowMenu(object sender)
        {
            var b = sender is AffinityDisplay;
            menuSave.Enabled = b;
            menuLoad.Enabled = b;
            contextMenu.Tag = sender;
            contextMenu.Show(Cursor.Position);
        }

        private bool IsFilterVisible(ItemData d)
        {
            return !d.Hidden && (!menuOnlyShowEnabled.Checked || d.Display.Enabled);
        }

        private void menuOnlyShowEnabled_Click(object sender, EventArgs e)
        {
            menuOnlyShowEnabled.Checked = !menuOnlyShowEnabled.Checked;
            DoFilter();
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            var d = contextMenu.Tag as AffinityDisplay;
            if (d == null)
                return;

            using (var f = new Affinity.formAffinitySelectDialog(d.Affinity))
            {
                f.ShowDialog(this);
            }
        }

        private void menuLoad_Click(object sender, EventArgs e)
        {
            var d = contextMenu.Tag as AffinityDisplay;
            if (d == null)
                return;

            using (var f = new Affinity.formAffinitySelectDialog(Affinity.formAffinitySelectDialog.DialogMode.Select))
            {
                if (f.ShowDialog(this) == DialogResult.OK && f.SelectedAffinity != null)
                {
                    d.Affinity = f.SelectedAffinity.Affinity;
                }
            }
        }

        private void menuSelectAccounts_Click(object sender, EventArgs e)
        {
            using (var f = new formAccountSelect("Select accounts", EnumerateAccounts(), true))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    var filter = new HashSet<Settings.IAccount>(f.Selected);
                    foreach (Control container in this.Controls)
                    {
                        var d = (ItemData)container.Tag;
                        if (d != null)
                            d.Hidden = !filter.Contains(d.Account);
                    }
                    DoFilter();
                }
            }
        }

        public void Save()
        {
            foreach (Control container in this.Controls)
            {
                var d = (ItemData)container.Tag;
                if (d == null || !d.Modified)
                    continue;
                d.Account.ProcessAffinity = d.Display.Enabled ? d.Display.Affinity : 0;
            }
        }
    }
}
