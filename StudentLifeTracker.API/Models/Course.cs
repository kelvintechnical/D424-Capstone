using System.ComponentModel.DataAnnotations;

namespace StudentLifeTracker.API.Models;

public class Course : BaseEntity
{
    [Required]
    public int TermId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "InProgress"; // InProgress, Completed, Dropped, PlanToTake

    [Required]
    [MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string InstructorPhone { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string InstructorEmail { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool NotificationsEnabled { get; set; } = true;

    // Navigation properties
    public Term Term { get; set; } = null!;
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}

