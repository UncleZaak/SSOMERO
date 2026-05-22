using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class ClassElectionApiService : IClassElectionApiService
{
    private readonly IApiService _api;
    private readonly ILogger<ClassElectionApiService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ClassElectionApiService(IApiService api, ILogger<ClassElectionApiService> logger)
    {
        _api    = api;
        _logger = logger;
    }

    private static StringContent Json<T>(T obj) =>
        new(JsonSerializer.Serialize(obj, JsonOpts), Encoding.UTF8, "application/json");

    public async Task<ClassElectionModel?> StartElectionAsync(Guid classId, CancellationToken ct = default)
    {
        try
        {
            var body = Json(new StartElectionRequest { ClassId = classId });
            var resp = await _api.PostAsync("class-elections/start", body, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassElectionModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "StartElectionAsync failed for class {ClassId}", classId);
            return null;
        }
    }

    public async Task<ClassElectionModel?> GetActiveElectionAsync(Guid classId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync($"class-elections/active/{classId}", ct);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassElectionModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetActiveElectionAsync failed for class {ClassId}", classId);
            return null;
        }
    }

    public async Task<ClassElectionModel?> VoteAsync(Guid electionId, Guid candidateStudentId, CancellationToken ct = default)
    {
        try
        {
            var body = Json(new VoteRequest { CandidateStudentId = candidateStudentId });
            var resp = await _api.PostAsync($"class-elections/{electionId}/vote", body, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassElectionModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "VoteAsync failed for election {ElectionId}", electionId);
            return null;
        }
    }

    public async Task<ClassElectionModel?> FinalizeElectionAsync(Guid electionId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync($"class-elections/{electionId}/finalize", new StringContent("", Encoding.UTF8, "application/json"), ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ClassElectionModel>(JsonOpts, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "FinalizeElectionAsync failed for election {ElectionId}", electionId);
            return null;
        }
    }

    public async Task<List<ClassDto>> GetMyClassesAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("my-classes", ct);
            if (!resp.IsSuccessStatusCode) return [];
            return await resp.Content.ReadFromJsonAsync<List<ClassDto>>(JsonOpts, ct) ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetMyClassesAsync failed");
            return [];
        }
    }
}
