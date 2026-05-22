using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class LecturerRegisterViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly ILogger<LecturerRegisterViewModel> _logger;

    public LecturerRegisterViewModel(IAuthService auth, ILogger<LecturerRegisterViewModel> logger)
    {
        _auth = auth;
        _logger = logger;

        SendOtpCommand               = new AsyncRelayCommand(SendOtpAsync);
        VerifyOtpCommand             = new AsyncRelayCommand(VerifyOtpAsync);
        ResendOtpCommand             = new AsyncRelayCommand(ResendOtpAsync, () => CanResendOtp);
        RegisterCommand              = new AsyncRelayCommand(RegisterAsync);
        GoBackCommand                = new Command(GoBack);
        TogglePasswordCommand        = new Command(() => { IsPasswordHidden        = !IsPasswordHidden; });
        ToggleConfirmPasswordCommand = new Command(() => { IsConfirmPasswordHidden = !IsConfirmPasswordHidden; });
    }

    // ---------- Step tracking ----------
    int currentStep = 1;
    public int CurrentStep
    {
        get => currentStep;
        set
        {
            if (SetProperty(ref currentStep, value))
                RaisePropertyChanged(nameof(StepLabel));
        }
    }

    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;

    /// <summary>e.g. "Step 2 of 2"</summary>
    public string StepLabel => $"Step {CurrentStep} of 2";

    // ---------- OTP ----------
    string email = string.Empty;
    public string Email { get => email; set => SetProperty(ref email, value); }

    string otpCode = string.Empty;
    public string OtpCode { get => otpCode; set => SetProperty(ref otpCode, value); }

    bool isOtpSent;
    public bool IsOtpSent { get => isOtpSent; set => SetProperty(ref isOtpSent, value); }

    bool isOtpVerified;
    public bool IsOtpVerified { get => isOtpVerified; set => SetProperty(ref isOtpVerified, value); }

    // Stored privately — never exposed to the View
    private string? _verificationToken;

    // ---------- OTP resend cooldown ----------
    private IDispatcherTimer? _cooldownTimer;
    int _otpCooldownSeconds;
    public int OtpCooldownSeconds
    {
        get => _otpCooldownSeconds;
        private set
        {
            if (SetProperty(ref _otpCooldownSeconds, value))
            {
                RaisePropertyChanged(nameof(CanResendOtp));
                RaisePropertyChanged(nameof(OtpCooldownText));
                ResendOtpCommand.NotifyCanExecuteChanged();
            }
        }
    }
    public bool CanResendOtp     => OtpCooldownSeconds == 0 && IsOtpSent && !IsOtpVerified;
    public string OtpCooldownText => OtpCooldownSeconds > 0 ? $"Resend in {OtpCooldownSeconds}s" : "Resend OTP";

    // ---------- Password visibility ----------
    bool isPasswordHidden = true;
    public bool IsPasswordHidden
    {
        get => isPasswordHidden;
        set { if (SetProperty(ref isPasswordHidden, value)) RaisePropertyChanged(nameof(PasswordToggleIcon)); }
    }
    public string PasswordToggleIcon => IsPasswordHidden ? "\U0001F441" : "\U0001F648";

    bool isConfirmPasswordHidden = true;
    public bool IsConfirmPasswordHidden
    {
        get => isConfirmPasswordHidden;
        set { if (SetProperty(ref isConfirmPasswordHidden, value)) RaisePropertyChanged(nameof(ConfirmPasswordToggleIcon)); }
    }
    public string ConfirmPasswordToggleIcon => IsConfirmPasswordHidden ? "\U0001F441" : "\U0001F648";

    // ---------- Personal info ----------
    string firstName = string.Empty;
    public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }

    string lastName = string.Empty;
    public string LastName { get => lastName; set => SetProperty(ref lastName, value); }

    string phone = string.Empty;
    public string Phone { get => phone; set => SetProperty(ref phone, value); }

    string staffId = string.Empty;
    public string StaffId { get => staffId; set => SetProperty(ref staffId, value); }

    string password = string.Empty;
    public string Password { get => password; set => SetProperty(ref password, value); }

    string confirmPassword = string.Empty;
    public string ConfirmPassword { get => confirmPassword; set => SetProperty(ref confirmPassword, value); }

    // ---------- UI ----------
    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    public IAsyncRelayCommand SendOtpCommand               { get; }
    public IAsyncRelayCommand VerifyOtpCommand             { get; }
    public IAsyncRelayCommand ResendOtpCommand             { get; }
    public IAsyncRelayCommand RegisterCommand              { get; }
    public ICommand           GoBackCommand                { get; }
    public ICommand           TogglePasswordCommand        { get; }
    public ICommand           ToggleConfirmPasswordCommand { get; }

    // ---------- Commands ----------
    private async Task SendOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Enter your email first.";
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var ok = await _auth.SendOtpAsync(Email.Trim());
            if (ok)
            {
                IsOtpSent = true;
                StartOtpCooldown();
            }
            else
                ErrorMessage = "Failed to send OTP. Please try again.";
        }
        catch (Exception ex)
        {
            HandleException(ex, "Send OTP");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task VerifyOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(OtpCode))
        {
            ErrorMessage = "Enter the OTP code.";
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var token = await _auth.VerifyOtpAsync(Email.Trim(), OtpCode.Trim());
            if (token is not null)
            {
                _verificationToken = token;
                IsOtpVerified = true;
                _cooldownTimer?.Stop();
                OtpCooldownSeconds = 0;
                ErrorMessage = "\u2713 Email verified successfully.";
                CurrentStep = 2;
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
            }
            else
            {
                ErrorMessage = "Invalid or expired OTP.";
            }
        }
        catch (Exception ex)
        {
            HandleException(ex, "Verify OTP");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        // OTP gate — cannot be bypassed
        if (!IsOtpVerified)
        {
            ErrorMessage = "Please verify your email with an OTP first.";
            return;
        }

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "First name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Last name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Phone) || Phone.Trim().Length < 7)
        {
            ErrorMessage = "A valid phone number is required.";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(Password,
                @"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$"))
        {
            ErrorMessage = "Password must contain an uppercase letter, a number and a special character.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsBusy = true;

        try
        {
            var dto = new LecturerRegisterDto
            {
                FirstName         = FirstName.Trim(),
                LastName          = LastName.Trim(),
                Email             = Email.Trim(),
                Phone             = Phone.Trim(),
                Password          = Password,
                StaffId           = string.IsNullOrWhiteSpace(StaffId) ? null : StaffId.Trim(),
                VerificationToken = _verificationToken ?? string.Empty
            };

            await _auth.RegisterLecturerAsync(dto);

            _logger.LogInformation("Lecturer registered: {Email}", dto.Email);
            await Shell.Current.DisplayAlert(
                "Registration Submitted",
                "Your registration is pending admin approval. You will be notified once approved.",
                "OK");
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            HandleException(ex, "Registration");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void HandleException(Exception ex, string operation)
    {
        _logger.LogError(ex, "{Operation} failed", operation);
        ErrorMessage = ex switch
        {
            TaskCanceledException => "The request timed out. Check your connection and try again.",
            HttpRequestException httpEx when !string.IsNullOrWhiteSpace(httpEx.Message) => httpEx.Message,
            _ => $"{operation} failed. Please try again."
        };
    }

    protected void OnPropertyChanged(string name) => RaisePropertyChanged(name);

    // ---------- Cooldown timer ----------
    private void StartOtpCooldown()
    {
        OtpCooldownSeconds = 60;
        _cooldownTimer?.Stop();
        _cooldownTimer = Application.Current!.Dispatcher.CreateTimer();
        _cooldownTimer.Interval = TimeSpan.FromSeconds(1);
        _cooldownTimer.Tick += (_, _) =>
        {
            if (OtpCooldownSeconds > 0)
                OtpCooldownSeconds--;
            else
                _cooldownTimer.Stop();
        };
        _cooldownTimer.Start();
    }

    private async Task ResendOtpAsync()
    {
        OtpCode = string.Empty;
        await SendOtpAsync();
    }

    private void GoBack()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            OnPropertyChanged(nameof(IsStep1));
            OnPropertyChanged(nameof(IsStep2));
            ErrorMessage = string.Empty;
        }
    }
}
