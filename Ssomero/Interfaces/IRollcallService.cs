using System.Collections.Generic;
using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IRollcallService
{
    Task<IEnumerable<RollcallDto>> GetMyRollcallsAsync();
    Task<IEnumerable<RollcallDto>> GetPendingApprovalsAsync();
    Task<bool> SubmitRollcallAsync(string scheduleId, Stream selfieStream, string fileName);
    Task<bool> ApproveRollcallAsync(string rollcallId);
    Task<bool> RejectRollcallAsync(string rollcallId);
}
