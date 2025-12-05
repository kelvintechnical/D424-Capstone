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
public class SearchController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ApplicationDbContext context, ILogger<SearchController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet("courses")]
    public async Task<ActionResult<ApiResponse<List<SearchResultDTO>>>> SearchCourses(
        [FromQuery] string query,
        [FromQuery] string? status = null,
        [FromQuery] int? termId = null)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<SearchResultDTO>>.ErrorResponse("User not authenticated."));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(ApiResponse<List<SearchResultDTO>>.ErrorResponse("Search query is required."));
            }

            var searchQuery = query.Trim().ToLower();

            var coursesQuery = _context.Courses
                .Include(c => c.Term)
                .Where(c => c.Term.UserId == userId);

            // Apply search filter (case-insensitive, partial matching)
            coursesQuery = coursesQuery.Where(c =>
                c.Title.ToLower().Contains(searchQuery) ||
                c.InstructorName.ToLower().Contains(searchQuery) ||
                c.InstructorEmail.ToLower().Contains(searchQuery));

            // Apply status filter if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                coursesQuery = coursesQuery.Where(c => c.Status == status);
            }

            // Apply term filter if provided
            if (termId.HasValue)
            {
                coursesQuery = coursesQuery.Where(c => c.TermId == termId.Value);
            }

            var courses = await coursesQuery
                .OrderBy(c => c.Title)
                .ToListAsync();

            var results = courses.Select(c => new SearchResultDTO
            {
                ResultType = "Course",
                Id = c.Id,
                Title = c.Title,
                Description = c.Status,
                ParentId = c.TermId,
                ParentTitle = c.Term.Title
            }).ToList();

            return Ok(ApiResponse<List<SearchResultDTO>>.SuccessResponse(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching courses");
            return StatusCode(500, ApiResponse<List<SearchResultDTO>>.ErrorResponse("An error occurred while searching courses."));
        }
    }

    [HttpGet("terms")]
    public async Task<ActionResult<ApiResponse<List<SearchResultDTO>>>> SearchTerms([FromQuery] string query)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<SearchResultDTO>>.ErrorResponse("User not authenticated."));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(ApiResponse<List<SearchResultDTO>>.ErrorResponse("Search query is required."));
            }

            var searchQuery = query.Trim().ToLower();

            var terms = await _context.Terms
                .Where(t => t.UserId == userId && t.Title.ToLower().Contains(searchQuery))
                .OrderBy(t => t.Title)
                .ToListAsync();

            var results = terms.Select(t => new SearchResultDTO
            {
                ResultType = "Term",
                Id = t.Id,
                Title = t.Title,
                Description = $"{t.StartDate:MMM dd, yyyy} - {t.EndDate:MMM dd, yyyy}",
                ParentId = null,
                ParentTitle = null
            }).ToList();

            return Ok(ApiResponse<List<SearchResultDTO>>.SuccessResponse(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching terms");
            return StatusCode(500, ApiResponse<List<SearchResultDTO>>.ErrorResponse("An error occurred while searching terms."));
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<SearchResultDTO>>>> SearchAll([FromQuery] string query)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<SearchResultDTO>>.ErrorResponse("User not authenticated."));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(ApiResponse<List<SearchResultDTO>>.ErrorResponse("Search query is required."));
            }

            var searchQuery = query.Trim().ToLower();
            var results = new List<SearchResultDTO>();

            // Search terms
            var terms = await _context.Terms
                .Where(t => t.UserId == userId && t.Title.ToLower().Contains(searchQuery))
                .OrderBy(t => t.Title)
                .ToListAsync();

            results.AddRange(terms.Select(t => new SearchResultDTO
            {
                ResultType = "Term",
                Id = t.Id,
                Title = t.Title,
                Description = $"{t.StartDate:MMM dd, yyyy} - {t.EndDate:MMM dd, yyyy}",
                ParentId = null,
                ParentTitle = null
            }));

            // Search courses
            var courses = await _context.Courses
                .Include(c => c.Term)
                .Where(c => c.Term.UserId == userId &&
                    (c.Title.ToLower().Contains(searchQuery) ||
                     c.InstructorName.ToLower().Contains(searchQuery) ||
                     c.InstructorEmail.ToLower().Contains(searchQuery)))
                .OrderBy(c => c.Title)
                .ToListAsync();

            results.AddRange(courses.Select(c => new SearchResultDTO
            {
                ResultType = "Course",
                Id = c.Id,
                Title = c.Title,
                Description = c.Status,
                ParentId = c.TermId,
                ParentTitle = c.Term.Title
            }));

            // Sort by title
            results = results.OrderBy(r => r.Title).ToList();

            return Ok(ApiResponse<List<SearchResultDTO>>.SuccessResponse(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing global search");
            return StatusCode(500, ApiResponse<List<SearchResultDTO>>.ErrorResponse("An error occurred while searching."));
        }
    }
}

