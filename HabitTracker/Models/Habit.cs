using System;

namespace HabitTracker.Models
{
    public enum HabitType
    {
        Bad, 
        Good 
    }

    public class Habit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastOccurrence { get; set; }
        public HabitType Type { get; set; }
    }

    public class HabitLog
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public string Note { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsCheatDay { get; set; }
    }
}