using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StudentLifeTracker.API.Controllers;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Tests.Helpers;
using Xunit;

namespace StudentProgressTracker.Tests.Controllers;

public class CoursesControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<CoursesController>> _loggerMock;
    private readonly CoursesController _controller;
    private readonly string _testUserId = "test-user-123";

    public CoursesControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CoursesController>>();
        _controller = new CoursesController(_context, _loggerMock.Object);
        TestHelpers.SetUserContext(_controller, _testUserId);
    }

    [Fact]
    public async Task Test_GetCoursesByTerm_ReturnsCoursesForTerm()
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
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCoursesByTerm(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<CourseDTO>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task Test_GetCourseById_WithValidId_ReturnsCourse()
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

        // Act
        var result = await _controller.GetCourse(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CourseDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Mathematics 101", response.Data.Title);
    }

    [Fact]
    public async Task Test_CreateCourse_WithValidData_ReturnsCreatedCourse()
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
        await _context.SaveChangesAsync();

        var courseDto = new CourseDTO
        {
            TermId = 1,
            Title = "Computer Science 101",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            Status = "InProgress",
            InstructorName = "Dr. Brown",
            InstructorPhone = "555-0300",
            InstructorEmail = "brown@university.edu",
            CreditHours = 4
        };

        // Act
        var result = await _controller.CreateCourse(courseDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CourseDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Computer Science 101", response.Data.Title);
    }

    [Fact]
    public async Task Test_UpdateCourse_WithValidData_ReturnsUpdatedCourse()
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

        var updateDto = new CourseDTO
        {
            Title = "Advanced Mathematics 101",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            Status = "InProgress",
            InstructorName = "Dr. Smith",
            InstructorPhone = "555-0100",
            InstructorEmail = "smith@university.edu",
            CreditHours = 4
        };

        // Act
        var result = await _controller.UpdateCourse(1, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CourseDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Advanced Mathematics 101", response.Data.Title);
        Assert.Equal(4, response.Data.CreditHours);
    }

    [Fact]
    public async Task Test_DeleteCourse_WithValidId_ReturnsSuccess()
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

        // Act
        var result = await _controller.DeleteCourse(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);

        // Verify course was deleted
        var deletedCourse = await _context.Courses.FindAsync(1);
        Assert.Null(deletedCourse);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

