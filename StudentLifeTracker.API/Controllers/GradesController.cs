using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.Shared.DTOs;
using System.Security.Claims;

namespace StudentLifeTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GradesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GradesController> _logger;

    public GradesController(ApplicationDbContext context, ILogger<GradesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<GradeDTO>>> AddOrUpdateGrade([FromBody] GradeDTO gradeDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<GradeDTO>.ErrorResponse("User not authenticated."));
            }

            // Validate input
            if (gradeDto == null)
            {
                return BadRequest(ApiResponse<GradeDTO>.ErrorResponse("Grade data is required."));
            }

            if (gradeDto.CourseId <= 0)
            {
                return BadRequest(ApiResponse<GradeDTO>.ErrorResponse("Invalid course ID."));
            }

            if (string.IsNullOrWhiteSpace(gradeDto.LetterGrade))
            {
                return BadRequest(ApiResponse<GradeDTO>.ErrorResponse("Letter grade is required."));
            }

            if (gradeDto.CreditHours <= 0 || gradeDto.CreditHours > 10)
            {
                return BadRequest(ApiResponse<GradeDTO>.ErrorResponse("Credit hours must be between 1 and 10."));
            }

            // Verify course belongs to user - improved query to avoid navigation property issues
            var course = await _context.Courses
                .Include(c => c.Term)
                .Where(c => c.Id == gradeDto.CourseId)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound(ApiResponse<GradeDTO>.ErrorResponse("Course not found."));
            }

            if (course.Term == null || course.Term.UserId != userId)
            {
                return StatusCode(403, ApiResponse<GradeDTO>.ErrorResponse("You do not have permission to modify this course."));
            }

            Grade grade;
            if (gradeDto.Id > 0)
            {
                // Update existing grade - verify it belongs to the course
                grade = await _context.Grades
                    .FirstOrDefaultAsync(g => g.Id == gradeDto.Id && g.CourseId == gradeDto.CourseId);

                if (grade == null)
                {
                    return NotFound(ApiResponse<GradeDTO>.ErrorResponse("Grade not found or does not belong to this course."));
                }

                grade.LetterGrade = gradeDto.LetterGrade;
                grade.Percentage = gradeDto.Percentage;
                grade.CreditHours = gradeDto.CreditHours;
                grade.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new grade
                grade = new Grade
                {
                    CourseId = gradeDto.CourseId,
                    LetterGrade = gradeDto.LetterGrade,
                    Percentage = gradeDto.Percentage,
                    CreditHours = gradeDto.CreditHours,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Grades.Add(grade);
            }

            await _context.SaveChangesAsync();

            var result = new GradeDTO
            {
                Id = grade.Id,
                CourseId = grade.CourseId,
                LetterGrade = grade.LetterGrade,
                Percentage = grade.Percentage,
                CreditHours = grade.CreditHours,
                CreatedAt = grade.CreatedAt,
                UpdatedAt = grade.UpdatedAt
            };

            return Ok(ApiResponse<GradeDTO>.SuccessResponse(result, "Grade saved successfully"));
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error saving grade");
            var errorMessage = "An error occurred while saving the grade to the database.";
            
            // Check for specific database errors
            if (dbEx.InnerException != null)
            {
                var innerMessage = dbEx.InnerException.Message.ToLower();
                if (innerMessage.Contains("foreign key") || innerMessage.Contains("constraint"))
                {
                    errorMessage = "The course associated with this grade could not be found.";
                }
                else if (innerMessage.Contains("duplicate") || innerMessage.Contains("unique"))
                {
                    errorMessage = "A grade with these details already exists.";
                }
            }
            
            return StatusCode(500, ApiResponse<GradeDTO>.ErrorResponse(errorMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving grade: {Message}", ex.Message);
            return StatusCode(500, ApiResponse<GradeDTO>.ErrorResponse($"An error occurred while saving the grade: {ex.Message}"));
        }
    }

    [HttpGet("term/{termId}")]
    public async Task<ActionResult<ApiResponse<List<GradeDTO>>>> GetTermGrades(int termId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<GradeDTO>>.ErrorResponse("User not authenticated."));
            }

            // Verify term belongs to user
            var term = await _context.Terms.FindAsync(termId);
            if (term == null || term.UserId != userId)
            {
                return NotFound(ApiResponse<List<GradeDTO>>.ErrorResponse("Term not found."));
            }

            var grades = await _context.Grades
                .Include(g => g.Course)
                .Where(g => g.Course.TermId == termId)
                .Select(g => new GradeDTO
                {
                    Id = g.Id,
                    CourseId = g.CourseId,
                    LetterGrade = g.LetterGrade,
                    Percentage = g.Percentage,
                    CreditHours = g.CreditHours,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<GradeDTO>>.SuccessResponse(grades));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting term grades");
            return StatusCode(500, ApiResponse<List<GradeDTO>>.ErrorResponse("An error occurred while retrieving grades."));
        }
    }

    [HttpGet("gpa/{termId}")]
    public async Task<ActionResult<ApiResponse<GpaResultDTO>>> GetTermGPA(int termId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<GpaResultDTO>.ErrorResponse("User not authenticated."));
            }

            // Verify term belongs to user
            var term = await _context.Terms.FindAsync(termId);
            if (term == null || term.UserId != userId)
            {
                return NotFound(ApiResponse<GpaResultDTO>.ErrorResponse("Term not found."));
            }

            var grades = await _context.Grades
                .Include(g => g.Course)
                .Where(g => g.Course.TermId == termId)
                .ToListAsync();

            var gpa = CalculateGPA(grades);

            var result = new GpaResultDTO
            {
                TermId = termId,
                GPA = gpa,
                TotalCreditHours = grades.Sum(g => g.CreditHours),
                GradeCount = grades.Count
            };

            return Ok(ApiResponse<GpaResultDTO>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating term GPA");
            return StatusCode(500, ApiResponse<GpaResultDTO>.ErrorResponse("An error occurred while calculating GPA."));
        }
    }

    [HttpGet("projection/{courseId}")]
    public async Task<ActionResult<ApiResponse<GradeProjectionDTO>>> GetGradeProjection(
        int courseId,
        [FromQuery] double currentGrade,
        [FromQuery] double finalWeight,
        [FromQuery] string targetGrade)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<GradeProjectionDTO>.ErrorResponse("User not authenticated."));
            }

            // Verify course belongs to user
            var course = await _context.Courses
                .Include(c => c.Term)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.Term.UserId == userId);

            if (course == null)
            {
                return NotFound(ApiResponse<GradeProjectionDTO>.ErrorResponse("Course not found."));
            }

            if (finalWeight <= 0 || finalWeight > 1)
            {
                return BadRequest(ApiResponse<GradeProjectionDTO>.ErrorResponse("Final weight must be between 0 and 1."));
            }

            // Convert target letter grade to percentage
            var targetPercent = ConvertLetterToPercent(targetGrade);
            if (targetPercent < 0)
            {
                return BadRequest(ApiResponse<GradeProjectionDTO>.ErrorResponse("Invalid target grade."));
            }

            // Calculate needed grade on final
            // Formula: needed = (target - (current × (1 - finalWeight))) / finalWeight
            var needed = (targetPercent - (currentGrade * (1 - finalWeight))) / finalWeight;

            var result = new GradeProjectionDTO
            {
                CourseId = courseId,
                CurrentGrade = currentGrade,
                FinalWeight = finalWeight,
                TargetGrade = targetGrade,
                TargetPercent = targetPercent,
                NeededOnFinal = needed,
                IsAchievable = needed >= 0 && needed <= 100
            };

            return Ok(ApiResponse<GradeProjectionDTO>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating grade projection");
            return StatusCode(500, ApiResponse<GradeProjectionDTO>.ErrorResponse("An error occurred while calculating projection."));
        }
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

    private double ConvertLetterToPercent(string letterGrade)
    {
        return letterGrade.ToUpper() switch
        {
            "A+" or "A" => 93.0,
            "A-" => 90.0,
            "B+" => 87.0,
            "B" => 83.0,
            "B-" => 80.0,
            "C+" => 77.0,
            "C" => 73.0,
            "C-" => 70.0,
            "D+" => 67.0,
            "D" => 63.0,
            "D-" => 60.0,
            "F" => 0.0,
            _ => -1.0
        };
    }
}

