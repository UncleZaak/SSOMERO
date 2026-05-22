using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<Assessment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Assessment>> GetAllAsync();
        Task<Assessment> CreateAsync(Assessment assessment);
        Task UpdateAsync(Assessment assessment);
        Task DeleteAsync(Guid id);
    }
}
