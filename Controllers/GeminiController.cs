using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiClient _geminiService;

        public GeminiController(IGeminiClient geminiService) =>_geminiService = geminiService;

        [HttpPost("generatePrompt", Name = "GeneratePrompt")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Generador de respuesta usando Gemini IA, para la optimización de las respuestas.")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh([FromBody] string req, CancellationToken cancellation = default)
        {
            var result = await _geminiService.GenerateContentAsync(req, cancellation);
            var response = new ApiResponse<string>(
                data: result,
                message: "Respuesta optimizada generada coreectamenet."
            );
            return Ok(response);
        }
    }
}
