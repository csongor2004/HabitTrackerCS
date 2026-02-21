using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace HabitTracker
{
    public static class DatabaseHelper
    {
        private const string DbPath = "Data Source=habits.db";

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(DbPath))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"CREATE TABLE IF NOT EXISTS Habits (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT,
                LastOccurrence DATETIME
            )";
                command.ExecuteNonQuery();
            }
        }
    }
}
