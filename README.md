# Cybersecurity Awareness Bot

A beginner-friendly **C# Windows Forms chatbot** that promotes cybersecurity awareness and demonstrates a clean, modular codebase, simple audio playback, and a GitHub Actions CI pipeline.

---

##  Overview


The **Cybersecurity Awareness Bot** runs as a Windows desktop application (WinForms). It presents a themed chat UI, plays a welcome audio clip, and answers common cybersecurity questions using a lightweight rule-based response engine.

Highlights:

- Modern WinForms UI with custom theming and message bubbles
- Modular design: UI (`MainForm`), response logic (`ResponseEngine`), helpers (`UIHelper`, `AudioPlayer`)
- Welcome audio playback (included `welcome.wav`)
- Sentiment-aware, topic-based responses
- CI pipeline with GitHub Actions

---

## Features

- Themed Windows Forms chat UI with an ASCII header
- Inline name prompt and message bubbles for user & bot
- Plays `welcome.wav` on startup (non-blocking)
- Rule-based `ResponseEngine` (topics: passwords, phishing, malware, ransomware, etc.)
- Simple sentiment-aware prefixes/suffixes to make replies friendlier
- Clean separation of concerns across small source files
- CI with GitHub Actions ensuring build and formatting checks

---

## Supported Questions

You can ask conversational prompts or simple topics such as:

- "How are you?"
- "What's your purpose?"
- "What can I ask about?"

Topic keywords supported: `password`, `phishing`, `malware`, `ransomware`, and `help` (shows the list).

If the bot cannot match a topic it returns a friendly fallback and suggestions.

---

## Project Structure

```
POE-Part-1.sln
.github/workflows/manual.yml
CyberSecurityBot/
    ├─ Program.cs
    ├─ MainForm.cs
    ├─ ResponseEngine.cs
    ├─ UIHelper.cs
    ├─ AudioPlayer.cs
    ├─ CyberSecurityBot.csproj
    └─ welcome.wav
.editorconfig
README.md
```

---

##  Getting Started 

### 📋 Prerequisites

- Windows 11/10 with .NET SDK 10.0+ installed (project targets `net10.0-windows`)
- Git (for cloning the repository)

---

### Installation

```bash
git clone <repo-url>
cd POE-ST10465421
dotnet restore
```

---

### Run the Application

From the repository root run:

```bash
dotnet run --project CyberSecurityBot
```

Or launch the built executable (Windows):

```powershell
& .\CyberSecurityBot\bin\Debug\net10.0-windows\CyberSecurityBot.exe
```

---

## Audio

`welcome.wav` is included in the project and copied to output at build time. If it's missing, the app will still run but without the welcome sound.

---

## UI Preview

The app shows an ASCII header, a chat pane with colored message bubbles, and an input area at the bottom. On first run it asks for your name inline, then accepts topic queries like `phishing` or `password`.

---

## CI/CD with GitHub Actions

The repository includes a GitHub Actions workflow at `.github/workflows/manual.yml` that:

- Restores dependencies
- Runs `dotnet format --verify-no-changes` (format check)
- Builds the project on .NET 10

---

## Code Quality

- `.editorconfig` enforces basic formatting rules
- CI checks formatting and build correctness
- Small, single-responsibility source files for maintainability

---


## 📄 License

This project is licensed under the MIT License.

---

## 👨‍💻 Author

Developed as part of a coursework project demonstrating a small desktop application with CI.

---

## ⭐ Acknowledgements

- .NET SDK & WinForms
- GitHub Actions
- Teaching materials and open-source libraries

---


