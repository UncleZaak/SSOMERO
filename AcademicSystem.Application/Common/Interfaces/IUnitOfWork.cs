using System;
using System.Threading.Tasks;

namespace AcademicSystem.Application.Common.Interfaces
{
    /// <summary>
    /// Unit of Work contract to control transactions and save operations.
    /// Implementations should be registered as scoped services.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Save changes to the database asynchronously.
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begin a new database transaction asynchronously.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit the current transaction asynchronously.
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback the current transaction asynchronously.
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
