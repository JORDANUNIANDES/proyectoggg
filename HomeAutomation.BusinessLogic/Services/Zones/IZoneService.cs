using HomeAutomation.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Services.Zones
{
    public interface IZoneService
    {
        Task<Zone> GetZoneByIdAsync(int zoneId);
        Task<List<Zone>> GetAllZonesAsync();
        Task<Zone> AddZoneAsync(Zone zone); // Devuelve la zona añadida con su ID
        Task UpdateZoneAsync(Zone zone);
        Task<bool> DeleteZoneAsync(int zoneId); // Devuelve true si se eliminó, false si hubo problemas (ej: zona con dispositivos)
    }
}
