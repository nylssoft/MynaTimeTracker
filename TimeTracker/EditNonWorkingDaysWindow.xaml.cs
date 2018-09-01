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
using System.Windows;
using System.Windows.Controls;

namespace TimeTracker
{
    public partial class EditNonWorkingDaysWindow : Window
    {
        public NonWorkingDays NonWorkingDays { get; private set; }

        public EditNonWorkingDaysWindow(Window owner, string title, NonWorkingDays nwd)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            datePickerFrom.SelectedDate = nwd.StartDay;
            datePickerTo.SelectedDate = nwd.EndDay;
            textBoxName.Text = nwd.Name;
            textBoxHours.Text = Convert.ToString(nwd.Hours);
            datePickerFrom.Focus();
            buttonOK.IsEnabled = false;
            NonWorkingDays = new NonWorkingDays { Id = nwd.Id };
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            NonWorkingDays.StartDay = datePickerFrom.SelectedDate.Value.GetDayDateTime();
            NonWorkingDays.EndDay = datePickerTo.SelectedDate.Value.GetDayDateTime();
            NonWorkingDays.Name = textBoxName.Text;
            NonWorkingDays.Hours = int.Parse(textBoxHours.Text);
            DialogResult = true;
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateControls();
        }

        
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.CaretIndex = tb.Text.Length;
            }
        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!datePickerTo.SelectedDate.HasValue && datePickerFrom.SelectedDate.HasValue ||
                datePickerTo.SelectedDate.HasValue && datePickerFrom.SelectedDate.HasValue &&
                datePickerTo.SelectedDate.Value.GetDayDateTime() < datePickerFrom.SelectedDate.Value.GetDayDateTime())
            {
                datePickerTo.SelectedDate = datePickerFrom.SelectedDate;
            }
            UpdateControls();
        }

        private void DatePickerTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            string txt = textBoxName.Text.Trim();
            bool enabled = !string.IsNullOrEmpty(txt);
            if (!datePickerFrom.SelectedDate.HasValue || !datePickerFrom.SelectedDate.HasValue ||
                datePickerTo.SelectedDate.Value.GetDayDateTime() < datePickerFrom.SelectedDate.Value.GetDayDateTime())
            {
                enabled = false;
            }
            if (!int.TryParse(textBoxHours.Text, out int hours) ||
                hours != 4 && hours != 8)
            {
                enabled = false;
            }
            buttonOK.IsEnabled = enabled;
        }

    }
}
