using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IGroupsService
{
    Task<IEnumerable<StudyGroupDto>> GetGroupsAsync(CancellationToken ct = default);
    Task<IEnumerable<GroupMessageDto>> GetGroupMessagesAsync(string groupId, CancellationToken ct = default);
    Task<bool> SendMessageAsync(string groupId, string text, CancellationToken ct = default);
}
