using SQLite;

namespace StudentProgressTracker.Models;

public class AcademicTerm
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }

	[NotNull]
	public string Title { get; set; } = string.Empty;

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public bool IsValid()
	{
		if (string.IsNullOrWhiteSpace(Title)) return false;
		return EndDate > StartDate;
	}
}







