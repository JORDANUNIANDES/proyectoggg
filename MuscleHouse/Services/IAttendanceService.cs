using System.Collections.Generic;
using System.Threading.Tasks;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Asistencia>> GetClientAttendanceAsync(int clienteId);
        Task<IEnumerable<Asistencia>> GetAllAttendanceAsync();
        Task<Asistencia> RegisterAttendanceAsync(int clienteId);
    }
}
