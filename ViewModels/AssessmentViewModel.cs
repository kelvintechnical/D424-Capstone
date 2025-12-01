using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class AssessmentViewModel : ObservableObject
{
	private readonly DatabaseService _db;
	private readonly NotificationService _notifications;

	[ObservableProperty] private ObservableCollection<Assessment> assessments = new();
	[ObservableProperty] private Assessment? objectiveAssessment;
	[ObservableProperty] private Assessment? performanceAssessment;
	[ObservableProperty] private bool canAddAssessment;
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private int courseId;
	[ObservableProperty] private DateTime objectiveStartDate = DateTime.Today;
	[ObservableProperty] private DateTime objectiveDueDate = DateTime.Today.AddDays(7);
	[ObservableProperty] private TimeSpan objectiveStartTime = TimeSpan.FromHours(9);
	[ObservableProperty] private TimeSpan objectiveDueTime = TimeSpan.FromHours(9);
	[ObservableProperty] private DateTime performanceStartDate = DateTime.Today;
	[ObservableProperty] private DateTime performanceDueDate = DateTime.Today.AddDays(7);
	[ObservableProperty] private TimeSpan performanceStartTime = TimeSpan.FromHours(9);
	[ObservableProperty] private TimeSpan performanceDueTime = TimeSpan.FromHours(9);

	partial void OnObjectiveAssessmentChanged(Assessment? value) => SyncObjectiveEditors();
	partial void OnPerformanceAssessmentChanged(Assessment? value) => SyncPerformanceEditors();

	public AssessmentViewModel(DatabaseService db, NotificationService notifications)
	{
		_db = db;
		_notifications = notifications;
	}

	public async Task LoadAssessmentsAsync(int courseId)
	{
		IsLoading = true;
		CourseId = courseId;
		try
		{
			Assessments.Clear();
			var list = await _db.GetAssessmentsByCourseAsync(courseId);
			foreach (var a in list)
			{
				// Convert UTC dates to local for display
				a.StartDate = ConvertUtcToLocal(a.StartDate);
				a.DueDate = ConvertUtcToLocal(a.DueDate);
				Assessments.Add(a);
			}
			ObjectiveAssessment = list.FirstOrDefault(a => a.Type == AssessmentType.Objective.ToString());
			PerformanceAssessment = list.FirstOrDefault(a => a.Type == AssessmentType.Performance.ToString());
			CanAddAssessment = list.Count < 2;
		}
		finally
		{
			IsLoading = false;
		}
	}

	public async Task SaveAssessmentAsync(Assessment assessment)
	{
		if (!ValidateAssessmentType(assessment))
			throw new InvalidOperationException("Assessment type must be Objective or Performance.");
		if (assessment.CourseId <= 0) assessment.CourseId = CourseId;

		assessment.StartDate = ConvertLocalToUtc(assessment.StartDate);
		assessment.DueDate = ConvertLocalToUtc(assessment.DueDate);

		if (!assessment.IsValid())
			throw new InvalidOperationException("Invalid assessment data.");

		var count = await _db.GetAssessmentCountByCourseAsync(assessment.CourseId);
		var existing = await _db.GetAssessmentsByCourseAsync(assessment.CourseId);
		var sameTypeExists = existing.Any(a => a.Type == assessment.Type && a.Id != assessment.Id);
		if (sameTypeExists)
			throw new InvalidOperationException("Each course can have only one Objective and one Performance assessment.");
		if (count >= 2 && assessment.Id == 0)
			throw new InvalidOperationException("Cannot add more than 2 assessments for a course.");

		await _db.SaveAssessmentAsync(assessment);
		// Schedule notifications for assessment start/due at 9 AM local time
		await _notifications.ScheduleAssessmentNotificationsAsync(
			assessment.Id,
			assessment.Name,
			assessment.StartDate,
			assessment.DueDate,
			assessment.NotificationsEnabled);

		await LoadAssessmentsAsync(assessment.CourseId);
	}

	public async Task DeleteAssessmentAsync(Assessment assessment)
	{
		await _db.DeleteAssessmentAsync(assessment.Id);
		await LoadAssessmentsAsync(assessment.CourseId);
	}

	public bool ValidateAssessmentType(Assessment assessment)
	{
		return assessment.Type == AssessmentType.Objective.ToString() ||
		       assessment.Type == AssessmentType.Performance.ToString();
	}

	public DateTime ConvertUtcToLocal(DateTime utcDate)
	{
		var utc = utcDate.Kind switch
		{
			DateTimeKind.Utc => utcDate,
			DateTimeKind.Local => utcDate.ToUniversalTime(),
			_ => DateTime.SpecifyKind(utcDate, DateTimeKind.Utc)
		};
		return utc.ToLocalTime();
	}

	public DateTime ConvertLocalToUtc(DateTime localDate) => DateTime.SpecifyKind(localDate, DateTimeKind.Local).ToUniversalTime();

	[RelayCommand]
	private async Task AddObjectiveAsync()
	{
		try
		{
			if (ObjectiveAssessment is not null)
			{
                await Application.Current.Windows[0].Page.DisplayAlert("Already Exists", "Objective assessment already exists. Please edit or delete it first.", "OK");
				return;
			}

			var a = new Assessment
			{
				CourseId = CourseId,
				Name = "Objective Assessment",
				Type = AssessmentType.Objective.ToString(),
				StartDate = CombineDateAndTime(DateTime.Today, TimeSpan.FromHours(9)),
				DueDate = CombineDateAndTime(DateTime.Today.AddDays(7), TimeSpan.FromHours(9)),
				NotificationsEnabled = true,
				CreatedAt = DateTime.UtcNow
			};
			await SaveAssessmentAsync(a);
            await Application.Current.Windows[0].Page.DisplayAlert("Success", "Objective assessment added", "OK");
		}
		catch (Exception ex)
		{
            await Application.Current.Windows[0].Page.DisplayAlert("Error", $"Failed to add assessment: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task AddPerformanceAsync()
	{
		try
		{
			if (PerformanceAssessment is not null)
			{
                await Application.Current.Windows[0].Page.DisplayAlert("Already Exists", "Performance assessment already exists. Please edit or delete it first.", "OK");
				return;
			}

			var a = new Assessment
			{
				CourseId = CourseId,
				Name = "Performance Assessment",
				Type = AssessmentType.Performance.ToString(),
				StartDate = CombineDateAndTime(DateTime.Today, TimeSpan.FromHours(9)),
				DueDate = CombineDateAndTime(DateTime.Today.AddDays(7), TimeSpan.FromHours(9)),
				NotificationsEnabled = true,
				CreatedAt = DateTime.UtcNow
			};
			await SaveAssessmentAsync(a);
            await Application.Current.Windows[0].Page.DisplayAlert("Success", "Performance assessment added", "OK");
		}
		catch (Exception ex)
		{
            await Application.Current.Windows[0].Page.DisplayAlert("Error", $"Failed to add assessment: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task SaveObjectiveAsync()
	{
		try
		{
			if (ObjectiveAssessment is null) return;
			ApplyObjectiveEditors();
			await SaveAssessmentAsync(ObjectiveAssessment);
            await Application.Current.Windows[0].Page.DisplayAlert("Success", "Objective assessment saved", "OK");
		}
		catch (Exception ex)
		{
            await Application.Current.Windows[0].Page.DisplayAlert("Error", $"Failed to save assessment: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task SavePerformanceAsync()
	{
		try
		{
			if (PerformanceAssessment is null) return;
			ApplyPerformanceEditors();
			await SaveAssessmentAsync(PerformanceAssessment);
			await Application.Current.Windows[0].Page.DisplayAlert("Success", "Performance assessment saved", "OK");
		}
		catch (Exception ex)
		{
			await Application.Current.Windows[0].Page.DisplayAlert("Error", $"Failed to save assessment: {ex.Message}", "OK");
		}
	}

	private void SyncObjectiveEditors()
	{
		if (ObjectiveAssessment is null)
		{
			ResetObjectiveEditors();
			return;
		}

		var localStart = NormalizeLocal(ObjectiveAssessment.StartDate);
		var localDue = NormalizeLocal(ObjectiveAssessment.DueDate);
		ObjectiveStartDate = localStart.Date;
		ObjectiveStartTime = localStart.TimeOfDay;
		ObjectiveDueDate = localDue.Date;
		ObjectiveDueTime = localDue.TimeOfDay;
	}

	private void SyncPerformanceEditors()
	{
		if (PerformanceAssessment is null)
		{
			ResetPerformanceEditors();
			return;
		}

		var localStart = NormalizeLocal(PerformanceAssessment.StartDate);
		var localDue = NormalizeLocal(PerformanceAssessment.DueDate);
		PerformanceStartDate = localStart.Date;
		PerformanceStartTime = localStart.TimeOfDay;
		PerformanceDueDate = localDue.Date;
		PerformanceDueTime = localDue.TimeOfDay;
	}

	private void ApplyObjectiveEditors()
	{
		if (ObjectiveAssessment is null) return;
		ObjectiveAssessment.StartDate = CombineDateAndTime(ObjectiveStartDate, ObjectiveStartTime);
		ObjectiveAssessment.DueDate = CombineDateAndTime(ObjectiveDueDate, ObjectiveDueTime);
	}

	private void ApplyPerformanceEditors()
	{
		if (PerformanceAssessment is null) return;
		PerformanceAssessment.StartDate = CombineDateAndTime(PerformanceStartDate, PerformanceStartTime);
		PerformanceAssessment.DueDate = CombineDateAndTime(PerformanceDueDate, PerformanceDueTime);
	}

	private void ResetObjectiveEditors()
	{
		ObjectiveStartDate = DateTime.Today;
		ObjectiveStartTime = TimeSpan.FromHours(9);
		ObjectiveDueDate = DateTime.Today.AddDays(7);
		ObjectiveDueTime = TimeSpan.FromHours(9);
	}

	private void ResetPerformanceEditors()
	{
		PerformanceStartDate = DateTime.Today;
		PerformanceStartTime = TimeSpan.FromHours(9);
		PerformanceDueDate = DateTime.Today.AddDays(7);
		PerformanceDueTime = TimeSpan.FromHours(9);
	}

	private static DateTime CombineDateAndTime(DateTime date, TimeSpan time) =>
		DateTime.SpecifyKind(date.Date + time, DateTimeKind.Local);

	private static DateTime NormalizeLocal(DateTime dateTime)
	{
		return dateTime.Kind switch
		{
			DateTimeKind.Local => dateTime,
			DateTimeKind.Utc => dateTime.ToLocalTime(),
			_ => DateTime.SpecifyKind(dateTime, DateTimeKind.Local)
		};
	}

	[RelayCommand]
	private async Task DeleteObjectiveAsync()
	{
		try
		{
			if (ObjectiveAssessment is null) return;

			bool confirm = await Application.Current.Windows[0].Page.DisplayAlert(
				"Delete Assessment",
				$"Are you sure you want to delete '{ObjectiveAssessment.Name}'?",
				"Delete",
				"Cancel");

			if (!confirm) return;

			await DeleteAssessmentAsync(ObjectiveAssessment);
            await Application.Current.Windows[0].Page.DisplayAlert("Success", "Objective assessment deleted", "OK");
		}
		catch (Exception ex)
		{
            await Application.Current.Windows[0].Page.DisplayAlert("Error", $"Failed to delete assessment: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task DeletePerformanceAsync()
	{
		try
		{
			if (PerformanceAssessment is null) return;

			bool confirm = await Application.Current.Windows[0].Page.DisplayAlert(
				"Delete Assessment",
				$"Are you sure you want to delete '{PerformanceAssessment.Name}'?",
				"Delete",
				"Cancel");

			if (!confirm) return;

			await DeleteAssessmentAsync(PerformanceAssessment);
            await Application.Current.Windows[0].Page.DisplayAlert("Success", "Performance assessment deleted", "OK");
		}
		catch (Exception ex)
		{
            await Application.Current.Windows[0].Page.DisplayAlert("Error", $"Failed to delete assessment: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task GoBackAsync()
	{
		await Shell.Current.GoToAsync("..");
	}
}







