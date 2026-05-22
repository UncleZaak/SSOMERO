using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Implementations;

namespace Ssomero.Api.Services.UnitTests;

[TestClass]
public class ClassAnnouncementServiceTests
{
    // ── Infrastructure ────────────────────────────────────────────────────────

    private static SsomeroDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ClassAnnouncementService CreateService(SsomeroDbContext db) =>
        new(db, new Mock<ILogger<ClassAnnouncementService>>().Object);

    // ── Seed helpers ──────────────────────────────────────────────────────────

    private static (Guid repId, Guid mainClassId, Guid subclassId, SsomeroDbContext db)
        SeedRepWithSubclass()
    {
        var db    = CreateDb();
        var ayId  = Guid.NewGuid();
        var semId = Guid.NewGuid();
        var progId = Guid.NewGuid();

        db.AcademicYears.Add(new AcademicYear { Id = ayId,   Name = "2024/25" });
        db.Semesters.Add(new Semester          { Id = semId,  Name = "Sem 1"   });
        var uni    = new University  { Id = Guid.NewGuid(), Name = "Test Uni" };
        var fac    = new Faculty     { Id = Guid.NewGuid(), Name = "Sci",   UniversityId = uni.Id };
        var dept   = new Department  { Id = Guid.NewGuid(), Name = "CS",    FacultyId    = fac.Id };
        var prog   = new AcademicProgram { Id = progId, Name = "BSc CS", DepartmentId = dept.Id, DurationSemesters = 8 };
        db.Universities.Add(uni);
        db.Faculties.Add(fac);
        db.Departments.Add(dept);
        db.Programs.Add(prog);

        var mainClass = new Class
        {
            Id             = Guid.NewGuid(), Name = "CS Year 1",
            ProgramId      = progId, YearOfStudy = 1,
            SemesterId     = semId,  AcademicYearId = ayId,
        };
        var subclass = new Class
        {
            Id             = Guid.NewGuid(), Name = "Group A",
            ParentClassId  = mainClass.Id,
            ProgramId      = progId, YearOfStudy = 1,
            SemesterId     = semId,  AcademicYearId = ayId,
        };
        db.Classes.AddRange(mainClass, subclass);

        var rep = new Student
        {
            Id = Guid.NewGuid(), FirstName = "Jane", SecondName = "Doe",
            Email = "jane@test.com", Phone = "0700000000", Gender = "F",
            Dob = new DateOnly(2000, 1, 1), PasswordHash = "hash", IsVerified = true,
        };
        db.Students.Add(rep);
        db.StudentClasses.Add(new StudentClass
        {
            StudentId = rep.Id, ClassId = mainClass.Id, Role = "class_rep", Status = "active",
        });

        db.SaveChanges();
        return (rep.Id, mainClass.Id, subclass.Id, db);
    }

    // ── GetAnnouncementsAsync ─────────────────────────────────────────────────

