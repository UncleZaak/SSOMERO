using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Implementations;

namespace Ssomero.Api.Services.UnitTests;

[TestClass]
public class ClassElectionServiceTests
{
    private static SsomeroDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ClassElectionService CreateService(SsomeroDbContext db) =>
        new(db, new Mock<ILogger<ClassElectionService>>().Object);

    // ── Seed helpers ─────────────────────────────────────────────────────────

    private static (Guid studentId, Guid classId, SsomeroDbContext db) SeedEnrolledStudent(string email = "s@test.com")
    {
        var db = CreateDb();

        var uniId  = Guid.NewGuid();
        var progId = Guid.NewGuid();
        var ayId   = Guid.NewGuid();
        var semId  = Guid.NewGuid();

        db.Universities.Add(new University { Id = uniId, Name = "U" });
        db.AcademicYears.Add(new AcademicYear { Id = ayId, Name = "2025" });
        db.Semesters.Add(new Semester { Id = semId, Name = "S1" });

        var fac  = new Faculty { Id = Guid.NewGuid(), Name = "F", UniversityId = uniId };
        var dept = new Department { Id = Guid.NewGuid(), Name = "D", FacultyId = fac.Id };
        db.Faculties.Add(fac);
        db.Departments.Add(dept);
        db.Programs.Add(new AcademicProgram { Id = progId, Name = "P", DepartmentId = dept.Id, DurationSemesters = 8 });

        var cls = new Class
        {
            Id             = Guid.NewGuid(),
            Name           = "Main Class",
            ParentClassId  = null,
            ProgramId      = progId,
            YearOfStudy    = 1,
            SemesterId     = semId,
            AcademicYearId = ayId,
        };
        db.Classes.Add(cls);

        var student = new Student
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Alice",
            SecondName   = "Student",
            Email        = email,
            Phone        = "0700",
            Gender       = "F",
            Dob          = new DateOnly(2000, 1, 1),
            PasswordHash = "x",
            IsVerified   = true,
        };
        db.Students.Add(student);

        db.StudentClasses.Add(new StudentClass
        {
            StudentId = student.Id,
            ClassId   = cls.Id,
            Role      = "student",
            Status    = "active",
        });

