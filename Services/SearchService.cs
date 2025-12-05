using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

public class SearchService
{
    private readonly DatabaseService _db;
    private readonly ApiService _apiService;

    public SearchService(DatabaseService db, ApiService apiService)
    {
        _db = db;
        _apiService = apiService;
    }

    public async Task<List<SearchResultDTO>> SearchCoursesAsync(string query, string? status = null, int? termId = null)
    {
        try
        {
            // Try API first
            var response = await _apiService.SearchCoursesAsync(query, status, termId);
            if (response.Success && response.Data != null)
            {
                return response.Data;
            }
        }
        catch
        {
            // Fall back to local search if API fails
        }

        // Local fallback
        return await SearchCoursesLocalAsync(query, status, termId);
    }

    public async Task<List<SearchResultDTO>> SearchTermsAsync(string query)
    {
        try
        {
            // Try API first
            var response = await _apiService.SearchTermsAsync(query);
            if (response.Success && response.Data != null)
            {
                return response.Data;
            }
        }
        catch
        {
            // Fall back to local search if API fails
        }

        // Local fallback
        return await SearchTermsLocalAsync(query);
    }

    public async Task<List<SearchResultDTO>> SearchAllAsync(string query)
    {
        try
        {
            // Try API first
            var response = await _apiService.SearchAllAsync(query);
            if (response.Success && response.Data != null)
            {
                return response.Data;
            }
        }
        catch
        {
            // Fall back to local search if API fails
        }

        // Local fallback - combine term and course results
        var results = new List<SearchResultDTO>();
        results.AddRange(await SearchTermsLocalAsync(query));
        results.AddRange(await SearchCoursesLocalAsync(query, null, null));
        return results.OrderBy(r => r.Title).ToList();
    }

    private async Task<List<SearchResultDTO>> SearchCoursesLocalAsync(string query, string? status, int? termId)
    {
        var searchQuery = query.Trim().ToLower();
        var allCourses = new List<Course>();

        if (termId.HasValue)
        {
            allCourses = await _db.GetCoursesByTermAsync(termId.Value);
        }
        else
        {
            // Get all courses from all terms
            var terms = await _db.GetAllTermsAsync();
            foreach (var term in terms)
            {
                var courses = await _db.GetCoursesByTermAsync(term.Id);
                allCourses.AddRange(courses);
            }
        }

        var filtered = allCourses.Where(c =>
            c.Title.ToLower().Contains(searchQuery) ||
            (c.Instructor?.Name?.ToLower().Contains(searchQuery) ?? false) ||
            (c.Instructor?.Email?.ToLower().Contains(searchQuery) ?? false));

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(c => c.Status == status);
        }

        var results = new List<SearchResultDTO>();
        foreach (var course in filtered)
        {
            var term = await _db.GetTermAsync(course.TermId);
            results.Add(new SearchResultDTO
            {
                ResultType = "Course",
                Id = course.Id,
                Title = course.Title,
                Description = course.Status,
                ParentId = course.TermId,
                ParentTitle = term?.Title
            });
        }

        return results.OrderBy(r => r.Title).ToList();
    }

    private async Task<List<SearchResultDTO>> SearchTermsLocalAsync(string query)
    {
        var searchQuery = query.Trim().ToLower();
        var terms = await _db.GetAllTermsAsync();

        var filtered = terms.Where(t => t.Title.ToLower().Contains(searchQuery));

        return filtered.Select(t => new SearchResultDTO
        {
            ResultType = "Term",
            Id = t.Id,
            Title = t.Title,
            Description = $"{t.StartDate:MMM dd, yyyy} - {t.EndDate:MMM dd, yyyy}",
            ParentId = null,
            ParentTitle = null
        }).OrderBy(r => r.Title).ToList();
    }
}

