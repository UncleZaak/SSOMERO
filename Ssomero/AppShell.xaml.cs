using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Ssomero.Interfaces;
using Ssomero.Navigation;

namespace Ssomero
{
    public partial class AppShell : Shell
    {
        private readonly INavigationDefinitionService _navDefs;
        private readonly ITopBarService _topBarService;

        // ── Flyout header bindable properties ────────────────────────────────

        public static readonly BindableProperty FlyoutUserNameProperty =
            BindableProperty.Create(nameof(FlyoutUserName), typeof(string), typeof(AppShell), "Ssomero");

        public static readonly BindableProperty FlyoutUserRoleProperty =
            BindableProperty.Create(nameof(FlyoutUserRole), typeof(string), typeof(AppShell), string.Empty);

        public static readonly BindableProperty FlyoutInitialsProperty =
            BindableProperty.Create(nameof(FlyoutInitials), typeof(string), typeof(AppShell), "S");

        public static readonly BindableProperty FlyoutHasPhotoProperty =
            BindableProperty.Create(nameof(FlyoutHasPhoto), typeof(bool), typeof(AppShell), false);

        public static readonly BindableProperty FlyoutPhotoUrlProperty =
            BindableProperty.Create(nameof(FlyoutPhotoUrl), typeof(string), typeof(AppShell), null);

        public static readonly BindableProperty FlyoutRoleKeyProperty =
            BindableProperty.Create(nameof(FlyoutRoleKey), typeof(string), typeof(AppShell), string.Empty);

        public string FlyoutUserName { get => (string)GetValue(FlyoutUserNameProperty);  set => SetValue(FlyoutUserNameProperty, value); }
        public string FlyoutUserRole { get => (string)GetValue(FlyoutUserRoleProperty);  set => SetValue(FlyoutUserRoleProperty, value); }
        public string FlyoutInitials { get => (string)GetValue(FlyoutInitialsProperty);  set => SetValue(FlyoutInitialsProperty, value); }
        public bool   FlyoutHasPhoto { get => (bool)GetValue(FlyoutHasPhotoProperty);    set => SetValue(FlyoutHasPhotoProperty, value); }
        public string? FlyoutPhotoUrl { get => (string?)GetValue(FlyoutPhotoUrlProperty); set => SetValue(FlyoutPhotoUrlProperty, value); }
        public string FlyoutRoleKey  { get => (string)GetValue(FlyoutRoleKeyProperty);   set => SetValue(FlyoutRoleKeyProperty, value); }

        // ── Current nav items (bound to CollectionView in FlyoutContent) ─────
        public ObservableCollection<NavigationItemDefinition> CurrentNavItems { get; } = [];

        // ── Navigate command (handles routing + logout) ───────────────────────
        public ICommand NavigateCommand { get; }

        public AppShell()
        {
            var services = IPlatformApplication.Current!.Services;
            _navDefs       = services.GetRequiredService<INavigationDefinitionService>();
            _topBarService = services.GetRequiredService<ITopBarService>();

            NavigateCommand = new Command<NavigationItemDefinition>(OnNavigate);

            // Keep flyout header in sync whenever profile changes (e.g. after photo update)
            _topBarService.ProfileChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(SyncFlyoutFromTopBarService);

            InitializeComponent();
            RegisterRoutes();
        }

        // ── Public API called by DashboardNavigator ───────────────────────────

        /// <summary>
        /// Loads the role-based flyout navigation list and refreshes user identity
        /// in both the flyout header and the AppTopBar singleton.
        /// </summary>
        public async Task RebuildFlyoutAsync(string role)
        {
            // Refresh identity state (API call + SecureStorage sync)
            await _topBarService.LoadAsync(forceRefresh: true);

            // Sync flyout header from the freshly loaded service
            SyncFlyoutFromTopBarService();

            // Rebuild navigation items for the role
            CurrentNavItems.Clear();
            foreach (var item in _navDefs.GetItemsForRole(role))
                CurrentNavItems.Add(item);
        }

        /// <summary>Clears the flyout and identity state. Called on logout.</summary>
        public void ClearFlyout()
        {
            _topBarService.Clear();
            SyncFlyoutFromTopBarService();
            CurrentNavItems.Clear();
        }

        // ── TopBarService → flyout sync ───────────────────────────────────────

        private void SyncFlyoutFromTopBarService()
        {
            FlyoutUserName = _topBarService.FullName;
            FlyoutUserRole = RoleDisplayName(_topBarService.Role);
            FlyoutInitials = _topBarService.Initials;
            FlyoutHasPhoto = _topBarService.HasPhoto;
            FlyoutPhotoUrl = _topBarService.PhotoUrlWithVersion;
            FlyoutRoleKey  = _topBarService.Role;
        }

