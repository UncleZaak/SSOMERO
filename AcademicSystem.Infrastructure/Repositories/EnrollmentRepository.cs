using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces.Repositories;
using AcademicSystem.Domain.Entities;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IEnrollmentRepository.
    /// </summary>
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Enrollment entity)
        {
            await _context.Set<Enrollment>().AddAsync(entity);
        }

        public void Delete(Enrollment entity)
        {
            _context.Set<Enrollment>().Remove(entity);
        }

        public async Task<IEnumerable<Enrollment>> GetAllAsync()
        {
            return await _context.Set<Enrollment>().ToListAsync();
        }

        public async Task<Enrollment?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Enrollment>().FindAsync(id);
        }

        public void Update(Enrollment entity)
        {
            _context.Set<Enrollment>().Update(entity);
        }
    }
}
