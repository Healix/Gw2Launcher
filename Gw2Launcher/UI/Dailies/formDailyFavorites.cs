using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Dailies
{
    public partial class formDailyFavorites : Base.BaseForm
    {
        private Dictionary<int, DataGridViewRow> achievements;
        private DataGridViewRow[] rows;
        private Api.Daily daily;
        private int filtering;
        private bool filteringb;
        private Settings.DailiesItemOptions type;

        public formDailyFavorites(Api.Daily daily, Settings.DailiesItemOptions type)
        {
            this.achievements = new Dictionary<int, DataGridViewRow>();
            this.daily = daily;
            this.type = type;

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            switch (type)
            {
                case Settings.DailiesItemOptions.Favorite:

                    this.Text = "Favorites";

                    break;
                case Settings.DailiesItemOptions.Ignored:

                    this.Text = "Ignored";

                    break;
            }

            var tags = Settings.Dailies.ItemOptions.ToArray();
            var count = 0;

            for (var i = 0; i < tags.Length; i++)
            {
                if (tags[i].Key.Type == Settings.DailiesKeyType.DailyObjective && tags[i].Value == type)
                {
                    if (count != i)
                    {
                        tags[count] = tags[i];
                    }
                    ++count;
                }
            }

            if (count > 0)
            {
                var rows = this.rows = new DataGridViewRow[count];

                for (var i = 0; i < count; i++)
                {
                    var id = tags[i].Key.ID;
                    var row = CreateRow(id);

                    row.Cells[columnSelected.Index].Value = CheckState.Checked;
                    row.Cells[columnName.Index].Value = "(" + id + ")";

                    achievements[id] = row;
                    rows[i] = row;
                }

                gridAchievements.Rows.AddRange(rows);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LoadData();
        }

        private DataGridViewRow CreateRow(ushort id)
        {
            var row = (DataGridViewRow)gridAchievements.RowTemplate.Clone();

            row.CreateCells(gridAchievements);
            row.Cells[columnId.Index].Value = id;

            return row;
        }

        private async void LoadData()
        {
            Api.Daily.Achievement[] stored;

            try
            {
                stored = await daily.GetStoredAchievements();

                if (IsDisposed)
                {
                    return;
                }

                Array.Sort<Api.Daily.Achievement>(stored, new Comparison<Api.Daily.Achievement>(
                    delegate(Api.Daily.Achievement a, Api.Daily.Achievement b)
                    {
                        return a.ID.CompareTo(b.ID);
                    }));
            }
            catch 
            {
                return;
            }

            var rows = new DataGridViewRow[stored.Length];
            var count = 0;

            for (var i = 0; i < stored.Length; i++)
            {
                var id = stored[i].ID;
                DataGridViewRow row;

                if (!achievements.ContainsKey(id))
                {
                    row = CreateRow(id);

                    row.Cells[columnSelected.Index].Value = CheckState.Unchecked;

                    achievements[id] = row;
                    rows[count++] = row;
                }
                else
                {
                    row = this.achievements[id];
                }

                row.Cells[columnName.Index].Value = stored[i].Name;
            }

            if (count > 0)
            {
                var _rows = rows;

                if (count < rows.Length)
                {
                    Array.Resize<DataGridViewRow>(ref _rows, count);
                }

                using (var o = gridAchievements.BeginUpdate(false))
                {
                    if (!string.IsNullOrWhiteSpace(textAdvanced.Text))
                    {
                        if (this.rows != null)
                        {
                            Filter(this.rows);
                        }
                        Filter(_rows);
                    }

                    if (this.rows != null)
                    {
                        if (this.rows.Length + count == rows.Length)
                        {
                            _rows = rows;
                        }
                        else
                        {
                            _rows = new DataGridViewRow[this.rows.Length + count];
                        }

                        Array.Copy(rows, 0, _rows, this.rows.Length, count);
                        Array.Copy(this.rows, _rows, this.rows.Length);
                    }

                    this.rows = _rows;
                    o.Rows = _rows;
                }
            }
        }

        private void textAdvanced_TextChanged(object sender, EventArgs e)
        {
            OnFilterChanged();
        }

        private async void OnFilterChanged()
        {
            var b = filtering != 0;

            filtering = Environment.TickCount;
            filteringb = true;

            if (filtering == 0)
                ++filtering;

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

            if (!IsDisposed && filteringb)
            {
                DoFilter();
            }
        }

        private void DoFilter()
        {
            filteringb = false;

            using (var o = gridAchievements.BeginUpdate(false))
            {
                Filter(rows);
                o.Rows = rows;
            }
        }

        private void Filter(DataGridViewRow[] rows)
        {
            var t = textAdvanced.Text;

            if (string.IsNullOrWhiteSpace(t))
            {
                for (var i = 0; i < rows.Length;i++)
                {
                    rows[i].Visible = true;
                }
            }
            else
            {
                ushort n;
                var b = ushort.TryParse(t, out n);

                for (var i = 0; i < rows.Length; i++)
                {
                    rows[i].Visible = b && (ushort)rows[i].Cells[columnId.Index].Value == n || ((string)rows[i].Cells[columnName.Index].Value).IndexOf(t, StringComparison.OrdinalIgnoreCase) != -1;
                }

            }
        }

        public bool Modified
        {
            get;
            set;
        }

        private void gridAchievements_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = gridAchievements.Rows[e.RowIndex];
            var id = (ushort)row.Cells[columnId.Index].Value;
            var c = row.Cells[columnSelected.Index];
            var b = (CheckState)c.Value == CheckState.Checked;

            c.Value = b ? CheckState.Unchecked : CheckState.Checked;

            SetValue(id, b);

            Modified = true;
        }

        private void SetValue(ushort id, bool b)
        {
            var key = new Settings.DailiesItemKey(Settings.DailiesKeyType.DailyObjective, id);

            if (b)
            {
                Settings.Dailies.ItemOptions.Remove(key);
            }
            else
            {
                Settings.Dailies.ItemOptions[key] = type;
            }
        }

        private void textAdvanced_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ushort id;
                if (ushort.TryParse(textAdvanced.Text, out id) && id > 0)
                {
                    DataGridViewRow row;

                    if (!achievements.TryGetValue(id, out row))
                    {
                        row = CreateRow(id);

                        row.Cells[columnSelected.Index].Value = CheckState.Checked;
                        row.Cells[columnName.Index].Value = "(" + id + ")";

                        achievements[id] = row;

                        var _rows = new DataGridViewRow[rows.Length + 1];

                        Array.Copy(rows, 0, _rows, 1, rows.Length);
                        _rows[0] = row;
                        rows = _rows;

                        textAdvanced.Clear();

                        e.SuppressKeyPress = true;
                        e.Handled = true;

                        SetValue(id, true);
                        DoFilter();
                        Modified = true;
                    }
                }
            }
        }

        private void textAdvanced_KeyUp(object sender, KeyEventArgs e)
        {
        }
    }
}
