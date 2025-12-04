namespace StudentLifeTracker.Shared.DTOs;

public class GradeDTO
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string LetterGrade { get; set; } = string.Empty; // A, B, C, D, F
    public decimal? Percentage { get; set; }
    public int CreditHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

