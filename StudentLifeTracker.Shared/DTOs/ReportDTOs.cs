namespace StudentLifeTracker.Shared.DTOs;

public class GpaReportDTO
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string TermTitle { get; set; } = string.Empty;
    public DateTime TermStartDate { get; set; }
    public DateTime TermEndDate { get; set; }
    public List<GpaReportCourseDTO> Courses { get; set; } = new();
    public int TotalCreditHours { get; set; }
    public double TermGPA { get; set; }
}

public class GpaReportCourseDTO
{
    public string CourseTitle { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public string LetterGrade { get; set; } = string.Empty;
    public double GradePoints { get; set; }
}

public class TranscriptReportDTO
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public List<TranscriptTermDTO> Terms { get; set; } = new();
    public double CumulativeGPA { get; set; }
    public int TotalCreditHours { get; set; }
}

public class TranscriptTermDTO
{
    public string TermTitle { get; set; } = string.Empty;
    public DateTime TermStartDate { get; set; }
    public DateTime TermEndDate { get; set; }
    public List<GpaReportCourseDTO> Courses { get; set; } = new();
    public double TermGPA { get; set; }
    public int TermCreditHours { get; set; }
}

public class FinancialSummaryDTO
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    public int IncomeCount { get; set; }
    public int ExpenseCount { get; set; }
}







