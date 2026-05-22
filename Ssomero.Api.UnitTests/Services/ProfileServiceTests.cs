using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Implementations;
using Ssomero.Api.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Ssomero.Api.Services.UnitTests;

[TestClass]
public class ProfileServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static SsomeroDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SsomeroDbContext(opts);
    }

    private static ProfileService CreateService(SsomeroDbContext db) =>
        new ProfileService(db, new Mock<ILogger<ProfileService>>().Object);

    // ── Seed helpers ──────────────────────────────────────────────────────────

    private static Student SeedStudent(SsomeroDbContext db, Guid? id = null)
    {
        var student = new Student
        {
            Id           = id ?? Guid.NewGuid(),
            FirstName    = "Jane",
            SecondName   = "Doe",
            Email        = "jane@example.com",
            Phone        = "0700000001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
            IsVerified   = true
        };
        db.Students.Add(student);
        db.SaveChanges();
        return student;
    }

    private static Lecturer SeedLecturer(SsomeroDbContext db, Guid? id = null)
    {
        var lecturer = new Lecturer
        {
            Id           = id ?? Guid.NewGuid(),
            FirstName    = "John",
            LastName     = "Smith",
            Email        = "john@example.com",
            Phone        = "0700000002",
            StaffId      = "STAFF-001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
            IsVerified   = true,
            IsApproved   = true
        };
        db.Lecturers.Add(lecturer);
        db.SaveChanges();
        return lecturer;
    }

    private static Admin SeedAdmin(SsomeroDbContext db, Guid? id = null)
    {
        var admin = new Admin
        {
            Id           = id ?? Guid.NewGuid(),
            Email        = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
            Status       = UserStatus.Active
        };
        db.Admins.Add(admin);
        db.SaveChanges();
        return admin;
    }

    // ── GetProfileAsync — Student ─────────────────────────────────────────────

    [TestMethod]
    public async Task GetProfile_Student_ReturnsStudentProfileDto()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);
        var svc = CreateService(db);

        var result = await svc.GetProfileAsync(student.Id, "Student");

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(StudentProfileDto));
        Assert.AreEqual("Student", result.Role);
        Assert.AreEqual(student.Email, result.Email);
        Assert.AreEqual("Jane", result.FirstName);
        Assert.AreEqual("Doe", result.LastName);
    }

    [TestMethod]
    public async Task GetProfile_Student_AttendancePercentage_ZeroWhenNoRecords()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);
        var svc = CreateService(db);

        var result = (StudentProfileDto)(await svc.GetProfileAsync(student.Id, "Student"))!;

        Assert.AreEqual(0.0, result.AttendancePercentage);
    }

    [TestMethod]
    public async Task GetProfile_Student_AttendancePercentage_CalculatedCorrectly()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);

        // 3 sessions, 2 present → 66.7 %
        var classId = Guid.NewGuid();
        db.Attendances.AddRange(
            new Attendance { Id = Guid.NewGuid(), StudentId = student.Id, ClassId = classId, Date = DateTime.UtcNow, IsPresent = true },
            new Attendance { Id = Guid.NewGuid(), StudentId = student.Id, ClassId = classId, Date = DateTime.UtcNow, IsPresent = true },
            new Attendance { Id = Guid.NewGuid(), StudentId = student.Id, ClassId = classId, Date = DateTime.UtcNow, IsPresent = false });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = (StudentProfileDto)(await svc.GetProfileAsync(student.Id, "Student"))!;

        Assert.AreEqual(66.7, result.AttendancePercentage);
    }

    [TestMethod]
    public async Task GetProfile_Student_SubscriptionStatus_ActiveWhenSubscribed()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);

        db.Subscriptions.Add(new Subscription
        {
            Id        = Guid.NewGuid(),
            UserId    = student.Id,
            Plan      = PaymentPlan.Monthly,
            IsActive  = true,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate   = DateTime.UtcNow.AddDays(25),
            PaymentId = Guid.NewGuid()
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = (StudentProfileDto)(await svc.GetProfileAsync(student.Id, "Student"))!;

        Assert.AreNotEqual("None", result.SubscriptionStatus);
    }

    [TestMethod]
    public async Task GetProfile_Student_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var result = await svc.GetProfileAsync(Guid.NewGuid(), "Student");

        Assert.IsNull(result);
    }

    // ── GetProfileAsync — Lecturer ────────────────────────────────────────────

    [TestMethod]
    public async Task GetProfile_Lecturer_ReturnsLecturerProfileDto()
    {
        await using var db = CreateDb();
        var lecturer = SeedLecturer(db);
        var svc = CreateService(db);

        var result = await svc.GetProfileAsync(lecturer.Id, "Lecturer");

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(LecturerProfileDto));
        Assert.AreEqual("Lecturer", result.Role);
        Assert.AreEqual("john@example.com", result.Email);
        var lp = (LecturerProfileDto)result;
        Assert.AreEqual("STAFF-001", lp.StaffId);
    }

    [TestMethod]
    public async Task GetProfile_Lecturer_StatsDefault_WhenNoClasses()
    {
        await using var db = CreateDb();
        var lecturer = SeedLecturer(db);
        var svc = CreateService(db);

        var result = (LecturerProfileDto)(await svc.GetProfileAsync(lecturer.Id, "Lecturer"))!;

        Assert.AreEqual(0, result.AssignedClassesCount);
        Assert.AreEqual(0, result.MaterialsUploadedCount);
        Assert.AreEqual(0, result.AttendanceSessionsManaged);
    }

    [TestMethod]
    public async Task GetProfile_Lecturer_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var result = await svc.GetProfileAsync(Guid.NewGuid(), "Lecturer");

        Assert.IsNull(result);
    }

    // ── GetProfileAsync — Admin ───────────────────────────────────────────────

    [TestMethod]
    public async Task GetProfile_Admin_ReturnsAdminProfileDto()
    {
        await using var db = CreateDb();
        var admin = SeedAdmin(db);
        var svc = CreateService(db);

        var result = await svc.GetProfileAsync(admin.Id, "Admin");

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AdminProfileDto));
        Assert.AreEqual("Admin", result.Role);
        Assert.AreEqual("admin@example.com", result.Email);
    }

    [TestMethod]
    public async Task GetProfile_Admin_IncludesAllUniversities()
    {
        await using var db = CreateDb();
        var admin = SeedAdmin(db);
        db.Universities.AddRange(
            new University { Id = Guid.NewGuid(), Name = "University A" },
            new University { Id = Guid.NewGuid(), Name = "University B" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = (AdminProfileDto)(await svc.GetProfileAsync(admin.Id, "Admin"))!;

        Assert.AreEqual(2, result.ManagedUniversities.Count);
    }

    [TestMethod]
    public async Task GetProfile_Admin_ReturnsNull_WhenNotFound()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var result = await svc.GetProfileAsync(Guid.NewGuid(), "Admin");

        Assert.IsNull(result);
    }

    // ── UpdateProfileAsync ────────────────────────────────────────────────────

    [TestMethod]
    public async Task UpdateProfile_Student_UpdatesNameAndPhone()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);
        var svc = CreateService(db);

        var result = await svc.UpdateProfileAsync(student.Id, "Student",
            new UpdateProfileDto("Alice", "Wonder", "0711111111", null));

        Assert.IsTrue(result);
        var updated = await db.Students.FindAsync(student.Id);
        Assert.AreEqual("Alice", updated!.FirstName);
        Assert.AreEqual("Wonder", updated.SecondName);
        Assert.AreEqual("0711111111", updated.Phone);
    }

    [TestMethod]
    public async Task UpdateProfile_Student_PhotoUrl_Updated()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);
        var svc = CreateService(db);

        await svc.UpdateProfileAsync(student.Id, "Student",
            new UpdateProfileDto(null, null, null, "https://example.com/photo.jpg"));

        var updated = await db.Students.FindAsync(student.Id);
        Assert.AreEqual("https://example.com/photo.jpg", updated!.Photo);
    }

    [TestMethod]
    public async Task UpdateProfile_Lecturer_UpdatesFields()
    {
        await using var db = CreateDb();
        var lecturer = SeedLecturer(db);
        var svc = CreateService(db);

        var result = await svc.UpdateProfileAsync(lecturer.Id, "Lecturer",
            new UpdateProfileDto("James", "Brown", "0722222222", null));

        Assert.IsTrue(result);
        var updated = await db.Lecturers.FindAsync(lecturer.Id);
        Assert.AreEqual("James", updated!.FirstName);
        Assert.AreEqual("Brown", updated.LastName);
    }

    [TestMethod]
    public async Task UpdateProfile_Student_ReturnsFalse_WhenNotFound()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var result = await svc.UpdateProfileAsync(Guid.NewGuid(), "Student",
            new UpdateProfileDto("X", null, null, null));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task UpdateProfile_NullFields_DoNotOverwriteExistingValues()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);
        var svc = CreateService(db);

        // Only update FirstName — other fields should remain unchanged
        await svc.UpdateProfileAsync(student.Id, "Student",
            new UpdateProfileDto("Updated", null, null, null));

        var updated = await db.Students.FindAsync(student.Id);
        Assert.AreEqual("Updated", updated!.FirstName);
        Assert.AreEqual("Doe", updated.SecondName);    // unchanged
        Assert.AreEqual("0700000001", updated.Phone);  // unchanged
    }

    // ── ChangePasswordAsync ───────────────────────────────────────────────────

    [TestMethod]
    public async Task ChangePassword_Student_Success()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);
        var svc = CreateService(db);

        var result = await svc.ChangePasswordAsync(student.Id, "Student",
            new ChangePasswordDto("Password1!", "NewSecure2@"));

        Assert.AreEqual(ChangePasswordResult.Success, result);
        var updated = await db.Students.FindAsync(student.Id);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify("NewSecure2@", updated!.PasswordHash));
    }

    [TestMethod]
    public async Task ChangePassword_Lecturer_Success()
    {
        await using var db = CreateDb();
        var lecturer = SeedLecturer(db);
        var svc = CreateService(db);

        var result = await svc.ChangePasswordAsync(lecturer.Id, "Lecturer",
            new ChangePasswordDto("Password1!", "NewSecure2@"));

        Assert.AreEqual(ChangePasswordResult.Success, result);
    }

    [TestMethod]
    public async Task ChangePassword_Admin_Success()
    {
        await using var db = CreateDb();
        var admin = SeedAdmin(db);
        var svc = CreateService(db);

        var result = await svc.ChangePasswordAsync(admin.Id, "Admin",
            new ChangePasswordDto("Password1!", "NewSecure2@"));

        Assert.AreEqual(ChangePasswordResult.Success, result);
    }

    [TestMethod]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsWrongCurrentPassword()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);
        var svc = CreateService(db);

        var result = await svc.ChangePasswordAsync(student.Id, "Student",
            new ChangePasswordDto("WrongPassword!", "NewSecure2@"));

        Assert.AreEqual(ChangePasswordResult.WrongCurrentPassword, result);
    }

    [TestMethod]
    public async Task ChangePassword_SameAsCurrentPassword_ReturnsSameAsCurrentPassword()
    {
        await using var db = CreateDb();
        var student = SeedStudent(db);
        var svc = CreateService(db);

        var result = await svc.ChangePasswordAsync(student.Id, "Student",
            new ChangePasswordDto("Password1!", "Password1!"));

        Assert.AreEqual(ChangePasswordResult.SameAsCurrentPassword, result);
    }

    [TestMethod]
    public async Task ChangePassword_UserNotFound_ReturnsUserNotFound()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        var result = await svc.ChangePasswordAsync(Guid.NewGuid(), "Student",
            new ChangePasswordDto("Password1!", "NewSecure2@"));

        Assert.AreEqual(ChangePasswordResult.UserNotFound, result);
    }

    // ── Tenant isolation ─────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetProfile_TenantIsolation_StudentCannotSeeOtherStudentProfile()
    {
        await using var db = CreateDb();
        var studentA = SeedStudent(db);
        // Seed a second student — service must use the userId filter
        var studentB = new Student
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Other",
            SecondName   = "User",
            Email        = "other@example.com",
            Phone        = "0700000099",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
            IsVerified   = true
        };
        db.Students.Add(studentB);
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = (StudentProfileDto)(await svc.GetProfileAsync(studentA.Id, "Student"))!;

        // The returned profile must belong to studentA, not studentB
        Assert.AreEqual(studentA.Email, result.Email);
    }
}
