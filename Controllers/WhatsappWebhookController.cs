using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Pipelines.Interfaces;
using CustomerService.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class WhatsappWebhookController : ControllerBase
    {
        private const int BotUserId = 1;

        private readonly IMessagePipeline _pipeline;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IUnitOfWork _uow;
        private readonly IHubContext<ChatHub> _hub;
        private readonly string _verifyToken;

        public WhatsappWebhookController(
            IMessagePipeline pipeline,
            IWhatsAppService whatsAppService,
            IUnitOfWork uow,
            IHubContext<ChatHub> hubContext,
            IConfiguration config)
        {
            _pipeline = pipeline;
            _whatsAppService = whatsAppService;
            _uow = uow;
            _hub = hubContext;
            _verifyToken = config["WhatsApp:VerifyToken"]!;
        }

        [HttpGet("webhook", Name = "VerifyWhatsappWebhook")]
        [SwaggerOperation(
            Summary = "Verify WhatsApp webhook subscription",
            Description = "Valida hub.mode y hub.verify_token y devuelve hub.challenge si coinciden.",
            Tags = new[] { "WhatsApp Webhook" }
        )]
        [SwaggerResponse(200, "Webhook verified successfully", typeof(string))]
        [SwaggerResponse(403, "Invalid verification token")]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode"), SwaggerParameter("Expected 'subscribe'", Required = true)] string mode,
            [FromQuery(Name = "hub.verify_token"), SwaggerParameter("Your verify token", Required = true)] string token,
            [FromQuery(Name = "hub.challenge"), SwaggerParameter("Challenge to echo back", Required = true)] string challenge)
        {
            if (mode == "subscribe" && token == _verifyToken)
                return Content(challenge, "text/plain");

            return Forbid();
        }

        [HttpPost("webhook", Name = "ReceiveWhatsappWebhook")]
        [SwaggerOperation(
            Summary = "Receive WhatsApp messages and status updates",
            Description = "Procesa mensajes entrantes, attachments y callbacks de estado.",
            Tags = new[] { "WhatsApp Webhook" }
        )]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReceiveAsync(
            [FromBody] WhatsAppUpdateRequest update,
            CancellationToken cancellation)
        {
            if (update?.Entry == null ||
                !update.Entry.Any() ||
                update.Entry.First().Changes.First().Value.Messages.Count() <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid payload structure."));
            }

            var change = update.Entry.First().Changes.First().Value;
            var incoming = change.Messages.First();

            // 1) Procesar cambios de estado (delivered, read, etc.)
            if (incoming.Statuses != null && incoming.Statuses.Any())
            {
                foreach (var st in incoming.Statuses)
                {
                    // Buscamos el mensaje por su ExternalId
                    var msg = await _uow.Messages.GetByExternalIdAsync(st.Id, cancellation);
                    if (msg != null)
                    {
                        msg.Status = st.Status;
                        if (st.Status == "delivered") msg.DeliveredAt = DateTime.UtcNow;
                        if (st.Status == "read") msg.ReadAt = DateTime.UtcNow;

                        _uow.Messages.Update(msg);
                        await _uow.SaveChangesAsync(cancellation);

                        // Notificar al front via SignalR
                        var statusDto = new MessageDto
                        {
                            MessageId = msg.MessageId,
                            ConversationId = msg.ConversationId,
                            SenderId = msg.SenderId,
                            Content = msg.Content,
                            MessageType = msg.MessageType,
                            CreatedAt = msg.CreatedAt,
                            Status = msg.Status,
                            DeliveredAt = msg.DeliveredAt,
                            ReadAt = msg.ReadAt,
                            Attachments = msg.Attachments
                                                .Select(a => new AttachmentDto
                                                {
                                                    AttachmentId = a.AttachmentId,
                                                    MessageId = a.MessageId,
                                                    MediaId = a.MediaId,
                                                    FileName = a.FileName,
                                                    MimeType = a.MimeType,
                                                    MediaUrl = a.MediaUrl,
                                                    CreatedAt = a.CreatedAt
                                                })
                                                .ToList()
                        };

                        await _hub.Clients
                                  .Group(msg.ConversationId.ToString())
                                  .SendAsync("MessageStatusChanged", statusDto, cancellation);
                    }
                }
            }

            // 2) Procesar mensaje entrante (texto o media)
            await _pipeline.ProcessIncomingAsync(change, cancellation);

            return Ok(ApiResponse<object>.Ok("Webhook event processed successfully."));
        }

        [HttpPost("{conversationId}/send")]
        [SwaggerOperation(
            Summary = "Envía un mensaje de texto en el contexto de una conversación",
            Description = "Guarda el mensaje en BD, lo envía por WhatsApp Cloud API y notifica a clientes SignalR."
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

            await _whatsAppService.SendTextAsync(conversationId, BotUserId, req.Body, cancellation);
            return Ok(ApiResponse<object>.Ok("Mensaje enviado y registrado correctamente."));
        }

        [HttpPost("{conversationId}/send-media", Name = "SendWhatsappMedia")]
        [SwaggerOperation(
            Summary = "Envía un archivo multimedia en el contexto de una conversación",
            Description = "Recibe multipart/form-data, guarda el archivo en BD, lo envía por WhatsApp Cloud API y notifica por SignalR."
        )]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendMediaAsync(
            int conversationId,
            IFormFile file,
            [FromForm] string? caption,
            CancellationToken cancellation)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Fail("Se debe enviar un archivo válido."));

            byte[] data;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellation);
                data = ms.ToArray();
            }

            await _whatsAppService.SendMediaAsync(
                conversationId,
                BotUserId,
                data,
                file.FileName,
                file.ContentType,
                caption,
                cancellation);

            return Ok(ApiResponse<object>.Ok("Archivo multimedia enviado correctamente."));
        }
    }
}