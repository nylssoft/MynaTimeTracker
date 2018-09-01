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
using System.ComponentModel;

namespace TimeTracker
{
    public class NonWorkingDays : INotifyPropertyChanged
    {
        public long Id { get; set; }

        private DateTime startDay;
        private DateTime endDay;
        private string name;
        private int hours;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public DateTime StartDay
        {
            get
            {
                return startDay;
            }
            set
            {
                startDay = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartDay"));
            }
        }

        public DateTime EndDay
        {
            get
            {
                return endDay;
            }
            set
            {
                endDay = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EndDay"));
            }
        }

        public int Hours
        {
            get
            {
                return hours;
            }
            set
            {
                hours = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hours"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalDays"));
            }
        }

        public double TotalDays
        {
            get
            {
                var days = Convert.ToInt32((EndDay - StartDay).TotalDays) + 1;
                return Hours == 4 ? days / 2.0 : days;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
