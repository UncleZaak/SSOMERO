using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken?> GetByIdAsync(Guid id);
        Task<IEnumerable<RefreshToken>> GetAllAsync();
        Task<RefreshToken> CreateAsync(RefreshToken token);
        Task UpdateAsync(RefreshToken token);
        Task DeleteAsync(Guid id);
    }
}
