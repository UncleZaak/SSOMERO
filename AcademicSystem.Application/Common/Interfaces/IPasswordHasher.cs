namespace AcademicSystem.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction for hashing passwords. Infrastructure should provide a secure implementation
    /// (ASP.NET Core Identity password hasher or equivalent) that is not part of the Application layer.
    /// </summary>
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
}
