using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces.Repositories;
using AcademicSystem.Domain.Entities;
using AcademicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Infrastructure.Persistence;


namespace AcademicSystem.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IStudentRepository.
    /// </summary>
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Student entity)
        {
            await _context.Set<Student>().AddAsync(entity);
        }

        public void Delete(Student entity)
        {
            _context.Set<Student>().Remove(entity);
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Set<Student>().ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Student>().FindAsync(id);
        }

        public void Update(Student entity)
        {
            _context.Set<Student>().Update(entity);
        }
    }
}
