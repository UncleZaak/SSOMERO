using System.Reflection;
using Microcharts.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Ssomero.Configuration;

namespace Ssomero
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMicrocharts()
                .UseLocalNotification(config =>
                {
                    config.AddAndroid(android =>
                    {
                        android.AddChannel(new NotificationChannelRequest
                        {
                            Id          = "ssomero_classes",
                            Name        = "Class Reminders",
                            Description = "Upcoming class and attendance notifications",
                            Importance  = AndroidImportance.High,
                            EnableVibration = true
                        });
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configuration
            var assembly = Assembly.GetExecutingAssembly();
            builder.Configuration.AddJsonStream(assembly.GetManifestResourceStream("Ssomero.appsettings.json")!);

            var devStream = assembly.GetManifestResourceStream("Ssomero.appsettings.Development.json");
            if (devStream is not null)
                builder.Configuration.AddJsonStream(devStream);

            var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();
            builder.Services.AddSingleton(apiSettings);

            // Logging
            builder.Logging.AddDebug();
#if !DEBUG
            // In Release/Maui production reduce verbosity and rely on platform crash reporting
            builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif

            // HttpClient configured from settings
            // On Android emulator, localhost refers to the emulator itself.
            // Replace with 10.0.2.2 which routes to the host machine's loopback.
            builder.Services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<ApiSettings>();
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HttpClientSetup");
                var baseUrl = settings.BaseUrl;

#if ANDROID
                if (baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                    baseUrl = baseUrl.Replace("localhost", "10.0.2.2", StringComparison.OrdinalIgnoreCase);
#endif

                logger.LogInformation("HttpClient BaseAddress = {BaseUrl}, Timeout = {Timeout}s",
                    baseUrl, settings.TimeoutSeconds);

                var handler = new HttpClientHandler();
#if DEBUG
                // Trust all certs during dev (self-signed / dev certs)
                handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif

                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(baseUrl),
                    Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
                };
            });

            builder.Services.AddSingleton<Interfaces.ITopBarService, Services.TopBarService>();
            builder.Services.AddSingleton<Interfaces.IProfilePhotoService, Services.ProfilePhotoService>();

            // Services
            builder.Services.AddSingleton<Services.TokenStorageService>();
            builder.Services.AddSingleton<Services.SessionService>();
            builder.Services.AddSingleton<Interfaces.IApiService, Services.ApiService>();
            builder.Services.AddSingleton<Interfaces.IAuthService, Services.AuthService>();
            builder.Services.AddSingleton<Interfaces.IProfileService, Services.ProfileService>();
            builder.Services.AddSingleton<Interfaces.ICoursesService, Services.CoursesService>();
            builder.Services.AddSingleton<Interfaces.IAnnouncementsService, Services.AnnouncementsService>();
            builder.Services.AddSingleton<Interfaces.IDashboardService, Services.DashboardService>();
            builder.Services.AddSingleton<Interfaces.IScheduleService, Services.ScheduleService>();
            builder.Services.AddSingleton<Interfaces.IRollcallService, Services.RollcallService>();
            builder.Services.AddSingleton<Interfaces.IAcademicService, Services.AcademicService>();
            builder.Services.AddSingleton<Interfaces.IAdminService, Services.AdminService>();
            builder.Services.AddSingleton<Interfaces.IMaterialsService, Services.MaterialsService>();
            builder.Services.AddSingleton<Interfaces.IGroupsService, Services.GroupsService>();
            builder.Services.AddSingleton<Interfaces.IPaymentsService, Services.PaymentsService>();
            builder.Services.AddSingleton<Interfaces.IAttendanceService, Services.AttendanceService>();
            builder.Services.AddSingleton<Interfaces.IStudentScheduleService, Services.StudentScheduleService>();
            builder.Services.AddSingleton<Interfaces.ICacheService, Services.CacheService>();
            builder.Services.AddSingleton<Interfaces.IRefreshCoordinator, Services.RefreshCoordinatorService>();
            builder.Services.AddSingleton<Interfaces.INotificationService, Services.NotificationService>();
            builder.Services.AddSingleton<Interfaces.IInsightsService, Services.InsightsService>();
            builder.Services.AddSingleton<Interfaces.IToastService, Services.ToastService>();
            builder.Services.AddSingleton<Interfaces.ILecturerApiService, Services.LecturerApiService>();
            builder.Services.AddSingleton<Interfaces.IClassRepApiService, Services.ClassRepApiService>();
            builder.Services.AddSingleton<Interfaces.IClassAnnouncementApiService, Services.ClassAnnouncementApiService>();
            builder.Services.AddSingleton<Interfaces.IClassElectionApiService, Services.ClassElectionApiService>();
            builder.Services.AddSingleton<Services.PollingService>();
            builder.Services.AddSingleton<Navigation.INavigationDefinitionService, Navigation.NavigationDefinitionService>();

            // Converters
            builder.Services.AddSingleton<Converters.NullToBoolConverter>();
            builder.Services.AddSingleton<Converters.PercentToDoubleConverter>();
            builder.Services.AddSingleton<Converters.StatusToColorConverter>();
            builder.Services.AddSingleton<Converters.InvertBoolConverter>();
            builder.Services.AddSingleton<Converters.AttendanceColorConverter>();

            // ViewModels
            builder.Services.AddTransient<ViewModels.LoginViewModel>();
            builder.Services.AddTransient<ViewModels.RegisterViewModel>();
            builder.Services.AddTransient<ViewModels.LecturerRegisterViewModel>();
            builder.Services.AddTransient<ViewModels.ForgotPasswordViewModel>();
            builder.Services.AddTransient<ViewModels.ResetPasswordViewModel>();
            builder.Services.AddTransient<ViewModels.ChangePasswordViewModel>();
            // DashboardViewModel must be a singleton to preserve state across role pages
            builder.Services.AddSingleton<ViewModels.DashboardViewModel>();
            builder.Services.AddTransient<ViewModels.CoursesViewModel>();
            builder.Services.AddTransient<ViewModels.CourseDetailViewModel>();
            builder.Services.AddTransient<ViewModels.AnnouncementsViewModel>();
            builder.Services.AddTransient<ViewModels.ScheduleViewModel>();
            builder.Services.AddTransient<ViewModels.UsersViewModel>();
            builder.Services.AddTransient<ViewModels.ProfileViewModel>();
            builder.Services.AddTransient<ViewModels.UniversitiesViewModel>();
            builder.Services.AddTransient<ViewModels.FacultiesViewModel>();
            builder.Services.AddTransient<ViewModels.ClassesViewModel>();
            builder.Services.AddTransient<ViewModels.AttendanceViewModel>();
            builder.Services.AddTransient<ViewModels.AttendanceMarkViewModel>();
            builder.Services.AddTransient<ViewModels.MaterialsViewModel>();
            builder.Services.AddTransient<ViewModels.GroupsViewModel>();
            builder.Services.AddTransient<ViewModels.GroupChatViewModel>();
            builder.Services.AddTransient<ViewModels.PaymentsViewModel>();
            builder.Services.AddTransient<ViewModels.PaymentHistoryViewModel>();
            builder.Services.AddTransient<ViewModels.AnalyticsViewModel>();

            // Pages
            builder.Services.AddTransient<Views.Auth.LoginPage>();
            builder.Services.AddTransient<Views.Auth.RegisterPage>();
            builder.Services.AddTransient<Views.Auth.LecturerRegisterPage>();
            builder.Services.AddTransient<Views.Auth.ForgotPasswordPage>();
            builder.Services.AddTransient<Views.Auth.ResetPasswordPage>();
            builder.Services.AddTransient<Views.Profile.ChangePasswordPage>();
            builder.Services.AddTransient<Views.Dashboard.DashboardPage>();
            builder.Services.AddTransient<Views.Courses.CoursesPage>();
            builder.Services.AddTransient<Views.Courses.CourseDetailPage>();
            builder.Services.AddTransient<Views.Announcements.AnnouncementsPage>();
            builder.Services.AddTransient<Views.Schedule.SchedulePage>();
            builder.Services.AddTransient<Views.Grades.GradesPage>();
            builder.Services.AddTransient<Views.Profile.ProfilePage>();
            builder.Services.AddTransient<Views.Assignments.AssignmentsPage>();
            builder.Services.AddTransient<Views.Chat.ChatPage>();
            builder.Services.AddTransient<Views.Search.SearchPage>();
            builder.Services.AddTransient<Views.Notifications.NotificationsPage>();
            builder.Services.AddTransient<Views.Admin.UsersPage>();
            builder.Services.AddTransient<Views.Admin.UniversitiesPage>();
            builder.Services.AddTransient<Views.Admin.FacultiesPage>();
            builder.Services.AddTransient<Views.Admin.DepartmentsPage>();
            builder.Services.AddTransient<Views.Admin.ProgramsPage>();
            builder.Services.AddTransient<Views.Admin.CurriculumPage>();
            builder.Services.AddTransient<Views.Admin.AdminAnalyticsPage>();
            builder.Services.AddTransient<Views.Admin.AuditLogsPage>();
            builder.Services.AddTransient<ViewModels.DepartmentsViewModel>();
            builder.Services.AddTransient<ViewModels.ProgramsViewModel>();
            builder.Services.AddTransient<ViewModels.CurriculumViewModel>();
            builder.Services.AddTransient<ViewModels.AdminAnalyticsViewModel>();
            builder.Services.AddTransient<ViewModels.AuditLogsViewModel>();
            builder.Services.AddTransient<Views.Dashboard.StudentDashboardPage>();
            builder.Services.AddTransient<Views.Dashboard.LecturerDashboardPage>();
            builder.Services.AddTransient<Views.Dashboard.AdminDashboardPage>();
            builder.Services.AddTransient<Views.Dashboard.ClassRepDashboardPage>();

            // Lecturer ViewModels
            builder.Services.AddTransient<ViewModels.LecturerDashboardViewModel>();
            builder.Services.AddTransient<ViewModels.LecturerClassesViewModel>();
            builder.Services.AddTransient<ViewModels.LecturerClassDetailsViewModel>();
            builder.Services.AddTransient<ViewModels.LecturerAttendanceViewModel>();
            builder.Services.AddTransient<ViewModels.LecturerMaterialsViewModel>();

            // Lecturer Pages
            builder.Services.AddTransient<Views.Lecturer.LecturerClassesPage>();
            builder.Services.AddTransient<Views.Lecturer.LecturerClassDetailsPage>();
            builder.Services.AddTransient<Views.Lecturer.LecturerAttendancePage>();
            builder.Services.AddTransient<Views.Lecturer.LecturerMaterialsPage>();
            builder.Services.AddTransient<Views.Student.ClassesPage>();
            builder.Services.AddTransient<Views.Student.AttendancePage>();
            builder.Services.AddTransient<Views.Student.AttendanceMarkPage>();
            builder.Services.AddTransient<Views.Student.MaterialsPage>();
            builder.Services.AddTransient<Views.Student.GroupsPage>();
            builder.Services.AddTransient<Views.Student.GroupChatPage>();
            builder.Services.AddTransient<Views.Student.AnalyticsPage>();
            builder.Services.AddTransient<Views.Student.PaymentsPage>();
            builder.Services.AddTransient<Views.Student.PaymentHistoryPage>();

            // ClassRep ViewModels
            builder.Services.AddTransient<ViewModels.ClassRepViewModel>();
            builder.Services.AddTransient<ViewModels.ClassRepStudentsViewModel>();
            builder.Services.AddTransient<ViewModels.ClassRepLecturersViewModel>();
            builder.Services.AddTransient<ViewModels.ClassRepAttendanceViewModel>();
            builder.Services.AddTransient<ViewModels.ClassRepAnnouncementsViewModel>();
            builder.Services.AddTransient<ViewModels.ClassRepAnalyticsViewModel>();
            builder.Services.AddTransient<ViewModels.ClassElectionViewModel>(sp =>
                new ViewModels.ClassElectionViewModel(
                    sp.GetRequiredService<Interfaces.IClassElectionApiService>(),
                    sp.GetRequiredService<Interfaces.INotificationService>(),
                    sp.GetRequiredService<Services.SessionService>()));

            // ClassRep Pages
            builder.Services.AddTransient<Views.ClassRep.ClassRepMyClassPage>();
            builder.Services.AddTransient<Views.ClassRep.ClassRepStudentsPage>();
            builder.Services.AddTransient<Views.ClassRep.ClassRepLecturersPage>();
            builder.Services.AddTransient<Views.ClassRep.ClassRepAttendancePage>();
            builder.Services.AddTransient<Views.ClassRep.ClassRepAnnouncementsPage>();
            builder.Services.AddTransient<Views.ClassRep.ClassRepAnalyticsPage>();
            builder.Services.AddTransient<Views.Elections.ClassElectionPage>();

            return builder.Build();
        }
    }
}
