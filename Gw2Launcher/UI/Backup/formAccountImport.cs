using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Gw2Launcher.Tools.Backup;

namespace Gw2Launcher.UI.Backup
{
    public partial class formAccountImport : Base.BaseForm
    {
        private const int MAX_TAG_LENGTH = 20;

        private class FieldMapping
        {
            public FieldMapping(AccountExporter.FieldData source, AccountExporter.FieldData mapping)
            {
                this.Source = source;
                this.Mapping = mapping;
            }

            public AccountExporter.FieldData Source
            {
                get;
                set;
            }

            public AccountExporter.FieldData Mapping
            {
                get;
                set;
            }
        }

        private class LabelSample : Label
        {

        }

        private class MappedImportData
        {
            public bool HasUnknownType
            {
                get;
                set;
            }

            public Dictionary<string,Settings.IFile> Files
            {
                get;
                set;
            }

            public Dictionary<string, AccountExporter.FieldValue>[] Accounts
            {
                get;
                set;
            }

            public HashSet<string> TagsWithData
            {
                get;
                set;
            }
        }

        private AccountExporter exporter;
        private AccountExporter.FieldData ignored;
        private Util.ReusableControls reusable;
        private List<Control[]> previous;
        private AccountExporter.ImportData imported;
        private MappedImportData mapped;

        public formAccountImport()
        {
            exporter = new AccountExporter();
            ignored = new AccountExporter.FieldData("");
            previous = new List<Control[]>();

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

            panelImportScroll.BringToFront();
            ShowPanel(panelImportScroll);
        }

        public void Load(string path)
        {
            imported = exporter.Import(path);

            LoadMapping(imported,
                delegate(AccountExporter.FieldData f)
                {
                    switch (f.Tag)
                    {
                        case AccountExporter.Tags.CREATED:
                        case AccountExporter.Tags.LAST_USED:
                        case AccountExporter.Tags.UID:

                            return true;
                    }

                    return false;
                });

            ShowPanel(panelMappingsScroll);
        }

        private MappedImportData GetMappedImportData(AccountExporter.ImportData data, Dictionary<AccountExporter.FieldData, AccountExporter.FieldData> mapping)
        {
            var hasUnknown = false;
            var mappings = new Dictionary<string, AccountExporter.FieldValue>[data.Accounts.Count];
            var files = new Dictionary<string, Settings.IFile>();
            var tagsWithData = new HashSet<string>();

            for (var i = mappings.Length - 1; i >= 0; --i)
            {
                var a = data.Accounts[i];
                var m = mappings[i] = new Dictionary<string, AccountExporter.FieldValue>(a.Values.Count);
                var unknownType = true;

                foreach (var v in a.Values)
                {
                    var f = mapping[v.Field];
                    if (!f.IsValid)
                        continue;

                    m[f.Tag] = v;

                    if (tagsWithData.Add(f.Tag) && string.IsNullOrWhiteSpace(v.Value))
                        tagsWithData.Remove(f.Tag);

                    switch (f.Tag)
                    {
                        case AccountExporter.Tags.DAT_PATH:
                        case AccountExporter.Tags.GFX_PATH:

                            if (!string.IsNullOrWhiteSpace(v.Value))
                                files[v.Value] = null;

                            break;
                        case AccountExporter.Tags.TYPE:

                            if (!hasUnknown && GetType(v.Value) != AccountExporter.AccountType.Any)
                                unknownType = false;

                            break;
                    }
                }

                if (unknownType)
                    hasUnknown = true;
            }

            return new MappedImportData()
                {
                    Files = files,
                    HasUnknownType = hasUnknown,
                    Accounts = mappings,
                    TagsWithData = tagsWithData,
                };
        }

        private AccountExporter.AccountType GetType(string type)
        {
            switch (type)
            {
                case "GuildWars1":
                    return AccountExporter.AccountType.Gw1;
                case "GuildWars2":
                    return AccountExporter.AccountType.Gw2;
            }

            switch (type.ToLower().Replace(" ", ""))
            {
                case "guildwars":
                case "guildwars1":
                case "gw1":
                    return AccountExporter.AccountType.Gw1;
                case "guildwars2":
                case "gw2":
                    return AccountExporter.AccountType.Gw2;
            }

            return AccountExporter.AccountType.Any;
        }

