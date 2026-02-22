using Microsoft.Data.Sqlite;
using HabitTracker.Models;
using System;
using System.Collections.Generic;

namespace HabitTracker.Services
{
    class DatabaseService
    {
        private const string DbPath = "Data Source=habits.db";

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Habits (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT,
                        LastOccurrence DATETIME,
                        Type INTEGER
                    );
                    CREATE TABLE IF NOT EXISTS Logs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        HabitId INTEGER,
                        Note TEXT,
                        Timestamp DATETIME,
                        IsCheatDay INTEGER DEFAULT 0,
                        FOREIGN KEY(HabitId) REFERENCES Habits(Id)
                    );";
                command.ExecuteNonQuery();
            }
        }

        public static void AddHabit(string name, HabitType type)
        {
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Habits (Name, LastOccurrence, Type) VALUES ($name, $now, $type)";
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$now", DateTime.Now);
                command.Parameters.AddWithValue("$type", (int)type);
                command.ExecuteNonQuery();
            }
        }

        public static List<Habit> GetHabits(HabitType type)
        {
            var habits = new List<Habit>();
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, LastOccurrence, Type FROM Habits WHERE Type = $type";
                command.Parameters.AddWithValue("$type", (int)type);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        habits.Add(new Habit
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            LastOccurrence = reader.GetDateTime(2),
                            Type = (HabitType)reader.GetInt32(3)
                        });
                    }
                }
            }
            return habits;
        }

        public static void ResetHabit(int id)
        {
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Habits SET LastOccurrence = $now WHERE Id = $id";
                command.Parameters.AddWithValue("$now", DateTime.Now);
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }
        public static void UpdateHabit(int id, string newName, HabitType newType)
        {
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Habits SET Name = $name, Type = $type WHERE Id = $id";
                command.Parameters.AddWithValue("$name", newName);
                command.Parameters.AddWithValue("$type", (int)newType);
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }


        public static List<HabitLog> GetLogsForHabit(int habitId)
        {
            var logs = new List<HabitLog>();
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, HabitId, Note, Timestamp FROM Logs WHERE HabitId = $habitId ORDER BY Timestamp DESC";
                command.Parameters.AddWithValue("$habitId", habitId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new HabitLog
                        {
                            Id = reader.GetInt32(0),
                            HabitId = reader.GetInt32(1),
                            Note = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Timestamp = reader.GetDateTime(3)
                        });
                    }
                }
            }
            return logs;
        }

        // Konkrét esemény törlése
        public static void DeleteLog(int logId)
        {
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Logs WHERE Id = $id";
                command.Parameters.AddWithValue("$id", logId);
                command.ExecuteNonQuery();
            }
        }
        public static void DeleteHabit(int id)
        {
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Habits WHERE Id = $id";
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }
        public static void RecordEvent(int habitId, string note = "", bool isCheatDay = false)
        {
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    var time = DateTime.Now;

                    if (!isCheatDay)
                    {
                        var updateCmd = connection.CreateCommand();
                        updateCmd.CommandText = "UPDATE Habits SET LastOccurrence = $now WHERE Id = $id";
                        updateCmd.Parameters.AddWithValue("$now", time);
                        updateCmd.Parameters.AddWithValue("$id", habitId);
                        updateCmd.ExecuteNonQuery();
                    }

                    var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO Logs (HabitId, Note, Timestamp, IsCheatDay) VALUES ($habitId, $note, $now, $isCheat)";
                    insertCmd.Parameters.AddWithValue("$habitId", habitId);
                    insertCmd.Parameters.AddWithValue("$note", note);
                    insertCmd.Parameters.AddWithValue("$now", time);
                    insertCmd.Parameters.AddWithValue("$isCheat", isCheatDay ? 1 : 0);
                    insertCmd.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }






    }
}