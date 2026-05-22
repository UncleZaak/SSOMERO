using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the AcademicClass entity.
    /// </summary>
    public interface IAcademicClassRepository
    {
        Task<AcademicClass?> GetByIdAsync(Guid id);
        Task<IEnumerable<AcademicClass>> GetAllAsync();
        Task AddAsync(AcademicClass entity);
        void Update(AcademicClass entity);
        void Delete(AcademicClass entity);
    }
}
