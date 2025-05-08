using CustomerService.API.Utils;
using Microsoft.Extensions.Options;

namespace CustomerService.API.Delegations
{
    internal sealed class GeminiDelegatingHandler(IOptions<GeminiOptions> geminiOptions)
    : DelegatingHandler
    {
        private readonly GeminiOptions _geminiOptions = geminiOptions.Value;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("x-goog-api-key", $"{_geminiOptions.ApiKey}");

            return base.SendAsync(request, cancellationToken);
        }
    }
}