        db.SaveChanges();
        return (student.Id, cls.Id, db);
    }

    // ── StartElection creates new election ───────────────────────────────────

    [TestMethod]
    public async Task StartElection_CreatesNewElection_AndAddsInitiatorAsCandidate()
    {
        var (studentId, classId, db) = SeedEnrolledStudent();
        var svc = CreateService(db);

        var result = await svc.StartElectionAsync(studentId, new StartElectionRequestDto(classId));

        Assert.AreEqual("Active", result.Status);
        Assert.AreEqual(classId, result.ClassId);
        Assert.AreEqual(1, result.Candidates.Count);
        Assert.IsTrue(result.Candidates[0].IsCurrentUser);
        Assert.IsTrue(result.SecondsRemaining > 0);
    }

    // ── StartElection returns existing active election ───────────────────────

    [TestMethod]
    public async Task StartElection_ReturnsExistingActiveElection_WhenOneExists()
    {
        var (studentId, classId, db) = SeedEnrolledStudent();
        var svc = CreateService(db);

        var first  = await svc.StartElectionAsync(studentId, new StartElectionRequestDto(classId));
        var second = await svc.StartElectionAsync(studentId, new StartElectionRequestDto(classId));

        Assert.AreEqual(first.Id, second.Id);
        Assert.AreEqual(1, db.ClassElections.Count(e => e.ClassId == classId));
    }

    // ── StartElection rejects unenrolled student ─────────────────────────────

    [TestMethod]
    public async Task StartElection_Throws_WhenStudentNotEnrolled()
    {
        var (_, classId, db) = SeedEnrolledStudent();
        var outsider = Guid.NewGuid();
        var svc = CreateService(db);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => svc.StartElectionAsync(outsider, new StartElectionRequestDto(classId)));
    }

    // ── StartElection rejects subclass ───────────────────────────────────────

    [TestMethod]
    public async Task StartElection_Throws_WhenClassIsSubclass()
    {
        var (studentId, _, db) = SeedEnrolledStudent();

        // Create a subclass
        var parent = db.Classes.First();
        var sub = new Class
        {
            Id             = Guid.NewGuid(),
            Name           = "Sub",
            ParentClassId  = parent.Id,
            ProgramId      = parent.ProgramId,
            YearOfStudy    = parent.YearOfStudy,
            SemesterId     = parent.SemesterId,
            AcademicYearId = parent.AcademicYearId,
        };
        db.Classes.Add(sub);
        db.SaveChanges();

        var svc = CreateService(db);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => svc.StartElectionAsync(studentId, new StartElectionRequestDto(sub.Id)));
    }

    // ── Vote succeeds ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Vote_Succeeds_AndRecordsVote()
    {
        var (student1, classId, db) = SeedEnrolledStudent("a@test.com");

        // Add a second student (voter)
        var voter = new Student
        {
            Id = Guid.NewGuid(), FirstName = "Bob", SecondName = "V",
            Email = "b@test.com", Phone = "0", Gender = "M",
            Dob = new DateOnly(2000, 1, 1), PasswordHash = "x", IsVerified = true,
        };
        db.Students.Add(voter);
        db.StudentClasses.Add(new StudentClass { StudentId = voter.Id, ClassId = classId, Role = "student", Status = "active" });
        db.SaveChanges();

        var svc = CreateService(db);
        var election = await svc.StartElectionAsync(student1, new StartElectionRequestDto(classId));

        var result = await svc.VoteAsync(voter.Id, election.Id, new VoteRequestDto(student1));

        Assert.AreEqual(1, result.Candidates.Single(c => c.StudentId == student1).VoteCount);
    }

    // ── Duplicate vote rejected ───────────────────────────────────────────────

    [TestMethod]
    public async Task Vote_Throws_WhenDuplicateVote()
    {
        var (candidateId, classId, db) = SeedEnrolledStudent("c@test.com");

        var voter = new Student
        {
            Id = Guid.NewGuid(), FirstName = "X", SecondName = "Y",
            Email = "v@test.com", Phone = "0", Gender = "M",
            Dob = new DateOnly(2000, 1, 1), PasswordHash = "x", IsVerified = true,
        };
        db.Students.Add(voter);
        db.StudentClasses.Add(new StudentClass { StudentId = voter.Id, ClassId = classId, Role = "student", Status = "active" });
        db.SaveChanges();

        var svc = CreateService(db);
        var election = await svc.StartElectionAsync(candidateId, new StartElectionRequestDto(classId));

        await svc.VoteAsync(voter.Id, election.Id, new VoteRequestDto(candidateId));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => svc.VoteAsync(voter.Id, election.Id, new VoteRequestDto(candidateId)));
    }

    // ── FinalizeElection promotes winner ─────────────────────────────────────

    [TestMethod]
    public async Task FinalizeElection_PromotesWinner()
    {
        var (candidateId, classId, db) = SeedEnrolledStudent("win@test.com");

        var voter = new Student
        {
            Id = Guid.NewGuid(), FirstName = "V", SecondName = "V",
            Email = "vv@test.com", Phone = "0", Gender = "M",
            Dob = new DateOnly(2000, 1, 1), PasswordHash = "x", IsVerified = true,
        };
        db.Students.Add(voter);
        db.StudentClasses.Add(new StudentClass { StudentId = voter.Id, ClassId = classId, Role = "student", Status = "active" });
        db.SaveChanges();

        var svc = CreateService(db);
        var election = await svc.StartElectionAsync(candidateId, new StartElectionRequestDto(classId));
        await svc.VoteAsync(voter.Id, election.Id, new VoteRequestDto(candidateId));

        var result = await svc.FinalizeElectionAsync(election.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual("Completed", result.Status);
        Assert.AreEqual(candidateId, result.WinnerStudentId);

        var membership = db.StudentClasses.Single(sc => sc.StudentId == candidateId && sc.ClassId == classId);
        Assert.AreEqual("class_rep", membership.Role);
    }

    // ── Previous Class Rep demoted ────────────────────────────────────────────

    [TestMethod]
    public async Task FinalizeElection_DemotesPreviousClassRep()
    {
        var (student1, classId, db) = SeedEnrolledStudent("cr@test.com");

        // Set student1 as existing rep
        var sc1 = db.StudentClasses.Single(s => s.StudentId == student1);
        sc1.Role = "class_rep";

        var student2 = new Student
        {
            Id = Guid.NewGuid(), FirstName = "N", SecondName = "W",
            Email = "nw@test.com", Phone = "0", Gender = "M",
            Dob = new DateOnly(2000, 1, 1), PasswordHash = "x", IsVerified = true,
        };
        db.Students.Add(student2);
        db.StudentClasses.Add(new StudentClass { StudentId = student2.Id, ClassId = classId, Role = "student", Status = "active" });
        db.SaveChanges();

        var svc = CreateService(db);
        var election = await svc.StartElectionAsync(student2.Id, new StartElectionRequestDto(classId));
        // student2 self-nominated; student1 votes for student2
        await svc.VoteAsync(student1, election.Id, new VoteRequestDto(student2.Id));

        await svc.FinalizeElectionAsync(election.Id);

        var demoted = db.StudentClasses.Single(s => s.StudentId == student1 && s.ClassId == classId);
        Assert.AreEqual("student", demoted.Role);

        var promoted = db.StudentClasses.Single(s => s.StudentId == student2.Id && s.ClassId == classId);
        Assert.AreEqual("class_rep", promoted.Role);
    }

    // ── Tie resolved by earliest candidate ───────────────────────────────────

    [TestMethod]
    public async Task FinalizeElection_TieResolved_ByEarliestCandidate()
    {
        var (candidate1, classId, db) = SeedEnrolledStudent("t1@test.com");

        var candidate2 = new Student
        {
            Id = Guid.NewGuid(), FirstName = "T2", SecondName = "X",
            Email = "t2@test.com", Phone = "0", Gender = "M",
            Dob = new DateOnly(2000, 1, 1), PasswordHash = "x", IsVerified = true,
        };
        db.Students.Add(candidate2);
        db.StudentClasses.Add(new StudentClass { StudentId = candidate2.Id, ClassId = classId, Role = "student", Status = "active" });
        db.SaveChanges();

        var svc = CreateService(db);
        // candidate1 starts election (earliest)
        var election = await svc.StartElectionAsync(candidate1, new StartElectionRequestDto(classId));

        // Manually add candidate2 with slightly later time to avoid DB race
        var cand2Entity = new ClassElectionCandidate
        {
            Id = Guid.NewGuid(),
            ElectionId = election.Id,
            StudentId = candidate2.Id,
            CreatedAt = DateTime.UtcNow.AddSeconds(1),
        };
        db.ClassElectionCandidates.Add(cand2Entity);
        db.SaveChanges();

        // No votes — tie at 0-0; candidate1 wins (earliest CreatedAt)
        var result = await svc.FinalizeElectionAsync(election.Id);

        Assert.AreEqual(candidate1, result!.WinnerStudentId);
    }

    // ── No votes: initiating candidate wins ──────────────────────────────────

    [TestMethod]
    public async Task FinalizeElection_InitiatorWins_WhenNoVotesCast()
    {
        var (studentId, classId, db) = SeedEnrolledStudent("nv@test.com");
        var svc = CreateService(db);
        var election = await svc.StartElectionAsync(studentId, new StartElectionRequestDto(classId));

        var result = await svc.FinalizeElectionAsync(election.Id);

        Assert.AreEqual(studentId, result!.WinnerStudentId);
    }

    // ── Already completed election returns existing result ───────────────────

    [TestMethod]
    public async Task FinalizeElection_ReturnsExistingResult_WhenAlreadyCompleted()
    {
        var (studentId, classId, db) = SeedEnrolledStudent("done@test.com");
        var svc = CreateService(db);
        var election = await svc.StartElectionAsync(studentId, new StartElectionRequestDto(classId));

        var first  = await svc.FinalizeElectionAsync(election.Id);
        var second = await svc.FinalizeElectionAsync(election.Id);

        Assert.AreEqual(first!.WinnerStudentId, second!.WinnerStudentId);
        Assert.AreEqual("Completed", second.Status);
    }
}
