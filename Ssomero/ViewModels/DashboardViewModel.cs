using System.Collections.ObjectModel;
using System.Windows.Input;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;

namespace Ssomero.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IDashboardService _dashboard;
    private readonly IAuthService _auth;
    private readonly SessionService _session;
    private readonly IStudentScheduleService _studentSchedule;
    private readonly IRefreshCoordinator _coordinator;
    private readonly IInsightsService _insights;
    private readonly IAdminService? _admin;
    private readonly INotificationService? _notifications;
    private DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);
    private bool _isLoaded;

    public ObservableCollection<ClassModel> TodayClasses { get; } = [];

    /// <summary>
    /// Clears all cached data. Call on logout so the next user doesn't see stale state.
    /// </summary>
    public void Reset()
    {
        TodayClasses.Clear();
        TeachingClasses.Clear();
        RecentAnnouncements.Clear();
        StudentName = "Student";
        TotalStudents = 0;
        TotalLecturers = 0;
        TotalPrograms = 0;
        PendingLecturers = 0;
        AttendancePercentage = 0;
        TeachingClassesCount = 0;
        ManagedClassesCount = 0;
        Role = "Student";
        SmartInsights.Clear();
        NextClass = null;
        AttendanceWarning = false;
        _lastLoaded = DateTime.MinValue;
        _isLoaded = false;
    }

    string role = "Student";
        public string Role
        {
            get => role;
            set
            {
                if (SetProperty(ref role, value))
                {
                    // BaseViewModel exposes RaisePropertyChanged
                    RaisePropertyChanged(nameof(IsStudent));
                    RaisePropertyChanged(nameof(IsLecturer));
                    RaisePropertyChanged(nameof(IsAdmin));
                    RaisePropertyChanged(nameof(IsClassRep));
                }
            }
        }

    public bool IsStudent => Role?.ToLowerInvariant() == "student";
    public bool IsLecturer => Role?.ToLowerInvariant() == "lecturer";
    public bool IsAdmin => Role?.ToLowerInvariant() == "admin";
    public bool IsClassRep => Role?.ToLowerInvariant() == "classrep" || Role?.ToLowerInvariant() == "classrepresentative";

    // Admin summary
    int totalStudents;
    public int TotalStudents { get => totalStudents; set => SetProperty(ref totalStudents, value); }

    int totalLecturers;
    public int TotalLecturers { get => totalLecturers; set => SetProperty(ref totalLecturers, value); }

    int totalPrograms;
    public int TotalPrograms { get => totalPrograms; set => SetProperty(ref totalPrograms, value); }

    int pendingLecturers;
    public int PendingLecturers { get => pendingLecturers; set => SetProperty(ref pendingLecturers, value); }

    // Quick actions
    public ICommand GoToUsersCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/UsersPage");
    });

    public ICommand GoToAnalyticsCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/AdminAnalyticsPage");
    });

    public ICommand GoToAuditLogsCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/AuditLogsPage");
    });

    public ICommand GoToAcademicCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/UniversitiesPage");
    });

    public ICommand GoToUniversitiesCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/UniversitiesPage");
    });

    public ICommand GoToFacultiesCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/FacultiesPage");
    });

    public ICommand GoToDepartmentsCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/DepartmentsPage");
    });

    public ICommand GoToProgramsCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/ProgramsPage");
    });

    public ICommand GoToCurriculumCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//AdminApp/CurriculumPage");
    });

    public ICommand RefreshCommand => new Command(async () => await RefreshAsync());

    // Opens course detail.
    public ICommand OpenClassCommand => new Command<Guid>(async (classId) =>
    {
        await Shell.Current.GoToAsync($"course-detail?id={classId}");
    });

    // Lecturer / ClassRep counts
    int teachingClassesCount;
    public int TeachingClassesCount { get => teachingClassesCount; set => SetProperty(ref teachingClassesCount, value); }

    int managedClassesCount;
    public int ManagedClassesCount { get => managedClassesCount; set => SetProperty(ref managedClassesCount, value); }
    public ObservableCollection<ClassDto> TeachingClasses { get; } = new();
    public ObservableCollection<AnnouncementDto> RecentAnnouncements { get; } = [];

    string studentName = "Student";
    public string StudentName
    {
        get => studentName;
        set => SetProperty(ref studentName, value);
    }

    string currentDate = DateTime.Now.ToString("dddd, MMMM dd");
    public string CurrentDate
    {
        get => currentDate;
        set => SetProperty(ref currentDate, value);
    }

    int attendancePercentage;
    public int AttendancePercentage
    {
        get => attendancePercentage;
        set
        {
            if (SetProperty(ref attendancePercentage, value))
                AttendanceStatus = value >= 75 ? "On Track" : value >= 50 ? "Needs Improvement" : "At Risk";
        }
    }

    string attendanceStatus = "On Track";
    public string AttendanceStatus
    {
        get => attendanceStatus;
        set => SetProperty(ref attendanceStatus, value);
    }

    int activeCourses;
    public int ActiveCourses
    {
        get => activeCourses;
        set => SetProperty(ref activeCourses, value);
    }

    int upcomingAssignments;
    public int UpcomingAssignments
    {
        get => upcomingAssignments;
        set => SetProperty(ref upcomingAssignments, value);
    }

    string errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    bool hasNotifications = true;
    public bool HasNotifications
    {
        get => hasNotifications;
        set => SetProperty(ref hasNotifications, value);
    }

    bool hasClasses;
    public bool HasClasses
    {
        get => hasClasses;
        set => SetProperty(ref hasClasses, value);
    }

    // Next/current class for smart header
    ClassModel? nextClass;
    public ClassModel? NextClass
    {
        get => nextClass;
        set { SetProperty(ref nextClass, value); RaisePropertyChanged(nameof(HasNextClass)); RaisePropertyChanged(nameof(NextClassLabel)); }
    }
    public bool HasNextClass    => NextClass is not null;
    public string NextClassLabel
    {
        get
        {
            if (NextClass is null) return string.Empty;
            if (NextClass.Status == ClassStatus.Active) return $"Now: {NextClass.CourseName}";
            if (!TimeOnly.TryParse(NextClass.Time, out var t)) return NextClass.CourseName;
            var mins = (int)(t.ToTimeSpan() - DateTime.Now.TimeOfDay).TotalMinutes;
            return mins <= 0 ? $"Now: {NextClass.CourseName}"
                 : mins <= 30 ? $"⏰ Starting in {mins} min"
                 : $"Next: {NextClass.Time} — {NextClass.CourseName}";
        }
    }

    bool attendanceWarning;
    public bool AttendanceWarning
    {
        get => attendanceWarning;
        set => SetProperty(ref attendanceWarning, value);
    }

    public ObservableCollection<string> SmartInsights { get; } = [];
    public bool HasSmartInsights => SmartInsights.Count > 0;

    string latestAnnouncementTitle = string.Empty;
    public string LatestAnnouncementTitle
    {
        get => latestAnnouncementTitle;
        set => SetProperty(ref latestAnnouncementTitle, value);
    }

    string latestAnnouncementBody = string.Empty;
    public string LatestAnnouncementBody
    {
        get => latestAnnouncementBody;
        set => SetProperty(ref latestAnnouncementBody, value);
    }

    string latestAnnouncementDate = string.Empty;
    public string LatestAnnouncementDate
    {
        get => latestAnnouncementDate;
        set => SetProperty(ref latestAnnouncementDate, value);
    }

    bool hasAnnouncement;
    public bool HasAnnouncement
    {
        get => hasAnnouncement;
        set => SetProperty(ref hasAnnouncement, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand NavigateToProfileCommand { get; }
    public ICommand NavigateToSearchCommand { get; }
    public ICommand NavigateToNotificationsCommand { get; }
    public ICommand NavigateToCoursesCommand { get; }
    public ICommand NavigateToAssignmentsCommand { get; }
    public ICommand NavigateToNewsCommand { get; }
    public ICommand NavigateToChatCommand { get; }
    public ICommand NavigateToClassesCommand { get; }
    public ICommand NavigateToScheduleCommand { get; }
    public ICommand NavigateToAttendanceCommand { get; }
    public ICommand NavigateToMaterialsCommand { get; }
    public ICommand NavigateToGroupsCommand { get; }
    public ICommand NavigateToAnalyticsCommand { get; }
    public ICommand NavigateToPaymentsCommand { get; }

    public DashboardViewModel(
        IDashboardService dashboard,
        IAuthService auth,
        SessionService session)
        : this(dashboard, auth, session, null!, null!, null!)
    {
    }

    public DashboardViewModel(
        IDashboardService dashboard,
        IAuthService auth,
        SessionService session,
        IStudentScheduleService studentSchedule,
        IRefreshCoordinator coordinator,
        IInsightsService insights,
        IAdminService? admin = null,
        INotificationService? notifications = null)
    {
        _dashboard       = dashboard;
        _auth            = auth;
        _session         = session;
        _studentSchedule = studentSchedule;
        _coordinator     = coordinator;
        _insights        = insights;
        _admin           = admin;
        _notifications   = notifications;

        LoadCommand = new Command(async () => await LoadAsync(forceRefresh: true));
        LogoutCommand = new Command(async () => await _auth.LogoutAsync());
        NavigateToProfileCommand = new Command(async () =>
        {
            var route = _session.Role switch
            {
                UserRole.Admin => "//AdminApp/AdminProfile",
                UserRole.Lecturer => "//LecturerApp/LecturerProfile",
                UserRole.ClassRepresentative => "//ClassRepApp/ClassRepProfile",
                _ => "//StudentApp/StudentProfile"
            };
            await Shell.Current.GoToAsync(route);
        });
        NavigateToSearchCommand       = new Command(async () => await Shell.Current.GoToAsync("search"));
        NavigateToNotificationsCommand = new Command(async () => await Shell.Current.GoToAsync("notifications"));
        NavigateToCoursesCommand      = new Command(async () => await Shell.Current.GoToAsync("courses"));
        NavigateToAssignmentsCommand  = new Command(async () => await Shell.Current.GoToAsync("assignments"));
        NavigateToNewsCommand         = new Command(async () => await Shell.Current.GoToAsync("news"));
        NavigateToChatCommand         = new Command(async () => await Shell.Current.GoToAsync("chat"));
        NavigateToClassesCommand      = new Command(async () =>
        {
            var classesRoute = _session.Role == UserRole.ClassRepresentative
                ? "//ClassRepApp/ClassRepDashboardPage"
                : "//StudentApp/ClassesPage";
            await Shell.Current.GoToAsync(classesRoute);
        });
        NavigateToScheduleCommand     = new Command(async () =>
        {
            var scheduleRoute = _session.Role == UserRole.ClassRepresentative
                ? "//ClassRepApp/ClassRepSchedule"
                : "//StudentApp/DashboardPage";
            await Shell.Current.GoToAsync(scheduleRoute);
        });
        NavigateToAttendanceCommand   = new Command(async () => await Shell.Current.GoToAsync("//StudentApp/AttendancePage"));
        NavigateToMaterialsCommand    = new Command(async () => await Shell.Current.GoToAsync("//StudentApp/MaterialsPage"));
        NavigateToGroupsCommand       = new Command(async () => await Shell.Current.GoToAsync("//StudentApp/GroupsPage"));
        NavigateToAnalyticsCommand    = new Command(async () => await Shell.Current.GoToAsync("analytics"));
        NavigateToPaymentsCommand     = new Command(async () => await Shell.Current.GoToAsync("payments"));

        RefreshStudentName();

        // React to coordinator notifications (polling ticks)
        _coordinator?.Subscribe(RefreshKeys.Dashboard,    async () => await RefreshAsync());
        _coordinator?.Subscribe(RefreshKeys.Schedule,     async () => await LoadScheduleAsync());
        _coordinator?.Subscribe(RefreshKeys.Announcements, async () => await LoadAnnouncementsAsync());
    }

    private void RefreshStudentName()
    {
        var user = _session.CurrentUser;
        if (user is null)
        {
            StudentName = "Student";
            return;
        }

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            StudentName = user.FullName;
            return;
        }

        StudentName = "Student";
    }

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (_isLoaded && !forceRefresh) return;
        if (IsBusy) return;
        if (!forceRefresh && DateTime.UtcNow - _lastLoaded < RefreshInterval) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        CurrentDate = DateTime.Now.ToString("dddd, MMMM dd");
        RefreshStudentName();
        Role = _session.Role.ToString();

        try
        {
            // ---- Admin branch: fetch real admin stats, skip student data ----
            if (IsAdmin)
            {
                var stats = _admin is not null ? await _admin.GetAdminStatsAsync() : null;
                if (stats is not null)
                {
                    TotalStudents    = stats.TotalStudents;
                    TotalLecturers   = stats.TotalLecturers;
                    TotalPrograms    = stats.TotalPrograms;
                    PendingLecturers = stats.PendingLecturers;
                }
                _lastLoaded = DateTime.UtcNow;
                _isLoaded = true;
                return;
            }

            // ---- Non-admin branch: student / lecturer / class-rep ----
            var data = await _dashboard.GetDashboardAsync(forceRefresh);
            ActiveCourses = data.ActiveCourses;
            UpcomingAssignments = data.UpcomingAssignments;
            AttendancePercentage = (int)(data.AttendancePercent * 100);

            RecentAnnouncements.Clear();
            foreach (var a in data.RecentAnnouncements)
                RecentAnnouncements.Add(a);

            // role-specific data from dashboard service response
            if (data.TotalStudents.HasValue) TotalStudents = data.TotalStudents.Value;
            if (data.TotalLecturers.HasValue) TotalLecturers = data.TotalLecturers.Value;
            if (data.TotalPrograms.HasValue) TotalPrograms = data.TotalPrograms.Value;

            TeachingClassesCount = data.TeachingClasses?.Count() ?? 0;
            ManagedClassesCount = data.ManagedClasses?.Count() ?? 0;
            TeachingClasses.Clear();
            if (data.TeachingClasses is not null)
            {
                foreach (var c in data.TeachingClasses)
                {
                    TeachingClasses.Add(new ClassDto(c.Id.ToString(), c.Name, c.CourseCode, c.ParentClassId?.ToString(), c.EnrolledStudents, c.LecturerName));
                }
            }

            if (data.RecentAnnouncements.Count > 0)
            {
                var latest = data.RecentAnnouncements[0];
                LatestAnnouncementTitle = latest.Title;
                LatestAnnouncementBody = latest.Body;
                LatestAnnouncementDate = latest.Date.ToString("MMM dd, yyyy");
                HasAnnouncement = true;
            }
            else
            {
                HasAnnouncement = false;
            }

            await LoadScheduleAsync(); // load real schedule; populates TodayClasses
            BuildSmartInsights();     // uses real TodayClasses data
            _lastLoaded = DateTime.UtcNow;
            _isLoaded = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load dashboard. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task RefreshAsync()
    {
        _isLoaded = false;
        await LoadAsync(forceRefresh: true);
    }

    private void BuildSmartInsights()
    {
        var missedToday   = TodayClasses.Count(c => c.Status == ClassStatus.Completed);
        var upcomingToday = TodayClasses.Count(c => c.Status == ClassStatus.Upcoming);

        var messages = _insights?.GenerateDashboardInsights(
            AttendancePercentage, missedToday, upcomingToday, RecentAnnouncements.Count)
            ?? [];

        SmartInsights.Clear();
        foreach (var m in messages) SmartInsights.Add(m);

        AttendanceWarning = AttendancePercentage > 0 && AttendancePercentage < 75;
        RaisePropertyChanged(nameof(HasSmartInsights));
    }

    private async Task LoadScheduleAsync()
    {
        try
        {
            if (!IsStudent && !IsClassRep) return;
            if (_studentSchedule is null) return;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var sessions = await _studentSchedule.GetWeekScheduleAsync(today, today);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                TodayClasses.Clear();
                foreach (var s in sessions)
                {
                    var now    = DateTime.Now;
                    var status = s.StartTime <= now && s.EndTime >= now ? ClassStatus.Active
                               : s.StartTime > now ? ClassStatus.Upcoming
                               : ClassStatus.Completed;

                    TodayClasses.Add(new ClassModel
                    {
                        Time       = s.StartTime.ToString("HH:mm"),
                        EndTime    = s.EndTime.ToString("HH:mm"),
                        CourseName = s.CourseName,
                        Location   = s.Location,
                        Status     = status
                    });
                }
                HasClasses = TodayClasses.Count > 0;
                NextClass  = TodayClasses.FirstOrDefault(c => c.Status == ClassStatus.Active)
                          ?? TodayClasses.FirstOrDefault(c => c.Status == ClassStatus.Upcoming);
                RaisePropertyChanged(nameof(HasNextClass));
                RaisePropertyChanged(nameof(NextClassLabel));
            });

            // Rebuild OS notifications for today's sessions (fire-and-forget, non-blocking)
            if (_notifications is not null && sessions.Count > 0)
                _ = _notifications.RebuildForSessionsAsync(sessions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] LoadScheduleAsync: {ex.Message}");
        }
    }

    private Task LoadAnnouncementsAsync()
    {
        // Announcements are loaded via DashboardService.GetDashboardAsync() in LoadAsync.
        // This is a lightweight hook for the coordinator to request a selective refresh.
        return LoadAsync(forceRefresh: true);
    }
}