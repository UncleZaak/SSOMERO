using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Services
{
    /// <summary>
    /// Service contract for student-related business logic.
    /// </summary>
    public interface IStudentService
    {
        Task<Student?> GetByIdAsync(Guid id);
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student> CreateAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(Guid id);
    }
}
