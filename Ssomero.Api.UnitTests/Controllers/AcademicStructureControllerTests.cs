using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Controllers.v1.Admin;
using Ssomero.Api.Data;
using Ssomero.Api.DTOs.Common;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers.UnitTests;

[TestClass]
public class AcademicStructureControllerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static SsomeroDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SsomeroDbContext(options);
    }

    private static AcademicStructureController CreateController(SsomeroDbContext db)
    {
        var audit = new Mock<IAuditLogService>();
        audit.Setup(a => a.LogAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        return new AcademicStructureController(db, audit.Object, NullLogger<AcademicStructureController>.Instance);
    }

    private static async Task<(University uni, Faculty fac, Department dept, AcademicProgram prog, Semester sem)> SeedFullHierarchyAsync(SsomeroDbContext db)
    {
        var uni = new University { Id = Guid.NewGuid(), Name = "Test University" };
        var fac = new Faculty { Id = Guid.NewGuid(), Name = "Test Faculty", UniversityId = uni.Id };
        var dept = new Department { Id = Guid.NewGuid(), Name = "Test Department", FacultyId = fac.Id };
        var prog = new AcademicProgram { Id = Guid.NewGuid(), Name = "Test Program", DepartmentId = dept.Id, DurationSemesters = 8 };
        var sem = new Semester { Id = Guid.NewGuid(), Name = "Semester 1", Number = 1 };

        db.Universities.Add(uni);
        db.Faculties.Add(fac);
        db.Departments.Add(dept);
        db.Programs.Add(prog);
        db.Semesters.Add(sem);
        await db.SaveChangesAsync();

        return (uni, fac, dept, prog, sem);
    }

    // ═══════════════════════════ PROGRAMS ════════════════════════════════════

    [TestMethod]
    public async Task GetPrograms_NoFilter_ReturnsAll()
    {
        await using var db = CreateDb();
        var (_, _, dept, _, _) = await SeedFullHierarchyAsync(db);
        db.Programs.Add(new AcademicProgram { Id = Guid.NewGuid(), Name = "Second Program", DepartmentId = dept.Id, DurationSemesters = 4 });
        await db.SaveChangesAsync();

        var result = await CreateController(db).GetPrograms(ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var page = ok.Value as PaginatedResponse<ProgramDto>;
        Assert.IsNotNull(page);
        Assert.AreEqual(2, page.TotalCount);
    }

    [TestMethod]
    public async Task GetPrograms_WithDepartmentFilter_ReturnsFiltered()
    {
        await using var db = CreateDb();
        var (uni, fac, dept, _, _) = await SeedFullHierarchyAsync(db);
        var dept2 = new Department { Id = Guid.NewGuid(), Name = "Other Dept", FacultyId = fac.Id };
        db.Departments.Add(dept2);
        db.Programs.Add(new AcademicProgram { Id = Guid.NewGuid(), Name = "Other Program", DepartmentId = dept2.Id, DurationSemesters = 4 });
        await db.SaveChangesAsync();

        var result = await CreateController(db).GetPrograms(departmentId: dept.Id, ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var page = ok.Value as PaginatedResponse<ProgramDto>;
        Assert.IsNotNull(page);
        Assert.AreEqual(1, page.TotalCount);
        Assert.AreEqual("Test Program", page.Data.First().Name);
    }

    [TestMethod]
    public async Task GetPrograms_IncludesHierarchyNames()
    {
        await using var db = CreateDb();
        await SeedFullHierarchyAsync(db);

        var result = await CreateController(db).GetPrograms(ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var page = ok.Value as PaginatedResponse<ProgramDto>;
        Assert.IsNotNull(page);
        var dto = page.Data.First();
        Assert.AreEqual("Test Department", dto.DepartmentName);
        Assert.AreEqual("Test Faculty", dto.FacultyName);
        Assert.AreEqual("Test University", dto.UniversityName);
    }

    [TestMethod]
    public async Task CreateProgram_HappyPath_Returns201()
    {
        await using var db = CreateDb();
        var (_, _, dept, _, _) = await SeedFullHierarchyAsync(db);

        var req = new CreateProgramRequest("New Program", dept.Id, 6);
        var result = await CreateController(db).CreateProgram(req, CancellationToken.None);

        var created = result as CreatedAtActionResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(1, await db.Programs.CountAsync(p => p.Name == "New Program"));
    }

    [TestMethod]
    public async Task CreateProgram_DuplicateName_Returns409()
    {
        await using var db = CreateDb();
        var (_, _, dept, _, _) = await SeedFullHierarchyAsync(db);

        var req = new CreateProgramRequest("Test Program", dept.Id, 6);
        var result = await CreateController(db).CreateProgram(req, CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    [TestMethod]
    public async Task CreateProgram_InvalidDepartment_Returns400()
    {
        await using var db = CreateDb();
        await SeedFullHierarchyAsync(db);

        var req = new CreateProgramRequest("New Program", Guid.NewGuid(), 6);
        var result = await CreateController(db).CreateProgram(req, CancellationToken.None);

        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad);
    }

    [TestMethod]
    public async Task UpdateProgram_HappyPath_ReturnsOk()
    {
        await using var db = CreateDb();
        var (_, _, dept, prog, _) = await SeedFullHierarchyAsync(db);

        var req = new UpdateProgramRequest("Renamed Program", dept.Id, 10);
        var result = await CreateController(db).UpdateProgram(prog.Id, req, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var dto = ok.Value as ProgramDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual("Renamed Program", dto.Name);
    }

    [TestMethod]
    public async Task UpdateProgram_NotFound_Returns404()
    {
        await using var db = CreateDb();
        var (_, _, dept, _, _) = await SeedFullHierarchyAsync(db);

        var result = await CreateController(db).UpdateProgram(Guid.NewGuid(), new UpdateProgramRequest("X", dept.Id, 4), CancellationToken.None);

        var notFound = result as NotFoundResult;
        Assert.IsNotNull(notFound);
    }

    [TestMethod]
    public async Task UpdateProgram_DuplicateName_Returns409()
    {
        await using var db = CreateDb();
        var (_, _, dept, prog, _) = await SeedFullHierarchyAsync(db);
        db.Programs.Add(new AcademicProgram { Id = Guid.NewGuid(), Name = "Existing Program", DepartmentId = dept.Id, DurationSemesters = 4 });
        await db.SaveChangesAsync();

        var req = new UpdateProgramRequest("Existing Program", dept.Id, 8);
        var result = await CreateController(db).UpdateProgram(prog.Id, req, CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    [TestMethod]
    public async Task DeleteProgram_HappyPath_ReturnsOk()
    {
        await using var db = CreateDb();
        var (_, _, dept, _, _) = await SeedFullHierarchyAsync(db);
        var emptyProg = new AcademicProgram { Id = Guid.NewGuid(), Name = "Empty Program", DepartmentId = dept.Id, DurationSemesters = 4 };
        db.Programs.Add(emptyProg);
        await db.SaveChangesAsync();

        var result = await CreateController(db).DeleteProgram(emptyProg.Id, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.IsFalse(await db.Programs.AnyAsync(p => p.Id == emptyProg.Id));
    }

    [TestMethod]
    public async Task DeleteProgram_WithCurriculumEntries_Returns409()
    {
        await using var db = CreateDb();
        var (_, _, _, prog, sem) = await SeedFullHierarchyAsync(db);
        db.Curricula.Add(new Curriculum
        {
            Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS101",
            CourseName = "Intro CS", YearOfStudy = 1, SemesterId = sem.Id
        });
        await db.SaveChangesAsync();

        var result = await CreateController(db).DeleteProgram(prog.Id, CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    // ═══════════════════════════ CURRICULUM ══════════════════════════════════

    [TestMethod]
    public async Task GetCurriculum_NoFilter_ReturnsAll()
    {
        await using var db = CreateDb();
        var (_, _, _, prog, sem) = await SeedFullHierarchyAsync(db);
        db.Curricula.Add(new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS101", CourseName = "Intro CS", YearOfStudy = 1, SemesterId = sem.Id });
        db.Curricula.Add(new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS102", CourseName = "Data Structures", YearOfStudy = 1, SemesterId = sem.Id });
        await db.SaveChangesAsync();

        var result = await CreateController(db).GetCurriculum(ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var page = ok.Value as PaginatedResponse<CurriculumAdminDto>;
        Assert.IsNotNull(page);
        Assert.AreEqual(2, page.TotalCount);
    }

    [TestMethod]
    public async Task GetCurriculum_WithProgramFilter_ReturnsFiltered()
    {
        await using var db = CreateDb();
        var (_, _, dept, prog, sem) = await SeedFullHierarchyAsync(db);
        var prog2 = new AcademicProgram { Id = Guid.NewGuid(), Name = "Prog2", DepartmentId = dept.Id, DurationSemesters = 4 };
        db.Programs.Add(prog2);
        db.Curricula.Add(new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS101", CourseName = "Intro CS", YearOfStudy = 1, SemesterId = sem.Id });
        db.Curricula.Add(new Curriculum { Id = Guid.NewGuid(), ProgramId = prog2.Id, CourseCode = "BUS101", CourseName = "Business Intro", YearOfStudy = 1, SemesterId = sem.Id });
        await db.SaveChangesAsync();

        var result = await CreateController(db).GetCurriculum(programId: prog.Id, ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var page = ok.Value as PaginatedResponse<CurriculumAdminDto>;
        Assert.IsNotNull(page);
        Assert.AreEqual(1, page.TotalCount);
        Assert.AreEqual("CS101", page.Data.First().CourseCode);
    }

    [TestMethod]
    public async Task GetCurriculum_IncludesHierarchyNames()
    {
        await using var db = CreateDb();
        var (_, _, _, prog, sem) = await SeedFullHierarchyAsync(db);
        db.Curricula.Add(new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS101", CourseName = "Intro CS", YearOfStudy = 1, SemesterId = sem.Id });
        await db.SaveChangesAsync();

        var result = await CreateController(db).GetCurriculum(ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var page = ok.Value as PaginatedResponse<CurriculumAdminDto>;
        Assert.IsNotNull(page);
        var dto = page.Data.First();
        Assert.AreEqual("Test Program", dto.ProgramName);
        Assert.AreEqual("Test Department", dto.DepartmentName);
        Assert.AreEqual("Test Faculty", dto.FacultyName);
        Assert.AreEqual("Test University", dto.UniversityName);
    }

    [TestMethod]
    public async Task CreateCurriculumEntry_HappyPath_Returns201()
    {
        await using var db = CreateDb();
        var (_, _, _, prog, sem) = await SeedFullHierarchyAsync(db);

        var req = new CreateCurriculumRequest(prog.Id, 1, sem.Id, "CS201", "Algorithms");
        var result = await CreateController(db).CreateCurriculumEntry(req, CancellationToken.None);

        var created = result as CreatedAtActionResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(1, await db.Curricula.CountAsync(c => c.CourseCode == "CS201"));
    }

    [TestMethod]
    public async Task CreateCurriculumEntry_DuplicateCourseCode_Returns409()
    {
        await using var db = CreateDb();
        var (_, _, _, prog, sem) = await SeedFullHierarchyAsync(db);
        db.Curricula.Add(new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS201", CourseName = "Algorithms", YearOfStudy = 1, SemesterId = sem.Id });
        await db.SaveChangesAsync();

        var req = new CreateCurriculumRequest(prog.Id, 1, sem.Id, "CS201", "Dup Algorithms");
        var result = await CreateController(db).CreateCurriculumEntry(req, CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    [TestMethod]
    public async Task CreateCurriculumEntry_InvalidProgram_Returns400()
    {
        await using var db = CreateDb();
        var (_, _, _, _, sem) = await SeedFullHierarchyAsync(db);

        var req = new CreateCurriculumRequest(Guid.NewGuid(), 1, sem.Id, "CS201", "Algorithms");
        var result = await CreateController(db).CreateCurriculumEntry(req, CancellationToken.None);

        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad);
    }

    [TestMethod]
    public async Task UpdateCurriculumEntry_HappyPath_ReturnsOk()
    {
        await using var db = CreateDb();
        var (_, _, _, prog, sem) = await SeedFullHierarchyAsync(db);
        var entry = new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS101", CourseName = "Intro", YearOfStudy = 1, SemesterId = sem.Id };
        db.Curricula.Add(entry);
        await db.SaveChangesAsync();

        var req = new UpdateCurriculumRequest(prog.Id, 2, sem.Id, "CS101", "Intro CS Updated");
        var result = await CreateController(db).UpdateCurriculumEntry(entry.Id, req, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var dto = ok.Value as CurriculumAdminDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual("Intro CS Updated", dto.CourseName);
    }

    [TestMethod]
    public async Task UpdateCurriculumEntry_DuplicateCourseCode_Returns409()
    {
        await using var db = CreateDb();
        var (_, _, _, prog, sem) = await SeedFullHierarchyAsync(db);
        var entry1 = new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS101", CourseName = "Intro", YearOfStudy = 1, SemesterId = sem.Id };
        var entry2 = new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS102", CourseName = "OOP", YearOfStudy = 1, SemesterId = sem.Id };
        db.Curricula.AddRange(entry1, entry2);
        await db.SaveChangesAsync();

        // Try to rename entry2's CourseCode to CS101 (already taken)
        var req = new UpdateCurriculumRequest(prog.Id, 1, sem.Id, "CS101", "OOP");
        var result = await CreateController(db).UpdateCurriculumEntry(entry2.Id, req, CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    [TestMethod]
    public async Task DeleteCurriculumEntry_HappyPath_ReturnsOk()
    {
        await using var db = CreateDb();
        var (_, _, _, prog, sem) = await SeedFullHierarchyAsync(db);
        var entry = new Curriculum { Id = Guid.NewGuid(), ProgramId = prog.Id, CourseCode = "CS101", CourseName = "Intro", YearOfStudy = 1, SemesterId = sem.Id };
        db.Curricula.Add(entry);
        await db.SaveChangesAsync();

        var result = await CreateController(db).DeleteCurriculumEntry(entry.Id, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.IsFalse(await db.Curricula.AnyAsync(c => c.Id == entry.Id));
    }

    [TestMethod]
    public async Task DeleteCurriculumEntry_NotFound_Returns404()
    {
        await using var db = CreateDb();

        var result = await CreateController(db).DeleteCurriculumEntry(Guid.NewGuid(), CancellationToken.None);

        var notFound = result as NotFoundResult;
        Assert.IsNotNull(notFound);
    }

    // ═══════════════════════════ DEPARTMENT UPDATE ═══════════════════════════

    [TestMethod]
    public async Task UpdateDepartment_HappyPath_ReturnsOk()
    {
        await using var db = CreateDb();
        var (_, fac, dept, _, _) = await SeedFullHierarchyAsync(db);

        var req = new UpdateDepartmentRequest("Renamed Department", fac.Id);
        var result = await CreateController(db).UpdateDepartment(dept.Id, req, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var dto = ok.Value as DepartmentDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual("Renamed Department", dto.Name);
        Assert.AreEqual("Test Faculty", dto.FacultyName);
        Assert.AreEqual("Test University", dto.UniversityName);
    }

    [TestMethod]
    public async Task UpdateDepartment_NotFound_Returns404()
    {
        await using var db = CreateDb();
        var (_, fac, _, _, _) = await SeedFullHierarchyAsync(db);

        var result = await CreateController(db).UpdateDepartment(Guid.NewGuid(), new UpdateDepartmentRequest("X", fac.Id), CancellationToken.None);

        var notFound = result as NotFoundResult;
        Assert.IsNotNull(notFound);
    }

    [TestMethod]
    public async Task UpdateDepartment_DuplicateName_Returns409()
    {
        await using var db = CreateDb();
        var (_, fac, dept, _, _) = await SeedFullHierarchyAsync(db);
        db.Departments.Add(new Department { Id = Guid.NewGuid(), Name = "Existing Dept", FacultyId = fac.Id });
        await db.SaveChangesAsync();

        var result = await CreateController(db).UpdateDepartment(dept.Id, new UpdateDepartmentRequest("Existing Dept", fac.Id), CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    [TestMethod]
    public async Task DeleteDepartment_WithPrograms_Returns409()
    {
        await using var db = CreateDb();
        var (_, _, dept, _, _) = await SeedFullHierarchyAsync(db);

        // The seeded dept already has a program
        var result = await CreateController(db).DeleteDepartment(dept.Id, CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    [TestMethod]
    public async Task DeleteDepartment_Empty_ReturnsOk()
    {
        await using var db = CreateDb();
        var (_, fac, _, _, _) = await SeedFullHierarchyAsync(db);
        var emptyDept = new Department { Id = Guid.NewGuid(), Name = "Empty Dept", FacultyId = fac.Id };
        db.Departments.Add(emptyDept);
        await db.SaveChangesAsync();

        var result = await CreateController(db).DeleteDepartment(emptyDept.Id, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.IsFalse(await db.Departments.AnyAsync(d => d.Id == emptyDept.Id));
    }

    // ═══════════════════════════ UNIVERSITY DELETE GUARD ═════════════════════

    [TestMethod]
    public async Task DeleteUniversity_WithFaculties_Returns409()
    {
        await using var db = CreateDb();
        var (uni, _, _, _, _) = await SeedFullHierarchyAsync(db);

        var result = await CreateController(db).DeleteUniversity(uni.Id, CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    [TestMethod]
    public async Task DeleteUniversity_Empty_ReturnsOk()
    {
        await using var db = CreateDb();
        var emptyUni = new University { Id = Guid.NewGuid(), Name = "Empty Uni" };
        await using var db2 = CreateDb();
        db2.Universities.Add(emptyUni);
        await db2.SaveChangesAsync();

        var result = await CreateController(db2).DeleteUniversity(emptyUni.Id, CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
    }

    // ═══════════════════════════ FACULTY DELETE GUARD ════════════════════════

    [TestMethod]
    public async Task DeleteFaculty_WithDepartments_Returns409()
    {
        await using var db = CreateDb();
        var (_, fac, _, _, _) = await SeedFullHierarchyAsync(db);

        var result = await CreateController(db).DeleteFaculty(fac.Id, CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict);
    }

    // ═══════════════════════════ PAGINATION / SEARCH ═════════════════════════

    [TestMethod]
    public async Task GetPrograms_Search_FiltersResults()
    {
        await using var db = CreateDb();
        var (_, _, dept, _, _) = await SeedFullHierarchyAsync(db);
        db.Programs.Add(new AcademicProgram { Id = Guid.NewGuid(), Name = "Biology Science", DepartmentId = dept.Id, DurationSemesters = 8 });
        await db.SaveChangesAsync();

        var result = await CreateController(db).GetPrograms(search: "Biology", ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var page = ok.Value as PaginatedResponse<ProgramDto>;
        Assert.IsNotNull(page);
        Assert.AreEqual(1, page.TotalCount);
    }

    [TestMethod]
    public async Task GetPrograms_Pagination_RespectsPageSize()
    {
        await using var db = CreateDb();
        var (_, _, dept, _, _) = await SeedFullHierarchyAsync(db);
        for (int i = 0; i < 5; i++)
            db.Programs.Add(new AcademicProgram { Id = Guid.NewGuid(), Name = $"Program {i}", DepartmentId = dept.Id, DurationSemesters = 4 });
        await db.SaveChangesAsync();

        var result = await CreateController(db).GetPrograms(page: 1, pageSize: 3, ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var page = ok.Value as PaginatedResponse<ProgramDto>;
        Assert.IsNotNull(page);
        Assert.AreEqual(3, page.Data.Count());
        Assert.AreEqual(6, page.TotalCount); // 5 + 1 seeded
    }
}
