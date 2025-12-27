# Student Progress Tracker - Test Suite

This test project contains **exactly 30 unit tests** for the Student Progress Tracker application, as required by Task 2.

## Test Structure

```
StudentProgressTracker.Tests/
├── Controllers/
│   ├── AuthControllerTests.cs      # 5 authentication tests
│   ├── TermsControllerTests.cs     # 5 term CRUD tests
│   ├── CoursesControllerTests.cs    # 5 course CRUD tests
│   ├── AssessmentsControllerTests.cs # 4 assessment CRUD tests
│   ├── GradesControllerTests.cs    # 4 grade and GPA tests
│   ├── FinancialControllerTests.cs  # 5 financial tests
│   └── SearchControllerTests.cs    # 2 search tests
└── Helpers/
    └── TestHelpers.cs               # Test utilities and helpers
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

## Test Coverage (30 Tests Total)

### AuthControllerTests (5 tests)
1. Login_WithValidCredentials_ShouldReturnToken
2. Login_WithInvalidCredentials_ShouldReturnUnauthorized
3. Login_WithEmptyEmail_ShouldReturnBadRequest
4. Login_WithEmptyPassword_ShouldReturnBadRequest
5. Register_WithValidData_ShouldReturnSuccess

### TermsControllerTests (5 tests)
6. Test_GetTerms_ReturnsListOfTerms
7. Test_GetTermById_WithValidId_ReturnsTerm
8. Test_CreateTerm_WithValidData_ReturnsCreatedTerm
9. Test_UpdateTerm_WithValidData_ReturnsUpdatedTerm
10. Test_DeleteTerm_WithValidId_ReturnsSuccess

### CoursesControllerTests (5 tests)
11. Test_GetCoursesByTerm_ReturnsCoursesForTerm
12. Test_GetCourseById_WithValidId_ReturnsCourse
13. Test_CreateCourse_WithValidData_ReturnsCreatedCourse
14. Test_UpdateCourse_WithValidData_ReturnsUpdatedCourse
15. Test_DeleteCourse_WithValidId_ReturnsSuccess

### AssessmentsControllerTests (4 tests)
16. Test_GetAssessmentsByCourse_ReturnsAssessments
17. Test_CreateAssessment_WithValidData_ReturnsCreatedAssessment
18. Test_UpdateAssessment_WithValidData_ReturnsUpdatedAssessment
19. Test_DeleteAssessment_WithValidId_ReturnsSuccess

### GradesControllerTests (4 tests)
20. Test_SaveGrade_WithValidData_ReturnsSuccess
21. Test_GetTermGrades_ReturnsGradesList
22. Test_GetTermGPA_CalculatesCorrectly
23. Test_GradeProjection_CalculatesRequiredScore

### FinancialControllerTests (5 tests)
24. Test_GetIncomes_ReturnsIncomeList
25. Test_CreateIncome_WithValidData_ReturnsCreatedIncome
26. Test_GetExpenses_ReturnsExpenseList
27. Test_CreateExpense_WithValidData_ReturnsCreatedExpense
28. Test_GetSummary_CalculatesCorrectTotals

### SearchControllerTests (2 tests)
29. SearchCourses_ShouldReturnMatchingCourses
30. SearchTerms_ShouldReturnMatchingTerms

## Test Statistics

- **Total Test Files:** 7 controller test files
- **Total Test Methods:** 30 (exactly as required)
- **Test Frameworks:** xUnit, FluentAssertions, Moq
- **Test Database:** Entity Framework InMemory

## Test Helpers

The `TestHelpers` class provides:
- `CreateInMemoryDbContext()` - Creates isolated in-memory database for each test
- `CreateTestConfiguration()` - Creates test JWT configuration
- `CreateMockUserManager()` - Mocks ASP.NET Identity UserManager
- `CreateMockSignInManager()` - Mocks ASP.NET Identity SignInManager
- `SetUserContext()` - Sets up authenticated user context for controller tests

## Notes

- Tests use **InMemory database** for isolation and speed
- Each test class creates its own database instance
- Tests are **independent** and can run in any order
- **Moq** is used for mocking dependencies
- **FluentAssertions** provides readable assertions
- All 30 tests pass successfully
