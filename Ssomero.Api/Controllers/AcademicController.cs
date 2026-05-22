using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.DTOs.Common;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Controllers;

/// <summary>
/// Returns academic hierarchy data for cascading dropdowns.
/// GET /api/universities
/// GET /api/faculties?universityId=
/// GET /api/departments?facultyId=
/// GET /api/programs?departmentId=
/// GET /api/entry-schemes
/// GET /api/intakes
/// GET /api/study-modes
/// GET /api/academic-years
/// GET /api/semesters
/// </summary>
[ApiController]
[Route("api")]
public class AcademicController : ControllerBase
{
    private readonly SsomeroDbContext _db;

    public AcademicController(SsomeroDbContext db)
    {
        _db = db;
    }

    [HttpGet("universities")]
    public async Task<IActionResult> GetUniversities()
    {
        var items = await _db.Universities
            .OrderBy(u => u.Name)
            .Select(u => new LookupDto(u.Id, u.Name))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("faculties")]
    public async Task<IActionResult> GetFaculties([FromQuery] Guid universityId)
    {
        var items = await _db.Faculties
            .Where(f => f.UniversityId == universityId)
            .OrderBy(f => f.Name)
            .Select(f => new LookupDto(f.Id, f.Name))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] Guid facultyId)
    {
        var items = await _db.Departments
            .Where(d => d.FacultyId == facultyId)
            .OrderBy(d => d.Name)
            .Select(d => new LookupDto(d.Id, d.Name))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("programs")]
    public async Task<IActionResult> GetPrograms([FromQuery] Guid departmentId)
    {
        var items = await _db.Programs
            .Where(p => p.DepartmentId == departmentId)
            .OrderBy(p => p.Name)
            .Select(p => new LookupDto(p.Id, p.Name))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("entry-schemes")]
    public async Task<IActionResult> GetEntrySchemes()
    {
        var items = await _db.EntrySchemes.OrderBy(e => e.Name).Select(e => new LookupDto(e.Id, e.Name)).ToListAsync();
        return Ok(items);
    }

    [HttpGet("intakes")]
    public async Task<IActionResult> GetIntakes()
    {
        var items = await _db.Intakes.OrderBy(i => i.Name).Select(i => new LookupDto(i.Id, i.Name)).ToListAsync();
        return Ok(items);
    }

    [HttpGet("study-modes")]
    public async Task<IActionResult> GetStudyModes()
    {
        var items = await _db.StudyModes.OrderBy(s => s.Name).Select(s => new LookupDto(s.Id, s.Name)).ToListAsync();
        return Ok(items);
    }

    [HttpGet("academic-years")]
    public async Task<IActionResult> GetAcademicYears()
    {
        var items = await _db.AcademicYears.OrderBy(a => a.Name).Select(a => new LookupDto(a.Id, a.Name)).ToListAsync();
        return Ok(items);
    }

    [HttpGet("semesters")]
    public async Task<IActionResult> GetSemesters()
    {
        var items = await _db.Semesters.OrderBy(s => s.Number).Select(s => new LookupDto(s.Id, s.Name)).ToListAsync();
        return Ok(items);
    }

    // ──────────────────────────────────────────────
    //  University CRUD (Admin only)
    // ──────────────────────────────────────────────

    [HttpGet("universities/details")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUniversityDetails([FromQuery] PaginationRequest req)
    {
        var query = _db.Universities.OrderBy(u => u.Name).AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(u => u.Name.Contains(req.Search));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(u => new UniversityDetailDto(u.Id, u.Name, u.Faculties.Count, "Active"))
            .ToListAsync();

        var result = new PaginatedResponse<UniversityDetailDto>
        {
            Data = items,
            TotalCount = totalCount,
            Page = req.Page,
            PageSize = req.PageSize
        };
        return Ok(result);
    }

    [HttpGet("universities/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUniversity(Guid id)
    {
        var u = await _db.Universities.Include(x => x.Faculties).FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        return Ok(new UniversityDetailDto(u.Id, u.Name, u.Faculties.Count, "Active"));
    }

    [HttpPost("universities")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUniversity([FromBody] CreateUniversityRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var exists = await _db.Universities.AnyAsync(u => u.Name == req.Name);
        if (exists) return Conflict(new { error = "University with this name already exists" });

        var uni = new University { Id = Guid.NewGuid(), Name = req.Name };
        _db.Universities.Add(uni);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUniversity), new { id = uni.Id },
            new UniversityDetailDto(uni.Id, uni.Name, 0, "Active"));
    }

    [HttpPut("universities/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUniversity(Guid id, [FromBody] UpdateUniversityRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var uni = await _db.Universities.FindAsync(id);
        if (uni is null) return NotFound();

        uni.Name = req.Name;
        await _db.SaveChangesAsync();

        return Ok(new UniversityDetailDto(uni.Id, uni.Name, 0, "Active"));
    }

    [HttpDelete("universities/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUniversity(Guid id)
    {
        var uni = await _db.Universities.Include(u => u.Faculties).FirstOrDefaultAsync(u => u.Id == id);
        if (uni is null) return NotFound();

        if (uni.Faculties.Count > 0)
            return BadRequest(new { error = "Cannot delete a university that has faculties. Remove faculties first." });

        _db.Universities.Remove(uni);
        await _db.SaveChangesAsync();

        return Ok(new { message = "University deleted" });
    }

    // ──────────────────────────────────────────────
    //  Faculty CRUD (Admin only)
    // ──────────────────────────────────────────────

    [HttpGet("faculties/details")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetFacultyDetails([FromQuery] PaginationRequest req)
    {
        var query = _db.Faculties.Include(f => f.University).OrderBy(f => f.Name).AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(f => f.Name.Contains(req.Search) || f.University.Name.Contains(req.Search));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(f => new FacultyDetailDto(f.Id, f.Name, f.UniversityId, f.University.Name, f.Departments.Count, "Active"))
            .ToListAsync();

        var result = new PaginatedResponse<FacultyDetailDto>
        {
            Data = items,
            TotalCount = totalCount,
            Page = req.Page,
            PageSize = req.PageSize
        };
        return Ok(result);
    }

    [HttpGet("faculties/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetFaculty(Guid id)
    {
        var f = await _db.Faculties.Include(x => x.University).Include(x => x.Departments).FirstOrDefaultAsync(x => x.Id == id);
        if (f is null) return NotFound();
        return Ok(new FacultyDetailDto(f.Id, f.Name, f.UniversityId, f.University.Name, f.Departments.Count, "Active"));
    }

    [HttpPost("faculties")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var uniExists = await _db.Universities.AnyAsync(u => u.Id == req.UniversityId);
        if (!uniExists) return BadRequest(new { error = "University not found." });

        var exists = await _db.Faculties.AnyAsync(f => f.Name == req.Name && f.UniversityId == req.UniversityId);
        if (exists) return Conflict(new { error = "A faculty with this name already exists in that university." });

        var faculty = new Faculty { Id = Guid.NewGuid(), Name = req.Name, UniversityId = req.UniversityId };
        _db.Faculties.Add(faculty);
        await _db.SaveChangesAsync();

        var uni = await _db.Universities.FindAsync(req.UniversityId);
        return CreatedAtAction(nameof(GetFaculty), new { id = faculty.Id },
            new FacultyDetailDto(faculty.Id, faculty.Name, faculty.UniversityId, uni!.Name, 0, "Active"));
    }

    [HttpPut("faculties/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateFaculty(Guid id, [FromBody] UpdateFacultyRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem();

        var faculty = await _db.Faculties.Include(f => f.University).FirstOrDefaultAsync(f => f.Id == id);
        if (faculty is null) return NotFound();

        faculty.Name = req.Name;
        faculty.UniversityId = req.UniversityId;
        await _db.SaveChangesAsync();

        return Ok(new FacultyDetailDto(faculty.Id, faculty.Name, faculty.UniversityId, faculty.University.Name, faculty.Departments.Count, "Active"));
    }

    [HttpDelete("faculties/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFaculty(Guid id)
    {
        var faculty = await _db.Faculties.Include(f => f.Departments).FirstOrDefaultAsync(f => f.Id == id);
        if (faculty is null) return NotFound();

        if (faculty.Departments.Count > 0)
            return BadRequest(new { error = "Cannot delete a faculty that has departments. Remove departments first." });

        _db.Faculties.Remove(faculty);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Faculty deleted" });
    }
}
