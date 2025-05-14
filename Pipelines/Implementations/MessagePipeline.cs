using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Data.Context;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Pipelines.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CustomerService.API.Pipelines.Implementations
{
    public class MessagePipeline : IMessagePipeline
    {
        private readonly CustomerSupportContext _db;
        private readonly IGeminiClient _geminiClient;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly string _systemPrompt;
        private const int BotUserId = 1;

        public MessagePipeline(
            CustomerSupportContext db,
            IGeminiClient geminiClient,
            IWhatsAppService whatsAppService,
            IHubContext<ChatHub> hubContext,
            IConfiguration config)
        {
            _db = db;
            _geminiClient = geminiClient;
            _whatsAppService = whatsAppService;
            _hubContext = hubContext;
            _systemPrompt = config["Gemini:SystemPrompt"]!;
        }
        public async Task ProcessIncomingAsync(
            string fromPhone,
            string externalId,
            string? text,
            string? mediaId,
            string? mimeType,
            string? caption,
            CancellationToken ct = default)
        {
            text = (text ?? "").Trim();
            if (string.IsNullOrEmpty(text))
                return;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == fromPhone, ct)
                       ?? new User { Phone = fromPhone, CreatedAt = DateTime.UtcNow };
            if (user.UserId == 0)
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync(ct);
            }

            var convo = await _db.Conversations
                .FirstOrDefaultAsync(c => c.ClientUserId == user.UserId && c.Status != "Closed", ct)
                    ?? new Conversation
                    {
                        ClientUserId = user.UserId,
                        Status = "Bot",
                        CreatedAt = DateTime.UtcNow,
                        Initialized = false
                    };

            if (convo.ConversationId == 0)
            {
                _db.Conversations.Add(convo);
                await _db.SaveChangesAsync(ct);
            }

            if (!convo.Initialized)
            {
                convo.Initialized = true;
                _db.Conversations.Update(convo);
                await _db.SaveChangesAsync(ct);

                var fullConvo = await _db.Conversations
                    .Include(c => c.Messages)
                    .Include(c => c.ClientUser)
                    .SingleAsync(c => c.ConversationId == convo.ConversationId, ct);

                var convoDto = new ConversationDto
                {
                    ConversationId = fullConvo.ConversationId,
                    CompanyId = fullConvo.CompanyId,
                    ClientUserId = fullConvo.ClientUserId,
                    AssignedAgent = fullConvo.AssignedAgent,
                    Status = fullConvo.Status,
                    CreatedAt = fullConvo.CreatedAt,
                    AssignedAt = fullConvo.AssignedAt,
                    ContactName = fullConvo.ClientUser?.FullName ?? fullConvo.ClientUser?.Phone ?? "",
                    TotalMensajes = fullConvo.Messages.Count,
                    UltimaActividad = fullConvo.Messages.Any()
                                      ? fullConvo.Messages.Max(m => m.CreatedAt)
                                      : fullConvo.CreatedAt,
                    Duracion = DateTime.UtcNow - fullConvo.CreatedAt
                };

                await _hubContext.Clients.All.SendAsync("ConversationCreated", convoDto, ct);

                await _whatsAppService.SendTextAsync(
                    convo.ConversationId,
                    BotUserId,
                    "¡Hola! Soy tu asistente AI de PCGroup S.A. ¿En qué puedo ayudarte?",
                    ct
                );
            }

            var incoming = new Message
            {
                ConversationId = convo.ConversationId,
                SenderId = user.UserId,
                ExternalId = externalId ?? Guid.NewGuid().ToString(),
                Content = text,
                MessageType = "Text",
                CreatedAt = DateTime.UtcNow
            };

            _db.Messages.Add(incoming);
            await _db.SaveChangesAsync(ct);

            await _hubContext.Clients
                .Group(convo.ConversationId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    Message = new
                    {
                        incoming.MessageId,
                        incoming.ConversationId,
                        incoming.SenderId,
                        incoming.Content,
                        incoming.MessageType,
                        incoming.CreatedAt
                    },
                    Attachments = Array.Empty<object>()
                }, ct);

            // Múltiples respuestas del bot mientras el estado sea "Bot"
            if (convo.Status == "Bot")
            {
                var history = await _db.Messages
                    .Where(m => m.ConversationId == convo.ConversationId)
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new
                    {
                        Role = m.SenderId == BotUserId ? "Support" : "user",
                        Content = m.Content!
                    })
                    .ToListAsync(ct);

                var sb = new StringBuilder();
                sb.AppendLine(_systemPrompt);
                foreach (var msg in history)
                    sb.Append(msg.Role == "ser" ? "User: " : "Support: ")
                      .AppendLine(msg.Content);
                sb.Append("User: ").AppendLine(text).Append("Support: ");

                var fullPrompt = sb.ToString();
                var rawReply = await _geminiClient.GenerateContentAsync(fullPrompt, "", ct);
                var reply = rawReply.Trim();

                await _whatsAppService.SendTextAsync(
                    convo.ConversationId,
                    BotUserId,
                    reply,
                    ct
                );

                // Guardar la respuesta del bot en la base de datos
                var botMessage = new Message
                {
                    ConversationId = convo.ConversationId,
                    SenderId = BotUserId,
                    Content = reply,
                    MessageType = "Text",
                    CreatedAt = DateTime.UtcNow
                };
                _db.Messages.Add(botMessage);
                await _db.SaveChangesAsync(ct);
            }
        }

    }
}