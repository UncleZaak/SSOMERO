using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Dtos;

// ---------- Auth ----------
public record SendOtpRequest([Required, EmailAddress] string Email);

public record VerifyOtpRequest([Required, EmailAddress] string Email, [Required] string OtpCode);

public record StudentRegisterRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string SecondName,
    [MaxLength(200)] string? OtherNames,
    [Required] DateOnly Dob,
    [Required, MaxLength(20)] string Gender,
    [Required, MaxLength(20)] string Phone,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8),
     RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
         ErrorMessage = "Password must be at least 8 characters and contain an uppercase letter, a number and a special character.")]
    string Password,
    string? Photo,
    // OTP verification proof
    [Required] string VerificationToken,
    // Academic profile
    [Required] Guid UniversityId,
    [Required] Guid FacultyId,
    [Required] Guid DepartmentId,
    [Required] Guid ProgramId,
    [Required] Guid EntrySchemeId,
    [Required] Guid IntakeId,
    [Required] Guid StudyModeId,
    [Required] Guid AcademicYearId,
    [Required] int YearOfStudy,
    [Required] Guid SemesterId
);

public record LecturerRegisterRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MaxLength(20)] string Phone,
    [Required, MinLength(8),
     RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
         ErrorMessage = "Password must be at least 8 characters and contain an uppercase letter, a number and a special character.")]
    string Password,
    string? Photo,
    string? StaffId,
    // OTP verification proof — required, same flow as student registration
    [Required] string VerificationToken
);

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

public record RefreshRequest([Required] string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, AuthUser User);

public record AuthUser(string Id, string Email, string Role, string FullName);

// ---------- Payments ----------

public record InitiatePaymentRequest(
    [Required] string Plan,
    [Required, MaxLength(20)] string PhoneNumber
);

public record VerifyPaymentRequest([Required] string TxRef);

/// <summary>
/// Webhook payload sent by the payment provider (Flutterwave / mock).
/// Only the fields needed for server-side verification are mapped here.
/// </summary>
public record WebhookPayload(string Event, string Status, string TxRef);

public record PaymentResponse(
    Guid Id,
    string Plan,
    decimal Amount,
    string Currency,
    string Status,
    string ExternalReference,
    DateTime CreatedAt,
    DateTime? VerifiedAt,
    string? Provider = null,
    string? PhoneNumber = null,
    string? FailureReason = null,
    string? ReceiptUrl = null
);

public record PaymentHistoryItemResponse(
    Guid Id,
    string Plan,
    decimal Amount,
    string Currency,
    string Status,
    string ExternalReference,
    string? Provider,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime? VerifiedAt,
    string? FailureReason,
    string? ReceiptUrl
);

public record SubscriptionResponse(
    Guid Id,
    string Plan,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);

public record CurrentPlanResponse(
    SubscriptionResponse? Subscription,
    PaymentResponse? LatestPayment
);

public record ReconcileResponse(int Recovered, int StillPending, int Total);
// ---------- Password Reset ----------
public record ForgotPasswordRequestDto([Required, EmailAddress] string Email);

public record VerifyResetOtpDto(
    [Required, EmailAddress] string Email,
    [Required] string OtpCode);

public record ResetPasswordDto(
    [Required, EmailAddress] string Email,
    [Required] string ResetToken,
    [Required, MinLength(8),
     RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
         ErrorMessage = "Password must be at least 8 characters and contain an uppercase letter, a lowercase letter, a number and a special character.")]
    string NewPassword);

// ---------- Lecturer admin ----------
public record AssignLecturerRequest([Required] Guid LecturerId, [Required] Guid ClassId);

// ---------- Lookup item ----------
public record LookupDto(Guid Id, string Name);

// ---------- University CRUD ----------
public record UniversityDetailDto(Guid Id, string Name, int FacultiesCount, string Status);

// ---------- Faculty CRUD ----------
public record FacultyDetailDto(Guid Id, string Name, Guid UniversityId, string UniversityName, int DepartmentsCount, string Status);

// ---------- Class view ----------
public record ClassDto(Guid Id, string Name, string? CourseCode, Guid? ParentClassId, int EnrolledStudents, string? LecturerName);

// ---------- Curriculum view ----------
public record CurriculumDto(Guid Id, string CourseCode, string CourseName, int YearOfStudy, string Semester);

// ---------- Dashboard ----------
public record DashboardResponse(
    int ActiveCourses,
    int UpcomingAssignments,
    double AttendancePercent,
    IEnumerable<AnnouncementResponse> RecentAnnouncements,
    IEnumerable<ClassDto>? MyClasses = null,
    IEnumerable<ClassDto>? TeachingClasses = null,
    IEnumerable<ClassDto>? ManagedClasses = null,
    int? TotalStudents = null,
    int? TotalLecturers = null,
    int? TotalPrograms = null
);

