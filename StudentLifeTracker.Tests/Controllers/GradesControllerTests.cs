using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StudentLifeTracker.API.Controllers;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.Shared.DTOs;
using StudentLifeTracker.Tests.Helpers;
using Xunit;

namespace StudentLifeTracker.Tests.Controllers;

public class GradesControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<GradesController>> _loggerMock;
    private readonly GradesController _controller;
    private readonly string _testUserId = "test-user-123";

    public GradesControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<GradesController>>();
        _controller = new GradesController(_context, _loggerMock.Object);
        TestHelpers.SetUserContext(_controller, _testUserId);
    }

    [Fact]
    public async Task Test_SaveGrade_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var term = new Term
        {
            Id = 1,
            UserId = _testUserId,
            Title = "Fall 2025",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Terms.Add(term);

        var course = new Course
        {
            Id = 1,
            TermId = 1,
            Title = "Mathematics 101",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            Status = "InProgress",
            InstructorName = "Dr. Smith",
            InstructorPhone = "555-0100",
            InstructorEmail = "smith@university.edu",
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var gradeDto = new GradeDTO
        {
            CourseId = 1,
            LetterGrade = "A",
            Percentage = 95.0m,
            CreditHours = 3
        };

        // Act
        var result = await _controller.AddOrUpdateGrade(gradeDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<GradeDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("A", response.Data.LetterGrade);
        Assert.Equal(95.0m, response.Data.Percentage);
    }

    [Fact]
    public async Task Test_GetTermGrades_ReturnsGradesList()
    {
        // Arrange
        var term = new Term
        {
            Id = 1,
            UserId = _testUserId,
            Title = "Fall 2025",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Terms.Add(term);

        var course1 = new Course
        {
            Id = 1,
            TermId = 1,
            Title = "Mathematics 101",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            Status = "InProgress",
            InstructorName = "Dr. Smith",
            InstructorPhone = "555-0100",
            InstructorEmail = "smith@university.edu",
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var course2 = new Course
        {
            Id = 2,
            TermId = 1,
            Title = "English 101",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            Status = "InProgress",
            InstructorName = "Dr. Jones",
            InstructorPhone = "555-0200",
            InstructorEmail = "jones@university.edu",
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Courses.AddRange(course1, course2);

        var grade1 = new Grade
        {
            Id = 1,
            CourseId = 1,
            LetterGrade = "A",
            Percentage = 95.0m,
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var grade2 = new Grade
        {
            Id = 2,
            CourseId = 2,
            LetterGrade = "B",
            Percentage = 85.0m,
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Grades.AddRange(grade1, grade2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTermGrades(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<GradeDTO>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task Test_GetTermGPA_CalculatesCorrectly()
    {
        // Arrange
        var term = new Term
        {
            Id = 1,
            UserId = _testUserId,
            Title = "Fall 2025",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Terms.Add(term);

        var course1 = new Course
        {
            Id = 1,
            TermId = 1,
            Title = "Mathematics 101",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            Status = "InProgress",
            InstructorName = "Dr. Smith",
            InstructorPhone = "555-0100",
            InstructorEmail = "smith@university.edu",
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var course2 = new Course
        {
            Id = 2,
            TermId = 1,
            Title = "English 101",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            Status = "InProgress",
            InstructorName = "Dr. Jones",
            InstructorPhone = "555-0200",
            InstructorEmail = "jones@university.edu",
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Courses.AddRange(course1, course2);

        // A = 4.0, B = 3.0
        // GPA = (4.0 * 3 + 3.0 * 3) / 6 = 3.5
        var grade1 = new Grade
        {
            Id = 1,
            CourseId = 1,
            LetterGrade = "A",
            Percentage = 95.0m,
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var grade2 = new Grade
        {
            Id = 2,
            CourseId = 2,
            LetterGrade = "B",
            Percentage = 85.0m,
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Grades.AddRange(grade1, grade2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTermGPA(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<GpaResultDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.TermId);
        Assert.Equal(6, response.Data.TotalCreditHours);
        Assert.Equal(2, response.Data.GradeCount);
        // GPA should be approximately 3.5 (A=4.0, B=3.0, both 3 credits)
        Assert.True(response.Data.GPA >= 3.4 && response.Data.GPA <= 3.6);
    }

    [Fact]
    public async Task Test_GradeProjection_CalculatesRequiredScore()
    {
        // Arrange
        var term = new Term
        {
            Id = 1,
            UserId = _testUserId,
            Title = "Fall 2025",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Terms.Add(term);

        var course = new Course
        {
            Id = 1,
            TermId = 1,
            Title = "Mathematics 101",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            Status = "InProgress",
            InstructorName = "Dr. Smith",
            InstructorPhone = "555-0100",
            InstructorEmail = "smith@university.edu",
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        // Current grade: 80%, Final weight: 0.3, Target: B (80%)
        // Needed = (80 - (80 * 0.7)) / 0.3 = (80 - 56) / 0.3 = 80
        var currentGrade = 80.0;
        var finalWeight = 0.3;
        var targetGrade = "B";

        // Act
        var result = await _controller.GetGradeProjection(1, currentGrade, finalWeight, targetGrade);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<GradeProjectionDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.CourseId);
        Assert.Equal(currentGrade, response.Data.CurrentGrade);
        Assert.Equal(finalWeight, response.Data.FinalWeight);
        Assert.Equal(targetGrade, response.Data.TargetGrade);
        Assert.True(response.Data.IsAchievable);
        // Should need approximately 80% on final
        Assert.True(response.Data.NeededOnFinal >= 79.0 && response.Data.NeededOnFinal <= 81.0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

