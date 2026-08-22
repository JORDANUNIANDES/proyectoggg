using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MuscleHouse.Services
{
    public class MockAIService : IAIService
    {
        public Task<string> ChatAsync(AIUserContext? context, string message)
        {
            if (context == null)
            {
                return Task.FromResult("Lamentablemente, no pude encontrar tu perfil de cliente para darte recomendaciones personalizadas. ¿Eres un usuario registrado en MUSCLE HOUSE?");
            }

            string trimmedMsg = message.Trim().ToLower();
            bool hasGreeting = Regex.IsMatch(trimmedMsg, @"\b(hola|buenas|buenos días|buenas tardes|buenas noches|hey|saludos)\b", RegexOptions.IgnoreCase);
            bool isPureGreeting = hasGreeting && (trimmedMsg.Length <= 18 || Regex.IsMatch(trimmedMsg, @"^(hola|buenas|buenos días|buenas tardes|buenas noches|hey|saludos)\s*[\!\?\. ]*$", RegexOptions.IgnoreCase));

            var response = new StringBuilder();

            // Scenario 1: Pure greeting from user
            if (isPureGreeting)
            {
                response.Append($"¡Hola, **{context.Nombre}**! 💪 ¿En qué te puedo apoyar hoy con tu entrenamiento en **MUSCLE HOUSE**?");
                response.Append("\n\n---\n*⚠️ Nota: Estas recomendaciones son generadas automáticamente basándose en tus datos de MUSCLE HOUSE.*");
                return Task.FromResult(response.ToString());
            }

            // Scenario 2: User greeted AND asked a question
            if (hasGreeting)
            {
                response.Append($"¡Hola, **{context.Nombre}**! ");
            }

            // Scenario 3: Pure question/request -> No greeting header!
            // Build direct answer based on user query
            if (trimmedMsg.Contains("pecho") || trimmedMsg.Contains("pectoral"))
            {
                response.Append("Para trabajar el pecho de forma efectiva en **MUSCLE HOUSE**, te recomiendo:\n\n");
                response.Append("1. **Press de Banca con Barra**: El ejercicio rey para sobrecarga progresiva en empuje horizontal.\n");
                response.Append("2. **Press Inclinado con Mancuernas**: Enfocado en la porción superior del pectoral.\n");
                response.Append("3. **Fondos en Paralelas / Aperturas con Polea**: Excelentes para aislamiento y estiramiento en tensión constante.\n\n");
            }
            else if (trimmedMsg.Contains("pierna") || trimmedMsg.Contains("piernas") || trimmedMsg.Contains("cuádriceps"))
            {
                response.Append("Para el desarrollo de piernas en **MUSCLE HOUSE**, la frecuencia óptima suele ser de **2 veces por semana** (frecuencia 2) para maximizar la síntesis proteica muscular.\n\n");
                response.Append("Ejercicios clave:\n");
                response.Append("- **Sentadilla con Barra**: 3-4 series x 6-8 reps.\n");
                response.Append("- **Prensa de Piernas / Prensa 45°**: 3 series x 10-12 reps.\n");
                response.Append("- **Peso Muerto Rumano / Prensa Femoral**: Para la cadena posterior.\n\n");
            }
            else
            {
                response.Append($"Analizando tu perfil para tu objetivo de **\"{context.Objetivo}\"**:\n\n");
            }

            // Append physical state context if relevant
            if (context.PesoActual.HasValue && !isPureGreeting)
            {
                response.Append($"### 📊 Estado Físico & Progreso\n");
                response.Append($"- **Peso**: {context.PesoActual.Value:F2} kg | **Medidas**: Pecho {context.Pecho ?? 0:F1} cm, Cintura {context.Cintura ?? 0:F1} cm, Brazo {context.Brazo ?? 0:F1} cm\n");
                response.Append("\n");
            }

            // Append active routine context if available
            if (!string.IsNullOrEmpty(context.NombreRutinaActiva) && context.EjerciciosRutina.Any())
            {
                response.Append($"### 🏋️ Rutina Activa: *{context.NombreRutinaActiva}*\n");
                foreach (var re in context.EjerciciosRutina.Take(3))
                {
                    response.Append($"- **{re.NombreEjercicio}**: {re.Series}x{re.Repeticiones} @ {re.PesoRecomendado:F1} kg\n");
                }
                response.Append("\n");
            }

            // Specific tips
            response.Append("### 💡 Recomendaciones de Entrenamiento\n");
            if (context.Objetivo.Contains("Aumento", StringComparison.OrdinalIgnoreCase) || context.Objetivo.Contains("Ganar", StringComparison.OrdinalIgnoreCase))
            {
                response.Append("1. Mantén la intensidad alta en el rango de 6 a 12 repeticiones.\n");
                response.Append("2. Aplica sobrecarga progresiva incrementando 1-2 kg cuando domines las repeticiones indicadas.\n");
            }
            else
            {
                response.Append("1. Mantén una alta densidad de entrenamiento y suficiente proteína en tu alimentación.\n");
                response.Append("2. Descansa entre 60 y 90 segundos entre series pesadas.\n");
            }

            response.Append("\n---\n*⚠️ Nota: Estas recomendaciones son generadas automáticamente basándose en tus datos de MUSCLE HOUSE. No sustituyen el asesoramiento médico o nutricional profesional.*");

            return Task.FromResult(response.ToString());
        }
    }
}
