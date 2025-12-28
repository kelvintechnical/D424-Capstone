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
public class AssessmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssessmentsController> _logger;

    public AssessmentsController(ApplicationDbContext context, ILogger<AssessmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<ApiResponse<List<AssessmentDTO>>>> GetAssessmentsByCourse(int courseId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<AssessmentDTO>>.ErrorResponse("User not authenticated."));
            }

            // Verify course belongs to user
            var course = await _context.Courses
                .Include(c => c.Term)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.Term.UserId == userId);
            
            if (course == null)
            {
                return NotFound(ApiResponse<List<AssessmentDTO>>.ErrorResponse("Course not found."));
            }

            var assessments = await _context.Assessments
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.StartDate)
                .Select(a => new AssessmentDTO
                {
                    Id = a.Id,
                    CourseId = a.CourseId,
                    Name = a.Name,
                    Type = a.Type,
                    StartDate = a.StartDate,
                    DueDate = a.DueDate,
                    NotificationsEnabled = a.NotificationsEnabled,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<AssessmentDTO>>.SuccessResponse(assessments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assessments for course {CourseId}", courseId);
            return StatusCode(500, ApiResponse<List<AssessmentDTO>>.ErrorResponse("An error occurred while retrieving assessments."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AssessmentDTO>>> GetAssessment(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<AssessmentDTO>.ErrorResponse("User not authenticated."));
            }

            var assessment = await _context.Assessments
                .Include(a => a.Course)
                    .ThenInclude(c => c.Term)
                .FirstOrDefaultAsync(a => a.Id == id && a.Course.Term.UserId == userId);

            if (assessment == null)
            {
                return NotFound(ApiResponse<AssessmentDTO>.ErrorResponse("Assessment not found."));
            }

            var result = new AssessmentDTO
            {
                Id = assessment.Id,
                CourseId = assessment.CourseId,
                Name = assessment.Name,
                Type = assessment.Type,
                StartDate = assessment.StartDate,
                DueDate = assessment.DueDate,
                NotificationsEnabled = assessment.NotificationsEnabled,
                CreatedAt = assessment.CreatedAt,
                UpdatedAt = assessment.UpdatedAt
            };

            return Ok(ApiResponse<AssessmentDTO>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assessment {AssessmentId}", id);
            return StatusCode(500, ApiResponse<AssessmentDTO>.ErrorResponse("An error occurred while retrieving the assessment."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AssessmentDTO>>> CreateAssessment([FromBody] AssessmentDTO assessmentDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<AssessmentDTO>.ErrorResponse("User not authenticated."));
            }

            if (assessmentDto == null)
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Assessment data is required."));
            }

            if (string.IsNullOrWhiteSpace(assessmentDto.Name))
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Assessment name is required."));
            }

            if (assessmentDto.CourseId <= 0)
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Invalid course ID."));
            }

            // Verify course belongs to user
            var course = await _context.Courses
                .Include(c => c.Term)
                .FirstOrDefaultAsync(c => c.Id == assessmentDto.CourseId && c.Term.UserId == userId);
            
            if (course == null)
            {
                return NotFound(ApiResponse<AssessmentDTO>.ErrorResponse("Course not found."));
            }

            // Validate assessment type
            if (string.IsNullOrWhiteSpace(assessmentDto.Type) || 
                (assessmentDto.Type != "Objective" && assessmentDto.Type != "Performance"))
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Assessment type must be 'Objective' or 'Performance'."));
            }

            // Validate dates
            if (assessmentDto.DueDate <= assessmentDto.StartDate)
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Due date must be after start date."));
            }

            // Check assessment limit (2 assessments per course)
            var assessmentCount = await _context.Assessments.CountAsync(a => a.CourseId == assessmentDto.CourseId);
            if (assessmentCount >= 2)
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Cannot add more than 2 assessments to a course."));
            }

            // Check if assessment type already exists for this course
            var existingAssessment = await _context.Assessments
                .FirstOrDefaultAsync(a => a.CourseId == assessmentDto.CourseId && a.Type == assessmentDto.Type);
            
            if (existingAssessment != null)
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse($"A {assessmentDto.Type} assessment already exists for this course."));
            }

            var assessment = new Assessment
            {
                CourseId = assessmentDto.CourseId,
                Name = assessmentDto.Name.Trim(),
                Type = assessmentDto.Type,
                StartDate = assessmentDto.StartDate.ToUniversalTime(),
                DueDate = assessmentDto.DueDate.ToUniversalTime(),
                NotificationsEnabled = assessmentDto.NotificationsEnabled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            var result = new AssessmentDTO
            {
                Id = assessment.Id,
                CourseId = assessment.CourseId,
                Name = assessment.Name,
                Type = assessment.Type,
                StartDate = assessment.StartDate,
                DueDate = assessment.DueDate,
                NotificationsEnabled = assessment.NotificationsEnabled,
                CreatedAt = assessment.CreatedAt,
                UpdatedAt = assessment.UpdatedAt
            };

            return Ok(ApiResponse<AssessmentDTO>.SuccessResponse(result, "Assessment created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assessment");
            return StatusCode(500, ApiResponse<AssessmentDTO>.ErrorResponse($"An error occurred while creating the assessment: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AssessmentDTO>>> UpdateAssessment(int id, [FromBody] AssessmentDTO assessmentDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<AssessmentDTO>.ErrorResponse("User not authenticated."));
            }

            if (assessmentDto == null)
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Assessment data is required."));
            }

            var assessment = await _context.Assessments
                .Include(a => a.Course)
                    .ThenInclude(c => c.Term)
                .FirstOrDefaultAsync(a => a.Id == id && a.Course.Term.UserId == userId);

            if (assessment == null)
            {
                return NotFound(ApiResponse<AssessmentDTO>.ErrorResponse("Assessment not found."));
            }

            if (string.IsNullOrWhiteSpace(assessmentDto.Name))
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Assessment name is required."));
            }

            // Validate assessment type
            if (string.IsNullOrWhiteSpace(assessmentDto.Type) || 
                (assessmentDto.Type != "Objective" && assessmentDto.Type != "Performance"))
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Assessment type must be 'Objective' or 'Performance'."));
            }

            // Validate dates
            if (assessmentDto.DueDate <= assessmentDto.StartDate)
            {
                return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse("Due date must be after start date."));
            }

            // Check if changing type would create a duplicate
            if (assessment.Type != assessmentDto.Type)
            {
                var existingAssessment = await _context.Assessments
                    .FirstOrDefaultAsync(a => a.CourseId == assessment.CourseId && 
                                             a.Type == assessmentDto.Type && 
                                             a.Id != id);
                
                if (existingAssessment != null)
                {
                    return BadRequest(ApiResponse<AssessmentDTO>.ErrorResponse($"A {assessmentDto.Type} assessment already exists for this course."));
                }
            }

            assessment.Name = assessmentDto.Name.Trim();
            assessment.Type = assessmentDto.Type;
            assessment.StartDate = assessmentDto.StartDate.ToUniversalTime();
            assessment.DueDate = assessmentDto.DueDate.ToUniversalTime();
            assessment.NotificationsEnabled = assessmentDto.NotificationsEnabled;
            assessment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new AssessmentDTO
            {
                Id = assessment.Id,
                CourseId = assessment.CourseId,
                Name = assessment.Name,
                Type = assessment.Type,
                StartDate = assessment.StartDate,
                DueDate = assessment.DueDate,
                NotificationsEnabled = assessment.NotificationsEnabled,
                CreatedAt = assessment.CreatedAt,
                UpdatedAt = assessment.UpdatedAt
            };

            return Ok(ApiResponse<AssessmentDTO>.SuccessResponse(result, "Assessment updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assessment {AssessmentId}", id);
            return StatusCode(500, ApiResponse<AssessmentDTO>.ErrorResponse($"An error occurred while updating the assessment: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAssessment(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated."));
            }

            var assessment = await _context.Assessments
                .Include(a => a.Course)
                    .ThenInclude(c => c.Term)
                .FirstOrDefaultAsync(a => a.Id == id && a.Course.Term.UserId == userId);

            if (assessment == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Assessment not found."));
            }

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Assessment deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assessment {AssessmentId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred while deleting the assessment: {ex.Message}"));
        }
    }
}












