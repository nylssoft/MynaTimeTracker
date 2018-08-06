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
    public class WorkTime : INotifyPropertyChanged
    {
        private DateTime startTime;
        private DateTime endTime;
        private Project project;
        private String description;

        public long Id { get; set; }

        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartTime"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Duration"));
            }
        }

        public DateTime EndTime
        {
            get
            {
                return endTime;
            }
            set
            {
                endTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EndTime"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Duration"));
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return EndTime - StartTime;
            }
        }

        public String Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Description"));
            }
        }

        public Project Project
        {
            get
            {
                return project;
            }

            set
            {
                project = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Project"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
