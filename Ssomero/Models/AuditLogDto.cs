namespace Ssomero.Models;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    // Derived display helpers
    public string ActionIcon => Action.ToUpperInvariant() switch
    {
        "CREATE" => "➕",
        "UPDATE" => "✏️",
        "DELETE" => "🗑️",
        "APPROVE" => "✅",
        "SUSPEND" => "⚠️",
        "ACTIVATE" => "🟢",
        _ => "📋"
    };

    public Color ActionColor => Action.ToUpperInvariant() switch
    {
        "CREATE"   => Color.FromArgb("#22C55E"),
        "UPDATE"   => Color.FromArgb("#3B82F6"),
        "DELETE"   => Color.FromArgb("#EF4444"),
        "APPROVE"  => Color.FromArgb("#10B981"),
        "SUSPEND"  => Color.FromArgb("#F59E0B"),
        "ACTIVATE" => Color.FromArgb("#22C55E"),
        _ => Color.FromArgb("#6B7280")
    };

    public string FormattedDate => CreatedAt.ToLocalTime().ToString("MMM dd, yyyy  HH:mm");
    public string PerformedBy => UserEmail ?? "System";
}

public class AuditLogPagedResult
{
    public List<AuditLogDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Math.Max(PageSize, 1));
}
