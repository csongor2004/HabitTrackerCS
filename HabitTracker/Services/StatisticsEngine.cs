using System;
using System.Collections.Generic;
using System.Linq;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    public class HabitStats
    {
        public TimeSpan LongestStreak { get; set; }
        public TimeSpan AverageStreak { get; set; }
        public DateTime? PredictedNextEvent { get; set; }
        public string AiSuggestion { get; set; }
    }

    public static class StatisticsEngine
    {
        public static HabitStats CalculateStats(List<HabitLog> logs)
        {
            var stats = new HabitStats
            {
                LongestStreak = TimeSpan.Zero,
                AverageStreak = TimeSpan.Zero,
                PredictedNextEvent = null,
                AiSuggestion = "Nincs elég adat a statisztikához."
            };

            if (logs == null || logs.Count < 2)
                return stats;

            var sortedLogs = logs.OrderBy(l => l.Timestamp).ToList();
            var intervals = new List<double>();

            for (int i = 1; i < sortedLogs.Count; i++)
            {
                var diff = sortedLogs[i].Timestamp - sortedLogs[i - 1].Timestamp;
                intervals.Add(diff.TotalHours);
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

                    double nextX = n + 1;
                    double predictedHours = (slope * nextX) + intercept;

                    if (predictedHours > 0)
                    {
                        stats.PredictedNextEvent = sortedLogs.Last().Timestamp.AddHours(predictedHours);

                        if (slope > 0)
                        {
                            stats.AiSuggestion = "Javuló tendencia. Egyre több időt bírsz ki két esemény között.";
                        }
                        else
                        {
                            stats.AiSuggestion = "A statisztika alapján közeledik a holtpont. Tervezz be egy kontrollált Cheat Day-t vagy elterelést!";
                        }
                    }
                }
            }
            else
            {
                stats.AiSuggestion = "A predikcióhoz legalább 3 regisztrált esemény szükséges.";
            }

            return stats;
        }
    }
}