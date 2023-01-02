using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Markers
{
    public partial class formMarkerTrigger : Base.BaseForm
    {
        private Settings.MarkerTriggerCondition source;

        public enum TriggerType
        {
            Click,
            Time,
            Map,
        }

        public formMarkerTrigger(Settings.MarkerTriggerCondition source = null)
        {
            this.source = source;

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            gridTrigger.Rows.AddRange(new DataGridViewRow[]
                {
                    CreateRow(TriggerType.Time, "Time"),
                    CreateRow(TriggerType.Map, "Map"),
                });

            comboTime.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.TimeOfDay, "Daily"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DayOfWeek, "Weekly"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DayOfMonth, "Monthly"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DayOfYear, "Yearly"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes, "Duration"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DurationInMinutesInterval, "Interval"),
                });

            if (source == null)
            {
                comboType.SelectedIndex = 0;
                comboTime.SelectedIndex = 0;
            }
            else
            {
                var t = GetType(source.Type);
                var row = GetRow(t);

                row.Selected = true;

                if (t == TriggerType.Time)
                {
                    Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>.Select(comboTime, source.Type);

                    if (source is Settings.MarkerTriggerDateTime)
                    {
                        var mt = (Settings.MarkerTriggerDateTime)source;
                        dateCustom.Value = mt.Date;
                        timeCustom.Value = mt.Date.Add(mt.Time);
                        comboDateKind.SelectedIndex = mt.Kind == DateTimeKind.Local ? 0 : 1;
                    }
                    else if (source is Settings.MarkerTriggerTime)
                    {
                        var mt = (Settings.MarkerTriggerTime)source;
                        numericDays.Value = mt.TotalMinutes / 1440;
                        numericHours.Value = (mt.TotalMinutes % 1440) / 60;
                        numericMinutes.Value = mt.TotalMinutes % 60;

                        if (source is Settings.MarkerTriggerDurationMinutesInterval)
                        {
                            var o = ((Settings.MarkerTriggerDurationMinutesInterval)source).Origin;
                            dateCustom.Value = o;
                            timeCustom.Value = o;
                            comboDateKind.SelectedIndex = o.Kind == DateTimeKind.Local ? 0 : 1;
                        }
                    }
                }
                else if (t == TriggerType.Map)
                {
                    if (source is Settings.MarkerTriggerMap)
                    {
                        var mt = (Settings.MarkerTriggerMap)source;

                        numericMapId.Value = (int)mt.Map;
                    }

                    if (source is Settings.MarkerTriggerMapCoordinate)
                    {
                        var mt = (Settings.MarkerTriggerMapCoordinate)source;
                        float x, y, z;
                        Tools.Mumble.MumbleData.ConvertCoordinates(new float[] { mt.X, mt.Z, mt.Y }, out x, out y, out z);
                        numericMapX.Value = (int)x;
                        numericMapY.Value = (int)y;
                        numericMapZ.Value = (int)z;
                        numericMapRadius.Value = (int)(mt.Radius * Tools.Mumble.MumbleData.METER_TO_INCH + 0.5f);
                    }
                }
            }

            gridTrigger.SelectionChanged += gridCategory_SelectionChanged;

            gridCategory_SelectionChanged(null, null);
        }

        private TriggerType GetType(Settings.MarkerTriggerCondition.TriggerType t)
        {
            switch (t)
            {
                case Settings.MarkerTriggerCondition.TriggerType.DayOfMonth:
                case Settings.MarkerTriggerCondition.TriggerType.DayOfWeek:
                case Settings.MarkerTriggerCondition.TriggerType.DayOfYear:
                case Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes:
                case Settings.MarkerTriggerCondition.TriggerType.DurationInMinutesInterval:
                case Settings.MarkerTriggerCondition.TriggerType.TimeOfDay:
                    return TriggerType.Time;
                case Settings.MarkerTriggerCondition.TriggerType.Map:
                case Settings.MarkerTriggerCondition.TriggerType.MapCoordinate:
                    return TriggerType.Map;
            };
            return TriggerType.Time;
        }

        private DataGridViewRow GetRow(TriggerType t)
        {
            foreach (DataGridViewRow row in gridTrigger.SelectedRows)
            {
                var cell = row.Cells[columnTrigger.Index];
                if (((Util.ComboItem<TriggerType>)cell.Value).Value == t)
                    return row;
            }
            return null;
        }

        private DataGridViewRow CreateRow(TriggerType t, string text)
        {
            DataGridViewRow row;

            row = (DataGridViewRow)gridTrigger.RowTemplate.Clone();
            row.CreateCells(gridTrigger);

            var cell = row.Cells[columnTrigger.Index];
            cell.Value = new Util.ComboItem<TriggerType>(t, text);

            return row;
        }

        private async void AutoFillMap(Tools.Mumble.MumbleMonitor.IMumbleProcess m)
        {
            using (var s = m.Subscribe(Tools.Mumble.MumbleMonitor.DataScope.Basic))
            {
                try
                {
                    var data = await s.GetData<Tools.Mumble.MumbleData.PositionData>(500);
                    float x, y, z;
                    Tools.Mumble.MumbleData.ConvertCoordinates(data.fAvatarPosition, out x, out y, out z);
                    
                    numericMapX.Value = (int)x;
                    numericMapY.Value = (int)y;
                    numericMapZ.Value = (int)z;
                    numericMapId.Value = (int)data.mapId;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        private DataGridViewRow GetSelected(DataGridView grid)
        {
            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                return row;
            }

            return null;
        }

        void gridCategory_SelectionChanged(object sender, EventArgs e)
        {
            var selected = GetSelected(gridTrigger);

            if (selected != null)
            {
                CurrentPanel = GetPanel(((Util.ComboItem<TriggerType>)selected.Cells[columnTrigger.Index].Value).Value);
            }
        }

        private void labelMapInsert_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();

            foreach (var a in Client.Launcher.GetActiveProcesses())
            {
                if (a.Type == Settings.AccountType.GuildWars2 && Client.Launcher.GetState(a) == Client.Launcher.AccountState.ActiveGame)
                {
                    var m = Client.Launcher.GetMumbleLink(a);
                    if (m == null)
                        continue;
                    menu.Items.Add(a.Name, null, fillMapData_Click).Tag = m;
                }
            }

            if (menu.Items.Count == 0)
            {
                menu.Items.Add("No accounts are active").Enabled = false;
            }

            menu.Closed += delegate
            {
                menu.Dispose();
            };

            menu.Show(Cursor.Position);
        }

        void fillMapData_Click(object sender, EventArgs e)
        {
            var m = (Tools.Mumble.MumbleMonitor.IMumbleProcess)((Control)sender).Tag;
            if (m != null)
                AutoFillMap(m);
        }

        private Panel _CurrentPanel;
        private Panel CurrentPanel
        {
            get
            {
                return _CurrentPanel;
            }
            set
            {
                if (_CurrentPanel != value)
                {
                    panelContainer.SuspendLayout();

                    if (_CurrentPanel != null)
                        _CurrentPanel.Visible = false;
                    value.Visible = true;
                    _CurrentPanel = value;

                    panelContainer.ResumeLayout();
                }
            }
        }

        private Panel GetPanel(TriggerType t)
        {
            switch (t)
            {
                case TriggerType.Map:
                    return panelMap;
                case TriggerType.Time:
                    return panelTime;
            }
            return null;
        }

        private void comboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentPanel = GetPanel(Util.ComboItem<TriggerType>.SelectedValue(comboType));
        }

        private void comboTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelTime.SuspendLayout();

            var t = Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>.SelectedValue(comboTime);
            string format;

            panelDuration.Visible = t == Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes || t == Settings.MarkerTriggerCondition.TriggerType.DurationInMinutesInterval;
            panelDateTime.Visible = true;
            labelTimeInterval.Visible = t == Settings.MarkerTriggerCondition.TriggerType.DurationInMinutesInterval;
            panelDateTime.Enabled = t != Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes;
            dateCustom.Visible = t != Settings.MarkerTriggerCondition.TriggerType.TimeOfDay;

            switch (t)
            {
                case Settings.MarkerTriggerCondition.TriggerType.DayOfWeek:

                    format = "dddd";

                    break;
                case Settings.MarkerTriggerCondition.TriggerType.DayOfYear:
                default:

                    format = "MM/dd/yyyy";

                    break;
            }

            dateCustom.CustomFormat = format;

            panelTime.ResumeLayout();
        }

        public Settings.MarkerTriggerCondition Result
        {
            get;
            private set;
        }

        private Settings.MarkerTriggerCondition.TriggerType GetSelectedTriggerType()
        {
            switch (Util.ComboItem<TriggerType>.SelectedValue(comboType))
            {
                case TriggerType.Map:

                    if (checkMapCoordinates.Checked)
                        return Settings.MarkerTriggerCondition.TriggerType.MapCoordinate;
                    else
                        return Settings.MarkerTriggerCondition.TriggerType.Map;

                case TriggerType.Time:

                    return Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>.SelectedValue(comboTime);
            }

            return Settings.MarkerTriggerCondition.TriggerType.None;
        }

        private Settings.MarkerTriggerCondition GetSelectedResult()
        {
            var t = GetSelectedTriggerType();

            switch (t)
            {
                case Settings.MarkerTriggerCondition.TriggerType.Map:

                    return new Settings.MarkerTriggerMap((uint)numericMapId.Value);

                case Settings.MarkerTriggerCondition.TriggerType.MapCoordinate:

                    var coords = Tools.Mumble.MumbleData.ConvertCoordinates(numericMapX.Value, numericMapY.Value, numericMapZ.Value);
                    return new Settings.MarkerTriggerMapCoordinate((uint)numericMapId.Value, coords[0], coords[2], coords[1], numericMapRadius.Value / Tools.Mumble.MumbleData.METER_TO_INCH);

                case Settings.MarkerTriggerCondition.TriggerType.DayOfMonth:
                case Settings.MarkerTriggerCondition.TriggerType.DayOfWeek:
                case Settings.MarkerTriggerCondition.TriggerType.DayOfYear:
                case Settings.MarkerTriggerCondition.TriggerType.TimeOfDay:

                    var d = dateCustom.Value;
                    d = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, comboDateKind.SelectedIndex == 0 ? DateTimeKind.Local : DateTimeKind.Utc);

                    return Settings.MarkerTriggerTime.From(t, d, timeCustom.Value.TimeOfDay);

                case Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes:

                    return new Settings.MarkerTriggerDurationMinutes(numericDays.Value * 24 * 60 + numericHours.Value * 60 + numericMinutes.Value);

                case Settings.MarkerTriggerCondition.TriggerType.DurationInMinutesInterval:

                    var d1 = dateCustom.Value;
                    var d2 = timeCustom.Value;

                    return new Settings.MarkerTriggerDurationMinutesInterval(numericDays.Value * 24 * 60 + numericHours.Value * 60 + numericMinutes.Value, new DateTime(d1.Year, d1.Month, d1.Day, d2.Hour, d2.Minute, d2.Second, comboDateKind.SelectedIndex == 0 ? DateTimeKind.Local : DateTimeKind.Utc));
            }

            return null;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            var t = GetSelectedResult();
            t.Description = textDescription.Text;

            this.Result = t;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
