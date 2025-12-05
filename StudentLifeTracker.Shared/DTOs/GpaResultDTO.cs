namespace StudentLifeTracker.Shared.DTOs;

public class GpaResultDTO
{
    public int TermId { get; set; }
    public double GPA { get; set; }
    public int TotalCreditHours { get; set; }
    public int GradeCount { get; set; }
}