    [TestMethod]
    public async Task GetAnnouncements_ReturnsEmpty_WhenNoAnnouncements()
    {
        var (repId, _, _, db) = SeedRepWithSubclass();
        var svc = CreateService(db);

        var result = await svc.GetAnnouncementsAsync(repId);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetAnnouncements_ReturnsEmpty_WhenUserIsNotRep()
    {
        var (_, _, _, db) = SeedRepWithSubclass();
        var svc = CreateService(db);

        var result = await svc.GetAnnouncementsAsync(Guid.NewGuid());

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetAnnouncements_ReturnsPostedAnnouncements_ForManagedClasses()
    {
        var (repId, mainClassId, _, db) = SeedRepWithSubclass();
        db.ClassAnnouncements.Add(new ClassAnnouncement
        {
            Id = Guid.NewGuid(), ClassId = mainClassId, CreatedBy = repId,
            Title = "Test Title", Body = "Test message",
        });
        await db.SaveChangesAsync();

        var svc    = CreateService(db);
        var result = await svc.GetAnnouncementsAsync(repId);

        Assert.HasCount(1, result);
        Assert.AreEqual("Test Title", result[0].Title);
    }

    [TestMethod]
    public async Task GetAnnouncements_ExcludesSoftDeleted()
    {
        var (repId, mainClassId, _, db) = SeedRepWithSubclass();
        db.ClassAnnouncements.Add(new ClassAnnouncement
        {
            Id = Guid.NewGuid(), ClassId = mainClassId, CreatedBy = repId,
            Title = "Deleted", Body = "Deleted msg",
            IsDeleted = true, DeletedAt = DateTime.UtcNow.AddMinutes(-5),
        });
        await db.SaveChangesAsync();

        var svc    = CreateService(db);
        var result = await svc.GetAnnouncementsAsync(repId);

        Assert.IsEmpty(result);
    }

    // ── CreateAnnouncementAsync ───────────────────────────────────────────────

    [TestMethod]
    public async Task CreateAnnouncement_Succeeds_ForManagedClass()
    {
        var (repId, mainClassId, _, db) = SeedRepWithSubclass();
        var svc = CreateService(db);

        var result = await svc.CreateAnnouncementAsync(repId,
            new CreateClassAnnouncementDto(mainClassId, "Hello Class", "This is a test message."));

        Assert.IsNotNull(result);
        Assert.AreEqual("Hello Class", result.Title);
        Assert.AreEqual(repId, result.CreatedBy);
    }

    [TestMethod]
    public async Task CreateAnnouncement_Succeeds_ForSubclass()
    {
        var (repId, _, subclassId, db) = SeedRepWithSubclass();
        var svc = CreateService(db);

        var result = await svc.CreateAnnouncementAsync(repId,
            new CreateClassAnnouncementDto(subclassId, "Subclass Note", "Details here."));

        Assert.IsNotNull(result);
        Assert.AreEqual(subclassId, result.ClassId);
    }

    [TestMethod]
    public async Task CreateAnnouncement_Throws_WhenClassNotManaged()
    {
        var (repId, _, _, db) = SeedRepWithSubclass();
        var svc = CreateService(db);

        bool threw = false;
        try { await svc.CreateAnnouncementAsync(repId, new CreateClassAnnouncementDto(Guid.NewGuid(), "Bad", "Should fail.")); }
        catch (InvalidOperationException) { threw = true; }
        Assert.IsTrue(threw, "Expected InvalidOperationException for unmanaged class.");
    }

    [TestMethod]
    public async Task CreateAnnouncement_Throws_WhenUserIsNotRep()
    {
        var (_, mainClassId, _, db) = SeedRepWithSubclass();
        var svc = CreateService(db);

        bool threw = false;
        try { await svc.CreateAnnouncementAsync(Guid.NewGuid(), new CreateClassAnnouncementDto(mainClassId, "Bad", "Should fail.")); }
        catch (InvalidOperationException) { threw = true; }
        Assert.IsTrue(threw, "Expected InvalidOperationException for non-rep user.");
    }

    [TestMethod]
    public async Task CreateAnnouncement_TrimsWhitespace()
    {
        var (repId, mainClassId, _, db) = SeedRepWithSubclass();
        var svc = CreateService(db);

        var result = await svc.CreateAnnouncementAsync(repId,
            new CreateClassAnnouncementDto(mainClassId, "  Trimmed Title  ", "  Trimmed message.  "));

        Assert.AreEqual("Trimmed Title",    result.Title);
        Assert.AreEqual("Trimmed message.", result.Message);
    }

    // ── DeleteAnnouncementAsync ───────────────────────────────────────────────

    [TestMethod]
    public async Task DeleteAnnouncement_Succeeds_WhenCalledByCreator()
    {
        var (repId, mainClassId, _, db) = SeedRepWithSubclass();
        var announcementId = Guid.NewGuid();
        db.ClassAnnouncements.Add(new ClassAnnouncement
        {
            Id = announcementId, ClassId = mainClassId, CreatedBy = repId,
            Title = "To Delete", Body = "Bye",
        });
        await db.SaveChangesAsync();

        var svc    = CreateService(db);
        var result = await svc.DeleteAnnouncementAsync(repId, announcementId);

        Assert.IsTrue(result);
        var deleted = await db.ClassAnnouncements.IgnoreQueryFilters()
                              .FirstAsync(a => a.Id == announcementId);
        Assert.IsTrue(deleted.IsDeleted);
        Assert.IsNotNull(deleted.DeletedAt);
    }

    [TestMethod]
    public async Task DeleteAnnouncement_ReturnsFalse_WhenCalledByOtherUser()
    {
        var (repId, mainClassId, _, db) = SeedRepWithSubclass();
        var announcementId = Guid.NewGuid();
        db.ClassAnnouncements.Add(new ClassAnnouncement
        {
            Id = announcementId, ClassId = mainClassId, CreatedBy = repId,
            Title = "Protected", Body = "Only creator can delete",
        });
        await db.SaveChangesAsync();

        var svc    = CreateService(db);
        var result = await svc.DeleteAnnouncementAsync(Guid.NewGuid(), announcementId);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task DeleteAnnouncement_ReturnsFalse_WhenNotFound()
    {
        var (repId, _, _, db) = SeedRepWithSubclass();
        var svc = CreateService(db);

        var result = await svc.DeleteAnnouncementAsync(repId, Guid.NewGuid());

        Assert.IsFalse(result);
    }

    // ── GetAnalyticsAsync ─────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetAnalytics_ReturnsZeroes_WhenUserIsNotRep()
    {
        var (_, _, _, db) = SeedRepWithSubclass();
        var svc    = CreateService(db);
        var result = await svc.GetAnalyticsAsync(Guid.NewGuid());

        Assert.AreEqual(0, result.TotalStudents);
        Assert.AreEqual(0, result.TotalSubclasses);
        Assert.AreEqual(0, result.AssignedLecturers);
    }

    [TestMethod]
    public async Task GetAnalytics_Returns8WeekTrend()
    {
        var (repId, _, _, db) = SeedRepWithSubclass();
        var svc    = CreateService(db);
        var result = await svc.GetAnalyticsAsync(repId);

        Assert.HasCount(8, result.AttendanceTrend);
        Assert.HasCount(8, result.StudentGrowthTrend);
    }

    [TestMethod]
    public async Task GetAnalytics_SubclassCount_MatchesSeedData()
    {
        var (repId, _, _, db) = SeedRepWithSubclass();
        var svc    = CreateService(db);
        var result = await svc.GetAnalyticsAsync(repId);

        // Seeded 1 subclass
        Assert.AreEqual(1, result.TotalSubclasses);
    }
}
