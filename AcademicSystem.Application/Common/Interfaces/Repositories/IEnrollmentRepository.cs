using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the Enrollment entity.
    /// </summary>
    public interface IEnrollmentRepository
    {
        Task<Enrollment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Enrollment>> GetAllAsync();
        Task AddAsync(Enrollment entity);
        void Update(Enrollment entity);
        void Delete(Enrollment entity);
    }
}
