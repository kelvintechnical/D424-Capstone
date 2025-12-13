# Student Progress Tracker - Test Suite

This test project contains comprehensive unit tests for the Student Progress Tracker application.

## Test Structure

```
StudentProgressTracker.Tests/
├── Controllers/
│   ├── AuthControllerTests.cs      # Authentication and authorization tests
│   ├── CRUDTests.cs                # Term, Course, Assessment CRUD operations
│   └── SearchControllerTests.cs    # Search functionality tests
├── Services/
│   └── GPACalculationTests.cs     # GPA calculation and grade projection tests
└── Helpers/
    └── TestHelpers.cs              # Test utilities and helpers
```

## Running Tests

### From Command Line
```bash
dotnet test
```

### From Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Run All Tests (Ctrl+R, A)

### With Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Coverage

### ✅ GPA Calculation Tests (Priority 1)
- Calculate GPA with all A grades → 4.0
- Calculate GPA with mixed grades (A, B, C)
- Calculate GPA with different credit hours (weighted correctly)
- Calculate GPA with empty course list → 0.0
- Grade projection: 78% current, want B, final worth 20%
- Grade projection: impossible target (40% current, want A)
- Letter grade to points conversion (A+, A, A-, B+, B, B-, C+, C, C-, D+, D, D-, F)
- Case-insensitive grade conversion
- Invalid grade handling

### ✅ Authentication Tests (Priority 2)
- Register new user successfully
- Register with existing email → fails
- Register with weak password → fails
- Login with valid credentials → returns JWT token
- Login with invalid password → 401 Unauthorized
- Login with non-existent user → 401 Unauthorized
- Token refresh structure (placeholder for full JWT validation)

### ✅ CRUD Operations Tests (Priority 3)
- Create term with valid dates
- Create term with end date before start date (validation test)
- Delete term → cascades to courses
- Create course within a term
- Update course status (In Progress, Completed, Dropped, Plan To Take)
- Delete course → cascades to assessments
- Create objective assessment
- Create performance assessment
- Validate assessment due date is after start date

### ✅ Search Tests (Priority 4)
- Search courses by title (partial match)
- Search courses by instructor name
- Search courses by instructor email
- Search terms by title
- Global search returns both terms and courses
- Filter courses by status
- Empty search query → BadRequest
- Case-insensitive search
- Search results sorted by title

## Test Statistics

- **Total Test Files:** 4
- **Total Test Methods:** 30+
- **Test Frameworks:** xUnit, FluentAssertions, Moq
- **Test Database:** Entity Framework InMemory

## Test Helpers

The `TestHelpers` class provides:
- `CreateInMemoryDbContext()` - Creates isolated in-memory database for each test
- `CreateTestConfiguration()` - Creates test JWT configuration
- `CreateMockUserManager()` - Mocks ASP.NET Identity UserManager
- `CreateMockSignInManager()` - Mocks ASP.NET Identity SignInManager

## Notes

- Tests use **InMemory database** for isolation and speed
- Each test class creates its own database instance
- Tests are **independent** and can run in any order
- **Moq** is used for mocking dependencies
- **FluentAssertions** provides readable assertions

## Future Test Additions

Potential areas for additional tests:
- Financial controller tests (Income, Expense, Category)
- Report generation tests
- Notification service tests
- API integration tests
- End-to-end workflow tests

