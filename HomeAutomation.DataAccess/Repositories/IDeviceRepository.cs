using HomeAutomation.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAutomation.DataAccess.Repositories
{
    public interface IDeviceRepository
    {
        Task<Device> GetByIdAsync(int deviceId);
        Task<List<Device>> GetAllAsync();
        Task<List<Device>> GetDevicesByUserIdAsync(int userId);
        Task<List<Device>> GetDevicesByZoneIdAsync(int zoneId);
        Task AddAsync(Device device);
        Task UpdateAsync(Device device);
        Task DeleteAsync(int deviceId);
    }
}
