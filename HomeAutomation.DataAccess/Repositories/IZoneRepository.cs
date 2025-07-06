using HomeAutomation.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAutomation.DataAccess.Repositories
{
    public interface IZoneRepository
    {
        Task<Zone> GetByIdAsync(int zoneId);
        Task<Zone> GetByIdWithDevicesAsync(int zoneId); // Para cargar dispositivos de la zona
        Task<List<Zone>> GetAllAsync();
        Task AddAsync(Zone zone);
        Task UpdateAsync(Zone zone);
        Task DeleteAsync(int zoneId);
        Task<Zone> GetByNameAsync(string name); // Para verificar unicidad
    }
}
