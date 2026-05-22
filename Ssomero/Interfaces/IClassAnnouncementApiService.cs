using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IClassAnnouncementApiService
{
    Task<List<ClassAnnouncementModel>> GetAnnouncementsAsync(CancellationToken ct = default);
    Task<ClassAnnouncementModel?> CreateAnnouncementAsync(CreateClassAnnouncementRequest request, CancellationToken ct = default);
    Task<bool> DeleteAnnouncementAsync(Guid announcementId, CancellationToken ct = default);
    Task<ClassRepAnalyticsModel?> GetAnalyticsAsync(CancellationToken ct = default);
}
