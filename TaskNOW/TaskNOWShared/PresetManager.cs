using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskNOW
{
    class PresetManager
    {
        private static Tuple<int, string, Func<DateTime, DateTime>>[] _TimePresets = new Tuple<int, string, Func<DateTime, DateTime>>[]
        {
            new Tuple<int, string, Func<DateTime, DateTime>>(0, "Current Time", delegate(DateTime now)
            {
                return now;
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(1, "In 30 Minutes", delegate(DateTime now)
            {
                return now.AddMinutes(30);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(2, "In 1 Hour", delegate(DateTime now)
            {
                return now.AddHours(1);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(3, "In 2 Hours", delegate(DateTime now)
            {
                return now.AddHours(2);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(4, "In 3 Hours", delegate(DateTime now)
            {
                return now.AddHours(3);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(5, "In 4 Hours", delegate(DateTime now)
            {
                return now.AddHours(4);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(6, "In 6 Hours", delegate(DateTime now)
            {
                return now.AddHours(6);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(7, "In 8 Hours", delegate(DateTime now)
            {
                return now.AddHours(8);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(8, "In 12 Hours", delegate(DateTime now)
            {
                return now.AddHours(12);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(9, "11:59 PM", delegate(DateTime now)
            {
                return SetTime(now, 23, 59, 59);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(10, "Noon", delegate(DateTime now)
            {
                return SetTime(now, 12, 0, 0);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(11, "9 PM", delegate(DateTime now)
            {
                return SetTime(now, 21, 0, 0);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(12, "6 PM", delegate(DateTime now)
            {
                return SetTime(now, 18, 0, 0);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(13, "3 PM", delegate(DateTime now)
            {
                return SetTime(now, 15, 0, 0);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(14, "9 AM", delegate(DateTime now)
            {
                return SetTime(now, 9, 0, 0);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(15, "6 AM", delegate(DateTime now)
            {
                return SetTime(now, 6, 0, 0);
            }),
            new Tuple<int, string, Func<DateTime, DateTime>>(16, "Manually Enter", null)
        };

        private static Tuple<int, string, Func<DateTime, DateTime, DateTime>>[] _DatePresets = new Tuple<int, string, Func<DateTime, DateTime, DateTime>>[]
        {
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(0, "Today or Within 24 Hours", delegate(DateTime now, DateTime time)
            {
                DateTime result = SetDate(time, now);
                if (result < now)
                    result += TimeSpan.FromDays(1);
                return result;
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(1, "Tomorrow", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(1), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(2, "In 2 Days", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(2), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(3, "In 3 Days", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(3), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(4, "In 4 Days", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(4), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(5, "In 5 Days", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(5), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(6, "In 6 Days", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(6), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(7, "In 7 Days", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(7), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(8, "In 2 Weeks", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(14), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(9, "In 3 Weeks", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(21), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(10, "In 4 Weeks", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(28), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(11, "In 30 Days", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddDays(30), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(12, "In 1 Month", delegate(DateTime now, DateTime time)
            {
                return SetTime(now.AddMonths(1), time);
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(13, "By Monday", delegate(DateTime now, DateTime time)
            {
                DateTime result = SetDate(time, now.AddDays(1));
                while (result.DayOfWeek != DayOfWeek.Monday)
                    result.AddDays(1);
                return result;
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(14, "By Tuesday", delegate(DateTime now, DateTime time)
            {
                DateTime result = SetDate(time, now.AddDays(1));
                while (result.DayOfWeek != DayOfWeek.Tuesday)
                    result.AddDays(1);
                return result;
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(15, "By Wednesday", delegate(DateTime now, DateTime time)
            {
                DateTime result = SetDate(time, now.AddDays(1));
                while (result.DayOfWeek != DayOfWeek.Wednesday)
                    result.AddDays(1);
                return result;
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(16, "By Thursday", delegate(DateTime now, DateTime time)
            {
                DateTime result = SetDate(time, now.AddDays(1));
                while (result.DayOfWeek != DayOfWeek.Thursday)
                    result.AddDays(1);
                return result;
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(17, "By Friday", delegate(DateTime now, DateTime time)
            {
                DateTime result = SetDate(time, now.AddDays(1));
                while (result.DayOfWeek != DayOfWeek.Friday)
                    result.AddDays(1);
                return result;
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(18, "By Saturday", delegate(DateTime now, DateTime time)
            {
                DateTime result = SetDate(time, now.AddDays(1));
                while (result.DayOfWeek != DayOfWeek.Saturday)
                    result.AddDays(1);
                return result;
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(19, "By Sunday", delegate(DateTime now, DateTime time)
            {
                DateTime result = SetDate(time, now.AddDays(1));
                while (result.DayOfWeek != DayOfWeek.Sunday)
                    result.AddDays(1);
                return result;
            }),
            new Tuple<int, string, Func<DateTime, DateTime, DateTime>>(20, "Manually Enter", null)
        };

        public static DateTime SetDate(DateTime input, DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, input.Hour, input.Minute, input.Second);
        }

        public static DateTime SetTime(DateTime input, DateTime time)
        {
            return new DateTime(input.Year, input.Month, input.Day, time.Hour, time.Minute, time.Second);
        }

        public static DateTime SetTime(DateTime input, int hour, int minute, int second)
        {
            return new DateTime(input.Year, input.Month, input.Day, hour, minute, second);
        }

        public static IEnumerable<Tuple<int, string, Func<DateTime, DateTime>>> GetTimePresets()
        {
            return new ReadOnlyCollection<Tuple<int, string, Func<DateTime, DateTime>>>(_TimePresets);
        }

        public static IEnumerable<Tuple<int, string, Func<DateTime, DateTime, DateTime>>> GetDatePresets()
        {
            return new ReadOnlyCollection<Tuple<int, string, Func<DateTime, DateTime, DateTime>>>(_DatePresets);
        }

        public static IEnumerable<Tuple<int, string>> GetProjectPresets()
        {
            List<Tuple<int, string>> result = new List<Tuple<int, string>>();
            int count = SettingsManager.GetSetting<int>("NumProjectPresets", true, 0);
            for (int i = 0; i < count; i++)
            {
                result.Add(new Tuple<int, string>(SettingsManager.GetSetting<int>("ProjectPresetId" + i, true, -1), SettingsManager.GetSetting<string>("ProjectPresetName" + i, true, "(Unknown Project)")));
            }
            return result;
        }

        public static void SetProjectPresets(ICollection<Project> projects)
        {
            SettingsManager.SetSetting<int>("NumProjectPresets", true, projects.Count);
            for (int i = 0; i < projects.Count; i++)
            {
                SettingsManager.SetSetting<int>("ProjectPresetId" + i, true, projects.ElementAt(i).Id);
                string str = projects.ElementAt(i).Name;
                str.ToString();
                SettingsManager.SetSetting<string>("ProjectPresetName" + i, true, projects.ElementAt(i).Name);
            }
        }
    }
}
