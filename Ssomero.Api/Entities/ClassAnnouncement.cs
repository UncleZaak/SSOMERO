using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class ClassAnnouncement
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid CreatedBy { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
