using System.Threading.Tasks;

namespace MuscleHouse.Services
{
    public interface ISecurityService
    {
        Task<bool> CanClientAccessAsync(string loggedInUserId, int targetClienteId);
        Task<bool> CanTrainerAccessAsync(string loggedInUserId, int targetClienteId);
        Task<bool> CanTrainerAccessRutinaAsync(string loggedInUserId, int targetRutinaId);
    }
}
