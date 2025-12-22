using Microsoft.EntityFrameworkCore;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.Shared.DTOs;
using System.Text;

namespace StudentLifeTracker.API.Services;

public class ReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GpaReportDTO> GenerateGpaReportAsync(string userId, int termId)
    {
        var term = await _context.Terms
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == termId && t.UserId == userId);

        if (term == null)
        {
            throw new ArgumentException("Term not found");
        }

        var grades = await _context.Grades
            .Include(g => g.Course)
            .Where(g => g.Course.TermId == termId)
            .ToListAsync();

        var courses = grades.Select(g => new GpaReportCourseDTO
        {
            CourseTitle = g.Course.Title,
            CreditHours = g.CreditHours,
            LetterGrade = g.LetterGrade,
            GradePoints = ConvertLetterToPoints(g.LetterGrade)
        }).ToList();

        var totalCreditHours = grades.Sum(g => g.CreditHours);
        var termGPA = CalculateGPA(grades);

        return new GpaReportDTO
        {
            ReportTitle = $"GPA Report - {term.Title}",
            GeneratedAt = DateTime.UtcNow,
            StudentName = term.User.Name,
            StudentEmail = term.User.Email ?? string.Empty,
            TermTitle = term.Title,
            TermStartDate = term.StartDate,
            TermEndDate = term.EndDate,
            Courses = courses,
            TotalCreditHours = totalCreditHours,
            TermGPA = termGPA
        };
    }

    public async Task<TranscriptReportDTO> GenerateTranscriptReportAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        var terms = await _context.Terms
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.StartDate)
            .ToListAsync();

        var transcriptTerms = new List<TranscriptTermDTO>();
        double totalPoints = 0;
        int totalCreditHours = 0;

        foreach (var term in terms)
        {
            var grades = await _context.Grades
                .Include(g => g.Course)
                .Where(g => g.Course.TermId == term.Id)
                .ToListAsync();

            var courses = grades.Select(g => new GpaReportCourseDTO
            {
                CourseTitle = g.Course.Title,
                CreditHours = g.CreditHours,
                LetterGrade = g.LetterGrade,
                GradePoints = ConvertLetterToPoints(g.LetterGrade)
            }).ToList();

            var termCreditHours = grades.Sum(g => g.CreditHours);
            var termGPA = CalculateGPA(grades);

            totalCreditHours += termCreditHours;
            foreach (var grade in grades)
            {
                totalPoints += ConvertLetterToPoints(grade.LetterGrade) * grade.CreditHours;
            }

            transcriptTerms.Add(new TranscriptTermDTO
            {
                TermTitle = term.Title,
                TermStartDate = term.StartDate,
                TermEndDate = term.EndDate,
                Courses = courses,
                TermGPA = termGPA,
                TermCreditHours = termCreditHours
            });
        }

        var cumulativeGPA = totalCreditHours > 0 ? totalPoints / totalCreditHours : 0.0;

        return new TranscriptReportDTO
        {
            ReportTitle = "Academic Transcript",
            GeneratedAt = DateTime.UtcNow,
            StudentName = user.Name,
            StudentEmail = user.Email ?? string.Empty,
            Terms = transcriptTerms,
            CumulativeGPA = cumulativeGPA,
            TotalCreditHours = totalCreditHours
        };
    }

    public string GenerateGpaCsv(GpaReportDTO report)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine(report.ReportTitle);
        csv.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        csv.AppendLine($"Student: {report.StudentName} ({report.StudentEmail})");
        csv.AppendLine($"Term: {report.TermTitle}");
        csv.AppendLine($"Term Dates: {report.TermStartDate:yyyy-MM-dd} to {report.TermEndDate:yyyy-MM-dd}");
        csv.AppendLine(); // Empty line

        // Column headers
        csv.AppendLine("Course Title,Credit Hours,Letter Grade,Grade Points");

        // Course rows
        foreach (var course in report.Courses)
        {
            csv.AppendLine($"{EscapeCsvField(course.CourseTitle)},{course.CreditHours},{course.LetterGrade},{course.GradePoints:F2}");
        }

        // Summary row
        csv.AppendLine(); // Empty line
        csv.AppendLine($"Total Credit Hours,{report.TotalCreditHours},,Term GPA,{report.TermGPA:F2}");

        return csv.ToString();
    }

    public string GenerateTranscriptCsv(TranscriptReportDTO report)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine(report.ReportTitle);
        csv.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        csv.AppendLine($"Student: {report.StudentName} ({report.StudentEmail})");
        csv.AppendLine(); // Empty line

        // Process each term
        foreach (var term in report.Terms)
        {
            csv.AppendLine($"Term: {term.TermTitle}");
            csv.AppendLine($"Term Dates: {term.TermStartDate:yyyy-MM-dd} to {term.TermEndDate:yyyy-MM-dd}");
            csv.AppendLine(); // Empty line

            // Column headers
            csv.AppendLine("Course Title,Credit Hours,Letter Grade,Grade Points");

            // Course rows
            foreach (var course in term.Courses)
            {
                csv.AppendLine($"{EscapeCsvField(course.CourseTitle)},{course.CreditHours},{course.LetterGrade},{course.GradePoints:F2}");
            }

            // Term summary
            csv.AppendLine(); // Empty line
            csv.AppendLine($"Term Credit Hours,{term.TermCreditHours},,Term GPA,{term.TermGPA:F2}");
            csv.AppendLine(); // Empty line between terms
        }

        // Overall summary
        csv.AppendLine("CUMULATIVE SUMMARY");
        csv.AppendLine($"Total Credit Hours,{report.TotalCreditHours},,Cumulative GPA,{report.CumulativeGPA:F2}");

        return csv.ToString();
    }

    private double CalculateGPA(List<Grade> grades)
    {
        if (grades == null || grades.Count == 0)
            return 0.0;

        double totalPoints = 0;
        int totalCreditHours = 0;

        foreach (var grade in grades)
        {
            var points = ConvertLetterToPoints(grade.LetterGrade);
            totalPoints += points * grade.CreditHours;
            totalCreditHours += grade.CreditHours;
        }

        if (totalCreditHours == 0)
            return 0.0;

        return totalPoints / totalCreditHours;
    }

    private double ConvertLetterToPoints(string letterGrade)
    {
        return letterGrade.ToUpper() switch
        {
            "A+" or "A" => 4.0,
            "A-" => 3.7,
            "B+" => 3.3,
            "B" => 3.0,
            "B-" => 2.7,
            "C+" => 2.3,
            "C" => 2.0,
            "C-" => 1.7,
            "D+" => 1.3,
            "D" => 1.0,
            "D-" => 0.7,
            "F" => 0.0,
            _ => 0.0
        };
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
