        // ── Command handler ───────────────────────────────────────────────────

        private async void OnNavigate(NavigationItemDefinition? item)
        {
            if (item is null || item.IsSeparator) return;

            FlyoutIsPresented = false;

            if (item.IsLogout)
            {
                ClearFlyout();
                var auth = IPlatformApplication.Current!.Services.GetRequiredService<IAuthService>();
                await auth.LogoutAsync();
                return;
            }

            await GoToAsync(item.Route);
        }

        // ── Route registration ────────────────────────────────────────────────

        private static void RegisterRoutes()
        {
            Routing.RegisterRoute("course-detail",           typeof(Views.Courses.CourseDetailPage));
            Routing.RegisterRoute("register",                typeof(Views.Auth.RegisterPage));
            Routing.RegisterRoute("register-lecturer",       typeof(Views.Auth.LecturerRegisterPage));
            Routing.RegisterRoute("forgot-password",         typeof(Views.Auth.ForgotPasswordPage));
            Routing.RegisterRoute("reset-password",          typeof(Views.Auth.ResetPasswordPage));
            Routing.RegisterRoute("change-password",         typeof(Views.Profile.ChangePasswordPage));

            Routing.RegisterRoute("courses",                 typeof(Views.Courses.CoursesPage));
            Routing.RegisterRoute("assignments",             typeof(Views.Assignments.AssignmentsPage));
            Routing.RegisterRoute("news",                    typeof(Views.Announcements.AnnouncementsPage));
            Routing.RegisterRoute("chat",                    typeof(Views.Chat.ChatPage));
            Routing.RegisterRoute("search",                  typeof(Views.Search.SearchPage));
            Routing.RegisterRoute("notifications",           typeof(Views.Notifications.NotificationsPage));

            // Student feature routes (pushed, not tab items)
            Routing.RegisterRoute("attendance-mark",         typeof(Views.Student.AttendanceMarkPage));
            Routing.RegisterRoute("group-chat",              typeof(Views.Student.GroupChatPage));
            Routing.RegisterRoute("analytics",               typeof(Views.Student.AnalyticsPage));
            Routing.RegisterRoute("payments",                typeof(Views.Student.PaymentsPage));
            Routing.RegisterRoute("payment-history",        typeof(Views.Student.PaymentHistoryPage));
            Routing.RegisterRoute("admin-analytics",         typeof(Views.Admin.AdminAnalyticsPage));
            Routing.RegisterRoute("audit-logs",              typeof(Views.Admin.AuditLogsPage));

            // Lecturer detail routes (pushed, not tab items)
            Routing.RegisterRoute("lecturer-class-details",  typeof(Views.Lecturer.LecturerClassDetailsPage));
            Routing.RegisterRoute("lecturer-class-students", typeof(Views.Lecturer.LecturerClassDetailsPage));
            Routing.RegisterRoute("lecturer-attendance",     typeof(Views.Lecturer.LecturerAttendancePage));
            Routing.RegisterRoute("lecturer-materials",      typeof(Views.Lecturer.LecturerMaterialsPage));

            // Elections
            Routing.RegisterRoute("ClassElectionPage", typeof(Views.Elections.ClassElectionPage));

            // ClassRep management routes (pushed on top of ClassRepApp shell)
            Routing.RegisterRoute("ClassRepMyClassPage",       typeof(Views.ClassRep.ClassRepMyClassPage));
            Routing.RegisterRoute("ClassRepStudentsPage",      typeof(Views.ClassRep.ClassRepStudentsPage));
            Routing.RegisterRoute("ClassRepLecturersPage",     typeof(Views.ClassRep.ClassRepLecturersPage));
            Routing.RegisterRoute("ClassRepAttendancePage",    typeof(Views.ClassRep.ClassRepAttendancePage));
            Routing.RegisterRoute("ClassRepAnnouncementsPage", typeof(Views.ClassRep.ClassRepAnnouncementsPage));
            Routing.RegisterRoute("ClassRepAnalyticsPage",     typeof(Views.ClassRep.ClassRepAnalyticsPage));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string RoleDisplayName(string role) => role?.ToLowerInvariant() switch
        {
            "admin"                               => "Administrator",
            "lecturer"                            => "Lecturer",
            "classrepresentative" or "classrep"   => "Class Representative",
            _                                     => "Student",
        };
    }
}