public record AnnouncementResponse(string Title, string Body, DateTime Date);

// ---------- Admin academic CRUD ----------
public record CreateUniversityRequest([Required, MaxLength(300)] string Name);
public record UpdateUniversityRequest([Required, MaxLength(300)] string Name);

// ---------- Department DTO ----------
public record DepartmentDto(
    Guid Id, string Name,
    Guid FacultyId, string FacultyName,
    Guid UniversityId, string UniversityName);

// ---------- Program DTO ----------
public record ProgramDto(
    Guid Id, string Name, int DurationSemesters,
    Guid DepartmentId, string DepartmentName,
    Guid FacultyId, string FacultyName,
    Guid UniversityId, string UniversityName);

// ---------- Curriculum admin DTO ----------
public record CurriculumAdminDto(
    Guid Id, string CourseCode, string CourseName, int YearOfStudy,
    Guid ProgramId, string ProgramName,
    string DepartmentName,
    string FacultyName,
    string UniversityName);

public record CreateFacultyRequest([Required, MaxLength(300)] string Name, [Required] Guid UniversityId);
public record UpdateFacultyRequest([Required, MaxLength(300)] string Name, [Required] Guid UniversityId);

public record CreateDepartmentRequest([Required, MaxLength(300)] string Name, [Required] Guid FacultyId);
public record UpdateDepartmentRequest([Required, MaxLength(300)] string Name, [Required] Guid FacultyId);

public record CreateProgramRequest([Required, MaxLength(300)] string Name, [Required] Guid DepartmentId, [Required, Range(1, 20)] int DurationSemesters);
public record UpdateProgramRequest([Required, MaxLength(300)] string Name, [Required] Guid DepartmentId, [Required, Range(1, 20)] int DurationSemesters);

public record CreateCurriculumRequest([Required] Guid ProgramId, [Required, Range(1, 10)] int YearOfStudy, [Required] Guid SemesterId, [Required, MaxLength(50)] string CourseCode, [Required, MaxLength(300)] string CourseName);
public record UpdateCurriculumRequest([Required] Guid ProgramId, [Required, Range(1, 10)] int YearOfStudy, [Required] Guid SemesterId, [Required, MaxLength(50)] string CourseCode, [Required, MaxLength(300)] string CourseName);

// ---------- Profile ----------

/// <summary>Base profile fields common to all roles.</summary>
public record ProfileDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? PhotoUrl { get; init; }
    public string Role { get; init; } = string.Empty;
    public string? UniversityName { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

/// <summary>Student-specific profile data.</summary>
public record StudentProfileDto : ProfileDto
{
    public string? StudentId { get; init; }
    public string? Program { get; init; }
    public string? Department { get; init; }
    public string? Faculty { get; init; }
    public double AttendancePercentage { get; init; }
    public string SubscriptionStatus { get; init; } = "None";
}

/// <summary>Lecturer-specific profile data.</summary>
public record LecturerProfileDto : ProfileDto
{
    public string? StaffId { get; init; }
    public int AssignedClassesCount { get; init; }
    public int MaterialsUploadedCount { get; init; }
    public int AttendanceSessionsManaged { get; init; }
}

/// <summary>Admin-specific profile data.</summary>
public record AdminProfileDto : ProfileDto
{
    public IReadOnlyList<string> ManagedUniversities { get; init; } = [];
    public string SystemRole { get; init; } = "Admin";
}

/// <summary>Fields a user may update on their own profile.</summary>
public record UpdateProfileDto(
    [MaxLength(100)] string? FirstName,
    [MaxLength(100)] string? LastName,
    [MaxLength(20)] string? PhoneNumber,
    [Url, MaxLength(500)] string? PhotoUrl);

/// <summary>Request to change the authenticated user's password.</summary>
public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required, MinLength(8),
     RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
         ErrorMessage = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number and a special character.")]
    string NewPassword);

// ---------- Schedule ----------
public record ClassSessionResponse(
    Guid SessionId,
    Guid ClassId,
    string CourseName,
    string? CourseCode,
    DateTime StartTime,
    DateTime EndTime,
    string? Location,
    string? Lecturer
);

public record StudentScheduleResponse(IEnumerable<ClassSessionResponse> Sessions);

// ---------- Attendance ----------
public record MarkAttendanceRequest(
    [Required] Guid SessionId,
    [Required] DateTime Timestamp,
    double? Latitude,
    double? Longitude,
    string? SelfieBase64
);

public record AttendanceRecordResponse(
    Guid Id,
    Guid ClassId,
    Guid? SessionId,
    string CourseName,
    DateTime Date,
    bool IsPresent,
    DateTime? SubmittedAt
);

