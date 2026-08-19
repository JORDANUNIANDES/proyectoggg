using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MuscleHouse.Services
{
    public class OpenAIAIService : IAIService
    {
        private readonly string? _apiKey;
        private readonly HttpClient _httpClient;
        private readonly MockAIService _mockFallback;

        public OpenAIAIService(IConfiguration configuration, HttpClient httpClient, MockAIService mockFallback)
        {
            _httpClient = httpClient;
            _mockFallback = mockFallback;

            // Priority: Environment Variable > AppSettings Configuration
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                ?? configuration["OpenAI:ApiKey"];
        }

        public async Task<string> ChatAsync(AIUserContext? context, string message)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                // API Key is absent -> Fallback to MockAIService
                return await _mockFallback.ChatAsync(context, message);
            }

            try
            {
                // Build client context text
                var contextText = BuildContextText(context);

                // Call OpenAI API Chat Completion endpoint with model gpt-4o-mini
                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new object[]
                    {
                        new
                        {
                            role = "system",
                            content = "Eres el Asistente IA oficial de MUSCLE HOUSE, un gimnasio moderno y deportivo con estética oscura. Tu trabajo es dar soporte nutricional, consejos de entrenamiento y motivación basados en los datos del cliente. Responde siempre en español de forma profesional y empática. Utiliza el CONTEXTO DEL CLIENTE proporcionado. Si no hay datos sobre un tema, indícalo amablemente sin inventar datos. Recuerda que no puedes modificar directamente las rutinas o la base de datos."
                        },
                        new
                        {
                            role = "system",
                            content = $"CONTEXTO DEL CLIENTE DE MUSCLE HOUSE:\n{contextText}"
                        },
                        new
                        {
                            role = "user",
                            content = message
                        }
                    },
                    temperature = 0.7
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
                {
                    Content = JsonContent.Create(requestBody)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return "⚠️ Error de comunicación con los servidores de OpenAI. Mostrando recomendación de MUSCLE HOUSE local...\n\n" +
                        await _mockFallback.ChatAsync(context, message);
                }

                var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (responseJson.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("message", out var msgObj) && msgObj.TryGetProperty("content", out var contentElement))
                    {
                        var content = contentElement.GetString() ?? string.Empty;
                        var result = new StringBuilder();
                        result.Append(content);
                        result.Append("\n\n---\n*⚠️ Nota: Estas recomendaciones son generadas por la IA de MUSCLE HOUSE. No sustituyen el asesoramiento médico o profesional.*");

                        return result.ToString();
                    }
                }

                return await _mockFallback.ChatAsync(context, message);
            }
            catch (Exception)
            {
                return "⚠️ No se pudo conectar con los servidores de OpenAI. Mostrando recomendación de MUSCLE HOUSE local...\n\n" +
                    await _mockFallback.ChatAsync(context, message);
            }
        }

        private string BuildContextText(AIUserContext? context)
        {
            if (context == null)
            {
                return "Cliente no registrado o sin perfil de datos disponible.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"- Nombre: {context.Nombre}");
            sb.AppendLine($"- Objetivo Fitness: {context.Objetivo}");

            if (context.PesoActual.HasValue)
            {
                sb.AppendLine($"- Peso Actual: {context.PesoActual.Value:F2} kg");
                sb.AppendLine($"- Medidas: Pecho {context.Pecho ?? 0:F1} cm, Cintura {context.Cintura ?? 0:F1} cm, Brazo {context.Brazo ?? 0:F1} cm, Pierna {context.Pierna ?? 0:F1} cm");
            }
            else
            {
                sb.AppendLine("- Registro Físico: No disponible");
            }

            if (!string.IsNullOrEmpty(context.NombreRutinaActiva) && context.EjerciciosRutina.Any())
            {
                sb.AppendLine($"- Rutina Activa: {context.NombreRutinaActiva}");
                sb.AppendLine("  Ejercicios Asignados:");
                foreach (var ex in context.EjerciciosRutina)
                {
                    sb.AppendLine($"    • {ex.NombreEjercicio} ({ex.GrupoMuscular}): {ex.Series}x{ex.Repeticiones} @ {ex.PesoRecomendado:F1} kg");
                }
            }
            else
            {
                sb.AppendLine("- Rutina Activa: Sin rutina asignada actualmente");
            }

            if (context.UltimosRegistrosEntrenamiento.Any())
            {
                sb.AppendLine("  Últimos Registros de Rendimiento:");
                foreach (var log in context.UltimosRegistrosEntrenamiento.Take(3))
                {
                    sb.AppendLine($"    • {log.NombreEjercicio}: {log.Series}x{log.Repeticiones} con {log.Peso:F1} kg (RPE {log.RPE ?? 8}/10) el {log.Fecha:dd/MM/yyyy}");
                }
            }

            return sb.ToString();
        }
    }
}
