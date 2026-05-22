namespace Ssomero.Models;

public class ClassModel
{
    public string Time { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public ClassStatus Status { get; set; }

    public string StatusText => Status switch
    {
        ClassStatus.Active    => "🟢 Active",
        ClassStatus.Upcoming  => "Upcoming",
        ClassStatus.Completed => "Done",
        _                     => string.Empty
    };

    public bool IsActive => Status == ClassStatus.Active;

    public string StatusColor => Status switch
    {
        ClassStatus.Active    => "#EDE9FE",
        ClassStatus.Upcoming  => "White",
        ClassStatus.Completed => "#F8FAFC",
        _                     => "White"
    };

    public string StatusTextColor => Status switch
    {
        ClassStatus.Active    => "#16A34A",
        ClassStatus.Upcoming  => "#5B21B6",
        ClassStatus.Completed => "#94A3B8",
        _                     => "#64748B"
    };
}

public enum ClassStatus
{
    Active,
    Upcoming,
    Completed
}
