using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using AcademicSystem.Application.Common.Interfaces;

namespace AcademicSystem.Application.Features.Auth.Commands
{
    public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
    {
        public RegisterStudentCommandValidator(IApplicationDbContext dbContext)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email is required.")
                .DependentRules(() =>
                {
                    RuleFor(x => x)
                        .MustAsync(async (cmd, ct) => !await dbContext.IsEmailInUseAsync(cmd.Email, ct))
                        .WithMessage("Email is already in use.");
                });

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.UniversityId).NotEmpty().WithMessage("UniversityId is required.");
            RuleFor(x => x.ProgrammeId).NotEmpty().WithMessage("ProgrammeId is required.");
            RuleFor(x => x.AcademicYearId).NotEmpty().WithMessage("AcademicYearId is required.");
            RuleFor(x => x.SemesterId).NotEmpty().WithMessage("SemesterId is required.");
        }
    }
}
