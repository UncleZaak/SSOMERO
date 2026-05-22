using System.Collections.ObjectModel;
using System.Windows.Input;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

[QueryProperty(nameof(ClassId), "ClassId")]
[QueryProperty(nameof(ClassName), "ClassName")]
public class LecturerClassDetailsViewModel : BaseViewModel
{
    private readonly ILecturerApiService _lecturer;

    public ObservableCollection<SessionSummaryDto> Sessions { get; } = [];

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand GoToStudentsCommand { get; }
    public ICommand GoToAttendanceCommand { get; }
    public ICommand GoToMaterialsCommand { get; }

    Guid _classId;
    public Guid ClassId
    {
        get => _classId;
        set { _classId = value; _ = LoadAsync(); }
    }

    string className = string.Empty;
    public string ClassName
    {
        get => className;
        set => SetProperty(ref className, value);
    }

    int enrolledStudents;
    public int EnrolledStudents { get => enrolledStudents; set => SetProperty(ref enrolledStudents, value); }

    string? courseCode;
    public string? CourseCode { get => courseCode; set => SetProperty(ref courseCode, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    bool hasError;
    public bool HasError { get => hasError; set => SetProperty(ref hasError, value); }

    public LecturerClassDetailsViewModel(ILecturerApiService lecturer)
    {
        _lecturer = lecturer;

        LoadCommand    = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());

        GoToStudentsCommand = new Command(async () =>
            await Shell.Current.GoToAsync("lecturer-class-students",
                new Dictionary<string, object> { ["ClassId"] = ClassId, ["ClassName"] = ClassName }));

        GoToAttendanceCommand = new Command<SessionSummaryDto>(async session =>
        {
            if (session is null) return;
            await Shell.Current.GoToAsync("lecturer-attendance",
                new Dictionary<string, object>
                {
                    ["SessionId"]  = session.Id,
                    ["ClassId"]    = ClassId,
                    ["SessionLabel"] = session.DisplayTime
                });
        });

        GoToMaterialsCommand = new Command(async () =>
            await Shell.Current.GoToAsync("lecturer-materials",
                new Dictionary<string, object> { ["ClassId"] = ClassId, ["ClassName"] = ClassName }));
    }

    public async Task LoadAsync()
    {
        if (ClassId == Guid.Empty || IsBusy) return;
        IsBusy   = true;
        HasError = false;
        try
        {
            var ct     = CreateLinkedToken();
            var detail = await _lecturer.GetClassDetailAsync(ClassId, ct);
            if (detail is null) { HasError = true; return; }

            ClassName        = detail.Name;
            CourseCode       = detail.CourseCode;
            EnrolledStudents = detail.EnrolledStudents;

            Sessions.Clear();
            foreach (var s in detail.Sessions)
                Sessions.Add(s);

            IsEmpty = Sessions.Count == 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception) { HasError = true; }
        finally { IsBusy = false; }
    }
}
