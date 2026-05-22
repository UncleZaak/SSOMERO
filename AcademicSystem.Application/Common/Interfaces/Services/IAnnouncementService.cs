using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Services
{
    public interface IAnnouncementService
    {
        Task<Announcement?> GetByIdAsync(Guid id);
        Task<IEnumerable<Announcement>> GetAllAsync();
        Task<Announcement> CreateAsync(Announcement announcement);
        Task UpdateAsync(Announcement announcement);
        Task DeleteAsync(Guid id);
    }
}
