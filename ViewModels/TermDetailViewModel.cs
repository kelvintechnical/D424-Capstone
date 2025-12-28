using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Helpers;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using StudentLifeTracker.Shared.DTOs;

namespace StudentProgressTracker.ViewModels;

public partial class TermDetailViewModel : ObservableObject
{
	private readonly DatabaseService _db;
	private readonly ApiService _apiService;
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

	public TermDetailViewModel(DatabaseService db, ApiService apiService)
	{
		_db = db;
		_apiService = apiService;
	}

	public async Task LoadTermAsync(int termId)
	{
		IsLoading = true;
		try
		{
			// Try to load from API first if authenticated
			if (await _apiService.IsAuthenticatedAsync())
			{
				try
				{
					var response = await _apiService.GetTermAsync(termId);
					if (response.Success && response.Data != null)
					{
						var termDto = response.Data;
						var t = ConvertToAcademicTerm(termDto);
						Term = t;
						Title = t.Title;
						var localStartDate = ConvertUtcToLocal(t.StartDate);
						var localEndDate = ConvertUtcToLocal(t.EndDate);
						
						// Store as previous valid dates
						_previousValidStartDate = localStartDate;
						_previousValidEndDate = localEndDate;
						
						StartDate = localStartDate;
						EndDate = localEndDate;
						// Also save to local database for offline access
						await _db.SaveTermAsync(t);
						return; // Successfully loaded from API
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Failed to load term from API: {ex.Message}");
					// Fall through to load from local database
				}
			}
			
			// Fallback to local database if API fails or not authenticated
			var localTerm = await _db.GetTermAsync(termId);
			if (localTerm is null) return;
			Term = localTerm;
			Title = localTerm.Title;
			var fallbackStartDate = ConvertUtcToLocal(localTerm.StartDate);
			var fallbackEndDate = ConvertUtcToLocal(localTerm.EndDate);
			
			// Store as previous valid dates
			_previousValidStartDate = fallbackStartDate;
			_previousValidEndDate = fallbackEndDate;
			
			StartDate = fallbackStartDate;
			EndDate = fallbackEndDate;
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
			
			// Save to API first if authenticated
			if (await _apiService.IsAuthenticatedAsync())
			{
				try
				{
					var termDto = new TermDTO
					{
						Id = Term.Id,
						Title = Term.Title,
						StartDate = Term.StartDate,
						EndDate = Term.EndDate,
						CreatedAt = Term.CreatedAt,
						UpdatedAt = DateTime.UtcNow
					};
					
					if (Term.Id == 0)
					{
						var response = await _apiService.CreateTermAsync(termDto);
						if (response.Success && response.Data != null && response.Data.Id > 0)
						{
							// Update term with server data
							Term.Id = response.Data.Id;
							Term.Title = response.Data.Title;
							Term.StartDate = response.Data.StartDate;
							Term.EndDate = response.Data.EndDate;
							Term.CreatedAt = response.Data.CreatedAt;
						}
					}
					else
					{
						var response = await _apiService.UpdateTermAsync(Term.Id, termDto);
						if (response.Success && response.Data != null)
						{
							// Update term with server data
							Term.Title = response.Data.Title;
							Term.StartDate = response.Data.StartDate;
							Term.EndDate = response.Data.EndDate;
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Failed to save term to API: {ex.Message}");
					// Fall through to save locally
				}
			}
			
			// Save to local database for offline access (or if API failed)
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
		
		// Delete from API first if authenticated
		if (await _apiService.IsAuthenticatedAsync() && Term.Id > 0)
		{
			try
			{
				var response = await _apiService.DeleteTermAsync(Term.Id);
				if (response.Success)
				{
					// Also delete from local database
					await _db.DeleteTermAsync(Term.Id);
					return; // Successfully deleted from API
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to delete term from API: {ex.Message}");
				// Fall through to delete locally
			}
		}
		
		// Fallback to local database if API fails or not authenticated
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

	[RelayCommand]
	private async Task ExportTermGpaAsync()
	{
		if (Term == null || Term.Id <= 0)
		{
			await Shell.Current.DisplayAlert("Error", "Invalid term. Please save the term first.", "OK");
			return;
		}

		try
		{
			IsLoading = true;

			var csvBytes = await _apiService.DownloadGpaReportCsvAsync(Term.Id);

			if (csvBytes == null || csvBytes.Length == 0)
			{
				await Shell.Current.DisplayAlert("Error", "No grades found for this term.", "OK");
				return;
			}

			// Use term title for filename
			string termTitle = Term.Title.Replace(" ", "_");
			var fileName = $"GPA_Report_{termTitle}_{DateTime.Now:yyyyMMdd}.csv";
			var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

			await File.WriteAllBytesAsync(filePath, csvBytes);

			await Share.Default.RequestAsync(new ShareFileRequest
			{
				Title = $"Export GPA Report - {Term.Title}",
				File = new ShareFile(filePath)
			});
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
			await Shell.Current.DisplayAlert("Error", $"Failed to export: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
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

	private static AcademicTerm ConvertToAcademicTerm(TermDTO termDto)
	{
		return new AcademicTerm
		{
			Id = termDto.Id,
			Title = termDto.Title,
			StartDate = termDto.StartDate,
			EndDate = termDto.EndDate,
			CreatedAt = termDto.CreatedAt
		};
	}
}

