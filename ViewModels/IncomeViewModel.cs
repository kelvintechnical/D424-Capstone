using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class IncomeViewModel : ObservableObject
{
	private readonly FinancialService _financialService;

	[ObservableProperty] private ObservableCollection<IncomeDTO> incomes = new();
	[ObservableProperty] private IncomeDTO? selectedIncome;
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private bool isEditing;
	[ObservableProperty] private decimal amount;
	[ObservableProperty] private string source = string.Empty;
	[ObservableProperty] private DateTime date = DateTime.Today;
	[ObservableProperty] private DateTime? startDate;
	[ObservableProperty] private DateTime? endDate;

	public IncomeViewModel(FinancialService financialService)
	{
		_financialService = financialService;
	}

	[RelayCommand]
	public async Task LoadIncomesAsync()
	{
		IsLoading = true;
		try
		{
			var incomeList = await _financialService.GetIncomesAsync(StartDate, EndDate);
			Incomes.Clear();
			foreach (var income in incomeList)
			{
				Incomes.Add(income);
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load incomes: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	public void StartNewIncome()
	{
		SelectedIncome = null;
		Amount = 0;
		Source = string.Empty;
		Date = DateTime.Today;
		IsEditing = true;
	}

	[RelayCommand]
	public void EditIncome(IncomeDTO income)
	{
		SelectedIncome = income;
		Amount = income.Amount;
		Source = income.Source;
		Date = income.Date;
		IsEditing = true;
	}

	[RelayCommand]
	public async Task SaveIncomeAsync()
	{
		if (Amount <= 0)
		{
			await Application.Current.MainPage.DisplayAlert("Validation", "Amount must be greater than 0", "OK");
			return;
		}

		if (string.IsNullOrWhiteSpace(Source))
		{
			await Application.Current.MainPage.DisplayAlert("Validation", "Source is required", "OK");
			return;
		}

		IsLoading = true;
		try
		{
			var income = new IncomeDTO
			{
				Id = SelectedIncome?.Id ?? 0,
				Amount = Amount,
				Source = Source,
				Date = Date
			};

			bool success;
			if (SelectedIncome != null)
			{
				success = await _financialService.UpdateIncomeAsync(SelectedIncome.Id, income);
			}
			else
			{
				success = await _financialService.CreateIncomeAsync(income);
			}

			if (success)
			{
				IsEditing = false;
				await LoadIncomesAsync();
				await Application.Current.MainPage.DisplayAlert("Success", "Income saved successfully", "OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Failed to save income", "OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save income: {ex.Message}", "OK");
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
		SelectedIncome = null;
	}

	[RelayCommand]
	public async Task DeleteIncomeAsync(IncomeDTO income)
	{
		var confirm = await Application.Current.MainPage.DisplayAlert(
			"Confirm Delete",
			$"Are you sure you want to delete income from {income.Source}?",
			"Yes",
			"No");

		if (!confirm) return;

		IsLoading = true;
		try
		{
			var success = await _financialService.DeleteIncomeAsync(income.Id);
			if (success)
			{
				await LoadIncomesAsync();
				await Application.Current.MainPage.DisplayAlert("Success", "Income deleted successfully", "OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete income", "OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete income: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	private async Task GoBackAsync()
	{
		try
		{
			// Try relative navigation first
			if (Shell.Current.Navigation.NavigationStack.Count > 1)
			{
				await Shell.Current.GoToAsync("..");
			}
			else
			{
				// Fallback to FinancialPage if navigation stack is empty
				await Shell.Current.GoToAsync($"//{nameof(Views.FinancialPage)}");
			}
		}
		catch
		{
			// If relative navigation fails, go to FinancialPage
			await Shell.Current.GoToAsync($"//{nameof(Views.FinancialPage)}");
		}
	}
}





