using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class CoursesService : ICoursesService
{
    private readonly IApiService _api;
    private readonly ILogger<CoursesService> _logger;

    public CoursesService(IApiService api, ILogger<CoursesService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesAsync()
    {
        var resp = await _api.GetAsync("courses");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetCourses returned {StatusCode}", resp.StatusCode);
            return [];
        }
        var list = await resp.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        return list ?? [];
    }

    public async Task<CourseDto?> GetCourseAsync(string id)
    {
        var resp = await _api.GetAsync($"courses/{id}");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetCourse({Id}) returned {StatusCode}", id, resp.StatusCode);
            return null;
        }
        return await resp.Content.ReadFromJsonAsync<CourseDto>();
    }
}