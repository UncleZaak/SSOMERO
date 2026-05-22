using System.Windows.Input;
using Microcharts;
using Microsoft.Maui.Controls;
using SkiaSharp;
using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.ViewModels;

public class ClassRepAnalyticsViewModel : BaseViewModel
{
    private readonly IClassAnnouncementApiService _analyticsApi;

    // ── Properties ────────────────────────────────────────────────────────────
    private ClassRepAnalyticsModel? _analytics;
    public ClassRepAnalyticsModel? Analytics
    {
        get => _analytics;
        set => SetProperty(ref _analytics, value);
    }

    private Chart? _attendanceTrendChart;
    public Chart? AttendanceTrendChart
    {
        get => _attendanceTrendChart;
        set => SetProperty(ref _attendanceTrendChart, value);
    }

    private Chart? _studentGrowthChart;
    public Chart? StudentGrowthChart
    {
        get => _studentGrowthChart;
        set => SetProperty(ref _studentGrowthChart, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); RaisePropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // ── Commands ──────────────────────────────────────────────────────────────
    public ICommand LoadCommand    { get; }
    public ICommand RefreshCommand { get; }

    public ClassRepAnalyticsViewModel(IClassAnnouncementApiService analyticsApi)
    {
        _analyticsApi  = analyticsApi;
        Title          = "Class Analytics";
        LoadCommand    = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        var ct = CreateLinkedToken();
        try
        {
            var data = await _analyticsApi.GetAnalyticsAsync(ct);

            if (data is null)
            {
                ErrorMessage = "Could not load analytics data.";
                return;
            }

            Analytics = data;
            BuildCharts(data);
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildCharts(ClassRepAnalyticsModel data)
    {
        // ── Attendance trend line chart ────────────────────────────────────
        var attendanceEntries = data.AttendanceTrend
            .Select(p => new ChartEntry((float)p.Value)
            {
                Label      = p.Label,
                ValueLabel = $"{p.Value:F0}%",
                Color      = SKColor.Parse("#6366F1"),
            })
            .ToArray();

        AttendanceTrendChart = new LineChart
        {
            Entries          = attendanceEntries,
            LineMode         = LineMode.Spline,
            LineSize         = 4,
            PointMode        = PointMode.Circle,
            PointSize        = 8,
            BackgroundColor  = SKColors.Transparent,
            LabelTextSize    = 26,
            ValueLabelOption = ValueLabelOption.TopOfElement,
        };

        // ── Student growth line chart ─────────────────────────────────────
        var growthEntries = data.StudentGrowthTrend
            .Select(p => new ChartEntry((float)p.Value)
            {
                Label      = p.Label,
                ValueLabel = $"{(int)p.Value}",
                Color      = SKColor.Parse("#10B981"),
            })
            .ToArray();

        StudentGrowthChart = new LineChart
        {
            Entries          = growthEntries,
            LineMode         = LineMode.Spline,
            LineSize         = 4,
            PointMode        = PointMode.Circle,
            PointSize        = 8,
            BackgroundColor  = SKColors.Transparent,
            LabelTextSize    = 26,
            ValueLabelOption = ValueLabelOption.TopOfElement,
        };
    }
}
