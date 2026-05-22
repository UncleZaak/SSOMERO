using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.DTOs.Common;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Controllers.v1.Admin;

[ApiController]
[Route("api/v1/admin/academic")]
[Authorize(Roles = "Admin")]
public class AcademicStructureController : ControllerBase
{
    private readonly SsomeroDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly ILogger<AcademicStructureController> _logger;

    public AcademicStructureController(
        SsomeroDbContext db,
        IAuditLogService audit,
        ILogger<AcademicStructureController> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    // ───────────────────────── Universities ─────────────────────────

    [HttpGet("universities")]
    public async Task<IActionResult> GetUniversities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = _db.Universities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Name.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UniversityDetailDto(u.Id, u.Name, u.Faculties.Count, "Active"))
            .ToListAsync(ct);

        return Ok(new PaginatedResponse<UniversityDetailDto> { Data = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    [HttpPost("universities")]
    public async Task<IActionResult> CreateUniversity([FromBody] CreateUniversityRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var name = req.Name.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Name is required." });

        var exists = await _db.Universities.AnyAsync(u => u.Name == name, ct);
        if (exists) return Conflict(new { message = "A university with this name already exists." });

        var university = new University { Id = Guid.NewGuid(), Name = name };
        _db.Universities.Add(university);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", nameof(University), university.Id.ToString(), newValues: name);
        return CreatedAtAction(nameof(GetUniversities), new { }, new UniversityDetailDto(university.Id, university.Name, 0, "Active"));
    }

    [HttpPut("universities/{id:guid}")]
    public async Task<IActionResult> UpdateUniversity(Guid id, [FromBody] UpdateUniversityRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var university = await _db.Universities.FindAsync([id], ct);
        if (university is null) return NotFound();

        var name = req.Name.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Name is required." });

        var duplicate = await _db.Universities.AnyAsync(u => u.Name == name && u.Id != id, ct);
        if (duplicate) return Conflict(new { message = "A university with this name already exists." });

        var old = university.Name;
        university.Name = name;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Update", nameof(University), id.ToString(), old, name);
        return Ok(new UniversityDetailDto(university.Id, university.Name, 0, "Active"));
    }

    [HttpDelete("universities/{id:guid}")]
    public async Task<IActionResult> DeleteUniversity(Guid id, CancellationToken ct = default)
    {
        var university = await _db.Universities.Include(u => u.Faculties).FirstOrDefaultAsync(u => u.Id == id, ct);
        if (university is null) return NotFound();
        if (university.Faculties.Count > 0)
            return Conflict(new { message = "Cannot delete a university that has faculties. Remove faculties first." });

        _db.Universities.Remove(university);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Delete", nameof(University), id.ToString());
        return Ok(new { message = "University deleted." });
    }

    // ───────────────────────── Faculties ─────────────────────────

    [HttpGet("faculties")]
    public async Task<IActionResult> GetFaculties(
        [FromQuery] Guid? universityId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = _db.Faculties.Include(f => f.University).AsQueryable();

        if (universityId.HasValue)
            query = query.Where(f => f.UniversityId == universityId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.Name.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FacultyDetailDto(f.Id, f.Name, f.UniversityId, f.University.Name, f.Departments.Count, "Active"))
            .ToListAsync(ct);

        return Ok(new PaginatedResponse<FacultyDetailDto> { Data = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    [HttpPost("faculties")]
    public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var name = req.Name.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Name is required." });

        var uniExists = await _db.Universities.AnyAsync(u => u.Id == req.UniversityId, ct);
        if (!uniExists) return BadRequest(new { message = "University not found." });

        var duplicate = await _db.Faculties.AnyAsync(f => f.Name == name && f.UniversityId == req.UniversityId, ct);
        if (duplicate) return Conflict(new { message = "A faculty with this name already exists in that university." });

        var faculty = new Faculty { Id = Guid.NewGuid(), Name = name, UniversityId = req.UniversityId };
        _db.Faculties.Add(faculty);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", nameof(Faculty), faculty.Id.ToString(), newValues: name);

        var uni = await _db.Universities.FindAsync([req.UniversityId], ct);
        return CreatedAtAction(nameof(GetFaculties), new { }, new FacultyDetailDto(faculty.Id, faculty.Name, faculty.UniversityId, uni!.Name, 0, "Active"));
    }

    [HttpPut("faculties/{id:guid}")]
    public async Task<IActionResult> UpdateFaculty(Guid id, [FromBody] UpdateFacultyRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var faculty = await _db.Faculties.Include(f => f.University).FirstOrDefaultAsync(f => f.Id == id, ct);
        if (faculty is null) return NotFound();

        var name = req.Name.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Name is required." });

        var uniExists = await _db.Universities.AnyAsync(u => u.Id == req.UniversityId, ct);
        if (!uniExists) return BadRequest(new { message = "University not found." });

        var duplicate = await _db.Faculties.AnyAsync(f => f.Name == name && f.UniversityId == req.UniversityId && f.Id != id, ct);
        if (duplicate) return Conflict(new { message = "A faculty with this name already exists in that university." });

        var old = faculty.Name;
        faculty.Name = name;
        faculty.UniversityId = req.UniversityId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Update", nameof(Faculty), id.ToString(), old, name);

        var uni = faculty.University ?? await _db.Universities.FindAsync([req.UniversityId], ct);
        return Ok(new FacultyDetailDto(faculty.Id, faculty.Name, faculty.UniversityId, uni!.Name, 0, "Active"));
    }

    [HttpDelete("faculties/{id:guid}")]
    public async Task<IActionResult> DeleteFaculty(Guid id, CancellationToken ct = default)
    {
        var faculty = await _db.Faculties.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == id, ct);
        if (faculty is null) return NotFound();
        if (faculty.Departments.Count > 0)
            return Conflict(new { message = "Cannot delete a faculty that has departments. Remove departments first." });

        _db.Faculties.Remove(faculty);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Delete", nameof(Faculty), id.ToString());
        return Ok(new { message = "Faculty deleted." });
    }

    // ───────────────────────── Departments ─────────────────────────

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments(
        [FromQuery] Guid? facultyId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = _db.Departments
            .Include(d => d.Faculty).ThenInclude(f => f.University)
            .AsQueryable();

        if (facultyId.HasValue)
            query = query.Where(d => d.FacultyId == facultyId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DepartmentDto(
                d.Id, d.Name,
                d.FacultyId, d.Faculty.Name,
                d.Faculty.UniversityId, d.Faculty.University.Name))
            .ToListAsync(ct);

        return Ok(new PaginatedResponse<DepartmentDto> { Data = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var name = req.Name.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Name is required." });

        var faculty = await _db.Faculties.Include(f => f.University).FirstOrDefaultAsync(f => f.Id == req.FacultyId, ct);
        if (faculty is null) return BadRequest(new { message = "Faculty not found." });

        var duplicate = await _db.Departments.AnyAsync(d => d.Name == name && d.FacultyId == req.FacultyId, ct);
        if (duplicate) return Conflict(new { message = "A department with this name already exists in that faculty." });

        var dept = new Department { Id = Guid.NewGuid(), Name = name, FacultyId = req.FacultyId };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", nameof(Department), dept.Id.ToString(), newValues: name);

        return CreatedAtAction(nameof(GetDepartments), new { },
            new DepartmentDto(dept.Id, dept.Name, faculty.Id, faculty.Name, faculty.UniversityId, faculty.University.Name));
    }

    [HttpPut("departments/{id:guid}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var dept = await _db.Departments.FindAsync([id], ct);
        if (dept is null) return NotFound();

        var name = req.Name.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Name is required." });

        var faculty = await _db.Faculties.Include(f => f.University).FirstOrDefaultAsync(f => f.Id == req.FacultyId, ct);
        if (faculty is null) return BadRequest(new { message = "Faculty not found." });

        var duplicate = await _db.Departments.AnyAsync(d => d.Name == name && d.FacultyId == req.FacultyId && d.Id != id, ct);
        if (duplicate) return Conflict(new { message = "A department with this name already exists in that faculty." });

        var old = dept.Name;
        dept.Name = name;
        dept.FacultyId = req.FacultyId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Update", nameof(Department), id.ToString(), old, name);

        return Ok(new DepartmentDto(dept.Id, dept.Name, faculty.Id, faculty.Name, faculty.UniversityId, faculty.University.Name));
    }

    [HttpDelete("departments/{id:guid}")]
    public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken ct = default)
    {
        var dept = await _db.Departments.Include(d => d.Programs).FirstOrDefaultAsync(d => d.Id == id, ct);
        if (dept is null) return NotFound();
        if (dept.Programs.Count > 0)
            return Conflict(new { message = "Cannot delete a department that has programs. Remove programs first." });

        _db.Departments.Remove(dept);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Delete", nameof(Department), id.ToString());
        return Ok(new { message = "Department deleted." });
    }

    // ───────────────────────── Programs ─────────────────────────

    [HttpGet("programs")]
    public async Task<IActionResult> GetPrograms(
        [FromQuery] Guid? departmentId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = _db.Programs
            .Include(p => p.Department).ThenInclude(d => d.Faculty).ThenInclude(f => f.University)
            .AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(p => p.DepartmentId == departmentId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProgramDto(
                p.Id, p.Name, p.DurationSemesters,
                p.DepartmentId, p.Department.Name,
                p.Department.FacultyId, p.Department.Faculty.Name,
                p.Department.Faculty.UniversityId, p.Department.Faculty.University.Name))
            .ToListAsync(ct);

        return Ok(new PaginatedResponse<ProgramDto> { Data = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    [HttpPost("programs")]
    public async Task<IActionResult> CreateProgram([FromBody] CreateProgramRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var name = req.Name.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Name is required." });

        var dept = await _db.Departments
            .Include(d => d.Faculty).ThenInclude(f => f.University)
            .FirstOrDefaultAsync(d => d.Id == req.DepartmentId, ct);
        if (dept is null) return BadRequest(new { message = "Department not found." });

        var duplicate = await _db.Programs.AnyAsync(p => p.Name == name && p.DepartmentId == req.DepartmentId, ct);
        if (duplicate) return Conflict(new { message = "A program with this name already exists in that department." });

        var prog = new AcademicProgram { Id = Guid.NewGuid(), Name = name, DepartmentId = req.DepartmentId, DurationSemesters = req.DurationSemesters };
        _db.Programs.Add(prog);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", nameof(AcademicProgram), prog.Id.ToString(), newValues: name);

        return CreatedAtAction(nameof(GetPrograms), new { },
            new ProgramDto(prog.Id, prog.Name, prog.DurationSemesters,
                dept.Id, dept.Name,
                dept.FacultyId, dept.Faculty.Name,
                dept.Faculty.UniversityId, dept.Faculty.University.Name));
    }

    [HttpPut("programs/{id:guid}")]
    public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] UpdateProgramRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var prog = await _db.Programs.FindAsync([id], ct);
        if (prog is null) return NotFound();

        var name = req.Name.Trim();
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Name is required." });

        var dept = await _db.Departments
            .Include(d => d.Faculty).ThenInclude(f => f.University)
            .FirstOrDefaultAsync(d => d.Id == req.DepartmentId, ct);
        if (dept is null) return BadRequest(new { message = "Department not found." });

        var duplicate = await _db.Programs.AnyAsync(p => p.Name == name && p.DepartmentId == req.DepartmentId && p.Id != id, ct);
        if (duplicate) return Conflict(new { message = "A program with this name already exists in that department." });

        var old = prog.Name;
        prog.Name = name;
        prog.DepartmentId = req.DepartmentId;
        prog.DurationSemesters = req.DurationSemesters;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Update", nameof(AcademicProgram), id.ToString(), old, name);

        return Ok(new ProgramDto(prog.Id, prog.Name, prog.DurationSemesters,
            dept.Id, dept.Name,
            dept.FacultyId, dept.Faculty.Name,
            dept.Faculty.UniversityId, dept.Faculty.University.Name));
    }

    [HttpDelete("programs/{id:guid}")]
    public async Task<IActionResult> DeleteProgram(Guid id, CancellationToken ct = default)
    {
        var prog = await _db.Programs.Include(p => p.CurriculumEntries).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prog is null) return NotFound();
        if (prog.CurriculumEntries.Count > 0)
            return Conflict(new { message = "Cannot delete a program that has curriculum entries. Remove curriculum entries first." });

        _db.Programs.Remove(prog);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Delete", nameof(AcademicProgram), id.ToString());
        return Ok(new { message = "Program deleted." });
    }

    // ───────────────────────── Curriculum ─────────────────────────

    [HttpGet("curriculum")]
    public async Task<IActionResult> GetCurriculum(
        [FromQuery] Guid? programId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = _db.Curricula
            .Include(c => c.Program)
                .ThenInclude(p => p.Department)
                .ThenInclude(d => d.Faculty)
                .ThenInclude(f => f.University)
            .AsQueryable();

        if (programId.HasValue)
            query = query.Where(c => c.ProgramId == programId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.CourseName.Contains(search) || c.CourseCode.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.YearOfStudy).ThenBy(c => c.CourseCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CurriculumAdminDto(
                c.Id, c.CourseCode, c.CourseName, c.YearOfStudy,
                c.ProgramId, c.Program.Name,
                c.Program.Department.Name,
                c.Program.Department.Faculty.Name,
                c.Program.Department.Faculty.University.Name))
            .ToListAsync(ct);

        return Ok(new PaginatedResponse<CurriculumAdminDto> { Data = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    [HttpPost("curriculum")]
    public async Task<IActionResult> CreateCurriculumEntry([FromBody] CreateCurriculumRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var courseCode = req.CourseCode.Trim();
        var courseName = req.CourseName.Trim();
        if (string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(courseName))
            return BadRequest(new { message = "CourseCode and CourseName are required." });

        var prog = await _db.Programs
            .Include(p => p.Department).ThenInclude(d => d.Faculty).ThenInclude(f => f.University)
            .FirstOrDefaultAsync(p => p.Id == req.ProgramId, ct);
        if (prog is null) return BadRequest(new { message = "Program not found." });

        var semExists = await _db.Semesters.AnyAsync(s => s.Id == req.SemesterId, ct);
        if (!semExists) return BadRequest(new { message = "Semester not found." });

        var duplicate = await _db.Curricula.AnyAsync(c => c.CourseCode == courseCode && c.ProgramId == req.ProgramId, ct);
        if (duplicate) return Conflict(new { message = "A curriculum entry with this course code already exists in that program." });

        var entry = new Curriculum
        {
            Id = Guid.NewGuid(),
            ProgramId = req.ProgramId,
            YearOfStudy = req.YearOfStudy,
            SemesterId = req.SemesterId,
            CourseCode = courseCode,
            CourseName = courseName
        };
        _db.Curricula.Add(entry);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", nameof(Curriculum), entry.Id.ToString(), newValues: courseCode);

        return CreatedAtAction(nameof(GetCurriculum), new { },
            new CurriculumAdminDto(entry.Id, entry.CourseCode, entry.CourseName, entry.YearOfStudy,
                prog.Id, prog.Name,
                prog.Department.Name,
                prog.Department.Faculty.Name,
                prog.Department.Faculty.University.Name));
    }

    [HttpPut("curriculum/{id:guid}")]
    public async Task<IActionResult> UpdateCurriculumEntry(Guid id, [FromBody] UpdateCurriculumRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var entry = await _db.Curricula.FindAsync([id], ct);
        if (entry is null) return NotFound();

        var courseCode = req.CourseCode.Trim();
        var courseName = req.CourseName.Trim();
        if (string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(courseName))
            return BadRequest(new { message = "CourseCode and CourseName are required." });

        var prog = await _db.Programs
            .Include(p => p.Department).ThenInclude(d => d.Faculty).ThenInclude(f => f.University)
            .FirstOrDefaultAsync(p => p.Id == req.ProgramId, ct);
        if (prog is null) return BadRequest(new { message = "Program not found." });

        var semExists = await _db.Semesters.AnyAsync(s => s.Id == req.SemesterId, ct);
        if (!semExists) return BadRequest(new { message = "Semester not found." });

        var duplicate = await _db.Curricula.AnyAsync(c => c.CourseCode == courseCode && c.ProgramId == req.ProgramId && c.Id != id, ct);
        if (duplicate) return Conflict(new { message = "A curriculum entry with this course code already exists in that program." });

        var oldCode = entry.CourseCode;
        entry.CourseCode = courseCode;
        entry.CourseName = courseName;
        entry.YearOfStudy = req.YearOfStudy;
        entry.SemesterId = req.SemesterId;
        entry.ProgramId = req.ProgramId;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Update", nameof(Curriculum), id.ToString(), oldCode, courseCode);

        return Ok(new CurriculumAdminDto(entry.Id, entry.CourseCode, entry.CourseName, entry.YearOfStudy,
            prog.Id, prog.Name,
            prog.Department.Name,
            prog.Department.Faculty.Name,
            prog.Department.Faculty.University.Name));
    }

    [HttpDelete("curriculum/{id:guid}")]
    public async Task<IActionResult> DeleteCurriculumEntry(Guid id, CancellationToken ct = default)
    {
        var entry = await _db.Curricula.FindAsync([id], ct);
        if (entry is null) return NotFound();

        _db.Curricula.Remove(entry);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("Delete", nameof(Curriculum), id.ToString());
        return Ok(new { message = "Curriculum entry deleted." });
    }
}

