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
    /// EF Core implementation of IAnnouncementRepository.
    /// </summary>
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Announcement entity)
        {
            await _context.Set<Announcement>().AddAsync(entity);
        }

        public void Delete(Announcement entity)
        {
            _context.Set<Announcement>().Remove(entity);
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await _context.Set<Announcement>().ToListAsync();
        }

        public async Task<Announcement?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Announcement>().FindAsync(id);
        }

        public void Update(Announcement entity)
        {
            _context.Set<Announcement>().Update(entity);
        }
    }
}
