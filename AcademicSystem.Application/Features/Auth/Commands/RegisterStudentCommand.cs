using System;
using MediatR;
using AcademicSystem.Application.Common.Models;

namespace AcademicSystem.Application.Features.Auth.Commands
{
    public class RegisterStudentCommand : IRequest<Result<Guid>>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public Guid UniversityId { get; set; }
        public Guid ProgrammeId { get; set; }
        public Guid AcademicYearId { get; set; }
        public Guid SemesterId { get; set; }
    }
}
