using System.Collections.Generic;

namespace CyberSecurityBot
{
    public enum QuestionType { TrueFalse, MultipleChoice }

    /// <summary>
    /// A single quiz question. For TrueFalse questions, Options should be
    /// ["True", "False"] and CorrectIndex 0 or 1. For MultipleChoice,
    /// Options holds up to 4 entries.
    /// </summary>
    public class QuizQuestion
    {
        public required string Text { get; init; }
        public required QuestionType Type { get; init; }
        public required List<string> Options { get; init; }
        public required int CorrectIndex { get; init; }
        public required string Explanation { get; init; }
    }

    /// <summary>
    /// Static bank of 12 cybersecurity quiz questions, mixing true/false
    /// and multiple-choice (max 4 options) formats.
    /// </summary>
    public static class QuizBank
    {
        public static List<QuizQuestion> GetQuestions() => new()
        {
            new QuizQuestion
            {
                Text = "Using the same password for multiple accounts is safe as long as the password is long.",
                Type = QuestionType.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "False — reusing passwords means one breach can compromise every account that shares it, regardless of password length."
            },
            new QuizQuestion
            {
                Text = "Which of these is the strongest defence against ransomware?",
                Type = QuestionType.MultipleChoice,
                Options = new List<string>
                {
                    "Regular offline backups",
                    "A longer Windows password",
                    "Clearing your browser cache",
                    "Disabling Wi-Fi at night"
                },
                CorrectIndex = 0,
                Explanation = "Offline backups let you restore your files without paying a ransom, making them the most reliable defence."
            },
            new QuizQuestion
            {
                Text = "Two-factor authentication (2FA) relies only on something you know, like a password.",
                Type = QuestionType.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "False — 2FA combines something you know (password) with something you have (a code, app, or device), adding a second layer."
            },
            new QuizQuestion
            {
                Text = "What is the main goal of a phishing email?",
                Type = QuestionType.MultipleChoice,
                Options = new List<string>
                {
                    "To slow down your internet",
                    "To trick you into giving up credentials or installing malware",
                    "To update your antivirus software",
                    "To back up your files automatically"
                },
                CorrectIndex = 1,
                Explanation = "Phishing emails impersonate trusted sources to trick recipients into revealing sensitive information or installing malicious software."
            },
            new QuizQuestion
            {
                Text = "A VPN makes you completely anonymous online with no need to trust anyone.",
                Type = QuestionType.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "False — a VPN hides your traffic from your network/ISP, but it shifts that trust to the VPN provider, who can still see your activity."
            },
            new QuizQuestion
            {
                Text = "Which password manager habit is recommended?",
                Type = QuestionType.MultipleChoice,
                Options = new List<string>
                {
                    "Writing passwords on a sticky note",
                    "Reusing one strong password everywhere",
                    "Using a password manager to generate and store unique passwords",
                    "Sharing passwords over email when needed"
                },
                CorrectIndex = 2,
                Explanation = "A password manager generates and stores unique, complex passwords for every account, removing the need to remember or reuse them."
            },
            new QuizQuestion
            {
                Text = "It's safe to click a link in an email if it looks like it's from your bank.",
                Type = QuestionType.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "False — phishing emails often spoof trusted senders. Always verify the sender and type the bank's URL directly instead of clicking email links."
            },
            new QuizQuestion
            {
                Text = "What does malware refer to?",
                Type = QuestionType.MultipleChoice,
                Options = new List<string>
                {
                    "Outdated hardware drivers",
                    "Malicious software such as viruses, trojans, and spyware",
                    "A weak Wi-Fi signal",
                    "A type of firewall"
                },
                CorrectIndex = 1,
                Explanation = "Malware is a broad term for malicious software designed to damage, disrupt, or gain unauthorised access to systems."
            },
            new QuizQuestion
            {
                Text = "SMS-based 2FA codes are just as secure as authenticator apps.",
                Type = QuestionType.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "False — SMS codes can be intercepted via SIM-swapping attacks, making authenticator apps or hardware keys a stronger choice."
            },
            new QuizQuestion
            {
                Text = "Which of these is a sign of a phishing attempt?",
                Type = QuestionType.MultipleChoice,
                Options = new List<string>
                {
                    "A sense of urgency demanding immediate action",
                    "An email addressed to you by name",
                    "A message sent during business hours",
                    "An email with no attachments"
                },
                CorrectIndex = 0,
                Explanation = "Phishing emails often create false urgency to pressure victims into acting quickly without thinking critically."
            },
            new QuizQuestion
            {
                Text = "Keeping your software updated helps protect against known vulnerabilities.",
                Type = QuestionType.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 0,
                Explanation = "True — updates often patch security flaws that attackers actively exploit, so staying current reduces your risk significantly."
            },
            new QuizQuestion
            {
                Text = "If your data appears in a breach, what should you do first?",
                Type = QuestionType.MultipleChoice,
                Options = new List<string>
                {
                    "Ignore it if nothing seems wrong",
                    "Change affected passwords and enable 2FA",
                    "Delete your email account",
                    "Wait a few months to see if anything happens"
                },
                CorrectIndex = 1,
                Explanation = "Changing affected passwords and enabling 2FA immediately limits the damage an attacker can do with leaked credentials."
            },
        };
    }
}