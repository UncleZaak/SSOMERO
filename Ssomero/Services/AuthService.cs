using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class AuthService : IAuthService
{
    private readonly IApiService _api;
    private readonly TokenStorageService _tokenStorage;
    private readonly SessionService _session;
    private readonly PollingService _polling;
    private readonly ICacheService _cache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IApiService api,
        TokenStorageService tokenStorage,
        SessionService session,
        PollingService polling,
        ICacheService cache,
        ILogger<AuthService> logger)
    {
        _api          = api;
        _tokenStorage = tokenStorage;
        _session      = session;
        _polling      = polling;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<AuthResponseDto?> LoginAsync(string email, string password)
    {
        _logger.LogInformation("Login attempt for {Email}", email);
        var payload = new { email, password };
        var resp = await _api.PostAsync("auth/login", JsonContent.Create(payload));
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("Login failed with status {StatusCode}: {Body}", resp.StatusCode, body);

            var errorMsg = "Invalid credentials or server error.";
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var errorProp))
                    errorMsg = errorProp.GetString() ?? errorMsg;
            }
            catch { /* response wasn't JSON */ }

            throw new HttpRequestException(errorMsg);
        }
        var dto = await resp.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (dto is not null)
        {
            await _tokenStorage.StoreTokensAsync(dto.AccessToken, dto.RefreshToken, dto.ExpiresAt);
            _logger.LogInformation("Login successful for {Email}", email);
        }
        return dto;
    }

    [Obsolete("Use RegisterStudentAsync instead. This method will be removed in a future release.")]
    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Registration attempt for {Email}", dto.Email);
        var resp = await _api.PostAsync("auth/register", JsonContent.Create(dto));
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("Registration failed with status {StatusCode}: {Body}", resp.StatusCode, body);
            return false;
        }
        return true;
    }

    public async Task<bool> RegisterStudentAsync(StudentRegisterDto dto)
    {
        _logger.LogInformation("Student registration for {Email}", dto.Email);
        var resp = await _api.PostAsync("auth/register", JsonContent.Create(dto));
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("Student registration failed: {StatusCode} {Body}", resp.StatusCode, body);

            var errorMsg = resp.StatusCode switch
            {
                System.Net.HttpStatusCode.Conflict    => "Email already registered. Please use a different email.",
                System.Net.HttpStatusCode.BadRequest  => "Validation error. Please check your details.",
                System.Net.HttpStatusCode.InternalServerError => "Server error. Please try again later.",
                _ => "Registration failed. Please try again."
            };

            // Prefer the server-supplied message if present
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var errProp))
                    errorMsg = errProp.GetString() ?? errorMsg;
            }
            catch { /* response wasn't JSON */ }

            throw new HttpRequestException(errorMsg);
        }
        return true;
    }

    public async Task<bool> RegisterLecturerAsync(LecturerRegisterDto dto)
    {
        _logger.LogInformation("Lecturer registration for {Email}", dto.Email);
        var resp = await _api.PostAsync("auth/lecturer/register", JsonContent.Create(dto));
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("Lecturer registration failed: {StatusCode} {Body}", resp.StatusCode, body);

            var errorMsg = resp.StatusCode switch
            {
                System.Net.HttpStatusCode.Conflict => "Email already registered. Please use a different email.",
                System.Net.HttpStatusCode.BadRequest => "Validation error. Please check your details.",
                System.Net.HttpStatusCode.InternalServerError => "Server error. Please try again later.",
                _ => "Registration failed. Please try again."
            };

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var errProp))
                    errorMsg = errProp.GetString() ?? errorMsg;
            }
            catch { }

            throw new HttpRequestException(errorMsg);
        }
        return true;
    }

    public async Task<bool> SendOtpAsync(string email)
    {
        _logger.LogInformation("Sending OTP to {Email}", email);
        var resp = await _api.PostAsync("auth/send-otp", JsonContent.Create(new { email }));
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("send-otp failed for {Email}: {StatusCode} {Body}", email, resp.StatusCode, body);
        }
        return resp.IsSuccessStatusCode;
    }

    public async Task<string?> VerifyOtpAsync(string email, string otpCode)
    {
        _logger.LogInformation("Verifying OTP for {Email}", email);
        var resp = await _api.PostAsync("auth/verify-otp", JsonContent.Create(new { email, otpCode }));
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("OTP verification failed for {Email}: {StatusCode}", email, resp.StatusCode);
            return null;
        }

        var result = await resp.Content.ReadFromJsonAsync<VerifyOtpResponseDto>();
        if (result?.VerificationToken is null)
            _logger.LogWarning("OTP verified but no token returned for {Email}", email);
        return result?.VerificationToken;
    }

    public async Task<bool> TryRefreshTokenAsync()
    {
        var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken)) return false;

        var payload = new { refreshToken };
        var resp = await _api.PostAsync("auth/refresh", JsonContent.Create(payload));
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Token refresh returned {StatusCode}; logging out", resp.StatusCode);
            await LogoutAsync();
            return false;
        }

        var dto = await resp.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (dto is null)
        {
            _logger.LogWarning("Token refresh succeeded but response body was empty; logging out");
            await LogoutAsync();
            return false;
        }

        await _tokenStorage.StoreTokensAsync(dto.AccessToken, dto.RefreshToken, dto.ExpiresAt);
        return true;
    }

    public async Task LogoutAsync()
    {
        _logger.LogInformation("User logging out");
        _polling.Stop();
        _cache.Clear();
        _session.Clear();
        try { SecureStorage.Remove("user_role"); } catch { }
        await _tokenStorage.ClearAsync();
        await Shell.Current.GoToAsync("//LoginPage");  // awaited to prevent navigation race
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        _logger.LogInformation("Forgot-password request for {Email}", email);
        var resp = await _api.PostAsync("auth/forgot-password", JsonContent.Create(new { email }));
        // Always returns 200 from the backend regardless of whether the email exists
        return resp.IsSuccessStatusCode;
    }

    public async Task<string?> VerifyResetOtpAsync(string email, string otpCode)
    {
        _logger.LogInformation("Verifying reset OTP for {Email}", email);
        var resp = await _api.PostAsync("auth/verify-reset-otp", JsonContent.Create(new { email, otpCode }));
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Reset OTP verification failed for {Email}: {StatusCode}", email, resp.StatusCode);
            return null;
        }
        var result = await resp.Content.ReadFromJsonAsync<VerifyResetOtpResponseDto>();
        return result?.ResetToken;
    }

    public async Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        _logger.LogInformation("Password reset attempt for {Email}", email);
        var resp = await _api.PostAsync("auth/reset-password",
            JsonContent.Create(new { email, resetToken, newPassword }));
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            var errorMsg = "Invalid or expired reset token.";
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var errProp))
                    errorMsg = errProp.GetString() ?? errorMsg;
            }
            catch { }
            throw new HttpRequestException(errorMsg);
        }
        return true;
    }
}

// Local DTO — used only within AuthService for deserialisation
file sealed record VerifyResetOtpResponseDto(string? ResetToken);
