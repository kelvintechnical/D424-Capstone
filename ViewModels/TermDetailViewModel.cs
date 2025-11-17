using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;

namespace StudentProgressTracker.ViewModels;

public partial class TermDetailViewModel : ObservableObject
{
	private readonly DatabaseService _db;

	[ObservableProperty] private AcademicTerm? term;
	[ObservableProperty] private string title = string.Empty;
	[ObservableProperty] private DateTime startDate = DateTime.Today;
	[ObservableProperty] private DateTime endDate = DateTime.Today;
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private bool isSaving;

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
			StartDate = ConvertUtcToLocal(t.StartDate);
			EndDate = ConvertUtcToLocal(t.EndDate);
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
}

