using HabitTracker.Models;
using HabitTracker.Services;
using System;
using System.Windows;
using System.Windows.Threading;

namespace HabitTracker.Views
{
    public partial class HabitListWindow : Window
    {
        private DispatcherTimer _timer;
        private HabitType _currentType;

        public HabitListWindow(HabitType type)
        {
            InitializeComponent();
            _currentType = type;
            Title = type == HabitType.Bad ? "Rossz szokások" : "Jó szokások";
            ActionBtn.Background = type == HabitType.Bad ? System.Windows.Media.Brushes.DarkRed : System.Windows.Media.Brushes.DarkGreen;

            RefreshList();
            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateStatusText();
        }

        private void HabitList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                TimeSpan diff = DateTime.Now - selected.LastOccurrence;
                StatusText.Text = $"{selected.Name}\n" +
                                 $"{diff.Days} nap, {diff.Hours} óra, " +
                                 $"{diff.Minutes} perc, {diff.Seconds} másodperc";
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
                var detailsWindow = new HabitDetailsWindow(selected) { Owner = this };
                if (detailsWindow.ShowDialog() == true)
                {
                    RefreshList();
                }
            }
        }
        private void RefreshList()
        {
            HabitList.ItemsSource = DatabaseService.GetHabits(_currentType);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddHabitWindow { Owner = this };
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