public record StudentAttendanceReport(
    double OverallPercent,
    IEnumerable<CourseAttendanceStat> CourseStats
);

public record CourseAttendanceStat(
    Guid ClassId,
    string CourseName,
    int TotalSessions,
    int AttendedSessions,
    double Percent,
    /// <summary>Average attendance percentage across all enrolled students in this class.</summary>
    double ClassAvgPercent
);

// ---------- Lecturer service ----------

public record LecturerClassDto(
    Guid Id,
    string Name,
    string? CourseCode,
    int EnrolledStudents,
    int TotalSessions
);

public record SessionSummaryDto(
    Guid Id,
    string DayOfWeek,
    string StartTime,
    string EndTime,
    string? Location,
    bool IsActive
);

public record LecturerClassDetailDto(
    Guid Id,
    string Name,
    string? CourseCode,
    int EnrolledStudents,
    IEnumerable<SessionSummaryDto> Sessions
);

public record LecturerStudentDto(
    Guid Id,
    string FullName,
    string Email,
    string Status
);

public record SessionAttendanceDto(
    Guid AttendanceId,
    Guid StudentId,
    string StudentName,
    bool IsPresent,
    DateTime? SubmittedAt
);

public record LecturerMarkAttendanceDto(
    [Required] Guid SessionId,
    [Required] Guid StudentId,
    [Required] bool IsPresent,
    string? Notes
);

public record ClassMaterialDto(
    Guid Id,
    string Title,
    string? FileUrl,
    DateTime CreatedAt
);

public record UploadMaterialDto(
    [Required] Guid ClassId,
    [Required, MaxLength(300)] string Title,
    [MaxLength(1000)] string? FileUrl
);

// ---------- Profile photo upload ----------

/// <summary>Response returned after a successful profile photo upload.</summary>
public sealed class UploadPhotoResponse
{
    public required string PhotoUrl { get; init; }
}

// ---------- Class Representative ----------

public record ClassRepMyClassDto(
    Guid Id,
    string Name,
    string ProgramName,
    int StudentCount,
    int SubclassCount,
    int LecturerCount
);

public record ClassRepSubclassDto(
    Guid Id,
    string Name,
    string? Description,
    int StudentCount,
    int LecturerCount,
    DateTime CreatedAt
);

public record CreateSubclassDto(
    [Required, MaxLength(300)] string Name,
    [MaxLength(1000)] string? Description
);

public record RenameSubclassDto(
    [Required, MaxLength(300)] string Name
);

public record ClassRepStudentDto(
    Guid Id,
    string FullName,
    string Email
);

public record ClassRepLecturerDto(
    Guid Id,
    string? StaffId,
    string FullName,
    string Email
);

public record AssignLecturerDto(
    [Required] Guid LecturerId
);

public record ClassRepAttendanceSummaryDto(
    double AverageAttendanceRate,
    int TotalSessions,
    int TotalAttendances
);

public record ClassRepStatsDto(
    int ManagedClasses,
    int TotalStudents,
    int TotalSubclasses,
    int AssignedLecturers,
    double AverageAttendanceRate
);

// ---------- Class Announcements ----------

public record ClassAnnouncementDto(
    Guid Id,
    Guid ClassId,
    string ClassName,
    Guid CreatedBy,
    string Title,
    string Message,
    DateTime CreatedAt
);

public record CreateClassAnnouncementDto(
    [Required] Guid ClassId,
    [Required, MaxLength(300)] string Title,
    [Required, MaxLength(4000)] string Message
);

// ---------- Class Rep Analytics ----------

public record TrendPointDto(string Label, double Value);

public record ClassRepAnalyticsDto(
    int TotalStudents,
    int TotalSubclasses,
    int AssignedLecturers,
    double AverageAttendanceRate,
    IReadOnlyList<TrendPointDto> AttendanceTrend,
    IReadOnlyList<TrendPointDto> StudentGrowthTrend
);

// ---------- Class Elections ----------

public record StartElectionRequestDto([Required] Guid ClassId);

public record VoteRequestDto([Required] Guid CandidateStudentId);

public record ElectionCandidateDto(
    Guid StudentId,
    string FullName,
    string StudentNumber,
    int VoteCount,
    bool IsCurrentUser
);

public record ClassElectionDto(
    Guid Id,
    Guid ClassId,
    string ClassName,
    DateTime StartedAt,
    DateTime EndsAt,
    string Status,
    int SecondsRemaining,
    bool CanVote,
    bool HasVoted,
    Guid? WinnerStudentId,
    string? WinnerName,
    List<ElectionCandidateDto> Candidates
);

