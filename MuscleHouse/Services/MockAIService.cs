using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;

namespace MuscleHouse.Services
{
    public class MockAIService : IAIService
    {
        private readonly ApplicationDbContext _context;

        public MockAIService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> ChatAsync(string clientUserId, string message)
        {
            // 1. Fetch real client details to build a personalized response
            var client = await _context.Clientes
                .Include(c => c.Progresos)
                .Include(c => c.RegistrosEntrenamiento)
                .ThenInclude(r => r.Ejercicio)
                .Include(c => c.Rutinas)
                .ThenInclude(r => r.RutinaEjercicios)
                .ThenInclude(re => re.Ejercicio)
                .FirstOrDefaultAsync(c => c.UserId == clientUserId);

            if (client == null)
            {
                return "Hola, soy tu Asistente de IA de MUSCLE HOUSE. Lamentablemente, no pude encontrar tu perfil de cliente para darte recomendaciones personalizadas. ¿Eres un usuario registrado?";
            }

            var latestProgress = client.Progresos.OrderByDescending(p => p.Fecha).FirstOrDefault();
            var workoutLogs = client.RegistrosEntrenamiento.OrderByDescending(l => l.Fecha).Take(3).ToList();
            var activeRoutine = client.Rutinas.FirstOrDefault(r => r.Activa);

            var response = new StringBuilder();
            response.Append($"¡Hola, **{client.Nombre}**! 💪 Soy tu Asistente de IA de **MUSCLE HOUSE**.\n\n");
            response.Append($"Analizando tu perfil, veo que tu objetivo actual es: **\"{client.Objetivo}\"**.\n\n");

            // Contextualize based on progress measurements
            if (latestProgress != null)
            {
                response.Append($"### 📊 Tu Estado Físico Actual\n");
                response.Append($"- **Peso**: {latestProgress.Peso:F2} kg\n");
                response.Append($"- **Medidas**: Pecho {latestProgress.Pecho:F1} cm | Cintura {latestProgress.Cintura:F1} cm | Brazo {latestProgress.Brazo:F1} cm | Pierna {latestProgress.Pierna:F1} cm\n");
                response.Append($"- **Últimas observaciones**: {latestProgress.Observaciones}\n\n");
            }

            // Contextualize based on routine
            if (activeRoutine != null)
            {
                response.Append($"### 🏋️ Tu Rutina Activa: *{activeRoutine.Nombre}*\n");
                response.Append("Ejercicios que tienes asignados:\n");
                foreach (var re in activeRoutine.RutinaEjercicios.Take(3))
                {
                    response.Append($"- **{re.Ejercicio?.Nombre}**: {re.Series} series x {re.Repeticiones} reps (Peso sugerido: {re.PesoRecomendado:F1} kg)\n");
                }
                response.Append("\n");
            }

            // Contextualize based on training logs and progressive overload
            if (workoutLogs.Any())
            {
                response.Append($"### 📈 Análisis de Rendimiento y Cargas\n");
                response.Append("He revisado tus últimos registros de entrenamiento:\n");
                foreach (var log in workoutLogs)
                {
                    response.Append($"- En **{log.Ejercicio?.Nombre}** registraste {log.Series}x{log.Repeticiones} con **{log.Peso:F1} kg** (RPE: {log.RPE ?? 8}/10) el {log.Fecha:dd/MM/yyyy}.\n");
                }

                // Check for progressive overload (e.g. if there are multiple logs for the same exercise)
                if (workoutLogs.Count >= 2 && workoutLogs[0].EjercicioId == workoutLogs[1].EjercicioId)
                {
                    var diff = workoutLogs[0].Peso - workoutLogs[1].Peso;
                    if (diff > 0)
                    {
                        response.Append($"\n¡Excelente! Veo un incremento de **+{diff:F1} kg** en tu peso de **{workoutLogs[0].Ejercicio?.Nombre}**. Esto es un ejemplo perfecto de sobrecarga progresiva. ¡Sigue así!\n");
                    }
                }
                response.Append("\n");
            }

            // Generate specific tips according to objectives
            response.Append("### 💡 Recomendaciones de IA\n");
            if (client.Objetivo.Contains("Aumento", StringComparison.OrdinalIgnoreCase) || client.Objetivo.Contains("masa", StringComparison.OrdinalIgnoreCase))
            {
                response.Append("1. **Nutrición**: Asegúrate de estar en un superávit calórico controlado (300-500 kcal extra) y consumir suficiente proteína (aprox. 1.8g a 2.2g por kg).\n");
                response.Append("2. **Entrenamiento**: Mantén la intensidad alta. Si puedes completar las series de tu rutina con el peso sugerido manteniendo una técnica perfecta, sube la carga un 2-5% en la siguiente sesión.\n");
                response.Append("3. **Recuperación**: El músculo crece durante el descanso. Duerme entre 7 y 8 horas diarias de calidad.\n");
            }
            else if (client.Objetivo.Contains("Pérdida", StringComparison.OrdinalIgnoreCase) || client.Objetivo.Contains("bajar", StringComparison.OrdinalIgnoreCase) || client.Objetivo.Contains("Tonificación", StringComparison.OrdinalIgnoreCase))
            {
                response.Append("1. **Nutrición**: Prioriza un déficit calórico moderado de 300-500 kcal y mantén un consumo de proteína elevado para proteger la masa muscular magra.\n");
                response.Append("2. **Entrenamiento**: No descuides el entrenamiento de fuerza pesada, complementando con actividad cardiovascular de baja intensidad (LISS) después de entrenar o rutinas tipo HIIT en días alternos.\n");
                response.Append("3. **Consistencia**: Mantén un registro diario de tus comidas y sigue asistiendo regularmente a MUSCLE HOUSE.\n");
            }
            else
            {
                response.Append("1. **Equilibrio**: Enfócate en la regularidad de tus entrenamientos (mínimo 3-4 días por semana).\n");
                response.Append("2. **Variedad**: Alterna sesiones de fuerza con movilidad y acondicionamiento cardiovascular.\n");
            }

            response.Append("\n---\n*⚠️ Nota: Estas recomendaciones son generadas automáticamente basándose en tus datos de MUSCLE HOUSE. No sustituyen el asesoramiento médico o nutricional profesional.*");

            return response.ToString();
        }
    }
}
