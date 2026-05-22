using System;
using System.Net.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Configuration;
using Ssomero.Converters;
using Ssomero.Interfaces;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Admin;
using Ssomero.Views.Announcements;
using Ssomero.Views.Assignments;
using Ssomero.Views.Auth;
using Ssomero.Views.Chat;
using Ssomero.Views.Courses;
using Ssomero.Views.Dashboard;
using Ssomero.Views.Grades;
using Ssomero.Views.Notifications;
using Ssomero.Views.Profile;
using Ssomero.Views.Schedule;
using Ssomero.Views.Search;

namespace Ssomero.UnitTests
{
    /// <summary>
    /// Tests for the MauiProgram class.
    /// </summary>
    /// <remarks>
    /// Note: CreateMauiApp is a bootstrap/configuration method that relies heavily on
    /// non-mockable MAUI framework types and static methods. These tests are integration-style
    /// tests that verify the actual configuration and service registration behavior.
    /// Tests may fail if embedded resources (appsettings.json) are not available in the test context.
    /// </remarks>
    [TestClass]
    public class MauiProgramTests
    {
        /// <summary>
        /// Tests that CreateMauiApp executes successfully and returns a non-null MauiApp instance.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_ValidExecution_ReturnsNonNullMauiApp()
        {
            // Arrange & Act
            MauiApp? app = null;
            Exception? exception = null;

            try
            {
                app = MauiProgram.CreateMauiApp();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, $"CreateMauiApp should not throw an exception. Exception: {exception?.Message}");
            Assert.IsNotNull(app, "CreateMauiApp should return a non-null MauiApp instance.");
        }

        /// <summary>
        /// Tests that ApiSettings singleton service is registered and can be resolved from the service provider.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_ApiSettingsRegistration_CanResolveApiSettings()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            ApiSettings? apiSettings = app.Services.GetService<ApiSettings>();

            // Assert
            Assert.IsNotNull(apiSettings, "ApiSettings should be registered and resolvable from the service provider.");
        }

        /// <summary>
        /// Tests that all singleton services are properly registered and can be resolved.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(TokenStorageService))]
        [DataRow(typeof(SessionService))]
        [DataRow(typeof(IApiService))]
        [DataRow(typeof(IAuthService))]
        [DataRow(typeof(ICoursesService))]
        [DataRow(typeof(IAnnouncementsService))]
        [DataRow(typeof(IDashboardService))]
        [DataRow(typeof(IScheduleService))]
        [DataRow(typeof(IRollcallService))]
        [DataRow(typeof(IAcademicService))]
        [DataRow(typeof(IAdminService))]
        [DataRow(typeof(NullToBoolConverter))]
        [DataRow(typeof(PercentToDoubleConverter))]
        [DataRow(typeof(StatusToColorConverter))]
        [DataRow(typeof(DashboardViewModel))]
        public void CreateMauiApp_SingletonServices_CanResolveService(Type serviceType)
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            object? service = app.Services.GetService(serviceType);

