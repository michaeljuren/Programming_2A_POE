using System;

namespace CyberSecurityBot
{
    /// <summary>
    /// Represents a single row in the `tasks` table.
    /// </summary>
    public class TaskItem
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Reminder { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Completed { get; set; }
    }
} 