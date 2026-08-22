using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pago>> GetClientPaymentsAsync(int clienteId)
        {
            return await _context.Pagos
                .Include(p => p.Membresia)
                .ThenInclude(m => m!.Plan)
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetAllPaymentsAsync()
        {
            return await _context.Pagos
                .Include(p => p.Cliente)
                .Include(p => p.Membresia)
                .ThenInclude(m => m!.Plan)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        public async Task<Pago> RegisterPaymentAsync(int clienteId, int membresiaId, decimal amount, string paymentMethod)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("El monto a pagar debe ser mayor a cero.");
            }

            var pago = new Pago
            {
                ClienteId = clienteId,
                MembresiaId = membresiaId,
                Monto = amount,
                Fecha = DateTime.Now,
                MetodoPago = paymentMethod,
                Estado = "Completado"
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
            return pago;
        }
    }
}
