using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace Ssomero.Services;

public class TokenStorageService
{
    private readonly ILogger<TokenStorageService> _logger;

    public TokenStorageService(ILogger<TokenStorageService> logger)
    {
        _logger = logger;
    }

    public async Task StoreTokensAsync(string accessToken, string refreshToken, DateTime? expiry = null)
    {
        try
        {
            await SecureStorage.Default.SetAsync("AccessToken", accessToken);
            await SecureStorage.Default.SetAsync("RefreshToken", refreshToken);
            if (expiry.HasValue)
            {
                await SecureStorage.Default.SetAsync("TokenExpiry", expiry.Value.ToString("o"));
            }
            _logger.LogInformation("Tokens stored successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store tokens in SecureStorage");
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync("AccessToken");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read access token from SecureStorage");
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync("RefreshToken");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read refresh token from SecureStorage");
            return null;
        }
    }

    public async Task<bool> IsTokenExpiredAsync()
    {
        try
        {
            var expiry = await SecureStorage.Default.GetAsync("TokenExpiry");
            if (string.IsNullOrEmpty(expiry)) return true;
            return DateTime.Parse(expiry) <= DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check token expiry");
            return true;
        }
    }

    public Task ClearAsync()
    {
        try
        {
            SecureStorage.Default.Remove("AccessToken");
            SecureStorage.Default.Remove("RefreshToken");
            SecureStorage.Default.Remove("TokenExpiry");
            _logger.LogInformation("Tokens cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear tokens from SecureStorage");
        }
        return Task.CompletedTask;
    }
}