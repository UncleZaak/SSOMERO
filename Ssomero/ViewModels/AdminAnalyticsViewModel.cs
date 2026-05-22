using System.Windows.Input;
using Microcharts;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class AdminAnalyticsViewModel : BaseViewModel
{
    private readonly IAdminService _admin;
    private readonly ILogger<AdminAnalyticsViewModel> _logger;

    // Cached raw data for chart rebuilding on filter change
    private List<AuditLogDto> _cachedAuditLogs = [];

    // ── Top Metrics ───────────────────────────────────────────────────────────
    int totalStudents;
    public int TotalStudents { get => totalStudents; set => SetProperty(ref totalStudents, value); }

    int totalLecturers;
    public int TotalLecturers { get => totalLecturers; set => SetProperty(ref totalLecturers, value); }

    int totalPrograms;
    public int TotalPrograms { get => totalPrograms; set => SetProperty(ref totalPrograms, value); }

    int totalClasses;
    public int TotalClasses { get => totalClasses; set => SetProperty(ref totalClasses, value); }

    int totalUniversities;
    public int TotalUniversities { get => totalUniversities; set => SetProperty(ref totalUniversities, value); }

    int totalFaculties;
    public int TotalFaculties { get => totalFaculties; set => SetProperty(ref totalFaculties, value); }

    int totalDepartments;
    public int TotalDepartments { get => totalDepartments; set => SetProperty(ref totalDepartments, value); }

    int activeStudents;
    public int ActiveStudents { get => activeStudents; set => SetProperty(ref activeStudents, value); }

    int suspendedStudents;
    public int SuspendedStudents { get => suspendedStudents; set => SetProperty(ref suspendedStudents, value); }

    int pendingLecturers;
    public int PendingLecturers { get => pendingLecturers; set => SetProperty(ref pendingLecturers, value); }

    double averageAttendanceRate;
    public double AverageAttendanceRate { get => averageAttendanceRate; set => SetProperty(ref averageAttendanceRate, value); }

    // ── Derived bar-chart widths (0–1) ────────────────────────────────────────
    public double StudentsProportion   => SafeProportion(TotalStudents, TotalStudents + TotalLecturers);
    public double LecturersProportion  => SafeProportion(TotalLecturers, TotalStudents + TotalLecturers);
    public double ActiveProportion     => SafeProportion(ActiveStudents, TotalStudents);
    public double SuspendedProportion  => SafeProportion(SuspendedStudents, TotalStudents);
    public double AttendanceProportion => AverageAttendanceRate / 100.0;

    // ── State ─────────────────────────────────────────────────────────────────
    bool hasError;
    public bool HasError { get => hasError; set => SetProperty(ref hasError, value); }

    string lastUpdated = string.Empty;
    public string LastUpdated { get => lastUpdated; set => SetProperty(ref lastUpdated, value); }

    // ── Charts ────────────────────────────────────────────────────────────────
    Chart? userDistributionChart;
    public Chart? UserDistributionChart { get => userDistributionChart; set => SetProperty(ref userDistributionChart, value); }

    Chart? auditActivityChart;
    public Chart? AuditActivityChart
    {
        get => auditActivityChart;
        set
        {
            if (SetProperty(ref auditActivityChart, value))
                RaisePropertyChanged(nameof(AuditChartEmpty));
        }
    }
    public bool AuditChartEmpty => auditActivityChart is null;

    Chart? attendanceChart;
    public Chart? AttendanceChart
    {
        get => attendanceChart;
        set
        {
            if (SetProperty(ref attendanceChart, value))
                RaisePropertyChanged(nameof(AttendanceChartEmpty));
        }
    }
    public bool AttendanceChartEmpty => attendanceChart is null;

    bool chartsLoaded;
    public bool ChartsLoaded { get => chartsLoaded; set => SetProperty(ref chartsLoaded, value); }

    // ── Trend charts ──────────────────────────────────────────────────────────
    Chart? studentGrowthChart;
    public Chart? StudentGrowthChart
    {
        get => studentGrowthChart;
        set
        {
            if (SetProperty(ref studentGrowthChart, value))
                RaisePropertyChanged(nameof(StudentGrowthChartEmpty));
        }
    }
    public bool StudentGrowthChartEmpty => studentGrowthChart is null;

    Chart? attendanceTrendChart;
    public Chart? AttendanceTrendChart
    {
        get => attendanceTrendChart;
        set
        {
            if (SetProperty(ref attendanceTrendChart, value))
                RaisePropertyChanged(nameof(AttendanceTrendChartEmpty));
        }
    }
    public bool AttendanceTrendChartEmpty => attendanceTrendChart is null;

    Chart? approvalTrendChart;
    public Chart? ApprovalTrendChart
    {
        get => approvalTrendChart;
        set
        {
            if (SetProperty(ref approvalTrendChart, value))
                RaisePropertyChanged(nameof(ApprovalTrendChartEmpty));
        }
    }
    public bool ApprovalTrendChartEmpty => approvalTrendChart is null;

    bool trendsLoaded;
    public bool TrendsLoaded { get => trendsLoaded; set => SetProperty(ref trendsLoaded, value); }

    // ── Delta indicators ──────────────────────────────────────────────────────
    string studentGrowthDelta = string.Empty;
    public string StudentGrowthDelta { get => studentGrowthDelta; set => SetProperty(ref studentGrowthDelta, value); }

    Color studentDeltaColor = Colors.Gray;
    public Color StudentDeltaColor { get => studentDeltaColor; set => SetProperty(ref studentDeltaColor, value); }

    string attendanceDelta = string.Empty;
    public string AttendanceDelta { get => attendanceDelta; set => SetProperty(ref attendanceDelta, value); }

    Color attendanceDeltaColor = Colors.Gray;
    public Color AttendanceDeltaColor { get => attendanceDeltaColor; set => SetProperty(ref attendanceDeltaColor, value); }

    string approvalsTotal = string.Empty;
    public string ApprovalsTotal { get => approvalsTotal; set => SetProperty(ref approvalsTotal, value); }

    // ── Time filter ───────────────────────────────────────────────────────────
    public IReadOnlyList<string> TimeRanges { get; } = ["Daily", "Weekly", "Monthly"];

    string selectedTimeRange = "Weekly";
    public string SelectedTimeRange
    {
        get => selectedTimeRange;
        set
        {
            if (SetProperty(ref selectedTimeRange, value))
                _ = RebuildChartsAsync();
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminAnalyticsViewModel(IAdminService admin, ILogger<AdminAnalyticsViewModel> logger)
    {
        _admin = admin;
        _logger = logger;
        Title = "Analytics";
        LoadCommand = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        HasError = false;
        ChartsLoaded = false;

        try
        {
            var ct = CreateLinkedToken();

            // ── 1. Stats (unchanged) ──────────────────────────────────────────
            var stats = await _admin.GetAdminStatsAsync(ct);
            if (stats is null)
            {
                HasError = true;
                await ShowErrorToastAsync("Could not load analytics data.");
                return;
            }

            TotalStudents      = stats.TotalStudents;
            TotalLecturers     = stats.TotalLecturers;
            TotalPrograms      = stats.TotalPrograms;
            TotalClasses       = stats.TotalClasses;
            TotalUniversities  = stats.TotalUniversities;
            TotalFaculties     = stats.TotalFaculties;
            TotalDepartments   = stats.TotalDepartments;
            ActiveStudents     = stats.ActiveStudents;
            SuspendedStudents  = stats.SuspendedStudents;
            PendingLecturers   = stats.PendingLecturers;
            AverageAttendanceRate = stats.AverageAttendanceRate;

            RaisePropertyChanged(nameof(StudentsProportion));
            RaisePropertyChanged(nameof(LecturersProportion));
            RaisePropertyChanged(nameof(ActiveProportion));
            RaisePropertyChanged(nameof(SuspendedProportion));
            RaisePropertyChanged(nameof(AttendanceProportion));

            // ── 2. User-distribution chart ────────────────────────────────────
            BuildUserDistributionChart();

            // ── 3. Attendance chart ───────────────────────────────────────────
            try
            {
                var attendance = await _admin.GetAttendanceSummaryAsync(ct);
                BuildAttendanceChart(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Attendance summary unavailable for chart");
            }

            // ── 4. Audit-activity chart (time-range aware) ────────────────────
            try
            {
                var (from, to) = GetDateRange();
                var auditResult = await _admin.GetAuditLogsAsync(
                    page: 1, pageSize: 200,
                    fromDate: from, toDate: to,
                    ct: ct);
                _cachedAuditLogs = auditResult?.Items ?? [];
                BuildAuditChart(_cachedAuditLogs);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audit logs unavailable for chart");
            }

            // ── 5. Trend charts ───────────────────────────────────────────────
            try
            {
                var (tFrom, tTo, gran) = GetTrendsDateRange();
                var trends = await _admin.GetTrendsAsync(tFrom, tTo, gran, ct);
                BuildTrendCharts(trends);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Trend data unavailable");
            }

            ChartsLoaded = true;
            LastUpdated = $"Updated {DateTime.Now:HH:mm}";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analytics load failed");
            HasError = true;
            await ShowErrorToastAsync("Failed to load analytics.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Rebuilds both audit and trend charts when the time-range picker changes.
    private async Task RebuildChartsAsync()
    {
        try
        {
            var ct = CreateLinkedToken();

            // Audit chart
            var (from, to) = GetDateRange();
            var result = await _admin.GetAuditLogsAsync(
                page: 1, pageSize: 200,
                fromDate: from, toDate: to,
                ct: ct);
            _cachedAuditLogs = result?.Items ?? [];
            BuildAuditChart(_cachedAuditLogs);

            // Trend charts
            var (tFrom, tTo, gran) = GetTrendsDateRange();
            var trends = await _admin.GetTrendsAsync(tFrom, tTo, gran, ct);
            BuildTrendCharts(trends);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Chart rebuild failed for range {Range}", SelectedTimeRange);
        }
    }

    // ── Chart builders ────────────────────────────────────────────────────────

    private void BuildUserDistributionChart()
    {
        var entries = new[]
        {
            new ChartEntry(TotalStudents)
            {
                Label = "Students",
                ValueLabel = TotalStudents.ToString(),
                Color = SKColor.Parse("#1976D2"),
                TextColor = SKColor.Parse("#1976D2")
            },
            new ChartEntry(TotalLecturers)
            {
                Label = "Lecturers",
                ValueLabel = TotalLecturers.ToString(),
                Color = SKColor.Parse("#388E3C"),
                TextColor = SKColor.Parse("#388E3C")
            },
            new ChartEntry(ActiveStudents)
            {
                Label = "Active",
                ValueLabel = ActiveStudents.ToString(),
                Color = SKColor.Parse("#22C55E"),
                TextColor = SKColor.Parse("#22C55E")
            },
            new ChartEntry(SuspendedStudents)
            {
                Label = "Suspended",
                ValueLabel = SuspendedStudents.ToString(),
                Color = SKColor.Parse("#EF4444"),
                TextColor = SKColor.Parse("#EF4444")
            },
            new ChartEntry(PendingLecturers)
            {
                Label = "Pending",
                ValueLabel = PendingLecturers.ToString(),
                Color = SKColor.Parse("#F59E0B"),
                TextColor = SKColor.Parse("#F59E0B")
            },
        };

        UserDistributionChart = new BarChart
        {
            Entries = entries,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 28f,
            ValueLabelTextSize = 28f,
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            IsAnimated = true
        };
    }

    private void BuildAttendanceChart(List<AdminAttendanceSummaryDto> attendance)
    {
        var top = attendance
            .OrderByDescending(a => a.AverageAttendanceRate)
            .Take(6)
            .ToList();

        if (top.Count == 0)
        {
            AttendanceChart = null;
            return;
        }

        var entries = top.Select(a =>
        {
            var color = a.AverageAttendanceRate >= 75
                ? SKColor.Parse("#22C55E")
                : a.AverageAttendanceRate >= 50
                    ? SKColor.Parse("#F59E0B")
                    : SKColor.Parse("#EF4444");

            var label = a.ClassName.Length > 10 ? a.ClassName[..10] + "\u2026" : a.ClassName;

            return new ChartEntry((float)a.AverageAttendanceRate)
            {
                Label = label,
                ValueLabel = $"{a.AverageAttendanceRate:F0}%",
                Color = color,
                TextColor = color
            };
        }).ToArray();

        AttendanceChart = new BarChart
        {
            Entries = entries,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 28f,
            ValueLabelTextSize = 28f,
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            MaxValue = 100f,
            IsAnimated = true
        };
    }

    private void BuildAuditChart(List<AuditLogDto> logs)
    {
        if (logs.Count == 0)
        {
            AuditActivityChart = null;
            return;
        }

        var grouped = logs
            .GroupBy(l => l.Action.ToUpperInvariant())
            .OrderByDescending(g => g.Count())
            .Take(6)
            .ToList();

        var palette = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CREATE"]   = "#22C55E",
            ["UPDATE"]   = "#3B82F6",
            ["DELETE"]   = "#EF4444",
            ["APPROVE"]  = "#10B981",
            ["SUSPEND"]  = "#F59E0B",
            ["ACTIVATE"] = "#22C55E",
        };

        var entries = grouped.Select(g =>
        {
            var hex = palette.TryGetValue(g.Key, out var c) ? c : "#6B7280";
            var color = SKColor.Parse(hex);
            return new ChartEntry(g.Count())
            {
                Label = g.Key,
                ValueLabel = g.Count().ToString(),
                Color = color,
                TextColor = color
            };
        }).ToArray();

        AuditActivityChart = new BarChart
        {
            Entries = entries,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 28f,
            ValueLabelTextSize = 28f,
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            IsAnimated = true
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private (DateTime from, DateTime to) GetDateRange() => SelectedTimeRange switch
    {
        "Daily"   => (DateTime.Today, DateTime.Today.AddDays(1).AddSeconds(-1)),
        "Monthly" => (DateTime.Today.AddDays(-30), DateTime.Today.AddDays(1).AddSeconds(-1)),
        _         => (DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1).AddSeconds(-1))
    };

    // Wider windows so trend line charts have enough data points to be meaningful
    private (DateTime from, DateTime to, string granularity) GetTrendsDateRange() =>
        SelectedTimeRange switch
        {
            "Daily"   => (DateTime.Today.AddDays(-14),  DateTime.Today, "daily"),
            "Monthly" => (DateTime.Today.AddDays(-365), DateTime.Today, "monthly"),
            _         => (DateTime.Today.AddDays(-84),  DateTime.Today, "weekly")
        };

    private void BuildTrendCharts(AdminTrendsDto? data)
    {
        TrendsLoaded = false;

        if (data is null)
        {
            StudentGrowthChart   = null;
            AttendanceTrendChart = null;
            ApprovalTrendChart   = null;
            StudentGrowthDelta   = string.Empty;
            AttendanceDelta      = string.Empty;
            ApprovalsTotal       = string.Empty;
            TrendsLoaded = true;
            return;
        }

        StudentGrowthChart = BuildLineChart(data.StudentGrowth, "#1976D2");
        (StudentGrowthDelta, StudentDeltaColor) = FormatDelta(ComputeDelta(data.StudentGrowth));

        AttendanceTrendChart = BuildLineChart(data.AttendanceTrend, "#22C55E");
        (AttendanceDelta, AttendanceDeltaColor) = FormatDelta(ComputeDelta(data.AttendanceTrend));

        ApprovalTrendChart = data.ApprovalsTrend.Count > 0
            ? BuildBarChart(data.ApprovalsTrend, "#10B981")
            : null;
        ApprovalsTotal = data.ApprovalsTrend.Sum(p => p.Value).ToString("F0");

        TrendsLoaded = true;
    }

    private static Chart? BuildLineChart(List<TimePointDto> points, string hexColor)
    {
        if (points.Count == 0) return null;
        var color = SKColor.Parse(hexColor);
        var entries = points.Select(p => new ChartEntry((float)p.Value)
        {
            Label      = p.Label,
            ValueLabel = p.Value.ToString("F0"),
            Color      = color,
            TextColor  = color
        }).ToArray();

        return new LineChart
        {
            Entries               = entries,
            BackgroundColor       = SKColors.Transparent,
            LabelTextSize         = 26f,
            ValueLabelTextSize    = 26f,
            LabelOrientation      = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            LineMode              = LineMode.Spline,
            LineSize              = 3f,
            PointMode             = PointMode.Circle,
            PointSize             = 10f,
            IsAnimated            = true
        };
    }

    private static Chart? BuildBarChart(List<TimePointDto> points, string hexColor)
    {
        if (points.Count == 0) return null;
        var color = SKColor.Parse(hexColor);
        var entries = points.Select(p => new ChartEntry((float)p.Value)
        {
            Label      = p.Label,
            ValueLabel = p.Value.ToString("F0"),
            Color      = color,
            TextColor  = color
        }).ToArray();

        return new BarChart
        {
            Entries               = entries,
            BackgroundColor       = SKColors.Transparent,
            LabelTextSize         = 26f,
            ValueLabelTextSize    = 26f,
            LabelOrientation      = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            IsAnimated            = true
        };
    }

    // Compare second half vs first half → growth percentage
    private static double ComputeDelta(List<TimePointDto> points)
    {
        if (points.Count < 2) return 0;
        var mid  = points.Count / 2;
        var prev = points.Take(mid).Average(p => p.Value);
        var curr = points.Skip(mid).Average(p => p.Value);
        if (prev == 0) return curr > 0 ? 100 : 0;
        return Math.Round((curr - prev) / prev * 100, 1);
    }

    private static (string text, Color color) FormatDelta(double delta)
    {
        if (delta == 0) return ("→ 0%", Colors.Gray);
        return delta > 0
            ? ($"▲ +{delta:F1}%", Color.FromArgb("#22C55E"))
            : ($"▼ {delta:F1}%",  Color.FromArgb("#EF4444"));
    }

    private static double SafeProportion(int part, int total)
        => total <= 0 ? 0 : Math.Clamp(part / (double)total, 0, 1);
}
