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
    /// <summary>
    /// Implementation of student business logic.
    /// Uses repositories for data access and provides validation and transactions.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentService(IStudentRepository studentRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Student?> GetByIdAsync(Guid id)
        {
            return await _studentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _studentRepository.GetAllAsync();
        }

        public async Task<Student> CreateAsync(Student student)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _studentRepository.AddAsync(student);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return student;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(Student student)
        {
            _studentRepository.Update(student);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _studentRepository.GetByIdAsync(id);
            if (existing == null) return;
            _studentRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
