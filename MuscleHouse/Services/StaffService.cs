using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;

        public StaffService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Entrenador>> GetAllTrainersAsync()
        {
            return await _context.Entrenadores
                .Include(e => e.User)
                .Where(e => e.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recepcionista>> GetAllReceptionistsAsync()
        {
            return await _context.Recepcionistas
                .Include(r => r.User)
                .Where(r => r.Activo)
                .ToListAsync();
        }

        public async Task<Entrenador?> GetTrainerByUserIdAsync(string userId)
        {
            return await _context.Entrenadores
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<Recepcionista?> GetRecepcionistaByUserIdAsync(string userId)
        {
            return await _context.Recepcionistas
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);
        }

        public async Task<Cliente?> GetClienteByIdAsync(int id)
        {
            return await _context.Clientes
                .Include(c => c.User)
                .Include(c => c.Entrenador)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cliente?> GetClienteByUserIdAsync(string userId)
        {
            return await _context.Clientes
                .Include(c => c.User)
                .Include(c => c.Entrenador)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<IEnumerable<Cliente>> GetAssignedClientsAsync(string trainerUserId)
        {
            var trainer = await GetTrainerByUserIdAsync(trainerUserId);
            if (trainer == null) return Enumerable.Empty<Cliente>();

            return await _context.Clientes
                .Include(c => c.User)
                .Where(c => c.EntrenadorId == trainer.Id && c.Activo)
                .ToListAsync();
        }

        public async Task<bool> AssignTrainerToClientAsync(int clienteId, int trainerId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null) return false;

            var trainer = await _context.Entrenadores.FindAsync(trainerId);
            if (trainer == null) return false;

            // If the trainer is already assigned and active, do nothing
            if (cliente.EntrenadorId == trainerId) return true;

            // 1. Close previous assignment
            var activeAssignments = await _context.AsignacionesEntrenadores
                .Where(ae => ae.ClienteId == clienteId && ae.FechaFin == null)
                .ToListAsync();

            foreach (var oldAsg in activeAssignments)
            {
                oldAsg.FechaFin = DateTime.Now;
            }

            // 2. Create new historical assignment
            var newAsg = new AsignacionEntrenador
            {
                ClienteId = clienteId,
                EntrenadorId = trainerId,
                FechaInicio = DateTime.Now,
                FechaFin = null
            };
            _context.AsignacionesEntrenadores.Add(newAsg);

            // 3. Update client current trainer
            cliente.EntrenadorId = trainerId;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
