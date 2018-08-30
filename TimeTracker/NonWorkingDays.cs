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

        private DateTime from;
        private DateTime to;
        private string name;

        public NonWorkingDays(DateTime from, DateTime to, string name)
        {
            this.from = from;
            this.to = to;
            this.name = name;
        }

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

        public DateTime From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("From"));
            }
        }

        public DateTime To
        {
            get
            {
                return to;
            }
            set
            {
                to = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("To"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
