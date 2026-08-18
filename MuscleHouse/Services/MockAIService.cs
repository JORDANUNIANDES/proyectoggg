using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuscleHouse.Services
{
    public class MockAIService : IAIService
    {
        public Task<string> ChatAsync(AIUserContext? context, string message)
        {
            if (context == null)
            {
                return Task.FromResult("Hola, soy tu Asistente de IA de MUSCLE HOUSE. Lamentablemente, no pude encontrar tu perfil de cliente para darte recomendaciones personalizadas. ¿Eres un usuario registrado?");
            }

            var response = new StringBuilder();
            response.Append($"¡Hola, **{context.Nombre}**! 💪 Soy tu Asistente de IA de **MUSCLE HOUSE**.\n\n");
            response.Append($"Analizando tu perfil, veo que tu objetivo actual es: **\"{context.Objetivo}\"**.\n\n");

            // Contextualize based on progress measurements
            if (context.PesoActual.HasValue)
            {
                response.Append($"### 📊 Tu Estado Físico Actual\n");
                response.Append($"- **Peso**: {context.PesoActual.Value:F2} kg\n");
                response.Append($"- **Medidas**: Pecho {context.Pecho ?? 0:F1} cm | Cintura {context.Cintura ?? 0:F1} cm | Brazo {context.Brazo ?? 0:F1} cm | Pierna {context.Pierna ?? 0:F1} cm\n");
                if (!string.IsNullOrEmpty(context.UltimasObservacionesProgreso))
                {
                    response.Append($"- **Últimas observaciones**: {context.UltimasObservacionesProgreso}\n");
                }
                response.Append("\n");
            }

            // Contextualize based on routine
            if (!string.IsNullOrEmpty(context.NombreRutinaActiva) && context.EjerciciosRutina.Any())
            {
                response.Append($"### 🏋️ Tu Rutina Activa: *{context.NombreRutinaActiva}*\n");
                response.Append("Ejercicios que tienes asignados:\n");
                foreach (var re in context.EjerciciosRutina.Take(3))
                {
                    response.Append($"- **{re.NombreEjercicio}**: {re.Series} series x {re.Repeticiones} reps (Peso sugerido: {re.PesoRecomendado:F1} kg)\n");
                }
                response.Append("\n");
            }

            // Contextualize based on training logs and progressive overload
            if (context.UltimosRegistrosEntrenamiento.Any())
            {
                response.Append($"### 📈 Análisis de Rendimiento y Cargas\n");
                response.Append("He revisado tus últimos registros de entrenamiento:\n");
                foreach (var log in context.UltimosRegistrosEntrenamiento.Take(3))
                {
                    response.Append($"- En **{log.NombreEjercicio}** registraste {log.Series}x{log.Repeticiones} con **{log.Peso:F1} kg** (RPE: {log.RPE ?? 8}/10) el {log.Fecha:dd/MM/yyyy}.\n");
                }

                if (context.IncrementoSobrecargaProgresiva.HasValue && context.IncrementoSobrecargaProgresiva.Value > 0)
                {
                    response.Append($"\n¡Excelente! Veo un incremento de **+{context.IncrementoSobrecargaProgresiva.Value:F1} kg** en tus cargas recientes. Esto es un ejemplo perfecto de sobrecarga progresiva. ¡Sigue así!\n");
                }
                response.Append("\n");
            }

            // Generate specific tips according to objectives
            response.Append("### 💡 Recomendaciones de IA\n");
            if (context.Objetivo.Contains("Aumento", StringComparison.OrdinalIgnoreCase) || context.Objetivo.Contains("masa", StringComparison.OrdinalIgnoreCase) || context.Objetivo.Contains("Ganar", StringComparison.OrdinalIgnoreCase))
            {
                response.Append("1. **Nutrición**: Asegúrate de estar en un superávit calórico controlado (300-500 kcal extra) y consumir suficiente proteína (aprox. 1.8g a 2.2g por kg).\n");
                response.Append("2. **Entrenamiento**: Mantén la intensidad alta. Si puedes completar las series de tu rutina con el peso sugerido manteniendo una técnica perfecta, sube la carga un 2-5% en la siguiente sesión.\n");
                response.Append("3. **Recuperación**: El músculo crece durante el descanso. Duerme entre 7 y 8 horas diarias de calidad.\n");
            }
            else if (context.Objetivo.Contains("Pérdida", StringComparison.OrdinalIgnoreCase) || context.Objetivo.Contains("bajar", StringComparison.OrdinalIgnoreCase) || context.Objetivo.Contains("Tonificación", StringComparison.OrdinalIgnoreCase))
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

            return Task.FromResult(response.ToString());
        }
    }
}
