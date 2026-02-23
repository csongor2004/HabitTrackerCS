using System;
using System.Collections.Generic;
using System.Linq;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    public class IntervalData
    {
        public double Hours { get; set; }
        public string EndDate { get; set; }
    }

    public class HabitStats
    {
        public TimeSpan CurrentStreak { get; set; }
        public TimeSpan LongestStreak { get; set; }
        public TimeSpan AverageStreak { get; set; }
        public int TotalEvents { get; set; }
        public int CheatDays { get; set; }
        public DateTime? PredictedNextEvent { get; set; }
        public string AiSuggestion { get; set; }
        public List<IntervalData> History { get; set; } = new List<IntervalData>();

        public int CurrentDailyStreak { get; set; }
        public int LongestDailyStreak { get; set; }
        public int TotalCompletions { get; set; }

        public string MostFrequentDay { get; set; }
        public string MostFrequentTimeOfDay { get; set; }
        public List<string> EarnedBadges { get; set; } = new List<string>();
    }

    public static class StatisticsEngine
    {
        public static HabitStats CalculateStats(List<HabitLog> logs)
        {
            var stats = new HabitStats { CurrentStreak = TimeSpan.Zero, LongestStreak = TimeSpan.Zero, AverageStreak = TimeSpan.Zero, TotalEvents = logs.Count, CheatDays = logs.Count(l => l.IsCheatDay), AiSuggestion = "Nincs elég adat a predikcióhoz." };
            var validLogs = logs.Where(l => !l.IsCheatDay).OrderBy(l => l.Timestamp).ToList();

            if (validLogs.Count > 0) stats.CurrentStreak = DateTime.Now - validLogs.Last().Timestamp;
            if (validLogs.Count < 2) return stats;

            var intervals = new List<double>();
            for (int i = 1; i < validLogs.Count; i++)
            {
                double hours = (validLogs[i].Timestamp - validLogs[i - 1].Timestamp).TotalHours;
                intervals.Add(hours);
                stats.History.Add(new IntervalData { Hours = hours, EndDate = validLogs[i].Timestamp.ToString("MM.dd") });
            }

            stats.LongestStreak = TimeSpan.FromHours(intervals.Max());
            stats.AverageStreak = TimeSpan.FromHours(intervals.Average());

            if (intervals.Count >= 3)
            {
                int n = intervals.Count;
                double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
                for (int i = 0; i < n; i++) { double x = i + 1; double y = intervals[i]; sumX += x; sumY += y; sumXY += x * y; sumX2 += x * x; }
                double denominator = (n * sumX2) - (sumX * sumX);
                if (denominator != 0)
                {
                    double slope = ((n * sumXY) - (sumX * sumY)) / denominator;
                    double intercept = (sumY - slope * sumX) / n;
                    double predictedHours = (slope * (n + 1)) + intercept;
                    if (predictedHours > 0)
                    {
                        stats.PredictedNextEvent = validLogs.Last().Timestamp.AddHours(predictedHours);
                        stats.AiSuggestion = slope > 0 ? "Javuló tendencia! Egyre több idő telik el." : "Az intervallumok rövidülnek. Fókuszálj a megelőzésre!";
                    }
                }
            }

            // Rossz szokások kitűzői
            if (stats.CurrentStreak.Days >= 3) stats.EarnedBadges.Add("🥉 3 Napja Tiszta!");
            if (stats.CurrentStreak.Days >= 7) stats.EarnedBadges.Add("🥈 1 Hete Tiszta!");
            if (stats.CurrentStreak.Days >= 30) stats.EarnedBadges.Add("🥇 1 Hónapja Tiszta!");
            if (stats.LongestStreak.Days >= 14 && stats.CurrentStreak.Days < 14) stats.EarnedBadges.Add("🛡️ Volt már 2 hetes sorozatod, meg tudod csinálni újra!");
            AnalyzeBehaviorPatterns(stats, validLogs);
            return stats;
        }
        public static void AnalyzeBehaviorPatterns(HabitStats stats, List<HabitLog> validLogs)
        {
            if (validLogs.Count == 0)
            {
                stats.MostFrequentDay = "Nincs adat";
                stats.MostFrequentTimeOfDay = "Nincs adat";
                return;
            }

            string[] huDays = { "Vasárnap", "Hétfő", "Kedd", "Szerda", "Csütörtök", "Péntek", "Szombat" };

            // Leggyakoribb nap kiszámítása
            var topDay = validLogs.GroupBy(l => l.Timestamp.DayOfWeek)
                                  .OrderByDescending(g => g.Count())
                                  .FirstOrDefault();

            if (topDay != null)
            {
                stats.MostFrequentDay = $"{huDays[(int)topDay.Key]} ({topDay.Count()} alkalom)";
            }

            // Leggyakoribb napszak kiszámítása
            var topHour = validLogs.GroupBy(l => l.Timestamp.Hour)
                                   .OrderByDescending(g => g.Count())
                                   .FirstOrDefault();

            if (topHour != null)
            {
                int h = topHour.Key;
                if (h >= 5 && h < 12) stats.MostFrequentTimeOfDay = $"Délelőtt ({topHour.Count()} alkalom)";
                else if (h >= 12 && h < 18) stats.MostFrequentTimeOfDay = $"Délután ({topHour.Count()} alkalom)";
                else if (h >= 18 && h < 23) stats.MostFrequentTimeOfDay = $"Este ({topHour.Count()} alkalom)";
                else stats.MostFrequentTimeOfDay = $"Éjszaka ({topHour.Count()} alkalom)";
            }
        }
        public static HabitStats CalculateGoodHabitStats(List<HabitLog> logs)
        {
            var stats = new HabitStats { TotalCompletions = logs.Count(l => !l.IsCheatDay), CheatDays = logs.Count(l => l.IsCheatDay), AiSuggestion = "Kezdd el rögzíteni a sikeres napokat!" };
            if (logs.Count == 0) return stats;

            var activeDates = logs.Select(l => l.Timestamp.Date).Distinct().OrderBy(d => d).ToList();

            if (activeDates.Count > 0)
            {
                int currentStreak = 1;
                int longestStreak = 1;
                int tempStreak = 1;

                for (int i = 1; i < activeDates.Count; i++)
                {
                    if ((activeDates[i] - activeDates[i - 1]).Days == 1) tempStreak++;
                    else { if (tempStreak > longestStreak) longestStreak = tempStreak; tempStreak = 1; }
                }
                if (tempStreak > longestStreak) longestStreak = tempStreak;
                stats.LongestDailyStreak = longestStreak;

                if ((DateTime.Today - activeDates.Last()).Days <= 1)
                {
                    currentStreak = 1;
                    for (int i = activeDates.Count - 1; i > 0; i--)
                    {
                        if ((activeDates[i] - activeDates[i - 1]).Days == 1) currentStreak++;
                        else break;
                    }
                    stats.CurrentDailyStreak = currentStreak;
                }
                else stats.CurrentDailyStreak = 0;

                if (stats.CurrentDailyStreak == 0) stats.AiSuggestion = "Megszakadt a sorozat. Kezdd újra még ma!";
                else if (stats.CurrentDailyStreak > 3) stats.AiSuggestion = $"Kiváló lendület! Már {stats.CurrentDailyStreak} napja zsinórban teljesíted!";
                else stats.AiSuggestion = "Jó úton haladsz, tartsd fenn a folyamatosságot!";
            }

           
            if (stats.CurrentDailyStreak >= 3) stats.EarnedBadges.Add("🔥 3 Napos Sikerszéria!");
            if (stats.CurrentDailyStreak >= 7) stats.EarnedBadges.Add("⚡ 1 Hetes Villám!");
            if (stats.TotalCompletions >= 10) stats.EarnedBadges.Add("🎯 10 Teljesítés!");
            if (stats.TotalCompletions >= 50) stats.EarnedBadges.Add("🏆 50 Teljesítés Mestere!");
            var nonCheatLogs = logs.Where(l => !l.IsCheatDay).ToList();
            AnalyzeBehaviorPatterns(stats, nonCheatLogs);
            return stats;
        }
    }
}