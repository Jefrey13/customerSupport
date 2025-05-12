using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using CustomerService.API.Utils;
using CustomerService.API.Services.Interfaces;

namespace CustomerService.API.Services.Implementations
{
    internal class GeminiClient : IGeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiClient> _logger;
        private readonly string _url;
        private readonly string _systemPrompt;
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public GeminiClient(
            HttpClient httpClient,
            ILogger<GeminiClient> logger,
            IOptions<GeminiOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _url = options.Value.Url;          // ruta relativa: "/v1beta/models/…"
            _systemPrompt = options.Value.SystemPrompt; // prompt desde configuración
        }

        public async Task<string> GenerateContentAsync(
            string systemContext,
            string userPrompt,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(systemContext))
                throw new ArgumentException("System context cannot be empty.", nameof(systemContext));
            if (string.IsNullOrWhiteSpace(userPrompt))
                throw new ArgumentException("User prompt cannot be empty.", nameof(userPrompt));

            _logger.LogInformation("Llamando a Gemini API…");

            var payload = new
            {
                model = "models/gemini-2.0-flash",
                contents = new[] {
    new {
      parts = new[] {
        new { text = systemContext },
        new { text = userPrompt    }
      }
    }
  },
                generationConfig = new
                {
                    temperature = 0,
                    top_p = 1,
                    top_k = 1,
                    candidate_count = 1
                }
            };


            var json = JsonConvert.SerializeObject(payload, Formatting.None, _serializerSettings);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Como BaseAddress ya es el host, aquí uso la ruta relativa
            var response = await _httpClient.PostAsync(_url, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API devolvió {StatusCode}: {Body}", response.StatusCode, body);
                throw new InvalidOperationException($"Gemini API error {response.StatusCode}");
            }

            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(body)
                                 ?? throw new InvalidOperationException("Empty response from Gemini.");

            var result = geminiResponse.Candidates?
                          .FirstOrDefault()?
                          .Content?
                          .Parts?
                          .FirstOrDefault()?
                          .Text
                          ?? string.Empty;

            _logger.LogInformation("Respuesta de Gemini recibida.");
            return result;
        }
    }
}