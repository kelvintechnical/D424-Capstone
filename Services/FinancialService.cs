using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;

namespace StudentProgressTracker.Services;

public class FinancialService
{
    private readonly ApiService _apiService;

    public FinancialService(ApiService apiService)
    {
        _apiService = apiService;
    }

    #region Income Methods

    public async Task<List<IncomeDTO>> GetIncomesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var response = await _apiService.GetIncomesAsync(startDate, endDate);
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }
        return new List<IncomeDTO>();
    }

    public async Task<IncomeDTO?> GetIncomeAsync(int id)
    {
        var response = await _apiService.GetIncomeAsync(id);
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }
        return null;
    }

    public async Task<bool> CreateIncomeAsync(IncomeDTO income)
    {
        var response = await _apiService.CreateIncomeAsync(income);
        return response.Success;
    }

    public async Task<bool> UpdateIncomeAsync(int id, IncomeDTO income)
    {
        var response = await _apiService.UpdateIncomeAsync(id, income);
        return response.Success;
    }

    public async Task<bool> DeleteIncomeAsync(int id)
    {
        var response = await _apiService.DeleteIncomeAsync(id);
        return response.Success;
    }

    #endregion

    #region Expense Methods

    public async Task<List<ExpenseDTO>> GetExpensesAsync(DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null)
    {
        var response = await _apiService.GetExpensesAsync(startDate, endDate, categoryId);
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }
        return new List<ExpenseDTO>();
    }

    public async Task<ExpenseDTO?> GetExpenseAsync(int id)
    {
        var response = await _apiService.GetExpenseAsync(id);
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }
        return null;
    }

    public async Task<bool> CreateExpenseAsync(ExpenseDTO expense)
    {
        var response = await _apiService.CreateExpenseAsync(expense);
        return response.Success;
    }

    public async Task<bool> UpdateExpenseAsync(int id, ExpenseDTO expense)
    {
        var response = await _apiService.UpdateExpenseAsync(id, expense);
        return response.Success;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        var response = await _apiService.DeleteExpenseAsync(id);
        return response.Success;
    }

    #endregion

    #region Category Methods

    public async Task<List<CategoryDTO>> GetCategoriesAsync()
    {
        var response = await _apiService.GetCategoriesAsync();
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }
        return new List<CategoryDTO>();
    }

    public async Task<CategoryDTO?> GetCategoryAsync(int id)
    {
        var response = await _apiService.GetCategoryAsync(id);
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }
        return null;
    }

    public async Task<bool> CreateCategoryAsync(CategoryDTO category)
    {
        var response = await _apiService.CreateCategoryAsync(category);
        return response.Success;
    }

    public async Task<bool> UpdateCategoryAsync(int id, CategoryDTO category)
    {
        var response = await _apiService.UpdateCategoryAsync(id, category);
        return response.Success;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var response = await _apiService.DeleteCategoryAsync(id);
        return response.Success;
    }

    #endregion

    #region Summary Methods

    public async Task<FinancialSummaryDTO?> GetFinancialSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var response = await _apiService.GetFinancialSummaryAsync(startDate, endDate);
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }
        return null;
    }

    #endregion
}



