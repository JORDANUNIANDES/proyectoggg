using HomeAutomation.DataAccess.Entities; // Para Device, etc.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Services.Devices
{
    public interface IDeviceService
    {
        Task<Device> GetDeviceByIdAsync(int deviceId);
        Task<List<Device>> GetAllDevicesAsync(); // Podría necesitar un User/Role para filtrar
        Task<List<Device>> GetDevicesByUserIdAsync(int userId);
        Task<Device> AddDeviceAsync(Device device, int userId); // Devuelve el dispositivo añadido con su ID
        Task UpdateDeviceAsync(Device device); // Podría necesitar User/Role para permisos
        Task DeleteDeviceAsync(int deviceId); // Podría necesitar User/Role para permisos
        Task AssignDeviceToZoneAsync(int deviceId, int? zoneId);
        Task SendCommandAsync(int deviceId, string command, string value);
        Task HandleDeviceStatusUpdateAsync(string deviceId, string command, string newStatus); // Para cuando SignalR notifica
    }
}
