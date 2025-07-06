using HomeAutomation.DataAccess.Entities;
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Scheduling
{
    public interface ISchedulingService
    {
        Task ScheduleRoutineAsync(Routine routine);
        Task UnscheduleRoutineAsync(int routineId);
        Task UpdateScheduledRoutineAsync(Routine routine);
        Task TriggerRoutineNowAsync(int routineId);
        Task InitializeAndStartSchedulerAsync(); // Para iniciar el scheduler al arrancar la app
    }
}
