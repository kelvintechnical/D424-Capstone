using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using StudentLifeTracker.Shared.DTOs;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StudentProgressTracker.ViewModels;

public partial class CourseDetailViewModel : ObservableObject
{
	private readonly DatabaseService _db;
	private readonly NotificationService _notifications;
	private readonly ApiService _apiService;
	private readonly GPAService _gpaService;

	[ObservableProperty] private Course? course;
	[ObservableProperty] private ObservableCollection<Assessment> assessments = new();
	[ObservableProperty] private Instructor? instructor;
	[ObservableProperty] private List<string> courseStatusOptions = GetEnumDisplayNames<CourseStatus>();
	[ObservableProperty] private string selectedStatus = GetEnumDisplayName(CourseStatus.InProgress);
	[ObservableProperty] private string? notes;
	[ObservableProperty] private DateTime startDate = DateTime.Today;
	[ObservableProperty] private DateTime endDate = DateTime.Today;
	[ObservableProperty] private TimeSpan startTime = TimeSpan.FromHours(9);
	[ObservableProperty] private TimeSpan endTime = TimeSpan.FromHours(9);
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private bool isSaving;
	[ObservableProperty] private int creditHours = 3;
	[ObservableProperty] private string currentGrade = string.Empty;
	[ObservableProperty] private string letterGrade = string.Empty;
	[ObservableProperty] private string gradePoints = "0.0";

	public CourseDetailViewModel(DatabaseService db, NotificationService notifications, ApiService apiService, GPAService gpaService)
	{
		_db = db;
		_notifications = notifications;
		_apiService = apiService;
		_gpaService = gpaService;
	}

	partial void OnCurrentGradeChanged(string value)
	{
		UpdateGradeDisplay();
	}

	partial void OnCreditHoursChanged(int value)
	{
		UpdateGradeDisplay();
	}

	private void UpdateGradeDisplay()
	{
		if (double.TryParse(CurrentGrade, out var percent) && percent >= 0 && percent <= 100)
		{
			LetterGrade = _gpaService.ConvertPercentToLetter(percent);
			var points = _gpaService.ConvertLetterToPoints(LetterGrade);
			GradePoints = points.ToString("F1");
		}
		else
		{
			LetterGrade = string.Empty;
			GradePoints = "0.0";
		}
	}

	public async Task LoadCourseAsync(int courseId)
	{
		IsLoading = true;
		try
		{
			var c = await _db.GetCourseAsync(courseId);
			if (c is null) return;
			Course = c;
			await LoadAssessmentsAsync(courseId);
			Instructor = c.Instructor;
			MapCourseToProperties();
		}
		finally
		{
			IsLoading = false;
		}
	}

	public async Task LoadAssessmentsAsync(int courseId)
	{
		Assessments.Clear();
		var list = await _db.GetAssessmentsByCourseAsync(courseId);
		foreach (var a in list) Assessments.Add(a);
	}

	public void MapCourseToProperties()
	{
		if (Course is null) return;
		// Convert enum value to display name
		if (Enum.TryParse<CourseStatus>(Course.Status, out var statusEnum))
		{
			SelectedStatus = GetEnumDisplayName(statusEnum);
		}
		else
		{
			SelectedStatus = Course.Status;
		}
		Notes = Course.Notes;
		CreditHours = Course.CreditHours;
		CurrentGrade = Course.CurrentGrade?.ToString() ?? string.Empty;
		LetterGrade = Course.LetterGrade ?? string.Empty;
		var localStart = ConvertUtcToLocal(Course.StartDate);
		var localEnd = ConvertUtcToLocal(Course.EndDate);
		StartDate = localStart.Date;
		StartTime = localStart.TimeOfDay;
		EndDate = localEnd.Date;
		EndTime = localEnd.TimeOfDay;
		UpdateGradeDisplay();
	}

	public void MapPropertiesToCourse()
	{
		if (Course is null) return;
		// Convert display name back to enum value
		Course.Status = GetEnumValueFromDisplayName<CourseStatus>(SelectedStatus).ToString();
		Course.Notes = Notes;
		Course.CreditHours = CreditHours;
		if (double.TryParse(CurrentGrade, out var grade))
		{
			Course.CurrentGrade = grade;
			Course.LetterGrade = _gpaService.ConvertPercentToLetter(grade);
		}
		var localStart = CombineDateAndTime(StartDate, StartTime);
		var localEnd = CombineDateAndTime(EndDate, EndTime);
		Course.StartDate = ConvertLocalToUtc(localStart);
		Course.EndDate = ConvertLocalToUtc(localEnd);
	}

	public DateTime ConvertUtcToLocal(DateTime utcDate) => utcDate.ToLocalTime();
	public DateTime ConvertLocalToUtc(DateTime localDateTime) => DateTime.SpecifyKind(localDateTime, DateTimeKind.Local).ToUniversalTime();
	private static DateTime CombineDateAndTime(DateTime date, TimeSpan time) =>
		DateTime.SpecifyKind(date.Date + time, DateTimeKind.Local);

