using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces;
using AutoMapper;
using AcademicSystem.Application.Common.Interfaces.Repositories;
using AcademicSystem.Application.Common.Interfaces.Services;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Infrastructure.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<RefreshToken?> GetByIdAsync(Guid id)
        {
            return await _refreshTokenRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<RefreshToken>> GetAllAsync()
        {
            return await _refreshTokenRepository.GetAllAsync();
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken token)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _refreshTokenRepository.AddAsync(token);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return token;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            _refreshTokenRepository.Update(token);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _refreshTokenRepository.GetByIdAsync(id);
            if (existing == null) return;
            _refreshTokenRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
