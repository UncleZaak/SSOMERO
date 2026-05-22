using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the Student entity. Defines async CRUD operations.
    /// </summary>
    public interface IStudentRepository
    {
        Task<Student?> GetByIdAsync(Guid id);
        Task<IEnumerable<Student>> GetAllAsync();
        Task AddAsync(Student entity);
        void Update(Student entity);
        void Delete(Student entity);
    }
}
