using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// Orchestrates incoming WhatsApp messages: persistence, AI generation, and response dispatch.
    /// </summary>
    public class MessagePipeline : IMessagePipeline
    {
        private readonly CustomerSupportContext _db;
        private readonly IGeminiClient _geminiClient;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly string _systemPrompt;
        private readonly int _botUserId;

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

            // Load system prompt and bot user ID from configuration
            _systemPrompt = config["Gemini:SystemPrompt"]!
                ?? throw new ArgumentException("Gemini:SystemPrompt is missing in configuration.");
            _botUserId = int.Parse(config["Bot:UserId"]!);
        }

        public async Task ProcessIncomingAsync(
            string fromPhone,
            string? text,
            string? mediaId,
            string? mimeType,
            string? caption,
            CancellationToken cancellationToken = default)
        {
            // --- 1) Ensure company exists
            var company = await _db.Companies
                .FirstOrDefaultAsync(c => c.Name == fromPhone, cancellationToken)
                ?? new Company { Name = fromPhone, CreatedAt = DateTime.UtcNow };
            if (company.CompanyId == 0)
            {
                _db.Companies.Add(company);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // --- 2) Ensure user exists
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Phone == fromPhone, cancellationToken)
                ?? new User
                {
                    FullName = fromPhone,
                    Email = $"{fromPhone}@wa.local",
                    CompanyId = company.CompanyId,
                    Phone = fromPhone,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
            if (user.UserId == 0)
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // --- 3) Ensure open conversation
            var conversation = await _db.Conversations
                .FirstOrDefaultAsync(c => c.CompanyId == company.CompanyId && c.Status != "Closed", cancellationToken)
                ?? new Conversation
                {
                    CompanyId = company.CompanyId,
                    ClientUserId = user.UserId,
                    Status = "Bot",
                    CreatedAt = DateTime.UtcNow
                };
            if (conversation.ConversationId == 0)
            {
                _db.Conversations.Add(conversation);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // --- 4) Save incoming message
            var incoming = new Message
            {
                ConversationId = conversation.ConversationId,
                SenderId = user.UserId,
                Content = text,
                MessageType = mediaId != null ? "Media" : "Text",
                CreatedAt = DateTime.UtcNow
            };
            _db.Messages.Add(incoming);
            await _db.SaveChangesAsync(cancellationToken);

            // --- 4.b) Attach media if present
            if (mediaId != null && mimeType != null)
            {
                _db.Attachments.Add(new Attachment
                {
                    MessageId = incoming.MessageId,
                    MediaId = mediaId,
                    MimeType = mimeType,
                    FileName = caption,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync(cancellationToken);
            }

            // --- 5) Emit incoming to front-end
            var attachments = mediaId != null
                ? await _db.Attachments
                    .Where(a => a.MessageId == incoming.MessageId)
                    .ToListAsync(cancellationToken)
                : new List<Attachment>();
            await _hubContext.Clients
                .Group(conversation.ConversationId.ToString())
                .SendAsync("ReceiveMessage", new { Message = incoming, Attachments = attachments }, cancellationToken);

            // --- 6) Handle human request keywords
            if (!string.IsNullOrWhiteSpace(text) &&
                new[] { "humano", "asesor", "persona" }
                    .Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                conversation.Status = "WaitingHuman";
                _db.Conversations.Update(conversation);
                await _db.SaveChangesAsync(cancellationToken);
                await SendBotReply(conversation, "Un agente te atenderá en breve…", cancellationToken);
                return;
            }

            // --- 7) Generate bot reply with system context + user text
            var systemContext = _systemPrompt;
            var userPrompt = text ?? string.Empty;
            var botReply = await _geminiClient.GenerateContentAsync(
                systemContext,
                userPrompt,
                cancellationToken);

            // --- 8) Dispatch bot reply
            await SendBotReply(conversation, botReply, cancellationToken);
        }

        private async Task SendBotReply(Conversation conversation, string reply, CancellationToken ct = default)
        {
            var botMessage = new Message
            {
                ConversationId = conversation.ConversationId,
                SenderId = _botUserId,
                Content = reply,
                MessageType = "Bot",
                CreatedAt = DateTime.UtcNow
            };
            _db.Messages.Add(botMessage);
            await _db.SaveChangesAsync(ct);

            // Send via WhatsApp Cloud API
            await _whatsAppService.SendTextAsync(conversation.Company?.Name ?? string.Empty, reply);

            // Emit to front-end via SignalR
            await _hubContext.Clients
                .Group(conversation.ConversationId.ToString())
                .SendAsync("ReceiveMessage", new { Message = botMessage, Attachments = Array.Empty<Attachment>() }, ct);
        }
    }
}