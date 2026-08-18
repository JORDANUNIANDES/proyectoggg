using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;

namespace MuscleHouse.Services
{
    public class AIContextService : IAIContextService
    {
        private readonly ApplicationDbContext _dbContext;

        public AIContextService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AIUserContext?> BuildClientContextAsync(string clientUserId)
        {
            if (string.IsNullOrWhiteSpace(clientUserId))
            {
                return null;
            }

            var client = await _dbContext.Clientes
                .Include(c => c.Progresos)
                .Include(c => c.RegistrosEntrenamiento)
                .ThenInclude(r => r.Ejercicio)
                .Include(c => c.Rutinas)
                .ThenInclude(r => r.RutinaEjercicios)
                .ThenInclude(re => re.Ejercicio)
                .FirstOrDefaultAsync(c => c.UserId == clientUserId);

            if (client == null)
            {
                return null;
            }

            var latestProgress = client.Progresos.OrderByDescending(p => p.Fecha).FirstOrDefault();
            var workoutLogs = client.RegistrosEntrenamiento.OrderByDescending(l => l.Fecha).Take(5).ToList();
            var activeRoutine = client.Rutinas.FirstOrDefault(r => r.Activa);

            decimal? progressiveOverloadDiff = null;
            if (workoutLogs.Count >= 2 && workoutLogs[0].EjercicioId == workoutLogs[1].EjercicioId)
            {
                progressiveOverloadDiff = workoutLogs[0].Peso - workoutLogs[1].Peso;
            }

            var context = new AIUserContext
            {
                ClienteId = client.Id,
                UserId = client.UserId,
                Nombre = client.Nombre,
                Objetivo = client.Objetivo ?? "General",
                PesoActual = latestProgress?.Peso,
                Pecho = latestProgress?.Pecho,
                Cintura = latestProgress?.Cintura,
                Brazo = latestProgress?.Brazo,
                Pierna = latestProgress?.Pierna,
                Cadera = latestProgress?.Cadera,
                UltimasObservacionesProgreso = latestProgress?.Observaciones,
                FechaUltimoProgreso = latestProgress?.Fecha,
                NombreRutinaActiva = activeRoutine?.Nombre,
                IncrementoSobrecargaProgresiva = progressiveOverloadDiff
            };

            if (activeRoutine != null && activeRoutine.RutinaEjercicios != null)
            {
                context.EjerciciosRutina = activeRoutine.RutinaEjercicios.Select(re => new AIRoutineExercise
                {
                    NombreEjercicio = re.Ejercicio?.Nombre ?? "Ejercicio",
                    GrupoMuscular = re.Ejercicio?.GrupoMuscular ?? "General",
                    Series = re.Series,
                    Repeticiones = re.Repeticiones,
                    PesoRecomendado = re.PesoRecomendado,
                    DescansoSegundos = re.DescansoSegundos
                }).ToList();
            }

            if (workoutLogs.Any())
            {
                context.UltimosRegistrosEntrenamiento = workoutLogs.Select(log => new AIWorkoutLog
                {
                    NombreEjercicio = log.Ejercicio?.Nombre ?? "Ejercicio",
                    Series = log.Series,
                    Repeticiones = log.Repeticiones,
                    Peso = log.Peso,
                    RPE = log.RPE,
                    Fecha = log.Fecha
                }).ToList();
            }

            return context;
        }
    }
}
