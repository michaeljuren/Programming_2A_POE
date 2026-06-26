# Cybersecurity Awareness Bot

A beginner-friendly C# Windows Forms application that teaches cybersecurity awareness through a conversational chatbot, a knowledge quiz, and simple task-management features.

---

## Overview

The Cybersecurity Awareness Bot runs as a Windows desktop app and combines a polished chat interface with practical cybersecurity guidance. It welcomes the user, answers common security questions, offers a short quiz, and can help manage small reminder-based tasks.

### Recent improvements

- A more polished WinForms interface with themed chat bubbles and an ASCII header
- Inline name prompt on first launch
- Interactive cybersecurity quiz with scoring and explanations
- Task-management commands such as adding, viewing, and deleting tasks
- Reminder flow for tasks with follow-up questions
- Activity log view for recent bot actions
- Welcome audio playback on startup

---

## Features

- Themed Windows Forms chat UI with a custom header and message bubbles
- Inline name prompt for a more conversational experience
- Rule-based response engine for topics such as passwords, phishing, malware, ransomware, and safe browsing
- Quiz mode with multiple-choice and true/false questions
- Task commands including add, view, and delete actions
- Reminder prompts for tasks and a simple activity log
- Welcome sound playback with graceful fallback if the audio file is missing

---

## Supported interactions

You can ask the bot questions such as:

- "How are you?"
- "What is your purpose?"
- "What can I ask about?"
- "Help"
- "Show log"

You can also try task commands such as:

- "Add task - Review password settings"
- "View tasks"
- "Delete task 2"

---

## Project structure

```text
POE-Part-1.sln
.github/workflows/manual.yml
CyberSecurityBot/
    ├─ ActivityLog.cs
    ├─ AppSettings.json
    ├─ AudioPlayer.cs
    ├─ MainForm.cs
    ├─ Program.cs
    ├─ QuizBank.cs
    ├─ QuizForm.cs
    ├─ ResponseEngine.cs
    ├─ TaskCommandHandler.cs
    ├─ TaskItem.cs
    ├─ TaskRepository.cs
    ├─ UIHelper.cs
    ├─ CyberSecurityBot.csproj
    └─ welcome.wav
.editorconfig
README.md
```

---

## Getting started

### Prerequisites

- Windows 10/11
- .NET SDK compatible with the project target (the app targets Windows .NET)
- Git for cloning the repository

### Installation

```bash
git clone <repo-url>
cd Programming_2A_POE
dotnet restore
```

### Run the application

```bash
dotnet run --project CyberSecurityBot
```

You can also run the compiled executable from the build output folder on Windows.

---

## Audio

The file welcome.wav is included in the project and is played at startup when available. If it is missing, the application still runs normally.

---

## CI/CD

The repository includes a GitHub Actions workflow at .github/workflows/manual.yml that restores dependencies, checks formatting, and builds the project.

---

## License

This project is licensed under the MIT License.


