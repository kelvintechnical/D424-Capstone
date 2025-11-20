using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StudentProgressTracker.ViewModels;

public partial class CourseDetailViewModel : ObservableObject
{
	private readonly DatabaseService _db;
	private readonly NotificationService _notifications;

	[ObservableProperty] private Course? course;
	[ObservableProperty] private ObservableCollection<Assessment> assessments = new();
	[ObservableProperty] private Instructor? instructor;
	[ObservableProperty] private List<string> courseStatusOptions = GetEnumDisplayNames<CourseStatus>();
	[ObservableProperty] private string selectedStatus = GetEnumDisplayName(CourseStatus.InProgress);
	[ObservableProperty] private string? notes;
	[ObservableProperty] private DateTime startDate = DateTime.Today;
	[ObservableProperty] private DateTime endDate = DateTime.Today;
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private bool isSaving;

	public CourseDetailViewModel(DatabaseService db, NotificationService notifications)
	{
		_db = db;
		_notifications = notifications;
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
		StartDate = ConvertUtcToLocal(Course.StartDate);
		EndDate = ConvertUtcToLocal(Course.EndDate);
	}

	public void MapPropertiesToCourse()
	{
		if (Course is null) return;
		// Convert display name back to enum value
		Course.Status = GetEnumValueFromDisplayName<CourseStatus>(SelectedStatus).ToString();
		Course.Notes = Notes;
		Course.StartDate = ConvertLocalToUtc(StartDate);
		Course.EndDate = ConvertLocalToUtc(EndDate);
	}

	public DateTime ConvertUtcToLocal(DateTime utcDate) => utcDate.ToLocalTime().Date;
	public DateTime ConvertLocalToUtc(DateTime localDate) => DateTime.SpecifyKind(localDate, DateTimeKind.Local).ToUniversalTime();

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

			// Send immediate notifications instead of scheduling for future dates
			await _notifications.SendImmediateCourseNotificationsAsync(
				Course.Id,
				Course.Title,
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







