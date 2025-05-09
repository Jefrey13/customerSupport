using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Data.context;
using CustomerService.API.Models;
using CustomerService.API.Pipelines.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Pipelines.Implementations
{
    public class MessagePipeline : IMessagePipeline
    {
        private readonly CustomerSupportContext _db;
        private readonly IGeminiClient _ai;
        private readonly IWhatsAppService _wa;
        private readonly IHubContext<ChatHub> _hub;
        private readonly Guid _botUserId;

        public MessagePipeline(
            CustomerSupportContext db,
            IGeminiClient ai,
            IWhatsAppService wa,
            IHubContext<ChatHub> hub,
            IConfiguration config)
        {
            _db = db;
            _ai = ai;
            _wa = wa;
            _hub = hub;
            _botUserId = Guid.Parse(config["Bot:UserId"]!);
        }

        public async Task ProcessIncomingAsync(
            string phone,
            string? text,
            string? mediaId,
            string? mimeType,
            string? caption,
            CancellationToken cancellationToken = default
        )
        {
            // 1) Get-or-Create Contact
            var contact = await _db.Contacts
                .FirstOrDefaultAsync(c => c.Phone == phone, cancellationToken)
               ?? new Contact { Phone = phone, ContactName = phone, Email = "" };
            if (contact.ContactId == Guid.Empty)
            {
                _db.Contacts.Add(contact);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // 2) Get-or-Create User for that Contact
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == contact.Email && contact.Email != "", cancellationToken)
               ?? new User
               {
                   FullName = contact.ContactName,
                   Email = contact.Email,
                   IsActive = true,
                   SecurityStamp = Guid.NewGuid(),
                   ConcurrencyStamp = Guid.NewGuid(),
                   CreatedBy = _botUserId,
                   CreatedAt = DateTime.UtcNow,
                   FailedLoginAttempts = 0
               };
            if (user.UserId == Guid.Empty)
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // 3) Get-or-Create Conversation abierta
            var conv = await _db.Conversations
                .FirstOrDefaultAsync(c => c.ContactId == contact.ContactId && c.Status != "Closed", cancellationToken)
               ?? new Conversation
               {
                   ContactId = contact.ContactId,
                   AssignedAgent = null,
                   Status = "Bot",
                   CreatedAt = DateTime.UtcNow,
                   CreatedBy = _botUserId
               };
            if (conv.ConversationId == 0)
            {
                _db.Conversations.Add(conv);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // 4) Guardar mensaje entrante
            var incoming = new Message
            {
                ConversationId = conv.ConversationId,
                SenderId = user.UserId,
                Content = text,
                Caption = caption,
                MessageType = mediaId != null ? "Media" : "Text",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.UserId
            };
            _db.Messages.Add(incoming);
            await _db.SaveChangesAsync(cancellationToken);

            // 4.b) Si es media, guarda en Attachments
            if (mediaId != null && mimeType != null)
            {
                var att = new Attachment
                {
                    MessageId = incoming.MessageId,
                    MediaId = mediaId,
                    MimeType = mimeType,
                    FileName = caption,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = user.UserId
                };
                _db.Attachments.Add(att);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // 5) Emitir al front (SignalR) junto con adjuntos
            var atts = mediaId != null
                ? await _db.Attachments
                    .Where(a => a.MessageId == incoming.MessageId)
                    .ToListAsync(cancellationToken)
                : new List<Attachment>();
            await _hub.Clients.Group(conv.ConversationId.ToString())
                .SendAsync("ReceiveMessage", new { Message = incoming, Attachments = atts });

            // 6) ¿Solicita humano?
            if (!string.IsNullOrWhiteSpace(text) &&
                new[] { "humano", "asesor", "persona" }
                  .Any(k => text!.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                conv.Status = "WaitingHuman";
                await _db.SaveChangesAsync(cancellationToken);
                await SendBotReply(conv, "Un agente te atenderá en breve…", cancellationToken);
                return;
            }

            // 7) Llamada a Gemini AI
            var botReply = await _ai.GenerateContentAsync(text ?? "", cancellationToken);

            // 8) Enviar y guardar respuesta del bot
            await SendBotReply(conv, botReply, cancellationToken);
        }

        private async Task SendBotReply(Conversation conv, string reply, CancellationToken cancellationToken = default)
        {
            // 8.a) Guardar mensaje Bot
            var botMsg = new Message
            {
                ConversationId = conv.ConversationId,
                SenderId = _botUserId,
                Content = reply,
                MessageType = "Bot",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _botUserId
            };
            _db.Messages.Add(botMsg);
            await _db.SaveChangesAsync(cancellationToken);

            // 8.b) Enviar texto al cliente
            await _wa.SendTextAsync(conv.Contact.Phone!, reply);

            // 8.c) Emitir por SignalR
            await _hub.Clients.Group(conv.ConversationId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    Message = botMsg,
                    Attachments = Array.Empty<Attachment>()
                });
        }
    }
}