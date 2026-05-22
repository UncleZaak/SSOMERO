using Ssomero.Models;

namespace Ssomero.Services;

/// <summary>
/// Holds the current user's session state (role, profile) for the lifetime of the app.
/// Populated after login; cleared on logout.
/// </summary>
public class SessionService
{
    public AuthUserDto? CurrentUser { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public void SetUser(AuthUserDto user)
    {
        CurrentUser = user;
        Role = user.Role?.ToLowerInvariant() switch
        {
            "admin" => UserRole.Admin,
            "lecturer" => UserRole.Lecturer,
            "classrepresentative" or "classrep" or "class_representative" => UserRole.ClassRepresentative,
            _ => UserRole.Student
        };
    }

    public void Clear()
    {
        CurrentUser = null;
        Role = UserRole.Student;
    }
}
