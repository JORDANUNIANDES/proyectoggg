using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Asistencia>> GetClientAttendanceAsync(int clienteId)
        {
            return await _context.Asistencias
                .Where(a => a.ClienteId == clienteId)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asistencia>> GetAllAttendanceAsync()
        {
            return await _context.Asistencias
                .Include(a => a.Cliente)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();
        }

        public async Task<Asistencia> RegisterAttendanceAsync(int clienteId)
        {
            var asistencia = new Asistencia
            {
                ClienteId = clienteId,
                FechaHora = DateTime.Now
            };

            _context.Asistencias.Add(asistencia);
            await _context.SaveChangesAsync();
            return asistencia;
        }
    }
}
