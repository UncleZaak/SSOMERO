using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

/// <summary>
/// Provides read/update/password-change operations scoped to the calling user's role.
/// All queries use Select projections — EF entities are never returned to callers.
/// </summary>
public sealed class ProfileService : IProfileService
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(SsomeroDbContext db, ILogger<ProfileService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── GetProfileAsync ──────────────────────────────────────────────────────

    public Task<ProfileDto?> GetProfileAsync(Guid userId, string role, CancellationToken ct = default)
        => role switch
        {
            "Student"             => GetStudentProfileAsync(userId, ct),
            "ClassRepresentative" => GetStudentProfileAsync(userId, ct),
            "Lecturer"            => GetLecturerProfileAsync(userId, ct),
            "Admin"               => GetAdminProfileAsync(userId, ct),
            _                     => Task.FromResult<ProfileDto?>(null)
        };

    private async Task<ProfileDto?> GetStudentProfileAsync(Guid userId, CancellationToken ct)
    {
        // Attendance stats — one sub-query so we touch the Attendance table only once.
        var totalSessions = await _db.Attendances
            .CountAsync(a => a.StudentId == userId, ct);

        var presentCount = totalSessions == 0 ? 0 :
            await _db.Attendances
                .CountAsync(a => a.StudentId == userId && a.IsPresent, ct);

        double attendancePct = totalSessions == 0
            ? 0.0
            : Math.Round((double)presentCount / totalSessions * 100, 1);

        // Subscription status
        var now = DateTime.UtcNow;
        var subscriptionStatus = await _db.Subscriptions
            .Where(s => s.UserId == userId && s.IsActive && s.EndDate > now)
            .Select(s => s.Plan.ToString())
            .FirstOrDefaultAsync(ct) ?? "None";

        // Main projection
        var student = await _db.Students
            .Where(s => s.Id == userId)
            .Select(s => new StudentProfileDto
            {
                Id             = s.Id,
                FirstName      = s.FirstName,
                LastName       = s.SecondName,
                Email          = s.Email,
                PhoneNumber    = s.Phone,
                PhotoUrl       = s.Photo,
                Role           = "Student",
                UniversityName = s.University != null ? s.University.Name : null,
                // StudentId — use the string representation of the PK (no separate reg-number field)
                StudentId      = s.Id.ToString(),
                Program        = s.AcademicProfile != null ? s.AcademicProfile.Program.Name   : null,
                Department     = s.AcademicProfile != null ? s.AcademicProfile.Department.Name : null,
                Faculty        = s.AcademicProfile != null ? s.AcademicProfile.Faculty.Name   : null,
                AttendancePercentage = attendancePct,
                SubscriptionStatus   = subscriptionStatus
            })
            .FirstOrDefaultAsync(ct);

        return student;
    }

    private async Task<ProfileDto?> GetLecturerProfileAsync(Guid userId, CancellationToken ct)
    {
        var assignedClassIds = await _db.LecturerClasses
            .Where(lc => lc.LecturerId == userId)
            .Select(lc => lc.ClassId)
            .ToListAsync(ct);

        var materialsCount = await _db.ClassMaterials
            .CountAsync(m => m.UploadedBy == userId, ct);

        var sessionsManaged = await _db.ClassSessions
            .CountAsync(s => assignedClassIds.Contains(s.ClassId), ct);

        var lecturer = await _db.Lecturers
            .Where(l => l.Id == userId)
            .Select(l => new LecturerProfileDto
            {
                Id             = l.Id,
                FirstName      = l.FirstName,
                LastName       = l.LastName,
                Email          = l.Email,
                PhoneNumber    = l.Phone,
                PhotoUrl       = l.Photo,
                Role           = "Lecturer",
                UniversityName = l.University != null ? l.University.Name : null,
                StaffId                    = l.StaffId,
                AssignedClassesCount       = assignedClassIds.Count,
                MaterialsUploadedCount     = materialsCount,
                AttendanceSessionsManaged  = sessionsManaged
            })
            .FirstOrDefaultAsync(ct);

        return lecturer;
    }

    private async Task<ProfileDto?> GetAdminProfileAsync(Guid userId, CancellationToken ct)
    {
        var admin = await _db.Admins
            .Where(a => a.Id == userId)
            .Select(a => new { a.Id, a.Email })
            .FirstOrDefaultAsync(ct);

        if (admin is null) return null;

        // Admins are global — return all university names as "managed"
        var universities = await _db.Universities
            .Select(u => u.Name)
            .ToListAsync(ct);

        return new AdminProfileDto
        {
            Id                  = admin.Id,
            FirstName           = string.Empty,
            LastName            = string.Empty,
            Email               = admin.Email,
            Role                = "Admin",
            ManagedUniversities = universities,
            SystemRole          = "Admin"
        };
    }

    // ── UpdateProfileAsync ───────────────────────────────────────────────────

    public async Task<bool> UpdateProfileAsync(
        Guid userId, string role, UpdateProfileDto dto, CancellationToken ct = default)
    {
        return role switch
        {
            "Student"             => await UpdateStudentAsync(userId, dto, ct),
            "ClassRepresentative" => await UpdateStudentAsync(userId, dto, ct),
            "Lecturer"            => await UpdateLecturerAsync(userId, dto, ct),
            "Admin"               => await UpdateAdminAsync(userId, dto, ct),
            _                     => false
        };
    }

    private async Task<bool> UpdateStudentAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == userId, ct);
        if (student is null) return false;

        if (!string.IsNullOrWhiteSpace(dto.FirstName))  student.FirstName  = dto.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.LastName))   student.SecondName = dto.LastName.Trim();
        if (dto.PhoneNumber is not null)                student.Phone      = dto.PhoneNumber.Trim();
        if (dto.PhotoUrl is not null)                   student.Photo      = dto.PhotoUrl.Trim();

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Student profile updated: {UserId}", userId);
        return true;
    }

    private async Task<bool> UpdateLecturerAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct)
    {
        var lecturer = await _db.Lecturers.FirstOrDefaultAsync(l => l.Id == userId, ct);
        if (lecturer is null) return false;

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) lecturer.FirstName = dto.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.LastName))  lecturer.LastName  = dto.LastName.Trim();
        if (dto.PhoneNumber is not null)               lecturer.Phone     = dto.PhoneNumber.Trim();
        if (dto.PhotoUrl is not null)                  lecturer.Photo     = dto.PhotoUrl.Trim();

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Lecturer profile updated: {UserId}", userId);
        return true;
    }

    private async Task<bool> UpdateAdminAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct)
    {
        // Admin entity only holds email + passwordHash — nothing editable via profile update.
        // Return true if the admin exists so the controller can return 204 without error.
        var exists = await _db.Admins.AnyAsync(a => a.Id == userId, ct);
        if (!exists) return false;
        _logger.LogInformation("Admin profile update called (no mutable fields): {UserId}", userId);
        return true;
    }

    // ── UpdatePhotoUrlAsync ──────────────────────────────────────────────────

    public async Task UpdatePhotoUrlAsync(
        Guid userId, string role, string url, CancellationToken ct = default)
    {
        switch (role)
        {
            case "Student":
            case "ClassRepresentative":
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == userId, ct)
                    ?? throw new InvalidOperationException($"Student {userId} not found.");
                student.Photo = url;
                break;
            }
            case "Lecturer":
            {
                var lecturer = await _db.Lecturers.FirstOrDefaultAsync(l => l.Id == userId, ct)
                    ?? throw new InvalidOperationException($"Lecturer {userId} not found.");
                lecturer.Photo = url;
                break;
            }
            case "Admin":
                // Admin entity has no Photo column — treat as a no-op.
                _logger.LogInformation("Admin photo URL update skipped (no Photo field): {UserId}", userId);
                return;
            default:
                throw new InvalidOperationException($"Unknown role '{role}'.");
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Photo URL updated for {UserId} ({Role})", userId, role);
    }

    // ── ChangePasswordAsync ──────────────────────────────────────────────────

    public async Task<ChangePasswordResult> ChangePasswordAsync(
        Guid userId, string role, ChangePasswordDto dto, CancellationToken ct = default)
    {
        // Never log passwords — only log the userId and role.
        _logger.LogInformation("Password change requested for {UserId} ({Role})", userId, role);

        return role switch
        {
            "Student"             => await ChangeStudentPasswordAsync(userId, dto, ct),
            "ClassRepresentative" => await ChangeStudentPasswordAsync(userId, dto, ct),
            "Lecturer"            => await ChangeLecturerPasswordAsync(userId, dto, ct),
            "Admin"               => await ChangeAdminPasswordAsync(userId, dto, ct),
            _                     => ChangePasswordResult.UserNotFound
        };
    }

    private async Task<ChangePasswordResult> ChangeStudentPasswordAsync(
        Guid userId, ChangePasswordDto dto, CancellationToken ct)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == userId, ct);
        if (student is null) return ChangePasswordResult.UserNotFound;

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, student.PasswordHash))
        {
            _logger.LogWarning("Wrong current password for student {UserId}", userId);
            return ChangePasswordResult.WrongCurrentPassword;
        }

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, student.PasswordHash))
            return ChangePasswordResult.SameAsCurrentPassword;

        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Password changed successfully for student {UserId}", userId);
        return ChangePasswordResult.Success;
    }

    private async Task<ChangePasswordResult> ChangeLecturerPasswordAsync(
        Guid userId, ChangePasswordDto dto, CancellationToken ct)
    {
        var lecturer = await _db.Lecturers.FirstOrDefaultAsync(l => l.Id == userId, ct);
        if (lecturer is null) return ChangePasswordResult.UserNotFound;

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, lecturer.PasswordHash))
        {
            _logger.LogWarning("Wrong current password for lecturer {UserId}", userId);
            return ChangePasswordResult.WrongCurrentPassword;
        }

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, lecturer.PasswordHash))
            return ChangePasswordResult.SameAsCurrentPassword;

        lecturer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Password changed successfully for lecturer {UserId}", userId);
        return ChangePasswordResult.Success;
    }

    private async Task<ChangePasswordResult> ChangeAdminPasswordAsync(
        Guid userId, ChangePasswordDto dto, CancellationToken ct)
    {
        var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Id == userId, ct);
        if (admin is null) return ChangePasswordResult.UserNotFound;

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, admin.PasswordHash))
        {
            _logger.LogWarning("Wrong current password for admin {UserId}", userId);
            return ChangePasswordResult.WrongCurrentPassword;
        }

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, admin.PasswordHash))
            return ChangePasswordResult.SameAsCurrentPassword;

        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Password changed successfully for admin {UserId}", userId);
        return ChangePasswordResult.Success;
    }
}
