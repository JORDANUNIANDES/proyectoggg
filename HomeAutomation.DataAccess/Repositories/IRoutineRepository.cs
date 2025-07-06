using HomeAutomation.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAutomation.DataAccess.Repositories
{
    public interface IRoutineRepository
    {
        Task<Routine> GetByIdAsync(int routineId);
        Task<Routine> GetByIdWithDevicesAsync(int routineId); // Para cargar RoutineDevices y los Devices asociados
        Task<List<Routine>> GetAllAsync();
        Task<List<Routine>> GetRoutinesByUserIdAsync(int userId);
        Task AddAsync(Routine routine);
        Task UpdateAsync(Routine routine); // Podría necesitar manejar RoutineDevices
        Task DeleteAsync(int routineId);
    }
}
