using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.API.Services;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Tests.Helpers;
using Xunit;

namespace StudentProgressTracker.Tests.Services;

/// <summary>
/// Unit tests for ReportService - tests data access and report generation.
/// Uses InMemory database to test Entity Framework queries and business logic integration.
/// </summary>
public class ReportServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ReportService _reportService;
    private readonly string _testUserId = "test-user-456";

    public ReportServiceTests()
    {
        _context = TestHelpers.CreateInMemoryDbContext($"ReportTest_{Guid.NewGuid()}");
        _reportService = new ReportService(_context);
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test user
        var user = new ApplicationUser
        {
            Id = _testUserId,
            UserName = "test@example.com",
            Email = "test@example.com",
            Name = "Test Student",
            EmailConfirmed = true
        };
        _context.Users.Add(user);

        // Create test term
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

        // Create test courses
        var course1 = new Course
        {
            Id = 1,
            TermId = 1,
            UserId = _testUserId,
            Title = "Introduction to Computer Science",
            CourseCode = "CS101",
            CreditHours = 3,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var course2 = new Course
        {
            Id = 2,
            TermId = 1,
            UserId = _testUserId,
            Title = "Calculus I",
            CourseCode = "MATH101",
            CreditHours = 4,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Courses.AddRange(course1, course2);

        // Create test grades
        var grade1 = new Grade
        {
            Id = 1,
            CourseId = 1,
            UserId = _testUserId,
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
            UserId = _testUserId,
            LetterGrade = "B",
            Percentage = 85.0m,
            CreditHours = 4,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Grades.AddRange(grade1, grade2);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GenerateGpaReportAsync_WithValidTerm_ReturnsCorrectGPA()
    {
        // Act
        var result = await _reportService.GenerateGpaReportAsync(_testUserId, 1);

        // Assert
        result.Should().NotBeNull();
        result.TermTitle.Should().Be("Fall 2025");
        result.StudentName.Should().Be("Test Student");
        result.Courses.Should().HaveCount(2);
        result.TotalCreditHours.Should().Be(7);
        // Expected GPA: (4.0*3 + 3.0*4) / 7 = (12 + 12) / 7 = 3.43
        result.TermGPA.Should().BeApproximately(3.43, 0.01);
    }

    [Fact]
    public async Task GenerateGpaReportAsync_WithInvalidTermId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = async () => await _reportService.GenerateGpaReportAsync(_testUserId, 999);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Term not found*");
    }

    [Fact]
    public async Task GenerateGpaReportAsync_WithNoGrades_ReturnsZeroGPA()
    {
        // Arrange - create term with no grades
        var term = new Term
        {
            Id = 2,
            UserId = _testUserId,
            Title = "Spring 2026",
            StartDate = new DateTime(2026, 1, 15),
            EndDate = new DateTime(2026, 5, 10),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Terms.Add(term);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GenerateGpaReportAsync(_testUserId, 2);

        // Assert
        result.Should().NotBeNull();
        result.TermGPA.Should().Be(0.0);
        result.Courses.Should().BeEmpty();
        result.TotalCreditHours.Should().Be(0);
    }

    [Fact]
    public void GenerateGpaCsv_WithValidReport_ReturnsFormattedCsv()
    {
        // Arrange
        var report = new GpaReportDTO
        {
            ReportTitle = "GPA Report - Fall 2025",
            GeneratedAt = new DateTime(2025, 12, 20, 10, 30, 0),
            StudentName = "Test Student",
            StudentEmail = "test@example.com",
            TermTitle = "Fall 2025",
            TermStartDate = new DateTime(2025, 9, 1),
            TermEndDate = new DateTime(2025, 12, 15),
            Courses = new List<GpaReportCourseDTO>
            {
                new GpaReportCourseDTO
                {
                    CourseTitle = "Introduction to Computer Science",
                    CreditHours = 3,
                    LetterGrade = "A",
                    GradePoints = 4.0
                }
            },
            TotalCreditHours = 3,
            TermGPA = 4.0
        };

        // Act
        var csv = _reportService.GenerateGpaCsv(report);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        csv.Should().Contain("GPA Report - Fall 2025");
        csv.Should().Contain("Test Student");
        csv.Should().Contain("Introduction to Computer Science");
        csv.Should().Contain("A");
        csv.Should().Contain("4.00");
        csv.Should().Contain("Term GPA,4.00");
    }

    [Fact]
    public void GenerateGpaCsv_WithCommaInCourseTitle_EscapesCorrectly()
    {
        // Arrange
        var report = new GpaReportDTO
        {
            ReportTitle = "GPA Report",
            GeneratedAt = DateTime.UtcNow,
            StudentName = "Test",
            StudentEmail = "test@example.com",
            TermTitle = "Fall 2025",
            TermStartDate = DateTime.UtcNow,
            TermEndDate = DateTime.UtcNow,
            Courses = new List<GpaReportCourseDTO>
            {
                new GpaReportCourseDTO
                {
                    CourseTitle = "Math, Science, and Technology",
                    CreditHours = 3,
                    LetterGrade = "A",
                    GradePoints = 4.0
                }
            },
            TotalCreditHours = 3,
            TermGPA = 4.0
        };

        // Act
        var csv = _reportService.GenerateGpaCsv(report);

        // Assert
        csv.Should().Contain("\"Math, Science, and Technology\"");
    }

    [Fact]
    public async Task GenerateTranscriptReportAsync_WithMultipleTerms_CalculatesCumulativeGPA()
    {
        // Arrange - add second term
        var term2 = new Term
        {
            Id = 2,
            UserId = _testUserId,
            Title = "Spring 2026",
            StartDate = new DateTime(2026, 1, 15),
            EndDate = new DateTime(2026, 5, 10),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Terms.Add(term2);

        var course3 = new Course
        {
            Id = 3,
            TermId = 2,
            UserId = _testUserId,
            Title = "Data Structures",
            CourseCode = "CS201",
            CreditHours = 3,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Courses.Add(course3);

        var grade3 = new Grade
        {
            Id = 3,
            CourseId = 3,
            UserId = _testUserId,
            LetterGrade = "A",
            Percentage = 92.0m,
            CreditHours = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Grades.Add(grade3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GenerateTranscriptReportAsync(_testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Terms.Should().HaveCount(2);
        result.TotalCreditHours.Should().Be(10); // 3 + 4 + 3
        // Cumulative: (4.0*3 + 3.0*4 + 4.0*3) / 10 = (12 + 12 + 12) / 10 = 3.6
        result.CumulativeGPA.Should().BeApproximately(3.6, 0.01);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

