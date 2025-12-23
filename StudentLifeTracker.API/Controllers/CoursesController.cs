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
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ApplicationDbContext context, ILogger<CoursesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet("term/{termId}")]
    public async Task<ActionResult<ApiResponse<List<CourseDTO>>>> GetCoursesByTerm(int termId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<CourseDTO>>.ErrorResponse("User not authenticated."));
            }

            // Verify term belongs to user
            var term = await _context.Terms.FindAsync(termId);
            if (term == null || term.UserId != userId)
            {
                return NotFound(ApiResponse<List<CourseDTO>>.ErrorResponse("Term not found."));
            }

            var courses = await _context.Courses
                .Where(c => c.TermId == termId)
                .OrderBy(c => c.StartDate)
                .Select(c => new CourseDTO
                {
                    Id = c.Id,
                    TermId = c.TermId,
                    Title = c.Title,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    InstructorName = c.InstructorName,
                    InstructorPhone = c.InstructorPhone,
                    InstructorEmail = c.InstructorEmail,
                    Notes = c.Notes,
                    NotificationsEnabled = c.NotificationsEnabled,
                    CreditHours = c.CreditHours,
                    CurrentGrade = c.CurrentGrade,
                    LetterGrade = c.LetterGrade,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<CourseDTO>>.SuccessResponse(courses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses for term {TermId}", termId);
            return StatusCode(500, ApiResponse<List<CourseDTO>>.ErrorResponse("An error occurred while retrieving courses."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CourseDTO>>> GetCourse(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<CourseDTO>.ErrorResponse("User not authenticated."));
            }

            var course = await _context.Courses
                .Include(c => c.Term)
                .FirstOrDefaultAsync(c => c.Id == id && c.Term.UserId == userId);

            if (course == null)
            {
                return NotFound(ApiResponse<CourseDTO>.ErrorResponse("Course not found."));
            }

            var result = new CourseDTO
            {
                Id = course.Id,
                TermId = course.TermId,
                Title = course.Title,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Status = course.Status,
                InstructorName = course.InstructorName,
                InstructorPhone = course.InstructorPhone,
                InstructorEmail = course.InstructorEmail,
                Notes = course.Notes,
                NotificationsEnabled = course.NotificationsEnabled,
                CreditHours = course.CreditHours,
                CurrentGrade = course.CurrentGrade,
                LetterGrade = course.LetterGrade,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt
            };

            return Ok(ApiResponse<CourseDTO>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course {CourseId}", id);
            return StatusCode(500, ApiResponse<CourseDTO>.ErrorResponse("An error occurred while retrieving the course."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourseDTO>>> CreateCourse([FromBody] CourseDTO courseDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<CourseDTO>.ErrorResponse("User not authenticated."));
            }

            if (courseDto == null)
            {
                return BadRequest(ApiResponse<CourseDTO>.ErrorResponse("Course data is required."));
            }

            if (string.IsNullOrWhiteSpace(courseDto.Title))
            {
                return BadRequest(ApiResponse<CourseDTO>.ErrorResponse("Course title is required."));
            }

            if (courseDto.TermId <= 0)
            {
                return BadRequest(ApiResponse<CourseDTO>.ErrorResponse("Invalid term ID."));
            }

            // Verify term belongs to user
            var term = await _context.Terms.FindAsync(courseDto.TermId);
            if (term == null || term.UserId != userId)
            {
                return NotFound(ApiResponse<CourseDTO>.ErrorResponse("Term not found."));
            }

            // Check course limit (6 courses per term)
            var courseCount = await _context.Courses.CountAsync(c => c.TermId == courseDto.TermId);
            if (courseCount >= 6)
            {
                return BadRequest(ApiResponse<CourseDTO>.ErrorResponse("Cannot add more than 6 courses to a term."));
            }

            if (courseDto.EndDate <= courseDto.StartDate)
            {
                return BadRequest(ApiResponse<CourseDTO>.ErrorResponse("End date must be after start date."));
            }

            var course = new Course
            {
                TermId = courseDto.TermId,
                Title = courseDto.Title.Trim(),
                StartDate = courseDto.StartDate.ToUniversalTime(),
                EndDate = courseDto.EndDate.ToUniversalTime(),
                Status = courseDto.Status ?? "InProgress",
                InstructorName = courseDto.InstructorName?.Trim() ?? string.Empty,
                InstructorPhone = courseDto.InstructorPhone?.Trim() ?? string.Empty,
                InstructorEmail = courseDto.InstructorEmail?.Trim() ?? string.Empty,
                Notes = courseDto.Notes?.Trim(),
                NotificationsEnabled = courseDto.NotificationsEnabled,
                CreditHours = courseDto.CreditHours > 0 ? courseDto.CreditHours : 3,
                CurrentGrade = courseDto.CurrentGrade,
                LetterGrade = courseDto.LetterGrade,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var result = new CourseDTO
            {
                Id = course.Id,
                TermId = course.TermId,
                Title = course.Title,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Status = course.Status,
                InstructorName = course.InstructorName,
                InstructorPhone = course.InstructorPhone,
                InstructorEmail = course.InstructorEmail,
                Notes = course.Notes,
                NotificationsEnabled = course.NotificationsEnabled,
                CreditHours = course.CreditHours,
                CurrentGrade = course.CurrentGrade,
                LetterGrade = course.LetterGrade,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt
            };

            return Ok(ApiResponse<CourseDTO>.SuccessResponse(result, "Course created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return StatusCode(500, ApiResponse<CourseDTO>.ErrorResponse($"An error occurred while creating the course: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CourseDTO>>> UpdateCourse(int id, [FromBody] CourseDTO courseDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<CourseDTO>.ErrorResponse("User not authenticated."));
            }

            if (courseDto == null)
            {
                return BadRequest(ApiResponse<CourseDTO>.ErrorResponse("Course data is required."));
            }

            var course = await _context.Courses
                .Include(c => c.Term)
                .FirstOrDefaultAsync(c => c.Id == id && c.Term.UserId == userId);

            if (course == null)
            {
                return NotFound(ApiResponse<CourseDTO>.ErrorResponse("Course not found."));
            }

            if (string.IsNullOrWhiteSpace(courseDto.Title))
            {
                return BadRequest(ApiResponse<CourseDTO>.ErrorResponse("Course title is required."));
            }

            if (courseDto.EndDate <= courseDto.StartDate)
            {
                return BadRequest(ApiResponse<CourseDTO>.ErrorResponse("End date must be after start date."));
            }

            course.Title = courseDto.Title.Trim();
            course.StartDate = courseDto.StartDate.ToUniversalTime();
            course.EndDate = courseDto.EndDate.ToUniversalTime();
            course.Status = courseDto.Status ?? course.Status;
            course.InstructorName = courseDto.InstructorName?.Trim() ?? string.Empty;
            course.InstructorPhone = courseDto.InstructorPhone?.Trim() ?? string.Empty;
            course.InstructorEmail = courseDto.InstructorEmail?.Trim() ?? string.Empty;
            course.Notes = courseDto.Notes?.Trim();
            course.NotificationsEnabled = courseDto.NotificationsEnabled;
            course.CreditHours = courseDto.CreditHours > 0 ? courseDto.CreditHours : course.CreditHours;
            course.CurrentGrade = courseDto.CurrentGrade;
            course.LetterGrade = courseDto.LetterGrade;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new CourseDTO
            {
                Id = course.Id,
                TermId = course.TermId,
                Title = course.Title,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Status = course.Status,
                InstructorName = course.InstructorName,
                InstructorPhone = course.InstructorPhone,
                InstructorEmail = course.InstructorEmail,
                Notes = course.Notes,
                NotificationsEnabled = course.NotificationsEnabled,
                CreditHours = course.CreditHours,
                CurrentGrade = course.CurrentGrade,
                LetterGrade = course.LetterGrade,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt
            };

            return Ok(ApiResponse<CourseDTO>.SuccessResponse(result, "Course updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId}", id);
            return StatusCode(500, ApiResponse<CourseDTO>.ErrorResponse($"An error occurred while updating the course: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCourse(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated."));
            }

            var course = await _context.Courses
                .Include(c => c.Term)
                .FirstOrDefaultAsync(c => c.Id == id && c.Term.UserId == userId);

            if (course == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Course not found."));
            }

            // Cascade delete will handle assessments
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Course deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course {CourseId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred while deleting the course: {ex.Message}"));
        }
    }
}

