using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    public class WeekOvertime
    {
        public WeekOvertime(int year, int week, double worktime, double requiredtime)
        {
            var dfi = DateTimeFormatInfo.CurrentInfo;
            var cal = dfi.Calendar;
            var fd = dfi.FirstDayOfWeek;
            var firstinyear = new DateTime(year, 1, 1);
            var firstDayOfWeek = firstinyear.AddDays(dfi.FirstDayOfWeek - firstinyear.DayOfWeek);
            Year = year;
            Week = week;
            FirstDay = firstDayOfWeek.AddDays(7 * (week - 1));
            LastDay = FirstDay.AddDays(6.0);
            WorkTimePerWeek = worktime;
            RequiredWorkTimePerWeek = requiredtime;
        }

        public int Year { get; private set; }

        public int Week { get; private set; }

        public DateTime FirstDay { get; private set; }

        public DateTime LastDay { get; private set; }

        public double WorkTimePerWeek { get; private set; }

        public double RequiredWorkTimePerWeek { get; private set; }

        public double Overtime
        {
            get
            {
                return WorkTimePerWeek - RequiredWorkTimePerWeek;
            }
        }

        public string WeekDate
        {
            get
            {
                string p = "dd.MM"; // @TODO: not localized
                return FirstDay.ToString(p) + " - " + LastDay.ToString(p);
            }
        }
    }
}
