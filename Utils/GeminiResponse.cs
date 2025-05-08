namespace CustomerService.API.Utils
{
    internal sealed class GeminiResponse
    {
        public Candidate[] Candidates { get; set; }
        public PromptFeedback PromptFeedback { get; set; }
    }
}
