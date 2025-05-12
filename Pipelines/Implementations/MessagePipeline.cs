using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Data.Context;
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
            // 0) Normalizar y descartar vacíos
            text = (text ?? "").Trim();
            if (string.IsNullOrEmpty(text))
                return;

            // 1) Crea o recupera el usuario
            var user = await _db.Users
                                .FirstOrDefaultAsync(u => u.Phone == fromPhone, ct)
                      ?? new User { Phone = fromPhone, CreatedAt = DateTime.UtcNow };

            if (user.UserId == 0)
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync(ct);
            }

            // 2) Crea o recupera la conversación activa
            var convo = await _db.Conversations
                                 .FirstOrDefaultAsync(c => c.ClientUserId == user.UserId
                                                        && c.Status != "Closed", ct)
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

            // 2.a) Bienvenida única
            if (!convo.Initialized)
            {
                convo.Initialized = true;
                _db.Conversations.Update(convo);
                await _db.SaveChangesAsync(ct);

                // Notificar nueva conversación a la UI de agentes
                await _hubContext.Clients.All
                                 .SendAsync("ConversationCreated", new
                                 {
                                     convo.ConversationId,
                                     convo.ClientUserId,
                                     convo.Status,
                                     convo.CreatedAt
                                 }, ct);

                // Saludo inicial vía WhatsApp
                await _whatsAppService.SendTextAsync(
                    convo.ConversationId,
                    BotUserId,
                    "¡Hola! Soy tu asistente de PCGroup S.A. ¿En qué puedo ayudarte?",
                    ct
                );

                // Si deseas cortar el flujo aquí:
                // return;
            }

            // 3) Persistir el mensaje entrante
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

            // Notificar al grupo de SignalR
            await _hubContext.Clients
                             .Group(convo.ConversationId.ToString())
                             .SendAsync("ReceiveMessage", new
                             {
                                 Message = incoming,
                                 Attachments = Array.Empty<object>()
                             }, ct);

            // 4) Generar la respuesta con Gemini
            var rawReply = await _geminiClient.GenerateContentAsync(_systemPrompt, text, ct);
            var reply = rawReply.Trim();

            // 5) **Solo una** llamada a SendTextAsync persistirá + notificará
            await _whatsAppService.SendTextAsync(
                convo.ConversationId,
                BotUserId,
                reply,
                ct
            );
        }
    }
}
