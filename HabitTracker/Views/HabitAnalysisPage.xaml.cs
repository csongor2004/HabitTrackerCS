using HabitTracker.Models;
using HabitTracker.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HabitTracker.Views
{
    public class ChartBar
    {
        public double PixelHeight { get; set; }
        public string DisplayHours { get; set; }
        public string EndDate { get; set; }
        public string ToolTipText { get; set; }
    }

    public partial class HabitAnalysisPage : Page
    {
        private Habit _habit;

        public HabitAnalysisPage(Habit habit)
        {
            InitializeComponent();
            _habit = habit;
            TitleText.Text = $"Elemzés: {_habit.Name}";
            this.Loaded += (s, e) => LoadAnalysis();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void LoadAnalysis()
        {
            var logs = DatabaseService.GetLogsForHabit(_habit.Id);
            var stats = StatisticsEngine.CalculateStats(logs);

            GenerateChart(stats);
            GenerateDeepAnalysis(stats);
        }

        private void GenerateChart(HabitStats stats)
        {
            if (stats.History.Count == 0)
                return;

            double maxHours = stats.History.Max(h => h.Hours);
            double maxPixelHeight = 300.0; // Az oszlopok maximális magassága

            var chartData = new List<ChartBar>();
            foreach (var item in stats.History)
            {
                double height = maxHours > 0 ? (item.Hours / maxHours) * maxPixelHeight : 0;

                chartData.Add(new ChartBar
                {
                    PixelHeight = height < 5 ? 5 : height, // Minimum 5 pixel, hogy látszódjon
                    DisplayHours = $"{item.Hours:F1}ó",
                    EndDate = item.EndDate,
                    ToolTipText = $"{item.Hours:F1} órát bírtál ki ekkorra: {item.EndDate}"
                });
            }

            ChartControl.ItemsSource = chartData;
        }

        private void GenerateDeepAnalysis(HabitStats stats)
        {
            string analysis = "";

            if (stats.History.Count < 3)
            {
                analysis = "Még nincs elegendő adat egy megbízható trend felállításához. Rögzíts még legalább pár alkalmat!";
            }
            else
            {
                var firstHalf = stats.History.Take(stats.History.Count / 2).Average(h => h.Hours);
                var secondHalf = stats.History.Skip(stats.History.Count / 2).Average(h => h.Hours);

                if (secondHalf > firstHalf * 1.2)
                {
                    analysis += "Kiváló teljesítmény! A legutóbbi időszakban átlagosan több mint 20%-kal tovább bírtad, mint a korábbiakban. A trend egyértelműen felfelé ível.\n\n";
                }
                else if (secondHalf < firstHalf * 0.8)
                {
                    analysis += "Figyelem! A legutóbbi időszakban a visszaesések közötti idő lerövidült. Érdemes lehet felülvizsgálni a jelenlegi stratégiát vagy beiktatni egy Cheat Day-t.\n\n";
                }
                else
                {
                    analysis += "Stagnálás figyelhető meg. Kiegyensúlyozott a teljesítményed, de az áttöréshez egy új stratégiára lehet szükség a kiváltó okok elkerülésére.\n\n";
                }

                analysis += $"Gyakoriság: Az elmúlt időszakban átlagosan {stats.AverageStreak.Days} naponta és {stats.AverageStreak.Hours} óránként történt esemény.\n\n";
                analysis += stats.AiSuggestion;
            }

            DeepAnalysisText.Text = analysis;
        }
    }
}