using HabitTracker.Models;
using HabitTracker.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HabitTracker.Views
{
    public partial class HabitDetailsPage : Page
    {
        private Habit _currentHabit;

        public HabitDetailsPage(Habit habit)
        {
            InitializeComponent();
            _currentHabit = habit;
            this.Loaded += (s, e) => LoadHabitData();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void LoadHabitData()
        {
            NameTextBox.Text = _currentHabit.Name;
            TypeComboBox.SelectedIndex = _currentHabit.Type == HabitType.Bad ? 0 : 1;
            RefreshLogs();
        }

        private void RefreshLogs()
        {
            var logs = DatabaseService.GetLogsForHabit(_currentHabit.Id);
            LogsList.ItemsSource = logs;

            var stats = StatisticsEngine.CalculateStats(logs);
            string statsText = $"Leghosszabb idő:\n{stats.LongestStreak.Days} nap, {stats.LongestStreak.Hours} óra\n\n" +
                               $"Átlagos idő:\n{stats.AverageStreak.Days} nap, {stats.AverageStreak.Hours} óra\n\n";

            if (stats.PredictedNextEvent.HasValue)
            {
                statsText += $"Várható következő holtpont:\n{stats.PredictedNextEvent.Value:yyyy.MM.dd HH:mm}\n\n";
            }
            statsText += $"Tipp:\n{stats.AiSuggestion}";
            StatsTextBlock.Text = statsText;
        }

        private void SaveHabit_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                var selectedType = TypeComboBox.SelectedIndex == 0 ? HabitType.Bad : HabitType.Good;
                DatabaseService.UpdateHabit(_currentHabit.Id, NameTextBox.Text, selectedType);
                _currentHabit.Name = NameTextBox.Text;
                _currentHabit.Type = selectedType;
                MessageBox.Show("Módosítások sikeresen mentve.", "Mentés", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

      

        private void DeleteLog_Click(object sender, RoutedEventArgs e)
        {
            if (LogsList.SelectedItem is HabitLog selectedLog)
            {
                DatabaseService.DeleteLog(selectedLog.Id);
                RefreshLogs();
            }
        }
        private void AddLog_Click(object sender, RoutedEventArgs e)
        {
            string note = NewLogNoteTextBox.Text;
            DatabaseService.RecordEvent(_currentHabit.Id, note, false);
            NewLogNoteTextBox.Clear();
            RefreshLogs();
        }

        private void AddCheatDay_Click(object sender, RoutedEventArgs e)
        {
            string note = string.IsNullOrWhiteSpace(NewLogNoteTextBox.Text) ? "Tervezett Cheat Day" : $"[CHEAT] {NewLogNoteTextBox.Text}";
            DatabaseService.RecordEvent(_currentHabit.Id, note, true);
            NewLogNoteTextBox.Clear();
            RefreshLogs();
        }
    }
}