            // Assert
            Assert.IsNotNull(service, $"Service of type {serviceType.Name} should be registered and resolvable.");
        }

        /// <summary>
        /// Tests that all transient view model services are properly registered and can be resolved.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(LoginViewModel))]
        [DataRow(typeof(RegisterViewModel))]
        [DataRow(typeof(CoursesViewModel))]
        [DataRow(typeof(CourseDetailViewModel))]
        [DataRow(typeof(AnnouncementsViewModel))]
        [DataRow(typeof(ScheduleViewModel))]
        [DataRow(typeof(UsersViewModel))]
        [DataRow(typeof(ProfileViewModel))]
        [DataRow(typeof(UniversitiesViewModel))]
        [DataRow(typeof(FacultiesViewModel))]
        public void CreateMauiApp_TransientViewModels_CanResolveViewModel(Type viewModelType)
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            object? viewModel = app.Services.GetService(viewModelType);

            // Assert
            Assert.IsNotNull(viewModel, $"ViewModel of type {viewModelType.Name} should be registered and resolvable.");
        }

        /// <summary>
        /// Tests that all transient page services are properly registered and can be resolved.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(LoginPage))]
        [DataRow(typeof(RegisterPage))]
        [DataRow(typeof(DashboardPage))]
        [DataRow(typeof(CoursesPage))]
        [DataRow(typeof(CourseDetailPage))]
        [DataRow(typeof(AnnouncementsPage))]
        [DataRow(typeof(SchedulePage))]
        [DataRow(typeof(GradesPage))]
        [DataRow(typeof(ProfilePage))]
        [DataRow(typeof(AssignmentsPage))]
        [DataRow(typeof(ChatPage))]
        [DataRow(typeof(SearchPage))]
        [DataRow(typeof(NotificationsPage))]
        [DataRow(typeof(UsersPage))]
        [DataRow(typeof(UniversitiesPage))]
        [DataRow(typeof(FacultiesPage))]
        [DataRow(typeof(StudentDashboardPage))]
        [DataRow(typeof(LecturerDashboardPage))]
        [DataRow(typeof(AdminDashboardPage))]
        [DataRow(typeof(ClassRepDashboardPage))]
        public void CreateMauiApp_TransientPages_CanResolvePage(Type pageType)
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            object? page = app.Services.GetService(pageType);

            // Assert
            Assert.IsNotNull(page, $"Page of type {pageType.Name} should be registered and resolvable.");
        }

        /// <summary>
        /// Tests that HttpClient singleton service is registered and properly configured.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_HttpClientRegistration_CanResolveHttpClient()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            HttpClient? httpClient = app.Services.GetService<HttpClient>();

            // Assert
            Assert.IsNotNull(httpClient, "HttpClient should be registered and resolvable from the service provider.");
            Assert.IsNotNull(httpClient.BaseAddress, "HttpClient should have a BaseAddress configured.");
            Assert.IsTrue(httpClient.Timeout.TotalSeconds > 0, "HttpClient should have a positive timeout configured.");
        }

        /// <summary>
        /// Tests that the same singleton instance is returned on multiple resolutions.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_SingletonServices_ReturnsSameInstance()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            TokenStorageService? instance1 = app.Services.GetService<TokenStorageService>();
            TokenStorageService? instance2 = app.Services.GetService<TokenStorageService>();

            // Assert
            Assert.IsNotNull(instance1, "First resolution should return non-null instance.");
            Assert.IsNotNull(instance2, "Second resolution should return non-null instance.");
            Assert.AreSame(instance1, instance2, "Singleton service should return the same instance on multiple resolutions.");
        }

        /// <summary>
        /// Tests that different transient instances are returned on multiple resolutions.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_TransientServices_ReturnsDifferentInstances()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            LoginViewModel? instance1 = app.Services.GetService<LoginViewModel>();
            LoginViewModel? instance2 = app.Services.GetService<LoginViewModel>();

            // Assert
            Assert.IsNotNull(instance1, "First resolution should return non-null instance.");
            Assert.IsNotNull(instance2, "Second resolution should return non-null instance.");
            Assert.AreNotSame(instance1, instance2, "Transient service should return different instances on multiple resolutions.");
        }

        /// <summary>
        /// Tests that ILoggerFactory is available and can create loggers.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_LoggingConfiguration_CanResolveLoggerFactory()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            ILoggerFactory? loggerFactory = app.Services.GetService<ILoggerFactory>();
            ILogger? logger = loggerFactory?.CreateLogger("Test");

            // Assert
            Assert.IsNotNull(loggerFactory, "ILoggerFactory should be registered and resolvable.");
            Assert.IsNotNull(logger, "ILoggerFactory should be able to create logger instances.");
        }

        /// <summary>
        /// Tests that ApiSettings has default or configured values after being loaded from configuration.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_ApiSettingsConfiguration_HasValidProperties()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            ApiSettings? apiSettings = app.Services.GetService<ApiSettings>();

            // Assert
            Assert.IsNotNull(apiSettings, "ApiSettings should be resolvable.");
            Assert.IsNotNull(apiSettings.BaseUrl, "BaseUrl should not be null.");
            Assert.IsTrue(apiSettings.TimeoutSeconds > 0, "TimeoutSeconds should be greater than 0.");
        }

        /// <summary>
        /// Tests that HttpClient singleton service is registered and properly configured with non-null BaseAddress.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_HttpClientRegistration_HasNonNullBaseAddress()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            HttpClient? httpClient = app.Services.GetService<HttpClient>();

            // Assert
            Assert.IsNotNull(httpClient, "HttpClient should be registered and resolvable.");
            Assert.IsNotNull(httpClient.BaseAddress, "HttpClient BaseAddress should be configured.");
        }

        /// <summary>
        /// Tests that HttpClient timeout is properly configured from ApiSettings.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_HttpClientConfiguration_TimeoutMatchesApiSettings()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();
            ApiSettings? apiSettings = app.Services.GetService<ApiSettings>();

            // Act
            HttpClient? httpClient = app.Services.GetService<HttpClient>();

            // Assert
            Assert.IsNotNull(httpClient, "HttpClient should be resolvable.");
            Assert.IsNotNull(apiSettings, "ApiSettings should be resolvable.");
            Assert.AreEqual(TimeSpan.FromSeconds(apiSettings.TimeoutSeconds), httpClient.Timeout,
                "HttpClient Timeout should match ApiSettings TimeoutSeconds.");
        }

        /// <summary>
        /// Tests that HttpClient BaseAddress is properly configured from ApiSettings.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_HttpClientConfiguration_BaseAddressFromApiSettings()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();
            ApiSettings? apiSettings = app.Services.GetService<ApiSettings>();

            // Act
            HttpClient? httpClient = app.Services.GetService<HttpClient>();

            // Assert
            Assert.IsNotNull(httpClient, "HttpClient should be resolvable.");
            Assert.IsNotNull(apiSettings, "ApiSettings should be resolvable.");
            Assert.IsNotNull(httpClient.BaseAddress, "HttpClient BaseAddress should be set.");

            string expectedBaseUrl = apiSettings.BaseUrl;
