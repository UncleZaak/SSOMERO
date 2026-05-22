using System.Windows.Input;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

[QueryProperty(nameof(CourseId), "courseId")]
public class AttendanceMarkViewModel : BaseViewModel
{
    private readonly IAttendanceService _attendance;
    private readonly IStudentScheduleService _schedule;
    private readonly IRefreshCoordinator? _coordinator;

    private double? _latitude;
    private double? _longitude;

    // ── Query property ────────────────────────────────────────────────────────
    string courseId = string.Empty;
    public string CourseId
    {
        get => courseId;
        set
        {
            SetProperty(ref courseId, value);
            _ = LoadAsync();
        }
    }

    // ── Observable state ──────────────────────────────────────────────────────
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

    bool isSubmitting;
    public bool IsSubmitting
    {
        get => isSubmitting;
        set { SetProperty(ref isSubmitting, value); RaisePropertyChanged(nameof(CanMark)); }
    }

    string gpsStatus = "GPS: not acquired";
    public string GpsStatus { get => gpsStatus; set => SetProperty(ref gpsStatus, value); }

    // ── Computed ──────────────────────────────────────────────────────────────
    public bool CanMark => CurrentSession is not null && !IsSubmitting;
    public bool HasGps  => _latitude.HasValue && _longitude.HasValue;

    public string MarkButtonLabel => CurrentSession is null
        ? "No Active Session"
        : IsSubmitting ? "Submitting…"
        : $"Mark Attendance — {CurrentSession.CourseName}";

    public string SessionInfoLabel => CurrentSession is null
        ? "No class in session right now."
        : $"{CurrentSession.CourseName} · {CurrentSession.TimeRange} · {CurrentSession.Location}";

    // ── Commands ──────────────────────────────────────────────────────────────
    public ICommand MarkAttendanceCommand { get; }
    public ICommand RefreshGpsCommand     { get; }

    public AttendanceMarkViewModel(
        IAttendanceService attendance,
        IStudentScheduleService schedule,
        IRefreshCoordinator? coordinator = null)
    {
        _attendance  = attendance;
        _schedule    = schedule;
        _coordinator = coordinator;

        MarkAttendanceCommand = new Command(async () => await MarkAsync(), () => CanMark);
        RefreshGpsCommand     = new Command(async () => await AcquireGpsAsync());
    }

    // ── Load ──────────────────────────────────────────────────────────────────
    public async Task LoadAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(CourseId)) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var sessions = await _schedule.GetWeekScheduleAsync();

            ClassSessionDto? session = null;
            if (Guid.TryParse(CourseId, out var courseGuid))
            {
                // Prefer an in-progress session, then the next upcoming one,
                // then any session for this course this week.
                session = sessions.FirstOrDefault(s => s.ClassId == courseGuid && s.IsNow)
                       ?? sessions.Where(s => s.ClassId == courseGuid && s.StartTime >= DateTime.Now)
                                  .MinBy(s => s.StartTime)
                       ?? sessions.FirstOrDefault(s => s.ClassId == courseGuid);
            }

            // Fall back to the globally active / next session
            CurrentSession = session
                          ?? await _schedule.GetCurrentSessionAsync()
                          ?? await _schedule.GetNextSessionAsync();

            _ = AcquireGpsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load session. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Mark ──────────────────────────────────────────────────────────────────
    private async Task MarkAsync()
    {
        if (CurrentSession is null || IsSubmitting) return;

        if (!HasGps)
            await AcquireGpsAsync();

        if (!HasGps)
        {
            ErrorMessage = "Location is required to mark attendance. Enable GPS and tap 'Refresh GPS'.";
            return;
        }

        IsSubmitting   = true;
        ErrorMessage   = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            Stream? selfieStream   = null;
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
            catch { /* camera is optional */ }

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

    // ── GPS ───────────────────────────────────────────────────────────────────
    private async Task AcquireGpsAsync()
    {
        try
        {
            GpsStatus = "GPS: acquiring…";
            var location = await Geolocation.Default.GetLastKnownLocationAsync()
                        ?? await Geolocation.Default.GetLocationAsync(
                               new GeolocationRequest(GeolocationAccuracy.Medium,
                                                      TimeSpan.FromSeconds(5)));
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
        catch (FeatureNotSupportedException) { _latitude = null; _longitude = null; GpsStatus = "GPS: not supported";      }
        catch (PermissionException)           { _latitude = null; _longitude = null; GpsStatus = "GPS: permission denied"; }
        catch                                 { _latitude = null; _longitude = null; GpsStatus = "GPS: error";             }
        finally { RaisePropertyChanged(nameof(HasGps)); }
    }
}
