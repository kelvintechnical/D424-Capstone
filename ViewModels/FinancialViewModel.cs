using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class FinancialViewModel : ObservableObject
{
	private readonly FinancialService _financialService;
	private readonly ApiService _apiService;

	[ObservableProperty] private FinancialSummaryDTO? summary;
	[ObservableProperty] private ObservableCollection<IncomeDTO> recentIncomes = new();
	[ObservableProperty] private ObservableCollection<ExpenseDTO> recentExpenses = new();
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private DateTime startDate = DateTime.Now.AddMonths(-1);
	[ObservableProperty] private DateTime endDate = DateTime.Now;

	public FinancialViewModel(FinancialService financialService, ApiService apiService)
	{
		_financialService = financialService;
		_apiService = apiService;
		// Initialize Summary with default values to prevent null binding issues
		Summary = new FinancialSummaryDTO
		{
			StartDate = StartDate,
			EndDate = EndDate,
			TotalIncome = 0,
			TotalExpenses = 0,
			NetAmount = 0,
			IncomeCount = 0,
			ExpenseCount = 0
		};
	}

	[RelayCommand]
	public async Task LoadDataAsync()
	{
		IsLoading = true;
		try
		{
			// Check if user is authenticated
			if (!await _apiService.IsAuthenticatedAsync())
			{
				// Reset to default values if not authenticated
				Summary = new FinancialSummaryDTO
				{
					StartDate = StartDate,
					EndDate = EndDate,
					TotalIncome = 0,
					TotalExpenses = 0,
					NetAmount = 0,
					IncomeCount = 0,
					ExpenseCount = 0
				};
				RecentIncomes.Clear();
				RecentExpenses.Clear();
				return;
			}

			// Load summary from API
			var summary = await _financialService.GetFinancialSummaryAsync(StartDate, EndDate);
			if (summary != null)
			{
				Summary = summary;
			}
			else
			{
				// Initialize with default values if API returns null
				Summary = new FinancialSummaryDTO
				{
					StartDate = StartDate,
					EndDate = EndDate,
					TotalIncome = 0,
					TotalExpenses = 0,
					NetAmount = 0,
					IncomeCount = 0,
					ExpenseCount = 0
				};
			}

			// Load recent transactions from API
			var incomes = await _financialService.GetIncomesAsync(StartDate, EndDate);
			var expenses = await _financialService.GetExpensesAsync(StartDate, EndDate);

			RecentIncomes.Clear();
			foreach (var income in incomes.OrderByDescending(i => i.Date).Take(5))
			{
				RecentIncomes.Add(income);
			}

			RecentExpenses.Clear();
			foreach (var expense in expenses.OrderByDescending(e => e.Date).Take(5))
			{
				RecentExpenses.Add(expense);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error loading financial data: {ex}");
			// Initialize with default values on error
			Summary = new FinancialSummaryDTO
			{
				StartDate = StartDate,
				EndDate = EndDate,
				TotalIncome = 0,
				TotalExpenses = 0,
				NetAmount = 0,
				IncomeCount = 0,
				ExpenseCount = 0
			};
			// Don't show alert for every error - just log it
			// await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load financial data: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	partial void OnStartDateChanged(DateTime value)
	{
		_ = LoadDataAsync();
	}

	partial void OnEndDateChanged(DateTime value)
	{
		_ = LoadDataAsync();
	}

	[RelayCommand]
	private async Task GoBackAsync()
	{
		try
		{
			if (Shell.Current.Navigation.NavigationStack.Count > 1)
			{
				await Shell.Current.GoToAsync("..");
			}
			else
			{
				await Shell.Current.GoToAsync($"//{nameof(Views.TermsPage)}");
			}
		}
		catch
		{
			await Shell.Current.GoToAsync($"//{nameof(Views.TermsPage)}");
		}
	}
}