        private Control[] CurrentPanel
        {
            get
            {
                if (previous.Count > 0)
                    return previous[previous.Count - 1];
                return null;
            }
        }

        private void ShowPanel(params Control[] panels)
        {
            //panelContent.SuspendLayout();

            var current = CurrentPanel;

            foreach (var p in panels)
            {
                p.Visible = true;
            }

            if (current != null)
            {
                foreach (var p in current)
                {
                    p.Visible = false;
                    p.SendToBack();
                }
            }

            previous.Add(panels);

            //panelContent.ResumeLayout();

            if (buttonCancel.Visible)
            {
                panelNav.SuspendLayout();

                buttonCancel.Visible = false;
                buttonBack.Visible = true;

                panelNav.ResumeLayout();
            }
        }

        private void ShowPrevious()
        {
            var i = previous.Count - 1;

            if (i > 0)
            {
                //panelContent.SuspendLayout();

                var current = previous[i];

                previous.RemoveAt(i);

                foreach (var p in previous[i - 1])
                {
                    p.Visible = true;
                }

                foreach (var p in current)
                {
                    p.Visible = false;
                    p.SendToBack();
                }

                //panelContent.ResumeLayout();
            }

            if (i <= 1 || buttonImport.Visible)
            {
                panelNav.SuspendLayout();

                if (i <= 1)
                {
                    buttonCancel.Visible = true;
                    buttonBack.Visible = false;
                }

                if (buttonImport.Visible)
                {
                    buttonImport.Visible = false;
                    buttonNext.Visible = true;
                }

                panelNav.ResumeLayout();
            }
        }

        private void LoadMapping(AccountExporter.ImportData data, Func<AccountExporter.FieldData, bool> isIgnored = null)
        {
            var items = new AccountExporter.FieldData[exporter.Fields.Length + 1];
            var samples = new Dictionary<AccountExporter.FieldData, string>(data.Fields.Length);

            //var items2 = new List<AccountExporter.FieldData>(exporter.Fields.Length + 1);
            //items2.Add(ignored);
            //foreach (var field in exporter.Fields)
            //{
            //    if (isIgnored == null || !isIgnored(field))
            //        items2.Add(field);
            //}
            //if (items2.Count > 1)
            //{
            //    items2.Sort(1, items2.Count - 1,
            //        Comparer<AccountExporter.FieldData>.Create(delegate(AccountExporter.FieldData f1, AccountExporter.FieldData f2)
            //        {
            //            return f1.Name.CompareTo(f2.Name);
            //        }));
            //}
            //items = items2.ToArray();

            items[0] = ignored;
            {
                var j = 1;
                for (var i = 0; i < exporter.Fields.Length; i++)
                {
                    switch (exporter.Fields[i].Tag)
                    {
                        case AccountExporter.Tags.UID:
                        case AccountExporter.Tags.CREATED:
                            continue;
                    }
                    items[j++] = exporter.Fields[i];
                }
                if (j != items.Length)
                    Array.Resize<AccountExporter.FieldData>(ref items, j);
            }
            Array.Sort<AccountExporter.FieldData>(items, 1, items.Length - 1,
                Comparer<AccountExporter.FieldData>.Create(delegate(AccountExporter.FieldData f1, AccountExporter.FieldData f2)
                {
                    return f1.Name.CompareTo(f2.Name);
                }));

            if (data.Accounts.Count > 0)
            {
                foreach (var v in data.Accounts[0].Values)
                {
                    samples[v.Field] = v.Value;
                }
            }

            panelMappings.SuspendLayout();
            tableMapping.SuspendLayout();

            #region Reusable controls

            if (reusable == null)
                reusable = new Util.ReusableControls();
            else
                reusable.ReleaseAll();

            var labels1 = reusable.CreateOrAll<Label>(data.Fields.Length,
                delegate
                {
                    return new Label()
                    {
                        Margin = labelTemplateField.Margin,
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        UseMnemonic = false,
                    };
                });
            var labels2 = reusable.CreateOrAll<LabelSample>(data.Fields.Length,
                delegate
                {
                    return new LabelSample()
                    {
                        Margin = labelTemplateSample.Margin,
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        ForeColor = SystemColors.GrayText,
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoEllipsis = true,
                        Height = labelTemplateField.Height,
                        UseMnemonic = false,
                    };
                });
            var combos = reusable.CreateOrAll<ComboBox>(data.Fields.Length,
                delegate
                {
                    var combo = new ComboBox()
                    {
                        Anchor = AnchorStyles.Left,
                        Margin = comboTemplateMapping.Margin,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Width = comboTemplateMapping.Width,
                    };
                    combo.Items.AddRange(items);
                    return combo;
                });

            #endregion

            tableMapping.RowCount = 1;

            foreach (var f in data.Fields)
            {
                var m = new FieldMapping(f, f.IsValid && (isIgnored == null || !isIgnored(f)) ? f : ignored);

                var labelTag = labels1.GetNext();
                var labelSample = labels2.GetNext();
                var comboMapping = combos.GetNext();

                string sample;
                if (!samples.TryGetValue(f, out sample))
                    sample = "";

                labelTag.Text = f.Tag.Length > MAX_TAG_LENGTH ? f.Tag.Substring(0, MAX_TAG_LENGTH) : f.Tag;
                labelSample.Text = sample;
                comboMapping.Tag = m;
                comboMapping.SelectedItem = m.Mapping;

                labelTag.Visible = true;
                labelSample.Visible = true;
                comboMapping.Visible = true;

                var row = tableMapping.RowCount++;

                tableMapping.Controls.Add(labelTag, 0, row);
                tableMapping.Controls.Add(comboMapping, 1, row);
                tableMapping.Controls.Add(labelSample, 2, row);
            }

            labels1.HideRemaining();
            labels2.HideRemaining();

            while (combos.HasNext)
            {
                var c = combos.GetNext();
                c.Visible = false;
                c.Tag = null;
            }

            var b = data.Fields.Length > 0;
            tableMapping.Visible = b;
            labelWarningBlankMapping.Visible = b;
            labelWarningIgnoredMappings.Visible = b;
            labelNoMappings.Visible = !b;

            tableMapping.ResumeLayout();
            panelMappings.ResumeLayout();
        }

