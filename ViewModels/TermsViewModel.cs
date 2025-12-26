using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using StudentLifeTracker.Shared.DTOs;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class TermsViewModel : ObservableObject
{
	private readonly DatabaseService _db;
	private readonly NotificationService _notifications;
	private readonly ApiService _apiService;

	[ObservableProperty]
	private ObservableCollection<AcademicTerm> terms = new();

	[ObservableProperty]
	private AcademicTerm? selectedTerm;

	[ObservableProperty]
	private AcademicTerm? currentTerm;

	[ObservableProperty]
	private bool isLoading;

	public TermsViewModel(DatabaseService db, NotificationService notifications, ApiService apiService)
	{
		_db = db;
		_notifications = notifications;
		_apiService = apiService;
	}

	public async Task LoadTermsAsync()
	{
		IsLoading = true;
		try
		{
			Terms.Clear();
			
			// Try to load from API first if authenticated
			if (await _apiService.IsAuthenticatedAsync())
			{
				try
				{
					var response = await _apiService.GetTermsAsync();
					if (response.Success && response.Data != null)
					{
						foreach (var termDto in response.Data)
						{
							var term = ConvertToAcademicTerm(termDto);
							Terms.Add(term);
							// Also save to local database for offline access
							await _db.SaveTermAsync(term);
						}
						return; // Successfully loaded from API
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Failed to load terms from API: {ex.Message}");
					// Fall through to load from local database
				}
			}
			
			// Fallback to local database if API fails or not authenticated
			var all = await _db.GetAllTermsAsync();
			foreach (var t in all) Terms.Add(t);
		}
		finally
		{
			IsLoading = false;
		}
	}

	public async Task AddTermAsync(AcademicTerm term)
	{
		if (!term.IsValid()) throw new InvalidOperationException("Invalid term");
		
		// Save to API first if authenticated
		if (await _apiService.IsAuthenticatedAsync())
		{
			try
			{
				var termDto = ConvertToTermDTO(term);
				var response = await _apiService.CreateTermAsync(termDto);
				if (response.Success && response.Data != null)
				{
					// Update term with server ID and data
					term.Id = response.Data.Id;
					term.Title = response.Data.Title;
					term.StartDate = response.Data.StartDate;
					term.EndDate = response.Data.EndDate;
					term.CreatedAt = response.Data.CreatedAt;
					// Save to local database for offline access
					await _db.SaveTermAsync(term);
					Terms.Add(term);
					return; // Successfully saved to API
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to save term to API: {ex.Message}");
				// Fall through to save locally
			}
		}
		
		// Fallback to local database if API fails or not authenticated
		await _db.SaveTermAsync(term);
		Terms.Add(term);
	}

	public async Task UpdateTermAsync(AcademicTerm term)
	{
		if (!term.IsValid()) throw new InvalidOperationException("Invalid term");
		
		// Update in API first if authenticated
		if (await _apiService.IsAuthenticatedAsync())
		{
			try
			{
				var termDto = ConvertToTermDTO(term);
				var response = await _apiService.UpdateTermAsync(term.Id, termDto);
				if (response.Success && response.Data != null)
				{
					// Update term with server data
					term.Title = response.Data.Title;
					term.StartDate = response.Data.StartDate;
					term.EndDate = response.Data.EndDate;
					// Save to local database for offline access
					await _db.SaveTermAsync(term);
					await LoadTermsAsync();
					return; // Successfully updated in API
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to update term in API: {ex.Message}");
				// Fall through to save locally
			}
		}
		
		// Fallback to local database if API fails or not authenticated
		await _db.SaveTermAsync(term);
		await LoadTermsAsync();
	}

	public async Task DeleteTermAsync(AcademicTerm term)
	{
		// Delete from API first if authenticated
		if (await _apiService.IsAuthenticatedAsync())
		{
			try
			{
				var response = await _apiService.DeleteTermAsync(term.Id);
				if (response.Success)
				{
					// Also delete from local database
					await _db.DeleteTermAsync(term.Id);
					Terms.Remove(term);
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
		await _db.DeleteTermAsync(term.Id);
		Terms.Remove(term);
	}

	public void SetCurrentTerm(AcademicTerm term)
	{
		SelectedTerm = term;
		CurrentTerm = term;
		Preferences.Set("last_term_id", term.Id);
	}

	public void LoadLastViewedTerm()
	{
		var lastId = Preferences.Get("last_term_id", -1);
		if (lastId <= 0) return;
		var found = Terms.FirstOrDefault(t => t.Id == lastId);
		if (found is not null)
		{
			CurrentTerm = found;
			SelectedTerm = found;
		}
	}

	[RelayCommand]
	private async Task TermTappedAsync(AcademicTerm term)
	{
		if (term == null) return;

		SetCurrentTerm(term);
		await Shell.Current.GoToAsync($"{nameof(Views.CourseListPage)}?termId={term.Id}");
	}

	[RelayCommand]
	private async Task AddAsync()
	{
		try
		{
			// Create new term with default values
			var newTerm = new AcademicTerm
			{
				Title = "New Term",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(4)
			};

			// Save to API first if authenticated, otherwise save locally
			if (await _apiService.IsAuthenticatedAsync())
			{
				try
				{
					var termDto = ConvertToTermDTO(newTerm);
					var response = await _apiService.CreateTermAsync(termDto);
					if (response.Success && response.Data != null)
					{
						newTerm.Id = response.Data.Id;
						newTerm.Title = response.Data.Title;
						newTerm.StartDate = response.Data.StartDate;
						newTerm.EndDate = response.Data.EndDate;
						newTerm.CreatedAt = response.Data.CreatedAt;
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Failed to create term in API: {ex.Message}");
				}
			}
			
			await _db.SaveTermAsync(newTerm);
			Terms.Add(newTerm);

			// Navigate to detail page for editing with date pickers
			await Shell.Current.GoToAsync($"{nameof(Views.TermDetailPage)}?termId={newTerm.Id}");
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to add term: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task EditTermAsync(AcademicTerm term)
	{
		if (term == null) return;
		await Shell.Current.GoToAsync($"{nameof(Views.TermDetailPage)}?termId={term.Id}");
	}

	[RelayCommand]
	private async Task DeleteAsync(AcademicTerm term)
	{
		try
		{
			bool confirm = await Application.Current.MainPage.DisplayAlert(
				"Delete Term",
				$"Are you sure you want to delete '{term.Title}'? This will delete all courses and assessments in this term.",
				"Delete",
				"Cancel");

			if (!confirm)
				return;

			await DeleteTermAsync(term);
			await Application.Current.MainPage.DisplayAlert("Success", "Term deleted successfully", "OK");
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete term: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task TestNotificationAsync()
	{
		try
		{
			var result = await _notifications.SendTestNotificationAsync(
				"Test Notification",
				"If you see this notification, notifications are working correctly!");
			
			if (!result)
			{
				await Application.Current.MainPage.DisplayAlert(
					"Notifications Disabled",
					"Notifications are disabled. Please enable them in:\n\nSettings > Apps > Student Progress Tracker > Notifications",
					"OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert(
					"Test Notification Sent",
					"A test notification has been sent. Check your notification tray!",
					"OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to send test notification: {ex.Message}", "OK");
		}
	}

	private static TermDTO ConvertToTermDTO(AcademicTerm term)
	{
		return new TermDTO
		{
			Id = term.Id,
			Title = term.Title,
			StartDate = term.StartDate,
			EndDate = term.EndDate,
			CreatedAt = term.CreatedAt,
			UpdatedAt = DateTime.UtcNow
		};
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







