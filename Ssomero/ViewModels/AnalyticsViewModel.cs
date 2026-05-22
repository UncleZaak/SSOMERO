using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class AnalyticsViewModel : BaseViewModel
{
    private readonly IAttendanceService _attendance;
    private readonly IInsightsService _insightsService;

    public ObservableCollection<AttendanceStatsDto> CourseStats { get; } = [];

    string errorMessage = string.Empty;
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

    double overallPercent;
    public double OverallPercent
    {
        get => overallPercent;
        set
        {
            if (SetProperty(ref overallPercent, value))
            {
                RaisePropertyChanged(nameof(OverallText));
                RaisePropertyChanged(nameof(OverallColor));
                RaisePropertyChanged(nameof(OverallProgressValue));
                RaisePropertyChanged(nameof(OverallStatus));
            }
        }
    }

    public string OverallText          => $"{OverallPercent:F0}%";
    public string OverallColor         => OverallPercent switch { >= 75 => "#10B981", >= 50 => "#F59E0B", _ => "#EF4444" };
    public double OverallProgressValue => OverallPercent / 100.0;
    public string OverallStatus        => OverallPercent switch
    {
        >= 75 => "On Track ✅",
        >= 50 => "Needs Improvement ⚠️",
        _     => "At Risk 🔴"
    };

    int presentCount;
    public int PresentCount { get => presentCount; set => SetProperty(ref presentCount, value); }

    int absentCount;
    public int AbsentCount { get => absentCount; set => SetProperty(ref absentCount, value); }

    bool isEmpty;
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }

    // For the bar-chart drawable
    public List<BarItem> BarItems { get; private set; } = [];

    public ObservableCollection<string> Insights { get; } = [];
    public bool HasInsights => Insights.Count > 0;

    public ICommand LoadCommand { get; }

    public AnalyticsViewModel(IAttendanceService attendance, IInsightsService insightsService)
    {
        _attendance      = attendance;
        _insightsService = insightsService;
        LoadCommand = new Command(async () => await LoadAsync(forceRefresh: true));
    }

    private DateTime _lastLoaded = DateTime.MinValue;

    public async Task LoadAsync(bool forceRefresh = false)
    {
        if (IsBusy) return;
        if (!forceRefresh && DateTime.UtcNow - _lastLoaded < TimeSpan.FromMinutes(5)) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var report = await _attendance.GetMyReportAsync();
            CourseStats.Clear();
            BarItems.Clear();

            if (report is not null)
            {
                OverallPercent = report.OverallPercent;
                var totalAttended = report.CourseStats.Sum(c => c.AttendedSessions);
                var totalSessions = report.CourseStats.Sum(c => c.TotalSessions);
                PresentCount = totalAttended;
                AbsentCount  = totalSessions - totalAttended;

                foreach (var s in report.CourseStats)
                {
                    CourseStats.Add(s);
                    BarItems.Add(new BarItem(
                        s.CourseName,
                        s.AttendancePercent,
                        s.AttendancePercent / 100.0,
                        $"{s.AttendedSessions}/{s.TotalSessions} sessions attended",
                        s.ClassAvgPercent,
                        s.ComparisonLabel));
                }

                BuildInsights(report);
            }

            IsEmpty = CourseStats.Count == 0;
            RaisePropertyChanged(nameof(HasInsights));
            _lastLoaded = DateTime.UtcNow;
            RaisePropertyChanged(nameof(BarItems));
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load analytics. " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildInsights(StudentAttendanceReportDto report)
    {
        Insights.Clear();
        foreach (var msg in _insightsService.GenerateReportInsights(report))
            Insights.Add(msg);
    }
}

public record BarItem(string Label, double Value, double Progress, string Detail, double ClassAvg = 0, string ComparisonLabel = "");
