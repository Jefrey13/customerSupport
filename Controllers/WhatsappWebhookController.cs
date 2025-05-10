using System.Linq;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Utils;
using CustomerService.API.Pipelines.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IMessagePipeline _pipeline;
        private readonly string _verifyToken;

        public WhatsappWebhookController(
            IMessagePipeline pipeline,
            IConfiguration config)
        {
            _pipeline = pipeline;
            _verifyToken = config["WhatsApp:VerifyToken"]!;
        }

        [HttpGet("webhook")]
        public IActionResult Get(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
            {
                if (mode == "subscribe" && token == _verifyToken)
                {
                    // Devuelve sólo el challenge, como texto plano
                    return new ContentResult
                    {
                        Content = challenge,
                        ContentType = "text/plain",
                        StatusCode = 200
                    };
                }

                return Forbid();
            }


        [HttpPost("webhook", Name = "ReceiveWhatsappMessage")]
        [SwaggerOperation(Summary = "Recibe mensajes entrantes de WhatsApp")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post(
            [FromBody] WhatsAppUpdateRequest update)
        {
            var msg = update.Entry
                         .First().Changes
                         .First().Value.Messages
                         .First();

            var from = msg.From;
            var text = msg.Text?.Body;
            var mediaId = msg.Image?.Id ?? msg.Video?.Id ?? msg.Document?.Id;
            var mimeType = msg.Image != null ? "image"
                          : msg.Video != null ? "video"
                          : msg.Document != null ? "document"
                          : null;
            var caption = msg.Caption;

            await _pipeline.ProcessIncomingAsync(
                from, text, mediaId, mimeType, caption);

            return Ok(new ApiResponse<object>(
                data: null,
                message: "Webhook processed."));
        }
    }
}