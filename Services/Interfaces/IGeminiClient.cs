namespace CustomerService.API.Services.Interfaces
{
    /// <summary>
    /// Defines contract for generating content using Google Gemini LLM.
    /// </summary>
    public interface IGeminiClient
    {
        /// <summary>
        /// Generates a response based on provided system context and user prompt.
        /// </summary>
        /// <param name="systemContext">Instructional context for the system role.</param>
        /// <param name="userPrompt">Actual text from the user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Generated text from Gemini.</returns>
        Task<string> GenerateContentAsync(
            string systemContext,
            string userPrompt,
            CancellationToken cancellationToken);
    }
}