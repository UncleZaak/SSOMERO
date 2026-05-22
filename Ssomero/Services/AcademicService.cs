using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class AcademicService : IAcademicService
{
    private readonly IApiService _api;
    private readonly ILogger<AcademicService> _logger;

    private static readonly TimeSpan LookupCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AdminCacheDuration = TimeSpan.FromMinutes(2);

    private readonly ConcurrentDictionary<string, (IEnumerable<LookupItem> Data, DateTime CachedAt)> _cache = new();
    private readonly ConcurrentDictionary<string, (object Data, DateTime CachedAt)> _adminCache = new();

    public AcademicService(IApiService api, ILogger<AcademicService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public Task<IEnumerable<LookupItem>> GetUniversitiesAsync() => GetCachedListAsync("universities");
    public Task<IEnumerable<LookupItem>> GetFacultiesAsync(string universityId) => GetCachedListAsync($"faculties?universityId={universityId}");
    public Task<IEnumerable<LookupItem>> GetDepartmentsAsync(string facultyId) => GetCachedListAsync($"departments?facultyId={facultyId}");
    public Task<IEnumerable<LookupItem>> GetProgramsAsync(string departmentId) => GetCachedListAsync($"programs?departmentId={departmentId}");
    public Task<IEnumerable<LookupItem>> GetEntrySchemesAsync() => GetCachedListAsync("entry-schemes");
    public Task<IEnumerable<LookupItem>> GetIntakesAsync() => GetCachedListAsync("intakes");
    public Task<IEnumerable<LookupItem>> GetStudyModesAsync() => GetCachedListAsync("study-modes");
    public Task<IEnumerable<LookupItem>> GetAcademicYearsAsync() => GetCachedListAsync("academic-years");
    public Task<IEnumerable<LookupItem>> GetSemestersAsync() => GetCachedListAsync("semesters");

    private async Task<IEnumerable<LookupItem>> GetCachedListAsync(string path)
    {
        if (_cache.TryGetValue(path, out var cached) && DateTime.UtcNow - cached.CachedAt < LookupCacheDuration)
        {
            return cached.Data;
        }

        var data = await GetListAsync(path);
        // Only cache non-empty results so a transient error does not lock out the picker for 10 minutes.
        if (data.Any())
            _cache[path] = (data, DateTime.UtcNow);
        return data;
    }

    private async Task<IEnumerable<LookupItem>> GetListAsync(string path)
    {
        try
        {
            var resp = await _api.GetAsync(path);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("{Path} returned {StatusCode}", path, resp.StatusCode);
                return [];
            }
            return await resp.Content.ReadFromJsonAsync<IEnumerable<LookupItem>>() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch {Path}", path);
            return [];
        }
    }

    // ──────────────────────────────────────────────
    //  University CRUD
    // ──────────────────────────────────────────────

    public async Task<List<UniversityDto>> GetUniversityDetailsAsync()
    {
        try
        {
            var resp = await _api.GetAsync("universities/details");
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<UniversityDto>>() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch university details");
            return [];
        }
    }

    public async Task<PaginatedResult<UniversityDto>> GetUniversitiesPaginatedAsync(int page, int pageSize, string? search = null)
    {
        try
        {
            var query = $"universities/details?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            var resp = await _api.GetAsync(query);
            if (!resp.IsSuccessStatusCode) return new PaginatedResult<UniversityDto>();
            return await resp.Content.ReadFromJsonAsync<PaginatedResult<UniversityDto>>()
                   ?? new PaginatedResult<UniversityDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch universities paginated");
            return new PaginatedResult<UniversityDto>();
        }
    }

    public async Task<UniversityDto?> GetUniversityByIdAsync(string id)
    {
        try
        {
            var resp = await _api.GetAsync($"universities/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<UniversityDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch university {Id}", id);
            return null;
        }
    }

    public async Task<UniversityDto?> CreateUniversityAsync(string name)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { name });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PostAsync("universities", content);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<UniversityDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create university");
            return null;
        }
    }

    public async Task<UniversityDto?> UpdateUniversityAsync(string id, string name)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { name });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PutAsync($"universities/{id}", content);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<UniversityDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update university {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteUniversityAsync(string id)
    {
        try
        {
            var resp = await _api.DeleteAsync($"universities/{id}");
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete university {Id}", id);
            return false;
        }
    }

    // ──────────────────────────────────────────────
    //  Faculty CRUD
    // ──────────────────────────────────────────────

    public async Task<List<FacultyDto>> GetFacultyDetailsAsync()
    {
        try
        {
            var resp = await _api.GetAsync("v1/admin/academic/faculties?page=1&pageSize=500");
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("v1/admin/academic/faculties returned {StatusCode}", resp.StatusCode);
                return [];
            }
            var paged = await resp.Content.ReadFromJsonAsync<PaginatedResult<FacultyDto>>();
            return paged?.Data?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch faculty details");
            return [];
        }
    }

    public async Task<FacultyDto?> CreateFacultyAsync(string name, string universityId)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { name, universityId });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PostAsync("v1/admin/academic/faculties", content);
            if (!resp.IsSuccessStatusCode) return null;
            InvalidateAdminCachePrefix($"faculties:{universityId}");
            return await resp.Content.ReadFromJsonAsync<FacultyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create faculty");
            return null;
        }
    }

    public async Task<FacultyDto?> UpdateFacultyAsync(string id, string name, string universityId)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { name, universityId });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PutAsync($"v1/admin/academic/faculties/{id}", content);
            if (!resp.IsSuccessStatusCode) return null;
            InvalidateAdminCachePrefix("faculties:");
            return await resp.Content.ReadFromJsonAsync<FacultyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update faculty {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteFacultyAsync(string id)
    {
        try
        {
            var resp = await _api.DeleteAsync($"v1/admin/academic/faculties/{id}");
            if (resp.IsSuccessStatusCode) InvalidateAdminCachePrefix("faculties:");
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete faculty {Id}", id);
            return false;
        }
    }

    // ──────────────────────────────────────────────
    //  Department CRUD
    // ──────────────────────────────────────────────

    public async Task<List<DepartmentDto>> GetDepartmentDetailsAsync(string? search = null)
    {
        try
        {
            var path = "v1/admin/academic/departments?pageSize=500";
            if (!string.IsNullOrWhiteSpace(search))
                path += $"&search={Uri.EscapeDataString(search)}";
            var resp = await _api.GetAsync(path);
            if (!resp.IsSuccessStatusCode) return [];
            var paged = await resp.Content.ReadFromJsonAsync<PaginatedResult<DepartmentDto>>();
            return paged?.Data?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch department details");
            return [];
        }
    }

    public async Task<DepartmentDto?> CreateDepartmentAsync(string name, string facultyId)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { name, facultyId = Guid.Parse(facultyId) });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PostAsync("v1/admin/academic/departments", content);
            if (!resp.IsSuccessStatusCode) return null;
            InvalidateAdminCachePrefix($"departments:{facultyId}");
            return await resp.Content.ReadFromJsonAsync<DepartmentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create department");
            return null;
        }
    }

    public async Task<DepartmentDto?> UpdateDepartmentAsync(string id, string name, string facultyId)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { name, facultyId = Guid.Parse(facultyId) });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PutAsync($"v1/admin/academic/departments/{id}", content);
            if (!resp.IsSuccessStatusCode) return null;
            InvalidateAdminCachePrefix("departments:");
            return await resp.Content.ReadFromJsonAsync<DepartmentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update department {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteDepartmentAsync(string id)
    {
        try
        {
            var resp = await _api.DeleteAsync($"v1/admin/academic/departments/{id}");
            if (resp.IsSuccessStatusCode) InvalidateAdminCachePrefix("departments:");
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete department {Id}", id);
            return false;
        }
    }

    // ──────────────────────────────────────────────
    //  Program CRUD
    // ──────────────────────────────────────────────

    public async Task<List<ProgramDto>> GetProgramDetailsAsync(string? search = null)
    {
        try
        {
            var path = "v1/admin/academic/programs?pageSize=500";
            if (!string.IsNullOrWhiteSpace(search))
                path += $"&search={Uri.EscapeDataString(search)}";
            var resp = await _api.GetAsync(path);
            if (!resp.IsSuccessStatusCode) return [];
            var paged = await resp.Content.ReadFromJsonAsync<PaginatedResult<ProgramDto>>();
            return paged?.Data?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch program details");
            return [];
        }
    }

    public async Task<ProgramDto?> CreateProgramAsync(string name, string departmentId, int durationSemesters)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { name, departmentId = Guid.Parse(departmentId), durationSemesters });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PostAsync("v1/admin/academic/programs", content);
            if (!resp.IsSuccessStatusCode) return null;
            InvalidateAdminCachePrefix($"programs:{departmentId}");
            return await resp.Content.ReadFromJsonAsync<ProgramDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create program");
            return null;
        }
    }

    public async Task<ProgramDto?> UpdateProgramAsync(string id, string name, string departmentId, int durationSemesters)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { name, departmentId = Guid.Parse(departmentId), durationSemesters });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PutAsync($"v1/admin/academic/programs/{id}", content);
            if (!resp.IsSuccessStatusCode) return null;
            InvalidateAdminCachePrefix("programs:");
            return await resp.Content.ReadFromJsonAsync<ProgramDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update program {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteProgramAsync(string id)
    {
        try
        {
            var resp = await _api.DeleteAsync($"v1/admin/academic/programs/{id}");
            if (resp.IsSuccessStatusCode) InvalidateAdminCachePrefix("programs:");
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete program {Id}", id);
            return false;
        }
    }

    // ──────────────────────────────────────────────
    //  Curriculum CRUD
    // ──────────────────────────────────────────────

    public async Task<List<CurriculumDto>> GetCurriculumDetailsAsync(string? programId = null, string? search = null)
    {
        try
        {
            var path = "v1/admin/academic/curriculum?pageSize=500";
            if (!string.IsNullOrWhiteSpace(programId)) path += $"&programId={Uri.EscapeDataString(programId)}";
            if (!string.IsNullOrWhiteSpace(search)) path += $"&search={Uri.EscapeDataString(search)}";

            var resp = await _api.GetAsync(path);
            if (!resp.IsSuccessStatusCode) return [];
            var paged = await resp.Content.ReadFromJsonAsync<PaginatedResult<CurriculumDto>>();
            return paged?.Data?.ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch curriculum details");
            return [];
        }
    }

    public async Task<CurriculumDto?> CreateCurriculumEntryAsync(string programId, int yearOfStudy, string semesterId, string courseCode, string courseName)
    {
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                programId = Guid.Parse(programId),
                yearOfStudy,
                semesterId = Guid.Parse(semesterId),
                courseCode,
                courseName
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PostAsync("v1/admin/academic/curriculum", content);
            if (!resp.IsSuccessStatusCode) return null;
            InvalidateAdminCachePrefix($"curriculum:{programId}");
            return await resp.Content.ReadFromJsonAsync<CurriculumDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create curriculum entry");
            return null;
        }
    }

    public async Task<CurriculumDto?> UpdateCurriculumEntryAsync(string id, string programId, int yearOfStudy, string semesterId, string courseCode, string courseName)
    {
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                programId = Guid.Parse(programId),
                yearOfStudy,
                semesterId = Guid.Parse(semesterId),
                courseCode,
                courseName
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _api.PutAsync($"v1/admin/academic/curriculum/{id}", content);
            if (!resp.IsSuccessStatusCode) return null;
            InvalidateAdminCachePrefix("curriculum:");
            return await resp.Content.ReadFromJsonAsync<CurriculumDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update curriculum entry {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteCurriculumEntryAsync(string id)
    {
        try
        {
            var resp = await _api.DeleteAsync($"v1/admin/academic/curriculum/{id}");
            if (resp.IsSuccessStatusCode) InvalidateAdminCachePrefix("curriculum:");
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete curriculum entry {Id}", id);
            return false;
        }
    }

    // ──────────────────────────────────────────────
    //  Parent-scoped paginated list methods (Phase 2)
    // ──────────────────────────────────────────────

    public Task<PaginatedResult<FacultyDto>> GetFacultiesByUniversityAsync(
        string universityId, int page = 1, int pageSize = 100, string? search = null, CancellationToken ct = default)
    {
        var key = $"faculties:{universityId}:{page}:{pageSize}:{search ?? string.Empty}";
        return GetAdminCachedAsync<FacultyDto>(key,
            $"v1/admin/academic/faculties?universityId={universityId}&page={page}&pageSize={pageSize}"
            + (string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Uri.EscapeDataString(search)}"),
            ct);
    }

    public Task<PaginatedResult<DepartmentDto>> GetDepartmentsByFacultyAsync(
        string facultyId, int page = 1, int pageSize = 100, string? search = null, CancellationToken ct = default)
    {
        var key = $"departments:{facultyId}:{page}:{pageSize}:{search ?? string.Empty}";
        return GetAdminCachedAsync<DepartmentDto>(key,
            $"v1/admin/academic/departments?facultyId={facultyId}&page={page}&pageSize={pageSize}"
            + (string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Uri.EscapeDataString(search)}"),
            ct);
    }

    public Task<PaginatedResult<ProgramDto>> GetProgramsByDepartmentAsync(
        string departmentId, int page = 1, int pageSize = 100, string? search = null, CancellationToken ct = default)
    {
        var key = $"programs:{departmentId}:{page}:{pageSize}:{search ?? string.Empty}";
        return GetAdminCachedAsync<ProgramDto>(key,
            $"v1/admin/academic/programs?departmentId={departmentId}&page={page}&pageSize={pageSize}"
            + (string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Uri.EscapeDataString(search)}"),
            ct);
    }

    public Task<PaginatedResult<CurriculumDto>> GetCurriculumByProgramAsync(
        string programId, int page = 1, int pageSize = 100, string? search = null, CancellationToken ct = default)
    {
        var key = $"curriculum:{programId}:{page}:{pageSize}:{search ?? string.Empty}";
        return GetAdminCachedAsync<CurriculumDto>(key,
            $"v1/admin/academic/curriculum?programId={programId}&page={page}&pageSize={pageSize}"
            + (string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Uri.EscapeDataString(search)}"),
            ct);
    }

    private async Task<PaginatedResult<T>> GetAdminCachedAsync<T>(string cacheKey, string path, CancellationToken ct)
    {
        if (_adminCache.TryGetValue(cacheKey, out var cached)
            && DateTime.UtcNow - cached.CachedAt < AdminCacheDuration)
        {
            return (PaginatedResult<T>)cached.Data;
        }

        try
        {
            var resp = await _api.GetAsync(path, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("{Path} returned {StatusCode}", path, resp.StatusCode);
                return new PaginatedResult<T>();
            }
            var result = await resp.Content.ReadFromJsonAsync<PaginatedResult<T>>(cancellationToken: ct)
                         ?? new PaginatedResult<T>();

            // Do NOT cache empty results
            if (result.Data.Any())
                _adminCache[cacheKey] = (result, DateTime.UtcNow);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch {Path}", path);
            return new PaginatedResult<T>();
        }
    }

    /// <summary>Remove all admin cache entries whose key starts with the given prefix.</summary>
    private void InvalidateAdminCachePrefix(string prefix)
    {
        foreach (var key in _adminCache.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
            _adminCache.TryRemove(key, out _);
    }
}
