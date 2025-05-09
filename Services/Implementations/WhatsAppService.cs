using CustomerService.API.Services.Interfaces;
using System.Net.Http.Headers;

namespace CustomerService.API.Services.Implementations
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _http;
        private readonly string _token;
        private readonly string _phoneNumberId;

        public WhatsAppService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _token = config["WhatsApp:Token"]!;
            _phoneNumberId = config["WhatsApp:PhoneNumberId"]!;
        }

        public async Task SendTextAsync(string toPhone, string text)
        {
            var url = $"https://graph.facebook.com/v16.0/{_phoneNumberId}/messages";
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
            var url = $"https://graph.facebook.com/v16.0/{_phoneNumberId}/media";

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
            var url = $"https://graph.facebook.com/v16.0/{_phoneNumberId}/messages";

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
