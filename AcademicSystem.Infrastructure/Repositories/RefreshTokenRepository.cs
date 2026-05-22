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
    /// EF Core implementation of IRefreshTokenRepository.
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken entity)
        {
            await _context.Set<RefreshToken>().AddAsync(entity);
        }

        public void Delete(RefreshToken entity)
        {
            _context.Set<RefreshToken>().Remove(entity);
        }

        public async Task<IEnumerable<RefreshToken>> GetAllAsync()
        {
            return await _context.Set<RefreshToken>().ToListAsync();
        }

        public async Task<RefreshToken?> GetByIdAsync(Guid id)
        {
            return await _context.Set<RefreshToken>().FindAsync(id);
        }

        public void Update(RefreshToken entity)
        {
            _context.Set<RefreshToken>().Update(entity);
        }
    }
}
