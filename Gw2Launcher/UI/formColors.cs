using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ColorNames = Gw2Launcher.UI.UiColors.Colors;

namespace Gw2Launcher.UI
{
    public partial class formColors : Base.BaseForm
    {
        private class ColorInfo
        {
            private UiColors.ColorValues values;
            private UiColors.ColorValues defaults;
            private ColorNames[][] shared;
            private bool previewed;

            public ColorNames[] GetShared(ColorNames c)
            {
                var i = (int)c;

                if (shared == null)
                    shared = UiColors.GetTheme(UiColors.Theme.Light).GetShared();

                if (i < 0 || i > shared.Length || shared[i] == null)
                    return new ColorNames[0];

                return shared[i];
            }

            public UiColors.ColorValues Values
            {
                get
                {
                    if (values == null)
                    {
                        var cv = UiColors.GetTheme();
                        values = new UiColors.ColorValues(cv.BaseTheme, cv.Decompress());
                    }
                    return values;
                }
                set
                {
                    values = value;
                }
            }

            public DataGridViewRow[] Rows
            {
                get;
                set;
            }

            public UiColors.ColorValues Original
            {
                get;
                set;
            }

            public UiColors.ColorValues Defaults
            {
                get
                {
                    if (defaults == null)
                        defaults = UiColors.GetTheme(UiColors.Theme.Settings);
                    return defaults;
                }
                set
                {
                    defaults = value;
                }
            }

            public void SetColor(ColorNames c, Color v)
            {
                Values[c] = v;
                Rows[(int)c].Cells[0].Value = v;
                Modified = true;
            }

            /// <summary>
            /// Applies the current colors to the current theme
            /// </summary>
            public void Preview()
            {
                if (values == null)
                    return;
                if (Original == null)
                    Original = UiColors.GetTheme().Clone();
                previewed = true;
                UiColors.SetColors(Values);
            }

            /// <summary>
            /// Resets the current theme to its original values
            /// </summary>
            public void Reset()
            {
                if (previewed && Original != null)
                {
                    UiColors.SetColors(Original);
                }
            }

            public void Save()
            {
                if (values == null)
                    return;
                Original = null;
                Modified = false;

                if (values.Equals(UiColors.GetTheme(UiColors.Theme.Light)))
                {
                    Settings.StyleColors.Clear();
                }
                else if (values.Equals(UiColors.GetTheme(UiColors.Theme.Dark)))
                {
                    Settings.StyleColors.Value = new Settings.UiColors()
                    {
                        Theme = UiColors.Theme.Dark,
                    };
                }
                else
                {
                    Settings.StyleColors.Value = new Settings.UiColors()
                    {
                        Colors = UiColors.Compress(values.BaseTheme, values.Decompress()),
                        Theme = UiColors.Theme.Settings,
                    };
                }
            }

            public bool Modified
            {
                get;
                set;
            }
        }

        private ColorInfo colorInfo;
        private Point locationColorDialog;

        public formColors()
        {
            locationColorDialog = new Point(int.MinValue, int.MinValue);

            InitializeComponents();

            Settings.StyleColors.ValueChanged += StyleColors_ValueChanged;
            this.Disposed += formColors_Disposed;
        }

        void formColors_Disposed(object sender, EventArgs e)
        {
            Settings.StyleColors.ValueChanged -= StyleColors_ValueChanged;
        }

        void StyleColors_ValueChanged(object sender, EventArgs e)
        {
            if (colorInfo != null && colorInfo.Original != null)
            {
                colorInfo.Original = UiColors.GetTheme(UiColors.Theme.Settings);
            }
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            InitColorsRows();
        }

        private void InitColorsRows()
        {
            colorInfo = new ColorInfo();
            var rows = colorInfo.Rows = new DataGridViewRow[colorInfo.Values.Count];

            for (var i = 0; i < rows.Length; i++)
            {
                var c = (ColorNames)i;
                var r = rows[i] = (DataGridViewRow)gridColors.RowTemplate.Clone();

                r.CreateCells(gridColors);

                r.Cells[0].Value = colorInfo.Values[c];
                r.Cells[1].Value = c.ToString();
                r.Tag = c;
            }

            darkToolStripMenuItem1.Checked = colorInfo.Values.BaseTheme == UiColors.Theme.Dark;
            lightToolStripMenuItem1.Checked = colorInfo.Values.BaseTheme == UiColors.Theme.Light;

            gridColors.SuspendLayout();

            gridColors.Rows.AddRange(rows);
            gridColors.Sort(columnName, ListSortDirection.Ascending);

            gridColors.ResumeLayout();
        }

        private void gridColors_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                var r = gridColors.Rows[e.RowIndex];
                var c = (ColorNames)r.Tag;
                var color = (Color)r.Cells[0].Value;

                using (var f = new ColorPicker.formColorDialog())
                {
                    if (locationColorDialog.X != int.MinValue)
                    {
                        f.StartPosition = FormStartPosition.Manual;
                        f.Location = locationColorDialog;
                    }

                    f.DefaultColor = colorInfo.Defaults[c];
                    f.AllowAlphaTransparency = UiColors.SupportsAlpha(c);
                    f.Color = color;

                    if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        if (changeAllSharedColorsToolStripMenuItem.Checked)
                        {
                            foreach (var cn in colorInfo.GetShared(c))
                            {
                                if (cn != c)
                                {
                                    colorInfo.SetColor(cn, f.Color);
                                }
                            }
                        }

                        colorInfo.SetColor(c, f.Color);

                        if (previewToolStripMenuItem.Checked)
                        {
                            colorInfo.Preview();
                        }
                    }

