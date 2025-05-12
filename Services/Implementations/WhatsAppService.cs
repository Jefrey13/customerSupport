    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using CustomerService.API.Services.Interfaces;
    using CustomerService.API.Repositories.Interfaces;
    using CustomerService.API.Utils;
    using Microsoft.AspNetCore.SignalR;
    using CustomerService.API.Models;
    using Microsoft.EntityFrameworkCore;

    namespace CustomerService.API.Services.Implementations
    {
        public class WhatsAppService : IWhatsAppService
        {
            private readonly HttpClient _http;
            private readonly IUnitOfWork _uow;
            private readonly IHubContext<ChatHub> _hub;
            private readonly string _token;
            private readonly string _phoneNumberId;

            public WhatsAppService(
                HttpClient http,
                IUnitOfWork uow,
                IHubContext<ChatHub> hubContext,
                IConfiguration config)
            {
                _http = http;
                _uow = uow;
                _hub = hubContext;
                _token = config["WhatsApp:Token"]!;
                _phoneNumberId = config["WhatsApp:PhoneNumberId"]!;
            }

            public async Task SendTextAsync(
                int conversationId,
                int senderId,
                string text,
                CancellationToken cancellation = default)
            {
                // 1) carga la conversación para obtener el teléfono
                var convo = await _uow.Conversations
                    .GetAll()
                    .Where(c => c.ConversationId == conversationId)
                    .Select(c => new { c.ConversationId, Phone = c.ClientUser!.Phone })
                    .SingleOrDefaultAsync(cancellation)
                    ?? throw new KeyNotFoundException("Conversation not found");

                // 2) persistimos el Message en BD
                var msg = new Message
                {
                    ConversationId = conversationId,
                    SenderId = senderId,
                    Content = text,
                    MessageType = "Text",
                    CreatedAt = DateTime.UtcNow,
                    ExternalId = Guid.NewGuid().ToString()
                };
                await _uow.Messages.AddAsync(msg, cancellation);
                await _uow.SaveChangesAsync(cancellation);

                // 3) llamamos a la Cloud API
                var url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/messages";
                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = convo.Phone,
                    type = "text",
                    text = new { body = text }
                };

                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(payload)
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var res = await _http.SendAsync(req, cancellation);
                res.EnsureSuccessStatusCode();

                // 4) notificamos por SignalR a los clientes suscritos a esta conversación
                var dto = new
                {
                    msg.MessageId,
                    msg.ConversationId,
                    msg.SenderId,
                    msg.Content,
                    msg.MessageType,
                    msg.CreatedAt
                };
                await _hub.Clients
                    .Group(conversationId.ToString())
                    .SendAsync("NewMessage", dto, cancellation);
            }

            public async Task SendTextAsync(string toPhone, string text)
            {
                var url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/messages";
                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = toPhone,
                    type = "text",
                    text = new { body = text }
                };

                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(payload)
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var res = await _http.SendAsync(req);
                res.EnsureSuccessStatusCode();
            }

            public async Task<string> UploadMediaAsync(byte[] data, string mimeType)
            {
                var url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/media";

                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(data), "file", "upload");
                content.Add(new StringContent("whatsapp"), "messaging_product");

                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var res = await _http.SendAsync(req);
                res.EnsureSuccessStatusCode();

                var body = await res.Content.ReadFromJsonAsync<MediaUploadResponse>();
                return body!.Id;
            }

            public async Task SendMediaAsync(string toPhone, string mediaId, string mimeType, string? caption = null)
            {
                var url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/messages";

                object payload = mimeType.StartsWith("image/")
                    ? new
                    {
                        messaging_product = "whatsapp",
                        to = toPhone,
                        type = "image",
                        image = new { id = mediaId, caption }
                    }
                    : mimeType.StartsWith("video/")
                    ? new
                    {
                        messaging_product = "whatsapp",
                        to = toPhone,
                        type = "video",
                        video = new { id = mediaId, caption }
                    }
                    : new
                    {
                        messaging_product = "whatsapp",
                        to = toPhone,
                        type = "document",
                        document = new { id = mediaId, filename = "file", caption }
                    };

                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(payload)
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var res = await _http.SendAsync(req);
                res.EnsureSuccessStatusCode();
            }

            private class MediaUploadResponse
            {
                public string Id { get; set; } = "";
            }
        }
    }