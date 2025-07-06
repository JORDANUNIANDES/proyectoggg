using HomeAutomation.DataAccess.Data;
using HomeAutomation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomation.DataAccess.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ApplicationDbContext _context;

        public DeviceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Device> GetByIdAsync(int deviceId)
        {
            // Include(d => d.Zone) para cargar la información de la zona asociada.
            // Include(d => d.User) para cargar el usuario dueño.
            // Include(d => d.RoutineDevices) si necesitas las rutinas asociadas directamente aquí.
            return await _context.Devices
                                 .Include(d => d.Zone)
                                 .Include(d => d.User)
                                 .FirstOrDefaultAsync(d => d.DeviceId == deviceId);
        }

        public async Task<List<Device>> GetAllAsync()
        {
            return await _context.Devices
                                 .Include(d => d.Zone)
                                 .Include(d => d.User)
                                 .ToListAsync();
        }

        public async Task<List<Device>> GetDevicesByUserIdAsync(int userId)
        {
            return await _context.Devices
                                 .Include(d => d.Zone)
                                 .Where(d => d.UserId == userId)
                                 .ToListAsync();
        }
        public async Task<List<Device>> GetDevicesByZoneIdAsync(int zoneId)
        {
            return await _context.Devices
                                 .Include(d => d.User) // Incluir User si es relevante al filtrar por zona
                                 .Where(d => d.ZoneId == zoneId)
                                 .ToListAsync();
        }

        public async Task AddAsync(Device device)
        {
            await _context.Devices.AddAsync(device);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Device device)
        {
            // Asegurarse que la entidad User o Zone no se intenten insertar como nuevas si solo se actualiza el Device.
            // Si device.User o device.Zone son entidades existentes y rastreadas, EF Core debería manejarlo.
            // Si son nuevas instancias solo con IDs, EF Core podría intentar crearlas.
            // Una forma es solo actualizar las propiedades escalares o manejar el estado de las entidades navegadas.
            _context.Devices.Update(device);
            // O, para más control:
            // var existingDevice = await _context.Devices.FindAsync(device.DeviceId);
            // if (existingDevice != null)
            // {
            //     _context.Entry(existingDevice).CurrentValues.SetValues(device);
            //     existingDevice.ZoneId = device.ZoneId; // Actualizar FK explícitamente
            // }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int deviceId)
        {
            var device = await GetByIdAsync(deviceId); // Usar GetByIdAsync para asegurar que se carguen las relaciones si hay reglas de eliminación en cascada que manejar.
            if (device != null)
            {
                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();
            }
        }
    }
}
