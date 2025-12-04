using System.ComponentModel.DataAnnotations;

namespace StudentLifeTracker.API.Models;

public class Term : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

