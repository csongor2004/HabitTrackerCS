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
    }
}