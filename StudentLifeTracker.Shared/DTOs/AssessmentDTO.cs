namespace StudentLifeTracker.Shared.DTOs;

public class AssessmentDTO
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Objective or Performance
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

