using System;
using System.Collections.Generic;

namespace CyberSecurityBot
{
    /// <summary>
    /// Contains all response logic. Extracted from ChatBot.cs so it can be
    /// shared between the console runner and the WinForms GUI.
    /// </summary>
    public class ResponseEngine
    {
        // ── Sentiment detection ───────────────────────────────────────────────

        private enum Sentiment { Neutral, Worried, Curious, Frustrated }

        private static readonly List<(string keyword, Sentiment sentiment)> _sentimentKeywords = new()
        {
            ("worried",     Sentiment.Worried),
            ("scared",      Sentiment.Worried),
            ("afraid",      Sentiment.Worried),

            ("curious",     Sentiment.Curious),
            ("wondering",   Sentiment.Curious),
            ("why",         Sentiment.Curious),

            ("frustrated",  Sentiment.Frustrated),
            ("annoyed",     Sentiment.Frustrated),
            ("don't understand", Sentiment.Frustrated),
        };

        private static readonly Dictionary<Sentiment, string> _sentimentPrefixes = new()
        {
            [Sentiment.Worried]    = "I understand this can feel concerning — you're not alone in worrying about this. ",
            [Sentiment.Curious]    = "Great question! Here's what you should know: ",
            [Sentiment.Frustrated] = "I hear you — this stuff can be genuinely frustrating. Let me try to make it clearer: ",
        };

        private static readonly Dictionary<Sentiment, string> _sentimentSuffixes = new()
        {
            [Sentiment.Worried]    = " Take a breath — being aware of the risk is already the first step to staying safe.",
            [Sentiment.Curious]    = " Feel free to ask if you'd like to go deeper on any of this.",
            [Sentiment.Frustrated] = " If anything is still unclear, just ask and I'll try a different explanation.",
        };

        private static Sentiment DetectSentiment(string input)
        {
            foreach (var (keyword, sentiment) in _sentimentKeywords)
            {
                if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    return sentiment;
            }
            return Sentiment.Neutral;
        }

        private static string ApplySentiment(Sentiment sentiment, string coreResponse)
        {
            if (sentiment == Sentiment.Neutral)
                return coreResponse;

            string prefix = _sentimentPrefixes.TryGetValue(sentiment, out var p) ? p : string.Empty;
            string suffix = _sentimentSuffixes.TryGetValue(sentiment, out var s) ? s : string.Empty;
            return prefix + coreResponse + suffix;
        }

        // ── Exact matches ─────────────────────────────────────────────────────

        private static readonly Dictionary<string, string> _exact =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["how are you?"]         = "I'm just code, but I'm running perfectly!",
            ["how are you"]          = "I'm just code, but I'm running perfectly!",
            ["what's your purpose?"] = "I promote cybersecurity awareness and safe online practices.",
            ["what is your purpose"] = "I promote cybersecurity awareness and safe online practices.",
            ["what can i ask about?"]= "You can ask about passwords, phishing, malware, ransomware, and more. You can also manage tasks (\"Add task - <title>\", \"view tasks\", \"delete task <id>\"), or click Quiz to test your knowledge!",
            ["help"]                 = "Topics I can help with:\n  • passwords\n  • phishing\n  • malware\n  • ransomware\n\nTask management:\n  • Add task - <title>\n  • view tasks\n  • delete task <id>\n\nYou can also click the Quiz button to test your knowledge.",
        };

        // ── Keyword responses ─────────────────────────────────────────────────

        private static readonly List<(string keyword, string response)> _keywords = new()
        {
            ("password",
             "Use long, unique passwords for every account — at least 12 characters mixing letters, numbers, and symbols. " +
             "A password manager (like Bitwarden or 1Password) can generate and store them securely so you never have to remember them all."),

            ("phishing",
             "Phishing attacks trick you into giving up credentials or installing malware via fake emails or websites. " +
             "Always verify the sender's address, hover over links before clicking, and never enter credentials on a page you reached via email. " +
             "When in doubt, go directly to the site by typing the URL yourself."),

            ("malware",
             "Malware is malicious software including viruses, trojans, spyware, and ransomware. " +
             "Keep your OS and apps updated, use reputable antivirus software, and avoid downloading files from untrusted sources."),

            ("ransomware",
             "Ransomware encrypts your files and demands payment for the key. " +
             "Regular offline backups are your best defence — if you have a clean backup, you can restore without paying. " +
             "Never open unexpected email attachments and keep software patched."),

        };

        // ── Public entry point ────────────────────────────────────────────────

        public string GetResponse(string input)
        {
            Sentiment sentiment = DetectSentiment(input);

            if (_exact.TryGetValue(input.Trim(), out string? exact))
                return exact;

            foreach (var (keyword, response) in _keywords)
            {
                if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    return ApplySentiment(sentiment, response);
            }

            return sentiment switch
            {
                Sentiment.Worried    => "I can tell you're concerned. Could you tell me more about what's worrying you? " +
                                        "I can help with topics like passwords, phishing, malware, and more — type \"help\" for the full list.",
                Sentiment.Curious    => "I'd love to help! I cover topics like passwords, phishing, malware, ransomware, and more. Type \"help\" to see everything.",
                Sentiment.Frustrated => "I'm sorry I'm not hitting the mark. Try typing a specific topic like \"phishing\" or \"password\", " +
                                        "or type \"help\" for the full list.",
                _                   => "I didn't quite understand that. Type \"help\" to see the topics I can assist with, or try rephrasing your question.",
            };
        }
    }
}