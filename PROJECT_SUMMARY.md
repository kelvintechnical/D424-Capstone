# Student Progress Tracker - Project Summary

## Project Purpose

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

---

## Core Functionality

### 1. Academic Management

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

### 2. Grade & GPA Management

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

### 3. Financial Tracking

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

### 4. Search & Organization

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

### 5. Reporting & Analytics

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

### 6. Notifications

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

### 7. User Authentication & Security

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

### 8. Offline & Cloud Sync

#### Offline Capabilities
- Local SQLite database for offline access
- Full functionality without internet connection
- Data persistence across app restarts

#### Cloud Synchronization
- RESTful API backend
- Automatic data synchronization
- Secure cloud storage
- Multi-device support capability

---

## Technology Stack

### Frontend Client (.NET MAUI)

**Framework & Runtime**
- **.NET 9.0 SDK** - Latest stable version
- **.NET MAUI (Multi-platform App UI)** - Cross-platform framework
- **Target Platforms:**
  - Android 5.0+ (API Level 21+)
  - Windows 10+ (version 10.0.19041.0+)
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

### Testing

**Test Framework**
- **xUnit** 2.6.1 - Unit testing framework
- **FluentAssertions** 6.12.0 - Readable assertions
- **Moq** 4.20.70 - Mocking framework
- **Microsoft.EntityFrameworkCore.InMemory** 8.0.0 - In-memory database for tests

**Test Coverage**
- GPA calculation tests
- Authentication tests
- CRUD operation tests
- Search functionality tests
- 30+ test methods covering critical business logic

### Development Tools

**IDE & Editors**
- Visual Studio 2022 (17.8+)
- Visual Studio Code (optional)
- .NET MAUI development workload
- ASP.NET and web development workload

**Build & Deployment**
- MSBuild for compilation
- NuGet package management
- Platform-specific build tools:
  - Android SDK and build tools
  - Windows SDK
  - Xcode (for iOS/macOS)

---

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

---

## Project Statistics

### Code Metrics
- **Total Source Files:** 100+ files
- **Lines of Code:** ~15,000+ lines
- **Projects:** 4 (Client, API, Shared, Tests)
- **Controllers:** 5 API controllers
- **ViewModels:** 12 ViewModels
- **Pages:** 15+ XAML pages
- **Services:** 7 business logic services
- **DTOs:** 15+ data transfer objects
- **Test Methods:** 30+ unit tests

### Database Schema
- **Client Tables:** 4 (Instructor, AcademicTerm, Course, Assessment)
- **Server Tables:** 14 (Users, Terms, Courses, Assessments, Grades, Incomes, Expenses, Categories, Identity tables)
- **Relationships:** 7 key relationships with cascade deletes
- **Indexes:** 8 performance indexes

### API Endpoints
- **Authentication:** 3 endpoints
- **Grades:** 4 endpoints
- **Search:** 3 endpoints
- **Reports:** 4 endpoints
- **Financial:** 15 endpoints (Income, Expense, Category, Summary)
- **Total:** 29 RESTful endpoints

---

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

**Phase 4: Testing & Documentation (Current)**
- Unit test suite
- Documentation completion
- Code cleanup and optimization

---

## Key Achievements

✅ **Cross-Platform Solution** - Single codebase for 4+ platforms  
✅ **Offline-First Architecture** - Full functionality without internet  
✅ **Comprehensive Feature Set** - Academic + Financial tracking  
✅ **Enterprise-Grade Security** - JWT authentication, data isolation  
✅ **Modern Architecture** - MVVM, DI, Service Layer patterns  
✅ **Production-Ready** - Error handling, logging, validation  
✅ **Well-Tested** - 30+ unit tests covering critical logic  
✅ **Fully Documented** - README, Technical Overview, Test Documentation  

---

## Target Users

- **College/University Students** - Managing coursework and grades
- **Graduate Students** - Tracking research and academic progress
- **Online Students** - Organizing remote learning
- **Part-Time Students** - Balancing work and education
- **Students with Financial Concerns** - Tracking income and expenses

---

## Deployment Platforms

### Mobile
- **Android** - Google Play Store or direct APK
- **iOS** - Apple App Store (requires developer account)

### Desktop
- **Windows** - Microsoft Store or direct installer
- **macOS** - Mac App Store or direct distribution

### Web API
- **On-Premises** - IIS on Windows Server
- **Cloud** - Azure App Service, AWS, Google Cloud
- **Container** - Docker support ready

---

## License & Dependencies

All dependencies use permissive open-source licenses:
- MIT License (MAUI, CommunityToolkit, SQLite, Swagger)
- Apache 2.0 License (ASP.NET Core, Entity Framework)
- Public Domain (SQLite)

**No proprietary or commercially-restricted dependencies**

---

## Project Status

**Current Version:** 1.0  
**Status:** ✅ **Production-Ready**  
**Last Updated:** December 2025

The application is feature-complete, well-tested, and ready for deployment. All core functionality has been implemented, tested, and documented.

---

*Built with modern .NET technologies for cross-platform academic and financial management.*

