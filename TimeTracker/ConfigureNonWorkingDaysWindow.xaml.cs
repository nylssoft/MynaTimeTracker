using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimeTracker
{
    public partial class ConfigureNonWorkingDaysWindow : Window
    {
        private Database database;
        private ObservableCollection<NonWorkingDays> nonWorkingDays = new ObservableCollection<NonWorkingDays>();

        public ConfigureNonWorkingDaysWindow(Window owner, string title, Database database)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.database = database;
            InitializeComponent();
            listView.ItemsSource = nonWorkingDays;
        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!datePickerTo.SelectedDate.HasValue && datePickerFrom.SelectedDate.HasValue)
            {
                datePickerTo.SelectedDate = datePickerFrom.SelectedDate;
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = new NonWorkingDays(datePickerFrom.SelectedDate.Value, datePickerTo.SelectedDate.Value, textBoxName.Text);
                nonWorkingDays.Add(item);
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
