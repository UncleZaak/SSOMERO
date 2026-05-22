using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class AttendanceViewModel : BaseViewModel
{
    private readonly IAttendanceService _attendance;
    private readonly IStudentScheduleService _schedule;
    private readonly INotificationService _notifications;
    private readonly IRefreshCoordinator? _coordinator;
    private DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(2);

    public ObservableCollection<AttendanceRecordDto> History { get; } = [];

    ClassSessionDto? currentSession;
    public ClassSessionDto? CurrentSession
    {
        get => currentSession;
        set
        {
            SetProperty(ref currentSession, value);
            RaisePropertyChanged(nameof(CanMark));
            RaisePropertyChanged(nameof(MarkButtonLabel));
            RaisePropertyChanged(nameof(SessionInfoLabel));
        }
    }

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    string successMessage = string.Empty;
    public string SuccessMessage { get => successMessage; set => SetProperty(ref successMessage, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    bool isSubmitting;
    public bool IsSubmitting
    {
        get => isSubmitting;
        set { SetProperty(ref isSubmitting, value); RaisePropertyChanged(nameof(CanMark)); }
    }

    double? _latitude;
    double? _longitude;

    string gpsStatus = "GPS: not acquired";
    public string GpsStatus { get => gpsStatus; set => SetProperty(ref gpsStatus, value); }

    public bool CanMark => CurrentSession is not null && !IsSubmitting;
    public bool HasGps  => _latitude.HasValue && _longitude.HasValue;
    public string MarkButtonLabel => CurrentSession is null
        ? "No Active Session"
        : IsSubmitting ? "Submitting…"
        : $"Mark Attendance — {CurrentSession.CourseName}";
    public string SessionInfoLabel => CurrentSession is null
        ? "No class in session right now."
        : $"{CurrentSession.CourseName} · {CurrentSession.TimeRange} · {CurrentSession.Location}";

    public ICommand LoadCommand { get; }
    public ICommand MarkAttendanceCommand { get; }
    public ICommand RefreshGpsCommand { get; }

    public AttendanceViewModel(
        IAttendanceService attendance,
        IStudentScheduleService schedule,
        INotificationService notifications,
        IRefreshCoordinator? coordinator = null)
    {
        _attendance    = attendance;
        _schedule      = schedule;
        _notifications = notifications;
        _coordinator   = coordinator;

        LoadCommand           = new Command(async () => await LoadAsync(forceRefresh: true));
        MarkAttendanceCommand = new Command(async () => await MarkAsync(), () => CanMark);
        RefreshGpsCommand     = new Command(async () => await AcquireGpsAsync());
    }

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (IsBusy) return;
        if (!forceRefresh && DateTime.UtcNow - _lastLoaded < RefreshInterval) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            CurrentSession = await _schedule.GetCurrentSessionAsync()
                          ?? await _schedule.GetNextSessionAsync();

            var history = await _attendance.GetHistoryAsync();
            History.Clear();
            foreach (var r in history.OrderByDescending(r => r.Date))
                History.Add(r);

            IsEmpty = History.Count == 0;

            var report = await _attendance.GetMyReportAsync();
            if (report is not null)
                foreach (var c in report.CourseStats.Where(c => c.AttendancePercent < 75))
                    await _notifications.SendAttendanceWarningAsync(c.CourseName, c.AttendancePercent);

            if (CurrentSession is not null && !CurrentSession.IsNow)
                await _notifications.ScheduleClassReminderAsync(CurrentSession);

            _ = AcquireGpsAsync();
            _lastLoaded = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkAsync()
    {
        if (CurrentSession is null || IsSubmitting) return;

        // Attempt GPS acquisition if not already acquired
        if (!HasGps)
            await AcquireGpsAsync();

        // Hard gate: GPS is required to mark attendance
        if (!HasGps)
        {
            ErrorMessage = "Location is required to mark attendance. Please enable GPS and tap 'Refresh GPS'.";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            Stream? selfieStream = null;
            string? selfieFileName = null;
            try
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo is not null)
                {
                    selfieStream   = await photo.OpenReadAsync();
                    selfieFileName = photo.FileName;
                }
            }
            catch { /* camera optional */ }

            await AcquireGpsAsync();

            var result = await _attendance.MarkAttendanceAsync(
                sessionId      : CurrentSession.SessionId,
                latitude       : _latitude,
                longitude      : _longitude,
                selfieStream   : selfieStream,
                selfieFileName : selfieFileName);

            selfieStream?.Dispose();

            if (result.Success)
            {
                SuccessMessage = $"✅ Attendance marked for {CurrentSession.CourseName}!";
                await LoadAsync(forceRefresh: true);
                // Notify other pages (e.g. Dashboard) that attendance/schedule changed
                if (_coordinator is not null)
                {
                    _ = _coordinator.NotifyAsync(RefreshKeys.Attendance);
                    _ = _coordinator.NotifyAsync(RefreshKeys.Schedule);
                }
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Submission failed.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Unexpected error: " + ex.Message;
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private async Task AcquireGpsAsync()
    {
        try
        {
            GpsStatus = "GPS: acquiring…";
            var location = await Geolocation.Default.GetLastKnownLocationAsync()
                        ?? await Geolocation.Default.GetLocationAsync(
                               new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));

            if (location is not null)
            {
                _latitude  = location.Latitude;
                _longitude = location.Longitude;
                GpsStatus  = $"GPS: {_latitude:F4}°, {_longitude:F4}°";
            }
            else
            {
                _latitude  = null;
                _longitude = null;
                GpsStatus  = "GPS: unavailable";
            }
        }
        catch (FeatureNotSupportedException) { _latitude = null; _longitude = null; GpsStatus = "GPS: not supported"; }
        catch (PermissionException)           { _latitude = null; _longitude = null; GpsStatus = "GPS: permission denied"; }
        catch                                 { _latitude = null; _longitude = null; GpsStatus = "GPS: error"; }
        finally
        {
            RaisePropertyChanged(nameof(HasGps));
        }
    }
}
