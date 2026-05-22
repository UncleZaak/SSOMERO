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
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EnrollmentService(IEnrollmentRepository enrollmentRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _enrollmentRepository = enrollmentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Enrollment?> GetByIdAsync(Guid id)
        {
            return await _enrollmentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Enrollment>> GetAllAsync()
        {
            return await _enrollmentRepository.GetAllAsync();
        }

        public async Task<Enrollment> CreateAsync(Enrollment enrollment)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _enrollmentRepository.AddAsync(enrollment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return enrollment;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(Enrollment enrollment)
        {
            _enrollmentRepository.Update(enrollment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _enrollmentRepository.GetByIdAsync(id);
            if (existing == null) return;
            _enrollmentRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
