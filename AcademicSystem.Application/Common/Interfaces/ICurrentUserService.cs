using System;

namespace AcademicSystem.Application.Common.Interfaces
{
    /// <summary>
    /// Provides information about the currently authenticated user. Implementation resides in Infrastructure/API.
    /// Application layer depends only on this abstraction.
    /// </summary>
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserEmail { get; }
        bool IsAuthenticated { get; }
    }
}
