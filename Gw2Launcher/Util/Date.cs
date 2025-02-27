using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    static class Date
    {
        /// <summary>
        /// Returns the next week
        /// </summary>
        /// <param name="date">Current date</param>
        /// <param name="startOfWeek">The day the week starts from</param>
        /// <param name="hour">The hour the week starts froms</param>
        /// <param name="minute">The minute the week starts from</param>
        public static DateTime GetNextWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday, int hour = 7, int minute = 30)
        {
            int days = (int)startOfWeek - (int)date.DayOfWeek;

            if (days < 0)
            {
                days = 7 + days;
            }
            else if (days == 0 && (date.Hour > hour || date.Hour == hour && date.Minute >= minute))
            {
                days = 7;
            }

            //return date.Date.Add(new TimeSpan(days, hour, minute, 0, 0));
            return new DateTime((((date.Ticks / 864000000000 + days) * 24 + hour) * 60 + minute) * 60 * 1000 * 10000);
        }

        /// <summary>
        /// Returns the current week
        /// </summary>
        /// <param name="date">Current date</param>
        /// <param name="startOfWeek">The day the week starts from</param>
        /// <param name="hour">The hour the week starts froms</param>
        /// <param name="minute">The minute the week starts from</param>
        public static DateTime GetWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday, int hour = 7, int minute = 30)
        {
            int days = (int)startOfWeek - (int)date.DayOfWeek;

            if (days > 0)
            {
                days = days - 7;
            }
            else if (days == 0 && (date.Hour < hour || date.Hour == hour && date.Minute < minute))
            {
                days = -7;
            }

            //return date.Date.Add(new TimeSpan(days, hour, minute, 0, 0));
            return new DateTime((((date.Ticks / 864000000000 + days) * 24 + hour) * 60 + minute) * 60 * 1000 * 10000);
        }

        /// <summary>
        /// Returns the week in the year or the week in the prior year if the date occurs before the start of the first week
        /// </summary>
        /// <param name="date">Current date</param>
        /// <param name="startOfWeek">The day the week starts from</param>
        /// <param name="hour">The hour the week starts froms</param>
        /// <param name="minute">The minute the week starts from</param>
        public static int GetWeekInYear(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday, int hour = 7, int minute = 30)
        {
            var first = GetNextWeek(new DateTime(date.Year, 1, 1, 0, 0, 0, 0, date.Kind), startOfWeek, hour, minute);

            if (date < first)
            {
                first = GetNextWeek(new DateTime(date.Year - 1, 1, 1, 0, 0, 0, 0, date.Kind), startOfWeek, hour, minute);
            }

            return date.Subtract(first).Days / 7;
        }
    }
}
