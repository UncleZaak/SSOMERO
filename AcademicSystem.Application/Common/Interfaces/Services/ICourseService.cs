using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Services
{
    public interface ICourseService
    {
        Task<Course?> GetByIdAsync(Guid id);
        Task<IEnumerable<Course>> GetAllAsync();
        Task<Course> CreateAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(Guid id);
    }
}
