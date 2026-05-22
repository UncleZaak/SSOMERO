using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }
}
