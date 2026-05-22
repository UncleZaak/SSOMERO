using System.Collections.Generic;
using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IAnnouncementsService
{
    Task<IEnumerable<AnnouncementDto>> GetAnnouncementsAsync();
}
