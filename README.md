# Student Progress Tracker

A modern, cross-platform academic management application built with .NET MAUI and ASP.NET Core. Track courses, assessments, grades, calculate GPA, and manage your academic progress across Android, Windows, iOS, and macOS platforms.

## Features

- **Academic Management**
  - Create and manage academic terms
  - Track courses with instructor details
  - Manage assessments (Objective and Performance)
  - Calculate GPA with grade projections
  - Generate academic reports and transcripts

- **Financial Tracking**
  - Track income and expenses
  - Manage expense categories
  - View financial summaries with date filtering

- **Search & Organization**
  - Search courses and terms
  - Filter by status and date ranges
  - Global search across all entities

- **Notifications**
  - Push notifications for course start/end dates
  - Assessment due date reminders
  - Platform-specific notification support (Android/iOS)

- **Cloud Sync**
  - RESTful API backend
  - Secure JWT authentication
  - Offline-first with local SQLite database
  - Automatic cloud synchronization

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
└── StudentLifeTracker.Shared/       # Shared DTOs library
    └── DTOs/                       # Data transfer objects
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

## Development

### Building the Solution

```bash
dotnet build
```

### Running Tests

Currently, no unit tests are included. The architecture supports easy testability:
- Services use dependency injection
- ViewModels can be tested independently
- API controllers are testable via integration tests

### Code Structure

- **MVVM Pattern:** All UI logic in ViewModels
- **Service Layer:** Business logic separated into services
- **Repository Pattern:** Entity Framework acts as repository
- **Dependency Injection:** All services registered in `MauiProgram.cs`

## Configuration Files

### API Configuration

- `StudentLifeTracker.API/appsettings.json` - Production settings
- `StudentLifeTracker.API/appsettings.Development.json` - Development settings
- `StudentLifeTracker.API/Properties/launchSettings.json` - Launch profiles

### Client Configuration

- `StudentProgressTracker.csproj` - Project configuration
- `global.json` - SDK version specification
- `MauiProgram.cs` - Dependency injection and app configuration

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

## Support

For technical questions or issues:
- Review the `TECHNICAL_OVERVIEW.md` for detailed architecture documentation
- Check Swagger UI at `/swagger` for API documentation
- Review inline code comments for implementation details

## Version

- **Application Version:** 1.0
- **.NET MAUI:** 9.0.0
- **ASP.NET Core:** 8.0.0
- **Last Updated:** December 2025

---

**Built with:** .NET MAUI, ASP.NET Core, Entity Framework Core, SQL Server, SQLite

