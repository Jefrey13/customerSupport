using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CustomerService.API.Data.context;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ConversationsController : ControllerBase
    {
        private readonly CustomerSupportContext _db;
        private readonly IHubContext<ChatHub> _hub;
        private readonly IWhatsAppService _wa;
        private readonly IGeminiClient _ai;

        public ConversationsController(
            CustomerSupportContext db,
            IHubContext<ChatHub> hub,
            IWhatsAppService wa,
            IGeminiClient ai)
        {
            _db = db;
            _hub = hub;
            _wa = wa;
            _ai = ai;
        }

        // GET /api/v1/conversations/{id}
        [HttpGet("{id}", Name = "GetConversation")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ConversationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConversation(int id)
        {
            var conv = await _db.Conversations
                .Include(c => c.Contact)
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (conv == null)
                return NotFound(new ApiResponse<object>(null, "Conversación no encontrada."));

            var dto = new ConversationDto
            {
                ConversationId = conv.ConversationId,
                ContactId = conv.ContactId,
                AssignedAgent = conv.AssignedAgent,
                Status = conv.Status,
                CreatedAt = conv.CreatedAt,
                CreatedBy = conv.CreatedBy,
                UpdatedAt = conv.UpdatedAt,
                UpdatedBy = conv.UpdatedBy
            };

            return Ok(new ApiResponse<ConversationDto>(dto, "OK"));
        }

        // POST /api/v1/conversations
        [HttpPost]
        [Authorize(Policy = "ClientPolicy")]
        [SwaggerOperation(Summary = "Cliente inicia una conversación (bot IA + WhatsApp)")]
        [ProducesResponseType(typeof(ApiResponse<ConversationDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateConversationByClient(
            [FromBody] CreateConversationRequest req
            , CancellationToken cancellation = default)
        {
            var contact = await _db.Contacts.FindAsync(req.ContactId);
            if (contact == null)
                return BadRequest(new ApiResponse<object>(null, "Contacto no existe."));

            var userId = Guid.Parse(User.FindFirst("sub")!.Value);

            // 1) Crear conversación
            var conv = new Conversation
            {
                ContactId = req.ContactId,
                AssignedAgent = null,
                Status = "Bot",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            _db.Conversations.Add(conv);
            await _db.SaveChangesAsync();

            // 2) IA: mensaje de bienvenida
            var welcome = "Bienvenido al soporte, ¿en qué podemos ayudarte hoy?";
            var botReply = await _ai.GenerateContentAsync(welcome, cancellation);

            // 3) Guardar mensaje bot
            var botMsg = new Message
            {
                ConversationId = conv.ConversationId,
                SenderId = userId, // o un BotUserId fijo
                Content = botReply,
                MessageType = "Bot",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            _db.Messages.Add(botMsg);
            await _db.SaveChangesAsync();

            // 4) Enviar por WhatsApp
            if (!string.IsNullOrEmpty(contact.Phone))
                await _wa.SendTextAsync(contact.Phone, botReply);

            // 5) Notificar front vía SignalR
            await _hub.Clients.Group(conv.ConversationId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    Message = botMsg,
                    Attachments = Array.Empty<object>()
                });

            // 6) Devolver DTO
            var dto = new ConversationDto
            {
                ConversationId = conv.ConversationId,
                ContactId = conv.ContactId,
                AssignedAgent = conv.AssignedAgent,
                Status = conv.Status,
                CreatedAt = conv.CreatedAt,
                CreatedBy = conv.CreatedBy
            };

            return CreatedAtAction(
                nameof(GetConversation),
                new { id = conv.ConversationId },
                new ApiResponse<ConversationDto>(dto, "Conversación iniciada.")
            );
        }

        // PATCH /api/v1/conversations/{id}
        [HttpPatch("{id}", Name = "AssignAgent")]
        [Authorize(Policy = "AgentPolicy")]
        [SwaggerOperation(Summary = "Asigna un agente y actualiza el estado")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Patch(
            int id,
            [FromBody] UpdateConversationRequest req)
        {
            var conv = await _db.Conversations.FindAsync(id);
            if (conv == null)
                return NotFound(new ApiResponse<object>(null, "Conversación no encontrada."));

            conv.AssignedAgent = req.AssignedAgent;
            conv.Status = req.Status;
            conv.UpdatedAt = DateTime.UtcNow;
            conv.UpdatedBy = Guid.Parse(User.FindFirst("sub")!.Value);

            await _db.SaveChangesAsync();

            await _hub.Clients.User(req.AssignedAgent.ToString())
                .SendAsync("AssignedConversation", new { ConversationId = id });

            return NoContent();
        }

        // POST /api/v1/conversations/{id}/messages
        [HttpPost("{id}/messages", Name = "AgentSendMessage")]
        [Authorize(Policy = "AgentPolicy")]
        [SwaggerOperation(Summary = "Envia un mensaje (texto o fichero) desde un agente")]
        [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostMessage(
            int id,
            [FromForm] SendMessageRequest req)
        {
            var conv = await _db.Conversations
                .Include(c => c.Contact)
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (conv == null)
                return NotFound(new ApiResponse<object>(null, "Conversación no encontrada."));

            string? mediaId = null, mimeType = null;
            if (req.File != null)
            {
                using var ms = new MemoryStream();
                await req.File.CopyToAsync(ms);
                var data = ms.ToArray();

                mediaId = await _wa.UploadMediaAsync(data, req.File.ContentType);
                mimeType = req.File.ContentType;

                await _wa.SendMediaAsync(
                    conv.Contact.Phone!,
                    mediaId,
                    mimeType,
                    req.Caption);
            }
            else if (!string.IsNullOrWhiteSpace(req.Content))
            {
                await _wa.SendTextAsync(conv.Contact.Phone!, req.Content);
            }

            var msg = new Message
            {
                ConversationId = id,
                SenderId = req.SenderId,
                Content = req.Content,
                Caption = req.Caption,
                MessageType = mediaId != null ? "Media" : "Text",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = req.SenderId
            };
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();

            if (mediaId != null)
            {
                _db.Attachments.Add(new Attachment
                {
                    MessageId = msg.MessageId,
                    MediaId = mediaId,
                    MimeType = mimeType,
                    FileName = req.File!.FileName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = req.SenderId
                });
                await _db.SaveChangesAsync();
            }

            var atts = mediaId != null
                ? await _db.Attachments
                    .Where(a => a.MessageId == msg.MessageId)
                    .ToListAsync()
                : new List<Attachment>();

            var result = new MessageDto
            {
                MessageId = msg.MessageId,
                ConversationId = msg.ConversationId,
                SenderId = msg.SenderId,
                Content = msg.Content,
                Caption = msg.Caption,
                MessageType = msg.MessageType,
                CreatedAt = msg.CreatedAt,
                CreatedBy = msg.CreatedBy,
                UpdatedAt = msg.UpdatedAt,
                UpdatedBy = msg.UpdatedBy,
                Attachments = atts.Select(a => new AttachmentDto
                {
                    AttachmentId = a.AttachmentId,
                    MessageId = a.MessageId,
                    MediaId = a.MediaId,
                    FileName = a.FileName,
                    MimeType = a.MimeType,
                    MediaUrl = a.MediaUrl
                }).ToList()
            };

            await _hub.Clients.Group(id.ToString())
                .SendAsync("ReceiveMessage", new { Message = result, Attachments = result.Attachments });

            return CreatedAtRoute("AgentSendMessage",
                new { id = msg.MessageId },
                new ApiResponse<MessageDto>(result, "Mensaje enviado."));
        }
    }
}