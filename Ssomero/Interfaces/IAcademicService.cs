using System.Collections.Generic;
using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IAcademicService
{
    Task<IEnumerable<LookupItem>> GetUniversitiesAsync();
    Task<IEnumerable<LookupItem>> GetFacultiesAsync(string universityId);
    Task<IEnumerable<LookupItem>> GetDepartmentsAsync(string facultyId);
    Task<IEnumerable<LookupItem>> GetProgramsAsync(string departmentId);
    Task<IEnumerable<LookupItem>> GetEntrySchemesAsync();
    Task<IEnumerable<LookupItem>> GetIntakesAsync();
    Task<IEnumerable<LookupItem>> GetStudyModesAsync();
    Task<IEnumerable<LookupItem>> GetAcademicYearsAsync();
    Task<IEnumerable<LookupItem>> GetSemestersAsync();

    // University CRUD
    Task<List<UniversityDto>> GetUniversityDetailsAsync();
    Task<PaginatedResult<UniversityDto>> GetUniversitiesPaginatedAsync(int page, int pageSize, string? search = null);
    Task<UniversityDto?> GetUniversityByIdAsync(string id);
    Task<UniversityDto?> CreateUniversityAsync(string name);
    Task<UniversityDto?> UpdateUniversityAsync(string id, string name);
    Task<bool> DeleteUniversityAsync(string id);

    // Faculty CRUD
    Task<List<FacultyDto>> GetFacultyDetailsAsync();
    Task<FacultyDto?> CreateFacultyAsync(string name, string universityId);
    Task<FacultyDto?> UpdateFacultyAsync(string id, string name, string universityId);
    Task<bool> DeleteFacultyAsync(string id);

    // Department CRUD
    Task<List<DepartmentDto>> GetDepartmentDetailsAsync(string? search = null);
    Task<DepartmentDto?> CreateDepartmentAsync(string name, string facultyId);
    Task<DepartmentDto?> UpdateDepartmentAsync(string id, string name, string facultyId);
    Task<bool> DeleteDepartmentAsync(string id);

    // Program CRUD
    Task<List<ProgramDto>> GetProgramDetailsAsync(string? search = null);
    Task<ProgramDto?> CreateProgramAsync(string name, string departmentId, int durationSemesters);
    Task<ProgramDto?> UpdateProgramAsync(string id, string name, string departmentId, int durationSemesters);
    Task<bool> DeleteProgramAsync(string id);

    // Curriculum CRUD
    Task<List<CurriculumDto>> GetCurriculumDetailsAsync(string? programId = null, string? search = null);
    Task<CurriculumDto?> CreateCurriculumEntryAsync(string programId, int yearOfStudy, string semesterId, string courseCode, string courseName);
    Task<CurriculumDto?> UpdateCurriculumEntryAsync(string id, string programId, int yearOfStudy, string semesterId, string courseCode, string courseName);
    Task<bool> DeleteCurriculumEntryAsync(string id);

    // Parent-scoped paginated list methods (Phase 2 – cascade filtering)
    Task<PaginatedResult<FacultyDto>> GetFacultiesByUniversityAsync(string universityId, int page = 1, int pageSize = 100, string? search = null, CancellationToken ct = default);
    Task<PaginatedResult<DepartmentDto>> GetDepartmentsByFacultyAsync(string facultyId, int page = 1, int pageSize = 100, string? search = null, CancellationToken ct = default);
    Task<PaginatedResult<ProgramDto>> GetProgramsByDepartmentAsync(string departmentId, int page = 1, int pageSize = 100, string? search = null, CancellationToken ct = default);
    Task<PaginatedResult<CurriculumDto>> GetCurriculumByProgramAsync(string programId, int page = 1, int pageSize = 100, string? search = null, CancellationToken ct = default);
}
