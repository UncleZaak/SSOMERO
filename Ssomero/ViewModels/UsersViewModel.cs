using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class UsersViewModel : BaseViewModel
{
    private readonly IAdminService _admin;
    private readonly IRefreshCoordinator _refresh;
    private readonly ILogger<UsersViewModel> _logger;

    private List<UserItem> _allUsers = [];

    public ObservableCollection<UserItem> FilteredUsers { get; } = [];

    string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetProperty(ref searchText, value))
                ApplyFilter();
        }
    }

    string selectedTab = "Students";
    public string SelectedTab
    {
        get => selectedTab;
        set
        {
            if (SetProperty(ref selectedTab, value))
            {
                RaisePropertyChanged(nameof(IsStudentsTab));
                RaisePropertyChanged(nameof(IsLecturersTab));
                ApplyFilter();
            }
        }
    }

    public bool IsStudentsTab => SelectedTab == "Students";
    public bool IsLecturersTab => SelectedTab == "Lecturers";

    int totalStudents;
    public int TotalStudents
    {
        get => totalStudents;
        set => SetProperty(ref totalStudents, value);
    }

    int totalLecturers;
    public int TotalLecturers
    {
        get => totalLecturers;
        set => SetProperty(ref totalLecturers, value);
    }

    int totalSuspended;
    public int TotalSuspended
    {
        get => totalSuspended;
        set => SetProperty(ref totalSuspended, value);
    }

    int pendingLecturers;
    public int PendingLecturers
    {
        get => pendingLecturers;
        set => SetProperty(ref pendingLecturers, value);
    }

    string errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    bool hasError;
    public bool HasError
    {
        get => hasError;
        set => SetProperty(ref hasError, value);
    }

    bool isEmpty;
    public bool IsEmpty
    {
        get => isEmpty;
        set => SetProperty(ref isEmpty, value);
    }

    // ── Bulk selection ────────────────────────────────────────────────────────
    bool isBulkMode;
    public bool IsBulkMode
    {
        get => isBulkMode;
        set
        {
            if (SetProperty(ref isBulkMode, value) && !value)
                ClearSelection();
        }
    }

    int selectedCount;
    public int SelectedCount
    {
        get => selectedCount;
        set
        {
            if (SetProperty(ref selectedCount, value))
                RaisePropertyChanged(nameof(HasSelection));
        }
    }

    public bool HasSelection => SelectedCount > 0;

    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand SwitchTabCommand { get; }
    public ICommand SuspendCommand { get; }
    public ICommand ActivateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ApproveCommand { get; }
    public ICommand ToggleBulkModeCommand { get; }
    public ICommand ToggleSelectAllCommand { get; }
    public ICommand BulkSuspendCommand { get; }
    public ICommand BulkActivateCommand { get; }
    public ICommand BulkDeleteCommand { get; }

    public UsersViewModel(IAdminService admin, IRefreshCoordinator refresh, ILogger<UsersViewModel> logger)
    {
        _admin = admin;
        _refresh = refresh;
        _logger = logger;
        Title = "User Management";

        LoadCommand    = new Command(async () => await LoadUsersAsync());
        SearchCommand  = new Command(ApplyFilter);
        SwitchTabCommand = new Command<string>(tab => SelectedTab = tab);
        SuspendCommand = new Command<UserItem>(async u => await SuspendAsync(u));
        ActivateCommand = new Command<UserItem>(async u => await ActivateAsync(u));
        DeleteCommand  = new Command<UserItem>(async u => await DeleteAsync(u));
        ApproveCommand = new Command<UserItem>(async u => await ApproveAsync(u));
        ToggleBulkModeCommand = new Command(() => IsBulkMode = !IsBulkMode);
        ToggleSelectAllCommand = new Command(ToggleSelectAll);
        BulkSuspendCommand  = new Command(async () => await BulkSuspendAsync());
        BulkActivateCommand = new Command(async () => await BulkActivateAsync());
        BulkDeleteCommand   = new Command(async () => await BulkDeleteAsync());
    }

    public async Task LoadUsersAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var ct = CreateLinkedToken();
            var studentsTask = _admin.GetStudentsAsync(ct);
            var lecturersTask = _admin.GetLecturersAsync(ct);
            await Task.WhenAll(studentsTask, lecturersTask);

            var students = await studentsTask;
            var lecturers = await lecturersTask;

            _allUsers = [.. students, .. lecturers];
            TotalStudents = students.Count;
            TotalLecturers = lecturers.Count;
            TotalSuspended = _allUsers.Count(u => u.Status == "Suspended");
            PendingLecturers = lecturers.Count(u => !u.IsApproved);

            ApplyFilter();
            await _refresh.NotifyAsync(RefreshKeys.Users);
        }
        catch (OperationCanceledException)
        {
            // Navigation away — ignore
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load users");
            HasError = true;
            ErrorMessage = "Failed to load users. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var role = SelectedTab == "Students" ? "Student" : "Lecturer";
        var query = _allUsers.Where(u => u.Role == role);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim();
            query = query.Where(u =>
                u.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        FilteredUsers.Clear();
        foreach (var user in query)
            FilteredUsers.Add(user);

        IsEmpty = FilteredUsers.Count == 0;
        UpdateSelectedCount();
    }

    private async Task SuspendAsync(UserItem user)
    {
        if (user is null || IsBusy) return;
        IsBusy = true;
        try
        {
            var success = user.Role == "Student"
                ? await _admin.SuspendStudentAsync(user.Id)
                : await _admin.SuspendLecturerAsync(user.Id);

            if (success)
            {
                await ShowSuccessToastAsync($"{user.Name} has been suspended.");
                await LoadUsersAsync();
            }
            else
                await ShowErrorToastAsync("Failed to suspend user.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Suspend failed for {UserId}", user.Id);
            await ShowErrorToastAsync("An error occurred. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ActivateAsync(UserItem user)
    {
        if (user is null || IsBusy) return;
        IsBusy = true;
        try
        {
            var success = user.Role == "Student"
                ? await _admin.ActivateStudentAsync(user.Id)
                : await _admin.ActivateLecturerAsync(user.Id);

            if (success)
            {
                await ShowSuccessToastAsync($"{user.Name} has been activated.");
                await LoadUsersAsync();
            }
            else
                await ShowErrorToastAsync("Failed to activate user.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Activate failed for {UserId}", user.Id);
            await ShowErrorToastAsync("An error occurred. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync(UserItem user)
    {
        if (user is null || IsBusy) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {user.Name}? This action cannot be easily undone.",
            "Delete", "Cancel");

        if (!confirmed) return;

        IsBusy = true;
        try
        {
            var success = user.Role == "Student"
                ? await _admin.DeleteStudentAsync(user.Id)
                : await _admin.DeleteLecturerAsync(user.Id);

            if (success)
            {
                await ShowSuccessToastAsync($"{user.Name} has been deleted.");
                await LoadUsersAsync();
            }
            else
                await ShowErrorToastAsync("Failed to delete user.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed for {UserId}", user.Id);
            await ShowErrorToastAsync("An error occurred. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApproveAsync(UserItem user)
    {
        if (user is null || IsBusy) return;
        IsBusy = true;
        try
        {
            var success = await _admin.ApproveLecturerAsync(user.Id);
            if (success)
            {
                await ShowSuccessToastAsync($"{user.Name} has been approved and can now log in.");
                await LoadUsersAsync();
            }
            else
                await ShowErrorToastAsync("Failed to approve lecturer. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Approve failed for {UserId}", user.Id);
            await ShowErrorToastAsync("An error occurred. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Bulk helpers ──────────────────────────────────────────────────────────
    private void ToggleSelectAll()
    {
        var allSelected = FilteredUsers.All(u => u.IsSelected);
        foreach (var u in FilteredUsers)
            u.IsSelected = !allSelected;
        UpdateSelectedCount();
    }

    public void OnItemSelectionChanged()
    {
        UpdateSelectedCount();
    }

    private void UpdateSelectedCount()
        => SelectedCount = FilteredUsers.Count(u => u.IsSelected);

    private void ClearSelection()
    {
        foreach (var u in FilteredUsers)
            u.IsSelected = false;
        SelectedCount = 0;
    }

    private async Task BulkSuspendAsync()
    {
        var selected = FilteredUsers.Where(u => u.IsSelected).ToList();
        if (selected.Count == 0) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Bulk Suspend",
            $"Suspend {selected.Count} selected user(s)?",
            "Suspend", "Cancel");
        if (!confirmed) return;

        IsBusy = true;
        int ok = 0;
        foreach (var u in selected)
        {
            var success = u.Role == "Student"
                ? await _admin.SuspendStudentAsync(u.Id)
                : await _admin.SuspendLecturerAsync(u.Id);
            if (success) ok++;
        }
        IsBusy = false;
        IsBulkMode = false;
        await ShowSuccessToastAsync($"{ok}/{selected.Count} users suspended.");
        await LoadUsersAsync();
    }

    private async Task BulkActivateAsync()
    {
        var selected = FilteredUsers.Where(u => u.IsSelected).ToList();
        if (selected.Count == 0) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Bulk Activate",
            $"Activate {selected.Count} selected user(s)?",
            "Activate", "Cancel");
        if (!confirmed) return;

        IsBusy = true;
        int ok = 0;
        foreach (var u in selected)
        {
            var success = u.Role == "Student"
                ? await _admin.ActivateStudentAsync(u.Id)
                : await _admin.ActivateLecturerAsync(u.Id);
            if (success) ok++;
        }
        IsBusy = false;
        IsBulkMode = false;
        await ShowSuccessToastAsync($"{ok}/{selected.Count} users activated.");
        await LoadUsersAsync();
    }

    private async Task BulkDeleteAsync()
    {
        var selected = FilteredUsers.Where(u => u.IsSelected).ToList();
        if (selected.Count == 0) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Bulk Delete",
            $"Permanently delete {selected.Count} selected user(s)? This cannot be undone.",
            "Delete", "Cancel");
        if (!confirmed) return;

        IsBusy = true;
        int ok = 0;
        foreach (var u in selected)
        {
            var success = u.Role == "Student"
                ? await _admin.DeleteStudentAsync(u.Id)
                : await _admin.DeleteLecturerAsync(u.Id);
            if (success) ok++;
        }
        IsBusy = false;
        IsBulkMode = false;
        await ShowSuccessToastAsync($"{ok}/{selected.Count} users deleted.");
        await LoadUsersAsync();
    }
}
