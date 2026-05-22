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
    /// EF Core implementation of IUserRepository.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User entity)
        {
            await _context.Set<User>().AddAsync(entity);
        }

        public void Delete(User entity)
        {
            _context.Set<User>().Remove(entity);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Set<User>().ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Set<User>().FindAsync(id);
        }

        public void Update(User entity)
        {
            _context.Set<User>().Update(entity);
        }
    }
}
