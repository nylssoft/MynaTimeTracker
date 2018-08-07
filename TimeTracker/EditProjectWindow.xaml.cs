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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TimeTracker
{
    public partial class EditProjectWindow : Window
    {
        public string ProjectName { get; private set; }

        private ObservableCollection<Project> projects;

        public EditProjectWindow(Window owner, string title, Project project, ObservableCollection<Project> projects)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.projects = projects;
            InitializeComponent();
            textBoxProject.Text = project.Name;
            textBoxProject.Focus();
            buttonOK.IsEnabled = false;
        }

        private void UpdateControls()
        {
            bool exists = false;
            string txt = textBoxProject.Text.Trim();
            foreach (var prj in projects)
            {
                if (string.Equals(prj.Name, txt))
                {
                    exists = true;
                }
            }
            bool enabled = !string.IsNullOrEmpty(txt) && !exists;
            buttonOK.IsEnabled = enabled;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ProjectName = textBoxProject.Text;
            DialogResult = true;
            Close();
        }

        private void TextBoxProject_TextChanged(object sender, TextChangedEventArgs e)
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
    }
}
