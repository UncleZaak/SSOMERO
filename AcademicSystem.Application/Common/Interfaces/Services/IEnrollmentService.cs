using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Services
{
    public interface IEnrollmentService
    {
        Task<Enrollment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Enrollment>> GetAllAsync();
        Task<Enrollment> CreateAsync(Enrollment enrollment);
        Task UpdateAsync(Enrollment enrollment);
        Task DeleteAsync(Guid id);
    }
}
