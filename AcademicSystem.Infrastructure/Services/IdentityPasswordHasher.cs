using System;
using AcademicSystem.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AcademicSystem.Infrastructure.Services
{
    /// <summary>
    /// Infrastructure implementation of <see cref="IPasswordHasher"/> using
    /// ASP.NET Core Identity's <see cref="PasswordHasher{TUser}"/>. Uses a
    /// generic object as the user type because the Application layer must not
    /// depend on Identity types.
    /// </summary>
    public class IdentityPasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

        public string HashPassword(string password)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));
            return _hasher.HashPassword(null, password);
        }

        public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword is null) throw new ArgumentNullException(nameof(hashedPassword));
            if (providedPassword is null) throw new ArgumentNullException(nameof(providedPassword));

            var result = _hasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
