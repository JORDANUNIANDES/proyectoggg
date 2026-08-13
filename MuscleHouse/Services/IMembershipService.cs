using System.Collections.Generic;
using System.Threading.Tasks;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public interface IMembershipService
    {
        Task<IEnumerable<Membresia>> GetClientMembershipsAsync(int clienteId);
        Task<Membresia?> GetActiveMembershipAsync(int clienteId);
        Task<Membresia> CreateMembershipAsync(int clienteId, int planId, int durationDays, decimal pricePaid);
        Task<Membresia> RenewMembershipAsync(int clienteId, int planId, int durationDays, decimal pricePaid);
        Task CheckAndExpireMembershipsAsync();
    }
}
