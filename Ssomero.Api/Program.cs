using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.InMemory;
using Hangfire.SqlServer; // ADDED: SQL Server storage support
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Ssomero.Api.BackgroundJobs;
using Ssomero.Api.Configuration;
using Ssomero.Api.Data;
using Ssomero.Api.HealthChecks;
using Ssomero.Api.Hubs;
using Ssomero.Api.Middleware;
using Ssomero.Api.Services;
using Ssomero.Api.Services.Implementations;
using Ssomero.Api.Services.Interfaces;
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ssomero-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers (when running behind a reverse proxy/load balancer)
// This must be done early so subsequent middleware (HSTS, HTTPS redirection) behave correctly.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // In production, restrict known networks or proxies for extra safety.
    // options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));
});

// ---------- Graceful Shutdown ----------
builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(30));

// ---------- Kestrel / Request Limits ----------
builder.WebHost.ConfigureKestrel(opts =>
{
    // Allow configuration of max request body size via configuration (bytes). Defaults to 10 MB.
    var max = builder.Configuration.GetValue<long?>("RequestLimits:MaxRequestBodySizeBytes") ?? 10 * 1024 * 1024;
    opts.Limits.MaxRequestBodySize = max;
});

// SAFE FIX: Removed explicit WriteTo.Console() and WriteTo.File() — appsettings.json
// already declares both sinks under "Serilog:WriteTo". Adding them here a second time
// caused every log line to be written twice (once per sink registration).
// ReadFrom.Configuration() is the single source of truth for sink configuration.
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext());

// ---------- Response Compression & HSTS ----------
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json", "application/octet-stream" });
});

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// ---------- Configuration ----------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection("Payment"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

// ---------- Database ----------
// Provider is selected by the "Database:Provider" key in configuration.
// Supported values: "sqlite" (default/dev), "sqlserver", "postgresql"
// For PostgreSQL add the Npgsql.EntityFrameworkCore.PostgreSQL package.
// For SQL Server add the Microsoft.EntityFrameworkCore.SqlServer package.
var dbProvider = builder.Configuration["Database:Provider"] ?? "sqlite";
// Treat an empty connection string as not configured so the code falls back to the
// sensible default used in development. appsettings.Production.json may contain
// an empty string for ConnectionStrings:Default which would otherwise be taken
// literally by GetConnectionString and result in an invalid/ephemeral SQLite
// target at runtime.
var configuredConnStr = builder.Configuration.GetConnectionString("Default");
var connStr = string.IsNullOrWhiteSpace(configuredConnStr)
    ? "Data Source=ssomero.db"
    : configuredConnStr;

// ---------- Startup diagnostics (temporary) ----------
// Log provider and whether a connection string was configured. Avoid logging
// the raw configuredConnStr when it may contain secrets; only log the full
// connection string when we are using the safe default SQLite file path.
Log.Information("Database Provider: {Provider}", dbProvider);
Log.Information("Connection String Configured: {Configured}", !string.IsNullOrWhiteSpace(configuredConnStr));
if (string.IsNullOrWhiteSpace(configuredConnStr))
{
    // Safe to log default SQLite connection string (no secrets)
    Log.Information("SQLite Connection String: {ConnectionString}", connStr);
}
else
{
    // Configured connection string present; do not log contents to avoid secrets.
    Log.Information("Using configured connection string from configuration or environment variables (value suppressed).");
}

builder.Services.AddDbContext<SsomeroDbContext>(opt =>
{
    // Switch database provider via "Database:Provider" config key.
    // To enable SQL Server:  add Microsoft.EntityFrameworkCore.SqlServer package.
    // To enable PostgreSQL: add Npgsql.EntityFrameworkCore.PostgreSQL package.
    switch (dbProvider.ToLowerInvariant())
    {
        // case "sqlserver":
        // case "mssql":
        //     opt.UseSqlServer(connStr);
        //     break;
        // case "postgresql":
        // case "postgres":
        //     opt.UseNpgsql(connStr);
        //     break;
        default:
            opt.UseSqlite(connStr);
            break;
    }
});

