using System.Collections.ObjectModel;
using System.Windows.Input;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

/// <summary>Row model that adds a mutable IsPresent toggle for the attendance UI.</summary>
public class AttendanceRowVm : BaseViewModel
{
    public Guid StudentId   { get; init; }
    public string StudentName { get; init; } = string.Empty;

    bool isPresent;
    public bool IsPresent
    {
        get => isPresent;
        set => SetProperty(ref isPresent, value);
    }
}

[QueryProperty(nameof(SessionId),    "SessionId")]
[QueryProperty(nameof(ClassId),      "ClassId")]
[QueryProperty(nameof(SessionLabel), "SessionLabel")]
public class LecturerAttendanceViewModel : BaseViewModel
{
    private readonly ILecturerApiService _lecturer;

    public ObservableCollection<AttendanceRowVm> Students { get; } = [];

    public ICommand LoadCommand   { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SaveCommand   { get; }

    Guid _sessionId;
    public Guid SessionId
    {
        get => _sessionId;
        set { _sessionId = value; _ = LoadAsync(); }
    }

    Guid _classId;
    public Guid ClassId { get => _classId; set => _classId = value; }

    string sessionLabel = string.Empty;
    public string SessionLabel { get => sessionLabel; set => SetProperty(ref sessionLabel, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    bool isSaving;
    public bool IsSaving { get => isSaving; set => SetProperty(ref isSaving, value); }

    bool hasError;
    public bool HasError { get => hasError; set => SetProperty(ref hasError, value); }

    public LecturerAttendanceViewModel(ILecturerApiService lecturer)
    {
        _lecturer = lecturer;

        LoadCommand    = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
        SaveCommand    = new Command(async () => await SaveAsync(), () => !IsSaving);
    }

    public async Task LoadAsync()
    {
        if (SessionId == Guid.Empty || IsBusy) return;
        IsBusy   = true;
        HasError = false;
        try
        {
            var ct          = CreateLinkedToken();
            // Load existing attendance records
            var existing    = await _lecturer.GetSessionAttendanceAsync(SessionId, ct);
            var existingMap = existing.ToDictionary(a => a.StudentId, a => a.IsPresent);

            // Load all enrolled students so lecturer can mark even those not yet recorded
            var students    = await _lecturer.GetClassStudentsAsync(ClassId, ct);

            Students.Clear();
            foreach (var s in students)
            {
                Students.Add(new AttendanceRowVm
                {
                    StudentId   = s.Id,
                    StudentName = s.FullName,
                    IsPresent   = existingMap.TryGetValue(s.Id, out var p) && p
                });
            }
            IsEmpty = Students.Count == 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception) { HasError = true; }
        finally { IsBusy = false; }
    }

    private async Task SaveAsync()
    {
        if (IsSaving || SessionId == Guid.Empty) return;
        IsSaving = true;
        int saved = 0, failed = 0;
        try
        {
            var ct = CreateLinkedToken();
            foreach (var row in Students)
            {
                var (success, _) = await _lecturer.MarkAttendanceAsync(
                    SessionId, row.StudentId, row.IsPresent, null, ct);
                if (success) saved++;
                else         failed++;
            }

            if (failed == 0)
                await ShowSuccessToastAsync($"Attendance saved for {saved} student(s).");
            else
                await ShowErrorToastAsync($"Saved {saved}, failed {failed}.");
        }
        catch (OperationCanceledException) { }
        catch (Exception) { await ShowErrorToastAsync("Failed to save attendance."); }
        finally { IsSaving = false; }
    }
}
