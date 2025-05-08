namespace CustomerService.API.Services.Interfaces
{
    public interface IGeminiClient
    {
        Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken);
    }
}