	public async Task SaveCourseAsync()
	{
		if (Course is null) return;
		IsSaving = true;
		try
		{
			MapPropertiesToCourse();
			if (Instructor is null || !new Models.Instructor
			{
				Id = Course.InstructorId,
				Name = Instructor.Name,
				Phone = Instructor.Phone,
				Email = Instructor.Email
			}.IsValid())
			{
				throw new InvalidOperationException("Instructor information is invalid.");
			}

			if (!Course.IsValid()) throw new InvalidOperationException("Invalid course.");

			Instructor.Id = Course.InstructorId;
			await _db.SaveInstructorAsync(Instructor);
			Course.InstructorId = Instructor.Id;
			await _db.SaveCourseAsync(Course);

			// Schedule notifications for course start/end using the exact user-selected times
			await _notifications.ScheduleCourseNotificationsAsync(
				Course.Id,
				Course.Title,
				Course.StartDate,
				Course.EndDate,
				Course.NotificationsEnabled);
		}
		finally
		{
			IsSaving = false;
		}
	}

	public async Task DeleteCourseAsync()
	{
		if (Course is null) return;
		await _db.DeleteCourseAsync(Course.Id);
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		try
		{
			await SaveCourseAsync();
			await Application.Current.MainPage.DisplayAlert("Success", "Course saved successfully", "OK");
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save course: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task DeleteAsync()
	{
		try
		{
			if (Course == null) return;

			bool confirm = await Application.Current.MainPage.DisplayAlert(
				"Delete Course",
				$"Are you sure you want to delete '{Course.Title}'? This will delete all assessments for this course.",
				"Delete",
				"Cancel");

			if (!confirm)
				return;

			await DeleteCourseAsync();
			await Application.Current.MainPage.DisplayAlert("Success", "Course deleted successfully", "OK");
			await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete course: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task ShareNotesAsync()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(Notes))
			{
				await Application.Current.MainPage.DisplayAlert("No Notes", "There are no notes to share", "OK");
				return;
			}

			await Share.Default.RequestAsync(new ShareTextRequest
			{
				Text = Notes,
				Title = "Share Course Notes"
			});
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to share notes: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task AssessmentTappedAsync(Assessment assessment)
	{
		if (assessment == null || Course == null) return;

		await Shell.Current.GoToAsync($"{nameof(Views.AssessmentsPage)}?courseId={Course.Id}");
	}

	[RelayCommand]
	private async Task ManageAssessmentsAsync()
	{
		if (Course == null) return;

		await Shell.Current.GoToAsync($"{nameof(Views.AssessmentsPage)}?courseId={Course.Id}");
	}

	[RelayCommand]
	private async Task GoBackAsync()
	{
		await Shell.Current.GoToAsync("..");
	}

	[RelayCommand]
	private async Task SaveGradeAsync()
	{
		if (Course == null)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "No course selected", "OK");
			return;
		}

		if (!double.TryParse(CurrentGrade, out var grade) || grade < 0 || grade > 100)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid grade percentage (0-100)", "OK");
			return;
		}

		if (CreditHours <= 0 || CreditHours > 10)
		{
			await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid credit hours (1-10)", "OK");
			return;
		}

		try
		{
			var letterGrade = _gpaService.ConvertPercentToLetter(grade);
			var gradeDto = new GradeDTO
			{
				CourseId = Course.Id,
				LetterGrade = letterGrade,
				Percentage = (decimal)grade,
				CreditHours = CreditHours
			};

			var response = await _apiService.SaveGradeAsync(gradeDto);
			if (response.Success && response.Data != null)
			{
				// Update local course
				Course.CurrentGrade = grade;
				Course.LetterGrade = letterGrade;
				Course.CreditHours = CreditHours;
				await _db.SaveCourseAsync(Course);

				LetterGrade = letterGrade;
				UpdateGradeDisplay();
				await Application.Current.MainPage.DisplayAlert("Success", "Grade saved successfully", "OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Error", response.Message ?? "Failed to save grade", "OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save grade: {ex.Message}", "OK");
		}
	}

	// Helper methods to read Display attribute from enum values
	private static List<string> GetEnumDisplayNames<TEnum>() where TEnum : Enum
	{
		return Enum.GetValues(typeof(TEnum))
			.Cast<TEnum>()
			.Select(e => GetEnumDisplayName(e))
			.ToList();
	}

	private static string GetEnumDisplayName<TEnum>(TEnum enumValue) where TEnum : Enum
	{
		var member = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
		var displayAttribute = member?.GetCustomAttribute<DisplayAttribute>();
		return displayAttribute?.Name ?? enumValue.ToString();
	}

	private static TEnum GetEnumValueFromDisplayName<TEnum>(string displayName) where TEnum : struct, Enum
	{
		foreach (var enumValue in Enum.GetValues<TEnum>())
		{
			if (GetEnumDisplayName(enumValue) == displayName)
			{
				return enumValue;
			}
		}
		// Fallback: try to parse as enum name
		return Enum.TryParse<TEnum>(displayName, out var result) ? result : default;
	}
}







