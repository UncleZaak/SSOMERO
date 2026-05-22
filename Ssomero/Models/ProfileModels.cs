namespace Ssomero.Models;

// ── Shared base ───────────────────────────────────────────────────────────────

public class ProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? UniversityName { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

// ── Role-specific profiles ────────────────────────────────────────────────────

public class StudentProfileDto : ProfileDto
{
    public string? StudentId { get; set; }
    public string? Program { get; set; }
    public string? Department { get; set; }
    public string? Faculty { get; set; }
    public double AttendancePercentage { get; set; }
    public string SubscriptionStatus { get; set; } = "None";
}

public class LecturerProfileDto : ProfileDto
{
    public string? StaffId { get; set; }
    public int AssignedClassesCount { get; set; }
    public int MaterialsUploadedCount { get; set; }
    public int AttendanceSessionsManaged { get; set; }
}

public class AdminProfileDto : ProfileDto
{
    public List<string> ManagedUniversities { get; set; } = [];
    public string SystemRole { get; set; } = "Admin";
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PhotoUrl { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
