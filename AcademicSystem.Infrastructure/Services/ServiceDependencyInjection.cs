using Microsoft.Extensions.DependencyInjection;
using AcademicSystem.Application.Common.Interfaces.Services;
using AcademicSystem.Infrastructure.Services;

namespace AcademicSystem.Infrastructure
{
    public static class ServiceDependencyInjection
    {
        /// <summary>
        /// Registers service layer implementations.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IAssessmentService, AssessmentService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            return services;
        }
    }
}
