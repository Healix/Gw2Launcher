using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Tools.Hotkeys;

namespace Gw2Launcher.UI
{
    public partial class formHotkey : Base.BaseForm
    {
        private bool accountMode;
        private List<Control> controls;

        public formHotkey(bool accountMode = false)
        {
            this.accountMode = accountMode;
            this.controls = new List<Control>();

            InitializeComponents();

            if (accountMode)
            {
                this.MinimumSize = new Size(this.MinimumSize.Width, this.MinimumSize.Height * 3 / 4);
                this.ClientSize = new Size(this.ClientSize.Width, this.ClientSize.Height * 3 / 4);
            }
            else
            {
                comboKeyPressMethod.Items.AddRange(new object[]
                {
                    new Util.ComboItem<KeyPressHotkey.KeyPressMethod>(KeyPressHotkey.KeyPressMethod.KeyEvent, "Keyboard input"),
                    new Util.ComboItem<KeyPressHotkey.KeyPressMethod>(KeyPressHotkey.KeyPressMethod.WindowMessage, "Window message"),
                });
                comboKeyPressMethod.SelectedIndex = 0;
            }
        }

        public formHotkey(Settings.Hotkey hotkey, Settings.IAccount account = null, bool accountMode = false)
            : this(accountMode)
        {
            if (hotkey == null)
            {
                return;
            }

            var info = HotkeyInfo.GetInfo(hotkey.Action);

            DataGridViewRow r;

            r = FindRow<HotkeyType>(gridCategory, columnCategory.Index, info.Type);

            if (r != null)
            {
                SelectRow(r, columnCategory.Index);
            }
            else
            {
                gridCategory.ClearSelection();
            }

            r = FindRow(info.Action);

            if (r != null)
            {
                SelectRow(r, columnAction.Index);
                gridAction.VisibleChanged += OnVisibleScrollToRow;

                if (!accountMode && info.Type == HotkeyType.Account && account != null)
                {
                    r = FindRow<Settings.IAccount>(gridAccounts, columnAccount.Index, account);

                    if (r != null)
                    {
                        SelectRow(r, columnAction.Index);
                        gridAccounts.VisibleChanged += OnVisibleScrollToRow;
                    }
                    else
                    {
                        gridAccounts.ClearSelection();
                    }
                }
            }
            else
            {
                gridCategory.ClearSelection();
            }

            keysHotkey.Keys = hotkey.Keys;

            switch (hotkey.Action)
            {
                case HotkeyAction.KeyPress:
                    {
                        var h = (KeyPressHotkey)hotkey;

                        keysKeyPress.Keys = h.KeyPress;
                        Util.ComboItem<KeyPressHotkey.KeyPressMethod>.Select(comboKeyPressMethod, h.Method);
                    }
                    break;
                case HotkeyAction.RunProgram:
                    {
                        var h = (RunProgramHotkey)hotkey;

                        textPath.Text = h.Path;
                        textArguments.Text = h.Arguments;
                    }
                    break;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (accountMode)
                gridAction.Focus();
            else
                gridCategory.Focus();
        }

        void OnVisibleScrollToRow(object sender, EventArgs e)
        {
            var grid = (Controls.ScaledDataGridView)sender;
            var row = GetSelected(grid);

            grid.VisibleChanged -= OnVisibleScrollToRow;

            if (row != null)
                grid.ScrollToRow(row.Index);
        }

        private void SelectRow(DataGridViewRow row, int column)
        {
            if (row != null)
                row.Cells[column].Selected = true;
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

            gridCategory.Rows.AddRange(new DataGridViewRow[]
                {
                    CreateRow(HotkeyType.Application, "General"),
                    CreateRow(HotkeyType.Account, "Account"),
                });

            var hotkeys = HotkeyInfo.Hotkeys;
            var rows = new DataGridViewRow[hotkeys.Length];
            var count = 0;

            for (var i = 0; i < hotkeys.Length; i++)
            {
                if (hotkeys[i] != null)
                    rows[count++] = CreateRow(hotkeys[i]);
            }

            if (count != rows.Length)
                Array.Resize(ref rows, count);

            gridAction.Rows.AddRange(rows);
            gridAction.Sort(columnAction, ListSortDirection.Ascending);

            if (accountMode)
            {
                panelCategory.Visible = false;
                SelectRow(FindRow<HotkeyType>(gridCategory, columnCategory.Index, HotkeyType.Account), columnCategory.Index);
            }

            gridCategory.SelectionChanged += gridCategory_SelectionChanged;
            gridAction.SelectionChanged += gridAction_SelectionChanged;

            gridCategory_SelectionChanged(null, null);
        }

        private DataGridViewRow CreateRow(HotkeyType t, string text)
        {
            DataGridViewRow row;

            row = (DataGridViewRow)gridCategory.RowTemplate.Clone();
            row.CreateCells(gridCategory);

            var cell = row.Cells[columnCategory.Index];
            cell.Value = new Util.ComboItem<HotkeyType>(t, text);

            return row;
        }

        private DataGridViewRow CreateRow(HotkeyInfo info)
        {
            DataGridViewRow row;

            row = (DataGridViewRow)gridAction.RowTemplate.Clone();
            row.CreateCells(gridAction);

            var cell = row.Cells[columnAction.Index];
            cell.Value = new Util.ComboItem<HotkeyInfo>(info, info.Name);
            cell.ToolTipText = info.Description;

            return row;
        }

        private DataGridViewRow CreateRow(Settings.IAccount a)
        {
            DataGridViewRow row;

            row = (DataGridViewRow)gridAccounts.RowTemplate.Clone();
            row.CreateCells(gridAccounts);

            var cell = row.Cells[columnAccount.Index];
            cell.Value = new Util.ComboItem<Settings.IAccount>(a, a.Name);

            return row;
        }

        private DataGridViewRow FindRow(HotkeyAction value)
        {
            var column = columnAccount.Index;

            foreach (DataGridViewRow row in gridAction.Rows)
            {
                if (((Util.ComboItem<HotkeyInfo>)row.Cells[column].Value).Value.Action == value)
                    return row;
            }

            return null;
        }

        private DataGridViewRow FindRow<T>(DataGridView grid, int column, T value)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (object.Equals(((Util.ComboItem<T>)row.Cells[column].Value).Value, value))
                    return row;
            }

