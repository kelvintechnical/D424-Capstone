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
public class TermsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TermsController> _logger;

    public TermsController(ApplicationDbContext context, ILogger<TermsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TermDTO>>>> GetTerms()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<TermDTO>>.ErrorResponse("User not authenticated."));
            }

            var terms = await _context.Terms
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.StartDate)
                .Select(t => new TermDTO
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Title = t.Title,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TermDTO>>.SuccessResponse(terms));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting terms");
            return StatusCode(500, ApiResponse<List<TermDTO>>.ErrorResponse("An error occurred while retrieving terms."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TermDTO>>> GetTerm(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<TermDTO>.ErrorResponse("User not authenticated."));
            }

            var term = await _context.Terms
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (term == null)
            {
                return NotFound(ApiResponse<TermDTO>.ErrorResponse("Term not found."));
            }

            var result = new TermDTO
            {
                Id = term.Id,
                UserId = term.UserId,
                Title = term.Title,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
                CreatedAt = term.CreatedAt,
                UpdatedAt = term.UpdatedAt
            };

            return Ok(ApiResponse<TermDTO>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting term {TermId}", id);
            return StatusCode(500, ApiResponse<TermDTO>.ErrorResponse("An error occurred while retrieving the term."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TermDTO>>> CreateTerm([FromBody] TermDTO termDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<TermDTO>.ErrorResponse("User not authenticated."));
            }

            if (termDto == null)
            {
                return BadRequest(ApiResponse<TermDTO>.ErrorResponse("Term data is required."));
            }

            if (string.IsNullOrWhiteSpace(termDto.Title))
            {
                return BadRequest(ApiResponse<TermDTO>.ErrorResponse("Term title is required."));
            }

            if (termDto.EndDate <= termDto.StartDate)
            {
                return BadRequest(ApiResponse<TermDTO>.ErrorResponse("End date must be after start date."));
            }

            var term = new Term
            {
                UserId = userId,
                Title = termDto.Title.Trim(),
                StartDate = termDto.StartDate.ToUniversalTime(),
                EndDate = termDto.EndDate.ToUniversalTime(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Terms.Add(term);
            await _context.SaveChangesAsync();

            var result = new TermDTO
            {
                Id = term.Id,
                UserId = term.UserId,
                Title = term.Title,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
                CreatedAt = term.CreatedAt,
                UpdatedAt = term.UpdatedAt
            };

            return Ok(ApiResponse<TermDTO>.SuccessResponse(result, "Term created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating term");
            return StatusCode(500, ApiResponse<TermDTO>.ErrorResponse($"An error occurred while creating the term: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TermDTO>>> UpdateTerm(int id, [FromBody] TermDTO termDto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<TermDTO>.ErrorResponse("User not authenticated."));
            }

            if (termDto == null)
            {
                return BadRequest(ApiResponse<TermDTO>.ErrorResponse("Term data is required."));
            }

            var term = await _context.Terms
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (term == null)
            {
                return NotFound(ApiResponse<TermDTO>.ErrorResponse("Term not found."));
            }

            if (string.IsNullOrWhiteSpace(termDto.Title))
            {
                return BadRequest(ApiResponse<TermDTO>.ErrorResponse("Term title is required."));
            }

            if (termDto.EndDate <= termDto.StartDate)
            {
                return BadRequest(ApiResponse<TermDTO>.ErrorResponse("End date must be after start date."));
            }

            term.Title = termDto.Title.Trim();
            term.StartDate = termDto.StartDate.ToUniversalTime();
            term.EndDate = termDto.EndDate.ToUniversalTime();
            term.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new TermDTO
            {
                Id = term.Id,
                UserId = term.UserId,
                Title = term.Title,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
                CreatedAt = term.CreatedAt,
                UpdatedAt = term.UpdatedAt
            };

            return Ok(ApiResponse<TermDTO>.SuccessResponse(result, "Term updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating term {TermId}", id);
            return StatusCode(500, ApiResponse<TermDTO>.ErrorResponse($"An error occurred while updating the term: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTerm(int id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated."));
            }

            var term = await _context.Terms
                .Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (term == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Term not found."));
            }

            // Cascade delete will handle courses and assessments
            _context.Terms.Remove(term);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Term deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting term {TermId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred while deleting the term: {ex.Message}"));
        }
    }
}

