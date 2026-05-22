using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Metrics;
using Ssomero.Api.Services;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly SsomeroDbContext _db;
    private readonly JwtService _jwt;
    private readonly OtpService _otp;
    private readonly ClassService _classService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuthController> _logger;
    private readonly IPasswordResetService _passwordReset;

    public AuthController(
        SsomeroDbContext db,
        JwtService jwt,
        OtpService otp,
        ClassService classService,
        IDistributedCache cache,
        ILogger<AuthController> logger,
        IPasswordResetService passwordReset)
    {
        _db = db;
        _jwt = jwt;
        _otp = otp;
        _classService = classService;
        _cache = cache;
        _logger = logger;
        _passwordReset = passwordReset;
    }

    /// <summary>POST /api/auth/send-otp  — sends a one-time password to the email.</summary>
    [HttpPost("send-otp")]
    [EnableRateLimiting("otp-send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var key = req.Email.ToLowerInvariant();
        var cooldownKey = $"otp:cooldown:{key}";

        // Enforce 60-second cooldown per email (survives restarts, works across instances)
        var cooldownValue = await _cache.GetStringAsync(cooldownKey);
        if (cooldownValue is not null
            && long.TryParse(cooldownValue, out var ticks)
            && new DateTime(ticks, DateTimeKind.Utc) > DateTime.UtcNow)
        {
            return StatusCode(429, new { error = "Please wait before requesting another OTP." });
        }

        // Set cooldown before sending so a retry cannot race in before the OTP is stored
        await _cache.SetStringAsync(
            cooldownKey,
            DateTime.UtcNow.AddSeconds(60).Ticks.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(65) });

        try
        {
            await _otp.GenerateOtpAsync(req.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate/send OTP for {Email}", key);
            // Remove cooldown so the user can retry immediately after a server-side failure
            await _cache.RemoveAsync(cooldownKey);
            return StatusCode(500, new { error = "Failed to send OTP. Please try again later." });
        }

        SsomeroMetrics.OtpSentTotal.Inc();
        return Ok(new { message = "OTP sent" });
    }

    private static readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web);

    /// <summary>POST /api/auth/verify-otp  — verifies a one-time password.</summary>
    [HttpPost("verify-otp")]
    [EnableRateLimiting("otp-verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var key = req.Email.ToLowerInvariant();
        var attemptsKey = $"otp:attempts:{key}";

        // Check attempt limit: max 5 attempts per 10-minute window
        var attemptsJson = await _cache.GetStringAsync(attemptsKey);
        if (attemptsJson is not null)
        {
            var state = JsonSerializer.Deserialize<OtpAttemptState>(attemptsJson, _jsonOpts);
            if (state is not null && state.Window > DateTime.UtcNow && state.Count >= 5)
                return StatusCode(429, new { error = "Too many attempts. Please request a new OTP." });
        }

        var verificationToken = await _otp.VerifyOtpAsync(req.Email, req.OtpCode);
        if (verificationToken is null)
        {
            // Increment attempt counter
            OtpAttemptState newState;
            if (attemptsJson is not null)
            {
                var existing = JsonSerializer.Deserialize<OtpAttemptState>(attemptsJson, _jsonOpts)!;
                newState = existing.Window > DateTime.UtcNow
                    ? existing with { Count = existing.Count + 1 }
                    : new OtpAttemptState(1, DateTime.UtcNow.AddMinutes(10));
            }
            else
            {
                newState = new OtpAttemptState(1, DateTime.UtcNow.AddMinutes(10));
            }

            await _cache.SetStringAsync(
                attemptsKey,
                JsonSerializer.Serialize(newState, _jsonOpts),
                new DistributedCacheEntryOptions { AbsoluteExpiration = newState.Window });

            SsomeroMetrics.OtpVerifiedTotal.WithLabels("failure").Inc();
            return BadRequest(new { error = "Invalid or expired OTP" });
        }

        // Success — clear attempt counter
        await _cache.RemoveAsync(attemptsKey);
        SsomeroMetrics.OtpVerifiedTotal.WithLabels("success").Inc();
        return Ok(new { message = "OTP verified", verificationToken });
    }

    private sealed record OtpAttemptState(int Count, DateTime Window);

    /// <summary>POST /api/auth/register  — student registration (requires verified OTP).</summary>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegisterRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        // Server-side OTP gate: validate the verification token
        if (string.IsNullOrWhiteSpace(req.VerificationToken)
            || !await _otp.ValidateVerificationTokenAsync(req.Email, req.VerificationToken))
        {
            return BadRequest(new { error = "Email not verified. Please complete OTP verification first." });
        }

        var normalizedEmail = req.Email.ToLowerInvariant();

        // Check duplicate email across ALL user types, ignoring soft-deleted records so a
        // formerly deleted user can re-register with the same address.
        if (await _db.Students.IgnoreQueryFilters().AnyAsync(s => s.Email == normalizedEmail && !s.IsDeleted)
            || await _db.Lecturers.IgnoreQueryFilters().AnyAsync(l => l.Email == normalizedEmail && !l.IsDeleted)
            || await _db.Admins.IgnoreQueryFilters().AnyAsync(a => a.Email == normalizedEmail && !a.IsDeleted))
        {
            return Conflict(new { error = "Email already registered" });
        }

        // Validate academic hierarchy
        var university = await _db.Universities.FindAsync(req.UniversityId);
        if (university is null) return BadRequest(new { error = "Invalid university" });

        var faculty = await _db.Faculties.FirstOrDefaultAsync(f => f.Id == req.FacultyId && f.UniversityId == req.UniversityId);
        if (faculty is null) return BadRequest(new { error = "Faculty does not belong to the selected university" });

        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == req.DepartmentId && d.FacultyId == req.FacultyId);
        if (dept is null) return BadRequest(new { error = "Department does not belong to the selected faculty" });

        var prog = await _db.Programs.FirstOrDefaultAsync(p => p.Id == req.ProgramId && p.DepartmentId == req.DepartmentId);
        if (prog is null) return BadRequest(new { error = "Program does not belong to the selected department" });

        // Wrap student creation, profile creation and class enrollment in a single transaction
        // so a failure in any step leaves no orphaned records.
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var student = new Student
            {
                Id = Guid.NewGuid(),
                FirstName = req.FirstName,
                SecondName = req.SecondName,
                OtherNames = req.OtherNames,
                Dob = req.Dob,
                Gender = req.Gender,
                Phone = req.Phone,
                Email = normalizedEmail,
                Photo = req.Photo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                IsVerified = true, // OTP was verified before registration
                CreatedAt = DateTime.UtcNow
            };
            _db.Students.Add(student);

            var profile = new AcademicProfile
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                UniversityId = req.UniversityId,
                FacultyId = req.FacultyId,
                DepartmentId = req.DepartmentId,
                ProgramId = req.ProgramId,
                EntrySchemeId = req.EntrySchemeId,
                IntakeId = req.IntakeId,
                StudyModeId = req.StudyModeId,
                AcademicYearId = req.AcademicYearId,
                YearOfStudy = req.YearOfStudy,
                SemesterId = req.SemesterId
            };
            _db.AcademicProfiles.Add(profile);
            await _db.SaveChangesAsync();

            // Auto-enroll into main class + subclasses from curriculum
            await _classService.EnrollStudentAsync(student.Id, req.ProgramId, req.YearOfStudy, req.SemesterId, req.AcademicYearId);

            await tx.CommitAsync();
            SsomeroMetrics.RegistrationsTotal.WithLabels("Student", "success").Inc();
            _logger.LogInformation("Student registered: {Email}", student.Email);
            return Created("", new { id = student.Id, email = student.Email });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            SsomeroMetrics.RegistrationsTotal.WithLabels("Student", "failure").Inc();
            _logger.LogError(ex, "Student registration failed and was rolled back for {Email}", normalizedEmail);
            return StatusCode(500, new { error = "Registration failed due to a server error. Please try again." });
        }
    }

    /// <summary>POST /api/auth/lecturer/register  — lecturer self-registration (requires verified OTP).</summary>
    [HttpPost("lecturer/register")]
    public async Task<IActionResult> RegisterLecturer([FromBody] LecturerRegisterRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        // OTP gate — same flow as student: send-otp ? verify-otp ? register
        if (string.IsNullOrWhiteSpace(req.VerificationToken)
            || !await _otp.ValidateVerificationTokenAsync(req.Email, req.VerificationToken))
        {
            return BadRequest(new { error = "Email not verified. Please complete OTP verification first." });
        }

        var normalizedEmail = req.Email.ToLowerInvariant();

        // Check duplicate email across ALL user types, ignoring soft-deleted records.
        if (await _db.Lecturers.IgnoreQueryFilters().AnyAsync(l => l.Email == normalizedEmail && !l.IsDeleted)
            || await _db.Students.IgnoreQueryFilters().AnyAsync(s => s.Email == normalizedEmail && !s.IsDeleted)
            || await _db.Admins.IgnoreQueryFilters().AnyAsync(a => a.Email == normalizedEmail && !a.IsDeleted))
        {
            return Conflict(new { error = "Email already registered" });
        }

        var lecturer = new Lecturer
        {
            Id = Guid.NewGuid(),
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = normalizedEmail,
            Phone = req.Phone,
            Photo = req.Photo,
            StaffId = req.StaffId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            IsVerified = true,
            IsApproved = false, // Admin must approve before login is permitted
            CreatedAt = DateTime.UtcNow
        };
        _db.Lecturers.Add(lecturer);
        await _db.SaveChangesAsync();

        SsomeroMetrics.RegistrationsTotal.WithLabels("Lecturer", "success").Inc();
        _logger.LogInformation("Lecturer registered (pending approval): {Email}", lecturer.Email);
        return Created("", new { id = lecturer.Id, message = "Registered. Awaiting admin approval." });
    }

    /// <summary>POST /api/auth/login  — authenticates student or lecturer and returns JWT.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        var email = req.Email.ToLowerInvariant();
        _logger.LogInformation("Login attempt for {Email}", email);

        // ?? 1. Try admin (bypass query filter so soft-deleted admins are detected) ??
        var admin = await _db.Admins.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Email == email);
        if (admin is not null)
        {
            if (admin.IsDeleted)
            {
                _logger.LogWarning("Admin login blocked — deleted account: {Email}", email);
                return BadRequest(new { error = "Admin account has been deleted" });
            }

            if (admin.Status != UserStatus.Active)
            {
                _logger.LogWarning("Admin login blocked — account not active ({Status}): {Email}", admin.Status, email);
                return BadRequest(new { error = "Admin account is not active" });
            }

            if (!BCrypt.Net.BCrypt.Verify(req.Password, admin.PasswordHash))
                return Unauthorized(new { error = "Invalid credentials" });

            SsomeroMetrics.LoginsTotal.WithLabels("Admin", "success").Inc();
            _logger.LogInformation("Admin login successful for {Email}", email);
            // Use email as display name for admin if no explicit name available
            return Ok(BuildAuthResponse(admin.Id, admin.Email, "Admin", admin.Email, null));
        }

        // ?? 2. Try student (bypass query filter so soft-deleted students are detected) ??
        var student = await _db.Students.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Email == email);
        if (student is not null)
        {
            _logger.LogDebug("Found student record for {Email}, checking state", email);

            if (student.IsDeleted)
            {
                _logger.LogWarning("Student login blocked — deleted account: {Email}", email);
                return BadRequest(new { error = "Account has been deleted" });
            }

            if (student.Status == UserStatus.Suspended)
            {
                _logger.LogWarning("Student login blocked — suspended: {Email}", email);
                return BadRequest(new { error = "Account is suspended" });
            }

            if (student.Status == UserStatus.Deactivated)
            {
                _logger.LogWarning("Student login blocked — deactivated: {Email}", email);
                return BadRequest(new { error = "Account is deactivated" });
            }

            if (!student.IsVerified)
            {
                _logger.LogWarning("Student login blocked — email not verified: {Email}", email);
                return BadRequest(new { error = "Email not verified" });
            }

            if (!BCrypt.Net.BCrypt.Verify(req.Password, student.PasswordHash))
                return Unauthorized(new { error = "Invalid credentials" });

            // Determine role: ClassRepresentative if student has an active class_rep membership on a main class
            var isClassRep = await _db.StudentClasses
                .AnyAsync(sc => sc.StudentId == student.Id
                             && sc.Role == "class_rep"
                             && sc.Status == "active"
                             && sc.Class.ParentClassId == null);

            var studentRole = isClassRep ? "ClassRepresentative" : "Student";
            SsomeroMetrics.LoginsTotal.WithLabels(studentRole, "success").Inc();
            var studentName = $"{student.FirstName} {student.SecondName}".Trim();
            _logger.LogInformation("Student login successful for {Email} as {Role}", email, studentRole);
            return Ok(BuildAuthResponse(student.Id, student.Email, studentRole, studentName, student.UniversityId));
        }

        // ?? 3. Try lecturer (bypass query filter so soft-deleted lecturers are detected) ??
        var lecturer = await _db.Lecturers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Email == email);
        if (lecturer is not null)
        {
            _logger.LogDebug("Found lecturer record for {Email}, checking state", email);

            if (lecturer.IsDeleted)
            {
                _logger.LogWarning("Lecturer login blocked — deleted account: {Email}", email);
                return BadRequest(new { error = "Account has been deleted" });
            }

            if (lecturer.Status == UserStatus.Suspended)
            {
                _logger.LogWarning("Lecturer login blocked — suspended: {Email}", email);
                return BadRequest(new { error = "Account is suspended" });
            }

            if (lecturer.Status == UserStatus.Deactivated)
            {
                _logger.LogWarning("Lecturer login blocked — deactivated: {Email}", email);
                return BadRequest(new { error = "Account is deactivated" });
            }

            if (!lecturer.IsVerified)
            {
                _logger.LogWarning("Lecturer login blocked — email not verified: {Email}", email);
                return BadRequest(new { error = "Email not verified" });
            }

            if (!lecturer.IsApproved)
            {
                _logger.LogWarning("Lecturer login blocked — not approved: {Email}", email);
                return StatusCode(403, new { error = "Account pending admin approval" });
            }

            if (!BCrypt.Net.BCrypt.Verify(req.Password, lecturer.PasswordHash))
                return Unauthorized(new { error = "Invalid credentials" });

            SsomeroMetrics.LoginsTotal.WithLabels("Lecturer", "success").Inc();
            var lecturerName = $"{lecturer.FirstName} {lecturer.LastName}".Trim();
            _logger.LogInformation("Lecturer login successful for {Email}", email);
            return Ok(BuildAuthResponse(lecturer.Id, lecturer.Email, "Lecturer", lecturerName, lecturer.UniversityId));
        }

        SsomeroMetrics.LoginsTotal.WithLabels("unknown", "failure").Inc();
        _logger.LogWarning("Login failed: no account found for {Email}", email);
        return Unauthorized(new { error = "Invalid credentials" });
    }

    /// <summary>POST /api/auth/refresh  — refreshes an expired access token.</summary>
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest req)
    {
        // TODO: In production, store refresh tokens in DB and validate.
        // Return 501 so clients know this isn't functional yet and don't
        // mistake the response for a valid token refresh.
        return StatusCode(501, new { error = "Token refresh is not yet implemented. Please log in again." });
    }

    // ?? Password Reset ??????????????????????????????????????????????????????????

    /// <summary>
    /// POST /api/auth/forgot-password
    /// Always returns the same generic response regardless of whether the email exists.
    /// </summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("pwd-forgot")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        try
        {
            await _passwordReset.SendResetOtpAsync(req.Email, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing forgot-password for {Email}",
                req.Email.ToLowerInvariant());
            // Intentionally fall through — generic response must be returned regardless.
        }

        return Ok(new { message = "If the account exists, a reset code has been sent." });
    }

    /// <summary>
    /// POST /api/auth/verify-reset-otp
    /// Returns the plaintext reset token on success; 422 on failure.
    /// </summary>
    [HttpPost("verify-reset-otp")]
    [EnableRateLimiting("pwd-verify-otp")]
    public async Task<IActionResult> VerifyResetOtp(
        [FromBody] VerifyResetOtpDto req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var resetToken = await _passwordReset.VerifyResetOtpAsync(req.Email, req.OtpCode, ct);
        if (resetToken is null)
            return UnprocessableEntity(new { error = "Invalid, expired, or already-used OTP." });

        return Ok(new { resetToken });
    }

    /// <summary>
    /// POST /api/auth/reset-password
    /// Resets the password using the token issued by verify-reset-otp.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDto req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var success = await _passwordReset.ResetPasswordAsync(
            req.Email, req.ResetToken, req.NewPassword, ct);

        if (!success)
            return UnprocessableEntity(new { error = "Invalid or expired reset token." });

        return Ok(new { message = "Password reset successful." });
    }

    private AuthResponse BuildAuthResponse(Guid userId, string email, string role, string fullName, Guid? universityId = null)
    {
        var accessToken = _jwt.GenerateAccessToken(userId, email, role, universityId);
        var refreshToken = _jwt.GenerateRefreshToken();
        var expiresAt = _jwt.GetAccessTokenExpiry();
        return new AuthResponse(accessToken, refreshToken, expiresAt, new AuthUser(userId.ToString(), email, role, fullName));
    }
}
