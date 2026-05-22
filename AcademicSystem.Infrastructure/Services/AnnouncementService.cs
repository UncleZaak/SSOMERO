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
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AnnouncementService(IAnnouncementRepository announcementRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _announcementRepository = announcementRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Announcement?> GetByIdAsync(Guid id)
        {
            return await _announcementRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await _announcementRepository.GetAllAsync();
        }

        public async Task<Announcement> CreateAsync(Announcement announcement)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _announcementRepository.AddAsync(announcement);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return announcement;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(Announcement announcement)
        {
            _announcementRepository.Update(announcement);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _announcementRepository.GetByIdAsync(id);
            if (existing == null) return;
            _announcementRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
