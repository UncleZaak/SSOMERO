using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;
using System.Globalization;

namespace Ssomero.Api.Controllers;

/// <summary>
/// Admin-only endpoints for user moderation, lecturer approval, and class assignment.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<AdminController> _logger;
    private readonly IApiCacheService _cache;

    // Cache keys
    private const string StatsCacheKey          = "admin:stats";
    private const string AttendanceCacheKey     = "admin:attendance:summary";
    private const string TrendsCacheKeyPrefix   = "admin:analytics:trends";

    // TTLs
    private static readonly TimeSpan StatsTtl       = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan AttendanceTtl  = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan TrendsTtl      = TimeSpan.FromMinutes(10);

    public AdminController(SsomeroDbContext db, ILogger<AdminController> logger, IApiCacheService cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>GET /api/admin/lecturers/pending — list unapproved lecturers.</summary>
    [HttpGet("lecturers/pending")]
    public async Task<IActionResult> GetPendingLecturers()
    {
        var list = await _db.Lecturers
            .Where(l => !l.IsApproved && l.IsVerified)
            .Select(l => new { l.Id, l.FirstName, l.LastName, l.Email, l.StaffId, l.CreatedAt })
            .ToListAsync();
        return Ok(list);
    }

    /// <summary>GET /api/admin/students — list all students (includes soft-deleted).</summary>
    [HttpGet("students")]
    public async Task<IActionResult> GetAllStudents()
    {
        var list = await _db.Students
            .IgnoreQueryFilters()
            .Where(s => !s.IsDeleted)
            .Include(s => s.AcademicProfile!)
                .ThenInclude(ap => ap.Program)
            .Select(s => new
            {
                s.Id,
                Name = s.FirstName + " " + s.SecondName,
                s.Email,
                Role = "Student",
                Status = s.Status.ToString(),
                Program = s.AcademicProfile != null && s.AcademicProfile.Program != null
                    ? s.AcademicProfile.Program.Name : (string?)null,
                s.CreatedAt
            })
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        return Ok(list);
    }

    /// <summary>GET /api/admin/lecturers — list all lecturers (includes soft-deleted).</summary>
    [HttpGet("lecturers")]
    public async Task<IActionResult> GetAllLecturers()
    {
        var list = await _db.Lecturers
            .IgnoreQueryFilters()
            .Where(l => !l.IsDeleted)
            .Select(l => new
            {
                l.Id,
                Name = l.FirstName + " " + l.LastName,
                l.Email,
                Role = "Lecturer",
                Status = l.Status.ToString(),
                l.StaffId,
                l.IsApproved,
                l.CreatedAt
            })
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        return Ok(list);
    }

    /// <summary>POST /api/admin/lecturers/{id}/approve</summary>
    [HttpPost("lecturers/{id:guid}/approve")]
    public async Task<IActionResult> ApproveLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.FindAsync(id);
        if (lecturer is null) return NotFound();
        lecturer.IsApproved = true;
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(StatsCacheKey);
        _logger.LogInformation("Lecturer approved: {Id}", id);
        return Ok(new { message = "Lecturer approved" });
    }

    /// <summary>POST /api/admin/lecturer/assign — assign a lecturer to a subclass.</summary>
    [HttpPost("lecturer/assign")]
    public async Task<IActionResult> AssignLecturer([FromBody] AssignLecturerRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var lecturer = await _db.Lecturers.FindAsync(req.LecturerId);
        if (lecturer is null || !lecturer.IsApproved)
            return BadRequest(new { error = "Lecturer not found or not approved" });

        var cls = await _db.Classes.FindAsync(req.ClassId);
        if (cls is null) return BadRequest(new { error = "Class not found" });

        if (cls.ParentClassId == null)
            return BadRequest(new { error = "Lecturers can only be assigned to subclasses." });

        var exists = await _db.LecturerClasses.AnyAsync(lc => lc.LecturerId == req.LecturerId && lc.ClassId == req.ClassId);
        if (exists) return Conflict(new { error = "Already assigned" });

        _db.LecturerClasses.Add(new LecturerClass
        {
            LecturerId = req.LecturerId,
            ClassId = req.ClassId,
            AssignedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Lecturer {LecturerId} assigned to class {ClassId}", req.LecturerId, req.ClassId);
        return Ok(new { message = "Lecturer assigned to class" });
    }

    // ??????????????????????????????????????????????
    //  Student moderation
    // ??????????????????????????????????????????????

    /// <summary>POST /api/admin/students/{id}/suspend</summary>
    [HttpPost("students/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendStudent(Guid id)
    {
        var student = await _db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return NotFound(new { error = "Student not found" });
        if (student.IsDeleted) return BadRequest(new { error = "Cannot modify a deleted account" });
        if (student.Status == UserStatus.Suspended) return Ok(new { message = "Student is already suspended" });

        student.Status = UserStatus.Suspended;
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(StatsCacheKey);
        _logger.LogInformation("Student suspended: {Id}", id);
        return Ok(new { message = "Student suspended", status = student.Status.ToString() });
    }

    /// <summary>POST /api/admin/students/{id}/activate</summary>
    [HttpPost("students/{id:guid}/activate")]
    public async Task<IActionResult> ActivateStudent(Guid id)
    {
        var student = await _db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return NotFound(new { error = "Student not found" });
        if (student.IsDeleted) return BadRequest(new { error = "Cannot activate a deleted account" });
        if (student.Status == UserStatus.Active) return Ok(new { message = "Student is already active" });

        student.Status = UserStatus.Active;
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(StatsCacheKey);
        _logger.LogInformation("Student activated: {Id}", id);
        return Ok(new { message = "Student activated", status = student.Status.ToString() });
    }

    /// <summary>POST /api/admin/students/{id}/deactivate</summary>
    [HttpPost("students/{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateStudent(Guid id)
    {
        var student = await _db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return NotFound(new { error = "Student not found" });
        if (student.IsDeleted) return BadRequest(new { error = "Cannot modify a deleted account" });
        if (student.Status == UserStatus.Deactivated) return Ok(new { message = "Student is already deactivated" });

        student.Status = UserStatus.Deactivated;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Student deactivated: {Id}", id);
        return Ok(new { message = "Student deactivated", status = student.Status.ToString() });
    }

    /// <summary>POST /api/admin/students/{id}/delete — soft delete</summary>
    [HttpPost("students/{id:guid}/delete")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        var student = await _db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return NotFound(new { error = "Student not found" });
        if (student.IsDeleted) return Ok(new { message = "Student is already deleted" });

        // Mangle email so the DB unique index never blocks a future re-registration
        // with the same address. Pattern: deleted_<id>_<original> (max 200 chars).
        student.Email = $"deleted_{id}_{student.Email}"[..Math.Min($"deleted_{id}_{student.Email}".Length, 200)];
        student.IsDeleted = true;
        student.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _cache.RemoveManyAsync(StatsCacheKey, AttendanceCacheKey);
        _logger.LogInformation("Student soft-deleted: {Id}", id);
        return Ok(new { message = "Student deleted" });
    }

    // ??????????????????????????????????????????????
    //  Lecturer moderation
    // ??????????????????????????????????????????????

    /// <summary>POST /api/admin/lecturers/{id}/suspend</summary>
    [HttpPost("lecturers/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == id);
        if (lecturer is null) return NotFound(new { error = "Lecturer not found" });
        if (lecturer.IsDeleted) return BadRequest(new { error = "Cannot modify a deleted account" });
        if (lecturer.Status == UserStatus.Suspended) return Ok(new { message = "Lecturer is already suspended" });

        lecturer.Status = UserStatus.Suspended;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Lecturer suspended: {Id}", id);
        return Ok(new { message = "Lecturer suspended", status = lecturer.Status.ToString() });
    }

    /// <summary>POST /api/admin/lecturers/{id}/activate</summary>
    [HttpPost("lecturers/{id:guid}/activate")]
    public async Task<IActionResult> ActivateLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == id);
        if (lecturer is null) return NotFound(new { error = "Lecturer not found" });
        if (lecturer.IsDeleted) return BadRequest(new { error = "Cannot activate a deleted account" });
        if (lecturer.Status == UserStatus.Active) return Ok(new { message = "Lecturer is already active" });

        lecturer.Status = UserStatus.Active;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Lecturer activated: {Id}", id);
        return Ok(new { message = "Lecturer activated", status = lecturer.Status.ToString() });
    }

    /// <summary>POST /api/admin/lecturers/{id}/deactivate</summary>
    [HttpPost("lecturers/{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == id);
        if (lecturer is null) return NotFound(new { error = "Lecturer not found" });
        if (lecturer.IsDeleted) return BadRequest(new { error = "Cannot modify a deleted account" });
        if (lecturer.Status == UserStatus.Deactivated) return Ok(new { message = "Lecturer is already deactivated" });

        lecturer.Status = UserStatus.Deactivated;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Lecturer deactivated: {Id}", id);
        return Ok(new { message = "Lecturer deactivated", status = lecturer.Status.ToString() });
    }

    /// <summary>POST /api/admin/lecturers/{id}/delete — soft delete</summary>
    [HttpPost("lecturers/{id:guid}/delete")]
    public async Task<IActionResult> DeleteLecturer(Guid id)
    {
        var lecturer = await _db.Lecturers.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == id);
        if (lecturer is null) return NotFound(new { error = "Lecturer not found" });
        if (lecturer.IsDeleted) return Ok(new { message = "Lecturer is already deleted" });

        // Mangle email so the DB unique index never blocks a future re-registration.
        lecturer.Email = $"deleted_{id}_{lecturer.Email}"[..Math.Min($"deleted_{id}_{lecturer.Email}".Length, 200)];
        lecturer.IsDeleted = true;
        lecturer.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _cache.RemoveManyAsync(StatsCacheKey, AttendanceCacheKey);
        _logger.LogInformation("Lecturer soft-deleted: {Id}", id);
        return Ok(new { message = "Lecturer deleted" });
    }

    // ??????????????????????????????????????????????
    //  University CRUD
    // ??????????????????????????????????????????????

    /// <summary>POST /api/admin/universities</summary>
    [HttpPost("universities")]
    public async Task<IActionResult> CreateUniversity([FromBody] CreateUniversityRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        if (await _db.Universities.AnyAsync(u => u.Name == req.Name))
            return Conflict(new { success = false, message = "A university with this name already exists" });

        var university = new University { Id = Guid.NewGuid(), Name = req.Name };
        _db.Universities.Add(university);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin created University {Name} ({Id})", university.Name, university.Id);
        return Created("", new { success = true, message = "University created successfully", data = new { university.Id, university.Name } });
    }

    /// <summary>PUT /api/admin/universities/{id}</summary>
    [HttpPut("universities/{id:guid}")]
    public async Task<IActionResult> UpdateUniversity(Guid id, [FromBody] UpdateUniversityRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var university = await _db.Universities.FindAsync(id);
        if (university is null)
            return NotFound(new { success = false, message = "University not found" });

        if (await _db.Universities.AnyAsync(u => u.Name == req.Name && u.Id != id))
            return Conflict(new { success = false, message = "A university with this name already exists" });

        university.Name = req.Name;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin updated University {Id} to {Name}", id, req.Name);
        return Ok(new { success = true, message = "University updated successfully", data = new { university.Id, university.Name } });
    }

    /// <summary>DELETE /api/admin/universities/{id}</summary>
    [HttpDelete("universities/{id:guid}")]
    public async Task<IActionResult> DeleteUniversity(Guid id)
    {
        var university = await _db.Universities.FindAsync(id);
        if (university is null)
            return NotFound(new { success = false, message = "University not found" });

        if (await _db.Faculties.AnyAsync(f => f.UniversityId == id))
            return BadRequest(new { success = false, message = "Cannot delete — university has faculties" });

        _db.Universities.Remove(university);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin deleted University {Name} ({Id})", university.Name, id);
        return Ok(new { success = true, message = "University deleted successfully" });
    }

    // ??????????????????????????????????????????????
    //  Faculty CRUD
    // ??????????????????????????????????????????????

    /// <summary>POST /api/admin/faculties</summary>
    [HttpPost("faculties")]
    public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var university = await _db.Universities.FindAsync(req.UniversityId);
        if (university is null)
            return BadRequest(new { success = false, message = "Invalid university" });

        if (await _db.Faculties.AnyAsync(f => f.Name == req.Name && f.UniversityId == req.UniversityId))
            return Conflict(new { success = false, message = "A faculty with this name already exists in this university" });

        var faculty = new Faculty { Id = Guid.NewGuid(), Name = req.Name, UniversityId = req.UniversityId };
        _db.Faculties.Add(faculty);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin created Faculty {Name} ({Id}) under University {UniversityId}", faculty.Name, faculty.Id, req.UniversityId);
        return Created("", new { success = true, message = "Faculty created successfully", data = new { faculty.Id, faculty.Name, faculty.UniversityId } });
    }

    /// <summary>PUT /api/admin/faculties/{id}</summary>
    [HttpPut("faculties/{id:guid}")]
    public async Task<IActionResult> UpdateFaculty(Guid id, [FromBody] UpdateFacultyRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var faculty = await _db.Faculties.FindAsync(id);
        if (faculty is null)
            return NotFound(new { success = false, message = "Faculty not found" });

        var university = await _db.Universities.FindAsync(req.UniversityId);
        if (university is null)
            return BadRequest(new { success = false, message = "Invalid university" });

        if (await _db.Faculties.AnyAsync(f => f.Name == req.Name && f.UniversityId == req.UniversityId && f.Id != id))
            return Conflict(new { success = false, message = "A faculty with this name already exists in this university" });

        faculty.Name = req.Name;
        faculty.UniversityId = req.UniversityId;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin updated Faculty {Id} to {Name}", id, req.Name);
        return Ok(new { success = true, message = "Faculty updated successfully", data = new { faculty.Id, faculty.Name, faculty.UniversityId } });
    }

    /// <summary>DELETE /api/admin/faculties/{id}</summary>
    [HttpDelete("faculties/{id:guid}")]
    public async Task<IActionResult> DeleteFaculty(Guid id)
    {
        var faculty = await _db.Faculties.FindAsync(id);
        if (faculty is null)
            return NotFound(new { success = false, message = "Faculty not found" });

        if (await _db.Departments.AnyAsync(d => d.FacultyId == id))
            return BadRequest(new { success = false, message = "Cannot delete — faculty has departments" });

        _db.Faculties.Remove(faculty);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin deleted Faculty {Name} ({Id})", faculty.Name, id);
        return Ok(new { success = true, message = "Faculty deleted successfully" });
    }

    // ??????????????????????????????????????????????
    //  Department CRUD
    // ??????????????????????????????????????????????

    /// <summary>POST /api/admin/departments</summary>
    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var faculty = await _db.Faculties.FindAsync(req.FacultyId);
        if (faculty is null)
            return BadRequest(new { success = false, message = "Invalid faculty" });

        if (await _db.Departments.AnyAsync(d => d.Name == req.Name && d.FacultyId == req.FacultyId))
            return Conflict(new { success = false, message = "A department with this name already exists in this faculty" });

        var department = new Department { Id = Guid.NewGuid(), Name = req.Name, FacultyId = req.FacultyId };
        _db.Departments.Add(department);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin created Department {Name} ({Id}) under Faculty {FacultyId}", department.Name, department.Id, req.FacultyId);
        return Created("", new { success = true, message = "Department created successfully", data = new { department.Id, department.Name, department.FacultyId } });
    }

    /// <summary>PUT /api/admin/departments/{id}</summary>
    [HttpPut("departments/{id:guid}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var department = await _db.Departments.FindAsync(id);
        if (department is null)
            return NotFound(new { success = false, message = "Department not found" });

        var faculty = await _db.Faculties.FindAsync(req.FacultyId);
        if (faculty is null)
            return BadRequest(new { success = false, message = "Invalid faculty" });

        if (await _db.Departments.AnyAsync(d => d.Name == req.Name && d.FacultyId == req.FacultyId && d.Id != id))
            return Conflict(new { success = false, message = "A department with this name already exists in this faculty" });

        department.Name = req.Name;
        department.FacultyId = req.FacultyId;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin updated Department {Id} to {Name}", id, req.Name);
        return Ok(new { success = true, message = "Department updated successfully", data = new { department.Id, department.Name, department.FacultyId } });
    }

    /// <summary>DELETE /api/admin/departments/{id}</summary>
    [HttpDelete("departments/{id:guid}")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department is null)
            return NotFound(new { success = false, message = "Department not found" });

        if (await _db.Programs.AnyAsync(p => p.DepartmentId == id))
            return BadRequest(new { success = false, message = "Cannot delete — department has programs" });

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin deleted Department {Name} ({Id})", department.Name, id);
        return Ok(new { success = true, message = "Department deleted successfully" });
    }

    // ??????????????????????????????????????????????
    //  Program CRUD
    // ??????????????????????????????????????????????

    /// <summary>POST /api/admin/programs</summary>
    [HttpPost("programs")]
    public async Task<IActionResult> CreateProgram([FromBody] CreateProgramRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var department = await _db.Departments.FindAsync(req.DepartmentId);
        if (department is null)
            return BadRequest(new { success = false, message = "Invalid department" });

        if (await _db.Programs.AnyAsync(p => p.Name == req.Name && p.DepartmentId == req.DepartmentId))
            return Conflict(new { success = false, message = "A program with this name already exists in this department" });

        var program = new AcademicProgram
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            DepartmentId = req.DepartmentId,
            DurationSemesters = req.DurationSemesters
        };
        _db.Programs.Add(program);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin created Program {Name} ({Id}) under Department {DepartmentId}", program.Name, program.Id, req.DepartmentId);
        return Created("", new { success = true, message = "Program created successfully", data = new { program.Id, program.Name, program.DepartmentId, program.DurationSemesters } });
    }

    /// <summary>PUT /api/admin/programs/{id}</summary>
    [HttpPut("programs/{id:guid}")]
    public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] UpdateProgramRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var program = await _db.Programs.FindAsync(id);
        if (program is null)
            return NotFound(new { success = false, message = "Program not found" });

        var department = await _db.Departments.FindAsync(req.DepartmentId);
        if (department is null)
            return BadRequest(new { success = false, message = "Invalid department" });

        if (await _db.Programs.AnyAsync(p => p.Name == req.Name && p.DepartmentId == req.DepartmentId && p.Id != id))
            return Conflict(new { success = false, message = "A program with this name already exists in this department" });

        program.Name = req.Name;
        program.DepartmentId = req.DepartmentId;
        program.DurationSemesters = req.DurationSemesters;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin updated Program {Id} to {Name}", id, req.Name);
        return Ok(new { success = true, message = "Program updated successfully", data = new { program.Id, program.Name, program.DepartmentId, program.DurationSemesters } });
    }

    /// <summary>DELETE /api/admin/programs/{id}</summary>
    [HttpDelete("programs/{id:guid}")]
    public async Task<IActionResult> DeleteProgram(Guid id)
    {
        var program = await _db.Programs.FindAsync(id);
        if (program is null)
            return NotFound(new { success = false, message = "Program not found" });

        if (await _db.Classes.AnyAsync(c => c.ProgramId == id))
            return BadRequest(new { success = false, message = "Cannot delete — program is used by classes" });

        if (await _db.Curricula.AnyAsync(c => c.ProgramId == id))
            return BadRequest(new { success = false, message = "Cannot delete — program has curriculum entries" });

        _db.Programs.Remove(program);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin deleted Program {Name} ({Id})", program.Name, id);
        return Ok(new { success = true, message = "Program deleted successfully" });
    }

    // ??????????????????????????????????????????????
    //  Curriculum CRUD
    // ??????????????????????????????????????????????

    /// <summary>POST /api/admin/curriculum</summary>
    [HttpPost("curriculum")]
    public async Task<IActionResult> CreateCurriculum([FromBody] CreateCurriculumRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var program = await _db.Programs.FindAsync(req.ProgramId);
        if (program is null)
            return BadRequest(new { success = false, message = "Invalid program" });

        var semester = await _db.Semesters.FindAsync(req.SemesterId);
        if (semester is null)
            return BadRequest(new { success = false, message = "Invalid semester" });

        if (await _db.Curricula.AnyAsync(c => c.ProgramId == req.ProgramId && c.YearOfStudy == req.YearOfStudy && c.SemesterId == req.SemesterId && c.CourseCode == req.CourseCode))
            return Conflict(new { success = false, message = "This course code already exists for the same program, year, and semester" });

        var entry = new Curriculum
        {
            Id = Guid.NewGuid(),
            ProgramId = req.ProgramId,
            YearOfStudy = req.YearOfStudy,
            SemesterId = req.SemesterId,
            CourseCode = req.CourseCode,
            CourseName = req.CourseName
        };
        _db.Curricula.Add(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin created Curriculum entry {CourseCode} ({Id}) for Program {ProgramId}", entry.CourseCode, entry.Id, req.ProgramId);
        return Created("", new { success = true, message = "Curriculum entry created successfully", data = new { entry.Id, entry.ProgramId, entry.YearOfStudy, entry.SemesterId, entry.CourseCode, entry.CourseName } });
    }

    /// <summary>PUT /api/admin/curriculum/{id}</summary>
    [HttpPut("curriculum/{id:guid}")]
    public async Task<IActionResult> UpdateCurriculum(Guid id, [FromBody] UpdateCurriculumRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var entry = await _db.Curricula.FindAsync(id);
        if (entry is null)
            return NotFound(new { success = false, message = "Curriculum entry not found" });

        var program = await _db.Programs.FindAsync(req.ProgramId);
        if (program is null)
            return BadRequest(new { success = false, message = "Invalid program" });

        var semester = await _db.Semesters.FindAsync(req.SemesterId);
        if (semester is null)
            return BadRequest(new { success = false, message = "Invalid semester" });

        if (await _db.Curricula.AnyAsync(c => c.ProgramId == req.ProgramId && c.YearOfStudy == req.YearOfStudy && c.SemesterId == req.SemesterId && c.CourseCode == req.CourseCode && c.Id != id))
            return Conflict(new { success = false, message = "This course code already exists for the same program, year, and semester" });

        entry.ProgramId = req.ProgramId;
        entry.YearOfStudy = req.YearOfStudy;
        entry.SemesterId = req.SemesterId;
        entry.CourseCode = req.CourseCode;
        entry.CourseName = req.CourseName;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin updated Curriculum entry {Id} to {CourseCode}", id, req.CourseCode);
        return Ok(new { success = true, message = "Curriculum entry updated successfully", data = new { entry.Id, entry.ProgramId, entry.YearOfStudy, entry.SemesterId, entry.CourseCode, entry.CourseName } });
    }

    /// <summary>DELETE /api/admin/curriculum/{id}</summary>
    [HttpDelete("curriculum/{id:guid}")]
    public async Task<IActionResult> DeleteCurriculum(Guid id)
    {
        var entry = await _db.Curricula.FindAsync(id);
        if (entry is null)
            return NotFound(new { success = false, message = "Curriculum entry not found" });

        _db.Curricula.Remove(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin deleted Curriculum entry {CourseCode} ({Id})", entry.CourseCode, id);
        return Ok(new { success = true, message = "Curriculum entry deleted successfully" });
    }

    // ??????????????????????????????????????????????
    //  GET list endpoints
    // ??????????????????????????????????????????????

    /// <summary>GET /api/admin/departments — list all departments with faculty/university context.</summary>
    [HttpGet("departments")]
    public async Task<IActionResult> GetAllDepartments([FromQuery] string? search = null)
    {
        var query = _db.Departments
            .Include(d => d.Faculty).ThenInclude(f => f.University)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search) || d.Faculty.Name.Contains(search));

        var items = await query
            .OrderBy(d => d.Faculty.University.Name).ThenBy(d => d.Faculty.Name).ThenBy(d => d.Name)
            .Select(d => new
            {
                Id = d.Id.ToString(),
                d.Name,
                FacultyId = d.FacultyId.ToString(),
                FacultyName = d.Faculty.Name,
                UniversityName = d.Faculty.University.Name,
                ProgramsCount = d.Programs.Count,
                Status = "Active"
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>GET /api/admin/programs — list all programs with department context.</summary>
    [HttpGet("programs")]
    public async Task<IActionResult> GetAllPrograms([FromQuery] string? search = null)
    {
        var query = _db.Programs
            .Include(p => p.Department).ThenInclude(d => d.Faculty)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Department.Name.Contains(search));

        var items = await query
            .OrderBy(p => p.Department.Faculty.Name).ThenBy(p => p.Department.Name).ThenBy(p => p.Name)
            .Select(p => new
            {
                Id = p.Id.ToString(),
                p.Name,
                DepartmentId = p.DepartmentId.ToString(),
                DepartmentName = p.Department.Name,
                FacultyName = p.Department.Faculty.Name,
                p.DurationSemesters,
                CurriculumCount = p.CurriculumEntries.Count,
                Status = "Active"
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>GET /api/admin/curriculum — list curriculum entries.</summary>
    [HttpGet("curriculum")]
    public async Task<IActionResult> GetAllCurriculum([FromQuery] Guid? programId = null, [FromQuery] string? search = null)
    {
        var query = _db.Curricula
            .Include(c => c.Program)
            .Include(c => c.Semester)
            .AsQueryable();

        if (programId.HasValue)
            query = query.Where(c => c.ProgramId == programId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.CourseCode.Contains(search) || c.CourseName.Contains(search));

        var items = await query
            .OrderBy(c => c.Program.Name).ThenBy(c => c.YearOfStudy).ThenBy(c => c.CourseCode)
            .Select(c => new
            {
                Id = c.Id.ToString(),
                ProgramId = c.ProgramId.ToString(),
                ProgramName = c.Program.Name,
                c.CourseCode,
                c.CourseName,
                c.YearOfStudy,
                SemesterId = c.SemesterId.ToString(),
                SemesterName = c.Semester.Name,
                Status = "Active"
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>GET /api/admin/stats — aggregate admin dashboard statistics.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetAdminStats()
    {
        var cached = await _cache.GetAsync<object>(StatsCacheKey);
        if (cached is not null) return Ok(cached);

        var totalStudents = await _db.Students.IgnoreQueryFilters().CountAsync(s => !s.IsDeleted);
        var activeStudents = await _db.Students.IgnoreQueryFilters().CountAsync(s => !s.IsDeleted && s.Status == UserStatus.Active);
        var suspendedStudents = await _db.Students.IgnoreQueryFilters().CountAsync(s => !s.IsDeleted && s.Status == UserStatus.Suspended);
        var totalLecturers = await _db.Lecturers.IgnoreQueryFilters().CountAsync(l => !l.IsDeleted);
        var pendingLecturers = await _db.Lecturers.IgnoreQueryFilters().CountAsync(l => !l.IsDeleted && !l.IsApproved);
        var totalPrograms = await _db.Programs.CountAsync();
        var totalClasses = await _db.Classes.CountAsync();
        var totalUniversities = await _db.Universities.CountAsync();
        var totalFaculties = await _db.Faculties.CountAsync();
        var totalDepartments = await _db.Departments.CountAsync();

        double avgAttendance = 0;
        if (await _db.ClassSessions.AnyAsync())
        {
            var sessions = await _db.ClassSessions.CountAsync();
            var records = await _db.Attendances.CountAsync();
            avgAttendance = sessions > 0 ? Math.Round((double)records / sessions * 100, 1) : 0;
        }

        var result = new
        {
            TotalStudents = totalStudents,
            TotalLecturers = totalLecturers,
            TotalPrograms = totalPrograms,
            TotalClasses = totalClasses,
            TotalUniversities = totalUniversities,
            TotalFaculties = totalFaculties,
            TotalDepartments = totalDepartments,
            ActiveStudents = activeStudents,
            SuspendedStudents = suspendedStudents,
            PendingLecturers = pendingLecturers,
            AverageAttendanceRate = avgAttendance
        };
        await _cache.SetAsync(StatsCacheKey, result, StatsTtl);
        return Ok(result);
    }

    /// <summary>GET /api/admin/all-classes — list all classes for admin view.</summary>
    [HttpGet("all-classes")]
    public async Task<IActionResult> GetAllClasses([FromQuery] string? search = null)
    {
        var query = _db.Classes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.CourseCode != null && c.CourseCode.Contains(search)));

        var items = await query
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                Id = c.Id.ToString(),
                c.Name,
                c.CourseCode,
                ParentClassId = c.ParentClassId != null ? c.ParentClassId.ToString() : (string?)null,
                EnrolledStudents = c.StudentClasses.Count(sc => sc.Status == "active"),
                LecturerName = c.LecturerClasses
                    .Select(lc => lc.Lecturer.FirstName + " " + lc.Lecturer.LastName)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>GET /api/admin/attendance/summary — per-class attendance summary.</summary>
    [HttpGet("attendance/summary")]
    public async Task<IActionResult> GetAttendanceSummary()
    {
        var cached = await _cache.GetAsync<object>(AttendanceCacheKey);
        if (cached is not null) return Ok(cached);

        var items = await _db.Classes
            .Select(c => new
            {
                ClassId = c.Id.ToString(),
                ClassName = c.Name,
                LecturerName = c.LecturerClasses
                    .Select(lc => lc.Lecturer.FirstName + " " + lc.Lecturer.LastName)
                    .FirstOrDefault() ?? "Unassigned",
                TotalSessions = c.Sessions.Count,
                TotalStudents = c.StudentClasses.Count(sc => sc.Status == "active"),
                AverageAttendanceRate = c.Sessions.Count > 0
                    ? Math.Round(
                        c.Sessions.SelectMany(s => s.Attendances).Count()
                        / (double)(c.Sessions.Count * Math.Max(1, c.StudentClasses.Count(sc => sc.Status == "active"))) * 100, 1)
                    : 0.0
            })
            .OrderByDescending(c => c.AverageAttendanceRate)
            .ToListAsync();

        await _cache.SetAsync(AttendanceCacheKey, items, AttendanceTtl);
        return Ok(items);
    }

    /// <summary>POST /api/admin/notify — broadcast notification to role/all users (in-memory store; extend for push).</summary>
    [HttpPost("notify")]
    public IActionResult SendNotification([FromBody] AdminNotifyRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Body))
            return BadRequest(new { success = false, message = "Title and body are required." });

        _logger.LogInformation("Admin broadcast: [{Target}] {Title}", req.TargetRole ?? "All", req.Title);

        // In a production system this would enqueue a job (e.g. SignalR / FCM).
        return Ok(new
        {
            success = true,
            message = "Notification queued successfully.",
            data = new
            {
                Id = Guid.NewGuid().ToString(),
                req.Title,
                req.Body,
                req.TargetRole,
                SentAt = DateTime.UtcNow
            }
        });
    }

    /// <summary>
    /// GET /api/admin/analytics/trends
    /// Returns time-bucketed trend data for students, lecturers, attendance and approvals.
    /// Supports <c>granularity</c> = daily | weekly | monthly (default: daily).
    /// </summary>
    [HttpGet("analytics/trends")]
    public async Task<IActionResult> GetAnalyticsTrends(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null,
        [FromQuery] string granularity = "daily")
    {
        var fromDate = DateTime.SpecifyKind(from ?? DateTime.UtcNow.AddDays(-30), DateTimeKind.Utc);
        var toDate   = DateTime.SpecifyKind(to   ?? DateTime.UtcNow,             DateTimeKind.Utc);

        var cacheKey = $"{TrendsCacheKeyPrefix}:{granularity}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
        var cached = await _cache.GetAsync<object>(cacheKey);
        if (cached is not null) return Ok(cached);

        // Date-bucket selector — keeps an anchor date for deterministic ordering.
        Func<DateTime, (string Label, DateTime Anchor)> bucket = granularity.ToLowerInvariant() switch
        {
            "monthly" => dt => (dt.ToString("MMM yyyy"), new DateTime(dt.Year, dt.Month, 1)),
            "weekly"  => dt =>
            {
                int week = ISOWeek.GetWeekOfYear(dt);
                int year = ISOWeek.GetYear(dt);
                return ($"W{week:D2} {year}", ISOWeek.ToDateTime(year, week, DayOfWeek.Monday));
            },
            _ => dt => (dt.ToString("MM/dd"), dt.Date)
        };

        // ?? Student registrations per bucket ??????????????????????????????????
        var studentDates = await _db.Students
            .IgnoreQueryFilters()
            .Where(s => !s.IsDeleted && s.CreatedAt >= fromDate && s.CreatedAt <= toDate)
            .Select(s => s.CreatedAt)
            .ToListAsync();

        var studentGrowth = studentDates
            .Select(d => bucket(d))
            .GroupBy(b => b.Label)
            .Select(g => new { label = g.Key, value = (double)g.Count(), anchor = g.First().Anchor })
            .OrderBy(g => g.anchor)
            .Select(g => new { g.label, g.value })
            .ToList();

        // ?? Lecturer registrations per bucket ?????????????????????????????????
        var lecturerDates = await _db.Lecturers
            .IgnoreQueryFilters()
            .Where(l => !l.IsDeleted && l.CreatedAt >= fromDate && l.CreatedAt <= toDate)
            .Select(l => l.CreatedAt)
            .ToListAsync();

        var lecturerGrowth = lecturerDates
            .Select(d => bucket(d))
            .GroupBy(b => b.Label)
            .Select(g => new { label = g.Key, value = (double)g.Count(), anchor = g.First().Anchor })
            .OrderBy(g => g.anchor)
            .Select(g => new { g.label, g.value })
            .ToList();

        // ?? Attendance rate per bucket ?????????????????????????????????????????
        var attendanceData = await _db.Attendances
            .Where(a => a.Date >= fromDate && a.Date <= toDate)
            .Select(a => new { a.Date, a.IsPresent })
            .ToListAsync();

        var attendanceTrend = attendanceData
            .Select(a => new { Bucket = bucket(a.Date), a.IsPresent })
            .GroupBy(a => a.Bucket.Label)
            .Select(g => new
            {
                label  = g.Key,
                value  = g.Any() ? Math.Round(g.Count(a => a.IsPresent) / (double)g.Count() * 100, 1) : 0.0,
                anchor = g.First().Bucket.Anchor
            })
            .OrderBy(g => g.anchor)
            .Select(g => new { g.label, g.value })
            .ToList();

        // ?? Lecturer approvals per bucket ?????????????????????????????????????
        var approvalDates = await _db.AuditLogs
            .Where(a => a.Action == "Approve" && a.CreatedAt >= fromDate && a.CreatedAt <= toDate)
            .Select(a => a.CreatedAt)
            .ToListAsync();

        var approvalsTrend = approvalDates
            .Select(d => bucket(d))
            .GroupBy(b => b.Label)
            .Select(g => new { label = g.Key, value = (double)g.Count(), anchor = g.First().Anchor })
            .OrderBy(g => g.anchor)
            .Select(g => new { g.label, g.value })
            .ToList();

        var result = new
        {
            StudentGrowth   = studentGrowth,
            LecturerGrowth  = lecturerGrowth,
            AttendanceTrend = attendanceTrend,
            ApprovalsTrend  = approvalsTrend
        };

        await _cache.SetAsync(cacheKey, result, TrendsTtl);
        return Ok(result);
    }

    /// <summary>GET /api/admin/audit-logs — paginated audit log with optional filters.</summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? action = null,
        [FromQuery] string? entity = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrWhiteSpace(entity))
            query = query.Where(a => a.EntityName == entity);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a =>
                (a.UserEmail != null && a.UserEmail.Contains(search)) ||
                a.Action.Contains(search) ||
                a.EntityName.Contains(search) ||
                (a.EntityId != null && a.EntityId.Contains(search)));

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.Action,
                EntityName = a.EntityName,
                a.EntityId,
                a.UserEmail,
                a.UserRole,
                a.IpAddress,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }
}

public record AdminNotifyRequest(string Title, string Body, string? TargetRole = null, string? TargetClassId = null);
