using System.Threading.Tasks;

namespace MuscleHouse.Services
{
    public interface IAIContextService
    {
        Task<AIUserContext?> BuildClientContextAsync(string clientUserId);
    }
}
