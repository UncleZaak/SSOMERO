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
    public class AssessmentService : IAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AssessmentService(IAssessmentRepository assessmentRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _assessmentRepository = assessmentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Assessment?> GetByIdAsync(Guid id)
        {
            return await _assessmentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Assessment>> GetAllAsync()
        {
            return await _assessmentRepository.GetAllAsync();
        }

        public async Task<Assessment> CreateAsync(Assessment assessment)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _assessmentRepository.AddAsync(assessment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return assessment;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(Assessment assessment)
        {
            _assessmentRepository.Update(assessment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _assessmentRepository.GetByIdAsync(id);
            if (existing == null) return;
            _assessmentRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
