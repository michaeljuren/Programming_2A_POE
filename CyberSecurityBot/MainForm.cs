using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace CyberSecurityBot
{
    public class MainForm : Form
    {
        // Controls
        private Panel headerPanel = null!;
        private RichTextBox asciiHeader = null!;
        private RichTextBox chatDisplay = null!;
        private TextBox inputBox = null!;
        private Button sendButton = null!;
        private Panel inputPanel = null!;
        private Button quizButton = null!;

        // Bot logic
        private readonly ResponseEngine _engine = new ResponseEngine();
        private readonly TaskCommandHandler _taskHandler = new TaskCommandHandler();
        private string? _userName;
        private bool _awaitingName = true;

        // Theme colours
        private static readonly Color BgDark       = Color.FromArgb(13,  17,  23);
        private static readonly Color BgMid        = Color.FromArgb(22,  27,  34);
        private static readonly Color BgPanel      = Color.FromArgb(30,  35,  45);
        private static readonly Color AccentCyan   = Color.FromArgb(0,  212, 212);
        private static readonly Color AccentGreen  = Color.FromArgb(57, 255, 107);
        private static readonly Color TextPrimary  = Color.FromArgb(230, 237, 243);
        private static readonly Color TextMuted    = Color.FromArgb(139, 148, 158);
        private static readonly Color UserBubble   = Color.FromArgb(33,  58,  88);
        private static readonly Color BotBubble    = Color.FromArgb(25,  40,  55);

        public MainForm()
        {
            InitializeComponent();
            PlayWelcomeSound();
            BeginInlineNamePrompt();
            Load += (_, _) => inputBox.Focus();
        }

        // ─── Build UI ──────────────────────────────────────────────────────────
        private void InitializeComponent()
        {
            SuspendLayout();

            // Form
            Text            = "CyberBot — Cybersecurity Awareness Assistant";
            Size            = new Size(920, 680);
            MinimumSize     = new Size(720, 520);
            BackColor       = BgDark;
            ForeColor       = TextPrimary;
            Font            = new Font("Segoe UI", 10f, FontStyle.Regular);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;

            // ── Header ────────────────────────────────────────────────────────
            headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 148,
                BackColor = BgMid,
            };
            headerPanel.Paint += HeaderPanel_Paint;

            asciiHeader = UIHelper.CreateAsciiHeader(
                fontSize: 7.4f,
                includeSubtitle: true,
                dock: DockStyle.Fill);

            quizButton = new Button
            {
                Text      = "🧠 Quiz",
                Size      = new Size(110, 38),
                BackColor = AccentCyan,
                ForeColor = BgDark,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
            };
            quizButton.FlatAppearance.BorderSize = 0;
            quizButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 180, 180);
            quizButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 150, 150);
            quizButton.Click += QuizButton_Click;

            var quizButtonHolder = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 130,
                BackColor = BgMid,
            };
            quizButton.Location = new Point(10, 16);
            quizButtonHolder.Controls.Add(quizButton);

            headerPanel.Controls.Add(asciiHeader);
            headerPanel.Controls.Add(quizButtonHolder);


            // ── Input panel ───────────────────────────────────────────────────
            inputPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 60,
                BackColor = BgMid,
                Padding   = new Padding(12, 10, 12, 10),
            };

            inputBox = new TextBox
            {
                Dock          = DockStyle.Fill,
                BackColor     = BgPanel,
                ForeColor     = TextPrimary,
                BorderStyle   = BorderStyle.None,
                Font          = new Font("Segoe UI", 11f),
                PlaceholderText = "Type a message…",
            };
            inputBox.KeyDown += InputBox_KeyDown;

            sendButton = new Button
            {
                Text      = "Send  ➤",
                Dock      = DockStyle.Right,
                Width     = 110,
                BackColor = AccentCyan,
                ForeColor = BgDark,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
            };
            sendButton.FlatAppearance.BorderSize  = 0;
            sendButton.FlatAppearance.MouseOverBackColor  = Color.FromArgb(0, 180, 180);
            sendButton.FlatAppearance.MouseDownBackColor  = Color.FromArgb(0, 150, 150);
            sendButton.Click += SendButton_Click;

            // Wrap input + button in a rounded inner panel
            var innerWrap = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = BgPanel,
                Padding   = new Padding(10, 0, 0, 0),
            };
            innerWrap.Controls.Add(inputBox);
            innerWrap.Controls.Add(sendButton);
            innerWrap.Paint += RoundedPanel_Paint;

            inputPanel.Controls.Add(innerWrap);

            // ── Chat display ──────────────────────────────────────────────────
            chatDisplay = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = BgDark,
                ForeColor   = TextPrimary,
                BorderStyle = BorderStyle.None,
                Font        = new Font("Segoe UI", 10.5f),
                ReadOnly    = true,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                Padding     = new Padding(14),
                WordWrap    = true,
            };

            // Assemble
            Controls.Add(chatDisplay);
            Controls.Add(inputPanel);
            Controls.Add(headerPanel);

            ResumeLayout(false);
        }

        // ─── Custom painting ───────────────────────────────────────────────────
        private void HeaderPanel_Paint(object? sender, PaintEventArgs e)
        {
            // Bottom border accent line
            using var pen = new Pen(AccentCyan, 2);
            int y = headerPanel.Height - 1;
            e.Graphics.DrawLine(pen, 0, y, headerPanel.Width, y);
        }

        private void RoundedPanel_Paint(object? sender, PaintEventArgs e)
        {
            // Draw a subtle rounded border
            var ctrl = (Panel)sender!;
            using var pen = new Pen(Color.FromArgb(55, 65, 80), 1);
            var rect = new Rectangle(0, 0, ctrl.Width - 1, ctrl.Height - 1);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int r = 8;
            using var path = RoundedRect(rect, r);
            e.Graphics.DrawPath(pen, path);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(bounds.Right - radius * 2, bounds.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ─── Inline name prompt ────────────────────────────────────────────────
        private void BeginInlineNamePrompt()
        {
            AppendMessage("BOT",
                "Welcome to CyberBot — your cybersecurity awareness assistant!\n" +
                "  Before we begin, what is your name?",
                AccentCyan, BotBubble);

            inputBox.PlaceholderText = "Enter your name…";
        }

        private void CompleteNamePrompt(string name)
        {
            _userName      = name;
            _awaitingName  = false;

            inputBox.PlaceholderText = "Type a message…";

            AppendMessage("BOT",
                $"Hello, {_userName}! You can ask me about passwords, phishing, malware, " +
                "safe browsing, and more. Type \"help\" to see all topics.",
                AccentCyan, BotBubble);
        }

        // ─── Quiz launch ───────────────────────────────────────────────────────
        private void QuizButton_Click(object? sender, EventArgs e)
        {
            using var quiz = new QuizForm();
            quiz.ShowDialog(this);

            AppendMessage("BOT",
                "Welcome back! Feel free to keep asking questions, or click Quiz again anytime to try for a better score.",
                AccentCyan, BotBubble);

            inputBox.Focus();
        }

        // ─── Sound ─────────────────────────────────────────────────────────────
        private static void PlayWelcomeSound()
        {
            try
            {
                string path = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "welcome.wav");

                if (System.IO.File.Exists(path))
                {
                    using var player = new SoundPlayer(path);
                    player.Play();
                }
            }
            catch { /* non-critical */ }
        }

        // ─── Input handling ────────────────────────────────────────────────────
        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                ProcessInput();
            }
        }

        private void SendButton_Click(object? sender, EventArgs e) => ProcessInput();

        private void ProcessInput()
        {
            string text = inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            inputBox.Clear();

            // ── Name collection phase ─────────────────────────────────────────
            if (_awaitingName)
            {
                AppendMessage("You", text, TextPrimary, UserBubble);
                CompleteNamePrompt(text);
                return;
            }

            // ── Normal chat phase ─────────────────────────────────────────────
            AppendMessage(_userName ?? "You", text, TextPrimary, UserBubble);

            if (text.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                AppendMessage("BOT", "Stay safe online! Goodbye.", AccentCyan, BotBubble);
                sendButton.Enabled = false;
                inputBox.Enabled   = false;
                return;
            }

            // ── Activity log lookup — purely a query, not a loggable action ────
            if (text.Equals("show log", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("activity log", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("view log", StringComparison.OrdinalIgnoreCase))
            {
                AppendMessage("BOT", ActivityLog.GetFormatted(), AccentCyan, BotBubble);
                return;
            }

            // ── Task management commands take priority (they carry their own
            //    multi-turn state for reminders / delete confirmation) ────────
            if (_taskHandler.TryHandle(text, out string taskResponse))
            {
                AppendMessage("BOT", taskResponse, AccentCyan, BotBubble);
                return;
            }

            string response = _engine.GetResponse(text.ToLower());
            AppendMessage("BOT", response, AccentCyan, BotBubble);
        }

        // ─── Chat rendering ────────────────────────────────────────────────────
        private void AppendMessage(string sender, string message, Color nameColor, Color bubbleColor)
        {
            chatDisplay.SuspendLayout();

            // Spacer
            chatDisplay.AppendText("\n");

            // Sender label
            int start = chatDisplay.TextLength;
            chatDisplay.AppendText($"  {sender}\n");
            chatDisplay.Select(start, sender.Length + 3);
            chatDisplay.SelectionColor = nameColor;
            chatDisplay.SelectionFont  = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            // Message body
            int msgStart = chatDisplay.TextLength;
            chatDisplay.AppendText($"  {message}\n");
            chatDisplay.Select(msgStart, message.Length + 3);
            chatDisplay.SelectionBackColor = bubbleColor;
            chatDisplay.SelectionColor     = TextPrimary;
            chatDisplay.SelectionFont      = new Font("Segoe UI", 10.5f);

            // Reset selection
            chatDisplay.Select(chatDisplay.TextLength, 0);
            chatDisplay.SelectionBackColor = BgDark;
            chatDisplay.SelectionColor     = TextPrimary;

            chatDisplay.ResumeLayout();
            chatDisplay.ScrollToCaret();
        }
    }
}