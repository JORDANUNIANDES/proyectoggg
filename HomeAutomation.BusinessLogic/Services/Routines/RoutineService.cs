using HomeAutomation.BusinessLogic.Scheduling; // Para ISchedulingService
using HomeAutomation.DataAccess.Entities;
using HomeAutomation.DataAccess.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Services.Routines
{
    public class RoutineService : IRoutineService
    {
        private readonly IRoutineRepository _routineRepository;
        private readonly ISchedulingService _schedulingService;
        private readonly ILogger<RoutineService> _logger;
        private readonly IDeviceRepository _deviceRepository; // Para validar DeviceIds en RoutineDevices

        public RoutineService(
            IRoutineRepository routineRepository,
            ISchedulingService schedulingService,
            IDeviceRepository deviceRepository,
            ILogger<RoutineService> logger)
        {
            _routineRepository = routineRepository ?? throw new ArgumentNullException(nameof(routineRepository));
            _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
            _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Routine> GetRoutineByIdAsync(int routineId)
        {
            _logger.LogInformation("Obteniendo rutina por ID: {RoutineId}", routineId);
            return await _routineRepository.GetByIdWithDevicesAsync(routineId); // Cargar dispositivos para la edición/visualización
        }

        public async Task<List<Routine>> GetAllRoutinesAsync()
        {
            // En una aplicación real, filtrar por usuario/rol.
            _logger.LogInformation("Obteniendo todas las rutinas.");
            return await _routineRepository.GetAllAsync();
        }

        public async Task<List<Routine>> GetRoutinesByUserIdAsync(int userId)
        {
            _logger.LogInformation("Obteniendo rutinas para el usuario ID: {UserId}", userId);
            return await _routineRepository.GetRoutinesByUserIdAsync(userId);
        }

        public async Task<Routine> AddRoutineAsync(Routine routine, int userId)
        {
            if (routine == null) throw new ArgumentNullException(nameof(routine));
            if (string.IsNullOrWhiteSpace(routine.Name)) throw new ArgumentException("El nombre de la rutina no puede estar vacío.", nameof(routine.Name));

            // Validar que los DeviceId en RoutineDevices existan y pertenezcan al usuario (o sean accesibles)
            if (routine.RoutineDevices != null)
            {
                foreach (var rd in routine.RoutineDevices)
                {
                    var device = await _deviceRepository.GetByIdAsync(rd.DeviceId);
                    if (device == null /*|| device.UserId != userId*/) // Descomentar si los dispositivos deben ser del mismo usuario
                    {
                        _logger.LogWarning("Intento de agregar rutina con dispositivo inválido ID: {DeviceId}", rd.DeviceId);
                        throw new KeyNotFoundException($"Dispositivo con ID {rd.DeviceId} no encontrado o no accesible.");
                    }
                }
            }

            routine.UserId = userId;
            _logger.LogInformation("Agregando nueva rutina: {RoutineName} para el usuario ID: {UserId}", routine.Name, userId);
            await _routineRepository.AddAsync(routine); // Esto debería guardar la rutina y sus RoutineDevices

            if (routine.IsEnabled)
            {
                await _schedulingService.ScheduleRoutineAsync(routine);
            }
            return routine;
        }

        public async Task UpdateRoutineAsync(Routine routine)
        {
            if (routine == null) throw new ArgumentNullException(nameof(routine));
            // Aquí se podrían añadir validaciones de permisos.

            _logger.LogInformation("Actualizando rutina ID: {RoutineId}", routine.RoutineId);

            // Validar dispositivos como en AddRoutineAsync
            if (routine.RoutineDevices != null)
            {
                foreach (var rd in routine.RoutineDevices)
                {
                    var device = await _deviceRepository.GetByIdAsync(rd.DeviceId);
                    if (device == null /*|| device.UserId != routine.UserId*/) // Asumiendo que routine.UserId está poblado
                    {
                         _logger.LogWarning("Intento de actualizar rutina ID {RoutineId} con dispositivo inválido ID: {DeviceId}", routine.RoutineId, rd.DeviceId);
                        throw new KeyNotFoundException($"Dispositivo con ID {rd.DeviceId} no encontrado o no accesible para la rutina.");
                    }
                }
            }

            await _routineRepository.UpdateAsync(routine); // UpdateAsync en el repo maneja la sincronización de RoutineDevices

            // Actualizar la programación en Quartz
            // UpdateScheduledRoutineAsync se encarga de desprogramar si ya no está habilitada, o reprogramar si cambió.
            await _schedulingService.UpdateScheduledRoutineAsync(routine);
        }

        public async Task DeleteRoutineAsync(int routineId)
        {
            // Aquí se podrían añadir validaciones de permisos.
            _logger.LogInformation("Eliminando rutina ID: {RoutineId}", routineId);
            await _schedulingService.UnscheduleRoutineAsync(routineId); // Primero desprogramar
            await _routineRepository.DeleteAsync(routineId); // Luego eliminar de la BD
        }

        public async Task ExecuteRoutineAsync(int routineId)
        {
            _logger.LogInformation("Ejecutando manualmente rutina ID: {RoutineId}", routineId);
            // El SchedulingService tiene un TriggerRoutineNow que internamente usa el ExecuteRoutineJob.
            // O, si queremos evitar Quartz para una ejecución manual directa:
            var routine = await _routineRepository.GetByIdWithDevicesAsync(routineId);
            if (routine == null)
            {
                _logger.LogWarning("Intento de ejecutar rutina no existente ID: {RoutineId}", routineId);
                throw new KeyNotFoundException("Rutina no encontrada.");
            }
            if (routine.RoutineDevices == null || !routine.RoutineDevices.Any())
            {
                _logger.LogInformation("Rutina ID: {RoutineId} no tiene acciones para ejecutar.", routineId);
                return;
            }

            // Esto duplica la lógica de ExecuteRoutineJob. Sería mejor si ExecuteRoutineJob
            // pudiera ser invocado directamente o si SchedulingService.TriggerRoutineNow es suficiente.
            // Por ahora, para ilustrar:
            foreach (var rd in routine.RoutineDevices)
            {
                if (rd.Device == null) continue;
                var parts = rd.ActionToPerform.Split(':');
                string command = parts[0];
                string value = parts.Length > 1 ? parts[1] : null;
                try
                {
                    // Asumimos que IDeviceService está disponible o se inyecta aquí
                    // o que SchedulingService.TriggerRoutineNow es la vía preferida.
                    // Aquí llamamos a SchedulingService para mantener la lógica de ejecución centralizada.
                    await _schedulingService.TriggerRoutineNowAsync(routineId);
                    break; // Solo necesitamos llamar a TriggerRoutineNow una vez para la rutina completa.
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "Error ejecutando acción '{Action}' en dispositivo {DeviceId} para rutina manual {RoutineId}", rd.ActionToPerform, rd.DeviceId, routineId);
                }
            }
        }
        public async Task ToggleRoutineEnabledAsync(int routineId, bool isEnabled)
        {
            var routine = await _routineRepository.GetByIdAsync(routineId);
            if (routine == null)
            {
                _logger.LogWarning("Intento de habilitar/deshabilitar rutina no existente ID: {RoutineId}", routineId);
                throw new KeyNotFoundException("Rutina no encontrada.");
            }

            if (routine.IsEnabled == isEnabled) return; // Sin cambios

            routine.IsEnabled = isEnabled;
            await _routineRepository.UpdateAsync(routine); // Guardar el cambio de estado

            if (isEnabled)
            {
                await _schedulingService.ScheduleRoutineAsync(routine);
                _logger.LogInformation("Rutina ID {RoutineId} habilitada y programada.", routineId);
            }
            else
            {
                await _schedulingService.UnscheduleRoutineAsync(routineId);
                _logger.LogInformation("Rutina ID {RoutineId} deshabilitada y desprogramada.", routineId);
            }
        }
    }
}
