using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ssomero.Api.Data.Configurations;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Data;

/// <summary>SQLite stores TimeOnly as ticks (long). This converter handles the round-trip.</summary>
internal sealed class TimeOnlyConverter()
    : ValueConverter<TimeOnly, long>(v => v.Ticks, v => new TimeOnly(v));

public class SsomeroDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public SsomeroDbContext(DbContextOptions<SsomeroDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Returns null when there is no HTTP context (background jobs, migrations) — filters are skipped in that case.
    private Guid? GetCurrentTenantId()
    {
        var claim = _httpContextAccessor?.HttpContext?.User.FindFirstValue("university_id");
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<AcademicProfile> AcademicProfiles => Set<AcademicProfile>();

    // Academic hierarchy
    public DbSet<University> Universities => Set<University>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AcademicProgram> Programs => Set<AcademicProgram>();

    // Lookups
    public DbSet<EntryScheme> EntrySchemes => Set<EntryScheme>();
    public DbSet<Intake> Intakes => Set<Intake>();
    public DbSet<StudyMode> StudyModes => Set<StudyMode>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Semester> Semesters => Set<Semester>();

    // Classes
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Curriculum> Curricula => Set<Curriculum>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();
    public DbSet<LecturerClass> LecturerClasses => Set<LecturerClass>();

    // OTP
    public DbSet<Otp> Otps => Set<Otp>();

    // Audit & Tenancy
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<ClassMaterial> ClassMaterials => Set<ClassMaterial>();

    // Payments & Subscriptions
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    // Password reset
    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();

    // Class Announcements
    public DbSet<ClassAnnouncement> ClassAnnouncements => Set<ClassAnnouncement>();

    // Class Elections
    public DbSet<ClassElection> ClassElections => Set<ClassElection>();
    public DbSet<ClassElectionCandidate> ClassElectionCandidates => Set<ClassElectionCandidate>();
    public DbSet<ClassElectionVote> ClassElectionVotes => Set<ClassElectionVote>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ---- Lecturer ----
        mb.Entity<Lecturer>(e =>
        {
            e.HasIndex(l => l.Email).IsUnique();
            e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(UserStatus.Active);
            e.HasQueryFilter(l => !l.IsDeleted && (GetCurrentTenantId() == null || l.UniversityId == GetCurrentTenantId()));
            e.HasOne(l => l.University).WithMany()
             .HasForeignKey(l => l.UniversityId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ---- Admin ----
        mb.Entity<Admin>(e =>
        {
            e.HasIndex(a => a.Email).IsUnique();
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(UserStatus.Active);
            e.HasQueryFilter(a => !a.IsDeleted);
        });

        // ---- AcademicProfile ----
        mb.Entity<AcademicProfile>(e =>
        {
            // Enforce true 1-to-1 at the DB level with an explicit unique index.
            // Run: dotnet ef migrations add AddAcademicProfileUniqueIndex
            e.HasIndex(ap => ap.StudentId).IsUnique();
            e.Property(ap => ap.RowVersion).IsRowVersion();

            e.HasOne(ap => ap.Student).WithOne(s => s.AcademicProfile)
             .HasForeignKey<AcademicProfile>(ap => ap.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(ap => !ap.Student.IsDeleted &&
                (GetCurrentTenantId() == null || ap.Student.UniversityId == GetCurrentTenantId()));
        });

        // ---- Academic hierarchy ----
        mb.Entity<University>(e =>
        {
            e.HasIndex(u => u.Name).IsUnique();
        });

        mb.Entity<Faculty>(e =>
        {
            e.HasIndex(f => new { f.Name, f.UniversityId }).IsUnique();
            e.HasOne(f => f.University).WithMany(u => u.Faculties)
             .HasForeignKey(f => f.UniversityId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Department>(e =>
        {
            e.HasIndex(d => new { d.Name, d.FacultyId }).IsUnique();
            e.HasOne(d => d.Faculty).WithMany(f => f.Departments)
             .HasForeignKey(d => d.FacultyId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<AcademicProgram>(e =>
        {
            e.HasIndex(p => new { p.Name, p.DepartmentId }).IsUnique();
            e.HasOne(p => p.Department).WithMany(d => d.Programs)
             .HasForeignKey(p => p.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Class self-referencing ----
        mb.Entity<Class>(e =>
        {
            e.HasOne(c => c.ParentClass).WithMany(c => c.SubClasses)
             .HasForeignKey(c => c.ParentClassId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Program).WithMany(p => p.Classes)
             .HasForeignKey(c => c.ProgramId);
        });

        // ---- Curriculum ----
        mb.Entity<Curriculum>(e =>
        {
            e.HasIndex(c => new { c.CourseCode, c.ProgramId }).IsUnique();
            e.HasOne(c => c.Program).WithMany(p => p.CurriculumEntries)
             .HasForeignKey(c => c.ProgramId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- StudentClass composite key ----
        mb.Entity<StudentClass>(e =>
        {
            e.HasKey(sc => new { sc.StudentId, sc.ClassId });
            e.HasOne(sc => sc.Student).WithMany(s => s.StudentClasses)
             .HasForeignKey(sc => sc.StudentId);
            e.HasOne(sc => sc.Class).WithMany(c => c.StudentClasses)
             .HasForeignKey(sc => sc.ClassId);
            e.HasQueryFilter(sc => !sc.Student.IsDeleted &&
                (GetCurrentTenantId() == null || sc.Student.UniversityId == GetCurrentTenantId()));
        });

        // ---- LecturerClass composite key ----
        mb.Entity<LecturerClass>(e =>
        {
            e.HasKey(lc => new { lc.LecturerId, lc.ClassId });
            e.HasOne(lc => lc.Lecturer).WithMany(l => l.LecturerClasses)
             .HasForeignKey(lc => lc.LecturerId);
            e.HasOne(lc => lc.Class).WithMany(c => c.LecturerClasses)
             .HasForeignKey(lc => lc.ClassId);
            e.HasQueryFilter(lc => !lc.Lecturer.IsDeleted &&
                (GetCurrentTenantId() == null || lc.Lecturer.UniversityId == GetCurrentTenantId()));
        });

        // ---- Otp ----
        mb.Entity<Otp>(e =>
        {
            e.HasIndex(o => new { o.Email, o.OtpCode });
            e.HasIndex(o => new { o.Email, o.VerificationToken });
        });

        // ---- Fluent configurations ----
        mb.ApplyConfiguration(new StudentConfiguration());
        mb.ApplyConfiguration(new AttendanceConfiguration());

        // Override Student query filter to add tenant scoping on top of soft-delete.
        // GetCurrentTenantId() returns null in background/migration contexts → no tenant filter applied.
        mb.Entity<Student>().HasQueryFilter(
            s => !s.IsDeleted && (GetCurrentTenantId() == null || s.UniversityId == GetCurrentTenantId()));

        // Mirror Student's filter on Attendance so required Student navigation is consistent.
        mb.Entity<Attendance>().HasQueryFilter(
            a => !a.Student.IsDeleted && (GetCurrentTenantId() == null || a.Student.UniversityId == GetCurrentTenantId()));

        // ---- RowVersion on Student ----
        mb.Entity<Student>().Property(s => s.RowVersion).IsRowVersion();

        // ---- ClassSession ----
        mb.Entity<ClassSession>(e =>
        {
            e.HasOne(cs => cs.Class).WithMany(c => c.Sessions)
             .HasForeignKey(cs => cs.ClassId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(cs => cs.StartTime).HasConversion<TimeOnlyConverter>();
            e.Property(cs => cs.EndTime).HasConversion<TimeOnlyConverter>();
        });

        // ---- Attendance FK to ClassSession ----
        mb.Entity<Attendance>(e =>
        {
            e.HasOne(a => a.Session).WithMany(cs => cs.Attendances)
             .HasForeignKey(a => a.SessionId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ---- AuditLog ----
        mb.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.EntityName);
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.CreatedAt);
        });

        // ---- ClassMaterial ----
        mb.Entity<ClassMaterial>(e =>
        {
            e.HasOne(m => m.Class).WithMany()
             .HasForeignKey(m => m.ClassId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Lecturer).WithMany()
             .HasForeignKey(m => m.UploadedBy)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(m => m.ClassId);
            // Mirror lecturer's global filter so EF doesn't warn about required-end filter mismatch.
            e.HasQueryFilter(m => !m.Lecturer!.IsDeleted &&
                (GetCurrentTenantId() == null || m.Lecturer.UniversityId == GetCurrentTenantId()));
        });

        // ---- Payment ----
        mb.Entity<Payment>(e =>
        {
            e.HasOne(p => p.Student).WithMany()
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => p.ExternalReference).IsUnique();
            e.HasIndex(p => p.UserId);
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Provider).HasConversion<string>().HasMaxLength(30);
            e.Property(p => p.Plan).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            // Mirror Student's global filter so EF doesn't warn about required-end filter mismatch.
            e.HasQueryFilter(p => !p.Student!.IsDeleted &&
                (GetCurrentTenantId() == null || p.Student.UniversityId == GetCurrentTenantId()));
        });

        // ---- Subscription ----
        mb.Entity<Subscription>(e =>
        {
            e.HasOne(s => s.Student).WithMany()
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Payment).WithMany()
             .HasForeignKey(s => s.PaymentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => new { s.UserId, s.IsActive });
            e.Property(s => s.Plan).HasConversion<string>().HasMaxLength(20);
            // Mirror Student's global filter so EF doesn't warn about required-end filter mismatch.
            e.HasQueryFilter(s => !s.Student!.IsDeleted &&
                (GetCurrentTenantId() == null || s.Student.UniversityId == GetCurrentTenantId()));
        });

        // ---- PasswordResetRequest ----
        mb.Entity<PasswordResetRequest>(e =>
        {
            e.HasIndex(p => p.Email);
            e.HasIndex(p => p.ExpiresAt);
            e.Property(p => p.Email).HasMaxLength(200);
            e.Property(p => p.OtpHash).HasMaxLength(200);
            e.Property(p => p.ResetTokenHash).HasMaxLength(200);
        });

        // ---- ClassAnnouncement ----
        mb.Entity<ClassAnnouncement>(e =>
        {
            e.HasOne(a => a.Class).WithMany()
             .HasForeignKey(a => a.ClassId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => a.ClassId);
            e.HasIndex(a => a.CreatedBy);
            e.HasQueryFilter(a => !a.IsDeleted);
        });

        // ---- ClassElection ----
        mb.Entity<ClassElection>(e =>
        {
            e.HasOne(el => el.Class).WithMany()
             .HasForeignKey(el => el.ClassId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(el => new { el.ClassId, el.Status });
            e.HasQueryFilter(el => !el.IsDeleted);
        });

        // ---- ClassElectionCandidate ----
        mb.Entity<ClassElectionCandidate>(e =>
        {
            e.HasOne(c => c.Election).WithMany(el => el.Candidates)
             .HasForeignKey(c => c.ElectionId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Student).WithMany()
             .HasForeignKey(c => c.StudentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(c => new { c.ElectionId, c.StudentId }).IsUnique();
            e.HasQueryFilter(c => !c.Election.IsDeleted);
        });

        // ---- ClassElectionVote ----
        mb.Entity<ClassElectionVote>(e =>
        {
            e.HasOne(v => v.Election).WithMany(el => el.Votes)
             .HasForeignKey(v => v.ElectionId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.VoterStudent).WithMany()
             .HasForeignKey(v => v.VoterStudentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(v => new { v.ElectionId, v.VoterStudentId }).IsUnique();
            e.HasQueryFilter(v => !v.Election.IsDeleted);
        });
    }
}
