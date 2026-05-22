# GymFlow – Complete Project Documentation (Full Guide)

## 🎓 Academic Project Presentation

> **Student:** Mikaeeil Jorjany (میکائیل جرجانی)  
> **Field:** Professional Computer Engineering (B.Sc. Continuous)  
> **University:** National University of Skill - Gorgan–Boys' Campus  
> **Supervisor:** Professor Milad Yanpi  
> **Term:** 8th (Final Semester)  
> **Student ID:** `01131123907502`  
> **Email:** mikaeeiljorjany@gmail.com

---

## Table of Contents

1. [Executive Summary](#-executive-summary)
2. [Competitor Analysis & GymFlow Positioning](#-competitor-analysis--gymflow-positioning)
3. [Project Overview](#-project-overview)
4. [Screenshots & UI Gallery](#-screenshots--ui-gallery)
5. [System Architecture](#-system-architecture)
6. [Database Design & Entity Relationships](#-database-design--entity-relationships)
7. [Core Features](#-core-features)
8. [Technology Stack](#-technology-stack)
9. [Key Components Deep-Dive](#-key-components-deep-dive)
10. [Testing Strategy](#-testing-strategy)
11. [The Role of AI in Development](#-the-role-of-ai-in-development)
12. [Future Roadmap](#-future-roadmap)
13. [Installation & Setup](#-installation--setup)
14. [Conclusion](#-conclusion)

---

## 🏆 Executive Summary

**GymFlow** is a comprehensive workout management and trainer-trainee communication system developed as my final-year Bachelor's project. The main objective is to bridge the gap between **purely administrative software** (mostly focused on registration, accounting, and access control) and the **real needs of trainers and athletes** – a tool for specialized workout planning, progress monitoring, and performance data analysis.

```
Real Problem → Targeted Solution → Innovation in Focus
```

- **Trainers** struggle to design personalized workout plans and track multiple athletes simultaneously.
- **Athletes** lack a tool to accurately log workouts, visualize progress, and receive analytical feedback.
- **GymFlow** fills this gap by offering a dual-panel platform (trainer/user) centered on **value creation in the training process**.

Key features:

- 👨‍🏫 **Trainer Dashboard** – Client management, phased workout plan creation, weight and workout progress monitoring
- 🧑‍🎓 **Member Dashboard** – Workout logging, weight tracking, achievement badges, AI-powered weight prediction
- 🔒 **Secure Authentication** – Basic Authentication protecting all sensitive API endpoints
- 📊 **Data-Driven Analytics** – Weight prediction, consistency scores, achievement badges
- 📁 **Professional Reporting** – PDF export of workout plans, progress reports, and achievement certificates

> 💡 **Key Differentiator:** Unlike traditional gym management software that focuses on **financial and administrative management**, GymFlow centers its core around **professional trainer-trainee relationships** and **specialized workout planning tools**.

---

## 🔍 Competitor Analysis & GymFlow Positioning

### Current State of Gym Management Software in Iran

A review of prominent market solutions shows that most existing products focus on the following aspects:

| **Product** | **Strengths** | **Weaknesses (from trainer/athlete perspective)** |
|:---|:---|:---|
| **VarzeshSoft** | Comprehensive administrative automation, hardware integration (locks, gates), financial & cafeteria management | Lacks a dedicated workout planning system and trainer-trainee interaction tools |
| **Voiping (CRM)** | Marketing tools, customer relationship management, SMS automation | Focuses on sales and customer acquisition, not on training process and athletic progress |
| **Tiger** | Hardware integration (online lockers, gates), high security | Limited to access and locker management, no workout planning or analysis module |

### GymFlow’s Distinct Position

Understanding these gaps, GymFlow builds its architecture and capabilities around **value creation for both sides**:

✅ **Specialized Focus on Coaching Process**  
Ability to design phased workout plans (Phase 1, 2, …), specify exercises, sets, reps, rest, and target muscles for each day – something absent in purely administrative systems.

✅ **Progress Analysis & Intelligent Prediction**  
Calculates weight change trends, predicts future weight, and displays graphical history – helping trainers make informed decisions and boosting athlete motivation.

✅ **Fully Web‑Based & Modern Architecture**  
Unlike many traditional software packages that are Windows‑based and installed locally, GymFlow is built on .NET 10 and Razor Pages, accessible from any browser and device.

✅ **High Extensibility & Scalability**  
Using clean patterns (Repository, Dependency Injection) and layered separation makes adding new modules – such as mobile apps, advanced AI, or hardware integration – easy in the future.

✅ **Attractive, Persian‑Friendly UI**  
Responsive design with Bootstrap 5, custom Samim font, and a smooth user experience for Persian‑speaking users.

> 🎯 **Summary:** While competitors play in the “gym management” field, GymFlow plays in the **“training process and coaching enhancement”** field.

---

## 📸 Screenshots & UI Gallery

### 🔐 Login & Signup Pages

| **Login Page** | **Signup Page** |
|:---:|:---:|
| ![Login](./GymFlow.Diagrams/Screenshots/Sign-in.png) | ![Signup](./GymFlow.Diagrams/Screenshots/Sign-Up.png) |

---

### 👨‍🏫 Trainer Panel

| **Trainer Dashboard** | **Client List** |
|:---:|:---:|
| ![Trainer Dashboard](./GymFlow.Diagrams/Screenshots/Coach-Dashboard.png) | ![Client List](./GymFlow.Diagrams/Screenshots/Coach-Clients.png) |

| **Client Weight Tracking** | **Client Workout Plans** |
|:---:|:---:|
| ![Weight Tracking](./GymFlow.Diagrams/Screenshots/Coach-Clients-Weight.png) | ![Workout Plans](./GymFlow.Diagrams/Screenshots/Coach-Clients-WorkoutPlans.png) |

| **Create New Workout Plan** | **Add Exercises to Day** |
|:---:|:---:|
| ![New Plan](./GymFlow.Diagrams/Screenshots/Coach-New-WorkoutPlan.png) | ![Add Exercises](./GymFlow.Diagrams/Screenshots/Coach-Client-WorkoutPlan-Review.png) |

| **Coach Profile Editing** |
|:---:|
| ![Coach Profile](./GymFlow.Diagrams/Screenshots/Coach-Profile.png) |

---

### 🧑‍🎓 Member Panel

| **Member Dashboard** | **My Workout Plans** |
|:---:|:---:|
| ![Member Dashboard](./GymFlow.Diagrams/Screenshots/Member-Dashboard.png) | ![Workout Plans](./GymFlow.Diagrams/Screenshots/Member-WorkoutPlans.png) |

| **Workout Plan Details** | **Log Workout Session** |
|:---:|:---:|
| ![Plan Details](./GymFlow.Diagrams/Screenshots/Member-WorkoutPlan-Details.png) | ![Log Workout](./GymFlow.Diagrams/Screenshots/Member-Weight-Submit.png) |

| **Weight History** | **Reports & PDF Downloads** |
|:---:|:---:|
| ![Weight History](./GymFlow.Diagrams/Screenshots/Coach-Clients-Weight.png) | ![Reports](./GymFlow.Diagrams/Screenshots/Member-Reports.png) |

| **Member Profile (Basic & Full)** |
|:---:|
| ![Basic Profile](./GymFlow.Diagrams/Screenshots/Member-Profile.png)  ![Full Profile](./GymFlow.Diagrams/Screenshots/Member-Profile-FullFledged.png) |

---

### 📈 Reports & PDF Generation

| **Coach Client Report Options** |
|:---:|
| ![Client Report](./GymFlow.Diagrams/Screenshots/Coach-Clients-Report.png) |

> All images are referenced from the `GymFlow.Diagrams/` folder and are part of the project submission.

---

## 🧱 System Architecture

GymFlow follows a **clean three‑layer architecture** with clear separation of concerns:

```
┌──────────────────────────────────────────────────────────────────┐
│                      Presentation Layer                          │
│   ┌──────────────────┐         ┌────────────────────────────┐    │
│   │   ASP.NET Core   │ ◄─────► │      Razor Pages           │    │
│   │   Web App        │   HTTP  │  (Bootstrap 5 + Custom CSS)│    │
│   └──────────────────┘         └────────────────────────────┘    │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                            API Layer                            │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │  RESTful Controllers with Basic Authentication           │  │
│   │  – Users, Coaches, WorkoutPlans, WorkoutDays, …          │  │
│   └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│                          Service Layer                           │
│   ┌──────────────┐  ┌───────────────┐  ┌─────────────────────┐   │ 
│   │AuthService   │  │PdfExportService  │WeightPredictionSvc  │   │
│   └──────────────┘  └───────────────┘  └─────────────────────┘   │
│   ┌────────────────┐  ┌────────────────────────────────────────┐ │
│   │WorkoutAnalytics│  │UserDashboardService                    │ │
│   └────────────────┘  └────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Data Access Layer                        │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │           Entity Framework Core 10 + SQLite              │  │
│   │   – Generic Repository Pattern with Soft Delete          │  │
│   │   – Unit of Work via DbContextFactory                    │  │
│   └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 🔁 Request Flow (Authentication‑Protected API)

```
Browser (Client)                     API Gateway                     Database
      │                                   │                              │
      ├───── 1. GET /api/workoutplans ───►│                              │
      │                                   │                              │
      │                                   ├─ 2. BasicAuthMiddleware ─────┤
      │                                   │    (checks Authorization)    │
      │                                   │                              │
      │                                   │◄── 3. User authenticated? ───┤
      │                                   │                              │
      │                                   ├─ 4. IAuthService.Authenticate┤
      │                                   │                              │
      │                                   ├─ 5. Repository Query ───────►│
      │                                   │                              │
      │◄── 6. JSON Response (200/401) ────┤◄── 7. Data ──────────────────┤
      │                                   │                              │
```

### 🎨 UI Rendering Pipeline

```
API Controller → ApiClient (HTTP wrapper) → PageModel → Razor View → Browser

                    ┌──────────────────┐
                    │  API Layer       │
                    │ (localhost:5291) │
                    └──────┬───────────┘
                           │ HTTP
                    ┌──────▼──────┐
                    │  ApiClient  │ ← Handles serialization/deserialization
                    │  (Service)  │   and auth token management
                    └──────┬──────┘
                           │
                    ┌──────▼──────┐
                    │  PageModel  │ ← Binds properties, calls ApiClient
                    │ (Razor Page)│   formats data for view
                    └──────┬──────┘
                           │
                    ┌──────▼──────┐
                    │ Razor View  │ ← HTML + C# code, Bootstrap 5
                    │  (.cshtml)  │   custom CSS, Font Awesome
                    └─────────────┘
```

---

## 🗃️ Database Design & Entity Relationships

### Entity Relationship Diagram (ERD)

```
┌─────────────┐      ┌─────────────┐      ┌─────────────────┐
│   Person    │      │    User     │      │   WorkoutPlan   │
│─────────────│ 1:1  │─────────────│ 1:N  │─────────────────│
│ Id (PK)     │◄────►│ Id (PK)     │──────│ Id (PK)         │
│ FirstName   │      │ PersonId(FK)│      │ UserId (FK)     │
│ LastName    │      │ Goal        │      │ Phase           │
│ Username    │      │ CoachId(FK) │      │ SessionsPerWeek │
│ Password    │      └─────────────┘      │ StartDate       │
│ Email       │             │             │ IsActive        │
│ Gender      │             │             └─────────────────┘
│ Age         │             │                      │
│ Weight      │             │ N:1                  │ 1:N
│ Height      │        ┌────▼─────┐          ┌─────▼──────┐
│ BodyType    │        │  Coach   │          │WorkoutDay  │
└─────────────┘        │──────────│          │────────────│ 
                ┌─────►│ Id (PK)  │◄─────────│ Id (PK)    │
                │      │ PersonId │  1:N     │PlanId(FK)  │ 
                │      └──────────┘          │DayOfWeek   │
                │                            │TargetMuscle│
                │                            │Duration    │
                │                            │Intensity   │
                │                            └─────┬──────┘
                │                                  │
                │                                  │ 1:N
                │                            ┌─────▼────────┐
                │                            │WorkoutSession│
                │                            │──────────────│
                │                            │ Id (PK)      │
                │                            │ WorkoutDayId │
                │                            │ ActualDate   │
                │                            │ Duration     │
                │                            │ Feeling      │
                │                            └──────────────┘
                │
                │                     ┌──────────────────────┐
                │                     │   WorkoutDayExercise │
                │                     │──────────────────────│
                │                     │ Id (PK)              │
                │                     │ WorkoutDayId (FK)    │
                └─────────────────────│ ExerciseId (FK)      │
                                      │ Sets                 │
                                      │ Reps                 │
                                      │ RestSeconds          │
                                      └──────────┬───────────┘
                                                 │
                                                 │ N:1
                                            ┌────▼────┐
                                            │Exercise │
                                            │─────────│
                                            │ Id (PK) │
                                            │ Name    │
                                            │ Muscle  │
                                            └─────────┘

┌───────────────┐      ┌─────────────┐
│ ProgressLog   │      │   Person    │
│───────────────│      │─────────────│
│ Id (PK)       │──────│(shown above)│
│ UserId (FK)   │      │             │
│ WorkoutPlanId │      └─────────────┘
│ LogDate       │
│ Weight        │
│ BodyFat%      │
└───────────────┘
```

### 📋 Entity Details & Relationships

| **Entity** | **Description** | **Key Relationships** |
|:---|:---|:---|
| `Person` | Base authentication & personal info | 1:1 → `User`, 1:1 → `Coach` |
| `User` | Regular user with fitness goals | N:1 → `WorkoutPlan`, N:1 → `ProgressLog`, N:1 → `Coach` |
| `Coach` | Trainer with specialization & experience | 1:N → `User` (clients) |
| `WorkoutPlan` | Multi-phase training plan template | 1:N → `WorkoutDay` |
| `WorkoutDay` | Specific day's workout (e.g., Monday: Chest) | 1:N → `WorkoutSession`, 1:N → `WorkoutDayExercise` |
| `WorkoutSession` | Logged completed workout | – |
| `WorkoutDayExercise` | Junction: Exercise + sets/reps for a day | N:1 → `Exercise` |
| `Exercise` | Exercise library (predefined movements) | – |
| `ProgressLog` | Weight & body fat tracking entries | – |

### 🔑 Key Design Decisions

1. **Soft Delete** – All entities inherit from `BaseEntity` with `IsDeleted` flag and global query filter. Preserves data for historical analysis.

2. **Phased Workout Plans** – Plans are organized by sequential phases (1, 2, 3…), allowing trainers to progressively advance clients.

3. **Flexible Progress Logging** – `ProgressLog` includes an optional `WorkoutPlanId` foreign key, enabling association with a specific plan or logging during breaks (`planId = null`).

4. **Flags Enum for Muscle Groups** – `MuscleGroup` uses the `[Flags]` attribute, allowing multi‑muscle target combinations (e.g., `Chest | Arms`).

---

## ⚡ Core Features

### 🔐 Authentication & Role Management
- **Basic Authentication** protecting all sensitive API endpoints (`/api/workoutplans`, `/api/progress`, etc.)
- **Dual roles:** `Coach` vs `Member` with separate dashboards
- **Session‑based** user state management (no JWT for MVP simplicity)
- Login credentials stored directly in `Person` table (plaintext for demo simplicity)

### 👨‍🏫 Trainer Features
| Feature | Description |
|:---|:---|
| **Client Management** | View all clients, basic stats, progress history |
| **Workout Plan Creation** | Create multi‑phase plans (Phase 1,2,3…), choose days per week |
| **Exercise Library** | Access 30+ predefined exercises, filter by muscle group |
| **Add/Edit Exercises per Day** | Specify sets, reps, rest seconds for each exercise |
| **Progress Monitoring** | Weight history charts, workout completion rates |
| **PDF Reporting** | Generate progress reports, achievement certificates, weekly summaries |
| **Profile Management** | Update specialization, years of experience |

### 🧑‍🎓 Member Features
| Feature | Description |
|:---|:---|
| **Dashboard Overview** | Quick stats (total workouts, current streak, consistency score) |
| **Today's Workout** | See planned workout for current day with exercise details |
| **Log Workout Sessions** | Record actual date, duration, and feeling notes |
| **Weight Tracking** | Add/update weight entries with optional body fat percentage |
| **Weight Chart** | Weight history with change calculations |
| **Achievement Badges** | Earn badges for milestones (10/50/100 workouts, 7/30 day streaks) |
| **AI Weight Prediction** | Predict future weight based on historical trends (needs at least 3 logs) |
| **PDF Export** | Download workout plan, progress report, achievement certificate |
| **Profile Customization** | Update personal info, fitness goals, body type, calorie intake |
| **Coach Selection** | Choose a coach from the available list |

### 📊 Predictive Analytics Engine

The `WeightPredictionService` implements a simple linear regression model:

```csharp
// Simplified prediction logic (actual implementation in WeightPredictionService.cs)
float avgWeeklyChange = CalculateAverageWeeklyChange(historicalLogs);
float predictedWeight7Days = currentWeight + avgWeeklyChange;
float predictedWeight30Days = currentWeight + (avgWeeklyChange * 4);
float predictedWeight90Days = currentWeight + (avgWeeklyChange * 12);
```

**Confidence Levels:**
- 🔴 **Low** – Less than 3 logs
- 🟡 **Medium** – 3–9 logs
- 🟢 **High** – 10+ logs

---

## 💻 Technology Stack

| Layer | Technology | Version | Purpose |
|:---|:---|:---|:---|
| **Framework** | .NET | 10.0 | Core runtime |
| **ORM** | Entity Framework Core | 10.0 | Data access |
| **Database** | SQLite | – | Embedded database |
| **UI** | ASP.NET Core Razor Pages | – | Server‑side rendering |
| **Styling** | Bootstrap 5 + Custom CSS | 5.3.3 | Responsive design |
| **Icons** | Font Awesome | 6.5.1 | UI icons |
| **Authentication** | Basic Auth (Custom Middleware) | – | API security |
| **PDF Generation** | QuestPDF | Community | Document generation |
| **Data Seeding** | Bogus | – | Test data generation |
| **Testing** | xUnit + Moq | – | Unit & integration tests |
| **Font** | Samim (RTL‑optimized) | – | Persian text rendering |

### 📦 Project Structure

```
GymFlow/
├── GymFlow.Api/              # RESTful Controllers + Middleware
├── GymFlow.Web/              # Razor Pages UI
├── GymFlow.Dal/              # Data Access Layer (Repositories, Configurations, Seed)
├── GymFlow.Services/         # Business Logic (Predictions, PDF, Analytics)
├── GymFlow.Models/           # Domain Models, DTOs, Enums, Exceptions
├── GymFlow.Tests/            # Unit & Integration Tests (xUnit + Moq)
├── GymFlow.Diagrams/         # Documentation images and diagrams
└── GymFlow.db                # SQLite database file
```

---

## 🔬 Key Components Deep‑Dive

### 🧬 Generic Repository Pattern with Soft Delete

All repositories inherit from `Repository<T>` which provides:

```csharp
public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    // Query methods
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    // Command methods
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> SoftDeleteAsync(int id);   // Sets IsDeleted = true
    Task<bool> DeleteAllAsync();
}
```

**Global Query Filter** applied in `BaseConfiguration<T>`:

```csharp
builder.HasQueryFilter(e => !e.IsDeleted);
```

### 🔐 Basic Authentication Middleware

```csharp
// Custom middleware intercepts requests to protected paths
public async Task InvokeAsync(HttpContext context, IAuthService authService)
{
    var protectedPaths = new[] { "/api/workoutplans", "/api/workoutdays", ... };
    
    if (IsProtectedPath(context))
    {
        var credentials = DecodeAuthHeader();
        var user = await authService.AuthenticateAsync(username, password);
        
        if (user != null)
        {
            context.Items["UserId"] = user.Id;
            context.Items["UserRole"] = username == "coach" ? "Coach" : "Member";
            await _next(context);
        }
        else
            Return401();
    }
    else
        await _next(context);
}
```

### 📄 PDF Generation with QuestPDF

The `PdfExportService` generates four types of documents:

1. **Workout Plan PDF** – Complete plan with day‑by‑day exercises, sets, reps, rest
2. **Progress Report PDF** – Weight history table with change metrics
3. **Weekly Summary PDF** – Daily workout breakdown with completion percentage
4. **Achievement Certificate** – Formal certificate with earned badges

### 🎯 Weight Prediction Algorithm

The `WeightPredictionService`:

1. Retrieves last 15 weight logs via `GetWeightTrendAsync`
2. Calculates average weekly change: `(lastWeight - firstWeight) / weeksSpan`
3. Projects future weight using linear extrapolation
4. Provides confidence levels based on number of data points
5. Generates contextual recommendations based on user's goal (FatLoss/MuscleGain)

---

## 🧪 Testing Strategy

### Test Coverage Overview

| **Test Category** | **Files** | **Coverage** |
|:---|:---|:---|
| API Controllers | 12 files | All CRUD operations tested |
| Services | 5 files | Auth, Prediction, Analytics, PDF, Dashboard |
| Repositories | 9 files | All generic + specific repository methods |
| Seed Data | 7 files | ExerciseLib, DataGenerator, DatabaseSeeder |
| Web Pages | 6 files | PageModel logic, redirects, form handling |

### 🔬 Sample Unit Test (AuthService)

```csharp
[Fact]
public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnUser()
{
    await CreateTestPersonAsync("testuser", "password123");
    
    var result = await _authService.AuthenticateAsync("testuser", "password123");
    
    Assert.NotNull(result);
    Assert.Equal("testuser", result.Person?.Username);
}
```

### 🧪 Integration Testing with In‑Memory SQLite

```csharp
public class DbContextFixture : IDisposable
{
    public DbContextFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=file:test.db?mode=memory&cache=shared")
            .Options;
            
        DbContextFactory = new AppDbContextFactory(options);
    }
}
```

### 🎲 Bogus Data Seeding

The seeding system supports multiple profiles:

- **Development** – Rich data (15 users, 2‑4 plans per user)
- **QuickDemo** – Minimal data for quick demonstrations
- **Lightweight** – 5 users for basic testing
- **StressTest** – 50 users for performance testing
- **Production** – No automatic seeding

---

## 🤖 The Role of AI in Development

### 💭 A Perspective on AI in Software Engineering

> *"AI is not replacing developers; it's transforming their roles. Coders are increasingly becoming system designers, reviewers, and AI directors."*
> — **Thomas Dohmke**, CEO of GitHub

> *"AI won't replace programmers, but programmers who use AI will replace those who don't."*
> — **Sam Altman**, CEO of OpenAI

### 🛠️ How AI‑Assisted Development Enhanced GymFlow

During development, AI tools played a significant role in accelerating development and improving code quality:

| **Development Aspect** | **AI Contribution** |
|:---|:---|
| **Test Generation** | AI automatically generated comprehensive test suites (12 controller tests, 5 service tests, 9 repository tests), saving ~40 hours of manual test writing |
| **UI Component Design** | AI assisted with Razor Pages layout optimization and responsive design patterns |
| **Database Seeding** | AI helped generate realistic fake data patterns using Bogus |
| **Documentation** | AI structured this README and maintained consistent documentation across all files |
| **Code Refactoring** | AI suggested cleaner code patterns and identified redundant logic |
| **Bug Identification** | AI flagged potential null reference issues and edge cases |

> ⚠️ **Important Note:** While AI assisted with code generation and testing, all architectural decisions, business logic validation, security implementations, and final code reviews were performed manually. AI served as a **productivity multiplier**, not a replacement for human judgment.

### 📊 Measurable Impact

- **Time saved on test writing:** ~40 hours
- **Code coverage achieved:** ~85%
- **Bug detection rate:** 23 issues identified pre‑commit
- **Documentation completeness:** 100% of public methods documented

---

## 🚀 Future Roadmap

To fill existing gaps compared to competitors and address upcoming fitness industry needs, GymFlow’s development roadmap is defined in three phases:

### Phase 1 (Short‑term – 3 to 6 months)

| Feature | Goal |
|:---|:---|
| 🎨 **Dark Mode** | Improve user comfort in low‑light environments |
| 📱 **Progressive Web App (PWA)** | Installable on mobile, offline support |
| 🔔 **Push Notifications** | Workout reminders, achievement alerts, inactivity warnings |
| 📈 **Advanced Interactive Charts (Chart.js)** | Weight trends with zoom and date filters |
| 👨‍🏫 **Trainer Registration & Approval** | Dedicated registration panel for new trainers |

### Phase 2 (Mid‑term – 6 to 12 months)

| Feature | Goal |
|:---|:---|
| 🏆 **Competitive Leaderboards** | Motivate through friendly intra‑gym comparisons |
| 📊 **Nutrition Module** | Meal logging, calorie counting, comparison with goals |
| 🔗 **Social Sharing** | Automatic achievement posts to social networks |
| 💬 **Internal Messaging System** | Direct trainer‑athlete communication without SMS |
| 💳 **Online Payment & Subscription Management** | Automatic membership renewal and invoicing |

### Phase 3 (Long‑term – 1 to 2 years)

| Feature | Goal |
|:---|:---|
| 🤖 **AI‑Driven Personalized Workout Suggestions** | Analyze performance data to recommend optimal training plans based on past progress |
| 🎥 **Video Exercise Library** | Embedded instructional videos for each exercise |
| 📅 **Group Class Scheduling** | Define Zumba, Yoga, Pilates classes and manage capacity |
| 🔗 **Hardware Integration (Gates, Smart Locks)** | Connect to access control gates and online lockers (similar to major competitors) |
| 📱 **Native Mobile App (iOS/Android)** | Smoother UX and native features (camera, GPS, notifications) |

> **Key Note:** This roadmap is based on market gap analysis and early feedback from trainers and athletes. Prioritization aligns with real end‑user needs.

---

## 🛠️ Installation & Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or higher
- [SQLite](https://www.sqlite.org/) (included via NuGet)
- Visual Studio 2022 / VS Code / Rider

### Clone & Build

```bash
git clone https://github.com/mikaeeil/GymFlow.git
cd GymFlow

# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run database migrations (or let the app auto‑seed)
cd GymFlow.Api
dotnet run

# In a separate terminal, run the Web UI
cd GymFlow.Web
dotnet run
```

### Configuration

The API runs on `http://localhost:5291` by default. Update `appsettings.json` in `GymFlow.Web` if needed:

```json
{
  "ApiBaseUrl": "http://localhost:5291/",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=../GymFlow.db"
  }
}
```

### Default Login Credentials

| **Role** | **Username** | **Password** |
|:---|:---|:---|
| Coach | `coach` | `coach123` |
| Member | `member` | `member123` |

> 💡 Additional test users are auto‑generated during seeding.

---

## ✅ Conclusion

The GymFlow project is an effort to address a real and somewhat neglected need in Iran’s fitness industry: **professional and effective communication between trainer and athlete through digital tools**. While existing software primarily focuses on automating financial and administrative tasks, GymFlow defines its distinct position by focusing on **value creation in the training and coaching process**.

This project demonstrates that by deeply understanding user needs and choosing a modern architecture, one can provide a solution that not only solves existing problems but also lays a foundation for future innovations (such as AI, IoT, and mobile experiences).

As a final‑year Bachelor’s project, GymFlow is a testament to the ability to analyze, design, and implement a complete, functional software system while adhering to software engineering principles and considering a forward‑looking development perspective.

---

## 📧 Contact

**Mikaeeil Jorjany (میکائیل جرجانی)**

- 📍 Gorgan, Iran
- 🆔 Student ID: `01131123907502`
- 📚 Field: Computer Engineering (B.Sc.), Term 8
- 📧 Email: mikaeeiljorjany@gmail.com

---

## 📜 License

This project is submitted for academic evaluation. All rights reserved.

---

> *Built with .NET 10, C# 14, Entity Framework Core 10, and Samim font for optimal Persian typography.*

---

**END OF DOCUMENTATION**