using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formNote : Form
    {
        private enum ExpiresType
        {
            None,
            DateTime,
            TimeSpan,
            DailyReset,
            MondayReset,

            CustomDateTime,
            CustomTimeSpan,
        }

        private class TimeSpan
        {
            public TimeSpan(int days, int hours, int minutes, int seconds)
            {
                this.days = days;
                this.hours = hours;
                this.minutes = minutes;
                this.seconds = seconds;
            }

            public int days, hours, minutes, seconds;

            public int TotalSeconds
            {
                get
                {
                    return seconds + minutes * 60 + hours * 3600 + days * 86400;
                }
            }
        }

        private class ExpiresItem
        {
            public ExpiresItem(ExpiresType type, object value)
            {
                this.Type = type;
                this.Value = value;
            }

            public ExpiresType Type
            {
                get;
                set;
            }

            public object Value
            {
                get;
                set;
            }

            public DateTime GetDate()
            {
                switch (Type)
                {
                    case ExpiresType.TimeSpan:
                        return DateTime.UtcNow.AddSeconds(((TimeSpan)Value).TotalSeconds);
                    case ExpiresType.DateTime:
                        return (DateTime)Value;
                    case ExpiresType.CustomDateTime:
                    case ExpiresType.CustomTimeSpan:
                    case ExpiresType.DailyReset:
                    case ExpiresType.MondayReset:
                        return ((Func<DateTime>)Value)();
                    default:
                        return DateTime.MaxValue;
                }
            }
        }

        private ExpiresItem lastItem;
        private bool visibleDate, visibleTime;
        private bool hasEvents;
        private TimeSpan customTime;
        private DateTime customDate;
        private byte indexCustomDate, indexCustomTime;

        public formNote()
        {
            InitializeComponent();

            var now = DateTime.Now;
            dateCustom.Value = customDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, DateTimeKind.Local).AddMinutes(1);
            customTime = new TimeSpan(0, 0, 0, 0);

            comboExpires.Items.AddRange(new Util.ComboItem<ExpiresItem>[]
                {
                    new Util.ComboItem<ExpiresItem>(new ExpiresItem(ExpiresType.None, DateTime.MaxValue), "Never"),
                    new Util.ComboItem<ExpiresItem>(new ExpiresItem(ExpiresType.TimeSpan, new TimeSpan(0,1,0,0)), "1 hour"),
                    new Util.ComboItem<ExpiresItem>(new ExpiresItem(ExpiresType.TimeSpan, new TimeSpan(0,24,0,0)), "24 hours"),
                    new Util.ComboItem<ExpiresItem>(new ExpiresItem(ExpiresType.TimeSpan, new TimeSpan(7,0,0,0)), "7 days"),
                    new Util.ComboItem<ExpiresItem>(new ExpiresItem(ExpiresType.DailyReset, new Func<DateTime>(
                        delegate
                        {
                            var d = DateTime.UtcNow.AddDays(1);
                            return new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, 0, DateTimeKind.Utc);
                        })), "Daily reset"),
                    new Util.ComboItem<ExpiresItem>(new ExpiresItem(ExpiresType.MondayReset, new Func<DateTime>(
                        delegate
                        {
                            var d = DateTime.UtcNow;
                            int days;
                            if (d.DayOfWeek > DayOfWeek.Monday)
                                days = 7 - (int)d.DayOfWeek + 1;
                            else
                            {
                                days = (int)DayOfWeek.Monday - (int)d.DayOfWeek;
                                if (days == 0 && (d.Hour > 7 || d.Hour == 7 && d.Minute >= 30))
                                    days = 7;
                            }
                            d = new DateTime(d.Year, d.Month, d.Day, 7, 30, 0, 0, DateTimeKind.Utc);
                            if (days > 0)
                                d = d.AddDays(days);
                            return d;
                        })), "Weekly reset (Monday)"),
                    new Util.ComboItem<ExpiresItem>(new ExpiresItem(ExpiresType.CustomDateTime, new Func<DateTime>(
                        delegate
                        {
                            var ticks = dateCustom.Value.Ticks;
                            if (comboZone.SelectedIndex == 0)
                                return new DateTime(ticks, DateTimeKind.Local).ToUniversalTime();
                            else
                                return new DateTime(ticks, DateTimeKind.Utc);
                        })), "Date/time"),
                    new Util.ComboItem<ExpiresItem>(new ExpiresItem(ExpiresType.CustomTimeSpan, new Func<DateTime>(
                        delegate
                        {
                            return DateTime.UtcNow.AddSeconds((int)numericDays.Value * 86400 + (int)numericHours.Value * 3600 + (int)numericMinutes.Value * 60 + (int)numericSeconds.Value);
                        })), "Duration"),
                });

            indexCustomDate = (byte)(comboExpires.Items.Count - 2);
            indexCustomTime = (byte)(indexCustomDate + 1);

            comboZone.SelectedIndex = 0;
            comboExpires.SelectedIndex = 1;
        }

        public formNote(DateTime expires, string text, bool notify)
            : this()
        {
            textMessage.Text = text;
            textMessage.Select(0, 0);

            if (expires == DateTime.MaxValue)
            {
                comboExpires.SelectedIndex = 0;
            }
            else
            {
                try
                {
                    comboExpires.SelectedIndex = indexCustomDate;
                    dateCustom.Value = expires.ToLocalTime();
                }
                catch
                {
                    try
                    {
                        dateCustom.Value = expires;
                        comboZone.SelectedIndex = 1;
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        comboExpires.SelectedIndex = 1;
                    }
                }
            }

            checkNotify.Checked = notify;
        }
        
        public string Message
        {
            get;
            private set;
        }

        public DateTime Expires
        {
            get;
            private set;
        }

        public bool NotifyOnExpiry
        {
            get;
            private set;
        }

        private bool DateVisible
        {
            get
            {
                return visibleDate;
            }
            set
            {
                if (visibleDate != value)
                {
                    if (value)
                    {
                        TimeVisible = false;
                        checkNotify.Top = panelSpecific.Bottom + panelSpecific.Top - comboExpires.Bottom;
                    }

                    panelSpecific.Visible = value;
                    visibleDate = value;
                }
            }
        }

        private bool TimeVisible
        {
            get
            {
                return visibleTime;
            }
            set
            {
                if (visibleTime != value)
                {
                    if (value)
                    {
                        DateVisible = false;
                        checkNotify.Top = panelDuration.Bottom + panelDuration.Top - comboExpires.Bottom;
                    }

                    panelDuration.Visible = value;
                    visibleTime = value;
                }
            }
        }

        private bool HasEvents
        {
            get
            {
                return hasEvents;
            }
            set
            {
                if (hasEvents != value)
                {
                    if (value)
                    {
                        dateCustom.ValueChanged += dateCustom_ValueChanged;
                        comboZone.SelectedIndexChanged += dateCustom_ValueChanged;
                        numericDays.TextChanged += numericTime_TextChanged;
                        numericHours.TextChanged += numericTime_TextChanged;
                        numericMinutes.TextChanged += numericTime_TextChanged;
                        numericSeconds.TextChanged += numericTime_TextChanged;
                    }
                    else
                    {
                        dateCustom.ValueChanged -= dateCustom_ValueChanged;
                        comboZone.SelectedIndexChanged -= dateCustom_ValueChanged;
                        numericDays.TextChanged -= numericTime_TextChanged;
                        numericHours.TextChanged -= numericTime_TextChanged;
                        numericMinutes.TextChanged -= numericTime_TextChanged;
                        numericSeconds.TextChanged -= numericTime_TextChanged;
                    }

                    hasEvents = value;
                }
            }
        }

        void dateCustom_ValueChanged(object sender, EventArgs e)
        {
            customDate = DateTime.MinValue;
            comboExpires.SelectedIndex = indexCustomDate;
        }

        void numericTime_TextChanged(object sender, EventArgs e)
        {
            customTime = null;
            comboExpires.SelectedIndex = indexCustomTime;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textMessage.TextLength == 0)
            {
                textMessage.Focus();
                return;
            }

            var item = Util.ComboItem<ExpiresItem>.SelectedValue(comboExpires);
            if (item == null)
                return;

            Message = textMessage.Text;
            Expires = item.GetDate();
            NotifyOnExpiry = checkNotify.Checked;

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkNotify_CheckedChanged(object sender, EventArgs e)
        {
            if (checkNotify.Checked && checkNotify.ContainsFocus && !Settings.NotesNotifications.HasValue)
            {
                if (MessageBox.Show(this, "Notifications haven't been configured, which can be done under the general tool settings.\n\nEnable the default configuration?", "Notifications not configured", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    Settings.NotesNotifications.Value = new Settings.NotificationScreenAttachment(0, Settings.ScreenAnchor.BottomRight, false);
                }
            }
        }

        private void comboExpires_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = Util.ComboItem<ExpiresItem>.SelectedValue(comboExpires);

            HasEvents = false;

            if (lastItem != null)
            {
                switch (lastItem.Type)
                {
                    case ExpiresType.None:

                        foreach (var c in new Controls.IntegerTextBox[] { numericDays,numericHours,numericMinutes,numericSeconds})
                        {
                            if (c.TextLength == 0)
                                c.Value = 0;
                        }

                        break;
                    case ExpiresType.CustomDateTime:

                        customDate = new DateTime(dateCustom.Value.Ticks, comboZone.SelectedIndex == 0 ? DateTimeKind.Local : DateTimeKind.Utc);

                        break;
                    case ExpiresType.CustomTimeSpan:

                        customTime = new TimeSpan(numericDays.Value, numericHours.Value, numericMinutes.Value, numericSeconds.Value);

                        break;
                }
            }

            switch (item.Type)
            {
                case ExpiresType.None:
                    
                    numericDays.Text = "";
                    numericHours.Text = "";
                    numericMinutes.Text = "";
                    numericSeconds.Text = "";
                    TimeVisible = true;
                    HasEvents = true;

                    break;
                case ExpiresType.TimeSpan:

                    var ts = (TimeSpan)item.Value;
                    numericDays.Value = ts.days;
                    numericHours.Value = ts.hours;
                    numericMinutes.Value = ts.minutes;
                    numericSeconds.Value = ts.seconds;
                    TimeVisible = true;
                    HasEvents = true;

                    break;
                case ExpiresType.DailyReset:
                case ExpiresType.MondayReset:

                    var dt = ((Func<DateTime>)item.Value)();
                    dateCustom.Value = dt;
                    comboZone.SelectedIndex = 1;
                    DateVisible = true;
                    HasEvents = true;

                    break;
                case ExpiresType.CustomDateTime:

                    if (customDate != DateTime.MinValue)
                    {
                        dateCustom.Value = customDate;
                        comboZone.SelectedIndex = customDate.Kind == DateTimeKind.Utc ? 1 : 0;
                    }

                    DateVisible = true;
                    HasEvents = false;

                    break;
                case ExpiresType.CustomTimeSpan:

                    if (customTime != null)
                    {
                        numericDays.Value = customTime.days;
                        numericHours.Value = customTime.hours;
                        numericMinutes.Value = customTime.minutes;
                        numericSeconds.Value = customTime.seconds;
                    }

                    TimeVisible = true;
                    HasEvents = false;

                    break;
            }

            lastItem = item;
        }
    }
}
