using System.Threading;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces;
using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core DbContext implementing the application contract. Infrastructure-only responsibility.
    /// Maps domain entities to tables and provides persistence helpers used by the Application layer.
    /// </summary>
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<University> Universities { get; set; } = null!;
        public DbSet<Programme> Programmes { get; set; } = null!;
        public DbSet<AcademicClass> AcademicClasses { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<ClassCourse> ClassCourses { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Assessment> Assessments { get; set; } = null!;
        public DbSet<Submission> Submissions { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Infrastructure can dispatch domain events here after changes are saved or before depending on strategy.
            // For now, execute the default behavior. Domain event dispatching and auditing can be added later.
            return await base.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsEmailInUseAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalized = email.Trim().ToLowerInvariant();
            return await Students.AnyAsync(s => s.NormalizedEmail == normalized && !s.IsDeleted, cancellationToken);
        }
    }
}