                    locationColorDialog = f.Location;
                }
            }
        }

        private void gridColors_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                var g = e.Graphics;

                var r = Rectangle.Inflate(e.CellBounds, -e.CellStyle.Padding.Left, -e.CellStyle.Padding.Top);

                using (var brush = new SolidBrush((Color)e.Value))
                {
                    g.FillRectangle(brush, r);
                    g.DrawRectangle(Pens.Black, r);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && colorInfo.Modified)
            {
                if (MessageBox.Show(this, "Changes have not been saved and will be reverted.\n\nSave changes?", "Save changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    Save();
                }
            }

            base.OnFormClosing(e);

            if (colorInfo != null)
            {
                colorInfo.Reset();
                colorInfo = null;
            }
        }

        private async void DoColorsFilter()
        {
            if (textColorsFilter.Tag != null)
                return;
            textColorsFilter.Tag = true;

            await Task.Delay(500);

            textColorsFilter.Tag = null;
            var t = textColorsFilter.Text;

            gridColors.SuspendLayout();

            if (t.StartsWith("shared:"))
            {
                var cn = ColorNames.Custom;

                try
                {
                    t = t.Substring(7);
                    if (t.Length > 0)
                        cn = (ColorNames)Enum.Parse(typeof(ColorNames), t, true);
                }
                catch { }

                foreach (DataGridViewRow row in gridColors.Rows)
                {
                    row.Visible = false;
                }

                if (cn != ColorNames.Custom)
                {
                    foreach (var c in colorInfo.GetShared(cn))
                    {
                        colorInfo.Rows[(int)c].Visible = true;
                    }
                }
            }
            else
            {
                foreach (DataGridViewRow row in gridColors.Rows)
                {
                    row.Visible = ((string)row.Cells[1].Value).IndexOf(t, StringComparison.OrdinalIgnoreCase) != -1;
                }
            }

            gridColors.ResumeLayout();
        }

        private void textColorsFilter_TextChanged(object sender, EventArgs e)
        {
            DoColorsFilter();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.Filter = "Colors|*.txt";

                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var count = 0;

                        foreach (var v in UiColors.Import(f.FileName))
                        {
                            colorInfo.SetColor(v.Key, v.Value);
                            ++count;
                        }

                        if (count > 0 && previewToolStripMenuItem.Checked)
                        {
                            colorInfo.Preview();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new SaveFileDialog())
            {
                f.Filter = "Colors in #RRGGBB|*.txt|Colors in R,G,B|*.txt";

                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var format = UiColors.ColorFormat.Hex;

                        if (f.FilterIndex == 2)
                            format = UiColors.ColorFormat.RGB;

                        UiColors.Export(f.FileName, colorInfo.Values, format);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Save()
        {
            colorInfo.Values.BaseTheme = darkToolStripMenuItem1.Checked ? UiColors.Theme.Dark : UiColors.Theme.Light;
            colorInfo.Save();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            previewToolStripMenuItem.Checked ^= true;

            if (previewToolStripMenuItem.Checked)
            {
                colorInfo.Preview();
            }
            else
            {
                colorInfo.Reset();
            }
        }

        private void LoadTheme(UiColors.Theme theme)
        {
            var colors = UiColors.GetTheme(theme);
            var count = colors.Count;

            for (var i = 0; i < count; i++)
            {
                colorInfo.SetColor((ColorNames)i, colors[(ColorNames)i]);
            }

            colorInfo.Values.BaseTheme = colors.BaseTheme;

            darkToolStripMenuItem1.Checked = colors.BaseTheme == UiColors.Theme.Dark;
            lightToolStripMenuItem1.Checked = colors.BaseTheme == UiColors.Theme.Light;

            if (previewToolStripMenuItem.Checked)
            {
                colorInfo.Preview();
            }
        }

        private void changeAllSharedColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeAllSharedColorsToolStripMenuItem.Checked ^= true;
        }

        private void lightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTheme(UiColors.Theme.Light);
        }

        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTheme(UiColors.Theme.Dark);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTheme(UiColors.Theme.Settings);

            colorInfo.Modified = false;
        }

        private void showSharedColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var row = (DataGridViewRow)contextColor.Tag;

            textColorsFilter.Text = "shared:" + (string)row.Cells[columnName.Index].Value;
        }

        private void gridColors_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.RowIndex >= 0)
            {
                var r = gridColors.Rows[e.RowIndex];
                r.Selected = true;
                contextColor.Tag = r;
                contextColor.Show(Cursor.Position);
            }
        }

        private void lightToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            lightToolStripMenuItem.Checked = true;
            darkToolStripMenuItem1.Checked = false;
        }

        private void darkToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            lightToolStripMenuItem.Checked = false;
            darkToolStripMenuItem1.Checked = true;
        }
    }
}
