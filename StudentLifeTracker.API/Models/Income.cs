using System.ComponentModel.DataAnnotations;

namespace StudentLifeTracker.API.Models;

public class Income : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(200)]
    public string Source { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}

