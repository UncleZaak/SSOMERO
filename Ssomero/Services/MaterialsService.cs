using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class MaterialsService : IMaterialsService
{
    private readonly IApiService _api;
    private readonly ILogger<MaterialsService> _logger;

    public MaterialsService(IApiService api, ILogger<MaterialsService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IEnumerable<StudyMaterialDto>> GetMaterialsAsync(string? courseId = null, CancellationToken ct = default)
    {
        var path = string.IsNullOrWhiteSpace(courseId)
            ? "materials"
            : $"materials?courseId={courseId}";

        var resp = await _api.GetAsync(path, ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("GetMaterials returned {StatusCode}", resp.StatusCode);
            return [];
        }
        return await resp.Content.ReadFromJsonAsync<IEnumerable<StudyMaterialDto>>(cancellationToken: ct) ?? [];
    }
}
