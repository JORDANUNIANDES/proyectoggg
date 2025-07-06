using HomeAutomation.DataAccess.Entities;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers; // Para GroupMatcher
using System;
using System.Linq; // Para .Any()
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Scheduling
{
    public class SchedulingService : ISchedulingService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<SchedulingService> _logger;
        private IScheduler _scheduler;

        // Constantes para los nombres de grupo de Quartz
        private const string RoutineJobGroup = "RoutineJobs";
        private const string RoutineTriggerGroup = "RoutineTriggers";


        public SchedulingService(ISchedulerFactory schedulerFactory, ILogger<SchedulingService> logger)
        {
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InitializeAndStartSchedulerAsync()
        {
            if (_scheduler == null || _scheduler.IsShutdown)
            {
                _scheduler = await _schedulerFactory.GetScheduler();
                _logger.LogInformation("Scheduler de Quartz obtenido/creado.");
            }

            if (!_scheduler.IsStarted)
            {
                await _scheduler.Start();
                _logger.LogInformation("Scheduler de Quartz iniciado.");
            }
            else
            {
                _logger.LogInformation("Scheduler de Quartz ya estaba iniciado.");
            }
        }


        private async Task<IScheduler> GetSchedulerAsync()
        {
            // Asegura que el scheduler esté inicializado y arrancado.
            // AddQuartzHostedService se encarga de esto si se usa DI correctamente.
            // Esta llamada es más para obtener la instancia.
            if (_scheduler == null || _scheduler.IsShutdown)
            {
                 _scheduler = await _schedulerFactory.GetScheduler();
                 if (!_scheduler.IsStarted) // En caso de que no se use AddQuartzHostedService o falle
                 {
                    await _scheduler.Start();
                    _logger.LogInformation("Scheduler de Quartz iniciado explícitamente en GetSchedulerAsync.");
                 }
            }
            return _scheduler;
        }

        private JobKey GetJobKey(int routineId) => new JobKey($"RoutineJob-{routineId}", RoutineJobGroup);
        private TriggerKey GetTriggerKey(int routineId) => new TriggerKey($"RoutineTrigger-{routineId}", RoutineTriggerGroup);

        public async Task ScheduleRoutineAsync(Routine routine)
        {
            if (routine == null) throw new ArgumentNullException(nameof(routine));

            if (!routine.IsEnabled)
            {
                _logger.LogInformation("Rutina {RoutineId} ({RoutineName}) no está habilitada, no se programará.", routine.RoutineId, routine.Name);
                // Asegurarse que si existía una programación previa, se elimine.
                await UnscheduleRoutineAsync(routine.RoutineId);
                return;
            }
            if (routine.ExecutionDays == 0 && routine.ScheduledTime == TimeSpan.Zero) // O alguna otra condición de "no programar"
            {
                 _logger.LogInformation("Rutina {RoutineId} ({RoutineName}) no tiene días o tiempo de ejecución válidos, no se programará.", routine.RoutineId, routine.Name);
                 await UnscheduleRoutineAsync(routine.RoutineId);
                 return;
            }


            var scheduler = await GetSchedulerAsync();
            var jobKey = GetJobKey(routine.RoutineId);

            // Si el job ya existe, es mejor desprogramarlo y volverlo a programar para asegurar que el trigger es el correcto.
            if (await scheduler.CheckExists(jobKey))
            {
                _logger.LogInformation("Job para rutina {RoutineId} ({RoutineName}) ya existe. Desprogramando antes de reprogramar.", routine.RoutineId, routine.Name);
                await scheduler.DeleteJob(jobKey); // DeleteJob también elimina los triggers asociados
            }

            IJobDetail job = JobBuilder.Create<ExecuteRoutineJob>()
                .WithIdentity(jobKey)
                .UsingJobData("RoutineId", routine.RoutineId) // Pasar el ID de la rutina al Job
                .StoreDurably() // Permite que el job exista sin triggers, útil si se va a disparar manualmente también
                .Build();

            string cronExpression = ConvertDaysAndTimeToCron(routine.ExecutionDays, routine.ScheduledTime);
            if (string.IsNullOrEmpty(cronExpression))
            {
                _logger.LogWarning("No se pudo generar expresión CRON para rutina {RoutineId} ({RoutineName}). No se programará.", routine.RoutineId, routine.Name);
                return; // No programar si no hay CRON válido
            }

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(GetTriggerKey(routine.RoutineId))
                .ForJob(jobKey) // Asociar con el JobDetail
                .WithCronSchedule(cronExpression, x => x.InTimeZone(TimeZoneInfo.Local)) // Usar zona horaria local
                .Build();

            try
            {
                await scheduler.ScheduleJob(job, trigger);
                _logger.LogInformation("Rutina {RoutineId} ({RoutineName}) programada con CRON: {CronExpression}", routine.RoutineId, routine.Name, cronExpression);
            }
            catch(SchedulerException ex)
            {
                 _logger.LogError(ex, "Error de Quartz al programar la rutina {RoutineId} ({RoutineName}). CRON: {CronExpression}", routine.RoutineId, routine.Name, cronExpression);
                 // Podrías querer relanzar o manejar de otra forma
            }
        }

        public async Task UnscheduleRoutineAsync(int routineId)
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = GetJobKey(routineId);
            if (await scheduler.CheckExists(jobKey))
            {
                await scheduler.DeleteJob(jobKey); // Esto también elimina los triggers asociados
                _logger.LogInformation("Rutina ID: {RoutineId} desprogramada (job y triggers eliminados).", routineId);
            }
            else
            {
                _logger.LogInformation("Intento de desprogramar rutina ID: {RoutineId} que no tenía un job asociado.", routineId);
            }
        }

        public async Task UpdateScheduledRoutineAsync(Routine routine)
        {
            if (routine == null) throw new ArgumentNullException(nameof(routine));
            _logger.LogInformation("Actualizando programación para rutina ID: {RoutineId} ({RoutineName}). Habilitada: {IsEnabled}", routine.RoutineId, routine.Name, routine.IsEnabled);
            // Desprogramar la versión anterior (si existe) y programar la nueva si está habilitada.
            await UnscheduleRoutineAsync(routine.RoutineId);
            if (routine.IsEnabled)
            {
                await ScheduleRoutineAsync(routine); // ScheduleRoutineAsync ya maneja la lógica de no programar si no hay días/hora.
            }
        }
        public async Task TriggerRoutineNowAsync(int routineId)
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = GetJobKey(routineId);
            if (await scheduler.CheckExists(jobKey))
            {
                _logger.LogInformation("Disparando manualmente job para rutina ID: {RoutineId}", routineId);
                // No es necesario pasar JobDataMap aquí si el JobDetail ya lo tiene (StoreDurably)
                // y el job está diseñado para leerlo desde JobDetail.JobDataMap.
                // Si el job no es durable o necesitas pasar datos específicos para esta ejecución manual,
                // podrías crear un JobDataMap aquí:
                // var jobData = new JobDataMap();
                // jobData.Put("RoutineId", routineId);
                // jobData.Put("ManualTrigger", true); // Ejemplo de dato adicional
                // await scheduler.TriggerJob(jobKey, jobData);
                await scheduler.TriggerJob(jobKey); // Asume que el JobDetail tiene el RoutineId
            }
            else
            {
                _logger.LogWarning("Intento de disparar manualmente rutina ID: {RoutineId}, pero no se encontró el job programado. " +
                                   "Esto podría significar que la rutina fue deshabilitada, eliminada, o nunca se programó correctamente. " +
                                   "Considere cargar la rutina y ejecutar sus acciones directamente si este es un caso de uso válido.", routineId);
                // Opcional: Cargar la rutina y ejecutar sus acciones directamente si el job no existe
                // pero se quiere permitir "Ejecutar ahora" incluso si no está programada.
                // Esto requeriría inyectar IRoutineRepository y IDeviceService aquí.
                // Por simplicidad, este método asume que el job existe.
            }
        }

        private string ConvertDaysAndTimeToCron(DayOfWeek daysOfWeekFlags, TimeSpan time)
        {
            if (daysOfWeekFlags == 0) // Ningún día seleccionado
            {
                _logger.LogDebug("ConvertDaysAndTimeToCron: No se seleccionaron días para la rutina.");
                return null; // No se puede generar CRON si no hay días.
            }

            var cronDays = new List<string>();
            if (daysOfWeekFlags.HasFlag(DayOfWeek.Sunday)) cronDays.Add("SUN");
            if (daysOfWeekFlags.HasFlag(DayOfWeek.Monday)) cronDays.Add("MON");
            if (daysOfWeekFlags.HasFlag(DayOfWeek.Tuesday)) cronDays.Add("TUE");
            if (daysOfWeekFlags.HasFlag(DayOfWeek.Wednesday)) cronDays.Add("WED");
            if (daysOfWeekFlags.HasFlag(DayOfWeek.Thursday)) cronDays.Add("THU");
            if (daysOfWeekFlags.HasFlag(DayOfWeek.Friday)) cronDays.Add("FRI");
            if (daysOfWeekFlags.HasFlag(DayOfWeek.Saturday)) cronDays.Add("SAT");

            if (!cronDays.Any())
            {
                 _logger.LogDebug("ConvertDaysAndTimeToCron: La lista de días CRON está vacía aunque daysOfWeekFlags no era cero (esto no debería pasar).");
                return null;
            }

            string daysString = string.Join(",", cronDays);

            // Formato CRON: Segundos Minutos Horas DíaDelMes Mes DíaDeLaSemana [Año]
            // Para "todos los meses" y "todos los días del mes donde aplique el día de la semana", usamos "?" para DíaDelMes.
            return $"{time.Seconds} {time.Minutes} {time.Hours} ? * {daysString}";
        }
    }
}
