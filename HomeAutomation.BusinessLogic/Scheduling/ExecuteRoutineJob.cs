using HomeAutomation.BusinessLogic.Services.Devices; // Para IDeviceService
using HomeAutomation.DataAccess.Repositories; // Para IRoutineRepository
using Microsoft.Extensions.Logging; // Para ILogger
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Scheduling
{
    [DisallowConcurrentExecution] // Evita que la misma rutina se ejecute concurrentemente si tarda mucho
    public class ExecuteRoutineJob : IRoutineJob
    {
        private readonly IRoutineRepository _routineRepository;
        private readonly IDeviceService _deviceService;
        private readonly ILogger<ExecuteRoutineJob> _logger;

        public ExecuteRoutineJob(
            IRoutineRepository routineRepository,
            IDeviceService deviceService,
            ILogger<ExecuteRoutineJob> logger)
        {
            _routineRepository = routineRepository ?? throw new ArgumentNullException(nameof(routineRepository));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;
            int routineId = jobDataMap.GetInt("RoutineId");

            _logger.LogInformation("Ejecutando RoutineJob para RoutineId: {RoutineId} - Hora: {ExecutionTime}", routineId, DateTime.Now);

            try
            {
                var routine = await _routineRepository.GetByIdWithDevicesAsync(routineId);
                if (routine == null)
                {
                    _logger.LogWarning("Rutina {RoutineId} no encontrada. Omitiendo ejecución. El job podría necesitar ser desprogramado si la rutina fue eliminada.", routineId);
                    // Opcionalmente, intentar desprogramar este job si la rutina no existe
                    // await context.Scheduler.UnscheduleJob(context.Trigger.Key); // Cuidado con esto, podría generar problemas si hay concurrencia.
                    return;
                }

                if(!routine.IsEnabled)
                {
                    _logger.LogInformation("Rutina {RoutineId} ({RoutineName}) no está habilitada. Omitiendo ejecución.", routineId, routine.Name);
                    // Quartz debería haber sido actualizado para no disparar jobs de rutinas deshabilitadas,
                    // pero esta es una doble verificación.
                    return;
                }

                if (routine.RoutineDevices == null || !routine.RoutineDevices.Any())
                {
                    _logger.LogInformation("Rutina {RoutineId} ({RoutineName}) no tiene acciones de dispositivo configuradas.", routineId, routine.Name);
                    return;
                }

                _logger.LogInformation("Procesando {ActionCount} acciones para la rutina '{RoutineName}' (ID: {RoutineId})", routine.RoutineDevices.Count, routine.Name, routineId);

                foreach (var routineDevice in routine.RoutineDevices)
                {
                    if (routineDevice.Device == null)
                    {
                        _logger.LogWarning("Dispositivo nulo para RoutineDevice Id {RoutineDeviceId} en Rutina {RoutineId}. Omitiendo esta acción.", routineDevice.RoutineDeviceId, routineId);
                        continue;
                    }

                    _logger.LogInformation("  Ejecutando acción '{ActionToPerform}' en dispositivo '{DeviceName}' (ID: {DeviceId}) para rutina '{RoutineName}'",
                        routineDevice.ActionToPerform, routineDevice.Device.Name, routineDevice.DeviceId, routine.Name);

                    string command;
                    string value = null;
                    var parts = routineDevice.ActionToPerform.Split(new[] { ':' }, 2); // Dividir máximo en 2 partes
                    command = parts[0];
                    if (parts.Length > 1)
                    {
                        value = parts[1];
                    }

                    try
                    {
                        await _deviceService.SendCommandAsync(routineDevice.DeviceId, command, value);
                        _logger.LogDebug("  Acción '{ActionToPerform}' enviada exitosamente al dispositivo {DeviceId} para rutina {RoutineId}.", routineDevice.ActionToPerform, routineDevice.DeviceId, routineId);
                    }
                    catch (Exception devEx)
                    {
                        _logger.LogError(devEx, "Error al ejecutar acción '{ActionToPerform}' en dispositivo {DeviceId} para rutina {RoutineId}.", routineDevice.ActionToPerform, routineDevice.DeviceId, routineId);
                    }
                    // Considerar si se necesita un delay entre acciones de una misma rutina.
                    // await Task.Delay(200); // Pequeña pausa entre comandos si es necesario
                }
                _logger.LogInformation("RoutineJob para RoutineId: {RoutineId} ({RoutineName}) completado.", routineId, routine.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico ejecutando RoutineJob para RoutineId: {RoutineId}.", routineId);
                // Considerar si el job debe ser reintentado por Quartz
                // throw new JobExecutionException(ex, refireImmediately: false); // No reintentar inmediatamente por defecto
            }
        }
    }
}
