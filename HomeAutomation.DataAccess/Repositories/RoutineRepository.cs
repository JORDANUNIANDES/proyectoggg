using HomeAutomation.DataAccess.Data;
using HomeAutomation.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomation.DataAccess.Repositories
{
    public class RoutineRepository : IRoutineRepository
    {
        private readonly ApplicationDbContext _context;

        public RoutineRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Routine> GetByIdAsync(int routineId)
        {
            return await _context.Routines.FindAsync(routineId);
        }

        public async Task<Routine> GetByIdWithDevicesAsync(int routineId)
        {
            return await _context.Routines
                                 .Include(r => r.RoutineDevices)
                                     .ThenInclude(rd => rd.Device) // Carga los Devices a través de RoutineDevices
                                 .Include(r => r.User) // Carga el usuario dueño
                                 .FirstOrDefaultAsync(r => r.RoutineId == routineId);
        }

        public async Task<List<Routine>> GetAllAsync()
        {
            return await _context.Routines
                                 .Include(r => r.User) // Opcional, si se muestra el dueño en una lista general
                                 .ToListAsync();
        }

        public async Task<List<Routine>> GetRoutinesByUserIdAsync(int userId)
        {
            return await _context.Routines
                                 .Where(r => r.UserId == userId)
                                 .Include(r => r.RoutineDevices) // Útil si se muestran detalles de acciones en la lista del usuario
                                     .ThenInclude(rd => rd.Device)
                                 .ToListAsync();
        }

        public async Task AddAsync(Routine routine)
        {
            // Si routine.RoutineDevices tiene elementos, EF Core los añadirá también
            // si son nuevas entidades y la relación está configurada.
            await _context.Routines.AddAsync(routine);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Routine routine)
        {
            // Actualizar una rutina puede ser complejo si sus RoutineDevices cambian.
            // Una estrategia común es:
            // 1. Cargar la rutina existente con sus RoutineDevices.
            // 2. Eliminar los RoutineDevices que ya no están en la rutina actualizada.
            // 3. Agregar los nuevos RoutineDevices.
            // 4. Actualizar las propiedades escalares de la rutina.

            var existingRoutine = await _context.Routines
                                                .Include(r => r.RoutineDevices)
                                                .FirstOrDefaultAsync(r => r.RoutineId == routine.RoutineId);

            if (existingRoutine == null)
            {
                // Manejar error, rutina no encontrada
                return;
            }

            // Actualizar propiedades escalares
            _context.Entry(existingRoutine).CurrentValues.SetValues(routine);
            existingRoutine.IsEnabled = routine.IsEnabled; // Asegurarse que se actualizan todas las necesarias
            existingRoutine.ScheduledTime = routine.ScheduledTime;
            existingRoutine.ExecutionDays = routine.ExecutionDays;


            // Sincronizar RoutineDevices
            // Eliminar los que ya no están
            var routineDevicesToRemove = existingRoutine.RoutineDevices
                .Where(erd => !routine.RoutineDevices.Any(nrd => nrd.DeviceId == erd.DeviceId && nrd.ActionToPerform == erd.ActionToPerform)) // O por ID si los RoutineDevice tienen ID del cliente
                .ToList();
            foreach (var rdToRemove in routineDevicesToRemove)
            {
                _context.RoutineDevices.Remove(rdToRemove);
            }

            // Agregar o actualizar los nuevos/modificados
            foreach (var newRd in routine.RoutineDevices)
            {
                var existingRd = existingRoutine.RoutineDevices
                    .FirstOrDefault(erd => erd.DeviceId == newRd.DeviceId && erd.ActionToPerform == newRd.ActionToPerform); // Asumiendo que la combinación es única o se maneja por ID

                if (existingRd == null) // Nuevo
                {
                    // Asegurarse que RoutineId está seteado o EF lo asocia
                    newRd.RoutineId = existingRoutine.RoutineId;
                    existingRoutine.RoutineDevices.Add(newRd); // EF Core debería manejar la FK
                }
                // else // Si RoutineDevice tuviera otras propiedades que actualizar
                // {
                //     _context.Entry(existingRd).CurrentValues.SetValues(newRd);
                // }
            }

            // _context.Routines.Update(routine); // Esto podría ser demasiado simple si RoutineDevices cambia.
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int routineId)
        {
            var routine = await GetByIdAsync(routineId); // GetByIdWithDevicesAsync no es necesario si Cascade Delete está configurado para RoutineDevices
            if (routine != null)
            {
                // Si OnDelete Cascade está configurado en la relación Routine -> RoutineDevice,
                // EF Core eliminará los RoutineDevices asociados automáticamente.
                _context.Routines.Remove(routine);
                await _context.SaveChangesAsync();
            }
        }
    }
}
