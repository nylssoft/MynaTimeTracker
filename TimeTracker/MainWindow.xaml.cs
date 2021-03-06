﻿/*
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace TimeTracker
{
    public partial class MainWindow : Window
    {
        private bool init = false;
        private ObservableCollection<WorkTime> workTimes = new ObservableCollection<WorkTime>();    
        private ObservableCollection<Project> projects = new ObservableCollection<Project>();
        private Database database = new Database();
        private DateTime? currentStartTime;
        private DateTime? selectedDate;
        private SortDecorator sortDecorator = new SortDecorator(ListSortDirection.Ascending);
        private OvertimeWindow overtimeWindow = null;

        public MainWindow()
        {
            InitializeComponent();
            listView.ItemsSource = workTimes;
            comboBoxProject.ItemsSource = projects;
        }

        #region Callback methods

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                HandleError(ex);
                Close();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock ||
                e.Reason == Microsoft.Win32.SessionSwitchReason.RemoteDisconnect)
            {
                Stop();
                CommandManager.InvalidateRequerySuggested();
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock ||
                e.Reason == Microsoft.Win32.SessionSwitchReason.RemoteConnect)
            {
                Start();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (overtimeWindow != null)
                {
                    if (overtimeWindow.ConfigureNonWorkingDaysWindow != null && !overtimeWindow.ConfigureNonWorkingDaysWindow.IsClosed)
                    {
                        overtimeWindow.ConfigureNonWorkingDaysWindow.Close();
                        overtimeWindow.ConfigureNonWorkingDaysWindow = null;
                    }
                    if (!overtimeWindow.IsClosed)
                    {
                        overtimeWindow.Close();
                        overtimeWindow = null;
                    }
                }
                Stop();
                if (WindowState == WindowState.Normal)
                {
                    Properties.Settings.Default.Left = Left;
                    Properties.Settings.Default.Top = Top;
                    Properties.Settings.Default.Width = Width;
                    Properties.Settings.Default.Height = Height;
                }
                if (comboBoxProject.SelectedItem is Project lastUsedProject)
                {
                    Properties.Settings.Default.LastUsedProject = lastUsedProject.Name;
                }
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            try
            {
                if (selectedDate != datePicker.SelectedDate)
                {
                    InitWorkTimes(datePicker.SelectedDate);
                    selectedDate = datePicker.SelectedDate;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void InitWorkTimes(DateTime? dt)
        {
            workTimes.Clear();
            if (dt.HasValue)
            {
                foreach (var wt in database.SelectWorkTimes(dt.Value))
                {
                    workTimes.Add(wt);
                }
            }
            UpdateTotalHours();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(listView);
            var lvitem = listView.GetItemAt(mousePosition);
            if (lvitem != null)
            {
                UpdateWorkTime(lvitem.Content as WorkTime, false);
            }
        }

        private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Command is RoutedUICommand r)) return;
            int selcount = (listView != null ? listView.SelectedItems.Count : 0);
            switch (r.Name)
            {
                case "Next":
                case "Previous":
                    e.CanExecute = datePicker.SelectedDate.HasValue;
                    break;
                case "Start":
                    e.CanExecute = !currentStartTime.HasValue;
                    break;
                case "Stop":
                    e.CanExecute = currentStartTime.HasValue;
                    break;
                case "Add":
                case "About":
                case "Exit":
                case "ConfigureProjects":
                case "CalculateOvertime":
                    e.CanExecute = true;
                    break;
                case "Edit":
                    e.CanExecute = selcount >= 1;
                    break;
                case "Remove":
                    e.CanExecute = selcount >= 1;
                    break;
                case "Merge":
                    e.CanExecute = selcount > 1;
                    break;
                default:
                    break;
            }
        }

        private void Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Command is RoutedUICommand r)) return;
            switch (r.Name)
            {
                case "Exit":
                    Close();
                    break;
                case "About":
                    About();
                    break;
                case "Next":
                    Next();
                    break;
                case "Previous":
                    Previous();
                    break;
                case "Start":
                    Start();
                    break;
                case "Stop":
                    Stop();
                    break;
                case "Edit":
                    UpdateWorkTime();
                    break;
                case "Remove":
                    DeleteWorkTime();
                    break;
                case "Add":
                    InsertWorkTime();
                    break;
                case "Merge":
                    MergeWorkTimes();
                    break;
                case "ConfigureProjects":
                    ConfigureProjects();
                    break;
                case "CalculateOvertime":
                    CalculateOvertime();
                    break;
                default:
                    break;
            }
        }

        private void ListView_ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            var column = (sender as GridViewColumnHeader);
            if (column == null || column.Tag == null) return;
            sortDecorator.Click(column);
            string sortBy = column.Tag.ToString();
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            viewlist.SortDescriptions.Clear();
            viewlist.SortDescriptions.Add(new SortDescription(sortBy, sortDecorator.Direction));
            viewlist.SortDescriptions.Add(new SortDescription("Id", sortDecorator.Direction));
        }
        #endregion

        #region Private methods

        private void Init()
        {
            init = true;
            Title = Properties.Resources.TITLE_TIMETRACKER;
            this.RestorePosition(
                Properties.Settings.Default.Left,
                Properties.Settings.Default.Top,
                Properties.Settings.Default.Width,
                Properties.Settings.Default.Height);
            string filename = Properties.Settings.Default.DatabaseFile.ReplaceSpecialFolder();
            var di = new FileInfo(filename).Directory;
            if (!di.Exists)
            {
                Directory.CreateDirectory(di.FullName);
            }
            database.Open(filename);
            currentStartTime = DateTime.Now;
            datePicker.SelectedDate = currentStartTime;
            InitWorkTimes(datePicker.SelectedDate);
            var dbprojects = database.SelectAllProjects();
            if (dbprojects.Count == 0)
            {
                dbprojects.Add(database.InsertProject(Properties.Resources.TEXT_DEFAULT_PROJECT));
            }
            Project lastUsedProject = null;
            foreach (var p in dbprojects)
            {
                projects.Add(p);
                if (string.Equals(p.Name, Properties.Settings.Default.LastUsedProject))
                {
                    lastUsedProject = p;
                }
            }
            comboBoxProject.SelectedIndex = 0;
            if (lastUsedProject != null)
            {
                comboBoxProject.SelectedItem = lastUsedProject;
            }
            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            var timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            UpdateStatus();
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            viewlist.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Ascending));
            viewlist.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
            var viewlistprojects = (CollectionView)CollectionViewSource.GetDefaultView(comboBoxProject.ItemsSource);
            viewlistprojects.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            init = false;
            sortDecorator.Click(gridViewColumHeaderStartTime);
        }

        private void UpdateStatus()
        {
            if (currentStartTime.HasValue)
            {
                var ts = DateTime.Now - currentStartTime.Value;
                textBlockStatus.Text = String.Format(Properties.Resources.TEXT_RECORD_START_0_1_2, currentStartTime.Value.ToShortDateString(), currentStartTime.Value.ToShortTimeString(), DurationValueConverter.Convert(ts));
            }
        }

        private void UpdateTotalHours()
        {
            double dur = WorkTime.CalculateTotalHours(workTimes);
            textBlockTotal.Text = string.Format(Properties.Resources.TEXT_TOTAL_0_1, datePicker.SelectedDate.Value.ToLongDateString(), DurationValueConverter.Convert(dur));
        }

        private void AddDays(int days)
        {
            try
            {
                if (datePicker.SelectedDate.HasValue)
                {
                    datePicker.SelectedDate = datePicker.SelectedDate.Value.AddDays(days);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Next()
        {
            AddDays(1);
        }

        private void Previous()
        {
            AddDays(-1);
        }

        private void Start()
        {
            try
            {
                if (currentStartTime.HasValue) return;
                currentStartTime = DateTime.Now;
                if (!datePicker.SelectedDate.HasValue ||
                    !currentStartTime.Value.IsSameDay(datePicker.SelectedDate.Value))
                {
                    datePicker.SelectedDate = currentStartTime.Value;
                }
                UpdateStatus();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Stop()
        {
            try
            {
                var project = comboBoxProject.SelectedItem as Project;
                if (!currentStartTime.HasValue || project == null)
                {
                    return;
                }
                var ts = DateTime.Now - currentStartTime.Value;
                // RemoveDisconnect and RemoteConnect events are fired twice
                // store working times only if longer than 1 minute
                if (ts.TotalMinutes >= 1.0)
                {
                    var wt = new WorkTime
                    {
                        StartTime = currentStartTime.Value,
                        EndTime = DateTime.Now,
                        Project = project,
                        Description = string.Empty
                    };
                    // @TODO: add 2 work times if start time and end time is on a different day?
                    database.InsertWorkTime(wt);
                    if (!datePicker.SelectedDate.HasValue ||
                        !wt.EndTime.IsSameDay(datePicker.SelectedDate.Value))
                    {
                        datePicker.SelectedDate = wt.EndTime;
                    }
                    else
                    {
                        workTimes.Add(wt);
                        UpdateTotalHours();
                    }
                    SelectWorkTime(wt);
                }
                currentStartTime = null;
                textBlockStatus.Text = Properties.Resources.TEXT_RECORD_STOP;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void InsertWorkTime()
        {
            try
            {
                var lastUsedProject = comboBoxProject.SelectedItem as Project;
                var wnd = new EditWindow(this, Properties.Resources.TITLE_ADD, null, projects, false, datePicker.SelectedDate, lastUsedProject);
                if (wnd.ShowDialog() == true)
                {
                    var wt = wnd.WorkTime;
                    database.InsertWorkTime(wt);
                    workTimes.Add(wt);
                    SelectWorkTime(wt);
                    UpdateTotalHours();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void SelectWorkTime(WorkTime wt)
        {
            for (int idx = 0; idx < listView.Items.Count; idx++)
            {
                if (listView.Items[idx] is WorkTime w)
                {
                    if (w.Id == wt.Id)
                    {
                        listView.ScrollIntoView(w);
                        listView.SelectedIndex = idx;
                        listView.FocusItem(idx);
                        break;
                    }
                }
            }
        }

        private void MergeWorkTimes()
        {
            try
            {
                if (listView.SelectedItems.Count < 2) return;
                WorkTime baseEntry = listView.SelectedItem as WorkTime;
                var startTime = baseEntry.StartTime;
                var endTime = baseEntry.EndTime;
                var deleteWorkTimes = new List<WorkTime>();
                foreach (WorkTime wt in listView.SelectedItems)
                {
                    if (wt.Id != baseEntry.Id)
                    {
                        if (wt.StartTime < startTime)
                        {
                            startTime = wt.StartTime;
                        }
                        if (wt.EndTime > endTime)
                        {
                            endTime = wt.EndTime;
                        }
                        deleteWorkTimes.Add(wt);
                    }
                }
                if (MessageBox.Show(
                    Properties.Resources.QUESTION_MERGE_ITEMS,
                    Title, MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    var idx = listView.SelectedIndex;
                    baseEntry.StartTime = startTime;
                    baseEntry.EndTime = endTime;
                    database.UpdateWorkTime(baseEntry);
                    foreach (var wt in deleteWorkTimes)
                    {
                        database.DeleteWorkTime(wt);
                        workTimes.Remove(wt);
                    }
                    UpdateTotalHours();
                    idx = Math.Min(idx, listView.Items.Count - 1);
                    if (idx >= 0)
                    {
                        listView.SelectedIndex = idx;
                        listView.FocusItem(idx);
                    }
                }

            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void UpdateWorkTime()
        {
            WorkTime wt = listView.SelectedItem as WorkTime;
            UpdateWorkTime(wt, listView.SelectedItems.Count > 1);
        }

        private void UpdateWorkTime(WorkTime wt, bool multipleSelection)
        {
            try
            {
                if (wt == null) return;
                var wnd = new EditWindow(this, Properties.Resources.TITLE_EDIT, wt, projects, multipleSelection);
                if (wnd.ShowDialog() == true)
                {
                    if (multipleSelection)
                    {
                        foreach (WorkTime w in listView.SelectedItems)
                        {
                            if (w.Id != wt.Id && w.Project != wt.Project)
                            {
                                w.Project = wt.Project;
                                database.UpdateWorkTime(w);
                            }
                        }
                    }
                    database.UpdateWorkTime(wt);
                    CollectionViewSource.GetDefaultView(listView.ItemsSource).Refresh();
                    UpdateTotalHours();
                    SelectWorkTime(wt);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void DeleteWorkTime()
        {
            try
            {
                if (MessageBox.Show(
                    Properties.Resources.QUESTION_DELETE_ITEMS,
                    Title, MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    var idx = listView.SelectedIndex;
                    var del = new List<WorkTime>();
                    foreach (WorkTime wt in listView.SelectedItems)
                    {
                        del.Add(wt);
                    }
                    foreach (var d in del)
                    {
                        database.DeleteWorkTime(d);
                        workTimes.Remove(d);
                    }
                    UpdateTotalHours();
                    idx = Math.Min(idx, listView.Items.Count - 1);
                    if (idx >= 0)
                    {
                        listView.SelectedIndex = idx;
                        listView.FocusItem(idx);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void About()
        {
            try
            {
                var dlg = new AboutWindow(this);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ConfigureProjects()
        {
            try
            {
                var dlg = new ConfigureProjectWindow(this, Properties.Resources.TITLE_CONFIGURE_PROJECT, database);
                dlg.ShowDialog();
                if (!dlg.Changed) return;
                Project selprj = comboBoxProject.SelectedItem as Project;
                projects.Clear();
                foreach (var p in database.SelectAllProjects())
                {
                    projects.Add(p);
                }
                if (projects.Count > 0)
                {
                    comboBoxProject.SelectedIndex = 0;
                    if (selprj != null)
                    {
                        foreach (var p in projects)
                        {
                            if (p.Id == selprj.Id)
                            {
                                comboBoxProject.SelectedItem = p;
                            }
                        }
                    }
                }
                InitWorkTimes(datePicker.SelectedDate);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void CalculateOvertime()
        {
            try
            {
                if (overtimeWindow == null || overtimeWindow.IsClosed)
                {
                    overtimeWindow = new OvertimeWindow(null, Properties.Resources.TITLE_OVERTIME, database);
                    overtimeWindow.Show();
                }
                else
                {
                    overtimeWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
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

        #endregion

    }
}
