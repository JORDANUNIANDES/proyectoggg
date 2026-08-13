using System.Collections.Generic;
using System.Threading.Tasks;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public interface IProgressService
    {
        Task<IEnumerable<Progreso>> GetClientProgressHistoryAsync(int clienteId);
        Task<Progreso> RegisterProgressAsync(int clienteId, decimal weight, decimal chest, decimal waist, decimal arm, decimal leg, decimal hip, string observations);
    }
}
