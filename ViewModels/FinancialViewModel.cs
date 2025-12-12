using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class FinancialViewModel : ObservableObject
{
	private readonly FinancialService _financialService;

	[ObservableProperty] private FinancialSummaryDTO? summary;
	[ObservableProperty] private ObservableCollection<IncomeDTO> recentIncomes = new();
	[ObservableProperty] private ObservableCollection<ExpenseDTO> recentExpenses = new();
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private DateTime startDate = DateTime.Now.AddMonths(-1);
	[ObservableProperty] private DateTime endDate = DateTime.Now;

	public FinancialViewModel(FinancialService financialService)
	{
		_financialService = financialService;
	}

	[RelayCommand]
	public async Task LoadDataAsync()
	{
		IsLoading = true;
		try
		{
			// Load summary
			Summary = await _financialService.GetFinancialSummaryAsync(StartDate, EndDate);

			// Load recent transactions
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
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load financial data: {ex.Message}", "OK");
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
}



