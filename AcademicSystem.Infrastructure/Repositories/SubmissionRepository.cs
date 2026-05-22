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
    /// EF Core implementation of ISubmissionRepository.
    /// </summary>
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubmissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Submission entity)
        {
            await _context.Set<Submission>().AddAsync(entity);
        }

        public void Delete(Submission entity)
        {
            _context.Set<Submission>().Remove(entity);
        }

        public async Task<IEnumerable<Submission>> GetAllAsync()
        {
            return await _context.Set<Submission>().ToListAsync();
        }

        public async Task<Submission?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Submission>().FindAsync(id);
        }

        public void Update(Submission entity)
        {
            _context.Set<Submission>().Update(entity);
        }
    }
}
