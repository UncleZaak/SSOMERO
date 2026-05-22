using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the Assessment entity.
    /// </summary>
    public interface IAssessmentRepository
    {
        Task<Assessment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Assessment>> GetAllAsync();
        Task AddAsync(Assessment entity);
        void Update(Assessment entity);
        void Delete(Assessment entity);
    }
}
