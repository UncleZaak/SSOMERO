using System.Collections.Generic;
using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IScheduleService
{
    Task<IEnumerable<ScheduleDto>> GetSchedulesAsync();
    Task<bool> CreateScheduleAsync(ScheduleDto schedule);
    Task<bool> CancelScheduleAsync(string scheduleId);
}
