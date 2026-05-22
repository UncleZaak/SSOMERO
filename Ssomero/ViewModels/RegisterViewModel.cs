using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IAcademicService _academic;
    private readonly ILogger<RegisterViewModel> _logger;

    public RegisterViewModel(IAuthService auth, IAcademicService academic, ILogger<RegisterViewModel> logger)
    {
        _auth = auth;
        _academic = academic;
        _logger = logger;

        SendOtpCommand    = new AsyncRelayCommand(SendOtpAsync);
        VerifyOtpCommand  = new AsyncRelayCommand(VerifyOtpAsync);
        ResendOtpCommand  = new AsyncRelayCommand(ResendOtpAsync, () => CanResendOtp);
        RegisterCommand   = new AsyncRelayCommand(RegisterAsync);
        RetryLookupsCommand          = new AsyncRelayCommand(RetryLookupsAsync);
        GoBackCommand     = new Command(GoBack);
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
    public bool IsStep3 => CurrentStep == 3;

    /// <summary>e.g. "Step 2 of 3"</summary>
    public string StepLabel => $"Step {CurrentStep} of 3";

    // ---------- Personal info ----------
    string firstName = string.Empty;
    public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }

    string secondName = string.Empty;
    public string SecondName { get => secondName; set => SetProperty(ref secondName, value); }

    string otherNames = string.Empty;
    public string OtherNames { get => otherNames; set => SetProperty(ref otherNames, value); }

    DateTime dob = DateTime.Now.AddYears(-18);
    public DateTime Dob { get => dob; set => SetProperty(ref dob, value); }

    string gender = string.Empty;
    public string Gender { get => gender; set => SetProperty(ref gender, value); }

    string phone = string.Empty;
    public string Phone { get => phone; set => SetProperty(ref phone, value); }

    string email = string.Empty;
    public string Email { get => email; set => SetProperty(ref email, value); }

    string password = string.Empty;
    public string Password { get => password; set => SetProperty(ref password, value); }

    string confirmPassword = string.Empty;
    public string ConfirmPassword { get => confirmPassword; set => SetProperty(ref confirmPassword, value); }

    // ---------- OTP ----------
    string otpCode = string.Empty;
    public string OtpCode { get => otpCode; set => SetProperty(ref otpCode, value); }

    bool isOtpSent;
    public bool IsOtpSent { get => isOtpSent; set => SetProperty(ref isOtpSent, value); }

    bool isOtpVerified;
    public bool IsOtpVerified { get => isOtpVerified; set => SetProperty(ref isOtpVerified, value); }

    string? verificationToken;

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
    public bool CanResendOtp   => OtpCooldownSeconds == 0 && IsOtpSent && !IsOtpVerified;
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

    // ---------- Lookup error / retry ----------
    bool hasLookupError;
    public bool HasLookupError { get => hasLookupError; private set => SetProperty(ref hasLookupError, value); }

    // ---------- Academic profile (cascading) ----------
    public ObservableCollection<LookupItem> Universities { get; } = [];
    public ObservableCollection<LookupItem> Faculties { get; } = [];
    public ObservableCollection<LookupItem> Departments { get; } = [];
    public ObservableCollection<LookupItem> Programs { get; } = [];
    public ObservableCollection<LookupItem> EntrySchemes { get; } = [];
    public ObservableCollection<LookupItem> Intakes { get; } = [];
    public ObservableCollection<LookupItem> StudyModes { get; } = [];
    public ObservableCollection<LookupItem> AcademicYears { get; } = [];
    public ObservableCollection<LookupItem> Semesters { get; } = [];

    LookupItem? selectedUniversity;
    public LookupItem? SelectedUniversity
    {
        get => selectedUniversity;
        set
        {
            if (SetProperty(ref selectedUniversity, value))
            {
                SelectedFaculty = null;
                Faculties.Clear();
                Departments.Clear();
                Programs.Clear();
                if (value is not null)
                    _ = LoadFacultiesAsync(value.Id);
            }
        }
    }

    LookupItem? selectedFaculty;
    public LookupItem? SelectedFaculty
    {
        get => selectedFaculty;
        set
        {
            if (SetProperty(ref selectedFaculty, value))
            {
                SelectedDepartment = null;
                Departments.Clear();
                Programs.Clear();
                if (value is not null)
                    _ = LoadDepartmentsAsync(value.Id);
            }
        }
    }

    LookupItem? selectedDepartment;
    public LookupItem? SelectedDepartment
    {
        get => selectedDepartment;
        set
        {
            if (SetProperty(ref selectedDepartment, value))
            {
                SelectedProgram = null;
                Programs.Clear();
                if (value is not null)
                    _ = LoadProgramsAsync(value.Id);
            }
        }
    }

    LookupItem? selectedProgram;
    public LookupItem? SelectedProgram { get => selectedProgram; set => SetProperty(ref selectedProgram, value); }

    LookupItem? selectedEntryScheme;
    public LookupItem? SelectedEntryScheme { get => selectedEntryScheme; set => SetProperty(ref selectedEntryScheme, value); }

    LookupItem? selectedIntake;
    public LookupItem? SelectedIntake { get => selectedIntake; set => SetProperty(ref selectedIntake, value); }

    LookupItem? selectedStudyMode;
    public LookupItem? SelectedStudyMode { get => selectedStudyMode; set => SetProperty(ref selectedStudyMode, value); }

    LookupItem? selectedAcademicYear;
    public LookupItem? SelectedAcademicYear { get => selectedAcademicYear; set => SetProperty(ref selectedAcademicYear, value); }

    int yearOfStudy = 1;
    public int YearOfStudy { get => yearOfStudy; set => SetProperty(ref yearOfStudy, value); }

    LookupItem? selectedSemester;
    public LookupItem? SelectedSemester { get => selectedSemester; set => SetProperty(ref selectedSemester, value); }

    // ---------- UI ----------
    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    public IAsyncRelayCommand SendOtpCommand    { get; }
    public IAsyncRelayCommand VerifyOtpCommand  { get; }
    public IAsyncRelayCommand ResendOtpCommand  { get; }
    public IAsyncRelayCommand RegisterCommand   { get; }
    public IAsyncRelayCommand RetryLookupsCommand { get; }
    public ICommand           GoBackCommand     { get; }
    public ICommand           TogglePasswordCommand        { get; }
    public ICommand           ToggleConfirmPasswordCommand { get; }

    bool _lookupsLoaded;

    // ---------- Init (load lookups) ----------
    public async Task InitAsync()
    {
        if (_lookupsLoaded)
            return;

        HasLookupError = false;
        IsBusy = true;
        try
        {
            await Task.WhenAll(
                LoadLookupAsync(Universities, () => _academic.GetUniversitiesAsync()),
                LoadLookupAsync(EntrySchemes, () => _academic.GetEntrySchemesAsync()),
                LoadLookupAsync(Intakes, () => _academic.GetIntakesAsync()),
                LoadLookupAsync(StudyModes, () => _academic.GetStudyModesAsync()),
                LoadLookupAsync(AcademicYears, () => _academic.GetAcademicYearsAsync()),
                LoadLookupAsync(Semesters, () => _academic.GetSemestersAsync())
            );
            _lookupsLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load academic lookups");
            HasLookupError = true;
            ErrorMessage = "Could not load academic data. Please check your connection.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Called by the Retry button on Step 3 when lookup loading failed.</summary>
    public async Task RetryLookupsAsync()
    {
        _lookupsLoaded = false;
        await InitAsync();
    }

    private async Task LoadFacultiesAsync(string universityId)
    {
        try
        {
            await LoadLookupAsync(Faculties, () => _academic.GetFacultiesAsync(universityId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load faculties");
            ErrorMessage = "Failed to load faculties. Check your connection.";
        }
    }

    private async Task LoadDepartmentsAsync(string facultyId)
    {
        try
        {
            await LoadLookupAsync(Departments, () => _academic.GetDepartmentsAsync(facultyId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load departments");
            ErrorMessage = "Failed to load departments. Check your connection.";
        }
    }

    private async Task LoadProgramsAsync(string departmentId)
    {
        try
        {
            await LoadLookupAsync(Programs, () => _academic.GetProgramsAsync(departmentId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load programs");
            ErrorMessage = "Failed to load programs. Check your connection.";
        }
    }

    private async Task LoadLookupAsync(ObservableCollection<LookupItem> target, Func<Task<IEnumerable<LookupItem>>> fetcher)
    {
        target.Clear();
        foreach (var item in await fetcher())
            target.Add(item);
    }

    // ---------- OTP ----------
    private async Task SendOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Enter your email first.";
            return;
        }

        if (IsBusy)
            return;

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
            {
                ErrorMessage = "Failed to send OTP. Please try again later.";
            }
        }
        catch (Exception ex)
        {
            HandleCommandException(ex, "Send OTP");
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

        if (IsBusy)
            return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var token = await _auth.VerifyOtpAsync(Email.Trim(), OtpCode.Trim());
            if (token is not null)
            {
                verificationToken = token;
                IsOtpVerified = true;
                CurrentStep = 2;
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
                OnPropertyChanged(nameof(IsStep3));
                // Cooldown no longer needed once verified
                _cooldownTimer?.Stop();
                OtpCooldownSeconds = 0;
            }
            else
            {
                ErrorMessage = "Invalid or expired OTP.";
            }
        }
        catch (Exception ex)
        {
            HandleCommandException(ex, "Verify OTP");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void GoToStep3()
    {
        ErrorMessage = string.Empty;

        // ---- Client-side validation before advancing to the academic profile step ----
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "First name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SecondName))
        {
            ErrorMessage = "Last name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Phone) || Phone.Trim().Length < 7)
        {
            ErrorMessage = "A valid phone number is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Gender))
        {
            ErrorMessage = "Please select your gender.";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(Password, @"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$"))
        {
            ErrorMessage = "Password must contain an uppercase letter, a number and a special character.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        CurrentStep = 3;
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
    }

    // ---------- Register ----------
    private async Task RegisterAsync()
    {
        if (IsBusy)
            return;

        if (!IsOtpVerified)
        {
            ErrorMessage = "Please verify your email with an OTP first.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            if (SelectedUniversity is null || SelectedFaculty is null || SelectedDepartment is null ||
                SelectedProgram is null || SelectedEntryScheme is null || SelectedIntake is null ||
                SelectedStudyMode is null || SelectedAcademicYear is null || SelectedSemester is null)
            {
                ErrorMessage = "Please complete all academic profile fields.";
                return;
            }

            var dto = new StudentRegisterDto
            {
                FirstName = FirstName.Trim(),
                SecondName = SecondName.Trim(),
                OtherNames = string.IsNullOrWhiteSpace(OtherNames) ? null : OtherNames.Trim(),
                Dob = Dob.ToString("yyyy-MM-dd"),
                Gender = Gender,
                Phone = Phone.Trim(),
                Email = Email.Trim(),
                Password = Password,
                VerificationToken = verificationToken ?? string.Empty,
                UniversityId = SelectedUniversity.Id,
                FacultyId = SelectedFaculty.Id,
                DepartmentId = SelectedDepartment.Id,
                ProgramId = SelectedProgram.Id,
                EntrySchemeId = SelectedEntryScheme.Id,
                IntakeId = SelectedIntake.Id,
                StudyModeId = SelectedStudyMode.Id,
                AcademicYearId = SelectedAcademicYear.Id,
                YearOfStudy = YearOfStudy,
                SemesterId = SelectedSemester.Id,
            };

            var success = await _auth.RegisterStudentAsync(dto);
            if (!success)
            {
                ErrorMessage = "Registration failed. Please check your details and try again.";
                return;
            }

            _logger.LogInformation("Student registered: {Email}", dto.Email);
            await Shell.Current.DisplayAlert(
                "Registration Successful! 🎉",
                "Your account has been created. You can now log in.",
                "Go to Login");
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            HandleCommandException(ex, "Registration");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void HandleCommandException(Exception ex, string operation)
    {
        _logger.LogError(ex, "{Operation} failed", operation);

        ErrorMessage = ex switch
        {
            TaskCanceledException => "The request timed out. Check your connection and try again.",
            HttpRequestException httpEx when !string.IsNullOrWhiteSpace(httpEx.Message) => httpEx.Message,
            _ => $"{operation} failed. Please try again."
        };
    }

    // ---------- OTP cooldown ----------
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
        if (!CanResendOtp || IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var ok = await _auth.SendOtpAsync(Email.Trim());
            if (ok)
                StartOtpCooldown();
            else
                ErrorMessage = "Failed to resend OTP. Please try again.";
        }
        catch (Exception ex)
        {
            HandleCommandException(ex, "Resend OTP");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ---------- Back navigation ----------
    private void GoBack()
    {
        ErrorMessage = string.Empty;
        if (CurrentStep <= 1) return;
        CurrentStep--;
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
    }

    protected void OnPropertyChanged(string name)
    {
        RaisePropertyChanged(name);
    }
}

