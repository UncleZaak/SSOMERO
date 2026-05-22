namespace Ssomero.Models;

/// <summary>A single date-bucketed data point returned by the trends endpoint.</summary>
public class TimePointDto
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

/// <summary>Full trends response from GET /api/admin/analytics/trends.</summary>
public class AdminTrendsDto
{
    public List<TimePointDto> StudentGrowth   { get; set; } = [];
    public List<TimePointDto> LecturerGrowth  { get; set; } = [];
    public List<TimePointDto> AttendanceTrend { get; set; } = [];
    public List<TimePointDto> ApprovalsTrend  { get; set; } = [];
}
