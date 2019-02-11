/*
    Myna Time Tracker
    Copyright (C) 2018-2019 Niels Stockfleth

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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TimeTracker
{
    public partial class EditWindow : Window
    {
        public WorkTime WorkTime { get; set; }

        public EditWindow(Window owner, string title, WorkTime wt, ObservableCollection<Project> projects, bool multipleSelection, DateTime? defaultDateTime = null, Project defaultProject = null)
        {
            Owner = owner;
            Title = title;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            comboBoxProject.ItemsSource = projects;
            if (wt != null)
            {
                WorkTime = wt;
                datePicker.SelectedDate = wt.StartTime;
                datePicker.IsEnabled = false;
                textBoxStartHour.Text = wt.StartTime.Hour.ToString("00");
                textBoxStartMinute.Text = wt.StartTime.Minute.ToString("00");
                textBoxEndHour.Text = wt.EndTime.Hour.ToString("00");
                textBoxEndMinute.Text = wt.EndTime.Minute.ToString("00");
                textBoxDescription.Text = wt.Description;
                foreach (var p in projects)
                {
                    if (p.Name == wt.Project.Name)
                    {
                        comboBoxProject.SelectedItem = p;
                    }
                }
                if (multipleSelection)
                {
                    textBoxStartHour.IsEnabled = false;
                    textBoxStartMinute.IsEnabled = false;
                    textBoxEndHour.IsEnabled = false;
                    textBoxEndMinute.IsEnabled = false;
                    textBoxDescription.IsEnabled = false;
                }
            }
            else
            {
                WorkTime = new WorkTime();
                datePicker.SelectedDate = defaultDateTime;
                comboBoxProject.SelectedItem = defaultProject;
                textBoxStartHour.Text = "00";
                textBoxStartMinute.Text = "00";
                textBoxEndHour.Text = "00";
                textBoxEndMinute.Text = "00";
            }
            if (multipleSelection)
            {
                comboBoxProject.Focus();
            }
            else
            {
                textBoxStartHour.Focus();
            }
            UpdateControls();
        }

        private void UpdateControls()
        {
            bool ok = false;
            try
            {
                if (int.TryParse(textBoxStartHour.Text, out int starthour) && starthour >= 0 && starthour <= 23 &&
                    int.TryParse(textBoxStartMinute.Text, out int startminute) && startminute >= 0 && startminute <= 59 &&
                    int.TryParse(textBoxEndHour.Text, out int endhour) && endhour >= 0 && endhour <= 23 &&
                    int.TryParse(textBoxEndMinute.Text, out int endminute) && endminute >= 0 && endminute <= 59 &&
                    datePicker.SelectedDate.HasValue &&
                    comboBoxProject.SelectedItem != null)
                {
                    ok = true;
                }
            }
            catch
            {
                // ignored
            }
            buttonOK.IsEnabled = ok;
        }

        private void TextBox_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateControls();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControls();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControls();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(textBoxStartHour.Text, out int starthour) && starthour >= 0 && starthour <= 23 &&
                    int.TryParse(textBoxStartMinute.Text, out int startminute) && startminute >= 0 && startminute <= 59 &&
                    int.TryParse(textBoxEndHour.Text, out int endhour) && endhour >= 0 && endhour <= 23 &&
                    int.TryParse(textBoxEndMinute.Text, out int endminute) && endminute >= 0 && endminute <= 59 &&
                    datePicker.SelectedDate.HasValue &&
                    comboBoxProject.SelectedItem != null &&
                    ((endhour > starthour) || (endhour == starthour && endminute >= startminute)))
                {
                    var dt = new DateTime(datePicker.SelectedDate.Value.Year, datePicker.SelectedDate.Value.Month, datePicker.SelectedDate.Value.Day);
                    WorkTime.StartTime = new DateTime(dt.Year, dt.Month, dt.Day, starthour, startminute, 0);
                    WorkTime.EndTime = new DateTime(dt.Year, dt.Month, dt.Day, endhour, endminute, 0);
                    WorkTime.Project = comboBoxProject.SelectedItem as Project;
                    WorkTime.Description = textBoxDescription.Text;
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.CaretIndex = tb.Text.Length;
            }
        }
    }
}
