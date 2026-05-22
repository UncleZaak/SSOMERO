namespace Ssomero.Navigation;

/// <summary>
/// Provides additive route-level authorization checks.
/// All existing navigation continues to work — this guard only adds UI-side
/// visibility checks to complement backend authorization.
/// </summary>
public static class RouteGuard
{
    // Routes that are restricted to specific roles.
    // Routes NOT listed here are implicitly accessible (no UI restriction).
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> RouteRoles =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["//AdminApp"]                    = Roles("Admin"),
            ["//AdminApp/AdminDashboardPage"] = Roles("Admin"),
            ["//AdminApp/UsersPage"]          = Roles("Admin"),
            ["//AdminApp/UniversitiesPage"]   = Roles("Admin"),
            ["//AdminApp/FacultiesPage"]      = Roles("Admin"),
            ["//AdminApp/DepartmentsPage"]    = Roles("Admin"),
            ["//AdminApp/ProgramsPage"]       = Roles("Admin"),
            ["//AdminApp/CurriculumPage"]     = Roles("Admin"),
            ["//AdminApp/AdminAnalyticsPage"] = Roles("Admin"),
            ["//AdminApp/AuditLogsPage"]      = Roles("Admin"),
            ["//AdminApp/AdminProfile"]       = Roles("Admin"),

            ["//LecturerApp"]                       = Roles("Lecturer"),
            ["//LecturerApp/LecturerDashboardPage"] = Roles("Lecturer"),
            ["//LecturerApp/LecturerClassesPage"]   = Roles("Lecturer"),
            ["//LecturerApp/LecturerSchedule"]      = Roles("Lecturer"),
            ["//LecturerApp/LecturerProfile"]       = Roles("Lecturer"),

            ["//ClassRepApp"]                         = Roles("ClassRepresentative", "ClassRep"),
            ["//ClassRepApp/ClassRepDashboardPage"]   = Roles("ClassRepresentative", "ClassRep"),
            ["//ClassRepApp/ClassRepSchedule"]        = Roles("ClassRepresentative", "ClassRep"),
            ["//ClassRepApp/ClassRepGrades"]          = Roles("ClassRepresentative", "ClassRep"),
            ["//ClassRepApp/ClassRepProfile"]         = Roles("ClassRepresentative", "ClassRep"),
        };

    /// <summary>
    /// Returns <c>true</c> if <paramref name="role"/> is permitted to access
    /// <paramref name="route"/> at the UI level.
    /// When no rule exists for a route, access is allowed (open route).
    /// </summary>
    public static bool CanAccessRoute(string role, string route)
    {
        if (!RouteRoles.TryGetValue(route, out var allowed))
            return true;

        // Normalise ClassRep aliases
        var normalised = role?.ToLowerInvariant() switch
        {
            "classrep" or "class_representative" => "ClassRepresentative",
            _ => role ?? string.Empty,
        };

        return allowed.Contains(normalised, StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlySet<string> Roles(params string[] roles) =>
        new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
}