        private Dictionary<AccountExporter.FieldData, AccountExporter.FieldData> GetMapping()
        {
            var mapping = new Dictionary<AccountExporter.FieldData, AccountExporter.FieldData>(imported.Fields.Length);

            foreach (Control c in tableMapping.Controls)
            {
                if (c is ComboBox && c.Tag != null)
                {
                    var  m = (FieldMapping)c.Tag;
                    var selected = (AccountExporter.FieldData)((ComboBox)c).SelectedItem;
                    mapping[m.Source] = selected;
                }
            }

            return mapping;
        }

        private void PopulateAccounts(MappedImportData data)
        {
            gridAccounts.SuspendLayout();
            gridAccounts.Rows.Clear();

            foreach (var a in data.Accounts)
            {
                var row = (DataGridViewRow)gridAccounts.RowTemplate.Clone();
                row.CreateCells(gridAccounts);

                string name;
                AccountExporter.FieldValue v;
                if (!a.TryGetValue(AccountExporter.Tags.NAME, out v) || string.IsNullOrWhiteSpace(name = v.Value))
                    name = "(no name)";

                row.Cells[columnName.Index].Value = name;
                row.Tag = a;
                
                gridAccounts.Rows.Add(row);
            }

            gridAccounts.Height = gridAccounts.GetPreferredSize(new Size(gridAccounts.Width, int.MaxValue)).Height;

            gridAccounts.ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (reusable != null)
                    reusable.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            var current = CurrentPanel[0];

            if (current == panelImportScroll)
            {
                if (imported == null)
                {
                    buttonBrowse_Click(sender, e);
                }
                else
                {
                    ShowPanel(panelMappingsScroll);
                }
            }
            else if (current == panelMappingsScroll)
            {
                mapped = GetMappedImportData(imported, GetMapping());

                PopulateAccounts(mapped);

                var hasLastUsed = mapped.TagsWithData.Contains(AccountExporter.Tags.LAST_USED);

                if (hasLastUsed || mapped.HasUnknownType || mapped.Files.Count > 0)
                {
                    panelFiles.Visible = mapped.Files.Count > 0;
                    panelType.Visible = mapped.HasUnknownType;
                    panelImportConfirm.Visible = hasLastUsed;

                    checkImportLastUsed.Visible = hasLastUsed;

                    ShowPanel(panelRequiredScroll);
                }
                else
                {
                    ShowPanel(panelAccounts);
                    ShowImport();
                }
            }
            else if (current == panelRequiredScroll)
            {
                ShowPanel(panelAccounts);
                ShowImport();
            }
        }

