using SQLite;

namespace StudentProgressTracker.Models;

public class Course
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public int TermId { get; set; }
	[NotNull]
	public string Title { get; set; } = string.Empty;
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	[NotNull]
	public string Status { get; set; } = CourseStatus.InProgress.ToString();
	public int InstructorId { get; set; }
	public string? Notes { get; set; }
	public bool NotificationsEnabled { get; set; } = true;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	[Ignore]
	public Instructor? Instructor { get; set; }

	public bool IsValid()
	{
		if (string.IsNullOrWhiteSpace(Title)) return false;
		if (InstructorId <= 0) return false;
		if (TermId <= 0) return false;
		if (string.IsNullOrWhiteSpace(Status)) return false;
		return EndDate > StartDate;
	}
}

public enum CourseStatus
{
	InProgress,
	Completed,
	Dropped,
	PlanToTake
}







