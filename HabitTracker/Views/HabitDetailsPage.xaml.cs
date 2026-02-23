using HabitTracker.Models;
using HabitTracker.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MessageBox = System.Windows.MessageBox;
namespace HabitTracker.Views
{
    public partial class HabitDetailsPage : Page
    {
        private Habit _currentHabit;

        public HabitDetailsPage(Habit habit)
        {
            InitializeComponent();
            _currentHabit = habit;
            this.Loaded += (s, e) => InitializePage();
        }

        private void InitializePage()
        {
            var habitsOfSameType = DatabaseService.GetHabits(_currentHabit.Type);
            HabitSelector.ItemsSource = habitsOfSameType;
            HabitSelector.SelectedItem = habitsOfSameType.FirstOrDefault(h => h.Id == _currentHabit.Id);
        }

        private void HabitSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HabitSelector.SelectedItem is Habit selected)
            {
                _currentHabit = selected;
                NameTextBox.Text = _currentHabit.Name;
                RefreshLogs();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void RefreshLogs()
        {
            var logs = DatabaseService.GetLogsForHabit(_currentHabit.Id);
            LogsList.ItemsSource = logs;

            if (_currentHabit.Type == HabitType.Bad)
            {
                var stats = StatisticsEngine.CalculateStats(logs);
                string statsText = $"Jelenlegi tiszta idő:\n{stats.CurrentStreak.Days} nap, {stats.CurrentStreak.Hours} óra\n\n" +
                                   $"Leghosszabb tiszta idő:\n{stats.LongestStreak.Days} nap, {stats.LongestStreak.Hours} óra\n\n" +
                                   $"Összes regisztrált alkalom: {stats.TotalEvents}\n" +
                                   $"Felhasznált Cheat Day-ek: {stats.CheatDays}\n\n";

                if (stats.PredictedNextEvent.HasValue) statsText += $"Várható következő statisztikai holtpont:\n{stats.PredictedNextEvent.Value:yyyy.MM.dd HH:mm}\n\n";
                statsText += $"AI Elemzés:\n{stats.AiSuggestion}";
                StatsTextBlock.Text = statsText;

                // Kitűzők betöltése
                BadgesList.ItemsSource = stats.EarnedBadges;
            }
            else
            {
                var stats = StatisticsEngine.CalculateGoodHabitStats(logs);
                string statsText = $"Jelenlegi napi sorozat:\n{stats.CurrentDailyStreak} nap\n\n" +
                                   $"Leghosszabb napi sorozat:\n{stats.LongestDailyStreak} nap\n\n" +
                                   $"Összes sikeres teljesítés:\n{stats.TotalCompletions} alkalom\n\n" +
                                   $"Felhasznált Pihenőnapok: {stats.CheatDays}\n\n" +
                                   $"AI Elemzés:\n{stats.AiSuggestion}";
                StatsTextBlock.Text = statsText;

                
                BadgesList.ItemsSource = stats.EarnedBadges;
            }
        }

        
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = $"{_currentHabit.Name}_Adatok";
            dialog.DefaultExt = ".csv";
            dialog.Filter = "Excel CSV fájlok (.csv)|*.csv";

            if (dialog.ShowDialog() == true)
            {
                var logs = DatabaseService.GetLogsForHabit(_currentHabit.Id);
                var sb = new System.Text.StringBuilder();

                
                sb.AppendLine("Datum;Idopont;Tipus;Megjegyzes");

                foreach (var log in logs.OrderBy(l => l.Timestamp))
                {
                    string type = log.IsCheatDay ? "Cheat Day" : "Normal";
                    
                    sb.AppendLine($"{log.Timestamp:yyyy.MM.dd};{log.Timestamp:HH:mm};{type};{log.Note}");
                }

                
                System.IO.File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                MessageBox.Show("Az adatok sikeresen exportálva lettek CSV formátumba!", "Sikeres Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveHabit_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                DatabaseService.UpdateHabit(_currentHabit.Id, NameTextBox.Text, _currentHabit.Type);
                _currentHabit.Name = NameTextBox.Text;
                InitializePage(); 
            }
        }
        private void OpenAnalysis_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HabitAnalysisPage(_currentHabit));
        }
        private void AddLog_Click(object sender, RoutedEventArgs e)
        {
            var todayLogs = DatabaseService.GetLogsForHabit(_currentHabit.Id).Where(l => l.Timestamp.Date == DateTime.Today);
            if (todayLogs.Any(l => l.IsCheatDay))
            {
                MessageBox.Show("A mai napra már rögzítettél egy Cheat Day-t, így normál alkalom nem adható hozzá!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string note = string.IsNullOrWhiteSpace(NewLogNoteTextBox.Text) ? "Normál rögzítés" : NewLogNoteTextBox.Text;
            DatabaseService.RecordEvent(_currentHabit.Id, note, false);
            NewLogNoteTextBox.Clear();
            RefreshLogs();
        }

        private void AddCheatDay_Click(object sender, RoutedEventArgs e)
        {
            string note = string.IsNullOrWhiteSpace(NewLogNoteTextBox.Text) ? "[CHEAT DAY] Tervezett pihenő" : $"[CHEAT DAY] {NewLogNoteTextBox.Text}";
            DatabaseService.RecordEvent(_currentHabit.Id, note, true);
            NewLogNoteTextBox.Clear();
            RefreshLogs();
        }

        private void DeleteLog_Click(object sender, RoutedEventArgs e)
        {
            if (LogsList.SelectedItem is HabitLog selectedLog)
            {
                // Átadjuk a szokás ID-ját is a visszatekeréshez
                DatabaseService.DeleteLog(selectedLog.Id, _currentHabit.Id);

                
                var updatedHabit = DatabaseService.GetHabits(_currentHabit.Type).FirstOrDefault(h => h.Id == _currentHabit.Id);
                if (updatedHabit != null)
                {
                    _currentHabit.LastOccurrence = updatedHabit.LastOccurrence;
                }

                RefreshLogs();
            }
        }
    }
}