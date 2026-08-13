using System.Threading.Tasks;

namespace MuscleHouse.Services
{
    public interface IAIService
    {
        Task<string> ChatAsync(string clientUserId, string message);
    }
}
