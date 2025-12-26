using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.Shared.DTOs;
using System.Security.Claims;

namespace StudentLifeTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FinancialController> _logger;

    public FinancialController(ApplicationDbContext context, ILogger<FinancialController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    #region Income Endpoints

    [HttpGet("income")]
    public async Task<ActionResult<ApiResponse<List<IncomeDTO>>>> GetIncomes(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<IncomeDTO>>.ErrorResponse("User not authenticated."));
            }

            var query = _context.Incomes.Where(i => i.UserId == userId);

            if (startDate.HasValue)
            {
                // Normalize to start of day
                var start = startDate.Value.Date;
                query = query.Where(i => i.Date >= start);
            }

            if (endDate.HasValue)
            {
                // Normalize to end of day
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.Date <= end);
            }

            var incomes = await query
                .OrderByDescending(i => i.Date)
                .Select(i => new IncomeDTO
                {
                    Id = i.Id,
                    UserId = i.UserId,
                    Amount = i.Amount,
                    Source = i.Source,
                    Date = i.Date,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<IncomeDTO>>.SuccessResponse(incomes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incomes");
            return StatusCode(500, ApiResponse<List<IncomeDTO>>.ErrorResponse("An error occurred while retrieving incomes."));
        }
    }

    [HttpGet("income/{id}")]
    public async Task<ActionResult<ApiResponse<IncomeDTO>>> GetIncome(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<IncomeDTO>.ErrorResponse("User not authenticated."));
            }

            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound(ApiResponse<IncomeDTO>.ErrorResponse("Income not found."));
            }

            var dto = new IncomeDTO
            {
                Id = income.Id,
                UserId = income.UserId,
                Amount = income.Amount,
                Source = income.Source,
                Date = income.Date,
                CreatedAt = income.CreatedAt,
                UpdatedAt = income.UpdatedAt
            };

            return Ok(ApiResponse<IncomeDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving income");
            return StatusCode(500, ApiResponse<IncomeDTO>.ErrorResponse("An error occurred while retrieving income."));
        }
    }

    [HttpPost("income")]
    public async Task<ActionResult<ApiResponse<IncomeDTO>>> CreateIncome([FromBody] IncomeDTO incomeDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<IncomeDTO>.ErrorResponse("User not authenticated."));
            }

            var income = new Income
            {
                UserId = userId,
                Amount = incomeDto.Amount,
                Source = incomeDto.Source,
                Date = incomeDto.Date,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();

            var result = new IncomeDTO
            {
                Id = income.Id,
                UserId = income.UserId,
                Amount = income.Amount,
                Source = income.Source,
                Date = income.Date,
                CreatedAt = income.CreatedAt,
                UpdatedAt = income.UpdatedAt
            };

            return Ok(ApiResponse<IncomeDTO>.SuccessResponse(result, "Income created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating income");
            return StatusCode(500, ApiResponse<IncomeDTO>.ErrorResponse("An error occurred while creating income."));
        }
    }

    [HttpPut("income/{id}")]
    public async Task<ActionResult<ApiResponse<IncomeDTO>>> UpdateIncome(int id, [FromBody] IncomeDTO incomeDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<IncomeDTO>.ErrorResponse("User not authenticated."));
            }

            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound(ApiResponse<IncomeDTO>.ErrorResponse("Income not found."));
            }

            income.Amount = incomeDto.Amount;
            income.Source = incomeDto.Source;
            income.Date = incomeDto.Date;
            income.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new IncomeDTO
            {
                Id = income.Id,
                UserId = income.UserId,
                Amount = income.Amount,
                Source = income.Source,
                Date = income.Date,
                CreatedAt = income.CreatedAt,
                UpdatedAt = income.UpdatedAt
            };

            return Ok(ApiResponse<IncomeDTO>.SuccessResponse(result, "Income updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating income");
            return StatusCode(500, ApiResponse<IncomeDTO>.ErrorResponse("An error occurred while updating income."));
        }
    }

    [HttpDelete("income/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteIncome(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated."));
            }

            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Income not found."));
            }

            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Income deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting income");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting income."));
        }
    }

    #endregion

    #region Expense Endpoints

    [HttpGet("expense")]
    public async Task<ActionResult<ApiResponse<List<ExpenseDTO>>>> GetExpenses(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? categoryId = null)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<ExpenseDTO>>.ErrorResponse("User not authenticated."));
            }

            var query = _context.Expenses.Where(e => e.UserId == userId);

            if (startDate.HasValue)
            {
                // Normalize to start of day
                var start = startDate.Value.Date;
                query = query.Where(e => e.Date >= start);
            }

            if (endDate.HasValue)
            {
                // Normalize to end of day
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(e => e.Date <= end);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            var expenses = await query
                .Include(e => e.Category)
                .OrderByDescending(e => e.Date)
                .Select(e => new ExpenseDTO
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    Amount = e.Amount,
                    CategoryId = e.CategoryId,
                    Description = e.Description,
                    Date = e.Date,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ExpenseDTO>>.SuccessResponse(expenses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses");
            return StatusCode(500, ApiResponse<List<ExpenseDTO>>.ErrorResponse("An error occurred while retrieving expenses."));
        }
    }

    [HttpGet("expense/{id}")]
    public async Task<ActionResult<ApiResponse<ExpenseDTO>>> GetExpense(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<ExpenseDTO>.ErrorResponse("User not authenticated."));
            }

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound(ApiResponse<ExpenseDTO>.ErrorResponse("Expense not found."));
            }

            var dto = new ExpenseDTO
            {
                Id = expense.Id,
                UserId = expense.UserId,
                Amount = expense.Amount,
                CategoryId = expense.CategoryId,
                Description = expense.Description,
                Date = expense.Date,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };

            return Ok(ApiResponse<ExpenseDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense");
            return StatusCode(500, ApiResponse<ExpenseDTO>.ErrorResponse("An error occurred while retrieving expense."));
        }
    }

    [HttpPost("expense")]
    public async Task<ActionResult<ApiResponse<ExpenseDTO>>> CreateExpense([FromBody] ExpenseDTO expenseDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<ExpenseDTO>.ErrorResponse("User not authenticated."));
            }

            // Verify category belongs to user
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == expenseDto.CategoryId && c.UserId == userId);

            if (category == null)
            {
                return BadRequest(ApiResponse<ExpenseDTO>.ErrorResponse("Invalid category."));
            }

            var expense = new Expense
            {
                UserId = userId,
                Amount = expenseDto.Amount,
                CategoryId = expenseDto.CategoryId,
                Description = expenseDto.Description,
                Date = expenseDto.Date,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            var result = new ExpenseDTO
            {
                Id = expense.Id,
                UserId = expense.UserId,
                Amount = expense.Amount,
                CategoryId = expense.CategoryId,
                Description = expense.Description,
                Date = expense.Date,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };

            return Ok(ApiResponse<ExpenseDTO>.SuccessResponse(result, "Expense created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, ApiResponse<ExpenseDTO>.ErrorResponse("An error occurred while creating expense."));
        }
    }

    [HttpPut("expense/{id}")]
    public async Task<ActionResult<ApiResponse<ExpenseDTO>>> UpdateExpense(int id, [FromBody] ExpenseDTO expenseDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<ExpenseDTO>.ErrorResponse("User not authenticated."));
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound(ApiResponse<ExpenseDTO>.ErrorResponse("Expense not found."));
            }

            // Verify category belongs to user if changed
            if (expense.CategoryId != expenseDto.CategoryId)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == expenseDto.CategoryId && c.UserId == userId);

                if (category == null)
                {
                    return BadRequest(ApiResponse<ExpenseDTO>.ErrorResponse("Invalid category."));
                }
            }

            expense.Amount = expenseDto.Amount;
            expense.CategoryId = expenseDto.CategoryId;
            expense.Description = expenseDto.Description;
            expense.Date = expenseDto.Date;
            expense.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new ExpenseDTO
            {
                Id = expense.Id,
                UserId = expense.UserId,
                Amount = expense.Amount,
                CategoryId = expense.CategoryId,
                Description = expense.Description,
                Date = expense.Date,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };

            return Ok(ApiResponse<ExpenseDTO>.SuccessResponse(result, "Expense updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense");
            return StatusCode(500, ApiResponse<ExpenseDTO>.ErrorResponse("An error occurred while updating expense."));
        }
    }

    [HttpDelete("expense/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteExpense(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated."));
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Expense not found."));
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Expense deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting expense."));
        }
    }

    #endregion

    #region Category Endpoints

    [HttpGet("category")]
    public async Task<ActionResult<ApiResponse<List<CategoryDTO>>>> GetCategories()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<CategoryDTO>>.ErrorResponse("User not authenticated."));
            }

            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Name = c.Name,
                    IsCustom = c.IsCustom,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<CategoryDTO>>.SuccessResponse(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, ApiResponse<List<CategoryDTO>>.ErrorResponse("An error occurred while retrieving categories."));
        }
    }

    [HttpGet("category/{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDTO>>> GetCategory(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<CategoryDTO>.ErrorResponse("User not authenticated."));
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound(ApiResponse<CategoryDTO>.ErrorResponse("Category not found."));
            }

            var dto = new CategoryDTO
            {
                Id = category.Id,
                UserId = category.UserId,
                Name = category.Name,
                IsCustom = category.IsCustom,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(ApiResponse<CategoryDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category");
            return StatusCode(500, ApiResponse<CategoryDTO>.ErrorResponse("An error occurred while retrieving category."));
        }
    }

    [HttpPost("category")]
    public async Task<ActionResult<ApiResponse<CategoryDTO>>> CreateCategory([FromBody] CategoryDTO categoryDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<CategoryDTO>.ErrorResponse("User not authenticated."));
            }

            // Check if category name already exists for user
            var existing = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == categoryDto.Name.ToLower());

            if (existing != null)
            {
                return BadRequest(ApiResponse<CategoryDTO>.ErrorResponse("Category with this name already exists."));
            }

            var category = new Category
            {
                UserId = userId,
                Name = categoryDto.Name,
                IsCustom = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = new CategoryDTO
            {
                Id = category.Id,
                UserId = category.UserId,
                Name = category.Name,
                IsCustom = category.IsCustom,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(ApiResponse<CategoryDTO>.SuccessResponse(result, "Category created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, ApiResponse<CategoryDTO>.ErrorResponse("An error occurred while creating category."));
        }
    }

    [HttpPut("category/{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDTO>>> UpdateCategory(int id, [FromBody] CategoryDTO categoryDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<CategoryDTO>.ErrorResponse("User not authenticated."));
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound(ApiResponse<CategoryDTO>.ErrorResponse("Category not found."));
            }

            // Check if new name conflicts with existing category
            if (category.Name.ToLower() != categoryDto.Name.ToLower())
            {
                var existing = await _context.Categories
                    .FirstOrDefaultAsync(c => c.UserId == userId && 
                        c.Id != id && 
                        c.Name.ToLower() == categoryDto.Name.ToLower());

                if (existing != null)
                {
                    return BadRequest(ApiResponse<CategoryDTO>.ErrorResponse("Category with this name already exists."));
                }
            }

            category.Name = categoryDto.Name;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new CategoryDTO
            {
                Id = category.Id,
                UserId = category.UserId,
                Name = category.Name,
                IsCustom = category.IsCustom,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(ApiResponse<CategoryDTO>.SuccessResponse(result, "Category updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, ApiResponse<CategoryDTO>.ErrorResponse("An error occurred while updating category."));
        }
    }

    [HttpDelete("category/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated."));
            }

            var category = await _context.Categories
                .Include(c => c.Expenses)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Category not found."));
            }

            // Check if category has expenses (cascade delete is restricted)
            if (category.Expenses.Any())
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete category with existing expenses. Please delete or reassign expenses first."));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting category."));
        }
    }

    #endregion

    #region Summary Endpoints

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<FinancialSummaryDTO>>> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<FinancialSummaryDTO>.ErrorResponse("User not authenticated."));
            }

            // Normalize dates: start at beginning of day, end at end of day
            var start = startDate.HasValue 
                ? startDate.Value.Date 
                : DateTime.UtcNow.AddMonths(-1).Date;
            var end = endDate.HasValue 
                ? endDate.Value.Date.AddDays(1).AddTicks(-1) // End of the day (23:59:59.9999999)
                : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date >= start && i.Date <= end)
                .SumAsync(i => (decimal?)i.Amount) ?? 0;

            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= start && e.Date <= end)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var summary = new FinancialSummaryDTO
            {
                StartDate = start,
                EndDate = end,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetAmount = totalIncome - totalExpenses,
                IncomeCount = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date >= start && i.Date <= end)
                    .CountAsync(),
                ExpenseCount = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date >= start && e.Date <= end)
                    .CountAsync()
            };

            return Ok(ApiResponse<FinancialSummaryDTO>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial summary");
            return StatusCode(500, ApiResponse<FinancialSummaryDTO>.ErrorResponse("An error occurred while retrieving financial summary."));
        }
    }

    #endregion
}





