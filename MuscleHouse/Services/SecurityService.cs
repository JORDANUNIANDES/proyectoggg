using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;

namespace MuscleHouse.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ApplicationDbContext _context;

        public SecurityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanClientAccessAsync(string loggedInUserId, int targetClienteId)
        {
            if (string.IsNullOrEmpty(loggedInUserId)) return false;

            // Retrieve the client and check if the UserId matches the loggedInUserId
            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == targetClienteId);

            return cliente != null && cliente.UserId == loggedInUserId;
        }

        public async Task<bool> CanTrainerAccessAsync(string loggedInUserId, int targetClienteId)
        {
            if (string.IsNullOrEmpty(loggedInUserId)) return false;

            // Find the trainer entity associated with the loggedInUserId
            var trainer = await _context.Entrenadores
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == loggedInUserId);

            if (trainer == null) return false;

            // Retrieve the client and check if they are currently assigned to this trainer
            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == targetClienteId);

            return cliente != null && cliente.EntrenadorId == trainer.Id;
        }

        public async Task<bool> CanTrainerAccessRutinaAsync(string loggedInUserId, int targetRutinaId)
        {
            if (string.IsNullOrEmpty(loggedInUserId)) return false;

            // Find the trainer entity associated with the loggedInUserId
            var trainer = await _context.Entrenadores
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == loggedInUserId);

            if (trainer == null) return false;

            // Find the Routine and ensure it was created by this trainer
            var rutina = await _context.Rutinas
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == targetRutinaId);

            return rutina != null && rutina.EntrenadorId == trainer.Id;
        }
    }
}
