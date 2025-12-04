using System.ComponentModel.DataAnnotations;

namespace StudentLifeTracker.API.Models;

public class Grade : BaseEntity
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(2)]
    public string LetterGrade { get; set; } = string.Empty; // A, B, C, D, F

    public decimal? Percentage { get; set; }

    [Required]
    [Range(1, 10)]
    public int CreditHours { get; set; }

    // Navigation property
    public Course Course { get; set; } = null!;
}

