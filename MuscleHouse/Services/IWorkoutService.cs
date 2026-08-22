using System.Collections.Generic;
using System.Threading.Tasks;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public interface IWorkoutService
    {
        Task<IEnumerable<Rutina>> GetClientRoutinesAsync(int clienteId);
        Task<Rutina?> GetActiveRoutineAsync(int clienteId);
        Task<Rutina> CreateRoutineAsync(string trainerUserId, int clienteId, string name, List<RutinaEjercicio> exercises);
        Task<Rutina> UpdateRoutineAsync(string trainerUserId, int routineId, string name, List<RutinaEjercicio> exercises);
        Task<IEnumerable<Ejercicio>> GetAllExercisesAsync();
        Task<RegistroEntrenamiento> RegisterWorkoutLogAsync(int clienteId, int ejercicioId, int series, int reps, decimal weight, int? rpe, string observations);
        Task<IEnumerable<RegistroEntrenamiento>> GetWorkoutLogsAsync(int clienteId);
    }
}
