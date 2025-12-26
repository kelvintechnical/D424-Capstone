using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class SearchViewModel : ObservableObject
{
	private readonly SearchService _searchService;

	[ObservableProperty] private string searchQuery = string.Empty;
	[ObservableProperty] private string selectedSearchType = "All"; // All, Terms, Courses
	[ObservableProperty] private string selectedStatus = "All"; // All, InProgress, Completed, Dropped, PlanToTake
	[ObservableProperty] private ObservableCollection<SearchResultDTO> searchResults = new();
	[ObservableProperty] private bool isSearching;
	[ObservableProperty] private bool hasSearched;

	public List<string> SearchTypeOptions { get; } = new() { "All", "Terms", "Courses" };
	public List<string> StatusOptions { get; } = new() { "All", "InProgress", "Completed", "Dropped", "PlanToTake" };

	partial void OnSelectedSearchTypeChanged(string value)
	{
		// Reset status filter when switching away from Courses
		if (value != "Courses" && SelectedStatus != "All")
		{
			SelectedStatus = "All";
		}
	}

	private readonly ApiService _apiService;

	public SearchViewModel(SearchService searchService, ApiService apiService)
	{
		_searchService = searchService;
		_apiService = apiService;
	}

	[RelayCommand]
	private async Task SearchAsync()
	{
		if (string.IsNullOrWhiteSpace(SearchQuery))
		{
			await Application.Current.MainPage.DisplayAlert("Search", "Please enter a search query", "OK");
			return;
		}

		// Check if user is authenticated
		if (!await _apiService.IsAuthenticatedAsync())
		{
			await Application.Current.MainPage.DisplayAlert("Authentication Required", "Please login to search courses and terms.", "OK");
			return;
		}

		IsSearching = true;
		HasSearched = true;
		SearchResults.Clear();

		try
		{
			List<SearchResultDTO> results;

			switch (SelectedSearchType)
			{
				case "Terms":
					results = await _searchService.SearchTermsAsync(SearchQuery);
					break;
				case "Courses":
					var status = SelectedStatus == "All" ? null : SelectedStatus;
					results = await _searchService.SearchCoursesAsync(SearchQuery, status);
					break;
				default: // "All"
					results = await _searchService.SearchAllAsync(SearchQuery);
					break;
			}

			foreach (var result in results)
			{
				SearchResults.Add(result);
			}

			// Don't show alert if no results - the UI already shows "No results found"
			// Only show alert if there was an actual error
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Search error: {ex}");
			await Application.Current.MainPage.DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
		}
		finally
		{
			IsSearching = false;
		}
	}

	[RelayCommand]
	private async Task SelectResultAsync(SearchResultDTO result)
	{
		if (result == null) return;

		try
		{
			if (result.ResultType == "Term")
			{
				await Shell.Current.GoToAsync($"{nameof(Views.TermDetailPage)}?termId={result.Id}");
			}
			else if (result.ResultType == "Course")
			{
				await Shell.Current.GoToAsync($"{nameof(Views.CourseDetailPage)}?courseId={result.Id}");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private void ClearSearchAsync()
	{
		SearchQuery = string.Empty;
		SearchResults.Clear();
		HasSearched = false;
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

