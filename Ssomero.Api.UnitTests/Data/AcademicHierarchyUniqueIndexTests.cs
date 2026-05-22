using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Implementations;

namespace Ssomero.Api.Data.UnitTests;

/// <summary>
/// Phase 3 — Verifies unique-index EF model configuration and the AcademicDuplicateAuditService.
/// InMemoryDatabase does not enforce unique constraints at the DB level; tests validate model metadata.
/// </summary>
[TestClass]
public class AcademicHierarchyUniqueIndexTests
{
    private static SsomeroDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SsomeroDbContext(opts);
    }

    // ?? Unique index model metadata ???????????????????????????????????????

    [TestMethod]
    public void University_HasUniqueIndex_OnName()
    {
        using var db = CreateDb();
        var et = db.Model.FindEntityType(typeof(University))!;
        Assert.IsTrue(et.GetIndexes().Any(i => i.IsUnique && i.Properties.Any(p => p.Name == nameof(University.Name))),
            "Expected unique index on University.Name");
    }

    [TestMethod]
    public void Faculty_HasUniqueIndex_OnNameAndUniversityId()
    {
        using var db = CreateDb();
        var et = db.Model.FindEntityType(typeof(Faculty))!;
        Assert.IsTrue(et.GetIndexes().Any(i => i.IsUnique
            && i.Properties.Any(p => p.Name == nameof(Faculty.Name))
            && i.Properties.Any(p => p.Name == nameof(Faculty.UniversityId))),
            "Expected unique index on Faculty(Name, UniversityId)");
    }

    [TestMethod]
    public void Department_HasUniqueIndex_OnNameAndFacultyId()
    {
        using var db = CreateDb();
        var et = db.Model.FindEntityType(typeof(Department))!;
        Assert.IsTrue(et.GetIndexes().Any(i => i.IsUnique
            && i.Properties.Any(p => p.Name == nameof(Department.Name))
            && i.Properties.Any(p => p.Name == nameof(Department.FacultyId))),
            "Expected unique index on Department(Name, FacultyId)");
    }

    [TestMethod]
    public void AcademicProgram_HasUniqueIndex_OnNameAndDepartmentId()
    {
        using var db = CreateDb();
        var et = db.Model.FindEntityType(typeof(AcademicProgram))!;
        Assert.IsTrue(et.GetIndexes().Any(i => i.IsUnique
            && i.Properties.Any(p => p.Name == nameof(AcademicProgram.Name))
            && i.Properties.Any(p => p.Name == nameof(AcademicProgram.DepartmentId))),
            "Expected unique index on AcademicProgram(Name, DepartmentId)");
    }

    [TestMethod]
    public void Curriculum_HasUniqueIndex_OnCourseCodeAndProgramId()
    {
        using var db = CreateDb();
        var et = db.Model.FindEntityType(typeof(Curriculum))!;
        Assert.IsTrue(et.GetIndexes().Any(i => i.IsUnique
            && i.Properties.Any(p => p.Name == nameof(Curriculum.CourseCode))
            && i.Properties.Any(p => p.Name == nameof(Curriculum.ProgramId))),
            "Expected unique index on Curriculum(CourseCode, ProgramId)");
    }

    // ?? DeleteBehavior.Restrict ???????????????????????????????????????????

    [TestMethod]
    public void Faculty_University_FK_IsRestrict()
    {
        using var db = CreateDb();
        var fk = db.Model.FindEntityType(typeof(Faculty))!
            .GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(University));
        Assert.AreEqual(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [TestMethod]
    public void Department_Faculty_FK_IsRestrict()
    {
        using var db = CreateDb();
        var fk = db.Model.FindEntityType(typeof(Department))!
            .GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Faculty));
        Assert.AreEqual(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [TestMethod]
    public void AcademicProgram_Department_FK_IsRestrict()
    {
        using var db = CreateDb();
        var fk = db.Model.FindEntityType(typeof(AcademicProgram))!
            .GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Department));
        Assert.AreEqual(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [TestMethod]
    public void Curriculum_Program_FK_IsRestrict()
    {
        using var db = CreateDb();
        var fk = db.Model.FindEntityType(typeof(Curriculum))!
            .GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(AcademicProgram));
        Assert.AreEqual(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    // ?? AcademicDuplicateAuditService ?????????????????????????????????????

    [TestMethod]
    public async Task DuplicateAudit_EmptyDatabase_NoReport()
    {
        using var db = CreateDb();
        var svc = new AcademicDuplicateAuditService(db, NullLogger<AcademicDuplicateAuditService>.Instance);
        var report = await svc.ReportDuplicatesAsync();
        Assert.IsFalse(report.HasDuplicates);
    }

    [TestMethod]
    public async Task DuplicateAudit_NoDuplicates_CleanDatabase_NoReport()
    {
        using var db = CreateDb();
        var uid = Guid.NewGuid();
        db.Universities.Add(new University { Id = uid, Name = "Makerere" });
        db.Faculties.Add(new Faculty { Id = Guid.NewGuid(), Name = "Science", UniversityId = uid });
        await db.SaveChangesAsync();

        var svc = new AcademicDuplicateAuditService(db, NullLogger<AcademicDuplicateAuditService>.Instance);
        var report = await svc.ReportDuplicatesAsync();
        Assert.IsFalse(report.HasDuplicates);
    }

    [TestMethod]
    public async Task DuplicateAudit_DuplicateUniversities_Reported()
    {
        using var db = CreateDb();
        db.Universities.Add(new University { Id = Guid.NewGuid(), Name = "Makerere" });
        db.Universities.Add(new University { Id = Guid.NewGuid(), Name = "Makerere" });
        db.Universities.Add(new University { Id = Guid.NewGuid(), Name = "Kyambogo" });
        await db.SaveChangesAsync();

        var svc = new AcademicDuplicateAuditService(db, NullLogger<AcademicDuplicateAuditService>.Instance);
        var report = await svc.ReportDuplicatesAsync();

        Assert.IsTrue(report.HasDuplicates);
        Assert.AreEqual(1, report.DuplicateUniversities.Count);
        Assert.AreEqual(2, report.DuplicateUniversities[0].Count);
    }

    [TestMethod]
    public async Task DuplicateAudit_DuplicateFaculties_SameUniversity_Reported()
    {
        using var db = CreateDb();
        var uid = Guid.NewGuid();
        db.Universities.Add(new University { Id = uid, Name = "Uni" });
        db.Faculties.Add(new Faculty { Id = Guid.NewGuid(), Name = "Science", UniversityId = uid });
        db.Faculties.Add(new Faculty { Id = Guid.NewGuid(), Name = "Science", UniversityId = uid });
        await db.SaveChangesAsync();

        var svc = new AcademicDuplicateAuditService(db, NullLogger<AcademicDuplicateAuditService>.Instance);
        var report = await svc.ReportDuplicatesAsync();

        Assert.AreEqual(1, report.DuplicateFaculties.Count);
    }

    [TestMethod]
    public async Task DuplicateAudit_SameFacultyName_DifferentUniversities_NotDuplicate()
    {
        using var db = CreateDb();
        var u1 = Guid.NewGuid(); var u2 = Guid.NewGuid();
        db.Universities.AddRange(
            new University { Id = u1, Name = "Uni1" },
            new University { Id = u2, Name = "Uni2" });
        db.Faculties.Add(new Faculty { Id = Guid.NewGuid(), Name = "Science", UniversityId = u1 });
        db.Faculties.Add(new Faculty { Id = Guid.NewGuid(), Name = "Science", UniversityId = u2 });
        await db.SaveChangesAsync();

        var svc = new AcademicDuplicateAuditService(db, NullLogger<AcademicDuplicateAuditService>.Instance);
        var report = await svc.ReportDuplicatesAsync();

        Assert.AreEqual(0, report.DuplicateFaculties.Count,
            "Same name in different universities is not a duplicate.");
    }

    [TestMethod]
    public async Task DuplicateAudit_DuplicateCurricula_SameCodeAndProgram_Reported()
    {
        using var db = CreateDb();
        var uid = Guid.NewGuid(); var fid = Guid.NewGuid();
        var did = Guid.NewGuid(); var pid = Guid.NewGuid(); var sid = Guid.NewGuid();
        db.Universities.Add(new University { Id = uid, Name = "Uni" });
        db.Faculties.Add(new Faculty { Id = fid, Name = "Fac", UniversityId = uid });
        db.Departments.Add(new Department { Id = did, Name = "Dept", FacultyId = fid });
        db.Programs.Add(new AcademicProgram { Id = pid, Name = "Prog", DepartmentId = did, DurationSemesters = 8 });
        db.Semesters.Add(new Semester { Id = sid, Name = "Sem1", Number = 1 });
        db.Curricula.Add(new Curriculum { Id = Guid.NewGuid(), ProgramId = pid, SemesterId = sid, CourseCode = "CS101", CourseName = "Intro", YearOfStudy = 1 });
        db.Curricula.Add(new Curriculum { Id = Guid.NewGuid(), ProgramId = pid, SemesterId = sid, CourseCode = "CS101", CourseName = "Intro Dup", YearOfStudy = 1 });
        await db.SaveChangesAsync();

        var svc = new AcademicDuplicateAuditService(db, NullLogger<AcademicDuplicateAuditService>.Instance);
        var report = await svc.ReportDuplicatesAsync();

        Assert.AreEqual(1, report.DuplicateCurricula.Count);
    }
}
