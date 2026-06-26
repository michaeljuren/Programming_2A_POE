using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CyberSecurityBot
{
    /// <summary>
    /// Modal quiz window. Presents questions one at a time (true/false or
    /// multiple choice), gives immediate feedback with an explanation, then
    /// shows a final score summary when complete.
    /// </summary>
    public class QuizForm : Form
    {
        // Theme — mirrors MainForm
        private static readonly Color BgDark      = Color.FromArgb(13,  17,  23);
        private static readonly Color BgMid       = UIHelper.BgMid;
        private static readonly Color BgPanel     = Color.FromArgb(30,  35,  45);
        private static readonly Color AccentCyan  = UIHelper.AccentCyan;
        private static readonly Color AccentGreen = Color.FromArgb(57, 255, 107);
        private static readonly Color AccentRed   = Color.FromArgb(255, 90,  90);
        private static readonly Color TextPrimary = Color.FromArgb(230, 237, 243);
        private static readonly Color TextMuted   = Color.FromArgb(139, 148, 158);

        // Controls
        private Label progressLabel = null!;
        private Label questionLabel = null!;
        private FlowLayoutPanel optionsPanel = null!;
        private Panel feedbackPanel = null!;
        private Label feedbackLabel = null!;
        private Label explanationLabel = null!;
        private Button nextButton = null!;
        private ProgressBar progressBar = null!;

        // State
        private readonly List<QuizQuestion> _questions = QuizBank.GetQuestions();
        private int _currentIndex = 0;
        private int _correctCount = 0;
        private bool _answered = false;
        private readonly List<Button> _optionButtons = new();

        public QuizForm()
        {
            Build();
            ShowQuestion();
        }

        // ─── Build UI ──────────────────────────────────────────────────────────
        private void Build()
        {
            Text            = "CyberBot Quiz — Test Your Knowledge";
            Size            = new Size(760, 640);
            MinimumSize     = new Size(640, 520);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = BgDark;
            ForeColor       = TextPrimary;
            Font            = new Font("Segoe UI", 10f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;

            // ── Header strip ─────────────────────────────────────────────────
            var headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 70,
                BackColor = BgMid,
            };

            var titleLabel = new Label
            {
                Text      = "🛡️  Cybersecurity Quiz",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AccentCyan,
                AutoSize  = true,
                Location  = new Point(20, 10),
            };

            progressLabel = new Label
            {
                Text      = "Question 1 of 12",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TextMuted,
                AutoSize  = true,
                Location  = new Point(22, 42),
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(progressLabel);

            progressBar = new ProgressBar
            {
                Dock      = DockStyle.Top,
                Height    = 6,
                Minimum   = 0,
                Maximum   = _questions.Count,
                Value     = 0,
                Style     = ProgressBarStyle.Continuous,
            };

            // ── Question + options ───────────────────────────────────────────
            var bodyPanel = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(24, 18, 24, 18),
            };

            questionLabel = new Label
            {
                Text        = "",
                Font        = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor   = TextPrimary,
                AutoSize    = true,
                Dock        = DockStyle.Top,
                TextAlign   = ContentAlignment.TopLeft,
                MaximumSize = new Size(720, 0),
                Margin      = new Padding(0, 0, 0, 12),
            };

            optionsPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoSize      = true,
                Height        = 220,
            };

            // ── Feedback area ────────────────────────────────────────────────
            feedbackPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 110,
                BackColor = BgPanel,
                Visible   = false,
                Margin    = new Padding(0, 10, 0, 0),
                Padding   = new Padding(14, 10, 14, 10),
            };

            feedbackLabel = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 28,
            };

            explanationLabel = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TextMuted,
                AutoSize  = false,
                Dock      = DockStyle.Fill,
            };

            feedbackPanel.Controls.Add(explanationLabel);
            feedbackPanel.Controls.Add(feedbackLabel);

            bodyPanel.Controls.Add(feedbackPanel);
            bodyPanel.Controls.Add(optionsPanel);
            bodyPanel.Controls.Add(questionLabel);

            // ── Footer / Next button ─────────────────────────────────────────
            var footerPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 64,
                BackColor = BgMid,
                Padding   = new Padding(20, 12, 20, 12),
            };

            nextButton = new Button
            {
                Text      = "Next  ➤",
                Dock      = DockStyle.Right,
                Width     = 140,
                BackColor = AccentCyan,
                ForeColor = BgDark,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Enabled   = false,
            };
            nextButton.FlatAppearance.BorderSize = 0;
            nextButton.Click += NextButton_Click;

            footerPanel.Controls.Add(nextButton);

            Controls.Add(bodyPanel);
            Controls.Add(footerPanel);
            Controls.Add(progressBar);
            Controls.Add(headerPanel);
        }

        // ─── Question rendering ────────────────────────────────────────────────
        private void ShowQuestion()
        {
            _answered = false;
            feedbackPanel.Visible = false;
            nextButton.Enabled = false;
            nextButton.Text = (_currentIndex == _questions.Count - 1) ? "Finish  ➤" : "Next  ➤";

            var q = _questions[_currentIndex];

            progressLabel.Text = $"Question {_currentIndex + 1} of {_questions.Count}";
            progressBar.Value  = _currentIndex;

            string typeTag = q.Type == QuestionType.TrueFalse ? "[True / False]" : "[Multiple Choice]";
            questionLabel.Text = $"{typeTag}\n{q.Text}";

            // Clear old option buttons
            foreach (var btn in _optionButtons) btn.Dispose();
            _optionButtons.Clear();
            optionsPanel.Controls.Clear();

            for (int i = 0; i < q.Options.Count; i++)
            {
                int optionIndex = i; // capture for closure
                var btn = new Button
                {
                    Text                = $"{(char)('A' + i)}.  {q.Options[i]}",
                    Width               = (optionsPanel.ClientSize.Width > 0) ? optionsPanel.ClientSize.Width - 6 : 520,
                    Height              = 42,
                    Margin              = new Padding(0, 0, 0, 8),
                    BackColor           = BgPanel,
                    ForeColor           = TextPrimary,
                    FlatStyle           = FlatStyle.Flat,
                    TextAlign           = ContentAlignment.MiddleLeft,
                    Padding             = new Padding(12, 0, 0, 0),
                    Cursor              = Cursors.Hand,
                    Font                = new Font("Segoe UI", 10.5f),
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(55, 65, 80);
                btn.FlatAppearance.BorderSize  = 1;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 48, 60);
                btn.Click += (_, _) => OnOptionSelected(optionIndex, btn);

                _optionButtons.Add(btn);
                optionsPanel.Controls.Add(btn);
            }
        }

        private void OnOptionSelected(int selectedIndex, Button clickedButton)
        {
            if (_answered) return; // ignore further clicks after answering
            _answered = true;

            var q = _questions[_currentIndex];
            bool correct = selectedIndex == q.CorrectIndex;
            if (correct) _correctCount++;

            // Disable all options, colour-code correct/incorrect
            for (int i = 0; i < _optionButtons.Count; i++)
            {
                _optionButtons[i].Enabled = false;

                if (i == q.CorrectIndex)
                {
                    _optionButtons[i].BackColor = Color.FromArgb(20, 60, 35);
                    _optionButtons[i].FlatAppearance.BorderColor = AccentGreen;
                }
                else if (i == selectedIndex)
                {
                    _optionButtons[i].BackColor = Color.FromArgb(60, 25, 25);
                    _optionButtons[i].FlatAppearance.BorderColor = AccentRed;
                }
            }

            feedbackLabel.Text      = correct ? "✓  Correct!" : "✗  Incorrect";
            feedbackLabel.ForeColor = correct ? AccentGreen : AccentRed;
            explanationLabel.Text   = q.Explanation;
            feedbackPanel.Visible   = true;

            nextButton.Enabled = true;
        }

        private void NextButton_Click(object? sender, EventArgs e)
        {
            _currentIndex++;

            if (_currentIndex >= _questions.Count)
            {
                progressBar.Value = _questions.Count;
                ShowResults();
            }
            else
            {
                ShowQuestion();
            }
        }

        // ─── Results screen ────────────────────────────────────────────────────
        private void ShowResults()
        {
            Controls.Clear();

            double percentage = (_correctCount / (double)_questions.Count) * 100;
            bool passed = percentage > 50;

            var resultPanel = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(30),
            };

            var icon = new Label
            {
                Text      = passed ? "🏆" : "📘",
                Font      = new Font("Segoe UI", 36f),
                AutoSize  = true,
                Location  = new Point(220, 30),
            };

            var scoreLabel = new Label
            {
                Text      = $"You scored {_correctCount} / {_questions.Count}",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AccentCyan,
                AutoSize  = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Location  = new Point(0, 110),
                Width     = 540,
            };
            scoreLabel.Left = (resultPanel.Width - scoreLabel.PreferredWidth) / 2;

            var percentLabel = new Label
            {
                Text      = $"({percentage:0}% correct)",
                Font      = new Font("Segoe UI", 10.5f),
                ForeColor = TextMuted,
                AutoSize  = true,
                Location  = new Point(0, 150),
                Width     = 540,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var messageLabel = new Label
            {
                Text      = passed
                    ? "Great job! You're a cybersecurity pro!"
                    : "Keep learning to stay safe online",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = passed ? AccentGreen : Color.FromArgb(255, 190, 90),
                AutoSize  = false,
                Width     = 540,
                Height    = 40,
                Location  = new Point(0, 190),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var closeButton = new Button
            {
                Text      = "Close",
                Width     = 200,
                Height    = 42,
                Location  = new Point(170, 260),
                BackColor = AccentCyan,
                ForeColor = BgDark,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };

            // Centre icon roughly
            icon.Left = (540 - icon.PreferredWidth) / 2;

            resultPanel.Controls.Add(icon);
            resultPanel.Controls.Add(scoreLabel);
            resultPanel.Controls.Add(percentLabel);
            resultPanel.Controls.Add(messageLabel);
            resultPanel.Controls.Add(closeButton);

            Controls.Add(resultPanel);
        }
    }
}