using SQLite;

namespace StudentProgressTracker.Models;

public class Assessment
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public int CourseId { get; set; }
	[NotNull]
	public string Name { get; set; } = string.Empty;
	[NotNull]
	public string Type { get; set; } = AssessmentType.Objective.ToString();
	public DateTime StartDate { get; set; }
	public DateTime DueDate { get; set; }
	public bool NotificationsEnabled { get; set; } = true;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public bool IsValid()
	{
		if (string.IsNullOrWhiteSpace(Name)) return false;
		if (CourseId <= 0) return false;
		if (string.IsNullOrWhiteSpace(Type)) return false;
		var allowed = new[] { AssessmentType.Objective.ToString(), AssessmentType.Performance.ToString() };
		if (!allowed.Contains(Type)) return false;
		return DueDate > StartDate;
	}
}

public enum AssessmentType
{
	Objective,
	Performance
}







