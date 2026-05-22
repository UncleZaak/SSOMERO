using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Implementations;

namespace Ssomero.Api.Services.UnitTests;

[TestClass]
public class ClassRepServiceTests
{
    private static SsomeroDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SsomeroDbContext(options);
    }

    private static ClassRepService CreateService(SsomeroDbContext db) =>
        new(db, new Mock<ILogger<ClassRepService>>().Object);

    // ── Seed helpers ─────────────────────────────────────────────────────────

    private static (Guid studentId, Guid mainClassId, SsomeroDbContext db) SeedRepWithMainClass()
    {
        var db = CreateDb();

        var uniId  = Guid.NewGuid();
        var progId = Guid.NewGuid();
        var ayId   = Guid.NewGuid();
        var semId  = Guid.NewGuid();

        db.Universities.Add(new University { Id = uniId, Name = "Test Uni" });
        db.AcademicYears.Add(new AcademicYear { Id = ayId, Name = "2024/25" });
        db.Semesters.Add(new Semester { Id = semId, Name = "Sem 1" });

        var faculty = new Faculty { Id = Guid.NewGuid(), Name = "Sci", UniversityId = uniId };
        var dept    = new Department { Id = Guid.NewGuid(), Name = "CS", FacultyId = faculty.Id };
        db.Faculties.Add(faculty);
        db.Departments.Add(dept);
        db.Programs.Add(new AcademicProgram { Id = progId, Name = "BSc CS", DepartmentId = dept.Id, DurationSemesters = 8 });

        var mainClass = new Class
        {
            Id             = Guid.NewGuid(),
            Name           = "BSc CS Year 1",
            ParentClassId  = null,
            ProgramId      = progId,
            YearOfStudy    = 1,
            SemesterId     = semId,
            AcademicYearId = ayId,
        };
        db.Classes.Add(mainClass);

        var student = new Student
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Jane",
            SecondName   = "Doe",
            Email        = "jane@test.com",
            Phone        = "0700000000",
            Gender       = "F",
            Dob          = new DateOnly(2000, 1, 1),
            PasswordHash = "hash",
            IsVerified   = true,
        };
        db.Students.Add(student);

        db.StudentClasses.Add(new StudentClass
        {
            StudentId = student.Id,
            ClassId   = mainClass.Id,
            Role      = "class_rep",
            Status    = "active",
        });

        db.SaveChanges();
        return (student.Id, mainClass.Id, db);
    }

    // ── GetMyClassAsync ──────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetMyClassAsync_ReturnsMainClass_WhenUserIsRep()
    {
        var (userId, mainClassId, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var result = await svc.GetMyClassAsync(userId);

        Assert.IsNotNull(result);
        Assert.AreEqual(mainClassId, result.Id);
    }

    [TestMethod]
    public async Task GetMyClassAsync_ReturnsNull_WhenUserIsNotRep()
    {
        var (_, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var result = await svc.GetMyClassAsync(Guid.NewGuid());

        Assert.IsNull(result);
    }

    // ── CreateSubclassAsync ──────────────────────────────────────────────────

    [TestMethod]
    public async Task CreateSubclassAsync_Succeeds_WithValidData()
    {
        var (userId, mainClassId, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var result = await svc.CreateSubclassAsync(userId, new CreateSubclassDto("Group A", "Morning group"));

        Assert.IsNotNull(result);
        Assert.AreEqual("Group A", result.Name);
        Assert.IsTrue(await db.Classes.AnyAsync(c => c.Name == "Group A" && c.ParentClassId == mainClassId));
    }

    [TestMethod]
    public async Task CreateSubclassAsync_RejectsDuplicateName()
    {
        var (userId, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        await svc.CreateSubclassAsync(userId, new CreateSubclassDto("Group A", null));

        bool threw = false;
        try { await svc.CreateSubclassAsync(userId, new CreateSubclassDto("group a", null)); }
        catch (InvalidOperationException) { threw = true; }
        Assert.IsTrue(threw, "Expected InvalidOperationException for duplicate name.");
    }

    [TestMethod]
    public async Task CreateSubclassAsync_Throws_WhenUserHasNoManagedClass()
    {
        var (_, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        bool threw = false;
        try { await svc.CreateSubclassAsync(Guid.NewGuid(), new CreateSubclassDto("X", null)); }
        catch (InvalidOperationException) { threw = true; }
        Assert.IsTrue(threw, "Expected InvalidOperationException for user with no managed class.");
    }

    // ── Ownership check ──────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetStudentsAsync_ReturnsEmpty_ForUnrelatedClass()
    {
        var (userId, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var result = await svc.GetStudentsAsync(userId, Guid.NewGuid());

        Assert.IsEmpty(result);
    }

    // ── RenameSubclassAsync ──────────────────────────────────────────────────

    [TestMethod]
    public async Task RenameSubclassAsync_Succeeds_WhenOwned()
    {
        var (userId, mainClassId, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var sub = await svc.CreateSubclassAsync(userId, new CreateSubclassDto("Old Name", null));
        var renamed = await svc.RenameSubclassAsync(userId, sub.Id, new RenameSubclassDto("New Name"));

        Assert.IsNotNull(renamed);
        Assert.AreEqual("New Name", renamed.Name);
    }

    [TestMethod]
    public async Task RenameSubclassAsync_ReturnsNull_ForUnrelatedSubclass()
    {
        var (userId, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var result = await svc.RenameSubclassAsync(userId, Guid.NewGuid(), new RenameSubclassDto("X"));

        Assert.IsNull(result);
    }

    // ── AssignLecturerAsync ──────────────────────────────────────────────────

    [TestMethod]
    public async Task AssignLecturerAsync_Succeeds_WhenLecturerApproved()
    {
        var (userId, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var sub = await svc.CreateSubclassAsync(userId, new CreateSubclassDto("Sub1", null));

        var lecturer = new Lecturer
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Dr",
            LastName     = "Smith",
            Email        = "dr@test.com",
            Phone        = "0700000001",
            PasswordHash = "hash",
            IsVerified   = true,
            IsApproved   = true,
            Status       = UserStatus.Active,
        };
        db.Lecturers.Add(lecturer);
        await db.SaveChangesAsync();

        var ok = await svc.AssignLecturerAsync(userId, sub.Id, new AssignLecturerDto(lecturer.Id));

        Assert.IsTrue(ok);
        Assert.IsTrue(await db.LecturerClasses.AnyAsync(lc => lc.ClassId == sub.Id && lc.LecturerId == lecturer.Id));
    }

    [TestMethod]
    public async Task AssignLecturerAsync_ReturnsFalse_ForUnrelatedSubclass()
    {
        var (userId, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var result = await svc.AssignLecturerAsync(userId, Guid.NewGuid(), new AssignLecturerDto(Guid.NewGuid()));

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AssignLecturerAsync_ReturnsFalse_ForUnapprovedLecturer()
    {
        var (userId, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var sub = await svc.CreateSubclassAsync(userId, new CreateSubclassDto("Sub2", null));

        var lecturer = new Lecturer
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Dr",
            LastName     = "Jones",
            Email        = "jones@test.com",
            Phone        = "0700000002",
            PasswordHash = "hash",
            IsVerified   = true,
            IsApproved   = false,  // not approved
            Status       = UserStatus.Active,
        };
        db.Lecturers.Add(lecturer);
        await db.SaveChangesAsync();

        var ok = await svc.AssignLecturerAsync(userId, sub.Id, new AssignLecturerDto(lecturer.Id));

        Assert.IsFalse(ok);
    }

    // ── RemoveStudentAsync ───────────────────────────────────────────────────

    [TestMethod]
    public async Task RemoveStudentAsync_DropsOnlyMembership_NotStudent()
    {
        var (userId, mainClassId, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        // Enroll a second student into the main class
        var student2 = new Student
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Bob",
            SecondName   = "Brown",
            Email        = "bob@test.com",
            Phone        = "070",
            Gender       = "M",
            Dob          = new DateOnly(2001, 1, 1),
            PasswordHash = "hash",
            IsVerified   = true,
        };
        db.Students.Add(student2);
        db.StudentClasses.Add(new StudentClass { StudentId = student2.Id, ClassId = mainClassId, Role = "student", Status = "active" });
        await db.SaveChangesAsync();

        var removed = await svc.RemoveStudentAsync(userId, mainClassId, student2.Id);

        Assert.IsTrue(removed);
        Assert.IsTrue(await db.Students.AnyAsync(s => s.Id == student2.Id)); // student still exists
        var membership = await db.StudentClasses.FirstAsync(sc => sc.StudentId == student2.Id && sc.ClassId == mainClassId);
        Assert.AreEqual("dropped", membership.Status);
    }

    // ── Stats ────────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task GetStatsAsync_ReturnsZeros_WhenUserHasNoManagedClass()
    {
        var (_, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        var stats = await svc.GetStatsAsync(Guid.NewGuid());

        Assert.AreEqual(0, stats.ManagedClasses);
        Assert.AreEqual(0, stats.TotalStudents);
        Assert.AreEqual(0, stats.TotalSubclasses);
        Assert.AreEqual(0, stats.AssignedLecturers);
        Assert.AreEqual(0, stats.AverageAttendanceRate);
    }

    [TestMethod]
    public async Task GetStatsAsync_ReturnsCorrectCounts()
    {
        var (userId, _, db) = SeedRepWithMainClass();
        var svc = CreateService(db);

        await svc.CreateSubclassAsync(userId, new CreateSubclassDto("Sub A", null));
        await svc.CreateSubclassAsync(userId, new CreateSubclassDto("Sub B", null));

        var stats = await svc.GetStatsAsync(userId);

        Assert.AreEqual(1, stats.ManagedClasses);
        Assert.AreEqual(2, stats.TotalSubclasses);
        Assert.AreEqual(0, stats.AverageAttendanceRate); // no attendance data
    }
}
