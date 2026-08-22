using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly ApplicationDbContext _context;

        public MembershipService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Membresia>> GetClientMembershipsAsync(int clienteId)
        {
            return await _context.Membresias
                .Include(m => m.Plan)
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.FechaInicio)
                .ToListAsync();
        }

        public async Task<Membresia?> GetActiveMembershipAsync(int clienteId)
        {
            await CheckAndExpireMembershipsAsync();

            return await _context.Membresias
                .Include(m => m.Plan)
                .Where(m => m.ClienteId == clienteId && m.Estado == "Activa")
                .OrderByDescending(m => m.FechaVencimiento)
                .FirstOrDefaultAsync();
        }

        public async Task<Membresia> CreateMembershipAsync(int clienteId, int planId, int durationDays, decimal pricePaid)
        {
            var active = await GetActiveMembershipAsync(clienteId);
            DateTime startDate = DateTime.Today;

            // If there's an active membership, the renewed one starts when the active one expires
            if (active != null)
            {
                startDate = active.FechaVencimiento;
            }

            var nueva = new Membresia
            {
                ClienteId = clienteId,
                PlanId = planId,
                FechaInicio = startDate,
                FechaVencimiento = startDate.AddDays(durationDays),
                PrecioPagado = pricePaid,
                Estado = "Activa"
            };

            _context.Membresias.Add(nueva);
            await _context.SaveChangesAsync();
            return nueva;
        }

        public async Task<Membresia> RenewMembershipAsync(int clienteId, int planId, int durationDays, decimal pricePaid)
        {
            // Renews is logically identical to creating/extending a membership without overwriting historical ones
            return await CreateMembershipAsync(clienteId, planId, durationDays, pricePaid);
        }

        public async Task CheckAndExpireMembershipsAsync()
        {
            var activeExpiring = await _context.Membresias
                .Where(m => m.Estado == "Activa" && m.FechaVencimiento < DateTime.Today)
                .ToListAsync();

            if (activeExpiring.Any())
            {
                foreach (var m in activeExpiring)
                {
                    m.Estado = "Vencida";
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
