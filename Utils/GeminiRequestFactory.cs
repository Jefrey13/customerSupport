using System;
using System.Collections.Generic;

namespace CustomerService.API.Utils
{
    /// <summary>
    /// Factory for constructing GeminiRequest objects
    /// with a predefined system context and user prompt.
    /// </summary>
    internal static class GeminiRequestFactory
    {
        /// <summary>
        /// Creates a GeminiRequest with both system and user messages,
        /// default generation settings, and strict safety filters.
        /// </summary>
        /// <param name="systemContext">
        /// Instructional context describing company identity,
        /// required client details, and response style guidelines.
        /// </param>
        /// <param name="userPrompt">
        /// The raw text input from the user to be processed.
        /// </param>
        /// <returns>A fully configured <see cref="GeminiRequest"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="userPrompt"/> is null or whitespace.
        /// </exception>
        public static GeminiRequest CreateRequest(
            string systemContext,
            string userPrompt)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
                throw new ArgumentException(
                    "The user prompt cannot be empty.",
                    nameof(userPrompt));

            // Build the conversation history: system context + user message
            var contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Role = "system",
                    Parts = new[]
                    {
                        new GeminiPart { Text = systemContext ?? string.Empty }
                    }
                },
                new GeminiContent
                {
                    Role = "user",
                    Parts = new[]
                    {
                        new GeminiPart { Text = userPrompt }
                    }
                }
            };

            // Configure generation parameters
            var generationConfig = new GenerationConfig
            {
                Temperature = 0,
                TopK = 1,
                TopP = 1,
                MaxOutputTokens = 2048,
                StopSequences = new List<object>()
            };

            // Define strict safety settings
            var safetySettings = new[]
            {
                new SafetySettings
                {
                    Category = "HARM_CATEGORY_HARASSMENT",
                    Threshold = "BLOCK_ONLY_HIGH"
                },
                new SafetySettings
                {
                    Category = "HARM_CATEGORY_HATE_SPEECH",
                    Threshold = "BLOCK_ONLY_HIGH"
                },
                new SafetySettings
                {
                    Category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                    Threshold = "BLOCK_ONLY_HIGH"
                },
                new SafetySettings
                {
                    Category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                    Threshold = "BLOCK_ONLY_HIGH"
                }
            };

            return new GeminiRequest
            {
                Contents = contents.ToArray(),
                GenerationConfig = generationConfig,
                SafetySettings = safetySettings
            };
        }
    }
}