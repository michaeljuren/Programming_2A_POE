using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CyberSecurityBot
{
    /// <summary>
    /// Detects and drives task-management chat commands:
    ///   "Add task - <title>"        → creates a task, then asks about a reminder
    ///   "view tasks"                → lists all tasks
    ///   "delete task <id>"          → asks for confirmation, then deletes
    /// Recognizes natural variations of each command (different verbs,
    /// optional filler words, missing separators) rather than requiring an
    /// exact phrase.
    /// Holds the small bit of conversation state needed for the multi-turn
    /// "add task" flow (waiting on a reminder answer) and the delete
    /// confirmation step.
    /// </summary>
    public class TaskCommandHandler
    {
        private readonly TaskRepository _repo = new TaskRepository();

        // Conversation state
        private enum PendingAction { None, AwaitingReminder, AwaitingReminderDays, AwaitingDeleteConfirmation }
        private PendingAction _pending = PendingAction.None;
        private int _pendingTaskId;

        // ── Add task ─────────────────────────────────────────────────────────
        // Matches a leading verb (add/create/make/new/...) plus the word
        // "task", optionally followed by filler ("a", "for", "to", "called",
        // "named") and a separator (-, :, –, —), before the actual title.
        // Examples that all match:
        //   "Add task - Review privacy settings"
        //   "Create a task to review my password"
        //   "New task: clean my inbox"
        //   "please add task for backing up files"
        //   "make a task called review settings"
        private static readonly Regex AddTaskPattern = new(
            @"^(?:please\s+)?(?:add|create|make|new|set up|setup)\s+(?:a\s+|an\s+)?task" +
            @"\s*(?:called|named|for|to|titled)?\s*[-:–—]?\s*(.+)$",
            RegexOptions.IgnoreCase);

        // ── View tasks ───────────────────────────────────────────────────────
        // Matches view/show/list/see/check/get + (my/the/all) + task(s),
        // or short forms like "tasks", "my tasks", "what are my tasks".
        private static readonly Regex ViewTasksPattern = new(
            @"^(?:(?:can\s+you\s+)?(?:show|view|list|see|check|get|display)\s+(?:me\s+)?(?:my\s+|the\s+|all\s+)?tasks?" +
            @"|what(?:'s| is| are)\s+(?:my\s+|on\s+my\s+)?tasks?(?:\s+list)?" +
            @"|my\s+tasks?" +
            @"|tasks?\s+list)\s*\??$",
            RegexOptions.IgnoreCase);

        // ── Delete task ──────────────────────────────────────────────────────
        // Matches delete/remove/cancel/clear + optional "task"/"#" + a number,
        // in either order ("delete task 3" or "delete 3").
        private static readonly Regex DeleteTaskPattern = new(
            @"^(?:please\s+)?(?:delete|remove|cancel|clear|get rid of)\s+(?:task\s*)?#?\s*(\d+)$",
            RegexOptions.IgnoreCase);

        private static readonly Regex ReminderDaysPattern =
            new(@"(\d+)\s*day", RegexOptions.IgnoreCase);

        /// <summary>
        /// Returns true if this input is relevant to task management
        /// (either starts a new command, or continues a pending one),
        /// and sets <paramref name="response"/> to the bot's reply.
        /// </summary>
        public bool TryHandle(string rawInput, out string response)
        {
            string input = rawInput.Trim();

            // ── Continue a pending multi-turn flow first ───────────────────────
            if (_pending == PendingAction.AwaitingReminder)
            {
                response = HandleReminderReply(input);
                return true;
            }

            if (_pending == PendingAction.AwaitingReminderDays)
            {
                response = HandleReminderDaysReply(input);
                return true;
            }

            if (_pending == PendingAction.AwaitingDeleteConfirmation)
            {
                response = HandleDeleteConfirmationReply(input);
                return true;
            }

            // ── New command? Check delete/view first since they're narrower
            //    and less likely to accidentally swallow an add-task title ───
            var deleteMatch = DeleteTaskPattern.Match(input);
            if (deleteMatch.Success)
            {
                int id = int.Parse(deleteMatch.Groups[1].Value);
                response = HandleDeleteRequest(id);
                return true;
            }

            if (ViewTasksPattern.IsMatch(input))
            {
                response = HandleViewTasks();
                return true;
            }

            var addMatch = AddTaskPattern.Match(input);
            if (addMatch.Success)
            {
                response = HandleAddTask(addMatch.Groups[1].Value.Trim());
                return true;
            }

            response = string.Empty;
            return false;
        }

        // ── Add task ─────────────────────────────────────────────────────────
        private string HandleAddTask(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "I need a title for the task — try \"Add task - <title>\".";

            string description = BuildTemplateDescription(title);

            try
            {
                int newId = _repo.AddTask(title, description, reminder: null);
                _pending = PendingAction.AwaitingReminder;
                _pendingTaskId = newId;

                ActivityLog.Log($"Added task #{newId} — \"{title}\"");

                return $"Task added with the description \"{description}\" Would you like a reminder?";
            }
            catch (Exception ex)
            {
                _pending = PendingAction.None;
                return $"I couldn't save that task — there was a database error: {ex.Message}";
            }
        }

        /// <summary>
        /// Simple template: reuses the title to produce a one-sentence
        /// description, matching the style "Review account privacy settings
        /// to ensure your data is protected."
        /// </summary>
        private static string BuildTemplateDescription(string title)
        {
            string lower = title.Trim();
            if (lower.Length == 0) return lower;

            // Lower-case the first letter so it reads naturally mid-sentence,
            // unless it looks like an acronym (all caps).
            string body = lower;
            if (!IsAllCaps(body))
                body = char.ToLower(body[0], CultureInfo.InvariantCulture) + body.Substring(1);

            return $"Reminder to {body}.";
        }

        private static bool IsAllCaps(string s)
        {
            foreach (char c in s)
            {
                if (char.IsLetter(c) && char.IsLower(c)) return false;
            }
            return true;
        }

        // ── Reminder follow-up ───────────────────────────────────────────────
        private string HandleReminderReply(string input)
        {
            int taskId = _pendingTaskId;

            bool isYes = Regex.IsMatch(input, @"^(yes|yep|yeah|sure|ok(ay)?)\b", RegexOptions.IgnoreCase);
            bool isNo  = Regex.IsMatch(input, @"^(no|nope|nah)\b", RegexOptions.IgnoreCase);

            // "Yes, remind me in 3 days." or just "in 3 days" / "3 days"
            var daysMatch = ReminderDaysPattern.Match(input);

            if (daysMatch.Success)
            {
                _pending = PendingAction.None;
                int days = int.Parse(daysMatch.Groups[1].Value);
                DateTime reminderDate = DateTime.Now.AddDays(days);

                try
                {
                    _repo.SetReminder(taskId, reminderDate);
                    ActivityLog.Log($"Set a {days}-day reminder on task #{taskId}");
                    return $"Got it! I'll remind you in {days} day{(days == 1 ? "" : "s")}.";
                }
                catch (Exception ex)
                {
                    return $"The task was saved, but I couldn't set the reminder due to a database error: {ex.Message}";
                }
            }

            if (isNo)
            {
                _pending = PendingAction.None;
                return "No problem — no reminder set for this task.";
            }

            if (isYes)
            {
                // Stay in a pending state so the next message (the number of
                // days) is still captured, instead of being treated as a new
                // top-level command.
                _pending = PendingAction.AwaitingReminderDays;
                return "Sure — how many days from now should I remind you?";
            }

            // Unclear reply — treat the task as saved without a reminder rather
            // than getting stuck waiting forever.
            _pending = PendingAction.None;
            return "I didn't catch a number of days, so I haven't set a reminder. " +
                   "You can always add one later by asking me again.";
        }

        /// <summary>
        /// Handles the follow-up after a plain "yes" — the person is now
        /// expected to reply with just a number of days (e.g. "3" or "3 days").
        /// </summary>
        private string HandleReminderDaysReply(string input)
        {
            _pending = PendingAction.None;
            int taskId = _pendingTaskId;

            // Accept "3 days", "in 3 days", or a bare number like "3".
            var daysMatch = ReminderDaysPattern.Match(input);
            int? days = daysMatch.Success
                ? int.Parse(daysMatch.Groups[1].Value)
                : (int.TryParse(input.Trim(), out int bareNumber) ? bareNumber : (int?)null);

            if (days.HasValue && days.Value > 0)
            {
                DateTime reminderDate = DateTime.Now.AddDays(days.Value);

                try
                {
                    _repo.SetReminder(taskId, reminderDate);
                    ActivityLog.Log($"Set a {days.Value}-day reminder on task #{taskId}");
                    return $"Got it! I'll remind you in {days.Value} day{(days.Value == 1 ? "" : "s")}.";
                }
                catch (Exception ex)
                {
                    return $"The task was saved, but I couldn't set the reminder due to a database error: {ex.Message}";
                }
            }

            return "I still didn't catch a number of days, so I haven't set a reminder. " +
                   "You can always add one later by asking me again.";
        }

        // ── View tasks ───────────────────────────────────────────────────────
        private string HandleViewTasks()
        {
            System.Collections.Generic.List<TaskItem> tasks;

            try
            {
                tasks = _repo.GetAllTasks();
            }
            catch (Exception ex)
            {
                return $"I couldn't load your tasks due to a database error: {ex.Message}";
            }

            if (tasks.Count == 0)
            {
                ActivityLog.Log("Viewed task list (empty)");
                return "You don't have any tasks yet. Try \"Add task - <title>\" to create one.";
            }

            ActivityLog.Log($"Viewed task list ({tasks.Count} task{(tasks.Count == 1 ? "" : "s")})");

            var sb = new StringBuilder();
            sb.AppendLine($"You have {tasks.Count} task{(tasks.Count == 1 ? "" : "s")}:");

            foreach (var t in tasks)
            {
                string status   = t.Completed ? "✓ done" : "pending";
                string reminder = t.Reminder.HasValue
                    ? t.Reminder.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
                    : "none";

                sb.AppendLine($"  #{t.Id} — {t.Title} [{status}] (reminder: {reminder})");
            }

            sb.Append("Type \"delete task <id>\" to remove one.");
            return sb.ToString();
        }

        // ── Delete task ──────────────────────────────────────────────────────
        private string HandleDeleteRequest(int id)
        {
            TaskItem? task;

            try
            {
                task = _repo.GetTaskById(id);
            }
            catch (Exception ex)
            {
                return $"I couldn't look up that task due to a database error: {ex.Message}";
            }

            if (task == null)
                return $"I couldn't find a task with ID #{id}. Type \"view tasks\" to see your current list.";

            _pending = PendingAction.AwaitingDeleteConfirmation;
            _pendingTaskId = id;

            return $"Are you sure you want to delete task #{id} — \"{task.Title}\"? (yes/no)";
        }

        private string HandleDeleteConfirmationReply(string input)
        {
            _pending = PendingAction.None;
            int taskId = _pendingTaskId;

            bool isYes = Regex.IsMatch(input, @"^(yes|yep|yeah|confirm)\b", RegexOptions.IgnoreCase);

            if (!isYes)
                return "Okay, I've left that task as-is.";

            try
            {
                bool deleted = _repo.DeleteTask(taskId);

                if (deleted)
                    ActivityLog.Log($"Deleted task #{taskId}");

                return deleted
                    ? $"Task #{taskId} has been deleted."
                    : $"Task #{taskId} no longer exists — it may have already been deleted.";
            }
            catch (Exception ex)
            {
                return $"I couldn't delete that task due to a database error: {ex.Message}";
            }
        }
    }
}