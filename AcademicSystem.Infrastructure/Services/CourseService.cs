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
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(ICourseRepository courseRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Course?> GetByIdAsync(Guid id)
        {
            return await _courseRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _courseRepository.GetAllAsync();
        }

        public async Task<Course> CreateAsync(Course course)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _courseRepository.AddAsync(course);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return course;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(Course course)
        {
            _courseRepository.Update(course);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _courseRepository.GetByIdAsync(id);
            if (existing == null) return;
            _courseRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
