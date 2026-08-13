using System;
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

        public async Task<string> ChatAsync(string clientUserId, string message)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                // API Key is absent -> Fallback to MockAIService
                return await _mockFallback.ChatAsync(clientUserId, message);
            }

            try
            {
                // Call OpenAI API Chat Completion endpoint
                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = "Eres un entrenador personal inteligente de primer nivel de MUSCLE HOUSE, un gimnasio moderno y deportivo con temática oscura y acento naranja. Escribe tus respuestas siempre en español de forma motivadora y profesional. El usuario te hará preguntas sobre rutinas, entrenamientos, nutrición o progreso." },
                        new { role = "user", content = message }
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
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return $"⚠️ Error de OpenAI API ({response.StatusCode}): {errorDetails}. Fallando al servicio local...\n\n" +
                        await _mockFallback.ChatAsync(clientUserId, message);
                }

                var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
                var content = responseJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                var result = new StringBuilder();
                result.Append(content);
                result.Append("\n\n---\n*⚠️ Nota: Estas recomendaciones son generadas por la IA de MUSCLE HOUSE. No sustituyen el asesoramiento médico o profesional.*");

                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"⚠️ Error de comunicación con la IA: {ex.Message}. Fallando al servicio local...\n\n" +
                    await _mockFallback.ChatAsync(clientUserId, message);
            }
        }
    }
}
