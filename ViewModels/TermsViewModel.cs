using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class TermsViewModel : ObservableObject
{
	private readonly DatabaseService _db;
	private readonly NotificationService _notifications;

	[ObservableProperty]
	private ObservableCollection<AcademicTerm> terms = new();

	[ObservableProperty]
	private AcademicTerm? selectedTerm;

	[ObservableProperty]
	private AcademicTerm? currentTerm;

	[ObservableProperty]
	private bool isLoading;

	public TermsViewModel(DatabaseService db, NotificationService notifications)
	{
		_db = db;
		_notifications = notifications;
	}

	public async Task LoadTermsAsync()
	{
		IsLoading = true;
		try
		{
			Terms.Clear();
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
		await _db.SaveTermAsync(term);
		Terms.Add(term);
	}

	public async Task UpdateTermAsync(AcademicTerm term)
	{
		if (!term.IsValid()) throw new InvalidOperationException("Invalid term");
		await _db.SaveTermAsync(term);
		await LoadTermsAsync();
	}

	public async Task DeleteTermAsync(AcademicTerm term)
	{
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

}







