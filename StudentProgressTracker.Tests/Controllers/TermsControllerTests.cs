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

public class TermsControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<TermsController>> _loggerMock;
    private readonly TermsController _controller;
    private readonly string _testUserId = "test-user-123";

    public TermsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<TermsController>>();
        _controller = new TermsController(_context, _loggerMock.Object);
        TestHelpers.SetUserContext(_controller, _testUserId);
    }

    [Fact]
    public async Task Test_GetTerms_ReturnsListOfTerms()
    {
        // Arrange
        var term1 = new Term
        {
            Id = 1,
            UserId = _testUserId,
            Title = "Fall 2025",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
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
        _context.Terms.AddRange(term1, term2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTerms();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<TermDTO>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task Test_GetTermById_WithValidId_ReturnsTerm()
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

        // Act
        var result = await _controller.GetTerm(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TermDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Fall 2025", response.Data.Title);
    }

    [Fact]
    public async Task Test_CreateTerm_WithValidData_ReturnsCreatedTerm()
    {
        // Arrange
        var termDto = new TermDTO
        {
            Title = "Summer 2026",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 8, 15)
        };

        // Act
        var result = await _controller.CreateTerm(termDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TermDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Summer 2026", response.Data.Title);
        Assert.Equal(_testUserId, response.Data.UserId);
    }

    [Fact]
    public async Task Test_UpdateTerm_WithValidData_ReturnsUpdatedTerm()
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

        var updateDto = new TermDTO
        {
            Title = "Fall 2025 Updated",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 20)
        };

        // Act
        var result = await _controller.UpdateTerm(1, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TermDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Fall 2025 Updated", response.Data.Title);
    }

    [Fact]
    public async Task Test_DeleteTerm_WithValidId_ReturnsSuccess()
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

        // Act
        var result = await _controller.DeleteTerm(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);

        // Verify term was deleted
        var deletedTerm = await _context.Terms.FindAsync(1);
        Assert.Null(deletedTerm);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

