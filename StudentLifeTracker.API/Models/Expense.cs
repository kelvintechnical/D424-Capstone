using System.ComponentModel.DataAnnotations;

namespace StudentLifeTracker.API.Models;

public class Expense : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

