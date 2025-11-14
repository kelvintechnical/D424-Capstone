using SQLite;
using System.Text.RegularExpressions;

namespace StudentProgressTracker.Models;

public class Instructor
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	[NotNull]
	public string Name { get; set; } = string.Empty;
	[NotNull]
	public string Phone { get; set; } = string.Empty;
	[NotNull]
	public string Email { get; set; } = string.Empty;

	public bool IsValid()
	{
		if (string.IsNullOrWhiteSpace(Name)) return false;
		if (string.IsNullOrWhiteSpace(Phone)) return false;
		if (string.IsNullOrWhiteSpace(Email)) return false;
		var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
		return emailRegex.IsMatch(Email);
	}
}







