using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;

namespace MuscleHouse.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly ApplicationDbContext _context;

        public WorkoutService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Rutina>> GetClientRoutinesAsync(int clienteId)
        {
            return await _context.Rutinas
                .Include(r => r.RutinaEjercicios)
                .ThenInclude(re => re.Ejercicio)
                .Include(r => r.Entrenador)
                .Where(r => r.ClienteId == clienteId)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Rutina?> GetActiveRoutineAsync(int clienteId)
        {
            return await _context.Rutinas
                .Include(r => r.RutinaEjercicios)
                .ThenInclude(re => re.Ejercicio)
                .Include(r => r.Entrenador)
                .Where(r => r.ClienteId == clienteId && r.Activa)
                .FirstOrDefaultAsync();
        }

        public async Task<Rutina> CreateRoutineAsync(string trainerUserId, int clienteId, string name, List<RutinaEjercicio> exercises)
        {
            var trainer = await _context.Entrenadores.FirstOrDefaultAsync(e => e.UserId == trainerUserId);
            if (trainer == null)
            {
                throw new UnauthorizedAccessException("El usuario logueado no es un Entrenador registrado.");
            }

            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null || cliente.EntrenadorId != trainer.Id)
            {
                throw new UnauthorizedAccessException("Este Entrenador no tiene asignado este Cliente.");
            }

            // Deactivate previous active routines of the client
            var previousActive = await _context.Rutinas.Where(r => r.ClienteId == clienteId && r.Activa).ToListAsync();
            foreach (var oldR in previousActive)
            {
                oldR.Activa = false;
            }

            var nueva = new Rutina
            {
                ClienteId = clienteId,
                EntrenadorId = trainer.Id,
                Nombre = name,
                FechaCreacion = DateTime.Now,
                Activa = true,
                RutinaEjercicios = exercises
            };

            _context.Rutinas.Add(nueva);
            await _context.SaveChangesAsync();
            return nueva;
        }

        public async Task<Rutina> UpdateRoutineAsync(string trainerUserId, int routineId, string name, List<RutinaEjercicio> exercises)
        {
            var trainer = await _context.Entrenadores.FirstOrDefaultAsync(e => e.UserId == trainerUserId);
            if (trainer == null)
            {
                throw new UnauthorizedAccessException("El usuario logueado no es un Entrenador registrado.");
            }

            var rutina = await _context.Rutinas
                .Include(r => r.RutinaEjercicios)
                .FirstOrDefaultAsync(r => r.Id == routineId);

            if (rutina == null || rutina.EntrenadorId != trainer.Id)
            {
                throw new UnauthorizedAccessException("Este Entrenador no es dueño de la rutina especificada.");
            }

            rutina.Nombre = name;

            // Remove existing exercises and re-add updated ones
            _context.RutinaEjercicios.RemoveRange(rutina.RutinaEjercicios);
            rutina.RutinaEjercicios = exercises;

            await _context.SaveChangesAsync();
            return rutina;
        }

        public async Task<IEnumerable<Ejercicio>> GetAllExercisesAsync()
        {
            return await _context.Ejercicios.OrderBy(e => e.Nombre).ToListAsync();
        }

        public async Task<RegistroEntrenamiento> RegisterWorkoutLogAsync(int clienteId, int ejercicioId, int series, int reps, decimal weight, int? rpe, string observations)
        {
            var log = new RegistroEntrenamiento
            {
                ClienteId = clienteId,
                EjercicioId = ejercicioId,
                Fecha = DateTime.Now,
                Series = series,
                Repeticiones = reps,
                Peso = weight,
                RPE = rpe,
                Observaciones = observations
            };

            _context.RegistrosEntrenamiento.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<IEnumerable<RegistroEntrenamiento>> GetWorkoutLogsAsync(int clienteId)
        {
            return await _context.RegistrosEntrenamiento
                .Include(l => l.Ejercicio)
                .Where(l => l.ClienteId == clienteId)
                .OrderByDescending(l => l.Fecha)
                .ToListAsync();
        }
    }
}
