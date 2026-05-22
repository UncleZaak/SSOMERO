using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AcademicSystem.Application.Common.Interfaces;
using AcademicSystem.Infrastructure.Persistence;

namespace AcademicSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var conn = configuration.GetConnectionString("DefaultConnection");
                // Register the migrations assembly explicitly to this Infrastructure assembly so
                // EF tools generate migrations in the correct project.
                options.UseSqlServer(conn, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
            });

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // AutoMapper registration: registers MappingProfile from Application project
            services.AddAutoMapper(typeof(AcademicSystem.Application.Common.Mappings.MappingProfile));
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // Unit of Work registration for transaction management
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Infrastructure implementations
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.IPasswordHasher, AcademicSystem.Infrastructure.Services.IdentityPasswordHasher>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.IAuthService, AcademicSystem.Infrastructure.Services.AuthService>();

            // Register repositories
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IStudentRepository, AcademicSystem.Infrastructure.Repositories.StudentRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IUserRepository, AcademicSystem.Infrastructure.Repositories.UserRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.ICourseRepository, AcademicSystem.Infrastructure.Repositories.CourseRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IAcademicClassRepository, AcademicSystem.Infrastructure.Repositories.AcademicClassRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IEnrollmentRepository, AcademicSystem.Infrastructure.Repositories.EnrollmentRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IAssessmentRepository, AcademicSystem.Infrastructure.Repositories.AssessmentRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.ISubmissionRepository, AcademicSystem.Infrastructure.Repositories.SubmissionRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IAnnouncementRepository, AcademicSystem.Infrastructure.Repositories.AnnouncementRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IProgrammeRepository, AcademicSystem.Infrastructure.Repositories.ProgrammeRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IUniversityRepository, AcademicSystem.Infrastructure.Repositories.UniversityRepository>();
            services.AddScoped<AcademicSystem.Application.Common.Interfaces.Repositories.IRefreshTokenRepository, AcademicSystem.Infrastructure.Repositories.RefreshTokenRepository>();

            // Other infrastructure services (IEmailService, IPaymentGateway, IFileStorageService) to be registered here.

            return services;
        }
    }
}
