using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the RefreshToken entity.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByIdAsync(Guid id);
        Task<IEnumerable<RefreshToken>> GetAllAsync();
        Task AddAsync(RefreshToken entity);
        void Update(RefreshToken entity);
        void Delete(RefreshToken entity);
    }
}
