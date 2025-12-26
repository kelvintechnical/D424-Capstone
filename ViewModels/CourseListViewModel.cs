using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services;
using StudentLifeTracker.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Linq;

namespace StudentProgressTracker.ViewModels;

public partial class CourseListViewModel : ObservableObject
{
	private readonly DatabaseService _db;
	private readonly ApiService _apiService;

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

	public CourseListViewModel(DatabaseService db, ApiService apiService)
	{
		_db = db;
		_apiService = apiService;
	}

	public async Task LoadCoursesAsync(int termId)
	{
		IsLoading = true;
		try
		{
			Courses.Clear();
			
			// Try to load from API first if authenticated
			if (await _apiService.IsAuthenticatedAsync())
			{
				try
				{
					var response = await _apiService.GetCoursesByTermAsync(termId);
					if (response.Success && response.Data != null)
					{
						foreach (var courseDto in response.Data)
						{
							var course = await ConvertToCourseAsync(courseDto);
							Courses.Add(course);
							// Also save to local database for offline access
							await _db.SaveCourseAsync(course);
						}
						
						var count = Courses.Count;
						CanAddCourse = count < 6;
						CourseCountDisplay = $"{count} of 6 courses";
						return; // Successfully loaded from API
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Failed to load courses from API: {ex.Message}");
					// Fall through to load from local database
				}
			}
			
			// Fallback to local database if API fails or not authenticated
			var localList = await _db.GetCoursesByTermAsync(termId);
			foreach (var localCourse in localList) Courses.Add(localCourse);

			var localCount = await _db.GetCourseCountByTermAsync(termId);
			CanAddCourse = localCount < 6;
			CourseCountDisplay = $"{localCount} of 6 courses";
		}
		finally
		{
			IsLoading = false;
		}
	}

	public async Task AddCourseAsync(Course course)
	{
		// Check course count from current collection or API
		var currentCount = Courses.Count;
		if (currentCount >= 6) throw new InvalidOperationException("Cannot add more than 6 courses to a term.");
		if (!course.IsValid()) throw new InvalidOperationException("Invalid course");

		// Save to API first if authenticated
		if (await _apiService.IsAuthenticatedAsync())
		{
			try
			{
				// Verify the term exists in the API and get the correct API termId
				int apiTermId = await GetApiTermIdAsync(course.TermId);
				
				var courseDto = await ConvertToCourseDTOAsync(course);
				courseDto.TermId = apiTermId; // Use the API termId
				
				System.Diagnostics.Debug.WriteLine($"Saving course to API - TermId: {apiTermId} (mapped from local {course.TermId}), Title: {course.Title}");
				
				var response = await _apiService.CreateCourseAsync(courseDto);
				if (response.Success && response.Data != null)
				{
					// Update course with server data
					var updatedCourse = await ConvertToCourseAsync(response.Data);
					course.Id = updatedCourse.Id;
					course.Title = updatedCourse.Title;
					course.StartDate = updatedCourse.StartDate;
					course.EndDate = updatedCourse.EndDate;
					course.Status = updatedCourse.Status;
					course.InstructorId = updatedCourse.InstructorId;
					course.Notes = updatedCourse.Notes;
					course.NotificationsEnabled = updatedCourse.NotificationsEnabled;
					course.CreditHours = updatedCourse.CreditHours;
					course.CurrentGrade = updatedCourse.CurrentGrade;
					course.LetterGrade = updatedCourse.LetterGrade;
					course.CreatedAt = updatedCourse.CreatedAt;
					// Save to local database for offline access
					await _db.SaveCourseAsync(course);
					await LoadCoursesAsync(course.TermId);
					return; // Successfully saved to API
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"Failed to create course in API:");
					System.Diagnostics.Debug.WriteLine($"  Message: {response.Message}");
					System.Diagnostics.Debug.WriteLine($"  Success: {response.Success}");
					System.Diagnostics.Debug.WriteLine($"  CourseDTO TermId: {courseDto.TermId}");
					if (response.Errors != null && response.Errors.Any())
					{
						System.Diagnostics.Debug.WriteLine($"  Errors: {string.Join(", ", response.Errors)}");
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Exception while saving course to API: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
				// Fall through to save locally
			}
		}
		
		// Fallback to local database if API fails or not authenticated
		await _db.SaveCourseAsync(course);
		Courses.Add(course);
		await LoadCoursesAsync(course.TermId);
	}

	public async Task UpdateCourseAsync(Course course)
	{
		if (!course.IsValid()) throw new InvalidOperationException("Invalid course");
		
		// Update in API first if authenticated
		if (await _apiService.IsAuthenticatedAsync())
		{
			try
			{
				// Verify the term exists in the API and get the correct API termId
				int apiTermId = await GetApiTermIdAsync(course.TermId);
				
				var courseDto = await ConvertToCourseDTOAsync(course);
				courseDto.TermId = apiTermId; // Use the API termId
				
				System.Diagnostics.Debug.WriteLine($"Updating course in API - TermId: {apiTermId} (mapped from local {course.TermId}), CourseId: {course.Id}");
				
				var response = await _apiService.UpdateCourseAsync(course.Id, courseDto);
				if (response.Success && response.Data != null)
				{
					// Update course with server data
					var updatedCourse = await ConvertToCourseAsync(response.Data);
					course.Title = updatedCourse.Title;
					course.StartDate = updatedCourse.StartDate;
					course.EndDate = updatedCourse.EndDate;
					course.Status = updatedCourse.Status;
					course.InstructorId = updatedCourse.InstructorId;
					course.Notes = updatedCourse.Notes;
					course.NotificationsEnabled = updatedCourse.NotificationsEnabled;
					course.CreditHours = updatedCourse.CreditHours;
					course.CurrentGrade = updatedCourse.CurrentGrade;
					course.LetterGrade = updatedCourse.LetterGrade;
					// Save to local database for offline access
					await _db.SaveCourseAsync(course);
					await LoadCoursesAsync(course.TermId);
					return; // Successfully updated in API
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"Failed to update course in API:");
					System.Diagnostics.Debug.WriteLine($"  Message: {response.Message}");
					System.Diagnostics.Debug.WriteLine($"  Success: {response.Success}");
					System.Diagnostics.Debug.WriteLine($"  CourseDTO TermId: {courseDto.TermId}");
					if (response.Errors != null && response.Errors.Any())
					{
						System.Diagnostics.Debug.WriteLine($"  Errors: {string.Join(", ", response.Errors)}");
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Exception while updating course in API: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
				// Fall through to save locally
			}
		}
		
		// Fallback to local database if API fails or not authenticated
		await _db.SaveCourseAsync(course);
		await LoadCoursesAsync(course.TermId);
	}

	public async Task DeleteCourseAsync(Course course)
	{
		// Delete from API first if authenticated
		if (await _apiService.IsAuthenticatedAsync() && course.Id > 0)
		{
			try
			{
				var response = await _apiService.DeleteCourseAsync(course.Id);
				if (response.Success)
				{
					// Also delete from local database
					await _db.DeleteCourseAsync(course.Id);
					Courses.Remove(course);
					var count = Courses.Count;
					CanAddCourse = count < 6;
					CourseCountDisplay = $"{count} of 6 courses";
					return; // Successfully deleted from API
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to delete course from API: {ex.Message}");
				// Fall through to delete locally
			}
		}
		
		// Fallback to local database if API fails or not authenticated
		await _db.DeleteCourseAsync(course.Id);
		Courses.Remove(course);
		var finalCount = Courses.Count;
		CanAddCourse = finalCount < 6;
		CourseCountDisplay = $"{finalCount} of 6 courses";
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

	private async Task<CourseDTO> ConvertToCourseDTOAsync(Course course)
	{
		var instructor = await _db.GetInstructorAsync(course.InstructorId);
		return new CourseDTO
		{
			Id = course.Id,
			TermId = course.TermId,
			Title = course.Title,
			StartDate = course.StartDate,
			EndDate = course.EndDate,
			Status = course.Status,
			InstructorName = instructor?.Name ?? string.Empty,
			InstructorPhone = instructor?.Phone ?? string.Empty,
			InstructorEmail = instructor?.Email ?? string.Empty,
			Notes = course.Notes,
			NotificationsEnabled = course.NotificationsEnabled,
			CreditHours = course.CreditHours,
			CurrentGrade = course.CurrentGrade,
			LetterGrade = course.LetterGrade,
			CreatedAt = course.CreatedAt,
			UpdatedAt = DateTime.UtcNow
		};
	}

	private async Task<Course> ConvertToCourseAsync(CourseDTO courseDto)
	{
		// Find or create instructor in local database
		Instructor? instructor = null;
		if (!string.IsNullOrWhiteSpace(courseDto.InstructorEmail))
		{
			// Try to find existing instructor by email
			var allInstructors = await _db.GetAllInstructorsAsync();
			instructor = allInstructors.FirstOrDefault(i => 
				i.Email.Equals(courseDto.InstructorEmail, StringComparison.OrdinalIgnoreCase));
		}
		
		// Create instructor if not found
		if (instructor == null)
		{
			instructor = new Instructor
			{
				Name = courseDto.InstructorName ?? "Unknown",
				Phone = courseDto.InstructorPhone ?? "000-000-0000",
				Email = courseDto.InstructorEmail ?? "instructor@example.com"
			};
			await _db.SaveInstructorAsync(instructor);
		}

		// Map API termId to local termId if needed
		int localTermId = courseDto.TermId;
		var localTerm = await _db.GetTermAsync(courseDto.TermId);
		if (localTerm == null)
		{
			// API termId doesn't exist locally - try to find matching term
			var allTerms = await _db.GetAllTermsAsync();
			var matchingTerm = allTerms.FirstOrDefault(t => 
				Math.Abs((t.StartDate - courseDto.StartDate).TotalDays) < 30);
			if (matchingTerm != null)
			{
				localTermId = matchingTerm.Id;
			}
		}

		return new Course
		{
			Id = courseDto.Id,
			TermId = localTermId,
			Title = courseDto.Title,
			StartDate = courseDto.StartDate,
			EndDate = courseDto.EndDate,
			Status = courseDto.Status,
			InstructorId = instructor.Id,
			Notes = courseDto.Notes,
			NotificationsEnabled = courseDto.NotificationsEnabled,
			CreditHours = courseDto.CreditHours,
			CurrentGrade = courseDto.CurrentGrade,
			LetterGrade = courseDto.LetterGrade,
			CreatedAt = courseDto.CreatedAt,
			Instructor = instructor
		};
	}

	private async Task<int> GetApiTermIdAsync(int localTermId)
	{
		// First try to get the term directly from API using local ID
		var termResponse = await _apiService.GetTermAsync(localTermId);
		if (termResponse.Success && termResponse.Data != null)
		{
			// Term exists in API with this ID
			return termResponse.Data.Id;
		}

		// Term doesn't exist in API with this ID - try to find it by matching title/date
		var termsResponse = await _apiService.GetTermsAsync();
		if (termsResponse.Success && termsResponse.Data != null)
		{
			// Load local term to match
			var localTerm = await _db.GetTermAsync(localTermId);
			if (localTerm != null)
			{
				var matchingTerm = termsResponse.Data.FirstOrDefault(t => 
					t.Title == localTerm.Title && 
					Math.Abs((t.StartDate - localTerm.StartDate).TotalDays) < 1);
				if (matchingTerm != null)
				{
					System.Diagnostics.Debug.WriteLine($"Mapped local termId {localTermId} to API termId {matchingTerm.Id}");
					return matchingTerm.Id;
				}
			}
		}

		// If we can't find a match, return the original ID (might work if IDs are synced)
		System.Diagnostics.Debug.WriteLine($"Warning: Could not find matching API termId for local termId {localTermId}, using original ID");
		return localTermId;
	}
}







