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

public class FinancialControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<FinancialController>> _loggerMock;
    private readonly FinancialController _controller;
    private readonly string _testUserId = "test-user-123";

    public FinancialControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<FinancialController>>();
        _controller = new FinancialController(_context, _loggerMock.Object);
        TestHelpers.SetUserContext(_controller, _testUserId);
    }

    [Fact]
    public async Task Test_GetIncomes_ReturnsIncomeList()
    {
        // Arrange
        var income1 = new Income
        {
            Id = 1,
            UserId = _testUserId,
            Amount = 1000.00m,
            Source = "Part-time Job",
            Date = new DateTime(2025, 12, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var income2 = new Income
        {
            Id = 2,
            UserId = _testUserId,
            Amount = 500.00m,
            Source = "Scholarship",
            Date = new DateTime(2025, 12, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Incomes.AddRange(income1, income2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetIncomes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<IncomeDTO>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task Test_CreateIncome_WithValidData_ReturnsCreatedIncome()
    {
        // Arrange
        var incomeDto = new IncomeDTO
        {
            Amount = 750.00m,
            Source = "Freelance Work",
            Date = new DateTime(2025, 12, 20)
        };

        // Act
        var result = await _controller.CreateIncome(incomeDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IncomeDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(750.00m, response.Data.Amount);
        Assert.Equal("Freelance Work", response.Data.Source);
        Assert.Equal(_testUserId, response.Data.UserId);
    }

    [Fact]
    public async Task Test_GetExpenses_ReturnsExpenseList()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            UserId = _testUserId,
            Name = "Food",
            IsCustom = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(category);

        var expense1 = new Expense
        {
            Id = 1,
            UserId = _testUserId,
            CategoryId = 1,
            Amount = 50.00m,
            Description = "Groceries",
            Date = new DateTime(2025, 12, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var expense2 = new Expense
        {
            Id = 2,
            UserId = _testUserId,
            CategoryId = 1,
            Amount = 25.00m,
            Description = "Restaurant",
            Date = new DateTime(2025, 12, 10),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Expenses.AddRange(expense1, expense2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetExpenses();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<ExpenseDTO>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task Test_CreateExpense_WithValidData_ReturnsCreatedExpense()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            UserId = _testUserId,
            Name = "Transportation",
            IsCustom = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var expenseDto = new ExpenseDTO
        {
            CategoryId = 1,
            Amount = 30.00m,
            Description = "Gas",
            Date = new DateTime(2025, 12, 20)
        };

        // Act
        var result = await _controller.CreateExpense(expenseDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ExpenseDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(30.00m, response.Data.Amount);
        Assert.Equal("Gas", response.Data.Description);
        Assert.Equal(1, response.Data.CategoryId);
        Assert.Equal(_testUserId, response.Data.UserId);
    }

    [Fact]
    public async Task Test_GetSummary_CalculatesCorrectTotals()
    {
        // Arrange
        var startDate = new DateTime(2025, 12, 1);
        var endDate = new DateTime(2025, 12, 31);

        var income1 = new Income
        {
            Id = 1,
            UserId = _testUserId,
            Amount = 1000.00m,
            Source = "Job",
            Date = new DateTime(2025, 12, 5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var income2 = new Income
        {
            Id = 2,
            UserId = _testUserId,
            Amount = 500.00m,
            Source = "Scholarship",
            Date = new DateTime(2025, 12, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Incomes.AddRange(income1, income2);

        var category = new Category
        {
            Id = 1,
            UserId = _testUserId,
            Name = "Food",
            IsCustom = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(category);

        var expense1 = new Expense
        {
            Id = 1,
            UserId = _testUserId,
            CategoryId = 1,
            Amount = 200.00m,
            Description = "Groceries",
            Date = new DateTime(2025, 12, 10),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var expense2 = new Expense
        {
            Id = 2,
            UserId = _testUserId,
            CategoryId = 1,
            Amount = 100.00m,
            Description = "Restaurant",
            Date = new DateTime(2025, 12, 20),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Expenses.AddRange(expense1, expense2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetSummary(startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<FinancialSummaryDTO>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1500.00m, response.Data.TotalIncome); // 1000 + 500
        Assert.Equal(300.00m, response.Data.TotalExpenses); // 200 + 100
        Assert.Equal(1200.00m, response.Data.NetAmount); // 1500 - 300
        Assert.Equal(2, response.Data.IncomeCount);
        Assert.Equal(2, response.Data.ExpenseCount);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

