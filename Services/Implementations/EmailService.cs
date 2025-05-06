using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace CustomerService.API.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _s;
        public EmailService(IOptions<EmailSettings> options) => _s = options.Value;
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            using var msg = new MailMessage(_s.From, to, subject, htmlBody) { IsBodyHtml = true };
            using var client = new SmtpClient(_s.Host, _s.Port)
            {
                EnableSsl = _s.EnableSsl,
                Credentials = new NetworkCredential(_s.Username, _s.Password)
            };
            await client.SendMailAsync(msg);
        }
    }
}
