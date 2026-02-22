using HabitTracker.Models;
using HabitTracker.Services;
using System.Windows;

namespace HabitTracker.Views
{
    public partial class HabitDetailsWindow : Window
    {
        private Habit _currentHabit;

        public HabitDetailsWindow(Habit habit)
        {
            InitializeComponent();
            _currentHabit = habit;
            LoadHabitData();
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
                DialogResult = true;
            }
        }

        private void AddLog_Click(object sender, RoutedEventArgs e)
        {
            string note = NewLogNoteTextBox.Text;
            DatabaseService.RecordEvent(_currentHabit.Id, note);
            NewLogNoteTextBox.Clear();
            RefreshLogs();
        }

        private void DeleteLog_Click(object sender, RoutedEventArgs e)
        {
            if (LogsList.SelectedItem is HabitLog selectedLog)
            {
                DatabaseService.DeleteLog(selectedLog.Id);
                RefreshLogs();
            }
        }
    }
}