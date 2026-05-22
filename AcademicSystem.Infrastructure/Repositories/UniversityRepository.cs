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
    /// EF Core implementation of IUniversityRepository.
    /// </summary>
    public class UniversityRepository : IUniversityRepository
    {
        private readonly ApplicationDbContext _context;

        public UniversityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(University entity)
        {
            await _context.Set<University>().AddAsync(entity);
        }

        public void Delete(University entity)
        {
            _context.Set<University>().Remove(entity);
        }

        public async Task<IEnumerable<University>> GetAllAsync()
        {
            return await _context.Set<University>().ToListAsync();
        }

        public async Task<University?> GetByIdAsync(Guid id)
        {
            return await _context.Set<University>().FindAsync(id);
        }

        public void Update(University entity)
        {
            _context.Set<University>().Update(entity);
        }
    }
}
