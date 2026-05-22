using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AcademicSystem.Application.Common.Interfaces;
using AcademicSystem.Application.Common.Models;
using AcademicSystem.Domain.Entities;

namespace AcademicSystem.Application.Features.Auth.Commands
{
    public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterStudentCommandHandler(IApplicationDbContext dbContext, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<Guid>> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
        {
            // Final uniqueness check to avoid race condition; db implementation may enforce unique index.
            if (await _dbContext.IsEmailInUseAsync(request.Email, cancellationToken))
            {
                return Result<Guid>.Failure("Email is already in use.");
            }

            var student = new Student(request.Email, request.UniversityId, request.ProgrammeId, request.AcademicYearId, request.SemesterId);
            var hashed = _passwordHasher.HashPassword(request.Password);
            student.SetPasswordHash(hashed);

            // Add to DbSet - implementation should attach entity to context.
            _dbContext.Students.Add(student);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(student.Id);
        }
    }
}
