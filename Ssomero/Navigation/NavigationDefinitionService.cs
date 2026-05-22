namespace Ssomero.Navigation;

/// <summary>
/// Centralised navigation definition service — single source of truth for all
/// role-based flyout navigation in the Ssomero platform.
/// </summary>
public sealed class NavigationDefinitionService : INavigationDefinitionService
{
    // ── Student ──────────────────────────────────────────────────────────────
    private static readonly IReadOnlyList<NavigationItemDefinition> StudentItems =
    [
        new() { Title = "Dashboard",  Route = "//StudentApp/DashboardPage",  Icon = "🏠" },
        new() { Title = "My Classes", Route = "//StudentApp/ClassesPage",    Icon = "📚" },
        new() { Title = "Attendance", Route = "//StudentApp/AttendancePage", Icon = "✅" },
        new() { Title = "Materials",  Route = "//StudentApp/MaterialsPage",  Icon = "📎" },
        new() { Title = "Groups",     Route = "//StudentApp/GroupsPage",     Icon = "👥" },
        new() { Title = "Class Rep Elections", Route = "ClassElectionPage", Icon = "🗳️" },
        new() { Title = "Profile",    Route = "//StudentApp/StudentProfile", Icon = "👤" },
        new() { IsSeparator = true },
        new() { Title = "Log Out", IsLogout = true, Icon = "🚪" },
    ];

    // ── Lecturer ─────────────────────────────────────────────────────────────
    private static readonly IReadOnlyList<NavigationItemDefinition> LecturerItems =
    [
        new() { Title = "Dashboard",  Route = "//LecturerApp/LecturerDashboardPage", Icon = "🏠" },
        new() { Title = "My Classes", Route = "//LecturerApp/LecturerClassesPage",   Icon = "📚" },
        new() { Title = "Schedule",   Route = "//LecturerApp/LecturerSchedule",      Icon = "📅" },
        new() { Title = "Profile",    Route = "//LecturerApp/LecturerProfile",       Icon = "👤" },
        new() { IsSeparator = true },
        new() { Title = "Log Out", IsLogout = true, Icon = "🚪" },
    ];

    // ── Admin ─────────────────────────────────────────────────────────────────
    private static readonly IReadOnlyList<NavigationItemDefinition> AdminItems =
    [
        new() { Title = "Dashboard",    Route = "//AdminApp/AdminDashboardPage", Icon = "📊" },
        new() { Title = "Users",        Route = "//AdminApp/UsersPage",          Icon = "👥" },
        new() { Title = "Universities", Route = "//AdminApp/UniversitiesPage",   Icon = "🏫" },
        new() { Title = "Faculties",    Route = "//AdminApp/FacultiesPage",      Icon = "🏛️" },
        new() { Title = "Departments",  Route = "//AdminApp/DepartmentsPage",    Icon = "🏢" },
        new() { Title = "Programs",     Route = "//AdminApp/ProgramsPage",       Icon = "📚" },
        new() { Title = "Curriculum",   Route = "//AdminApp/CurriculumPage",     Icon = "📋" },
        new() { Title = "Analytics",    Route = "//AdminApp/AdminAnalyticsPage", Icon = "📈" },
        new() { Title = "Audit Logs",   Route = "//AdminApp/AuditLogsPage",      Icon = "🗒️" },
        new() { Title = "Profile",      Route = "//AdminApp/AdminProfile",       Icon = "👤" },
        new() { IsSeparator = true },
        new() { Title = "Log Out", IsLogout = true, Icon = "🚪" },
    ];

    // ── Class Representative ──────────────────────────────────────────────────
    private static readonly IReadOnlyList<NavigationItemDefinition> ClassRepItems =
    [
        new() { Title = "Dashboard",          Route = "//ClassRepApp/ClassRepDashboardPage", Icon = "🏠" },
        new() { Title = "My Class",           Route = "ClassRepMyClassPage",                 Icon = "📚" },
        new() { Title = "Announcements",      Route = "ClassRepAnnouncementsPage",           Icon = "📢" },
        new() { Title = "Attendance Reports", Route = "ClassRepAttendancePage",              Icon = "📊" },
        new() { Title = "Analytics",          Route = "ClassRepAnalyticsPage",               Icon = "📈" },
        new() { Title = "Schedule",           Route = "//ClassRepApp/ClassRepSchedule",      Icon = "📅" },
        new() { Title = "Class Rep Elections", Route = "ClassElectionPage", Icon = "🗳️" },
        new() { Title = "Profile",            Route = "//ClassRepApp/ClassRepProfile",       Icon = "👤" },
        new() { IsSeparator = true },
        new() { Title = "Log Out", IsLogout = true, Icon = "🚪" },
    ];

    /// <inheritdoc/>
    public IReadOnlyList<NavigationItemDefinition> GetItemsForRole(string role) => role switch
    {
        "Admin"               => AdminItems,
        "Lecturer"            => LecturerItems,
        "ClassRepresentative" => ClassRepItems,
        "ClassRep"            => ClassRepItems,
        _                     => StudentItems,   // Student + fallback
    };
}
