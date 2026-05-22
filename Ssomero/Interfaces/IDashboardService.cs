using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(bool forceRefresh = false);
}
