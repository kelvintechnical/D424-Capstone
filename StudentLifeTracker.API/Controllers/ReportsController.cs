using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentLifeTracker.API.Services;
using StudentLifeTracker.Shared.DTOs;
using System.Security.Claims;
using System.Text;

namespace StudentLifeTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet("gpa/{termId}")]
    public async Task<ActionResult<ApiResponse<GpaReportDTO>>> GetGpaReport(int termId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<GpaReportDTO>.ErrorResponse("User not authenticated."));
            }

            var report = await _reportService.GenerateGpaReportAsync(userId, termId);
            return Ok(ApiResponse<GpaReportDTO>.SuccessResponse(report));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<GpaReportDTO>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating GPA report");
            return StatusCode(500, ApiResponse<GpaReportDTO>.ErrorResponse("An error occurred while generating the report."));
        }
    }

    [HttpGet("gpa/{termId}/csv")]
    public async Task<IActionResult> GetGpaReportCsv(int termId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var report = await _reportService.GenerateGpaReportAsync(userId, termId);
            var csv = _reportService.GenerateGpaCsv(report);

            var fileName = $"GPA_Report_{report.TermTitle.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv);

            return File(bytes, "text/csv", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating GPA report CSV");
            return StatusCode(500, "An error occurred while generating the report.");
        }
    }

    [HttpGet("transcript")]
    public async Task<ActionResult<ApiResponse<TranscriptReportDTO>>> GetTranscriptReport()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<TranscriptReportDTO>.ErrorResponse("User not authenticated."));
            }

            var report = await _reportService.GenerateTranscriptReportAsync(userId);
            return Ok(ApiResponse<TranscriptReportDTO>.SuccessResponse(report));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<TranscriptReportDTO>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transcript report");
            return StatusCode(500, ApiResponse<TranscriptReportDTO>.ErrorResponse("An error occurred while generating the report."));
        }
    }

    [HttpGet("transcript/csv")]
    public async Task<IActionResult> GetTranscriptCsv()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var report = await _reportService.GenerateTranscriptReportAsync(userId);
            var csv = _reportService.GenerateTranscriptCsv(report);

            var fileName = $"Academic_Transcript_{DateTime.UtcNow:yyyyMMdd}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv);

            return File(bytes, "text/csv", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transcript CSV");
            return StatusCode(500, "An error occurred while generating the report.");
        }
    }
}









