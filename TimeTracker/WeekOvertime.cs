/*
    Myna Time Tracker
    Copyright (C) 2018 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Globalization;

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
                string p = "MM/dd"; // english
                if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de")
                {
                    p = "dd.MM";
                }
                return FirstDay.ToString(p) + " - " + LastDay.ToString(p);
            }
        }
    }
}
