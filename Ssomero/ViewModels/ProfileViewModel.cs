using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IProfileService _profileService;
    private readonly IProfilePhotoService _photoService;
    private readonly SessionService _session;
    private readonly DashboardViewModel? _dashboardVm;
    private readonly ILogger<ProfileViewModel> _logger;
    private readonly IRefreshCoordinator? _refresh;

    public ProfileViewModel(
        IAuthService authService,
        IProfileService profileService,
        SessionService session,
        ILogger<ProfileViewModel> logger,
        IProfilePhotoService? photoService = null,
        DashboardViewModel? dashboardVm = null,
        IRefreshCoordinator? refresh = null)
    {
        _authService     = authService;
        _profileService  = profileService;
        _photoService    = photoService;
        _session         = session;
        _logger          = logger;
        _dashboardVm     = dashboardVm;
        _refresh         = refresh;

        LoadCommand            = new AsyncRelayCommand(LoadProfileAsync);
        SaveCommand            = new AsyncRelayCommand(SaveProfileAsync);
        CancelEditCommand      = new Command(CancelEdit);
        ToggleEditCommand      = new Command(ToggleEdit);
        ChangePhotoCommand      = new AsyncRelayCommand(ChangePhotoAsync);
        LogoutCommand          = new Command(async () => await LogoutAsync());
        NavigateToChangePasswordCommand = new Command(async () =>
            await Shell.Current.GoToAsync("change-password"));
        NavigateToPaymentsCommand = new Command(async () =>
            await Shell.Current.GoToAsync("payments"));
        NavigateToAnalyticsCommand = new Command(async () =>
            await Shell.Current.GoToAsync("analytics"));
        NavigateToClassesCommand = new Command(async () =>
            await Shell.Current.GoToAsync("//LecturerDashboard"));
        NavigateToUsersCommand = new Command(async () =>
            await Shell.Current.GoToAsync("//AdminDashboard"));
        NavigateToAuditLogsCommand = new Command(async () =>
            await Shell.Current.GoToAsync("audit-logs"));
        NavigateToAdminAnalyticsCommand = new Command(async () =>
            await Shell.Current.GoToAsync("admin-analytics"));

        _refresh?.Subscribe(RefreshKeys.Subscription, OnSubscriptionUpdatedAsync);
    }

    // ── Commands ──────────────────────────────────────────────────────────────
    public AsyncRelayCommand LoadCommand  { get; }
    public AsyncRelayCommand SaveCommand  { get; }
    public AsyncRelayCommand ChangePhotoCommand { get; }
    public ICommand CancelEditCommand     { get; }
    public ICommand ToggleEditCommand     { get; }
    public ICommand LogoutCommand         { get; }
    public ICommand NavigateToChangePasswordCommand { get; }
    public ICommand NavigateToPaymentsCommand       { get; }
    public ICommand NavigateToAnalyticsCommand      { get; }
    public ICommand NavigateToClassesCommand        { get; }
    public ICommand NavigateToUsersCommand          { get; }
    public ICommand NavigateToAuditLogsCommand      { get; }
    public ICommand NavigateToAdminAnalyticsCommand { get; }

    // ── Profile data ──────────────────────────────────────────────────────────
    ProfileDto? _profile;

    string fullName = string.Empty;
    public string FullName { get => fullName; set => SetProperty(ref fullName, value); }

    string email = string.Empty;
    public string Email { get => email; set => SetProperty(ref email, value); }

    string role = string.Empty;
    public string Role { get => role; set => SetProperty(ref role, value); }

    string initials = string.Empty;
    public string Initials { get => initials; set => SetProperty(ref initials, value); }

    string? universityName;
    public string? UniversityName { get => universityName; private set => SetProperty(ref universityName, value); }

    string? photoUrl;
    public string? PhotoUrl { get => photoUrl; private set => SetProperty(ref photoUrl, value); }

    bool hasPhoto;
    public bool HasPhoto { get => hasPhoto; private set => SetProperty(ref hasPhoto, value); }

    bool isUploading;
    public bool IsUploading { get => isUploading; private set => SetProperty(ref isUploading, value); }

    // ── Edit fields
    string editFirstName = string.Empty;
    public string EditFirstName { get => editFirstName; set => SetProperty(ref editFirstName, value); }

    string editLastName = string.Empty;
    public string EditLastName { get => editLastName; set => SetProperty(ref editLastName, value); }

    string editPhone = string.Empty;
    public string EditPhone { get => editPhone; set => SetProperty(ref editPhone, value); }

    string editPhotoUrl = string.Empty;
    public string EditPhotoUrl { get => editPhotoUrl; set => SetProperty(ref editPhotoUrl, value); }

    bool isEditMode;
    public bool IsEditMode
    {
        get => isEditMode;
        private set
        {
            if (SetProperty(ref isEditMode, value))
                RaisePropertyChanged(nameof(IsViewMode));
        }
    }
    public bool IsViewMode => !IsEditMode;

    // ── Role visibility flags ─────────────────────────────────────────────────
    public bool IsStudent  => Role == "Student";
    public bool IsLecturer => Role == "Lecturer";
    public bool IsAdmin    => Role == "Admin";

    // ── Student extras ────────────────────────────────────────────────────────
    string? studentId;
    public string? StudentId { get => studentId; private set => SetProperty(ref studentId, value); }

    string? program;
    public string? Program { get => program; private set => SetProperty(ref program, value); }

    string? department;
    public string? Department { get => department; private set => SetProperty(ref department, value); }

    string? faculty;
    public string? Faculty { get => faculty; private set => SetProperty(ref faculty, value); }

    double attendancePct;
    public double AttendancePct { get => attendancePct; private set => SetProperty(ref attendancePct, value); }

    string subscriptionStatus = "None";
    public string SubscriptionStatus { get => subscriptionStatus; private set => SetProperty(ref subscriptionStatus, value); }

    // ── Lecturer extras ───────────────────────────────────────────────────────
    string? staffId;
    public string? StaffId { get => staffId; private set => SetProperty(ref staffId, value); }

    int assignedClassesCount;
    public int AssignedClassesCount { get => assignedClassesCount; private set => SetProperty(ref assignedClassesCount, value); }

    int materialsCount;
    public int MaterialsCount { get => materialsCount; private set => SetProperty(ref materialsCount, value); }

    int sessionsManaged;
    public int SessionsManaged { get => sessionsManaged; private set => SetProperty(ref sessionsManaged, value); }

    // ── Admin extras ──────────────────────────────────────────────────────────
    string managedUniversities = string.Empty;
    public string ManagedUniversities { get => managedUniversities; private set => SetProperty(ref managedUniversities, value); }

    string systemRole = "Admin";
    public string SystemRole { get => systemRole; private set => SetProperty(ref systemRole, value); }

    // ── Error / retry ─────────────────────────────────────────────────────────
    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool hasError;
    public bool HasError { get => hasError; private set => SetProperty(ref hasError, value); }

    // ── Load ──────────────────────────────────────────────────────────────────
    public async Task LoadProfileAsync()
    {
        ErrorMessage = string.Empty;
        HasError     = false;
        IsBusy       = true;
        try
        {
            var ct = CreateLinkedToken();
            _profile = await _profileService.GetProfileAsync(ct);
            if (_profile is null)
            {
                HasError     = true;
                ErrorMessage = "Could not load profile. Please try again.";
                return;
            }
            ApplyProfile(_profile);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadProfileAsync failed");
            HasError     = true;
            ErrorMessage = "An error occurred loading your profile.";
        }
        finally { IsBusy = false; }
    }

    /// <summary>Called by RefreshCoordinator when a payment completes — reloads profile to update SubscriptionStatus.</summary>
    private async Task OnSubscriptionUpdatedAsync()
    {
        try { await LoadProfileAsync(); }
        catch (Exception ex) { _logger.LogError(ex, "OnSubscriptionUpdatedAsync failed"); }
    }

    /// <summary>Lightweight refresh using session data — used when navigating back to profile tab.</summary>
    public void RefreshProfile()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        FullName = string.IsNullOrWhiteSpace(user.FullName) ? "User" : user.FullName;
        Email    = user.Email;
        Role     = user.Role ?? "Student";
        Initials = GetInitials(FullName);
        RaisePropertyChanged(nameof(IsStudent));
        RaisePropertyChanged(nameof(IsLecturer));
        RaisePropertyChanged(nameof(IsAdmin));
    }

    private void ApplyProfile(ProfileDto p)
    {
        FullName       = $"{p.FirstName} {p.LastName}".Trim();
        Email          = p.Email;
        Role           = p.Role;
        Initials       = GetInitials(FullName);
        UniversityName = p.UniversityName;
        PhotoUrl       = p.PhotoUrl;
        HasPhoto       = !string.IsNullOrWhiteSpace(p.PhotoUrl);

        RaisePropertyChanged(nameof(IsStudent));
        RaisePropertyChanged(nameof(IsLecturer));
        RaisePropertyChanged(nameof(IsAdmin));

        if (p is StudentProfileDto sp)
        {
            StudentId          = sp.StudentId;
            Program            = sp.Program;
            Department         = sp.Department;
            Faculty            = sp.Faculty;
            AttendancePct      = sp.AttendancePercentage;
            SubscriptionStatus = sp.SubscriptionStatus;
        }
        else if (p is LecturerProfileDto lp)
        {
            StaffId             = lp.StaffId;
            AssignedClassesCount = lp.AssignedClassesCount;
            MaterialsCount      = lp.MaterialsUploadedCount;
            SessionsManaged     = lp.AttendanceSessionsManaged;
        }
        else if (p is AdminProfileDto ap)
        {
            ManagedUniversities = ap.ManagedUniversities.Count == 0
                ? "None"
                : string.Join(", ", ap.ManagedUniversities);
            SystemRole = ap.SystemRole;
        }

        // Seed edit fields
        EditFirstName = p.FirstName;
        EditLastName  = p.LastName;
        EditPhone     = p.PhoneNumber ?? string.Empty;
        EditPhotoUrl  = p.PhotoUrl   ?? string.Empty;
    }

    // ── Edit ──────────────────────────────────────────────────────────────────
    private void ToggleEdit() => IsEditMode = !IsEditMode;

    private void CancelEdit()
    {
        if (_profile is not null)
        {
            EditFirstName = _profile.FirstName;
            EditLastName  = _profile.LastName;
            EditPhone     = _profile.PhoneNumber ?? string.Empty;
            EditPhotoUrl  = _profile.PhotoUrl   ?? string.Empty;
        }
        IsEditMode = false;
    }

    private async Task SaveProfileAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(EditFirstName) && string.IsNullOrWhiteSpace(EditLastName))
        {
            ErrorMessage = "First or last name cannot both be empty.";
            return;
        }

        IsBusy = true;
        try
        {
            var ct  = CreateLinkedToken();
            var dto = new UpdateProfileRequest
            {
                FirstName   = EditFirstName.Trim(),
                LastName    = EditLastName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(EditPhone) ? null : EditPhone.Trim(),
                PhotoUrl    = string.IsNullOrWhiteSpace(EditPhotoUrl) ? null : EditPhotoUrl.Trim()
            };

            var ok = await _profileService.UpdateProfileAsync(dto, ct);
            if (!ok)
            {
                ErrorMessage = "Failed to save profile. Please try again.";
                return;
            }

            // Optimistically refresh local state
            if (_profile is not null)
            {
                _profile.FirstName   = dto.FirstName ?? _profile.FirstName;
                _profile.LastName    = dto.LastName  ?? _profile.LastName;
                _profile.PhoneNumber = dto.PhoneNumber;
                _profile.PhotoUrl    = dto.PhotoUrl;
            }
            ApplyProfile(_profile ?? new ProfileDto
            {
                FirstName = dto.FirstName ?? string.Empty,
                LastName  = dto.LastName  ?? string.Empty,
                Role      = Role,
                Email     = Email
            });

            IsEditMode = false;
            await ShowSuccessToastAsync("Profile updated successfully.");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveProfileAsync failed");
            ErrorMessage = "An error occurred saving your profile.";
        }
        finally { IsBusy = false; }
    }

    // ── Photo upload ──────────────────────────────────────────────────────────
    private async Task ChangePhotoAsync()
    {
        if (IsUploading) return;

        var action = await Shell.Current.DisplayActionSheet(
            "Profile Photo", "Cancel", null,
            "📷 Take Photo", "🖼️ Choose from Gallery", "🗑️ Remove Photo");

        if (string.IsNullOrEmpty(action) || action == "Cancel")
            return;

        string? localPath = null;

        if (action == "📷 Take Photo")
            localPath = await _photoService.CapturePhotoAsync();
        else if (action == "🖼️ Choose from Gallery")
            localPath = await _photoService.PickFromGalleryAsync();
        else if (action == "🗑️ Remove Photo")
        {
            IsUploading = true;
            try
            {
                await _photoService.RemoveAsync();
                PhotoUrl = null;
                HasPhoto = false;
                await ShowSuccessToastAsync("Profile photo removed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemovePhoto failed");
                await ShowErrorToastAsync("Unable to remove photo.");
            }
            finally { IsUploading = false; }
            return;
        }

        if (localPath is null) return; // user cancelled picker or permission denied

        IsUploading = true;
        try
        {
            var newUrl = await _photoService.UploadAsync(localPath);
            if (newUrl is not null)
            {
                // Optimistic local update — no full API reload needed
                PhotoUrl = newUrl;
                HasPhoto = true;
                if (_profile is not null) _profile.PhotoUrl = newUrl;
                await ShowSuccessToastAsync("Profile photo updated.");
            }
            else
            {
                await ShowErrorToastAsync("Unable to upload photo. Please try again.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangePhotoAsync upload failed");
            await ShowErrorToastAsync("Unable to upload photo.");
        }
        finally { IsUploading = false; }
    }

    // ── Logout ────────────────────────────────────────────────────────────────
    private async Task LogoutAsync()
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Log Out", "Are you sure you want to log out?", "Log Out", "Cancel");
        if (!confirmed) return;

        _dashboardVm?.Reset();
        await _authService.LogoutAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "U";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
            : parts[0][..1].ToUpperInvariant();
    }
}

