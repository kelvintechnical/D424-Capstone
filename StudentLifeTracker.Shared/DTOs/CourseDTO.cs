namespace StudentLifeTracker.Shared.DTOs;

public class CourseDTO
{
    public int Id { get; set; }
    public int TermId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string InstructorPhone { get; set; } = string.Empty;
    public string InstructorEmail { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

