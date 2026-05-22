using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Services;

namespace Ssomero.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IApiService _api;
    private readonly SessionService _session;
    private readonly PollingService _polling;
    private readonly ILogger<LoginViewModel> _logger;

    public LoginViewModel(IAuthService authService, IApiService api, SessionService session, PollingService polling, ILogger<LoginViewModel> logger)
    {
        _authService = authService;
        _api = api;
        _session = session;
        _polling = polling;
        _logger = logger;
        LoginCommand = new Command(async () => await LoginAsync());
        GoToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("register"));
        GoToLecturerRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("register-lecturer"));
        GoBackCommand = new Command(async () =>
        {
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
                await Shell.Current.GoToAsync("..");
        });
        ForgotPasswordCommand = new Command(async () =>
            await Shell.Current.GoToAsync("forgot-password"));
        SelectStudentCommand = new Command(() => SelectedRole = "Student");
        SelectLecturerCommand = new Command(() => SelectedRole = "Lecturer");
        TogglePasswordVisibilityCommand = new Command(() => IsPasswordHidden = !IsPasswordHidden);
    }

    public LoginViewModel(IAuthService authService, IApiService api, SessionService session, ILogger<LoginViewModel> logger)
        : this(authService, api, session, null!, logger) { }

    string email = string.Empty;
    public string Email { get => email; set => SetProperty(ref email, value); }

    string password = string.Empty;
    public string Password { get => password; set => SetProperty(ref password, value); }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    string selectedRole = "Student";
    public string SelectedRole
    {
        get => selectedRole;
        set
        {
            if (SetProperty(ref selectedRole, value))
            {
                RaisePropertyChanged(nameof(IsStudentSelected));
                RaisePropertyChanged(nameof(IsLecturerSelected));
            }
        }
    }

    public bool IsStudentSelected => SelectedRole == "Student";
    public bool IsLecturerSelected => SelectedRole == "Lecturer";

    bool isPasswordHidden = true;
    public bool IsPasswordHidden
    {
        get => isPasswordHidden;
        set
        {
            if (SetProperty(ref isPasswordHidden, value))
                RaisePropertyChanged(nameof(IsPasswordVisible));
        }
    }

    public bool IsPasswordVisible => !IsPasswordHidden;

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }
    public ICommand GoToLecturerRegisterCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand ForgotPasswordCommand { get; }
    public ICommand SelectStudentCommand { get; }
    public ICommand SelectLecturerCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }

    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var healthy = await _api.CheckHealthAsync();
            if (!healthy)
            {
                ErrorMessage = "Server not reachable. Please check your connection and try again.";
                return;
            }

            var resp = await _authService.LoginAsync(Email?.Trim() ?? string.Empty, Password ?? string.Empty);
            if (resp is null)
            {
                ErrorMessage = "Unexpected server response. Please try again.";
                return;
            }

            if (resp.User is not null)
                _session.SetUser(resp.User);

            // Persist role for shell redirection and restore
            var roleName = _session.Role.ToString();
            try
            {
                if (!string.IsNullOrWhiteSpace(roleName))
                    await SecureStorage.SetAsync("user_role", roleName);
            }
            catch
            {
                // ignore secure storage failures
            }

            // Safe navigation guard
            if (string.IsNullOrWhiteSpace(roleName))
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            // Delegate navigation to DashboardNavigator
            await Ssomero.Navigation.DashboardNavigator.GoToDashboardAsync(roleName);
            _polling?.Start();
        }
        catch (HttpRequestException httpEx)
        {
            ErrorMessage = httpEx.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed unexpectedly");
            ErrorMessage = ex is TaskCanceledException
                ? "The request timed out. Please check your connection."
                : $"Login failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
