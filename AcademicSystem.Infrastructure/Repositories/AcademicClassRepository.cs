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
    /// EF Core implementation of IAcademicClassRepository.
    /// </summary>
    public class AcademicClassRepository : IAcademicClassRepository
    {
        private readonly ApplicationDbContext _context;

        public AcademicClassRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AcademicClass entity)
        {
            await _context.Set<AcademicClass>().AddAsync(entity);
        }

        public void Delete(AcademicClass entity)
        {
            _context.Set<AcademicClass>().Remove(entity);
        }

        public async Task<IEnumerable<AcademicClass>> GetAllAsync()
        {
            return await _context.Set<AcademicClass>().ToListAsync();
        }

        public async Task<AcademicClass?> GetByIdAsync(Guid id)
        {
            return await _context.Set<AcademicClass>().FindAsync(id);
        }

        public void Update(AcademicClass entity)
        {
            _context.Set<AcademicClass>().Update(entity);
        }
    }
}
