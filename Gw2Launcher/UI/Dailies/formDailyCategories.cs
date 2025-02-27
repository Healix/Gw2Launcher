using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Api;

namespace Gw2Launcher.UI.Dailies
{
    public partial class formDailyCategories : Base.BaseForm
    {
        private enum ChangeType
        {
            None,
            Grid,
            Text,
        }

        private Dictionary<ushort, DataGridViewRow> categories;
        private Daily.Category[] existing;
        private HashSet<ushort> selected;
        private ushort[] ids;
        private ChangeType pending, last;
        private int filtering;

        private class Category
        {
            public ushort ID
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public bool Temporary
            {
                get;
                set;
            }

            public bool Hidden
            {
                get;
                set;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public formDailyCategories(ushort[] ids = null, object categories = null)
        {
            this.ids = ids;
            this.categories = new Dictionary<ushort, DataGridViewRow>();

            if (categories is Daily.Category[])
            {
                this.existing = (Daily.Category[])categories;
            }

            selected = new HashSet<ushort>();

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            var defaultIds = Daily.GetDefaultCategories();

            if (this.ids != null)
            {
                gridSelected.SuspendLayout();

                for (var i = 0; i < ids.Length; i++)
                {
                    var row = (DataGridViewRow)gridCategories.RowTemplate.Clone();
                    var id = ids[i];

                    row.CreateCells(gridCategories);
                    row.Cells[0].Value = new Category()
                    {
                        ID = id,
                        Name = GetDefaultName(id),
                    };

                    categories[id] = row;
                    selected.Add(id);
                    gridSelected.Rows.Add(row);
                }

                gridSelected.ResumeLayout();
            }

            gridCategories.SuspendLayout();

            for (var i = 0; i < defaultIds.Length; i++)
            {
                var id = defaultIds[i];

                if (!categories.ContainsKey(id))
                {
                    var row = (DataGridViewRow)gridCategories.RowTemplate.Clone();

                    row.CreateCells(gridCategories);
                    row.Cells[0].Value = new Category()
                    {
                        ID = id,
                        Name = GetDefaultName(id),
                    };

                    categories[id] = row;
                    gridCategories.Rows.Add(row);
                }
            }

            gridCategories.ResumeLayout();

            UpdateTextFromGrid();

            textAdvanced.TextChanged += textAdvanced_TextChanged;
        }

        private string GetDefaultName(ushort id)
        {
            if (existing != null)
            {
                for (var i = 0; i < existing.Length; i++)
                {
                    if (existing[i].ID == id)
                    {
                        return existing[i].Name;
                    }
                }
            }

            switch (id)
            {
                case 88:
                    return "Daily Fractals";
                case 238:
                    return "Daily Living World Season 3";
                case 243:
                    return "Daily Living World Season 4";
                case 250:
                    return "Daily Strike Mission";
                case 321:
                    return "Daily End of Dragons";
                case 330:
                    return "Daily Icebrood Saga";
            }

            return id.ToString();
        }

        private async void labelRefresh_Click(object sender, EventArgs e)
        {
            labelRefresh.Enabled = false;

            try
            {
                var categories = await Daily.GetCategoriesAsync(Settings.ShowDailiesLanguage.Value);

                if (IsDisposed)
                {
                    return;
                }

                Array.Sort(categories);

                try
                {
                    gridCategories.SuspendLayout();
                    gridSelected.SuspendLayout();

                    labelRefresh.Visible = false;

                    var count = 0;
                    var rows = new DataGridViewRow[categories.Length];

                    foreach (var c in categories)
                    {
                        bool added;
                        DataGridViewRow row;

                        if (added = !this.categories.TryGetValue(c.ID, out row))
                        {
                            row = (DataGridViewRow)gridCategories.RowTemplate.Clone();

                            row.CreateCells(gridCategories);

                            rows[count++] = row;
                            this.categories[c.ID] = row;
                        }

                        var hidden = true;

                        if (c.Tomorrow != null && c.Tomorrow.Length > 0 && !Daily.Category.Equals(c.Today, c.Tomorrow))
                        {
                            switch (c.ID)
                            {
                                case 267:
                                case 282:

                                    //ignored

                                    break;
                                default:

                                    hidden = false;

                                    break;
                            }
                        }

                        row.Cells[0].Value = new Category()
                        {
                            ID = c.ID,
                            Name = c.Name,
                            Hidden = hidden,
                        };

                    }

                    if (count > 0)
                    {
                        Array.Resize<DataGridViewRow>(ref rows, count);
                        Filter(rows);
                        gridCategories.Rows.AddRange(rows);
                    }

                    checkShowAll.Visible = true;
                }
                finally
                {
                    gridCategories.ResumeLayout();
                    gridSelected.ResumeLayout();
                }
            }
            catch (Exception x)
            {
                Util.Logging.Log(x);

                if (!IsDisposed && Visible)
                {
                    MessageBox.Show(this, "Unable to retrieve categories", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            labelRefresh.Enabled = true;
        }

        private void buttonOrder_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                gridSelected.MoveSelectedRows(sender == buttonOrderUp, true);
                OnGridChanged();
            }
        }

        private void DoPending()
        {
            if (pending != ChangeType.None)
            {
                last = pending;

                switch (pending)
                {
                    case ChangeType.Grid:

                        UpdateTextFromGrid();

                        break;
                    case ChangeType.Text:

                        UpdateGridFromText();

                        break;
                }

                pending = ChangeType.None;
            }
        }

        private void OnGridChanged()
        {
            if (pending != ChangeType.None)
                return;
            
            pending = ChangeType.Grid;
        }
        
        private void UpdateTextFromGrid()
        {
            var count = gridSelected.Rows.Count;

            if (count > 0)
            {
                var sb = new StringBuilder(count * 4);

                foreach (DataGridViewRow row in gridSelected.Rows)
                {
                    sb.Append(((Category)row.Cells[0].Value).ID);
                    sb.Append(", ");
                }

                sb.Length -= 2;

                textAdvanced.Text = sb.ToString();
            }
            else
            {
                textAdvanced.Text = "";
            }
        }

        private void UpdateGridFromText()
        {
            var hs = new HashSet<ushort>();
            var ids = GetIDsFromText(hs);
            List<ushort> removed = null;

            gridCategories.SuspendLayout();
            gridSelected.SuspendLayout();

            gridSelected.Rows.Clear();

            //remove rows that were previously added, but are no longer used
            foreach (var row in categories.Values)
            {
                var c = (Category)row.Cells[0].Value;

                if (c.Temporary && !hs.Contains(c.ID))
                {
                    if (!selected.Remove(c.ID))
                    {
                        gridCategories.Rows.Remove(row);
                    }

                    row.Dispose();

                    if (removed == null)
                        removed = new List<ushort>();
                    removed.Add(c.ID);
                }
            }

            if (removed != null)
            {
                foreach (var id in removed)
                {
                    categories.Remove(id);
                }
            }

            var rows = new DataGridViewRow[ids.Length];
            int i;

            //set up selected rows
            for (i = 0; i < ids.Length;i++)
            {
                var id = ids[i];
                DataGridViewRow row;

                if (categories.TryGetValue(id, out row))
                {
                    if (!selected.Remove(id))
                    {
                        gridCategories.Rows.Remove(row);
                    }

                    row.Visible = true;
                }
                else
                {
                    row = (DataGridViewRow)gridCategories.RowTemplate.Clone();

                    row.CreateCells(gridCategories);
                    row.Cells[0].Value = new Category()
                    {
                        ID = id,
                        Name = GetDefaultName(id),
                        Temporary = true,
                    };

                    categories[id] = row;
                }

                rows[i] = row;
            }

            gridSelected.Rows.AddRange(rows);

            if (selected.Count > rows.Length)
            {
                rows = new DataGridViewRow[selected.Count];
            }

            i = 0;

            //move remaining rows that were previously selected
            foreach (var id in selected)
            {
                DataGridViewRow row;

                if (categories.TryGetValue(id, out row))
                {
                    rows[i++] = row;

                }
            }

            if (i > 0)
            {
                if (i != rows.Length)
                {
                    Array.Resize<DataGridViewRow>(ref rows, i);
                }
                Filter(rows);
                gridCategories.Rows.AddRange(rows);
            }

            selected.Clear();

            for (i = 0; i < ids.Length; i++)
            {
                selected.Add(ids[i]);
            }

            gridSelected.ResumeLayout();
            gridCategories.ResumeLayout();
        }

        private void MoveRows(object sender)
        {
            Controls.ScaledDataGridView to;
            var from = (Controls.ScaledDataGridView)sender;

            if (from == gridCategories)
                to = gridSelected;
            else
                to = gridCategories;

            MoveRows(from, to);
        }

        private void MoveRows(Controls.ScaledDataGridView from, Controls.ScaledDataGridView to)
        {
            var selected = from.GetSelected();

            if (selected.Length > 0)
            {
                var selecting = to == gridSelected;

                from.SuspendLayout();
                to.SuspendLayout();

                for (var i = selected.Length - 1; i >= 0; --i)
                {
                    from.Rows.Remove(selected[i]);

                    var c = (Category)selected[i].Cells[0].Value;

                    if (selecting)
                        this.selected.Add(c.ID);
                    else
                        this.selected.Remove(c.ID);
                }

                to.Rows.AddRange(selected);

                from.ResumeLayout();
                to.ResumeLayout();

                OnGridChanged();
            }
        }

        private void buttonAddRemove_Click(object sender, EventArgs e)
        {
            if (sender == buttonAdd)
                MoveRows(gridCategories, gridSelected);
            else
                MoveRows(gridSelected, gridCategories);
        }

        private void grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            MoveRows(sender);
        }

        private void grid_KeyUp(object sender, KeyEventArgs e)
        {
            bool b;

            if (e.KeyCode == Keys.Enter)
            {
                b = true;
            }
            else if (sender == gridCategories)
            {
                b = e.KeyCode == Keys.Right;
            }
            else
            {
                b = e.KeyCode == Keys.Left;
            }

            if (b)
            {
                e.Handled = true;

                MoveRows(sender);
            }
        }

        private void grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
            }
        }

