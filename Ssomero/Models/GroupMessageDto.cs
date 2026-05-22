namespace Ssomero.Models;

public class GroupMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsOwn { get; set; }

    public string FormattedTime => SentAt.ToString("HH:mm");
    public string InitialsDisplay => SenderName.Length > 0
        ? string.Concat(SenderName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2).Select(w => w[0].ToString())).ToUpper()
        : "?";
}
