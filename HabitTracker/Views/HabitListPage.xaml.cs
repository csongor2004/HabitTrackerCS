using HabitTracker.Models;
using HabitTracker.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HabitTracker.Views
{
    public partial class HabitListPage : Page
    {
        private DispatcherTimer _timer;
        private HabitType _currentType;

        public HabitListPage(HabitType type)
        {
            InitializeComponent();
            _currentType = type;
            PageTitleText.Text = type == HabitType.Bad ? "Rossz szokások (Leszokás)" : "Jó szokások (Rászokás)";
            ActionBtn.Background = type == HabitType.Bad ? System.Windows.Media.Brushes.DarkRed : System.Windows.Media.Brushes.DarkGreen;

            this.Loaded += (s, e) => RefreshList();
            StartTimer();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) => UpdateStatusText();
        private void HabitList_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateStatusText();

        private void UpdateStatusText()
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                TimeSpan diff = DateTime.Now - selected.LastOccurrence;
                StatusText.Text = $"{selected.Name}\n\n" +
                                 $"{diff.Days} nap, {diff.Hours} óra\n" +
                                 $"{diff.Minutes} perc, {diff.Seconds} mp";
            }
            else
            {
                StatusText.Text = "Válassz szokást a listából!";
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                var result = MessageBox.Show("Biztosan rögzíted az eseményt?", "Megerősítés", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    DatabaseService.RecordEvent(selected.Id, "Gyors rögzítés");
                    RefreshList();
                }
            }
        }

        private void EditHabit_Click(object sender, RoutedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                NavigationService.Navigate(new HabitDetailsPage(selected));
            }
        }

        private void RefreshList()
        {
            HabitList.ItemsSource = DatabaseService.GetHabits(_currentType);
            UpdateStatusText();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddHabitWindow { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                DatabaseService.AddHabit(dialog.HabitName, _currentType);
                RefreshList();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                DatabaseService.DeleteHabit(selected.Id);
                RefreshList();
            }
        }
    }
}