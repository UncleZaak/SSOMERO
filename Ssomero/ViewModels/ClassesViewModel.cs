using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class ClassesViewModel : BaseViewModel
{
    private readonly ICoursesService _courses;
    private readonly IRollcallService _rollcall;
    private DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

    public ObservableCollection<ClassItemVm> Items { get; } = [];

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    public ICommand LoadCommand { get; }
    public ICommand OpenClassCommand { get; }
    public ICommand MarkAttendanceCommand { get; }

    public ClassesViewModel(ICoursesService courses, IRollcallService rollcall)
    {
        _courses = courses;
        _rollcall = rollcall;
        LoadCommand = new Command(async () => await LoadAsync(forceRefresh: true));
        OpenClassCommand = new Command<string>(async id =>
            await Shell.Current.GoToAsync($"course-detail?courseId={id}"));
        MarkAttendanceCommand = new Command<string>(async id =>
            await Shell.Current.GoToAsync($"attendance-mark?courseId={id}"));
    }

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (IsBusy) return;
        if (!forceRefresh && DateTime.UtcNow - _lastLoaded < RefreshInterval) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var courses = (await _courses.GetCoursesAsync()).ToList();
            var rollcalls = (await _rollcall.GetMyRollcallsAsync()).ToList();

            Items.Clear();
            foreach (var c in courses)
            {
                var attended = rollcalls.Count(r => r.CourseName == c.Name && r.Status == RollcallStatus.ApprovedByLecturer);
                var pct = c.TotalSessions > 0 ? (int)Math.Round(attended * 100.0 / c.TotalSessions) : 0;
                Items.Add(new ClassItemVm(c, pct));
            }

            IsEmpty = Items.Count == 0;
            _lastLoaded = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load classes. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class ClassItemVm
{
    public string Id { get; }
    public string Name { get; }
    public string? CourseCode { get; }
    public string? LecturerName { get; }
    public int EnrolledStudents { get; }
    public int AttendancePct { get; }
    public string AttendancePctText => $"{AttendancePct}%";

    public ClassItemVm(CourseDto c, int attendancePct)
    {
        Id = c.Id;
        Name = c.Name;
        CourseCode = null; // CourseDto doesn't carry code
        LecturerName = c.Lecturer;
        EnrolledStudents = c.EnrolledStudents;
        AttendancePct = attendancePct;
    }
}
