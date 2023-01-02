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
    public partial class formMarkerReset : Base.StackFormBase
    {
        public formMarkerReset()
        {
            InitializeComponent();

            comboReset.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.TimeOfDay, "Daily"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DayOfWeek, "Weekly"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DayOfMonth, "Monthly"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DayOfYear, "Yearly"),
                    new Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>(Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes, "Duration"),
                });

            this.Size = Size.Empty;

            comboReset.SelectedIndex = 0;
        }

        private void comboReset_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelContainer.SuspendLayout();

            var r = Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>.SelectedValue(comboReset);
            string format;

            panelDuration.Visible = r == Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes;
            panelSpecific.Visible = true;
            checkRelative.Visible = r == Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes;
            panelSpecific.Enabled = r != Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes || checkRelative.Checked;
            dateCustom.Visible = r != Settings.MarkerTriggerCondition.TriggerType.TimeOfDay;

            switch (r)
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

            panelContainer.ResumeLayout();
        }

        public Settings.MarkerTriggerTime Result
        {
            get;
            private set;
        }

        public DateTime SelectedDate
        {
            get;
            private set;
        }

        public TimeSpan SelectedTime
        {
            get;
            private set;
        }

        public TimeSpan SelectedDuration
        {
            get;
            private set;
        }

        public bool ResultKeepRelative
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
            var r = Util.ComboItem<Settings.MarkerTriggerCondition.TriggerType>.SelectedValue(comboReset);
            var d = dateCustom.Value;

            this.SelectedDate = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, comboZone.SelectedIndex == 0 ? DateTimeKind.Local : DateTimeKind.Utc);
            this.SelectedTime = timeCustom.Value.TimeOfDay;
            this.SelectedDuration = new TimeSpan(numericDays.Value, numericHours.Value, numericMinutes.Value, 0);
            this.ResultKeepRelative = checkRelative.Checked;

            switch (r)
            {
                case Settings.MarkerTriggerCondition.TriggerType.DurationInMinutes:

                    this.Result = Settings.MarkerTriggerTime.From(r, this.SelectedDate, this.SelectedDuration);

                    break;
                case Settings.MarkerTriggerCondition.TriggerType.TimeOfDay:
                case Settings.MarkerTriggerCondition.TriggerType.DayOfWeek:
                case Settings.MarkerTriggerCondition.TriggerType.DayOfMonth:
                case Settings.MarkerTriggerCondition.TriggerType.DayOfYear:
                default:

                    this.Result = Settings.MarkerTriggerTime.From(r, this.SelectedDate, this.SelectedTime);

                    break;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
