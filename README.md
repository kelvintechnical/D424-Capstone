# Student Progress Tracker

A comprehensive, cross-platform academic management application built with .NET MAUI and ASP.NET Core. Track courses, assessments, grades, calculate GPA, and manage your academic progress across Android, Windows, iOS, and macOS platforms.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [System Architecture](#system-architecture)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Setup Instructions](#setup-instructions)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Project Deliverables](#project-deliverables)
- [Development Timeline](#development-timeline)
- [Testing](#testing)
- [Target Platforms](#target-platforms)
- [Troubleshooting](#troubleshooting)
- [Security Notes](#security-notes)
- [License](#license)
- [Support](#support)

## Overview

**Student Progress Tracker** is a comprehensive academic management application designed to help students organize, track, and manage their educational journey. The application provides a centralized platform for students to monitor their academic progress, manage courses and assessments, calculate grades and GPA, track finances, and stay organized with notifications and reminders.

### Problem Statement

Students face challenges managing:
- Multiple academic terms and courses across semesters
- Course schedules, instructor information, and assessment deadlines
- Grade calculations and GPA tracking
- Financial planning during their education
- Staying organized and meeting important deadlines

### Solution

A unified, cross-platform application that consolidates all academic and financial management needs into a single, intuitive interface accessible on mobile devices and desktop computers.

## Features

### Academic Management

#### Term Management
- Create, edit, and delete academic terms (semesters, quarters, etc.)
- Track term start and end dates
- Organize courses by term
- View term summaries with course counts

#### Course Management
- **Course Information:**
  - Course title and description
  - Start and end dates
  - Credit hours
  - Course status (In Progress, Completed, Dropped, Plan to Take)
  - Current grade tracking
  - Letter grade calculation

- **Instructor Management:**
  - Instructor name, email, and phone number
  - Quick contact access
  - Instructor information per course

- **Course Features:**
  - Add, edit, and delete courses
  - Associate courses with academic terms
  - Track course progress and status
  - Optional course notes
  - Share course details

#### Assessment Management
- **Assessment Types:**
  - Objective assessments (tests, quizzes, exams)
  - Performance assessments (projects, papers, presentations)

- **Assessment Features:**
  - Assessment name and type
  - Start date and due date tracking
  - Link assessments to parent courses
  - Notification scheduling for deadlines
  - Edit and delete assessments
  - Share assessment details

### Grade & GPA Management

#### GPA Calculation
- **Per-Term GPA:**
  - Calculate GPA for individual terms
  - Weighted by credit hours
  - Support for plus/minus grades (A+, A, A-, B+, B, B-, etc.)
  - Letter grade to GPA point conversion (A=4.0 to F=0.0)

- **Cumulative GPA:**
  - Overall GPA across all terms
  - Total credit hours tracking
  - Grade history

#### Grade Projection Tool
- Calculate required final exam score to achieve target grade
- Input: current grade percentage, final exam weight, target letter grade
- Output: score needed on final exam
- Achievability indicator (shows if target is mathematically possible)

#### Grade Management
- Add and update grades per course
- Support for letter grades and percentage grades
- Credit hour specification
- Grade history tracking

### Financial Tracking

#### Income Management
- Track income sources (jobs, scholarships, grants, etc.)
- Record income amounts and dates
- Date range filtering
- Income history and summaries

#### Expense Management
- Track expenses with descriptions
- Categorize expenses by custom categories
- Record expense amounts and dates
- Filter by category and date range

#### Category Management
- Create custom expense categories
- Edit and delete categories
- Category validation (prevents deletion if expenses exist)

#### Financial Summary
- Total income and expenses for date ranges
- Net amount calculation (income - expenses)
- Transaction counts
- Date range filtering

### Search & Organization

#### Search Capabilities
- **Course Search:**
  - Search by course title
  - Search by instructor name
  - Search by instructor email
  - Filter by course status
  - Filter by term

- **Term Search:**
  - Search terms by title
  - View term details

- **Global Search:**
  - Search across all terms and courses simultaneously
  - Unified results with categorization
  - Results sorted alphabetically

#### Search Features
- Case-insensitive partial matching
- Real-time search results
- Result categorization (Term, Course)
- Navigation to search results
- Parent relationship display

### Reporting & Analytics

#### GPA Reports
- Term-based GPA reports
- Course list with grades
- Credit hour summary
- GPA calculation breakdown
- Export to CSV format

#### Transcript Reports
- Complete academic transcript
- All terms and courses
- Cumulative GPA
- Total credit hours
- Export to CSV format

### Notifications

#### Notification Types
- Course start date reminders
- Course end date reminders
- Assessment start date reminders
- Assessment due date reminders

#### Notification Features
- Per-course notification toggle
- Per-assessment notification toggle
- Platform-specific support (Android, iOS)
- Scheduled local notifications
- Permission request handling
- Test notification functionality

### User Authentication & Security

#### Authentication
- Email/password-based registration
- Secure login with JWT tokens
- Token refresh mechanism
- Password requirements enforcement
- Unique email validation

#### Security Features
- JWT (JSON Web Token) authentication
- Secure password hashing (PBKDF2)
- Encrypted credential storage
- User-specific data isolation
- HTTPS enforcement
- Refresh token rotation

### Offline & Cloud Sync

#### Offline Capabilities
- Local SQLite database for offline access
- Full functionality without internet connection
- Data persistence across app restarts

#### Cloud Synchronization
- RESTful API backend
- Automatic data synchronization
- Secure cloud storage
- Multi-device support capability

## Technology Stack

### Frontend Client (.NET MAUI)

**Framework & Runtime**
- **.NET 9.0 SDK** - Latest stable version
- **.NET MAUI (Multi-platform App UI)** - Cross-platform framework
- **Current Target Platform:**
  - Windows 10+ (version 10.0.19041.0+) - Primary development platform
- **Supported Platforms (configurable):**
  - Android 5.0+ (API Level 21+)
  - iOS 13.0+
  - macOS 10.15+

**Key NuGet Packages**
- `Microsoft.Maui.Controls` 9.0.0 - UI framework and controls
- `CommunityToolkit.Mvvm` 8.3.2 - MVVM pattern helpers
- `CommunityToolkit.Maui` 9.1.1 - Additional UI controls and behaviors
- `sqlite-net-pcl` 1.9.172 - Local SQLite database
- `Plugin.LocalNotification` 12.0.2 - Push notification support

**Architecture Patterns**
- **MVVM (Model-View-ViewModel)** - Separation of concerns
- **Dependency Injection** - Service registration and resolution
- **Service-based Architecture** - Business logic in services

**UI Technology**
- XAML for declarative UI
- Data binding with MVVM
- Platform-specific implementations
- Responsive layouts

### Backend API (ASP.NET Core)

**Framework & Runtime**
- **.NET 8.0 (LTS)** - Long-term support version
- **ASP.NET Core Web API** - RESTful API framework
- **C# 12** - Latest C# language features
- **Nullable Reference Types** - Enhanced type safety

**Key NuGet Packages**
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.0 - User management
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0 - JWT authentication
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0.0 - Database ORM
- `Swashbuckle.AspNetCore` 6.6.2 - Swagger/OpenAPI documentation

**Architecture Patterns**
- **RESTful API Design** - Standard HTTP methods and status codes
- **Repository Pattern** - Via Entity Framework Core
- **Service Layer** - Business logic separation
- **Dependency Injection** - Built-in IoC container

**API Features**
- JWT-based authentication
- Role-based authorization
- Swagger/OpenAPI documentation
- CORS support
- Error handling and logging

### Database Technologies

**Client Database (SQLite)**
- **SQLite 3** - Embedded, file-based database
- Async operations for responsive UI
- Auto-migrations
- Local data persistence
- Offline-first architecture

**Server Database (SQL Server)**
- **Microsoft SQL Server** - Enterprise database
- **SQL Server LocalDB** - Development database
- **Azure SQL Database** - Cloud deployment support
- Entity Framework Core ORM
- Code-first migrations
- Relationship configuration with cascade deletes

### Shared Libraries

**StudentLifeTracker.Shared**
- **.NET 8.0** - Shared library
- **DTOs (Data Transfer Objects)** - 15+ DTOs for type-safe API communication
- Type-safe data contracts
- Shared between client and server

## System Architecture

### 3-Tier Architecture

```
┌─────────────────────────────────────┐
│   Client Layer (.NET MAUI)          │
│   - XAML UI                          │
│   - ViewModels (MVVM)                │
│   - Local SQLite Database            │
│   - Services                         │
└──────────────┬──────────────────────┘
               │ HTTPS/JSON
               │ JWT Authentication
┌──────────────▼──────────────────────┐
│   API Layer (ASP.NET Core)          │
│   - RESTful Controllers              │
│   - JWT Authentication               │
│   - Business Logic Services          │
│   - Entity Framework Core            │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Data Layer                         │
│   - SQL Server (Cloud)               │
│   - SQLite (Client)                  │
│   - Shared DTOs                      │
└─────────────────────────────────────┘
```

### Key Architectural Principles

- **Separation of Concerns** - Clear boundaries between layers
- **Dependency Injection** - Loose coupling and testability
- **MVVM Pattern** - UI logic separation
- **Repository Pattern** - Data access abstraction
- **Service Layer** - Business logic encapsulation
- **Offline-First** - Local database with cloud sync

## Prerequisites

### Required Software

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** (17.8 or later) with:
  - .NET Multi-platform App UI development workload
  - ASP.NET and web development workload
  - Mobile development with .NET workload
- **SQL Server** (or SQL Server LocalDB for development)
- **Android SDK** (for Android development)
- **Windows SDK 10.0.19041.0+** (for Windows development)

### Optional (for iOS/macOS)

- **Xcode** (Mac only, for iOS/macOS builds)
- **Apple Developer Account** (for device deployment)

## Project Structure

```
StudentProgressTracker/
├── StudentProgressTracker/          # .NET MAUI Client Application
│   ├── Models/                      # Data models
│   ├── ViewModels/                  # MVVM ViewModels
│   ├── Views/                       # XAML pages
│   ├── Services/                    # Business logic services
│   ├── Helpers/                     # Utility classes
│   └── Resources/                   # Images, fonts, styles
├── StudentLifeTracker.API/          # ASP.NET Core Web API
│   ├── Controllers/                 # API endpoints
│   ├── Models/                     # Entity models
│   ├── Services/                   # Business services
│   ├── Data/                       # DbContext
│   └── appsettings.json            # Configuration
├── StudentLifeTracker.Shared/       # Shared DTOs library
│   └── DTOs/                       # Data transfer objects
└── StudentProgressTracker.Tests/    # xUnit test project
    ├── Controllers/                # Controller tests
    ├── Services/                   # Service tests
    └── Helpers/                    # Test utilities
```

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd "D424 Capstone"
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

Or restore from Visual Studio:
- Right-click the solution → **Restore NuGet Packages**

### 3. Configure the API

#### Database Connection

Edit `StudentLifeTracker.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StudentLifeTrackerDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

For production, use a full SQL Server connection string:
```json
"DefaultConnection": "Server=your-server;Database=StudentLifeTrackerDb;User Id=your-user;Password=your-password;"
```

#### JWT Settings

Update JWT configuration in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!",
    "Issuer": "StudentLifeTracker",
    "Audience": "StudentLifeTracker",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

**Important:** Change the `SecretKey` to a secure random string in production!

### 4. Configure the Client

Edit `Services/ApiService.cs` and update the API base URL:

```csharp
private readonly string _baseUrl = "https://localhost:7119"; // Change to your API URL
```

For production, use your deployed API URL:
```csharp
private readonly string _baseUrl = "https://your-api-domain.com";
```

### 5. Initialize the Database

The API will automatically create the database on first run using `EnsureCreated()`. For production, use Entity Framework migrations:

```bash
cd StudentLifeTracker.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Running the Application

### Step 1: Start the API

1. Open the solution in Visual Studio
2. Set `StudentLifeTracker.API` as the startup project
3. Press F5 or click **Run**
4. The API will start on `https://localhost:7119` (or the port configured in `launchSettings.json`)
5. Swagger UI will be available at `https://localhost:7119/swagger`

### Step 2: Run the Client Application

1. Set `StudentProgressTracker` as the startup project
2. Select your target platform:
   - **Windows Machine** - for Windows desktop
   - **Android Emulator** - for Android (create an emulator first)
   - **iOS Simulator** - for iOS (Mac only)
3. Press F5 or click **Run**

### First-Time Setup

1. **Register a new account** using the Register page
2. **Login** with your credentials
3. **Create an academic term** to get started
4. **Add courses** to your term
5. **Add assessments** to your courses

## API Endpoints

### Authentication (`/api/auth`)

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token

### Grades (`/api/grades`)

- `POST /api/grades` - Add or update grade
- `GET /api/grades/term/{termId}` - Get all grades for term
- `GET /api/grades/gpa/{termId}` - Calculate term GPA
- `GET /api/grades/projection/{courseId}` - Calculate grade projection

### Search (`/api/search`)

- `GET /api/search/courses` - Search courses (query, status, termId filters)
- `GET /api/search/terms` - Search terms
- `GET /api/search/all` - Global search across all entities

### Reports (`/api/reports`)

- `GET /api/reports/gpa/{termId}` - Get GPA report (JSON)
- `GET /api/reports/gpa/{termId}/csv` - Download GPA report (CSV)
- `GET /api/reports/transcript` - Get full transcript (JSON)
- `GET /api/reports/transcript/csv` - Download transcript (CSV)

### Financial (`/api/financial`)

#### Income
- `GET /api/financial/income` - Get all incomes (with optional date filters)
- `GET /api/financial/income/{id}` - Get income by ID
- `POST /api/financial/income` - Create new income
- `PUT /api/financial/income/{id}` - Update income
- `DELETE /api/financial/income/{id}` - Delete income

#### Expenses
- `GET /api/financial/expense` - Get all expenses (with optional date/category filters)
- `GET /api/financial/expense/{id}` - Get expense by ID
- `POST /api/financial/expense` - Create new expense
- `PUT /api/financial/expense/{id}` - Update expense
- `DELETE /api/financial/expense/{id}` - Delete expense

#### Categories
- `GET /api/financial/category` - Get all categories
- `GET /api/financial/category/{id}` - Get category by ID
- `POST /api/financial/category` - Create new category
- `PUT /api/financial/category/{id}` - Update category
- `DELETE /api/financial/category/{id}` - Delete category

#### Summary
- `GET /api/financial/summary` - Get financial summary (with optional date range)

**Note:** All endpoints except `/api/auth/*` require JWT authentication. Include the token in the `Authorization` header: `Bearer <your-token>`

## Project Deliverables

Project deliverables for the Student Progress Tracker application support planning, quality assurance, and long-term maintainability throughout the development lifecycle. These artifacts provide structure for development activities while ensuring the application can be tested, deployed, and maintained effectively.

### Planning Deliverables

**Project Schedule**
- Development timeline with milestones, dependencies, and task assignments (see Development Timeline section)
- Clear visibility into progress and coordination across development activities
- Phased approach: Foundation → Backend & Authentication → Advanced Features → Testing & Documentation
- Total development time: ~3 weeks (November 26 - December 12, 2025)

### Quality Assurance Deliverables

**xUnit Test Project**
- **Location:** `StudentProgressTracker.Tests/`
- **Test Framework:** xUnit 2.6.1 with FluentAssertions 6.12.0 and Moq 4.20.70
- **Test Coverage:** 30+ unit tests covering:
  - GPA calculation tests (weighted by credit hours, plus/minus grades)
  - Authentication workflows (registration, login, token refresh)
  - CRUD operations (Terms, Courses, Assessments with cascade deletes)
  - Search functionality (courses by title/instructor/email, terms, global search)
- **Test Execution:** Tests use Entity Framework InMemory database for isolation and speed
- **Test Documentation:** Comprehensive test execution results and coverage details documented in `StudentProgressTracker.Tests/README.md`

### Technical Documentation

**TECHNICAL_OVERVIEW.md**
- Comprehensive technical documentation describing:
  - System architecture (3-tier client-server architecture)
  - Application Programming Interface (API) endpoints (29 RESTful endpoints)
  - Data models and database schemas (SQLite client, SQL Server server)
  - Service implementations and business logic
  - Security features and authentication mechanisms
  - Deployment and scalability considerations
  - Technology stack and dependencies

**Interactive API Documentation**
- **Swagger/OpenAPI** documentation available when backend service is running
- Accessible at `/swagger` endpoint in development mode
- Interactive API testing and exploration interface
- Complete endpoint documentation with request/response schemas

### User Documentation

**README.md**
- Setup instructions for building and running the application
- Configuration guidance for API and client
- Prerequisites and development environment requirements
- Troubleshooting information for common issues
- API endpoint reference
- Platform-specific deployment instructions

**Test Suite README**
- **Location:** `StudentProgressTracker.Tests/README.md`
- Test execution results and coverage details
- Test structure and organization
- Running instructions and coverage reporting
- Test statistics and helper utilities

### Documentation Artifacts

All documentation is maintained in the project repository:
- `README.md` - User-facing setup and configuration guide (this file)
- `TECHNICAL_OVERVIEW.md` - Comprehensive technical architecture documentation
- `StudentProgressTracker.Tests/README.md` - Test suite documentation and coverage details

## Development Timeline

**Total Development Time:** ~3 weeks (November 26 - December 12, 2025)

**Phase 1: Foundation (Week 1)**
- Project setup and infrastructure
- Core models and database
- Basic UI pages
- Notification system

**Phase 2: Backend & Authentication (Week 2)**
- RESTful API development
- JWT authentication
- Cloud database setup
- Client-server integration

**Phase 3: Advanced Features (Week 3)**
- GPA calculation and reporting
- Search functionality
- Financial tracking
- CSV export capabilities

**Phase 4: Testing & Documentation**
- Unit test suite
- Documentation completion
- Code cleanup and optimization

**Phase 5: Recent Updates (December 2025)**
- Fixed XAML compiled binding errors (IncomePage)
- Fixed XAML resource converter name mismatches (InvertedBoolConverter → InverseBoolConverter)
- Simplified project configuration to Windows-only for streamlined development
- Enhanced error logging and window activation handling
- Resolved project cross-contamination issues with proper MSBuild exclusions

## Testing

### Test Framework

- **xUnit** 2.6.1 - Unit testing framework
- **FluentAssertions** 6.12.0 - Readable assertions
- **Moq** 4.20.70 - Mocking framework
- **Microsoft.EntityFrameworkCore.InMemory** 8.0.0 - In-memory database for tests

### Test Coverage

- GPA calculation tests
- Authentication tests
- CRUD operation tests
- Search functionality tests
- 30+ test methods covering critical business logic

### Running Tests

#### From Command Line
```bash
dotnet test
```

#### From Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Run All Tests (Ctrl+R, A)

#### With Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

For detailed test documentation, see `StudentProgressTracker.Tests/README.md`.

## Target Platforms

### Windows
- **Minimum:** Windows 10 version 10.0.19041.0
- **Package Type:** Unpackaged executable or MSIX
- **Deployment:** Direct installer or Microsoft Store

### Android
- **Minimum:** Android 5.0 (API Level 21)
- **Package:** APK or AAB
- **Deployment:** Google Play Store or direct APK installation

### iOS
- **Minimum:** iOS 13.0
- **Package:** IPA
- **Deployment:** Apple App Store (requires Apple Developer account)
- **Build Requirement:** Mac with Xcode

### macOS
- **Minimum:** macOS 10.15
- **Package:** APP bundle
- **Deployment:** Mac App Store or direct distribution
- **Build Requirement:** Mac with Xcode

## Troubleshooting

### API Won't Start

1. Check SQL Server is running
2. Verify connection string in `appsettings.json`
3. Check port 7119 is not in use (or change in `launchSettings.json`)
4. Review error logs in the console

### Client Can't Connect to API

1. Verify API is running and accessible
2. Check API base URL in `ApiService.cs`
3. For Android emulator, use `http://10.0.2.2:7119` instead of `localhost`
4. Check firewall settings

### Database Errors

1. Ensure SQL Server LocalDB is installed (for development)
2. Verify connection string is correct
3. Check database permissions
4. Try deleting and recreating the database

### Build Errors

1. Restore NuGet packages: `dotnet restore`
2. Clean solution: `dotnet clean`
3. Rebuild: `dotnet build`
4. Check .NET SDK versions match `global.json`

## Security Notes

- **JWT Secret Key:** Must be changed in production
- **HTTPS:** Required for production deployments
- **Connection Strings:** Never commit production connection strings to source control
- **API Keys:** Store securely using environment variables or Azure Key Vault

## License

This project uses open-source dependencies with permissive licenses:
- .NET MAUI - MIT License
- ASP.NET Core - Apache 2.0 License
- CommunityToolkit - MIT License
- SQLite - Public Domain

**No proprietary or commercially-restricted dependencies**

## Support

For technical questions or issues:
- Review the `TECHNICAL_OVERVIEW.md` for detailed architecture documentation
- Check Swagger UI at `/swagger` for API documentation
- Review inline code comments for implementation details
- See `StudentProgressTracker.Tests/README.md` for test documentation

## Version

- **Application Version:** 1.0
- **.NET MAUI:** 9.0.0
- **ASP.NET Core:** 8.0.0
- **Last Updated:** December 2025

---

**Built with:** .NET MAUI, ASP.NET Core, Entity Framework Core, SQL Server, SQLite
