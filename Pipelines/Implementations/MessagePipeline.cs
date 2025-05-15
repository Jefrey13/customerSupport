using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Data.Context;
using CustomerService.API.Dtos.RequestDtos;
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
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly string _systemPrompt;
        private const int BotUserId = 1;

        public MessagePipeline(
            CustomerSupportContext db,
            IGeminiClient geminiClient,
            IWhatsAppService whatsAppService,
            IMessageService messageService,
            IHubContext<ChatHub> hubContext,
            IConfiguration config)
        {
            _db = db;
            _geminiClient = geminiClient;
            _whatsAppService = whatsAppService;
            _hubContext = hubContext;
            _messageService = messageService;
            _systemPrompt = config["Gemini:SystemPrompt"]!;
        }

        public async Task ProcessIncomingAsync(
             //string fromPhone,
             //string externalId,
             //string? text,
             //string? mediaId,
             //string? mimeType,
             //string? caption,
             ChangeValue value,
            CancellationToken ct = default)
        {
            // 1) Idempotencia: si ya existe este externalId, no procesar de nuevo
            //if (!string.IsNullOrEmpty(externalId) &&
            //    await _db.Messages.AnyAsync(m => m.ExternalId == externalId, ct))
            //{
            //    return;
            //}



            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Phone == value.Messages.First().From, ct)
                ?? new User { Phone = value.Messages.First().From, CreatedAt = DateTime.UtcNow };

            if (user.UserId == 0)
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync(ct);
            }

            // Crear o recuperar la conversación activa
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

            // Marcar conversación inicializada y notificar UI.
            if (!convo.Initialized)
            {
                convo.Initialized = true;
                _db.Conversations.Update(convo);
                await _db.SaveChangesAsync(ct);

                await _hubContext.Clients.All
                                 .SendAsync("ConversationCreated", new
                                 {
                                     convo.ConversationId,
                                     convo.ClientUserId,
                                     convo.Status,
                                     convo.CreatedAt
                                 }, ct);

                //Extraer conversación y compartir enviar a la ui.
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
            }

            // Persistir y notificar el mensaje entrante

           
                var incoming = new Models.Message
                {
                    ConversationId = convo.ConversationId,
                    SenderId = user.UserId,
                    Content = value.Messages.First().Text?.Body,
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

            if (convo.Status == "Bot")
            {
                var fullPromp = "";
                List<MessageDto> messages = _messageService.GetByConversationAsync(convo.ConversationId).Result.ToList();
                foreach (var item in messages)
                {
                    fullPromp = _systemPrompt + item.Content;
                }

                var rawReply = await _geminiClient.GenerateContentAsync(fullPromp, incoming.Content, ct);
                var reply = rawReply.Trim();

                await _whatsAppService.SendTextAsync(
                    convo.ConversationId,
                    BotUserId,
                    reply,
                    ct
                );
            }
        }
    }
}