        private void ShowImport()
        {
            panelNav.SuspendLayout();

            buttonImport.Enabled = false;
            buttonNext.Visible = false;
            buttonImport.Visible = true;

            panelNav.ResumeLayout();

            EnableImport();
        }

        async void EnableImport()
        {
            EventHandler onChanged = null;
            onChanged = delegate
            {
                buttonImport.VisibleChanged -= onChanged;
                onChanged = null;
            };
            buttonImport.VisibleChanged += onChanged;

            await Task.Delay(500);

            if (onChanged != null)
            {
                buttonImport.Enabled = true;
                onChanged(null, null);
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.Filter = "XML or CSV files|*.xml;*.csv";

                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        Load(f.FileName);

                        textBrowse.Text = f.FileName;
                        textBrowse.Select(textBrowse.TextLength, 0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            ShowPrevious();
        }

        private Task<Settings.IFile> CopyAsync(Settings.IFile file, bool move)
        {
            return Task.Run(
                delegate
                {
                    return Copy(file, move);
                });
        }

        private Settings.IFile Copy(Settings.IFile file, bool move)
        {
            if (File.Exists(file.Path))
            {
                var tmp = Path.GetTempFileName();

                if (move)
                {
                    if (File.Exists(tmp))
                    {
                        try
                        {
                            File.Delete(tmp);
                        }
                        catch { }
                    }

                    File.Move(file.Path, tmp);
                }
                else
                {
                    File.Copy(file.Path, tmp, true);
                }
                
                file.Path = tmp;

                return file;
            }
            else
            {
                var tmp = Path.GetTempFileName();

                try
                {
                    using (File.Open(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                    { }
                }
                catch { }

                file.Path = tmp;

                return file;
            }
        }

        private async Task<Settings.IFile> GetFile(Client.FileManager.FileType type, string path)
        {
            var file = Client.FileManager.FindFile(type, path);
            if (file != null)
                return file;

            switch (type)
            {
                case Client.FileManager.FileType.Dat:
                    file = Settings.CreateDatFile();
                    break;
                case Client.FileManager.FileType.Gfx:
                    file = Settings.CreateGfxFile();
                    break;
                default:
                    return null;
            }

            file.Path = path;
            try
            {
                await CopyAsync(file, radioFilesMove.Checked);
            }
            catch { }
            return file;
        }

        private async void buttonImport_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            buttonImport.Enabled = false;

            var items = new Dictionary<string, AccountExporter.FieldValue>[gridAccounts.Rows.Count];
            var accounts = this.Accounts = new List<Settings.IAccount>(items.Length);
            var files = new Dictionary<string, Settings.IFile>();
            var fileKeys = new string[] { AccountExporter.Tags.GFX_PATH, AccountExporter.Tags.DAT_PATH };
            var i = 0;
            formProgressBar f = null;
            var fileCount = 0;

            foreach (DataGridViewRow row in gridAccounts.Rows)
            {
                var values = items[i++] = (Dictionary<string, AccountExporter.FieldValue>)row.Tag;
                AccountExporter.FieldValue v;

                foreach (var k in fileKeys)
                {
                    if (values.TryGetValue(k, out v) && !string.IsNullOrWhiteSpace(v.Value) && !files.ContainsKey(v.Value))
                    {
                        Settings.IFile file = null;

                        switch (k)
                        {
                            case AccountExporter.Tags.GFX_PATH:
                                file = Client.FileManager.FindFile(Client.FileManager.FileType.Gfx, v.Value);
                                break;
                            case AccountExporter.Tags.DAT_PATH:
                                file = Client.FileManager.FindFile(Client.FileManager.FileType.Dat, v.Value);
                                break;
                        }

                        files[v.Value] = file;

                        if (file == null)
                            ++fileCount;
                    }
                }
            }

            if (fileCount > 0)
            {
                f = new formProgressBar()
                {
                    Maximum = items.Length * 2,
                };

                f.CenterAt(this);
                f.Show(this);
            }

            using (f)
            {
                foreach (var values in items)
                {
                    if (f != null && !f.IsDisposed)
                        ++f.Value;

                    var type = radioGw1.Checked ? Settings.AccountType.GuildWars1 : Settings.AccountType.GuildWars2;
                    AccountExporter.FieldValue v;

                    if (values.TryGetValue(AccountExporter.Tags.TYPE, out v))
                    {
                        switch (GetType(v.Value))
                        {
                            case AccountExporter.AccountType.Gw1:
                                type = Settings.AccountType.GuildWars1;
                                break;
                            case AccountExporter.AccountType.Gw2:
                                type = Settings.AccountType.GuildWars2;
                                break;
                        }
                    }

                    var a = Settings.CreateAccount(type);
                    accounts.Add(a);

                    a.Name = a.UID.ToString();

                    foreach (var tag in values.Keys)
                    {
                        var kv = values[tag];

                        try
                        {
                            switch (tag)
                            {
                                case AccountExporter.Tags.GFX_PATH:

                                    if (a is Settings.IGw2Account)
                                    {
                                        Settings.IFile file;
                                        if (files.TryGetValue(kv.Value, out file) && file == null)
                                        {
                                            file = await GetFile(Client.FileManager.FileType.Gfx, kv.Value);
                                        }
                                        ((Settings.IGw2Account)a).GfxFile = (Settings.IGfxFile)file;
                                    }

                                    break;
                                case AccountExporter.Tags.DAT_PATH:

                                    if (a is Settings.IGw2Account)
                                    {
                                        Settings.IFile file;
                                        if (files.TryGetValue(kv.Value, out file) && file == null)
                                        {
                                            file = await GetFile(Client.FileManager.FileType.Dat, kv.Value);
                                        }
                                        ((Settings.IGw2Account)a).DatFile = (Settings.IDatFile)file;
                                    }

                                    break;
                                case AccountExporter.Tags.CREATED:

                                    //denied

                                    break;
                                case AccountExporter.Tags.LAST_USED:

                                    {
                                        DateTime d;
                                        if (DateTime.TryParse(kv.Value, out d))
                                        {
                                            if (d.Kind != DateTimeKind.Utc)
                                            {
                                                if (d.Kind == DateTimeKind.Local)
                                                    d = d.ToUniversalTime();
                                                else
                                                    d = new DateTime(d.Ticks, DateTimeKind.Utc);
                                            }
                                            a.LastUsedUtc = d;
                                        }
                                    }

                                    break;
                                default:

                                    if (kv.Field.SetValue(a, kv.Value))
                                        continue;

                                    break;
                            }
                        }
                        catch { }
                    }

                    if (f != null && !f.IsDisposed)
                        ++f.Value;
                }

                if (f != null)
                {
                    await Task.Delay(500);
                }
            }

            buttonImport.Enabled = true;
            this.Enabled = true;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void gridAccounts_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.RowIndex >= 0)
            {
                if (!gridAccounts.IsSelected(e.RowIndex))
                {
                    gridAccounts.ClearSelected();
                    gridAccounts.SelectRow(gridAccounts.Rows[e.RowIndex], true);

                    ToolStripDropDownClosedEventHandler onClosed = null;
                    onClosed = delegate(object o, ToolStripDropDownClosedEventArgs c)
                    {
                        contextAccounts.Closed -= onClosed;
                        if (c.CloseReason != ToolStripDropDownCloseReason.ItemClicked)
                            gridAccounts.ClearSelected();
                    };
                    contextAccounts.Closed += onClosed;

                    //var hasSelected = false;

                    //foreach (var row in gridAccounts.GetSelectedEnumerable())
                    //{
                    //    hasSelected = true;
                    //    break;
                    //}

                    //if (!hasSelected)
                    //{
                    //    gridAccounts.SelectRow(gridAccounts.Rows[e.RowIndex], true);
                    //}
                }

                contextAccounts.Show(Cursor.Position);
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selected = gridAccounts.GetSelected();
            if (selected.Count == 0)
                return;

            gridAccounts.SuspendLayout();

            foreach (var row in selected)
            {
                gridAccounts.Rows.Remove(row);
            }

            gridAccounts.ResumeLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var brush = new SolidBrush(panelNav.BackColor))
            {
                e.Graphics.FillRectangle(brush, 0, panel1.Bottom, panel1.Width, this.ClientSize.Height - panel1.Bottom);
            }
        }

        public List<Settings.IAccount> Accounts
        {
            get;
            private set;
        }
    }
}
