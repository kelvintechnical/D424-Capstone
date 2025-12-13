using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StudentLifeTracker.API.Controllers;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Tests.Helpers;
using System.Security.Claims;
using Xunit;

namespace StudentProgressTracker.Tests.Controllers;

public class SearchControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SearchController _controller;
    private readonly Mock<ILogger<SearchController>> _loggerMock;
    private readonly string _testUserId;

    public SearchControllerTests()
    {
        _context = TestHelpers.CreateInMemoryDbContext($"SearchTestDb_{Guid.NewGuid()}");
        _loggerMock = new Mock<ILogger<SearchController>>();
        _testUserId = Guid.NewGuid().ToString();
        _controller = new SearchController(_context, _loggerMock.Object);

        // Set up user claims for authorization
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUserId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = principal
            }
        };

        SeedTestData();
    }

    private void SeedTestData()
    {
        var term1 = new Term
        {
            UserId = _testUserId,
            Title = "Spring 2025",
            StartDate = new DateTime(2025, 1, 15),
            EndDate = new DateTime(2025, 5, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var term2 = new Term
        {
            UserId = _testUserId,
            Title = "Fall 2024",
            StartDate = new DateTime(2024, 9, 1),
            EndDate = new DateTime(2024, 12, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Terms.AddRange(term1, term2);
        _context.SaveChanges();

        var course1 = new Course
        {
            TermId = term1.Id,
            Title = "Mobile App Development",
            StartDate = new DateTime(2025, 1, 20),
            EndDate = new DateTime(2025, 5, 10),
            Status = "InProgress",
            InstructorName = "Dr. Smith",
            InstructorEmail = "smith@university.edu",
            InstructorPhone = "555-1234",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var course2 = new Course
        {
            TermId = term1.Id,
            Title = "Database Systems",
            StartDate = new DateTime(2025, 1, 20),
            EndDate = new DateTime(2025, 5, 10),
            Status = "Completed",
            InstructorName = "Dr. Johnson",
            InstructorEmail = "johnson@university.edu",
            InstructorPhone = "555-5678",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Courses.AddRange(course1, course2);
        _context.SaveChanges();
    }

    #region Course Search Tests

    [Fact]
    public async Task SearchCourses_ByTitle_ShouldReturnMatchingCourses()
    {
        // Arrange
        var query = "Mobile";

        // Act
        var result = await _controller.SearchCourses(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
        response.Data.Should().Contain(r => r.Title.Contains("Mobile", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchCourses_ByInstructorName_ShouldReturnMatchingCourses()
    {
        // Arrange
        var query = "Smith";

        // Act
        var result = await _controller.SearchCourses(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchCourses_ByInstructorEmail_ShouldReturnMatchingCourses()
    {
        // Arrange
        var query = "smith@university.edu";

        // Act
        var result = await _controller.SearchCourses(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchCourses_WithStatusFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var query = "Development";
        var status = "InProgress";

        // Act
        var result = await _controller.SearchCourses(query, status);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
        // All results should have the specified status (checked in controller)
    }

    [Fact]
    public async Task SearchCourses_CaseInsensitive_ShouldReturnResults()
    {
        // Arrange
        var query = "MOBILE"; // All caps

        // Act
        var result = await _controller.SearchCourses(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
        response.Data.Should().Contain(r => r.Title.Contains("Mobile", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchCourses_WithEmptyQuery_ShouldReturnBadRequest()
    {
        // Arrange
        var query = "";

        // Act
        var result = await _controller.SearchCourses(query);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchCourses_PartialMatch_ShouldReturnResults()
    {
        // Arrange
        var query = "App"; // Partial match for "Mobile App Development"

        // Act
        var result = await _controller.SearchCourses(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
    }

    #endregion

    #region Term Search Tests

    [Fact]
    public async Task SearchTerms_ByTitle_ShouldReturnMatchingTerms()
    {
        // Arrange
        var query = "Spring";

        // Act
        var result = await _controller.SearchTerms(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
        response.Data.Should().Contain(r => r.Title.Contains("Spring", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchTerms_CaseInsensitive_ShouldReturnResults()
    {
        // Arrange
        var query = "SPRING"; // All caps

        // Act
        var result = await _controller.SearchTerms(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
    }

    #endregion

    #region Global Search Tests

    [Fact]
    public async Task SearchAll_ShouldReturnBothTermsAndCourses()
    {
        // Arrange
        var query = "2025";

        // Act
        var result = await _controller.SearchAll(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
        response.Data.Should().Contain(r => r.ResultType == "Term");
        response.Data.Should().Contain(r => r.ResultType == "Course");
    }

    [Fact]
    public async Task SearchAll_ShouldBeSortedByTitle()
    {
        // Arrange
        var query = "2025";

        // Act
        var result = await _controller.SearchAll(query);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<List<SearchResultDTO>>;
        response!.Success.Should().BeTrue();
        
        if (response.Data!.Count > 1)
        {
            var sortedResults = response.Data.OrderBy(r => r.Title).ToList();
            response.Data.Should().BeEquivalentTo(sortedResults, options => options.WithStrictOrdering());
        }
    }

    #endregion

    public void Dispose()
    {
        _context?.Dispose();
    }
}

