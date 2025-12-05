namespace StudentLifeTracker.Shared.DTOs;

public class GradeProjectionDTO
{
    public int CourseId { get; set; }
    public double CurrentGrade { get; set; }
    public double FinalWeight { get; set; }
    public string TargetGrade { get; set; } = string.Empty;
    public double TargetPercent { get; set; }
    public double NeededOnFinal { get; set; }
    public bool IsAchievable { get; set; }
}

