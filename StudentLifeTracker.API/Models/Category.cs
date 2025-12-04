using System.ComponentModel.DataAnnotations;

namespace StudentLifeTracker.API.Models;

public class Category : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsCustom { get; set; } = true;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

