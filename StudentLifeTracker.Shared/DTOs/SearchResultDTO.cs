namespace StudentLifeTracker.Shared.DTOs;

public class SearchResultDTO
{
    public string ResultType { get; set; } = string.Empty; // "Term" or "Course"
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // Status for courses, date range for terms
    public int? ParentId { get; set; } // termId for courses
    public string? ParentTitle { get; set; } // Term title for courses
}

