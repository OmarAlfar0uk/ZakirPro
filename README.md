<div align="center">

# 🧠 ZakirPro
### Enterprise Examination Management & Automated Assessment Platform

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Background Worker](https://img.shields.io/badge/Worker-Hosted_Services-orange?style=for-the-badge&logo=dotnet&logoColor=white)](#-background-services)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20%26%20Modular-blue?style=for-the-badge&logo=diagram-project&logoColor=white)](#-system-architecture)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellowgreen?style=for-the-badge)](LICENSE)
[![Author](https://img.shields.io/badge/Author-Omar%20Alfarouk-orange?style=for-the-badge&logo=github&logoColor=white)](https://github.com/OmarAlfar0uk)

<p align="center">
  <a href="#-key-features">Key Features</a> •
  <a href="#-background-services">Background Services</a> •
  <a href="#-tech-stack">Tech Stack</a> •
  <a href="#-project-structure">Project Structure</a> •
  <a href="#-getting-started">Getting Started</a> •
  <a href="#-author">Author</a>
</p>

</div>

---

## 📌 Executive Overview

**ZakirPro** is a comprehensive educational testing and assessment management system architected for schools, universities, and certification bodies. Built on ASP.NET Core with strict adherence to Clean Architecture, ZakirPro automates the end-to-end lifecycle of student examinations—from randomized question generation and timed test-taking sessions to real-time auto-submission and instant grading.

> [!NOTE]
> Features an autonomous **`ExamAutoSubmitService`** background worker running on .NET's `IHostedService`, guaranteeing timed exam integrity even if students disconnect or navigate away.

---

## ✨ Key Features

| ⚡ Feature | 💡 Description | 🛠 Engineering Detail |
|---|---|---|
| **⏱️ Autonomous Exam Auto-Submit** | Background enforcement of strict test expiration times | Background worker (`ExamAutoSubmitService`) auditing active sessions |
| **🔐 Stateless JWT Authentication** | Granular role-based authorization for Students & Instructors | Custom `JwtService` issuing cryptographic tokens with claim enforcement |
| **📁 Secure File Storage Service** | Attachments, diagrams, and exam resource upload | Abstracted `IFileService` with content-type verification and hashing |
| **📧 Automated Email Dispatch** | Exam invitations, submission receipts, and report cards | Integrated with MailKit via `MailKitEmailService` for asynchronous delivery |
| **📊 Real-Time Analytics & Scoring** | Immediate automated evaluation of multiple-choice & numerical questions | Mathematical calculation engine with audit log retention |

---

## ⚙️ Background Services

The core differentiator in ZakirPro is its resilient background worker orchestration:

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant API as ZakirPro API
    participant Worker as ⚙️ ExamAutoSubmitService
    participant DB as SQL Server Database
    participant Email as 📧 MailKit Service

    Student->>API: Start Exam Session (Timer Begins)
    API->>DB: Record ExamSession (StartTime, ExpiryTime, InProgress)
    Note over Worker: Background Loop Checks Every N Seconds
    Worker->>DB: Query Expired In-Progress Sessions
    DB-->>Worker: Return Expired Sessions
    Worker->>DB: Auto-Submit & Calculate Partial Score
    Worker->>Email: Dispatch Completion Receipt to Student
    Email-->>Student: Deliver Grade & Feedback Notification
```

---

## ⚡ Tech Stack

| Category | Technology | Purpose |
|---|---|---|
| **Core Platform** | ![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![C#](https://img.shields.io/badge/C%23_12-239120?style=flat-square&logo=csharp&logoColor=white) | High-performance Web API and asynchronous task host |
| **Background Processing** | ![Hosted Service](https://img.shields.io/badge/IHostedService-Background_Worker-orange?style=flat-square) | Scheduled cron and background exam session evaluation |
| **Database & ORM** | ![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![SQL Server](https://img.shields.io/badge/MS_SQL_Server-CC292B?style=flat-square&logo=microsoftsqlserver&logoColor=white) | Relational model storage, migrations, and transactional updates |
| **Security & Identity** | ![JWT](https://img.shields.io/badge/JWT-Authentication-black?style=flat-square&logo=jsonwebtokens&logoColor=white) | Secure token management via `ITokenService` / `JwtService` |
| **Mailing** | ![MailKit](https://img.shields.io/badge/MailKit-SMTP-blue?style=flat-square) | Production-grade MIME-based email transport |

---

## 📂 Project Structure

```text
ZakirPro/
├── ZakirPro/
│   ├── Common/
│   │   ├── Abstractions/         # Contracts (ITokenService, IEmailService, IFileService)
│   │   ├── BackgroundServices/   # ExamAutoSubmitService (IHostedService worker)
│   │   ├── Extensions/           # Dependency injection & service registration
│   │   └── Infrastructure/       # Concrete implementations (JwtService, MailKitEmailService)
│   ├── Controllers/              # RESTful API Controllers
│   ├── Data/                     # ApplicationDbContext, Model Configurations & Migrations
│   ├── Models/                   # Domain Entities (Exam, Question, Submission, User)
│   └── Program.cs                # Entry point, services pipeline, hosted service registration
└── ZakirPro.sln                  # Visual Studio Solution
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB or Docker)

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/OmarAlfar0uk/ZakirPro.git
   cd ZakirPro
   ```

2. **Restore & Build Solution:**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run the Application:**
   ```bash
   dotnet run --project ZakirPro
   ```

---

## 👨‍💻 Author

**Omar Alfarouk**  
*Full-Stack .NET & Software Engineer*  

- 🌐 **GitHub:** [@OmarAlfar0uk](https://github.com/OmarAlfar0uk)
- 💼 **LinkedIn:** [omar-alfarouk](https://www.linkedin.com/in/omar-alfarouk-252471251/)
- 📧 **Email:** [omaralfarouk646@gmail.com](mailto:omaralfarouk646@gmail.com)

---

<div align="center">
  <sub>Built with ❤️ by Omar Alfarouk. Licensed under the <a href="LICENSE">MIT License</a>.</sub>
</div>
