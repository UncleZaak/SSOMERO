using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the Course entity.
    /// </summary>
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(Guid id);
        Task<IEnumerable<Course>> GetAllAsync();
        Task AddAsync(Course entity);
        void Update(Course entity);
        void Delete(Course entity);
    }
}
