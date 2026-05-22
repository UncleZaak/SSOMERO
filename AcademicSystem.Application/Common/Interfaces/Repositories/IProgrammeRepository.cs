using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the Programme entity.
    /// </summary>
    public interface IProgrammeRepository
    {
        Task<Programme?> GetByIdAsync(Guid id);
        Task<IEnumerable<Programme>> GetAllAsync();
        Task AddAsync(Programme entity);
        void Update(Programme entity);
        void Delete(Programme entity);
    }
}
