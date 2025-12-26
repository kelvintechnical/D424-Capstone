using StudentProgressTracker.Helpers;
using StudentProgressTracker.Models;
using StudentProgressTracker.ViewModels;
using StudentLifeTracker.Shared.DTOs;

namespace StudentProgressTracker.Views;

[QueryProperty(nameof(TermId), "termId")]
public partial class CourseListPage : ContentPage
{
	public int TermId
	{
		get => _termId;
		set
		{
			_termId = value;
			_ = LoadAsync();
		}
	}

	private int _termId;

	public CourseListPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<CourseListViewModel>();
	}

	private async Task LoadAsync()
	{
		if (TermId <= 0) return;

		var vm = (CourseListViewModel)BindingContext;
		var apiService = ServiceHelper.GetRequiredService<Services.ApiService>();
		var db = ServiceHelper.GetRequiredService<Services.DatabaseService>();

		AcademicTerm? term = null;

		// Try to load term from API first if authenticated
		if (await apiService.IsAuthenticatedAsync())
		{
			try
			{
				var response = await apiService.GetTermAsync(TermId);
				if (response.Success && response.Data != null)
				{
					var termDto = response.Data;
					term = ConvertToAcademicTerm(termDto);
					// Also save to local database for offline access
					await db.SaveTermAsync(term);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to load term from API: {ex.Message}");
				// Fall through to load from local database
			}
		}

		// Fallback to local database if API fails or not authenticated
		if (term == null)
		{
			term = await db.GetTermAsync(TermId);
		}

		if (term != null)
		{
			vm.SetCurrentTerm(term);
		}

		await vm.LoadCoursesAsync(TermId);
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

	private async void LoginToolbarItem_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//LoginPage");
	}
}







