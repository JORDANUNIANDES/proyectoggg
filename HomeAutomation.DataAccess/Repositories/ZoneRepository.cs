using HomeAutomation.DataAccess.Data;
using HomeAutomation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomation.DataAccess.Repositories
{
    public class ZoneRepository : IZoneRepository
    {
        private readonly ApplicationDbContext _context;

        public ZoneRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Zone> GetByIdAsync(int zoneId)
        {
            return await _context.Zones.FindAsync(zoneId);
        }

        public async Task<Zone> GetByIdWithDevicesAsync(int zoneId)
        {
            return await _context.Zones
                                 .Include(z => z.Devices) // Cargar los dispositivos asociados
                                 .FirstOrDefaultAsync(z => z.ZoneId == zoneId);
        }

        public async Task<Zone> GetByNameAsync(string name)
        {
            return await _context.Zones.FirstOrDefaultAsync(z => z.Name == name);
        }

        public async Task<List<Zone>> GetAllAsync()
        {
            return await _context.Zones.ToListAsync();
        }

        public async Task AddAsync(Zone zone)
        {
            await _context.Zones.AddAsync(zone);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Zone zone)
        {
            _context.Zones.Update(zone);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int zoneId)
        {
            var zone = await GetByIdAsync(zoneId); // Podríamos necesitar GetByIdWithDevicesAsync si hay lógica de desasignación
            if (zone != null)
            {
                // Considerar qué pasa con los dispositivos en esta zona.
                // EF Core podría manejarlo basado en la configuración de FK (SET NULL si ZoneId es nullable en Device).
                // O podrías necesitar desasignarlos manualmente aquí o en el servicio antes de eliminar.
                // Por ejemplo, si ZoneId en Device no es nullable y no hay CascadeOnDelete, esto fallará.
                // Si ZoneId es nullable y OnDelete es SetNull, EF Core lo hará.
                // Si quieres eliminar los dispositivos (no usual), necesitarías configurar CascadeOnDelete en la relación.
                _context.Zones.Remove(zone);
                await _context.SaveChangesAsync();
            }
        }
    }
}
