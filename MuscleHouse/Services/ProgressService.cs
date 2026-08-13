using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public class ProgressService : IProgressService
    {
        private readonly ApplicationDbContext _context;

        public ProgressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Progreso>> GetClientProgressHistoryAsync(int clienteId)
        {
            return await _context.Progresos
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        public async Task<Progreso> RegisterProgressAsync(int clienteId, decimal weight, decimal chest, decimal waist, decimal arm, decimal leg, decimal hip, string observations)
        {
            var progreso = new Progreso
            {
                ClienteId = clienteId,
                Fecha = DateTime.Now,
                Peso = weight,
                Pecho = chest,
                Cintura = waist,
                Brazo = arm,
                Pierna = leg,
                Cadera = hip,
                Observaciones = observations
            };

            _context.Progresos.Add(progreso);
            await _context.SaveChangesAsync();
            return progreso;
        }
    }
}
