namespace Ssomero.Models;

public class ClassSessionDto
{
    public Guid SessionId  { get; set; }
    public Guid ClassId    { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime   { get; set; }
    public string Location    { get; set; } = string.Empty;
    public string Lecturer    { get; set; } = string.Empty;

    public bool IsNow        => DateTime.Now >= StartTime && DateTime.Now <= EndTime;
    public bool IsToday      => StartTime.Date == DateTime.Today;
    public string TimeRange  => $"{StartTime:HH:mm} – {EndTime:HH:mm}";
    public string DayLabel   => StartTime.Date == DateTime.Today ? "Today"
                              : StartTime.Date == DateTime.Today.AddDays(1) ? "Tomorrow"
                              : StartTime.ToString("dddd, MMM dd");

    public string UrgencyLabel
    {
        get
        {
            if (IsNow) return "🟢 In Progress";
            var mins = (int)(StartTime - DateTime.Now).TotalMinutes;
            if (mins < 0) return "Done";
            if (mins <= 15) return $"⏰ In {mins} min";
            if (mins <= 60) return $"In {mins} min";
            return StartTime.ToString("HH:mm");
        }
    }
}
