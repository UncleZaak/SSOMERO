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
    /// EF Core implementation of IAssessmentRepository.
    /// </summary>
    public class AssessmentRepository : IAssessmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AssessmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Assessment entity)
        {
            await _context.Set<Assessment>().AddAsync(entity);
        }

        public void Delete(Assessment entity)
        {
            _context.Set<Assessment>().Remove(entity);
        }

        public async Task<IEnumerable<Assessment>> GetAllAsync()
        {
            return await _context.Set<Assessment>().ToListAsync();
        }

        public async Task<Assessment?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Assessment>().FindAsync(id);
        }

        public void Update(Assessment entity)
        {
            _context.Set<Assessment>().Update(entity);
        }
    }
}
