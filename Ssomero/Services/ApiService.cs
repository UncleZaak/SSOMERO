using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Configuration;
using Microsoft.Extensions.Http;
#if DEBUG
using Ssomero.Services;
#endif

namespace Ssomero.Services;

public class ApiService : IApiService
{
    private readonly System.Net.Http.IHttpClientFactory _clientFactory;
    private readonly TokenStorageService _tokenStorage;
    private readonly ILogger<ApiService> _logger;
    private readonly ApiSettings _apiSettings;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    // Auth endpoints return 401 for invalid credentials, NOT expired tokens.
    // They must never trigger the automatic token-refresh-and-retry flow.
    private static readonly string[] AuthPaths = ["auth/login", "auth/register", "auth/send-otp", "auth/verify-otp", "auth/refresh"];

    public ApiService(System.Net.Http.IHttpClientFactory clientFactory, TokenStorageService tokenStorage, ILogger<ApiService> logger, ApiSettings apiSettings)
    {
        _clientFactory = clientFactory;
        _tokenStorage = tokenStorage;
        _logger = logger;
        _apiSettings = apiSettings;
    }

    // Backwards-compatible constructor used by unit tests or callers that do not supply ApiSettings.
    // Uses default ApiSettings instance so existing tests that new-up ApiService continue to compile.
    // Made internal so the DI container does not see multiple public constructors and cause
    // an AmbiguousConstructorException at startup. Unit tests can still access this because
    // the assembly grants InternalsVisibleTo Ssomero.UnitTests.
    internal ApiService(System.Net.Http.IHttpClientFactory clientFactory, TokenStorageService tokenStorage, ILogger<ApiService> logger)
        : this(clientFactory, tokenStorage, logger, new ApiSettings())
    {
    }

    // Backwards-compatible constructors that accept a HttpClient instance (used by many unit tests).
    private sealed class SimpleHttpClientFactory : System.Net.Http.IHttpClientFactory
    {
        private readonly System.Net.Http.HttpClient _client;
        public SimpleHttpClientFactory(System.Net.Http.HttpClient client) => _client = client;
        public System.Net.Http.HttpClient CreateClient(string name) => _client;
    }

    // Constructors that accept an HttpClient are provided for tests/helpers that create
    // an HttpClient directly. Make these internal to avoid DI ambiguity in production.
    internal ApiService(System.Net.Http.HttpClient client, TokenStorageService tokenStorage, ILogger<ApiService> logger, ApiSettings apiSettings)
        : this(new SimpleHttpClientFactory(client), tokenStorage, logger, apiSettings)
    {
    }

    internal ApiService(System.Net.Http.HttpClient client, TokenStorageService tokenStorage, ILogger<ApiService> logger)
        : this(new SimpleHttpClientFactory(client), tokenStorage, logger, new ApiSettings())
    {
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string path, HttpContent? content = null)
    {
        // Create a fresh request with absolute URI (do not rely on shared HttpClient.BaseAddress being mutated)
        var baseUrl = _apiSettings.BaseUrl ?? string.Empty;
#if DEBUG
        try
        {
            // If developer override exists, prefer it (Preferences-based service)
            var overrideUrl = Microsoft.Maui.Storage.Preferences.Get("dev:BaseUrl", null);
            if (!string.IsNullOrWhiteSpace(overrideUrl)) baseUrl = overrideUrl;
        }
        catch
        {
            // ignore and use configured base
        }
#endif

        var absolute = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : (baseUrl.TrimEnd('/') + "/" + path.TrimStart('/'));

        var request = new HttpRequestMessage(method, new Uri(absolute));
        if (content is not null)
        {
            content.Headers.ContentType ??= new MediaTypeHeaderValue("application/json");
            request.Content = content;
        }
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return request;
    }

    private async Task<HttpResponseMessage> SendWithRefreshAsync(HttpMethod method, string path, HttpContent? content = null, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(method, path, content);
        HttpResponseMessage response;
        var apiClient = _clientFactory.CreateClient("ApiClient");
        try
        {
            response = await apiClient.SendAsync(request, ct);
        }
        catch (TaskCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Request to {Path} was cancelled", path);
            throw;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // HttpClient.Timeout expired — not a caller cancellation
            _logger.LogError(ex, "Request to {Path} timed out after HttpClient.Timeout elapsed", path);
            throw new HttpRequestException(
                "The request timed out. Check your connection and ensure the API is reachable.",
                ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error requesting {Method} {Path}", method, path);
            throw new HttpRequestException(
                "Unable to connect to the Ssomero API. Ensure the backend is running and the configured API base URL is reachable.",
                ex,
                ex.StatusCode);
        }

        // Auth endpoints legitimately return 401 for wrong credentials.
        // Only attempt token refresh for protected (non-auth) endpoints.
        if (response.StatusCode != HttpStatusCode.Unauthorized
            || Array.Exists(AuthPaths, p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return response;

        _logger.LogWarning("Received 401 for {Path}, attempting token refresh", path);
        var refreshed = await TryRefreshAsync(ct);
        if (!refreshed)
        {
            // Token refresh failed — force re-login
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _tokenStorage.ClearAsync();
                await Shell.Current.GoToAsync("//LoginPage");
            });
            return response;
        }

        // Retry original request with new token
        _logger.LogInformation("Token refreshed, retrying {Method} {Path}", method, path);
        var retryContent = await CloneContentAsync(content);
        var retry = await CreateRequestAsync(method, path, retryContent);
        return await apiClient.SendAsync(retry, ct);
    }

    private static async Task<HttpContent?> CloneContentAsync(HttpContent? original)
    {
        if (original is null) return null;
        var bytes = await original.ReadAsByteArrayAsync();
        var clone = new ByteArrayContent(bytes);
        if (original.Headers.ContentType is not null)
            clone.Headers.ContentType = original.Headers.ContentType;
        return clone;
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct = default)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            // Double-check: maybe another thread already refreshed
            if (!await _tokenStorage.IsTokenExpiredAsync())
                return true;

            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var payload = JsonContent.Create(new { refreshToken });
            var request = new HttpRequestMessage(HttpMethod.Post, "auth/refresh") { Content = payload };
            var client = _clientFactory.CreateClient("ApiClient");
            var resp = await client.SendAsync(request, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Refresh endpoint returned {StatusCode}", resp.StatusCode);
                return false;
            }

            var dto = await resp.Content.ReadFromJsonAsync<AuthResponseDto>(ct);
            if (dto is null || string.IsNullOrEmpty(dto.AccessToken)) return false;

            await _tokenStorage.StoreTokensAsync(dto.AccessToken, dto.RefreshToken, dto.ExpiresAt);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Token refresh failed unexpectedly");
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task<bool> CheckHealthAsync(CancellationToken ct = default)
    {
        try
        {
            // Use a short timeout so the user isn't left waiting when the backend is down.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            var client = _clientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("health", timeoutCts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed");
            return false;
        }
    }

    public Task<HttpResponseMessage> GetAsync(string path, CancellationToken ct = default)
        => SendWithRefreshAsync(HttpMethod.Get, path, ct: ct);

    public Task<HttpResponseMessage> PostAsync(string path, HttpContent content, CancellationToken ct = default)
        => SendWithRefreshAsync(HttpMethod.Post, path, content, ct);

    public Task<HttpResponseMessage> PutAsync(string path, HttpContent content, CancellationToken ct = default)
        => SendWithRefreshAsync(HttpMethod.Put, path, content, ct);

    public Task<HttpResponseMessage> DeleteAsync(string path, CancellationToken ct = default)
        => SendWithRefreshAsync(HttpMethod.Delete, path, ct: ct);
}