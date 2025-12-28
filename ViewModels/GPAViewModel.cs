using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class GPAViewModel : ObservableObject
{
	private readonly ApiService _apiService;
	private readonly GPAService _gpaService;

	[ObservableProperty] private double currentTermGPA;
	[ObservableProperty] private ObservableCollection<CourseGradeInfo> coursesWithGrades = new();
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private int? selectedTermId;
	[ObservableProperty] private CourseGradeInfo? selectedCourse;
	[ObservableProperty] private string currentGradeInput = string.Empty;
	[ObservableProperty] private string finalWeightInput = string.Empty;
	[ObservableProperty] private string selectedTargetGrade = "B";
	[ObservableProperty] private string projectionResult = string.Empty;
	[ObservableProperty] private bool isCalculatingProjection;

	public List<string> TargetGradeOptions { get; } = new() { "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-", "F" };

	public GPAViewModel(ApiService apiService, GPAService gpaService)
	{
		_apiService = apiService;
		_gpaService = gpaService;
	}

	public async Task LoadGPADataAsync(int termId)
	{
		IsLoading = true;
		SelectedTermId = termId;
		try
		{
			// Get term GPA
			var gpaResponse = await _apiService.GetTermGPAAsync(termId);
			if (gpaResponse.Success && gpaResponse.Data != null)
			{
				CurrentTermGPA = gpaResponse.Data.GPA;
			}

			// Get all courses for the term (for dropdown)
			var coursesResponse = await _apiService.GetCoursesByTermAsync(termId);
			var courses = new Dictionary<int, CourseDTO>();
			if (coursesResponse.Success && coursesResponse.Data != null)
			{
				foreach (var course in coursesResponse.Data)
				{
					courses[course.Id] = course;
				}
			}

			// Get term grades
			var gradesResponse = await _apiService.GetTermGradesAsync(termId);
			var gradesByCourseId = new Dictionary<int, GradeDTO>();
			if (gradesResponse.Success && gradesResponse.Data != null)
			{
				foreach (var grade in gradesResponse.Data)
				{
					gradesByCourseId[grade.CourseId] = grade;
				}
			}

			// Populate CoursesWithGrades - include all courses, with grade info if available
			CoursesWithGrades.Clear();
			foreach (var course in courses.Values)
			{
				var grade = gradesByCourseId.ContainsKey(course.Id) ? gradesByCourseId[course.Id] : null;
				var points = grade != null ? _gpaService.ConvertLetterToPoints(grade.LetterGrade) : 0.0;
				
				CoursesWithGrades.Add(new CourseGradeInfo
				{
					CourseId = course.Id,
					CourseTitle = course.Title,
					LetterGrade = grade?.LetterGrade ?? string.Empty,
					Percentage = grade?.Percentage,
					CreditHours = grade?.CreditHours ?? course.CreditHours,
					GradePoints = points
				});
			}

			// If no courses found, show message
			if (CoursesWithGrades.Count == 0)
			{
				await Application.Current.MainPage.DisplayAlert("No Courses", "No courses found for this term. Please add courses first.", "OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load GPA data: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	partial void OnSelectedCourseChanged(CourseGradeInfo? value)
	{
		if (value != null && value.Percentage.HasValue)
		{
			CurrentGradeInput = value.Percentage.Value.ToString("F1");
		}
		else if (value != null)
		{
			// Course selected but no grade recorded yet
			CurrentGradeInput = string.Empty;
		}
		else
		{
			// No course selected
			CurrentGradeInput = string.Empty;
		}
	}

	[RelayCommand]
	private async Task CalculateProjectionAsync()
	{
		if (SelectedCourse == null)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "Please select a course", "OK");
			return;
		}

		// Use the actual grade from the selected course
		if (!SelectedCourse.Percentage.HasValue)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "The selected course does not have a grade recorded yet. Please record a grade first.", "OK");
			return;
		}

		var currentGrade = (double)SelectedCourse.Percentage.Value;
		
		// Validate the grade is in valid range (should always be if from API, but double-check)
		if (currentGrade < 0 || currentGrade > 100)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "The course grade is invalid (must be 0-100)", "OK");
			return;
		}

		if (!double.TryParse(FinalWeightInput, out var finalWeight) || finalWeight <= 0 || finalWeight > 1)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid final weight (0-1, e.g., 0.3 for 30%)", "OK");
			return;
		}

		IsCalculatingProjection = true;
		try
		{
			var response = await _apiService.GetGradeProjectionAsync(
				SelectedCourse.CourseId,
				currentGrade,
				finalWeight,
				SelectedTargetGrade);

			if (response.Success && response.Data != null)
			{
				var data = response.Data;
				if (data.IsAchievable)
				{
					ProjectionResult = $"You need {data.NeededOnFinal:F1}% on the final to get a {data.TargetGrade}.";
				}
				else
				{
					ProjectionResult = $"It is not possible to achieve a {data.TargetGrade} with the current grade and final weight.";
				}
			}
			else
			{
				ProjectionResult = response.Message ?? "Failed to calculate projection";
			}
		}
		catch (Exception ex)
		{
			ProjectionResult = $"Error: {ex.Message}";
		}
		finally
		{
			IsCalculatingProjection = false;
		}
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
	private async Task ExportTranscriptAsync()
	{
		try
		{
			IsLoading = true;

			var csvBytes = await _apiService.DownloadTranscriptCsvAsync();

			if (csvBytes == null || csvBytes.Length == 0)
			{
				await Shell.Current.DisplayAlert("Error", "No data to export. Add some courses with grades first.", "OK");
				return;
			}

			// Create filename with timestamp
			var fileName = $"Transcript_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
			var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

			// Write file
			await File.WriteAllBytesAsync(filePath, csvBytes);

			// Share/save the file
			await Share.Default.RequestAsync(new ShareFileRequest
			{
				Title = "Export Academic Transcript",
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

	[RelayCommand]
	private async Task ExportTermGpaAsync()
	{
		if (SelectedTermId == null || !SelectedTermId.HasValue)
		{
			await Shell.Current.DisplayAlert("Error", "Please select a term first", "OK");
			return;
		}

		try
		{
			IsLoading = true;

			var csvBytes = await _apiService.DownloadGpaReportCsvAsync(SelectedTermId.Value);

			if (csvBytes == null || csvBytes.Length == 0)
			{
				await Shell.Current.DisplayAlert("Error", "No grades found for this term.", "OK");
				return;
			}

			// Try to get term title for filename
			string termTitle = $"Term_{SelectedTermId.Value}";
			try
			{
				var termResponse = await _apiService.GetTermAsync(SelectedTermId.Value);
				if (termResponse.Success && termResponse.Data != null)
				{
					termTitle = termResponse.Data.Title.Replace(" ", "_");
				}
			}
			catch
			{
				// If we can't get term title, use the default
			}

			var fileName = $"GPA_Report_{termTitle}_{DateTime.Now:yyyyMMdd}.csv";
			var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

			await File.WriteAllBytesAsync(filePath, csvBytes);

			await Share.Default.RequestAsync(new ShareFileRequest
			{
				Title = $"Export GPA Report - {termTitle.Replace("_", " ")}",
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
}

public class CourseGradeInfo
{
	public int CourseId { get; set; }
	public string CourseTitle { get; set; } = string.Empty;
	public string LetterGrade { get; set; } = string.Empty;
	public decimal? Percentage { get; set; }
	public int CreditHours { get; set; }
	public double GradePoints { get; set; }
}


