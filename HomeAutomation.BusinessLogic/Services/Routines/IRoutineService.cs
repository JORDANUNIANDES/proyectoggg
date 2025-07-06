using HomeAutomation.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Services.Routines
{
    public interface IRoutineService
    {
        Task<Routine> GetRoutineByIdAsync(int routineId);
        Task<List<Routine>> GetAllRoutinesAsync(); // Podría necesitar User/Role para filtrar
        Task<List<Routine>> GetRoutinesByUserIdAsync(int userId);
        Task<Routine> AddRoutineAsync(Routine routine, int userId); // Devuelve la rutina añadida con su ID
        Task UpdateRoutineAsync(Routine routine); // Podría necesitar User/Role para permisos
        Task DeleteRoutineAsync(int routineId); // Podría necesitar User/Role para permisos
        Task ExecuteRoutineAsync(int routineId); // Para ejecución manual
        Task ToggleRoutineEnabledAsync(int routineId, bool isEnabled);
    }
}
