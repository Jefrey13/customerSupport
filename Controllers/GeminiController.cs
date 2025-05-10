using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiClient _geminiService;
        private readonly string _systemPrompt;

        public GeminiController(
            IGeminiClient geminiService,
            IConfiguration config)
        {
            _geminiService = geminiService;
            _systemPrompt = config["Gemini:SystemPrompt"]
                ?? throw new ArgumentException("Gemini:SystemPrompt is missing in configuration.");
        }

        [HttpPost("generatePrompt", Name = "GeneratePrompt")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Generador de respuesta usando Gemini IA, para la optimización de las respuestas.")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> Refresh(
            [FromBody] string req,
            CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(req))
            {
                return BadRequest(new ApiResponse<object>(
                    data: null,
                    message: "El prompt no puede estar vacío."));
            }

            // Genera la respuesta usando contexto y prompt del usuario
            var result = await _geminiService.GenerateContentAsync(
                _systemPrompt,
                req,
                cancellation);

            var response = new ApiResponse<string>(
                data: result,
                message: "Respuesta optimizada generada correctamente."
            );

            return Ok(response);
        }
    }
}
