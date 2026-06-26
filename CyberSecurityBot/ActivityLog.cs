using System;
using System.Collections.Generic;

namespace CyberSecurityBot
{
    /// <summary>
    /// Tracks the chatbot's own actions during the current session — not
    /// user input, just what the bot has *done* (answered a topic, added a
    /// task, finished a quiz, etc.). Keeps only the most recent
    /// <see cref="MaxEntries"/> actions, newest first.
    ///
    /// Static/shared so any part of the app (MainForm, TaskCommandHandler,
    /// QuizForm) can log an action without needing a reference passed around.
    /// Resets automatically whenever the app restarts, since it only lives
    /// in memory for the session.
    /// </summary>
    public static class ActivityLog
    {
        public const int MaxEntries = 10;

        private static readonly List<(DateTime timestamp, string action)> _entries = new();

        /// <summary>
        /// Records a new bot action. If the log already has
        /// <see cref="MaxEntries"/> items, the oldest one is dropped.
        /// </summary>
        public static void Log(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return;

            _entries.Insert(0, (DateTime.Now, action));

            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(_entries.Count - 1);
        }

        /// <summary>
        /// Returns the log formatted for chat display, newest action first.
        /// </summary>
        public static string GetFormatted()
        {
            if (_entries.Count == 0)
                return "No activity logged yet this session.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Activity log (most recent {_entries.Count} action{(_entries.Count == 1 ? "" : "s")}):");

            for (int i = 0; i < _entries.Count; i++)
            {
                var (timestamp, action) = _entries[i];
                sb.AppendLine($"  {i + 1}. [{timestamp:HH:mm:ss}] {action}");
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Clears the log. Mainly useful for testing or a future "clear log" command.
        /// </summary>
        public static void Clear() => _entries.Clear();
    }
}
