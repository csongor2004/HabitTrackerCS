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
    }

    public static class StatisticsEngine
    {
        public static HabitStats CalculateStats(List<HabitLog> logs)
        {
            var stats = new HabitStats
            {
                CurrentStreak = TimeSpan.Zero,
                LongestStreak = TimeSpan.Zero,
                AverageStreak = TimeSpan.Zero,
                TotalEvents = logs.Count,
                CheatDays = logs.Count(l => l.IsCheatDay),
                AiSuggestion = "Nincs elég adat a részletes predikcióhoz."
            };

            var validLogs = logs.Where(l => !l.IsCheatDay).OrderBy(l => l.Timestamp).ToList();

            if (validLogs.Count > 0)
            {
                stats.CurrentStreak = DateTime.Now - validLogs.Last().Timestamp;
            }

            if (validLogs.Count < 2)
                return stats;

            var intervals = new List<double>();
            for (int i = 1; i < validLogs.Count; i++)
            {
                double hours = (validLogs[i].Timestamp - validLogs[i - 1].Timestamp).TotalHours;
                intervals.Add(hours);

                
                stats.History.Add(new IntervalData
                {
                    Hours = hours,
                    EndDate = validLogs[i].Timestamp.ToString("MM.dd")
                });
            }

            stats.LongestStreak = TimeSpan.FromHours(intervals.Max());
            stats.AverageStreak = TimeSpan.FromHours(intervals.Average());

            if (intervals.Count >= 3)
            {
                int n = intervals.Count;
                double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

                for (int i = 0; i < n; i++)
                {
                    double x = i + 1;
                    double y = intervals[i];
                    sumX += x;
                    sumY += y;
                    sumXY += x * y;
                    sumX2 += x * x;
                }

                double denominator = (n * sumX2) - (sumX * sumX);
                if (denominator != 0)
                {
                    double slope = ((n * sumXY) - (sumX * sumY)) / denominator;
                    double intercept = (sumY - slope * sumX) / n;
                    double predictedHours = (slope * (n + 1)) + intercept;

                    if (predictedHours > 0)
                    {
                        stats.PredictedNextEvent = validLogs.Last().Timestamp.AddHours(predictedHours);

                        if (slope > 0)
                            stats.AiSuggestion = "Javuló tendencia! Egyre több idő telik el két alkalom között.";
                        else
                            stats.AiSuggestion = "A statisztika alapján az intervallumok rövidülnek. Fókuszálj a megelőzésre, vagy iktass be egy tervezett Cheat Day-t a feszültség levezetésére.";
                    }
                }
            }

            return stats;
        }
    }
}