using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction over persistence used by the Application layer. Implemented by Infrastructure.
    /// Keep methods minimal and intent-revealing.
    /// </summary>
    public interface IApplicationDbContext
    {
        DbSet<Student> Students { get; }
        DbSet<User> Users { get; }
        DbSet<University> Universities { get; }
        DbSet<Programme> Programmes { get; }
        DbSet<AcademicClass> AcademicClasses { get; }
        DbSet<Course> Courses { get; }
        DbSet<ClassCourse> ClassCourses { get; }
        DbSet<Enrollment> Enrollments { get; }
        DbSet<Assessment> Assessments { get; }
        DbSet<Submission> Submissions { get; }
        DbSet<Announcement> Announcements { get; }
        DbSet<RefreshToken> RefreshTokens { get; }

        /// <summary>
        /// Save changes with cancellation token. Implementations should dispatch domain events and handle concurrency.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Check whether an email is already registered for a non-deleted user (student/lecturer).
        /// The application layer uses this to enforce uniqueness without depending on Identity.
        /// </summary>
        Task<bool> IsEmailInUseAsync(string email, CancellationToken cancellationToken = default);
    }
}
