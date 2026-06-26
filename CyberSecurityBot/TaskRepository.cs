using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MySqlConnector;

namespace CyberSecurityBot
{
    /// <summary>
    /// Handles all database access for the `tasks` table on the
    /// `task-manager` MySQL database. Reads its connection string from
    /// appsettings.json (copied to the output directory at build time).
    /// </summary>
    public class TaskRepository
    {
        private readonly string _connectionString;

        public TaskRepository()
        {
            _connectionString = LoadConnectionString();
        }

        private static string LoadConnectionString()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            if (!File.Exists(path))
                throw new FileNotFoundException(
                    "appsettings.json not found next to the executable. " +
                    "Copy appsettings.json into the CyberSecurityBot project folder and set " +
                    "your MySQL connection details (see README).");

            string json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);

            string? connStr = doc.RootElement
                .GetProperty("ConnectionStrings")
                .GetProperty("TaskManager")
                .GetString();

            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException(
                    "ConnectionStrings:TaskManager is missing or empty in appsettings.json.");

            return connStr;
        }

        /// <summary>
        /// Inserts a new task and returns the auto-generated ID.
        /// </summary>
        public int AddTask(string title, string description, DateTime? reminder)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                INSERT INTO task (TITLE, DESCRIPTION, REMINDER, CREATED_AT, COMPLETED)
                VALUES (@title, @description, @reminder, @createdAt, @completed);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@reminder", reminder.HasValue ? reminder.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@completed", false);

            object? result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Sets the reminder date/time on an existing task.
        /// </summary>
        public void SetReminder(int taskId, DateTime reminder)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = "UPDATE task SET REMINDER = @reminder WHERE ID = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@reminder", reminder);
            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns all tasks ordered by most recently created first.
        /// </summary>
        public List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT ID, TITLE, DESCRIPTION, REMINDER, CREATED_AT, COMPLETED
                FROM task
                ORDER BY CREATED_AT DESC;";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(new TaskItem
                {
                    Id          = reader.GetInt32("ID"),
                    Title       = reader.GetString("TITLE"),
                    Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION"))
                                        ? null : reader.GetString("DESCRIPTION"),
                    Reminder    = reader.IsDBNull(reader.GetOrdinal("REMINDER"))
                                        ? null : reader.GetDateTime("REMINDER"),
                    CreatedAt   = reader.GetDateTime("CREATED_AT"),
                    Completed   = reader.GetBoolean("COMPLETED"),
                });
            }

            return tasks;
        }

        /// <summary>
        /// Fetches a single task by ID, or null if it doesn't exist.
        /// </summary>
        public TaskItem? GetTaskById(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT ID, TITLE, DESCRIPTION, REMINDER, CREATED_AT, COMPLETED
                FROM task
                WHERE ID = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new TaskItem
            {
                Id          = reader.GetInt32("ID"),
                Title       = reader.GetString("TITLE"),
                Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION"))
                                    ? null : reader.GetString("DESCRIPTION"),
                Reminder    = reader.IsDBNull(reader.GetOrdinal("REMINDER"))
                                    ? null : reader.GetDateTime("REMINDER"),
                CreatedAt   = reader.GetDateTime("CREATED_AT"),
                Completed   = reader.GetBoolean("COMPLETED"),
            };
        }

        /// <summary>
        /// Deletes a task by ID. Returns true if a row was removed.
        /// </summary>
        public bool DeleteTask(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = "DELETE FROM task WHERE ID = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }
}