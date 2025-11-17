using SQLite;
using StudentProgressTracker.Models;
using System.Diagnostics;

namespace StudentProgressTracker.Services;

public class DatabaseService
{
	private readonly string _databasePath;
	private SQLiteAsyncConnection? _connection;

	public static DatabaseService? Current { get; set; }

	public DatabaseService(string databasePath)
	{
		_databasePath = databasePath;
	}

	public async Task InitializeAsync()
	{
		try
		{
			if (_connection is not null) return;
			_connection = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

			await _connection.CreateTableAsync<Instructor>();
			await _connection.CreateTableAsync<AcademicTerm>();
			await _connection.CreateTableAsync<Course>();
			await _connection.CreateTableAsync<Assessment>();

			var terms = await _connection.Table<AcademicTerm>().ToListAsync();
			if (terms.Count == 0)
			{
				await SeedDataAsync();
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[DB][Initialize] {ex}");
			throw;
		}
	}

	private async Task<SQLiteAsyncConnection> GetConnectionAsync()
	{
		if (_connection is null)
		{
			// Auto-initialize if not already done
			await InitializeAsync();
		}
		return _connection!;
	}

	private SQLiteAsyncConnection GetConnection()
	{
		if (_connection is null) throw new InvalidOperationException("Database not initialized. Call InitializeAsync() first.");
		return _connection;
	}

	public async Task<Instructor?> GetInstructorAsync(int id)
	{
		try { return await GetConnection().FindAsync<Instructor>(id); }
		catch (Exception ex) { Debug.WriteLine($"[DB][GetInstructor] {ex}"); throw; }
	}

	public async Task<List<Instructor>> GetAllInstructorsAsync()
	{
		try { return await GetConnection().Table<Instructor>().ToListAsync(); }
		catch (Exception ex) { Debug.WriteLine($"[DB][GetAllInstructors] {ex}"); throw; }
	}

	public async Task<int> SaveInstructorAsync(Instructor instructor)
	{
		try { return instructor.Id == 0 ? await GetConnection().InsertAsync(instructor) : await GetConnection().UpdateAsync(instructor); }
		catch (Exception ex) { Debug.WriteLine($"[DB][SaveInstructor] {ex}"); throw; }
	}

	public async Task<List<AcademicTerm>> GetAllTermsAsync()
	{
		try 
		{ 
			var connection = await GetConnectionAsync();
			return await connection.Table<AcademicTerm>().OrderBy(t => t.StartDate).ToListAsync(); 
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][GetAllTerms] {ex}"); throw; }
	}

	public async Task<AcademicTerm?> GetTermAsync(int id)
	{
		try { return await GetConnection().FindAsync<AcademicTerm>(id); }
		catch (Exception ex) { Debug.WriteLine($"[DB][GetTerm] {ex}"); throw; }
	}

	public async Task<int> SaveTermAsync(AcademicTerm term)
	{
		try
		{
			term.StartDate = term.StartDate.ToUniversalTime();
			term.EndDate = term.EndDate.ToUniversalTime();
			return term.Id == 0 ? await GetConnection().InsertAsync(term) : await GetConnection().UpdateAsync(term);
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][SaveTerm] {ex}"); throw; }
	}

	public async Task DeleteTermAsync(int id)
	{
		try
		{
			var courses = await GetConnection().Table<Course>().Where(c => c.TermId == id).ToListAsync();
			foreach (var c in courses) { await DeleteCourseAsync(c.Id); }
			await GetConnection().DeleteAsync<AcademicTerm>(id);
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][DeleteTerm] {ex}"); throw; }
	}

	public async Task<List<Course>> GetCoursesByTermAsync(int termId)
	{
		try
		{
			var list = await GetConnection().Table<Course>().Where(c => c.TermId == termId).OrderBy(c => c.StartDate).ToListAsync();
			foreach (var c in list) { c.Instructor = await GetInstructorAsync(c.InstructorId); }
			return list;
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][GetCoursesByTerm] {ex}"); throw; }
	}

	public async Task<Course?> GetCourseAsync(int id)
	{
		try
		{
			var course = await GetConnection().FindAsync<Course>(id);
			if (course is not null) { course.Instructor = await GetInstructorAsync(course.InstructorId); }
			return course;
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][GetCourse] {ex}"); throw; }
	}

	public async Task<int> SaveCourseAsync(Course course)
	{
		try
		{
			course.StartDate = course.StartDate.ToUniversalTime();
			course.EndDate = course.EndDate.ToUniversalTime();
			return course.Id == 0 ? await GetConnection().InsertAsync(course) : await GetConnection().UpdateAsync(course);
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][SaveCourse] {ex}"); throw; }
	}

	public async Task DeleteCourseAsync(int id)
	{
		try
		{
			var assessments = await GetConnection().Table<Assessment>().Where(a => a.CourseId == id).ToListAsync();
			foreach (var a in assessments) { await GetConnection().DeleteAsync<Assessment>(a.Id); }
			await GetConnection().DeleteAsync<Course>(id);
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][DeleteCourse] {ex}"); throw; }
	}

	public async Task<int> GetCourseCountByTermAsync(int termId)
	{
		try { return await GetConnection().Table<Course>().Where(c => c.TermId == termId).CountAsync(); }
		catch (Exception ex) { Debug.WriteLine($"[DB][GetCourseCountByTerm] {ex}"); throw; }
	}

	public async Task<List<Assessment>> GetAssessmentsByCourseAsync(int courseId)
	{
		try { return await GetConnection().Table<Assessment>().Where(a => a.CourseId == courseId).OrderBy(a => a.DueDate).ToListAsync(); }
		catch (Exception ex) { Debug.WriteLine($"[DB][GetAssessmentsByCourse] {ex}"); throw; }
	}

	public async Task<Assessment?> GetAssessmentAsync(int id)
	{
		try { return await GetConnection().FindAsync<Assessment>(id); }
		catch (Exception ex) { Debug.WriteLine($"[DB][GetAssessment] {ex}"); throw; }
	}

	public async Task<int> SaveAssessmentAsync(Assessment assessment)
	{
		try
		{
			assessment.StartDate = assessment.StartDate.ToUniversalTime();
			assessment.DueDate = assessment.DueDate.ToUniversalTime();
			return assessment.Id == 0 ? await GetConnection().InsertAsync(assessment) : await GetConnection().UpdateAsync(assessment);
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][SaveAssessment] {ex}"); throw; }
	}

	public async Task DeleteAssessmentAsync(int id)
	{
		try { await GetConnection().DeleteAsync<Assessment>(id); }
		catch (Exception ex) { Debug.WriteLine($"[DB][DeleteAssessment] {ex}"); throw; }
	}

	public async Task<int> GetAssessmentCountByCourseAsync(int courseId)
	{
		try { return await GetConnection().Table<Assessment>().Where(a => a.CourseId == courseId).CountAsync(); }
		catch (Exception ex) { Debug.WriteLine($"[DB][GetAssessmentCountByCourse] {ex}"); throw; }
	}

	private async Task SeedDataAsync()
	{
		try
		{
			var term = new AcademicTerm
			{
				Title = "Spring 2025",
				StartDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
				EndDate = new DateTime(2025, 5, 15, 0, 0, 0, DateTimeKind.Utc),
				CreatedAt = DateTime.UtcNow
			};
			await GetConnection().InsertAsync(term);

			var instructor = new Instructor
			{
				Name = "Anika Patel",
				Phone = "555-123-4567",
				Email = "anika.patel@strimeuniversity.edu"
			};
			await GetConnection().InsertAsync(instructor);

			var course = new Course
			{
				TermId = term.Id,
				Title = "Mobile App Development",
				StartDate = new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc),
				EndDate = new DateTime(2025, 5, 10, 0, 0, 0, DateTimeKind.Utc),
				Status = CourseStatus.InProgress.ToString(),
				InstructorId = instructor.Id,
				Notes = "Sample course for testing",
				NotificationsEnabled = true,
				CreatedAt = DateTime.UtcNow
			};
			await GetConnection().InsertAsync(course);

			var a1 = new Assessment
			{
				CourseId = course.Id,
				Name = "Mobile App Development - Objective Assessment",
				Type = AssessmentType.Objective.ToString(),
				StartDate = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
				DueDate = new DateTime(2025, 4, 15, 0, 0, 0, DateTimeKind.Utc),
				NotificationsEnabled = true,
				CreatedAt = DateTime.UtcNow
			};
			var a2 = new Assessment
			{
				CourseId = course.Id,
				Name = "Mobile App Development - Performance Assessment",
				Type = AssessmentType.Performance.ToString(),
				StartDate = new DateTime(2025, 4, 16, 0, 0, 0, DateTimeKind.Utc),
				DueDate = new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc),
				NotificationsEnabled = true,
				CreatedAt = DateTime.UtcNow
			};
			await GetConnection().InsertAllAsync(new[] { a1, a2 });
		}
		catch (Exception ex) { Debug.WriteLine($"[DB][Seed] {ex}"); throw; }
	}
}

