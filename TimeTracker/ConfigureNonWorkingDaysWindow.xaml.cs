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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TimeTracker
{
    public partial class ConfigureNonWorkingDaysWindow : Window
    {
        public bool Changed { get; private set; }

        private Database database;
        private ObservableCollection<NonWorkingDays> nonWorkingDays = new ObservableCollection<NonWorkingDays>();
        private SortDecorator sortDecorator = new SortDecorator(ListSortDirection.Ascending);
        private bool init = false;

        public ConfigureNonWorkingDaysWindow(Window owner, string title, Database database)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.database = database;
            init = true;
            InitializeComponent();
            foreach (var nwd in database.SelectAllNonWorkingDays())
            {
                nonWorkingDays.Add(nwd);
            }
            listView.ItemsSource = nonWorkingDays;
            datePickerFrom.Focus();
            UpdateControls();
            init = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            viewlist.SortDescriptions.Add(new SortDescription("StartDay", ListSortDirection.Ascending));
            viewlist.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
            sortDecorator.Click(gridViewColumHeaderStartDay);
        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            if (!datePickerTo.SelectedDate.HasValue && datePickerFrom.SelectedDate.HasValue ||
                datePickerTo.SelectedDate.HasValue && datePickerFrom.SelectedDate.HasValue &&
                datePickerTo.SelectedDate.Value.GetDayDateTime() < datePickerFrom.SelectedDate.Value.GetDayDateTime())
            {
                datePickerTo.SelectedDate = datePickerFrom.SelectedDate;
            }
        }

        private void DatePickerTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            UpdateControls();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (init) return;
            var txt = textBoxName.Text.Trim();
            if (string.IsNullOrEmpty(txt) ||
                !datePickerFrom.SelectedDate.HasValue ||
                !datePickerTo.SelectedDate.HasValue ||
                datePickerTo.SelectedDate.Value.GetDayDateTime() < datePickerFrom.SelectedDate.Value.GetDayDateTime())
            {
                return;
            }
            if (!int.TryParse(textBoxHours.Text, out int hours) ||
                hours != 4 && hours != 8)
            {
                return;
            }
            try
            {
                var nwd = new NonWorkingDays
                {
                    StartDay = datePickerFrom.SelectedDate.Value.GetDayDateTime(),
                    EndDay = datePickerTo.SelectedDate.Value.GetDayDateTime(),
                    Name = textBoxName.Text,
                    Hours = hours
                };
                database.InsertNonWorkDays(nwd);
                nonWorkingDays.Add(nwd);
                SelectNonWorkingDays(nwd);
                textBoxName.Text = "";
                datePickerFrom.SelectedDate = null;
                datePickerTo.SelectedDate = null;
                datePickerFrom.Focus();
                Changed = true;
                UpdateControls();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (init) return;
            if (listView.SelectedItems.Count != 1) return;
            NonWorkingDays nwd = listView.SelectedItem as NonWorkingDays;
            if (nwd == null) return;
            var w = new EditNonWorkingDaysWindow(this, Properties.Resources.TITLE_EDIT_FREEDAYS, nwd);
            if (w.ShowDialog() == true)
            {
                try
                {
                    w.NonWorkingDays.Id = nwd.Id;
                    database.UpdateNonWorkingDays(w.NonWorkingDays);
                    nwd.StartDay = w.NonWorkingDays.StartDay;
                    nwd.EndDay = w.NonWorkingDays.EndDay;
                    nwd.Name = w.NonWorkingDays.Name;
                    nwd.Hours = w.NonWorkingDays.Hours;
                    CollectionViewSource.GetDefaultView(listView.ItemsSource).Refresh();
                    SelectNonWorkingDays(nwd);
                    Changed = true;
                    UpdateControls();
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            }
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (init) return;
            if (listView.SelectedItems.Count > 0)
            {
                int idx = listView.SelectedIndex;
                var tobedeleted = new List<NonWorkingDays>();
                foreach (NonWorkingDays nwd in listView.SelectedItems)
                {
                    tobedeleted.Add(nwd);
                }
                try
                {
                    foreach (var nwd in tobedeleted)
                    {
                        database.DeleteNonWorkingDays(nwd);
                        Changed = true;
                        nonWorkingDays.Remove(nwd);
                    }
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
                idx = Math.Min(idx, listView.Items.Count - 1);
                if (idx >= 0)
                {
                    listView.SelectedIndex = idx;
                    listView.FocusItem(idx);
                }
                UpdateControls();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (init) return;
            Close();
        }

        private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (init) return;
            UpdateControls();
        }

        private void TextBoxHours_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (init) return;
            UpdateControls();
        }

        private void TextBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (init) return;
            if (e.Key == Key.Return)
            {
                ButtonAdd_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            UpdateControls();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (init) return;
            ButtonEdit_Click(sender, null);
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (init) return;
            if (e.Key == Key.Delete)
            {
                ButtonRemove_Click(sender, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                ButtonEdit_Click(sender, null);
                e.Handled = true;
            }
        }

        private void ListView_ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            if (init) return;
            var column = (sender as GridViewColumnHeader);
            if (column == null || column.Tag == null) return;
            sortDecorator.Click(column);
            string sortBy = column.Tag.ToString();
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            viewlist.SortDescriptions.Clear();
            viewlist.SortDescriptions.Add(new SortDescription(sortBy, sortDecorator.Direction));
            viewlist.SortDescriptions.Add(new SortDescription("Id", sortDecorator.Direction));
        }


        private void UpdateControls()
        {
            var selprj = listView.SelectedItem as NonWorkingDays;
            var txt = textBoxName.Text.Trim();
            buttonAdd.IsEnabled = txt.Length > 0;
            if (!datePickerFrom.SelectedDate.HasValue ||
                !datePickerTo.SelectedDate.HasValue ||
                datePickerTo.SelectedDate.Value.GetDayDateTime() < datePickerFrom.SelectedDate.Value.GetDayDateTime())
            {
                buttonAdd.IsEnabled = false;
            }
            if (!int.TryParse(textBoxHours.Text, out int hours) ||
                hours != 4 && hours != 8)
            {
                buttonAdd.IsEnabled = false;
            }
            buttonRemove.IsEnabled = selprj != null;
            buttonEdit.IsEnabled = listView.SelectedItems.Count == 1;
        }

        private void SelectNonWorkingDays(NonWorkingDays nwd)
        {
            for (int idx = 0; idx < listView.Items.Count; idx++)
            {
                if (listView.Items[idx] is NonWorkingDays n)
                {
                    if (n.Id == nwd.Id)
                    {
                        listView.ScrollIntoView(n);
                        listView.SelectedIndex = idx;
                        listView.FocusItem(idx);
                        break;
                    }
                }
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
