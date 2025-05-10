using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using CustomerService.API.Utils;

namespace CustomerService.API.Delegations
{
    internal class GeminiDelegatingHandler : DelegatingHandler
    {
        private readonly string _apiKey;

        public GeminiDelegatingHandler(IOptions<GeminiOptions> options)
        {
            _apiKey = options.Value.ApiKey;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Inyecta la API key en el header
            request.Headers.Add("x-goog-api-key", _apiKey);
            return base.SendAsync(request, cancellationToken);
        }
    }
}