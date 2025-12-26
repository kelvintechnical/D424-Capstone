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

public class AssessmentsControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<AssessmentsController>> _loggerMock;
    private readonly AssessmentsController _controller;
    private readonly string _testUserId = "test-user-123";

    public AssessmentsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<AssessmentsController>>();
        _controller = new AssessmentsController(_context, _loggerMock.Object);
        TestHelpers.SetUserContext(_controller, _testUserId);
    }

    [Fact]
    public async Task Test_GetAssessmentsByCourse_ReturnsAssessments()
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

        var assessment1 = new Assessment
        {
            Id = 1,
            CourseId = 1,
            Name = "Midterm Exam",
            Type = "Objective",
            StartDate = new DateTime(2025, 10, 15),
            DueDate = new DateTime(2025, 10, 15),
            NotificationsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var assessment2 = new Assessment
        {
            Id = 2,
            CourseId = 1,
            Name = "Final Project",
            Type = "Performance",
            StartDate = new DateTime(2025, 11, 1),
            DueDate = new DateTime(2025, 12, 10),
            NotificationsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Assessments.AddRange(assessment1, assessment2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAssessmentsByCourse(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<AssessmentDTO>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task Test_CreateAssessment_WithValidData_ReturnsCreatedAssessment()
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

        var assessmentDto = new AssessmentDTO
        {
            CourseId = 1,
            Name = "Quiz 1",
            Type = "Objective",
            StartDate = new DateTime(2025, 9, 15),
            DueDate = new DateTime(2025, 9, 15),
            NotificationsEnabled = true
        };

        // Act
        var result = await _controller.CreateAssessment(assessmentDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AssessmentDTO>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Quiz 1", response.Data.Name);
    }

    [Fact]
    public async Task Test_UpdateAssessment_WithValidData_ReturnsUpdatedAssessment()
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

        var assessment = new Assessment
        {
            Id = 1,
            CourseId = 1,
            Name = "Midterm Exam",
            Type = "Objective",
            StartDate = new DateTime(2025, 10, 15),
            DueDate = new DateTime(2025, 10, 15),
            NotificationsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();

        var updateDto = new AssessmentDTO
        {
            Name = "Midterm Exam Updated",
            Type = "Objective",
            StartDate = new DateTime(2025, 10, 20),
            DueDate = new DateTime(2025, 10, 20),
            NotificationsEnabled = false
        };

        // Act
        var result = await _controller.UpdateAssessment(1, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AssessmentDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Midterm Exam Updated", response.Data.Name);
        Assert.False(response.Data.NotificationsEnabled);
    }

    [Fact]
    public async Task Test_DeleteAssessment_WithValidId_ReturnsSuccess()
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

        var assessment = new Assessment
        {
            Id = 1,
            CourseId = 1,
            Name = "Midterm Exam",
            Type = "Objective",
            StartDate = new DateTime(2025, 10, 15),
            DueDate = new DateTime(2025, 10, 15),
            NotificationsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteAssessment(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);

        // Verify assessment was deleted
        var deletedAssessment = await _context.Assessments.FindAsync(1);
        Assert.Null(deletedAssessment);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

