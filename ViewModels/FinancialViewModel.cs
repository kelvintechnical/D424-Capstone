using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;
using System.Text;

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

	[RelayCommand]
	private async Task ExportFinancialReportAsync()
	{
		try
		{
			IsLoading = true;

			// Get user info
			var user = await _apiService.GetUserAsync();
			if (user == null)
			{
				await Shell.Current.DisplayAlert("Error", "User information not available. Please log in again.", "OK");
				return;
			}

			// Get all incomes and expenses for the date range (not just recent 5)
			var allIncomes = await _financialService.GetIncomesAsync(StartDate, EndDate);
			var allExpenses = await _financialService.GetExpensesAsync(StartDate, EndDate);
			var categories = await _financialService.GetCategoriesAsync();

			// Create a dictionary for category lookup
			var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

			// Build CSV
			var csv = new StringBuilder();

			// Header
			csv.AppendLine("=====================================");
			csv.AppendLine("STUDENT PROGRESS TRACKER");
			csv.AppendLine("FINANCIAL REPORT");
			csv.AppendLine($"Generated: {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}");
			csv.AppendLine($"Student: {user.Name}");
			csv.AppendLine($"Report Period: {StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}");
			csv.AppendLine();

			// Summary
			if (Summary == null)
			{
				// Calculate summary if not available
				var summary = await _financialService.GetFinancialSummaryAsync(StartDate, EndDate);
				if (summary != null)
				{
					Summary = summary;
				}
			}

			csv.AppendLine("FINANCIAL SUMMARY");
			csv.AppendLine($"Total Income: {FormatCurrency(Summary?.TotalIncome ?? 0)}");
			csv.AppendLine($"Total Expenses: {FormatCurrency(Summary?.TotalExpenses ?? 0)}");
			csv.AppendLine($"Net Amount: {FormatCurrency(Summary?.NetAmount ?? 0)}");
			csv.AppendLine();

			// Income entries
			csv.AppendLine("INCOME ENTRIES");
			csv.AppendLine("Date,Source,Amount");

			if (allIncomes != null && allIncomes.Any())
			{
				foreach (var income in allIncomes.OrderByDescending(i => i.Date))
				{
					string amount = FormatCurrency(income.Amount);
					csv.AppendLine($"{income.Date:MMM dd, yyyy},{EscapeCsvField(income.Source)},{amount}");
				}
			}
			else
			{
				csv.AppendLine("No income recorded");
			}
			csv.AppendLine();

			// Expense entries
			csv.AppendLine("EXPENSE ENTRIES");
			csv.AppendLine("Date,Category,Description,Amount");

			if (allExpenses != null && allExpenses.Any())
			{
				foreach (var expense in allExpenses.OrderByDescending(e => e.Date))
				{
					string categoryName = categoryDict.ContainsKey(expense.CategoryId) 
						? categoryDict[expense.CategoryId] 
						: "Unknown";
					string amount = FormatCurrency(expense.Amount);
					csv.AppendLine($"{expense.Date:MMM dd, yyyy},{EscapeCsvField(categoryName)},{EscapeCsvField(expense.Description)},{amount}");
				}
			}
			else
			{
				csv.AppendLine("No expenses recorded");
			}

			// Save and share
			var fileName = $"Financial_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
			var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

			await File.WriteAllTextAsync(filePath, csv.ToString());

			await Share.Default.RequestAsync(new ShareFileRequest
			{
				Title = "Export Financial Report",
				File = new ShareFile(filePath)
			});
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
			await Shell.Current.DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	private string EscapeCsvField(string field)
	{
		if (string.IsNullOrEmpty(field))
			return string.Empty;

		// If field contains comma, quote, or newline, wrap in quotes and escape quotes
		if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
		{
			return $"\"{field.Replace("\"", "\"\"")}\"";
		}

		return field;
	}

	private string FormatCurrency(decimal amount)
	{
		// Format as currency with $ sign and 2 decimal places
		// Use ToString("C2") which includes currency symbol, commas, and 2 decimals
		// Then escape it since it may contain commas
		return EscapeCsvField(amount.ToString("C2"));
	}
}