        private ushort[] GetIDsFromText(HashSet<ushort> ids = null)
        {
            if (ids == null)
                ids = new HashSet<ushort>();
            var split = textAdvanced.Text.Split(',', '\n');
            var result = new ushort[split.Length];
            var i = 0;

            foreach (var t in split)
            {
                ushort id;
                if (ushort.TryParse(t.Trim(), out id) && ids.Add(id))
                {
                    result[i++] = id;
                }
            }

            Array.Resize<ushort>(ref result, i);

            return result;
        }

        public ushort[] SelectedCategories
        {
            get;
            private set;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var t = pending == ChangeType.None ? last : pending;
            ushort[] selected;

            switch (t)
            {
                case ChangeType.Text:

                    selected = GetIDsFromText();

                    break;
                case ChangeType.Grid:

                    selected = new ushort[gridSelected.Rows.Count];
                    int i = 0;

                    foreach (DataGridViewRow row in gridSelected.Rows)
                    {
                        selected[i++] = (ushort)((Category)row.Cells[0].Value).ID;
                    }

                    break;
                default:

                    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

                    return;
            }

            this.SelectedCategories = selected;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        void textAdvanced_TextChanged(object sender, EventArgs e)
        {
            if (pending == ChangeType.None)
            {
                pending = ChangeType.Text;
            }
        }

        private void panelCategories_SizeChanged(object sender, EventArgs e)
        {
            var w = (panelCategories.Width - panelCategoriesLeftRight.Width) / 2;

            if (stackPanel2.Width != w)
            {
                stackPanel2.Width = w;
            }
        }

        private void radioCategories_CheckedChanged(object sender, EventArgs e)
        {
            var c = (RadioButton)sender;

            if (c.Checked)
            {
                var b = c == radioCategories;

                this.SuspendLayout();

                DoPending();

                panelCategories.Visible = b;
                panelCategoriesUpDown.Visible = b;
                panelAllCategories.Visible = b;
                textAdvanced.Visible = !b;

                this.ResumeLayout();
            }
        }

        private void checkShowAll_CheckedChanged(object sender, EventArgs e)
        {
            DoFilter();
        }

        private void textCategories_TextChanged(object sender, EventArgs e)
        {
            OnFilterChanged();
        }

        private async void OnFilterChanged()
        {
            var b = filtering != 0;

            filtering = Environment.TickCount;

            if (b)
            {
                return;
            }

            do
            {
                await Task.Delay(500);
            }
            while ((Environment.TickCount - filtering) < 500);

            filtering = 0;

            if (!IsDisposed)
            {
                DoFilter();
            }
        }

        private void DoFilter()
        {
            using (var o = gridCategories.BeginUpdate(true))
            {
                Filter(o.Rows);
            }
        }

        private void Filter(DataGridViewRow[] rows)
        {
            var all = checkShowAll.Checked;
            var t = textCategories.Text;

            if (string.IsNullOrWhiteSpace(t))
            {
                for (var i = 0; i < rows.Length; i++)
                {
                    rows[i].Visible = all || !((Category)rows[i].Cells[0].Value).Hidden;
                }
            }
            else
            {
                ushort n;
                var b = ushort.TryParse(t, out n);

                for (var i = 0; i < rows.Length; i++)
                {
                    var c = (Category)rows[i].Cells[0].Value;
                    var v = all || !c.Hidden;

                    if (v)
                    {
                        v = b && c.ID == n || c.Name.IndexOf(t, StringComparison.OrdinalIgnoreCase) != -1;
                    }

                    rows[i].Visible = v;
                }
            }
        }
    }
}
