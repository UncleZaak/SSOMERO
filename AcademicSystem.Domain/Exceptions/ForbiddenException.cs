using System;

namespace AcademicSystem.Domain.Exceptions
{
    public class ForbiddenException : DomainException
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
