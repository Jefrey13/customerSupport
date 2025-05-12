using System.Linq;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Utils;
using CustomerService.API.Pipelines.Interfaces;
using CustomerService.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IMessagePipeline _pipeline;
        private readonly IWhatsAppService _whatsAppService;
        private readonly string _verifyToken;

        public WhatsappWebhookController(
            IMessagePipeline pipeline,
            IWhatsAppService whatsAppService,
            IConfiguration config)
        {
            _pipeline = pipeline;
            _whatsAppService = whatsAppService;
            _verifyToken = config["WhatsApp:VerifyToken"]!;
        }

        /// <summary>
        /// Verifies the webhook subscription request from WhatsApp Cloud API.
        /// </summary>
        [HttpGet("webhook", Name = "VerifyWhatsappWebhook")]
        [SwaggerOperation(
            Summary = "Verify WhatsApp webhook subscription",
            Description = "Valida hub.mode y hub.verify_token y devuelve hub.challenge si coinciden.",
            Tags = new[] { "WhatsApp Webhook" }
        )]
        [SwaggerResponse(200, "Webhook verified successfully", typeof(string))]
        [SwaggerResponse(403, "Invalid verification token")]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode"), SwaggerParameter("Expected 'subscribe'", Required = true)]
            string mode,

            [FromQuery(Name = "hub.verify_token"), SwaggerParameter("Your verify token", Required = true)]
            string token,

            [FromQuery(Name = "hub.challenge"), SwaggerParameter("Challenge to echo back", Required = true)]
            string challenge)
        {
            if (mode == "subscribe" && token == _verifyToken)
                return Content(challenge, "text/plain");

            return Forbid();
        }

        /// <summary>
        /// Recibe actualizaciones de WhatsApp Cloud API.
        /// Solo procesa ‘messages’ y descarta ‘statuses’ para evitar duplicados.
        /// </summary>
        [HttpPost("webhook", Name = "ReceiveWhatsappWebhook")]
        [SwaggerOperation(
            Summary = "Receive WhatsApp messages",
            Description = "Procesa solo mensajes entrantes, ignora callbacks de estado.",
            Tags = new[] { "WhatsApp Webhook" }
        )]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReceiveAsync([FromBody] WhatsAppUpdateRequest update)
        {
            if (update?.Entry == null || !update.Entry.Any())
                return BadRequest(ApiResponse<object>.Fail("Invalid payload structure."));

            // Tomamos el primer cambio
            var change = update.Entry
                               .First()
                               .Changes
                               .First()
                               .Value;

            // Procesar solo si hay mensajes entrantes
            if (change.Messages != null && change.Messages.Any())
            {
                var msg = change.Messages.First();
                var from = msg.From;
                var extId = msg.MessageId;                       
                var text = msg.Text?.Body;
                var mediaId = msg.Image?.Id
                              ?? msg.Video?.Id
                              ?? msg.Document?.Id;
                var mime = msg.Image != null ? "image"
                              : msg.Video != null ? "video"
                              : msg.Document != null ? "document"
                              : null;
                var caption = msg.Caption;

                await _pipeline.ProcessIncomingAsync(
                    from,
                    extId,
                    text,
                    mediaId,
                    mime,
                    caption);
            }

            return Ok(new ApiResponse<object>(
                data: null,
                message: "Webhook event processed successfully."
            ));
        }

        /// <summary>
        /// Envía un mensaje de WhatsApp, lo persiste en BD y notifica vía SignalR.
        /// </summary>
        [HttpPost("{conversationId}/send")]
        [SwaggerOperation(
            Summary = "Envía un mensaje en el contexto de una conversación",
            Description = "Guarda el mensaje en la base de datos, lo envía por WhatsApp Cloud API y notifica a clientes SignalR."
        )]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendMessageAsync(
            int conversationId,
            [FromBody] SendWhatsAppRequest req,
            CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(req.To) || string.IsNullOrWhiteSpace(req.Body))
                return BadRequest(ApiResponse<object>.Fail("Los campos 'to' y 'body' son obligatorios."));

            // Aquí defines quién está enviando:
            // si es tu bot, podrías tener un constante o extraerlo de contexto.
            const int BotUserId = 1;

            await _whatsAppService
                .SendTextAsync(conversationId, BotUserId, req.Body, cancellation);

            return Ok(ApiResponse<object>.Ok(message: "Mensaje enviado y registrado correctamente."));
        }
    }
}