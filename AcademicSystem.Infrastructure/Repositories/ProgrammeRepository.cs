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
    /// EF Core implementation of IProgrammeRepository.
    /// </summary>
    public class ProgrammeRepository : IProgrammeRepository
    {
        private readonly ApplicationDbContext _context;

        public ProgrammeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Programme entity)
        {
            await _context.Set<Programme>().AddAsync(entity);
        }

        public void Delete(Programme entity)
        {
            _context.Set<Programme>().Remove(entity);
        }

        public async Task<IEnumerable<Programme>> GetAllAsync()
        {
            return await _context.Set<Programme>().ToListAsync();
        }

        public async Task<Programme?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Programme>().FindAsync(id);
        }

        public void Update(Programme entity)
        {
            _context.Set<Programme>().Update(entity);
        }
    }
}
