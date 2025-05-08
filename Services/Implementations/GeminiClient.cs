using System.Text;
using System.Text.Json;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using EllipticCurve;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace CustomerService.API.Services.Implementations
{
    internal sealed class GeminiClient:IGeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiClient> _logger;
        private readonly string _apikey;

        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public GeminiClient(HttpClient httpClient, ILogger<GeminiClient> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apikey = config["Gemini:ApiKey"!];
        }

        public async Task<string?> GenerateContentAsync(string prompt, CancellationToken cancellationToken)
        {
            var requestBody = GeminiRequestFactory.CreateRequest(prompt);
            var content = new StringContent(JsonConvert.SerializeObject(requestBody, Formatting.None, _serializerSettings), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apikey}", content, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseBody);

            var geminiResponseText = geminiResponse?.Candidates[0].Content.Parts[0].Text;

            return geminiResponseText;
        }
    }
}