using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using AcademicSystem.API.Middleware;
using AcademicSystem.Application;
using AcademicSystem.Infrastructure;
using AcademicSystem.Application.Common.Interfaces;
using AcademicSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration - simple console logger for dev. In production, configure sinks via config.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/academic-system-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

builder.Host.UseSerilog();

// Add configuration
var configuration = builder.Configuration;

// Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);
// Register application services (service implementations)
builder.Services.AddApplicationServices();

// Register dev stubs and current user service
builder.Services.AddScoped<ICurrentUserService, ApiCurrentUserService>();
builder.Services.AddScoped<IEmailService, EmailServiceStub>();
builder.Services.AddScoped<IFileStorageService, FileStorageLocal>();
builder.Services.AddHttpContextAccessor();

// Register controllers and minimal API behaviors
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

// Uniform model validation error responses
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problems = new
        {
            Message = "Validation failed",
            Errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
        };
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problems);
    };
});

// Health checks
builder.Services.AddHealthChecks();

// CORS - allow frontend origin during development. Adjust origins for production.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Authentication skeleton (JWT symmetric for dev only)
// Read JWT config: prefer production "Jwt:Key"/Issuer/Audience; fall back to development key
var jwtKey = configuration["Jwt:Key"] ?? configuration["Jwt:DevelopmentKey"] ?? "dev-key-change-me-please";
var jwtIssuer = configuration["Jwt:Issuer"];
var jwtAudience = configuration["Jwt:Audience"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
        ValidIssuer = jwtIssuer,
        ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrInstructor", policy => policy.RequireRole("Admin", "Instructor"));
});

var app = builder.Build();

app.UseSerilogRequestLogging();

// Global exception middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapControllers();

// Development-only seed endpoint to create test users (Admin, Instructor, User)
app.MapPost("/seed", async (IServiceProvider sp) =>
{
    var userRepo = sp.GetRequiredService<AcademicSystem.Application.Common.Interfaces.Repositories.IUserRepository>();
    var passwordHasher = sp.GetRequiredService<AcademicSystem.Application.Common.Interfaces.IPasswordHasher>();
    var uow = sp.GetRequiredService<AcademicSystem.Application.Common.Interfaces.IUnitOfWork>();

    var users = await userRepo.GetAllAsync();
    bool hasAdmin = System.Linq.Enumerable.Any(users, u => u.Email == "admin@local");
    if (!hasAdmin)
    {
        var admin = new AcademicSystem.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "admin@local",
            PasswordHash = passwordHasher.Hash("Admin123!"),
            Role = 1
        };
        await userRepo.AddAsync(admin);
    }

    bool hasInstructor = System.Linq.Enumerable.Any(users, u => u.Email == "instructor@local");
    if (!hasInstructor)
    {
        var inst = new AcademicSystem.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "instructor@local",
            PasswordHash = passwordHasher.Hash("Instructor123!"),
            Role = 2
        };
        await userRepo.AddAsync(inst);
    }

    bool hasUser = System.Linq.Enumerable.Any(users, u => u.Email == "user@local");
    if (!hasUser)
    {
        var user = new AcademicSystem.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = "user@local",
            PasswordHash = passwordHasher.Hash("User123!"),
            Role = 0
        };
        await userRepo.AddAsync(user);
    }

    await uow.SaveChangesAsync();
    return Results.Ok(new { seeded = true });
}).AllowAnonymous();

// Smoke test endpoint to register a student via Mediator if needed
app.MapGet("/smoke", async (IServiceProvider sp) =>
{
    try
    {
        var db = sp.GetRequiredService<AcademicSystem.Infrastructure.Persistence.ApplicationDbContext>();
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new { db = canConnect });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message);
    }
});

app.Run();

// Minimal ICurrentUserService implementation for API
public class ApiCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            if (Guid.TryParse(sub, out var gid)) return gid;
            return null;
        }
    }

    public string? UserEmail => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
