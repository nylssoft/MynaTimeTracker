using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    public partial class OvertimeWindow : Window
    {
        private Database database;
        private ISet<DateTime> freeDays = new HashSet<DateTime>();
        private ObservableCollection<WeekOvertime> weekOvertimes = new ObservableCollection<WeekOvertime>();
        private DateTime from;
        private DateTime to;

        public OvertimeWindow(Window owner, string title, Database database)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.database = database;
            InitializeComponent();
            to = DateTime.Today;
            from = to;
            var f = database.GetFirstStartTime();
            if (f.HasValue)
            {
                from = f.Value.GetDayDateTime();
            }
            listView.ItemsSource = weekOvertimes;
            textBlockInfo.Text = $"Überstunden von {from.ToLongDateString()} bis {to.ToLongDateString()}";
            Calculate();
        }

        private void ButtonNonWorkingDays_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var w = new ConfigureNonWorkingDaysWindow(this, "Arbeitsfreie Tage", database);
                w.ShowDialog();
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

        private void Calculate()
        {
            try
            {
                var dfi = DateTimeFormatInfo.CurrentInfo;
                var cal = dfi.Calendar;

                freeDays.Clear();
                freeDays.Add(new DateTime(2018, 08, 15));
                freeDays.Add(new DateTime(2018, 10, 03));
                freeDays.Add(new DateTime(2018, 11, 01));
                freeDays.Add(new DateTime(2018, 12, 25));
                freeDays.Add(new DateTime(2018, 12, 26));

                IDictionary<Tuple<int,int>, Tuple<double, double>> weekInfo = new Dictionary<Tuple<int, int>, Tuple<double, double>>();

                double overTime = 0.0;
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
                        overTime += hours;
                        weekInfo[key] = Tuple.Create(t.Item1 + hours, t.Item2);
                    }
                    else
                    {
                        overTime += hours - 8.0;
                        weekInfo[key] = Tuple.Create(t.Item1 + hours, t.Item2 + 8.0);
                    }
                    dt = dt.AddDays(1.0);
                }
                textBlockResult.Text = $"Gesamte Überstunden: {DurationValueConverter.Convert(overTime)}";
                weekOvertimes.Clear();
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
    }
}
