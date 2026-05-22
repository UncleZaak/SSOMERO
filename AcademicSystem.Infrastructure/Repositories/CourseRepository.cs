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
    /// EF Core implementation of ICourseRepository.
    /// </summary>
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Course entity)
        {
            await _context.Set<Course>().AddAsync(entity);
        }

        public void Delete(Course entity)
        {
            _context.Set<Course>().Remove(entity);
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _context.Set<Course>().ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Course>().FindAsync(id);
        }

        public void Update(Course entity)
        {
            _context.Set<Course>().Update(entity);
        }
    }
}
