using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class CourseListViewModel : ObservableObject
{
	private readonly DatabaseService _db;

	[ObservableProperty]
	private AcademicTerm? currentTerm;

	[ObservableProperty]
	private ObservableCollection<Course> courses = new();

	[ObservableProperty]
	private Course? selectedCourse;

	[ObservableProperty]
	private bool isLoading;

	[ObservableProperty]
	private bool canAddCourse;

	[ObservableProperty]
	private string courseCountDisplay = "0 of 6 courses";

	public CourseListViewModel(DatabaseService db)
	{
		_db = db;
	}

	public async Task LoadCoursesAsync(int termId)
	{
		IsLoading = true;
		try
		{
			Courses.Clear();
			var list = await _db.GetCoursesByTermAsync(termId);
			foreach (var c in list) Courses.Add(c);

			var count = await _db.GetCourseCountByTermAsync(termId);
			CanAddCourse = count < 6;
			CourseCountDisplay = $"{count} of 6 courses";
		}
		finally
		{
			IsLoading = false;
		}
	}

	public async Task AddCourseAsync(Course course)
	{
		var count = await _db.GetCourseCountByTermAsync(course.TermId);
		if (count >= 6) throw new InvalidOperationException("Cannot add more than 6 courses to a term.");
		if (!course.IsValid()) throw new InvalidOperationException("Invalid course");

		await _db.SaveCourseAsync(course);
		Courses.Add(course);
		await LoadCoursesAsync(course.TermId);
	}

	public async Task UpdateCourseAsync(Course course)
	{
		if (!course.IsValid()) throw new InvalidOperationException("Invalid course");
		await _db.SaveCourseAsync(course);
		await LoadCoursesAsync(course.TermId);
	}

	public async Task DeleteCourseAsync(Course course)
	{
		await _db.DeleteCourseAsync(course.Id);
		Courses.Remove(course);
		var count = await _db.GetCourseCountByTermAsync(course.TermId);
		CanAddCourse = count < 6;
		CourseCountDisplay = $"{count} of 6 courses";
	}

	public void SetCurrentTerm(AcademicTerm term)
	{
		CurrentTerm = term;
		Preferences.Set("last_term_id", term.Id);
	}

	public AcademicTerm? LoadLastTermFromPreferences(IEnumerable<AcademicTerm> availableTerms)
	{
		var lastId = Preferences.Get("last_term_id", -1);
		return availableTerms.FirstOrDefault(t => t.Id == lastId);
	}

	[RelayCommand]
	private async Task CourseTappedAsync(Course course)
	{
		if (course == null) return;

		await Shell.Current.GoToAsync($"{nameof(Views.CourseDetailPage)}?courseId={course.Id}");
	}

	[RelayCommand]
	private async Task AddAsync()
	{
		try
		{
			if (CurrentTerm == null)
			{
				await Application.Current.MainPage.DisplayAlert("Error", "No term selected", "OK");
				return;
			}

			var count = await _db.GetCourseCountByTermAsync(CurrentTerm.Id);
			if (count >= 6)
			{
				await Application.Current.MainPage.DisplayAlert("Limit Reached", "You can only have 6 courses per term", "OK");
				return;
			}

			// Create new course with default instructor
			var defaultInstructor = new Instructor
			{
				Name = "New Instructor",
				Phone = "000-000-0000",
				Email = "instructor@example.com"
			};

			await _db.SaveInstructorAsync(defaultInstructor);

			var newCourse = new Course
			{
				TermId = CurrentTerm.Id,
				Title = "New Course",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(4),
				Status = CourseStatus.PlanToTake.ToString(),
				InstructorId = defaultInstructor.Id,
				Notes = "",
				NotificationsEnabled = true
			};

			await _db.SaveCourseAsync(newCourse);

			// Navigate to detail page for editing
			await Shell.Current.GoToAsync($"{nameof(Views.CourseDetailPage)}?courseId={newCourse.Id}");
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to create course: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private async Task DeleteAsync(Course course)
	{
		try
		{
			bool confirm = await Application.Current.MainPage.DisplayAlert(
				"Delete Course",
				$"Are you sure you want to delete '{course.Title}'? This will delete all assessments for this course.",
				"Delete",
				"Cancel");

			if (!confirm)
				return;

			await DeleteCourseAsync(course);
			await Application.Current.MainPage.DisplayAlert("Success", "Course deleted successfully", "OK");
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete course: {ex.Message}", "OK");
		}
	}

	[RelayCommand]
	private void SelectCourse(Course course)
	{
		SelectedCourse = course;
	}

	[RelayCommand]
	private async Task GoBackAsync()
	{
		await Shell.Current.GoToAsync("..");
	}
}