// ---------- Authentication ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ---------- Distributed Cache ----------
// Uses Redis when "ConnectionStrings:Redis" is set; falls back to in-process memory cache
// which is suitable for single-instance / dev deployments.
// To enable Redis: add the StackExchange.Redis and Microsoft.Extensions.Caching.StackExchangeRedis packages
// and set ConnectionStrings:Redis in appsettings or environment variables.
var redisConnStr = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnStr))
{
    // builder.Services.AddStackExchangeRedisCache(opt => opt.Configuration = redisConnStr);
    // Uncomment the line above and add the Redis package for multi-instance deployments.
    builder.Services.AddDistributedMemoryCache();
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// ---------- Services ----------
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<ClassService>();
builder.Services.AddHostedService<OtpCleanupService>();

// ---------- Infrastructure Services ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IApiCacheService, ApiCacheService>();
builder.Services.AddScoped<ILecturerService, LecturerService>();
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<EmailJobs>();
builder.Services.AddScoped<NotificationJobs>();

// ---------- Payment ----------
var paymentSettings = builder.Configuration.GetSection("Payment").Get<PaymentSettings>() ?? new PaymentSettings();
builder.Services.AddHttpClient("Flutterwave", client =>
{
    client.BaseAddress = new Uri(paymentSettings.BaseUrl);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {paymentSettings.SecretKey}");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentReconciliationService, PaymentReconciliationService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IClassRepService, ClassRepService>();
builder.Services.AddScoped<IClassAnnouncementService, ClassAnnouncementService>();
builder.Services.AddScoped<IClassElectionService, ClassElectionService>();
builder.Services.AddScoped<AcademicDuplicateAuditService>();

// ---------- File Storage ----------
// Uses Azure Blob Storage when AzureStorage:ConnectionString is set (production).
// Falls back to local disk storage under wwwroot/uploads/avatars/ (development/test).
if (builder.Configuration["AzureStorage:ConnectionString"] is { Length: > 0 })
    builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
else
    builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// ---------- SignalR ----------
builder.Services.AddSignalR();

// UPDATED: Use SQL Server storage when "ConnectionStrings:Hangfire" is configured;
// falls back to In-Memory so development / SQLite-only environments are unaffected.
// Add the connection string to appsettings.json (or an environment variable) to
// enable persistent job storage in production:
//   "ConnectionStrings": { "Hangfire": "Server=...;Database=HangfireDb;..." }
var hangfireConnStr = builder.Configuration.GetConnectionString("Hangfire");
builder.Services.AddHangfire(cfg =>
{
    if (!string.IsNullOrWhiteSpace(hangfireConnStr))
        cfg.UseSqlServerStorage(hangfireConnStr);
    else
        cfg.UseInMemoryStorage(); // SAFE FALLBACK for dev / SQLite environments
});
builder.Services.AddHangfireServer();

// ---------- Controllers ----------
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ---------- Health Checks ----------
builder.Services.AddScoped<CacheHealthCheck>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SsomeroDbContext>("database", tags: ["ready"])
    .AddCheck<CacheHealthCheck>("cache",
        failureStatus: HealthStatus.Degraded,
        tags: ["ready"]);

// ---------- Rate Limiting ----------
// Policies are applied per-endpoint via [EnableRateLimiting("policy-name")].
// All limits are per-IP (RemoteIpAddress). Adjust PermitLimit/Window for production.
builder.Services.AddRateLimiter(opt =>
{
    // General auth endpoints: 20 req/min (catch-all for register, refresh, etc.)
    opt.AddFixedWindowLimiter("auth", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 20;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // OTP send: 3 requests per minute — prevents OTP flooding / email abuse
    opt.AddFixedWindowLimiter("otp-send", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 3;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // OTP verify: 5 attempts per 10 minutes — aligned with DB-level attempt tracking
    opt.AddFixedWindowLimiter("otp-verify", o =>
    {
        o.Window = TimeSpan.FromMinutes(10);
        o.PermitLimit = 5;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // Login: 5 attempts per minute — brute-force protection
    opt.AddFixedWindowLimiter("auth-login", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 5;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // Forgot-password: 3 requests per minute — prevent email flooding
    opt.AddFixedWindowLimiter("pwd-forgot", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 3;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // Verify-reset-otp: 5 attempts per 10 minutes — aligned with DB-level tracking
    opt.AddFixedWindowLimiter("pwd-verify-otp", o =>
    {
        o.Window = TimeSpan.FromMinutes(10);
        o.PermitLimit = 5;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opt.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync(
            "{\"error\":\"Too many requests. Please slow down and try again later.\"}", ct);
    };
});

// ---------- CORS ----------
// REST API origins are read from AllowedOrigins config key.
// In production set this to your actual frontend domain(s).
var restAllowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "https://localhost:5173"];

builder.Services.AddCors(opt =>
{
    // REST API — restricted to configured origins
    opt.AddDefaultPolicy(policy => policy
        .WithOrigins(restAllowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader());

    // SignalR — credentials required for WebSocket upgrade
    var signalROrigins = builder.Configuration
        .GetSection("AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:5173", "https://localhost:5173"];

    opt.AddPolicy("SignalR", policy => policy
        .WithOrigins(signalROrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// ---------- Production Secret Validation ----------
// Fail fast if required secrets are absent in Production so the process never
// starts with insecure defaults.
if (!builder.Environment.IsDevelopment())
{
    var required = new[]
    {
        ("ADMIN_EMAIL",              builder.Configuration["Admin:Email"]),
        ("ADMIN_PASSWORD",           builder.Configuration["Admin:Password"]),
        ("JWT__Secret",              builder.Configuration["Jwt:Secret"]),
        ("EmailSettings__Password",  builder.Configuration["EmailSettings:Password"]),
    };

    var missing = required
        .Where(kv => string.IsNullOrWhiteSpace(kv.Item2))
        .Select(kv => kv.Item1)
        .ToList();

    if (missing.Count > 0)
    {
        // Log via bootstrap logger — the full logger is not yet built.
        Log.Fatal("FATAL: The following required environment variable(s) are not set: {Variables}. " +
                  "Set them before starting in Production.", string.Join(", ", missing));
        Environment.Exit(1);
    }
}

var app = builder.Build();

// ---------- Migrate & Seed ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SsomeroDbContext>();
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    // Startup migration & seed diagnostics (temporary) - avoid logging secrets
    Log.Information("Applying database migrations...");
    await db.Database.MigrateAsync();
    Log.Information("Database migrations completed.");
    Log.Information("Starting database seed...");
    await DbSeeder.SeedAsync(db, cfg);
    Log.Information("Database seed completed.");
}

// ---------- Middleware pipeline ----------
app.UseForwardedHeaders();
// Security headers middleware must run early to set HSTS and CSP headers
app.UseMiddleware<SecurityHeadersMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseResponseCompression();
}
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

// Prometheus HTTP metrics (built-in request counters from prometheus-net)
app.UseHttpMetrics();

// Swagger — only in Development OR when EnableSwagger config flag is true
var enableSwagger = app.Configuration.GetValue<bool>("EnableSwagger");
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// SAFE FIX: Create wwwroot at runtime if it does not exist to suppress the
// "WebRootPath was not found" warning and allow UseStaticFiles() to initialise
// correctly. This is a no-op when the folder already exists.
var webRootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
    Log.Information("Created missing wwwroot directory at {Path}", webRootPath);
}
app.UseStaticFiles(); // Serve uploaded files (e.g., /uploads/photos/*)
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<AuditRequestMiddleware>();
app.MapControllers();

// ---------- SignalR Hubs ----------
app.MapHub<NotificationHub>("/hubs/notifications").RequireCors("SignalR");
app.MapHub<ChatHub>("/hubs/chat").RequireCors("SignalR");

// ---------- Hangfire ----------
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter(app.Environment)]
});

// ---------- Recurring Jobs ----------
RecurringJob.AddOrUpdate<EmailJobs>("cleanup-otps", j => j.CleanupExpiredOtpsAsync(), Cron.Hourly);
RecurringJob.AddOrUpdate<EmailJobs>("welcome-emails", j => j.SendWelcomeEmailsAsync(), Cron.Daily);

// ---------- Health Checks ----------
// Shared JSON writer: { "status": "Healthy", "checks": { "database": "Healthy", ... } }
static Task WriteHealthResponse(HttpContext ctx, HealthReport report)
{
    ctx.Response.ContentType = "application/json";
    var result = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.ToDictionary(
            e => e.Key,
            e => e.Value.Status.ToString())
    });
    return ctx.Response.WriteAsync(result);
}

// GET /api/health — liveness (no dependencies, always fast)
app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    Predicate = _ => false,   // exclude all named checks → only reports overall status
    ResponseWriter = WriteHealthResponse
}).AllowAnonymous();

// GET /api/health/ready — readiness (database + cache)
app.MapHealthChecks("/api/health/ready", new HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
}).AllowAnonymous();

// ---------- Prometheus Metrics ----------
// GET /metrics — restricted to internal/admin in Production
app.MapMetrics("/metrics").AddEndpointFilter(async (ctx, next) =>
{
    if (!ctx.HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment())
    {
        var config = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedKey = config["Metrics:ApiKey"];

        if (!string.IsNullOrWhiteSpace(expectedKey))
        {
            if (!ctx.HttpContext.Request.Headers.TryGetValue("X-Metrics-Key", out var supplied)
                || supplied != expectedKey)
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Results.Unauthorized();
            }
        }
    }
    return await next(ctx);
});

// ---------- Graceful Shutdown Logging ----------
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
    Log.Information("Shutdown signal received. Waiting for in-flight requests to complete..."));
lifetime.ApplicationStopped.Register(() =>
    Log.Information("Application stopped."));

app.Run();

// ADDED: Non-invasive helper to mask email addresses before writing to logs.
// Usage in controllers/services: Log.Information("Login attempt for {Email}", LogHelpers.MaskEmail(email));
// Returns: a***@domain.com — preserves domain for diagnostics without exposing the full address.
static class LogHelpers
{
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "[empty]";
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0) return "***";
        return $"{email[0]}***{email[atIndex..]}";
    }
}

