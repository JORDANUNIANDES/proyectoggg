using System.Collections.Generic;
using System.Threading.Tasks;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<Pago>> GetClientPaymentsAsync(int clienteId);
        Task<IEnumerable<Pago>> GetAllPaymentsAsync();
        Task<Pago> RegisterPaymentAsync(int clienteId, int membresiaId, decimal amount, string paymentMethod);
    }
}
