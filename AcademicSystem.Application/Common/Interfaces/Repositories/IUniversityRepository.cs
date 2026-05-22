using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the University entity.
    /// </summary>
    public interface IUniversityRepository
    {
        Task<University?> GetByIdAsync(Guid id);
        Task<IEnumerable<University>> GetAllAsync();
        Task AddAsync(University entity);
        void Update(University entity);
        void Delete(University entity);
    }
}
