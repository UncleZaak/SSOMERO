using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the Submission entity.
    /// </summary>
    public interface ISubmissionRepository
    {
        Task<Submission?> GetByIdAsync(Guid id);
        Task<IEnumerable<Submission>> GetAllAsync();
        Task AddAsync(Submission entity);
        void Update(Submission entity);
        void Delete(Submission entity);
    }
}
