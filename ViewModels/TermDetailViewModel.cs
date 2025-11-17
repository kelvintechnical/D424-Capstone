using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Helpers;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;

namespace StudentProgressTracker.ViewModels;

public partial class TermDetailViewModel : ObservableObject
{
	private readonly DatabaseService _db;
	private DateTime _previousValidStartDate = DateTime.Today;
	private DateTime _previousValidEndDate = DateTime.Today;
	private bool _isValidatingDate;

	[ObservableProperty] private AcademicTerm? term;
	[ObservableProperty] private string title = string.Empty;
	[ObservableProperty] private DateTime startDate = DateTime.Today;
	[ObservableProperty] private DateTime endDate = DateTime.Today;
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private bool isSaving;
	[ObservableProperty] private string startDateError = string.Empty;
	[ObservableProperty] private string endDateError = string.Empty;

	public TermDetailViewModel(DatabaseService db)
	{
		_db = db;
	}

	public async Task LoadTermAsync(int termId)
	{
		IsLoading = true;
		try
		{
			var t = await _db.GetTermAsync(termId);
			if (t is null) return;
			Term = t;
			Title = t.Title;
			var localStartDate = ConvertUtcToLocal(t.StartDate);
			var localEndDate = ConvertUtcToLocal(t.EndDate);
			
			// Store as previous valid dates
			_previousValidStartDate = localStartDate;
			_previousValidEndDate = localEndDate;
			
			StartDate = localStartDate;
			EndDate = localEndDate;
		}
		finally
		{
			IsLoading = false;
		}
	}

	public void MapPropertiesToTerm()
	{
		if (Term is null) return;
		Term.Title = Title;
		Term.StartDate = ConvertLocalToUtc(StartDate);
		Term.EndDate = ConvertLocalToUtc(EndDate);
	}

	public DateTime ConvertUtcToLocal(DateTime utcDate) => utcDate.ToLocalTime().Date;
	public DateTime ConvertLocalToUtc(DateTime localDate) => DateTime.SpecifyKind(localDate, DateTimeKind.Local).ToUniversalTime();

	public async Task SaveTermAsync()
	{
		if (Term is null) return;
		IsSaving = true;
		try
		{
			MapPropertiesToTerm();
			if (!Term.IsValid()) throw new InvalidOperationException("Invalid term. End date must be after start date.");
			await _db.SaveTermAsync(Term);
		}
		finally
		{
			IsSaving = false;
		}
	}

	public async Task DeleteTermAsync()
	{
		if (Term is null) return;
		await _db.DeleteTermAsync(Term.Id);
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		try
		{
			await SaveTermAsync();
			if (Application.Current?.MainPage is not null)
			{
				await Application.Current.MainPage.DisplayAlert("Success", "Term saved successfully", "OK");
			}
			await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			if (Application.Current?.MainPage is not null)
			{
				await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save term: {ex.Message}", "OK");
			}
		}
	}

	[RelayCommand]
	private async Task DeleteAsync()
	{
		try
		{
			if (Term == null) return;

			if (Application.Current?.MainPage is null) return;

			bool confirm = await Application.Current.MainPage.DisplayAlert(
				"Delete Term",
				$"Are you sure you want to delete '{Term.Title}'? This will delete all courses and assessments in this term.",
				"Delete",
				"Cancel");

			if (!confirm)
				return;

			await DeleteTermAsync();
			await Application.Current.MainPage.DisplayAlert("Success", "Term deleted successfully", "OK");
			await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			if (Application.Current?.MainPage is not null)
			{
				await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete term: {ex.Message}", "OK");
			}
		}
	}

	[RelayCommand]
	private async Task GoBackAsync()
	{
		await Shell.Current.GoToAsync("..");
	}

	/// <summary>
	/// Called when StartDate property changes. Validates the date and reverts if invalid.
	/// </summary>
	partial void OnStartDateChanged(DateTime value)
	{
		if (_isValidatingDate || IsLoading) return;

		var validation = DateValidationHelper.ValidateDate(value);
		
		if (!validation.IsValid)
		{
			// Show error message
			StartDateError = validation.ErrorMessage;
			
			// Revert to previous valid date
			_isValidatingDate = true;
			StartDate = _previousValidStartDate;
			_isValidatingDate = false;
			
			// Display alert to user
			if (Application.Current?.MainPage is not null)
			{
				_ = Application.Current.MainPage.DisplayAlert(
					"Invalid Date",
					validation.ErrorMessage,
					"OK");
			}
		}
		else
		{
			// Date is valid, clear error and update previous valid date
			StartDateError = string.Empty;
			_previousValidStartDate = value;
		}
	}

	/// <summary>
	/// Called when EndDate property changes. Validates the date and reverts if invalid.
	/// </summary>
	partial void OnEndDateChanged(DateTime value)
	{
		if (_isValidatingDate || IsLoading) return;

		var validation = DateValidationHelper.ValidateDate(value);
		
		if (!validation.IsValid)
		{
			// Show error message
			EndDateError = validation.ErrorMessage;
			
			// Revert to previous valid date
			_isValidatingDate = true;
			EndDate = _previousValidEndDate;
			_isValidatingDate = false;
			
			// Display alert to user
			if (Application.Current?.MainPage is not null)
			{
				_ = Application.Current.MainPage.DisplayAlert(
					"Invalid Date",
					validation.ErrorMessage,
					"OK");
			}
		}
		else
		{
			// Date is valid, clear error and update previous valid date
			EndDateError = string.Empty;
			_previousValidEndDate = value;
		}
	}
}

