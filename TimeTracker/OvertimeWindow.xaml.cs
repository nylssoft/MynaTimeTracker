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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace TimeTracker
{
    public partial class OvertimeWindow : Window
    {
        private Database database;
        private ObservableCollection<WeekOvertime> weekOvertimes = new ObservableCollection<WeekOvertime>();

        public OvertimeWindow(Window owner, string title, Database database)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.database = database;
            InitializeComponent();
            listView.ItemsSource = weekOvertimes;
        }

        public ConfigureNonWorkingDaysWindow ConfigureNonWorkingDaysWindow { get; set; } = null;

        public bool IsClosed { get; set; } = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var f = database.GetFirstStartTime();
                if (f.HasValue)
                {
                    datePickerStartDay.SelectedDate = f.Value.GetDayDateTime();
                }
                Calculate();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private void ButtonNonWorkingDays_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConfigureNonWorkingDaysWindow == null || ConfigureNonWorkingDaysWindow.IsClosed)
                {
                    ConfigureNonWorkingDaysWindow = new ConfigureNonWorkingDaysWindow(null, Properties.Resources.TITLE_CONFIGURE_FREEDAYS, database);
                    ConfigureNonWorkingDaysWindow.Show();
                }
                else
                {
                    ConfigureNonWorkingDaysWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Calculate();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Calculate()
        {
            var oldcursor = Cursor;
            Cursor = Cursors.Wait;
            try
            {
                textBlockResult.Text = "";
                textBlockInfo.Text = "";
                weekOvertimes.Clear();
                if (!datePickerStartDay.SelectedDate.HasValue ||
                    !double.TryParse(textBoxHoursPerWeek.Text, out double hoursPerWeek) ||
                    !double.TryParse(textBoxStartOverTimeHours.Text, out double overTimeBefore))
                {
                    return;
                }
                var from = datePickerStartDay.SelectedDate.Value.GetDayDateTime();
                var to = DateTime.Today;
                textBlockInfo.Text = string.Format(Properties.Resources.TEXT_OVERTIME_0_1, from.ToLongDateString(), to.ToLongDateString());
                var freeDays = new HashSet<DateTime>();
                var halfDays = new HashSet<DateTime>();
                foreach (var nwd in database.SelectAllNonWorkingDays())
                {
                    for (var day = nwd.StartDay; day <= nwd.EndDay; day = day.AddDays(1.0))
                    {
                        freeDays.Add(day);
                        if (nwd.Hours == 4)
                        {
                            halfDays.Add(day);
                        }
                    }
                }
                var weekInfo = new Dictionary<Tuple<int, int>, Tuple<double, double>>();
                double overTime = overTimeBefore;
                double hoursPerDay = hoursPerWeek / 5.0;
                var dfi = DateTimeFormatInfo.CurrentInfo;
                var cal = dfi.Calendar;
                var dt = from;
                while (dt < to)
                {
                    var hours = WorkTime.CalculateTotalHours(database.SelectWorkTimes(dt));
                    var weekofyear = cal.GetWeekOfYear(dt, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                    var key = Tuple.Create(dt.Year, weekofyear);
                    if (!weekInfo.TryGetValue(key, out Tuple<double, double> t))
                    {
                        t = Tuple.Create(0.0, 0.0);
                        weekInfo[key] = t;
                    }
                    if (!dt.IsWorkDay() || freeDays.Contains(dt))
                    {
                        if (dt.IsWorkDay() && halfDays.Contains(dt))
                        {
                            overTime += hours - hoursPerDay / 2;
                            weekInfo[key] = Tuple.Create(t.Item1 + hours, t.Item2 + hoursPerDay / 2);
                        }
                        else
                        {
                            overTime += hours;
                            weekInfo[key] = Tuple.Create(t.Item1 + hours, t.Item2);
                        }
                    }
                    else
                    {
                        overTime += hours - hoursPerDay;
                        weekInfo[key] = Tuple.Create(t.Item1 + hours, t.Item2 + hoursPerDay);
                    }
                    dt = dt.AddDays(1.0);
                }
                textBlockResult.Text = string.Format(Properties.Resources.TEXT_TOTAL_OVERTIME_0, DurationValueConverter.Convert(overTime));
                foreach (var elem in weekInfo)
                {
                    var weekovertime = new WeekOvertime(
                        elem.Key.Item1,
                        elem.Key.Item2,
                        elem.Value.Item1,
                        elem.Value.Item2);
                    weekOvertimes.Add(weekovertime);
                }
            }
            catch (Exception ex)
            {
                Cursor = oldcursor;
                HandleError(ex);
            }
            finally
            {
                Cursor = oldcursor;
            }
        }

        private void HandleError(Exception ex)
        {
            MessageBox.Show(
                this,
                string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message),
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
