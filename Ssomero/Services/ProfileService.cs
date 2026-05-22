using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class ProfileService : IProfileService
{
    private readonly IApiService _api;
    private readonly SessionService _session;
    private readonly ILogger<ProfileService> _logger;

    // Cached options for deserialising role-specific DTOs
    private static readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web);

    public ProfileService(IApiService api, SessionService session, ILogger<ProfileService> logger)
    {
        _api     = api;
        _session = session;
        _logger  = logger;
    }

    public async Task<ProfileDto?> GetProfileAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.GetAsync("profile", ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetProfile returned {StatusCode}", resp.StatusCode);
                return null;
            }

            // Deserialise into the concrete role-specific type so all extra fields are populated.
            var role = _session.Role;
            var json = await resp.Content.ReadAsStringAsync(ct);

            ProfileDto? profile = role switch
            {
                UserRole.Student  => JsonSerializer.Deserialize<StudentProfileDto>(json, _jsonOpts),
                UserRole.Lecturer => JsonSerializer.Deserialize<LecturerProfileDto>(json, _jsonOpts),
                UserRole.Admin    => JsonSerializer.Deserialize<AdminProfileDto>(json, _jsonOpts),
                _                 => JsonSerializer.Deserialize<ProfileDto>(json, _jsonOpts)
            };

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfileAsync failed");
            return null;
        }
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileRequest dto, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PutAsync("profile", JsonContent.Create(dto), ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateProfileAsync failed");
            return false;
        }
    }

    public async Task<string?> ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken ct = default)
    {
        try
        {
            var resp = await _api.PostAsync("profile/change-password", JsonContent.Create(dto), ct);
            if (resp.IsSuccessStatusCode) return null; // null = success

            var body = await resp.Content.ReadAsStringAsync(ct);
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var err))
                    return err.GetString() ?? "Password change failed.";
            }
            catch { }

            return "Password change failed. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangePasswordAsync failed");
            return "Network error. Please check your connection.";
        }
    }
}
