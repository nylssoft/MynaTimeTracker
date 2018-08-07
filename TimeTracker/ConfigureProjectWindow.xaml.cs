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
    public partial class ConfigureProjectWindow : Window
    {
        public bool Changed { get; private set; }

        private ObservableCollection<Project> projects = new ObservableCollection<Project>();
        private Database database;
        private ISet<Project> projectsInUse;

        public ConfigureProjectWindow(Window owner, string title, Database database)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.database = database;
            InitializeComponent();
            listBoxProject.ItemsSource = projects;
            foreach (var p in database.SelectAllProjects())
            {
                projects.Add(p);
            }
            projectsInUse = new HashSet<Project>(database.SelectProjectInUse());
            textBoxProject.Focus();
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listBoxProject.ItemsSource);
            viewlist.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            UpdateControls();
        }

        private void UpdateControls()
        {
            var selprj = listBoxProject.SelectedItem as Project;
            var txt = textBoxProject.Text.Trim();
            bool exists = false;
            foreach (var prj in projects)
            {
                if (string.Equals(prj.Name, txt))
                {
                    exists = true;
                    break;
                }
            }            
            buttonAddProject.IsEnabled = txt.Length > 0 && !exists;
            buttonRemoveProject.IsEnabled = selprj != null && !projectsInUse.Contains(selprj);
            buttonEditProject.IsEnabled = listBoxProject.SelectedItems.Count == 1;
        }

        private void ListBoxProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControls();
        }

        private void TextBoxProject_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateControls();
        }

        private void ButtonAddProject_Click(object sender, RoutedEventArgs e)
        {
            var txt = textBoxProject.Text.Trim();
            if (string.IsNullOrEmpty(txt)) return;
            foreach (var prj in projects)
            {
                if (string.Equals(prj.Name, txt))
                {
                    return;
                }
            }
            try
            {
                var p = database.InsertProject(txt);
                projects.Add(p);
                textBoxProject.Text = "";
                Changed = true;
                UpdateControls();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void SelectProject(Project prj)
        {
            for (int idx = 0; idx < listBoxProject.Items.Count; idx++)
            {
                if (listBoxProject.Items[idx] is Project p)
                {
                    if (p.Id == prj.Id)
                    {
                        listBoxProject.ScrollIntoView(p);
                        listBoxProject.SelectedIndex = idx;
                        listBoxProject.FocusItem(idx);
                        break;
                    }
                }
            }
        }

        private void ButtonEditProject_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxProject.SelectedItems.Count != 1) return;
            Project prj = listBoxProject.SelectedItem as Project;
            if (prj == null) return;
            var w = new EditProjectWindow(this, Properties.Resources.TITLE_EDIT_PROJECT, prj, projects);
            if (w.ShowDialog() == true)
            {
                try
                {
                    database.RenameProject(prj, w.ProjectName);
                    SelectProject(prj);
                    Changed = true;
                    UpdateControls();
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            }
        }

        private void ButtonRemoveProject_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxProject.SelectedItems.Count > 0)
            {
                int idx = listBoxProject.SelectedIndex;
                List<Project> tobedeleted = new List<Project>();
                foreach (Project p in listBoxProject.SelectedItems)
                {
                    if (!projectsInUse.Contains(p))
                    {
                        tobedeleted.Add(p);
                    }
                }
                try
                {
                    foreach (var p in tobedeleted)
                    {
                        database.DeleteProject(p);
                        Changed = true;
                        projects.Remove(p);
                    }
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
                idx = Math.Min(idx, listBoxProject.Items.Count - 1);
                if (idx >= 0)
                {
                    listBoxProject.SelectedIndex = idx;
                    listBoxProject.FocusItem(idx);
                }
                UpdateControls();
            }
        }

        private void TextBoxProject_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ButtonAddProject_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ListBoxProject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ButtonEditProject_Click(sender, null);
        }

        private void ListBoxProject_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ButtonRemoveProject_Click(sender, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                ButtonEditProject_Click(sender, null);
                e.Handled = true;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