#if ANDROID
            if (expectedBaseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                expectedBaseUrl = expectedBaseUrl.Replace("localhost", "10.0.2.2", StringComparison.OrdinalIgnoreCase);
#endif

            Assert.AreEqual(expectedBaseUrl, httpClient.BaseAddress.ToString().TrimEnd('/'),
                "HttpClient BaseAddress should match ApiSettings BaseUrl (with platform-specific adjustments).");
        }

        /// <summary>
        /// Tests that IConfiguration is available and can retrieve configuration sections.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_ConfigurationSetup_CanResolveConfiguration()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            IConfiguration? configuration = app.Services.GetService<IConfiguration>();

            // Assert
            Assert.IsNotNull(configuration, "IConfiguration should be registered and resolvable.");
        }

        /// <summary>
        /// Tests that DashboardViewModel is registered as singleton and returns the same instance.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_DashboardViewModelSingleton_ReturnsSameInstance()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            DashboardViewModel? instance1 = app.Services.GetService<DashboardViewModel>();
            DashboardViewModel? instance2 = app.Services.GetService<DashboardViewModel>();

            // Assert
            Assert.IsNotNull(instance1, "First DashboardViewModel instance should be resolvable.");
            Assert.IsNotNull(instance2, "Second DashboardViewModel instance should be resolvable.");
            Assert.AreSame(instance1, instance2, "DashboardViewModel should be registered as singleton and return the same instance.");
        }

        /// <summary>
        /// Tests that all required converter services are registered correctly.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(NullToBoolConverter))]
        [DataRow(typeof(PercentToDoubleConverter))]
        [DataRow(typeof(StatusToColorConverter))]
        public void CreateMauiApp_ConverterServices_CanResolveConverter(Type converterType)
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            object? converter = app.Services.GetService(converterType);

            // Assert
            Assert.IsNotNull(converter, $"Converter of type {converterType.Name} should be registered and resolvable.");
        }

        /// <summary>
        /// Tests that ApiSettings is registered as singleton and returns the same instance.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_ApiSettingsSingleton_ReturnsSameInstance()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            ApiSettings? instance1 = app.Services.GetService<ApiSettings>();
            ApiSettings? instance2 = app.Services.GetService<ApiSettings>();

            // Assert
            Assert.IsNotNull(instance1, "First ApiSettings instance should be resolvable.");
            Assert.IsNotNull(instance2, "Second ApiSettings instance should be resolvable.");
            Assert.AreSame(instance1, instance2, "ApiSettings should be registered as singleton and return the same instance.");
        }

        /// <summary>
        /// Tests that HttpClient is registered as singleton and returns the same instance.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_HttpClientSingleton_ReturnsSameInstance()
        {
            // Arrange
            MauiApp app = MauiProgram.CreateMauiApp();

            // Act
            HttpClient? instance1 = app.Services.GetService<HttpClient>();
            HttpClient? instance2 = app.Services.GetService<HttpClient>();

            // Assert
            Assert.IsNotNull(instance1, "First HttpClient instance should be resolvable.");
            Assert.IsNotNull(instance2, "Second HttpClient instance should be resolvable.");
            Assert.AreSame(instance1, instance2, "HttpClient should be registered as singleton and return the same instance.");
        }
    }
}