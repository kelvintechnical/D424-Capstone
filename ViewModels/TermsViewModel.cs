using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class TermsViewModel : ObservableObject
{
	private readonly DatabaseService _db;

	[ObservableProperty]
	private ObservableCollection<AcademicTerm> terms = new();

	[ObservableProperty]
	private AcademicTerm? selectedTerm;

	[ObservableProperty]
	private AcademicTerm? currentTerm;

	[ObservableProperty]
	private bool isLoading;

	public TermsViewModel(DatabaseService db)
	{
		_db = db;
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
			// Prompt user for term details
			string title = await Application.Current.MainPage.DisplayPromptAsync(
				"New Term",
				"Enter term title (e.g., Spring 2025):",
				placeholder: "Term Title",
				maxLength: 50);

			if (string.IsNullOrWhiteSpace(title))
				return;

			// Get start date
			var startDateStr = await Application.Current.MainPage.DisplayPromptAsync(
				"Start Date",
				"Enter start date (MM/DD/YYYY):",
				placeholder: "MM/DD/YYYY",
				keyboard: Keyboard.Default);

			if (string.IsNullOrWhiteSpace(startDateStr))
				return;

			if (!DateTime.TryParse(startDateStr, out DateTime startDate))
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Invalid start date format", "OK");
				return;
			}

			// Get end date
			var endDateStr = await Application.Current.MainPage.DisplayPromptAsync(
				"End Date",
				"Enter end date (MM/DD/YYYY):",
				placeholder: "MM/DD/YYYY",
				keyboard: Keyboard.Default);

			if (string.IsNullOrWhiteSpace(endDateStr))
				return;

			if (!DateTime.TryParse(endDateStr, out DateTime endDate))
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Invalid end date format", "OK");
				return;
			}

			var newTerm = new AcademicTerm
			{
				Title = title,
				StartDate = startDate.ToUniversalTime(),
				EndDate = endDate.ToUniversalTime()
			};

			if (!newTerm.IsValid())
			{
				await Application.Current.MainPage.DisplayAlert("Error", "End date must be after start date", "OK");
				return;
			}

			await AddTermAsync(newTerm);
			await Application.Current.MainPage.DisplayAlert("Success", "Term added successfully", "OK");
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to add term: {ex.Message}", "OK");
		}
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
}







