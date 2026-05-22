using Ssomero.Api.Dtos;

namespace Ssomero.Api.Services.Interfaces;

public interface IClassAnnouncementService
{
    /// <summary>Returns announcements for all classes managed by the given class rep.</summary>
    Task<IReadOnlyList<ClassAnnouncementDto>> GetAnnouncementsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Creates a new announcement. Enforces ownership of the target class.</summary>
    Task<ClassAnnouncementDto> CreateAnnouncementAsync(Guid userId, CreateClassAnnouncementDto dto, CancellationToken ct = default);

    /// <summary>Soft-deletes an announcement. Only the creator may delete.</summary>
    Task<bool> DeleteAnnouncementAsync(Guid userId, Guid announcementId, CancellationToken ct = default);

    /// <summary>Returns aggregated analytics and trend data for the class rep's managed classes.</summary>
    Task<ClassRepAnalyticsDto> GetAnalyticsAsync(Guid userId, CancellationToken ct = default);
}