            return null;
        }

        private DataGridViewRow GetSelected(DataGridView grid)
        {
            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                return row;
            }

            return null;
        }

        void gridAction_SelectionChanged(object sender, EventArgs e)
        {
            panelContent.SuspendLayout();

            HashSet<Control> visible;

            if (controls.Count > 0)
            {
                visible = new HashSet<Control>(controls);
                controls.Clear();
            }
            else
                visible = null;

            foreach (DataGridViewRow row in gridAction.SelectedRows)
            {
                var info = ((Util.ComboItem<HotkeyInfo>)row.Cells[columnAction.Index].Value).Value;

                if (!accountMode && info.Type == HotkeyType.Account)
                {
                    if (!gridAccounts.Enabled)
                    {
                        gridAccounts.SuspendLayout();
                        foreach (var account in Util.Accounts.GetAccounts())
                        {
                            gridAccounts.Rows.Add(CreateRow(account));
                        }
                        gridAccounts.Enabled = true;
                        gridAccounts.ResumeLayout();
                    }

                    controls.Add(panelAccounts);
                }

                switch (info.Action)
                {
                    case HotkeyAction.RunProgram:

                        controls.Add(panelProgram);

                        break;
                    case HotkeyAction.KeyPress:

                        controls.Add(panelKeyPress);

                        break;
                }
            }

            foreach (var c in controls)
            {
                c.Visible = true;
                if (visible != null)
                    visible.Remove(c);
            }

            if (visible != null)
            {
                foreach (var c in visible)
                {
                    c.Visible = false;
                }
            }

            panelContent.ResumeLayout();
        }

        void gridCategory_SelectionChanged(object sender, EventArgs e)
        {
            var selected = GetSelected(gridCategory);

            if (selected == null)
            {
                foreach (DataGridViewRow r in gridAction.Rows)
                {
                    r.Visible = false;
                }
                gridAction.ClearSelection();
            }
            else
            {
                var type = ((Util.ComboItem<HotkeyType>)selected.Cells[columnCategory.Index].Value).Value;
                var first = true;

                foreach (DataGridViewRow r in gridAction.Rows)
                {
                    var info = ((Util.ComboItem<HotkeyInfo>)r.Cells[columnAction.Index].Value).Value;
                    var b = info.Type == type;

                    r.Visible = b;

                    if (b && first)
                    {
                        SelectRow(r, columnAction.Index);
                        first = false;
                    }
                }
            }
        }

        public Settings.Hotkey Result
        {
            get;
            private set;
        }

        public Settings.IAccount SelectedAccount
        {
            get;
            private set;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            var keys = keysHotkey.Keys;

            if (!Windows.Hotkeys.IsValid(keys))
            {
                MessageBox.Show(this, "Invalid hotkey selected", "Invalid hotkey", MessageBoxButtons.OK, MessageBoxIcon.Error);
                keysHotkey.Focus();
                return;
            }

            var r = GetSelected(gridAction);
            if (r == null)
                return;
            var info = ((Util.ComboItem<HotkeyInfo>)r.Cells[columnAction.Index].Value).Value;

            if (!accountMode && info.Type == HotkeyType.Account)
            {
                r = GetSelected(gridAccounts);
                if (r == null)
                {
                    MessageBox.Show(this, "No account has been selected", "Account required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    gridAccounts.Focus();
                    return;
                }
                SelectedAccount = (Settings.IAccount)((Util.ComboItem<Settings.IAccount>)r.Cells[columnAccount.Index].Value).Value;
            }

            Settings.Hotkey hotkey;

            switch (info.Action)
            {
                case HotkeyAction.RunProgram:

                    if (textPath.TextLength == 0 || !System.IO.File.Exists(textPath.Text))
                    {
                        MessageBox.Show(this, "The selected program is not valid", "Invalid program", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textPath.Focus();
                        return;
                    }

                    hotkey = new RunProgramHotkey(info.Action, keys)
                    {
                        Path = textPath.Text,
                        Arguments = textArguments.Text,
                    };

                    break;
                case HotkeyAction.KeyPress:

                    if (!Windows.Hotkeys.IsValid(keysKeyPress.Keys))
                    {
                        MessageBox.Show(this, "The key to press is not valid", "Invalid key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        keysKeyPress.Focus();
                        return;
                    }

                    hotkey = new KeyPressHotkey(info.Action, keys)
                    {
                        KeyPress = keysKeyPress.Keys,
                        Method = Util.ComboItem<KeyPressHotkey.KeyPressMethod>.SelectedValue(comboKeyPressMethod, KeyPressHotkey.KeyPressMethod.KeyEvent),
                    };

                    break;
                default:

                    hotkey = HotkeyManager.From(info.Action, keys);

                    break;
            }

            this.Result = hotkey;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonPath_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.ValidateNames = false;
                f.Filter = "Executables|*.exe|All files|*.*";

                if (textPath.TextLength != 0)
                {
                    try
                    {
                        f.InitialDirectory = System.IO.Path.GetDirectoryName(textPath.Text);
                    }
                    catch { }
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    textPath.Text = f.FileName;
                    textPath.Select(textPath.TextLength, 0);
                }
            }
        }
    }
}
