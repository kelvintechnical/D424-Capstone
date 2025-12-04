using System.ComponentModel.DataAnnotations;

namespace StudentLifeTracker.API.Models;

public class Assessment : BaseEntity
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "Objective"; // Objective or Performance

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public bool NotificationsEnabled { get; set; } = true;

    // Navigation property
    public Course Course { get; set; } = null!;
}

