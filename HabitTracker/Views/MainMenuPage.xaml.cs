using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.Views
{
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
            this.Loaded += (s, e) => GenerateAiSummary();
        }

        private void GenerateAiSummary()
        {
            var badHabits = DatabaseService.GetHabits(HabitType.Bad);
            var goodHabits = DatabaseService.GetHabits(HabitType.Good);
            string summary = "";

            
            int missingGoodHabits = 0;
            foreach (var habit in goodHabits)
            {
                var logs = DatabaseService.GetLogsForHabit(habit.Id);
                
                if (logs.Count == 0 || logs.First().Timestamp.Date < DateTime.Today)
                {
                    missingGoodHabits++;
                }
            }

            if (missingGoodHabits > 0)
            {
                summary += $"💡 Ma még {missingGoodHabits} jó szokás vár teljesítésre a sorozataid fenntartásához.\n\n";
            }

            bool dangerZone = false;
            foreach (var habit in badHabits)
            {
                var logs = DatabaseService.GetLogsForHabit(habit.Id);
                var stats = StatisticsEngine.CalculateStats(logs);

                if (stats.PredictedNextEvent.HasValue)
                {
                    var timeUntilPredicted = stats.PredictedNextEvent.Value - DateTime.Now;

                    
                    if (timeUntilPredicted.TotalHours > 0 && timeUntilPredicted.TotalHours <= 24)
                    {
                        summary += $"⚠️ Figyelem: A statisztika szerint a(z) '{habit.Name}' szokásnál az elkövetkező 24 órában visszaesés várható! Legyél óvatos vagy tervezz egy pihenőt!\n\n";
                        dangerZone = true;
                    }
                }
            }

            
            if (string.IsNullOrEmpty(summary))
            {
                summary = "✅ Minden rendben van! A mai jó szokásaidat már teljesítetted, és egyetlen rossz szokásnál sincs statisztikai holtpont a láthatáron.";
            }
            if (AppSettings.ShowMotivation)
            {
                string[] quotes = {
                    "\"A motiváció elindít, a megszokás mozgásban tart.\"",
                    "\"Minden nagy utazás egy kis lépéssel kezdődik.\"",
                    "\"Ne a tökéletességre törekedj, hanem a folyamatos fejlődésre!\"",
                    "\"A mai nehézség a holnapi erő.\""
                };
                string randomQuote = quotes[new Random().Next(quotes.Length)];
                summary += $"\n\n💡 Napi tipp: {randomQuote}";
            }
            AiSummaryText.Text = summary.Trim();
            AiDashboardPanel.Visibility = Visibility.Visible;
        }

        private void BadHabits_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HabitListPage(HabitType.Bad));
        }

        private void GoodHabits_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HabitListPage(HabitType.Good));
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SettingsPage());
        }
    }
}