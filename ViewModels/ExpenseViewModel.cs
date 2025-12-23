using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class ExpenseViewModel : ObservableObject
{
	private readonly FinancialService _financialService;

	[ObservableProperty] private ObservableCollection<ExpenseDTO> expenses = new();
	[ObservableProperty] private ObservableCollection<CategoryDTO> categories = new();
	[ObservableProperty] private ExpenseDTO? selectedExpense;
	[ObservableProperty] private CategoryDTO? selectedCategory;
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private bool isEditing;
	[ObservableProperty] private decimal amount;
	[ObservableProperty] private string description = string.Empty;
	[ObservableProperty] private DateTime date = DateTime.Today;
	[ObservableProperty] private int selectedCategoryId;

	partial void OnSelectedCategoryChanged(CategoryDTO? value)
	{
		if (value != null)
		{
			SelectedCategoryId = value.Id;
		}
	}
	[ObservableProperty] private DateTime? startDate;
	[ObservableProperty] private DateTime? endDate;

	public ExpenseViewModel(FinancialService financialService)
	{
		_financialService = financialService;
	}

	[RelayCommand]
	public async Task LoadDataAsync()
	{
		IsLoading = true;
		try
		{
			// Load categories
			var categoryList = await _financialService.GetCategoriesAsync();
			Categories.Clear();
			foreach (var category in categoryList)
			{
				Categories.Add(category);
			}

			// Load expenses
			var expenseList = await _financialService.GetExpensesAsync(StartDate, EndDate);
			Expenses.Clear();
			foreach (var expense in expenseList)
			{
				Expenses.Add(expense);
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	public void StartNewExpense()
	{
		SelectedExpense = null;
		Amount = 0;
		Description = string.Empty;
		Date = DateTime.Today;
		SelectedCategoryId = Categories.FirstOrDefault()?.Id ?? 0;
		IsEditing = true;
	}

	[RelayCommand]
	public void EditExpense(ExpenseDTO expense)
	{
		SelectedExpense = expense;
		Amount = expense.Amount;
		Description = expense.Description;
		Date = expense.Date;
		SelectedCategoryId = expense.CategoryId;
		SelectedCategory = Categories.FirstOrDefault(c => c.Id == expense.CategoryId);
		IsEditing = true;
	}

	[RelayCommand]
	public async Task SaveExpenseAsync()
	{
		if (Amount <= 0)
		{
			await Application.Current.MainPage.DisplayAlert("Validation", "Amount must be greater than 0", "OK");
			return;
		}

		if (string.IsNullOrWhiteSpace(Description))
		{
			await Application.Current.MainPage.DisplayAlert("Validation", "Description is required", "OK");
			return;
		}

		if (SelectedCategoryId <= 0)
		{
			await Application.Current.MainPage.DisplayAlert("Validation", "Please select a category", "OK");
			return;
		}

		IsLoading = true;
		try
		{
			var expense = new ExpenseDTO
			{
				Id = SelectedExpense?.Id ?? 0,
				Amount = Amount,
				Description = Description,
				Date = Date,
				CategoryId = SelectedCategoryId
			};

			bool success;
			if (SelectedExpense != null)
			{
				success = await _financialService.UpdateExpenseAsync(SelectedExpense.Id, expense);
			}
			else
			{
				success = await _financialService.CreateExpenseAsync(expense);
			}

			if (success)
			{
				IsEditing = false;
				await LoadDataAsync();
				await Application.Current.MainPage.DisplayAlert("Success", "Expense saved successfully", "OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Failed to save expense", "OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save expense: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	public void CancelEdit()
	{
		IsEditing = false;
		SelectedExpense = null;
	}

	[RelayCommand]
	public async Task DeleteExpenseAsync(ExpenseDTO expense)
	{
		var confirm = await Application.Current.MainPage.DisplayAlert(
			"Confirm Delete",
			$"Are you sure you want to delete this expense?",
			"Yes",
			"No");

		if (!confirm) return;

		IsLoading = true;
		try
		{
			var success = await _financialService.DeleteExpenseAsync(expense.Id);
			if (success)
			{
				await LoadDataAsync();
				await Application.Current.MainPage.DisplayAlert("Success", "Expense deleted successfully", "OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete expense", "OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete expense: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	public string GetCategoryName(int categoryId)
	{
		return Categories.FirstOrDefault(c => c.Id == categoryId)?.Name ?? "Unknown";
	}

	[RelayCommand]
	private async Task GoBackAsync()
	{
		await Shell.Current.GoToAsync("..");
	}
}

