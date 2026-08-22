using System.Collections.Generic;
using System.Threading.Tasks;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public interface IStaffService
    {
        Task<IEnumerable<Entrenador>> GetAllTrainersAsync();
        Task<IEnumerable<Recepcionista>> GetAllReceptionistsAsync();
        Task<Entrenador?> GetTrainerByUserIdAsync(string userId);
        Task<Recepcionista?> GetRecepcionistaByUserIdAsync(string userId);
        Task<bool> AssignTrainerToClientAsync(int clienteId, int trainerId);
        Task<Cliente?> GetClienteByIdAsync(int id);
        Task<Cliente?> GetClienteByUserIdAsync(string userId);
        Task<IEnumerable<Cliente>> GetAssignedClientsAsync(string trainerUserId);
    }
}
