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

			// Get term grades
			var gradesResponse = await _apiService.GetTermGradesAsync(termId);
			if (gradesResponse.Success && gradesResponse.Data != null)
			{
				CoursesWithGrades.Clear();
				foreach (var grade in gradesResponse.Data)
				{
					var points = _gpaService.ConvertLetterToPoints(grade.LetterGrade);
					CoursesWithGrades.Add(new CourseGradeInfo
					{
						CourseId = grade.CourseId,
						CourseTitle = $"Course {grade.CourseId}",
						LetterGrade = grade.LetterGrade,
						Percentage = grade.Percentage,
						CreditHours = grade.CreditHours,
						GradePoints = points
					});
				}
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
	}

	[RelayCommand]
	private async Task CalculateProjectionAsync()
	{
		if (SelectedCourse == null)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "Please select a course", "OK");
			return;
		}

		if (!double.TryParse(CurrentGradeInput, out var currentGrade) || currentGrade < 0 || currentGrade > 100)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid current grade (0-100)", "OK");
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

