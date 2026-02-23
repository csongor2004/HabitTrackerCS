using HabitTracker.Models;
using HabitTracker.Services;
using System;
using System.Linq;
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
            ActionBtn.Content = type == HabitType.Bad ? "ESEMÉNY (BOTLÁS) RÖGZÍTÉSE" : "SIKERES TELJESÍTÉS";

            this.Loaded += (s, e) => RefreshList();
            StartTimer();
        }

        private void OpenAnalysis_Click(object sender, RoutedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected) NavigationService.Navigate(new HabitAnalysisPage(selected));
            else MessageBox.Show("Előbb válassz ki egy szokást!");
        }

        private void Back_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();

        private void StartTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) => UpdateStatusText();
        private void HabitList_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateStatusText();

        private void UpdateStatusText()
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                RightPanel.Visibility = Visibility.Visible;
                NoSelectionText.Visibility = Visibility.Collapsed;
                SelectedHabitNameText.Text = selected.Name;

                if (_currentType == HabitType.Bad)
                {
                    TimeLabelText.Text = "Utolsó alkalom óta eltelt idő:";
                    TimeSpan diff = DateTime.Now - selected.LastOccurrence;
                    StatusText.Text = $"{diff.Days} nap, {diff.Hours} óra\n{diff.Minutes} perc, {diff.Seconds} mp";
                }
                else
                {
                    TimeLabelText.Text = "Jelenlegi folyamatos sorozat (Streak):";
                    var logs = DatabaseService.GetLogsForHabit(selected.Id);
                    var stats = StatisticsEngine.CalculateGoodHabitStats(logs);
                    StatusText.Text = $"{stats.CurrentDailyStreak} nap\n(Összesen: {stats.TotalCompletions} alkalom)";
                }
            }
            else
            {
                RightPanel.Visibility = Visibility.Hidden;
                NoSelectionText.Visibility = Visibility.Visible;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                string note = string.IsNullOrWhiteSpace(QuickNoteTextBox.Text) ? "Gyors rögzítés" : QuickNoteTextBox.Text;
                DatabaseService.RecordEvent(selected.Id, note, false);
                QuickNoteTextBox.Clear();
                RefreshList();
            }
        }

        private void EditHabit_Click(object sender, RoutedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected) NavigationService.Navigate(new HabitDetailsPage(selected));
            else MessageBox.Show("Előbb válassz ki egy szokást!");
        }

        private void RefreshList()
        {
            int? selectedId = (HabitList.SelectedItem as Habit)?.Id;
            var habits = DatabaseService.GetHabits(_currentType);
            HabitList.ItemsSource = habits;
            if (selectedId.HasValue) HabitList.SelectedItem = habits.FirstOrDefault(h => h.Id == selectedId.Value);
            UpdateStatusText();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddHabitWindow { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true) { DatabaseService.AddHabit(dialog.HabitName, _currentType); RefreshList(); }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                if (MessageBox.Show($"Törlöd a(z) '{selected.Name}' szokást?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                { DatabaseService.DeleteHabit(selected.Id); RefreshList(); }
            }
        }
    }
}