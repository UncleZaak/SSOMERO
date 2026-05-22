using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

/// <summary>
/// A file or link uploaded by a lecturer for a specific subclass.
/// </summary>
public class ClassMaterial
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? FileUrl { get; set; }

    /// <summary>FK to the lecturer who uploaded this material.</summary>
    public Guid UploadedBy { get; set; }
    public Lecturer? Lecturer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
