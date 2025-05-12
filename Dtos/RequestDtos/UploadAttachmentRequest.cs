using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace CustomerService.API.Dtos.RequestDtos
{
    public class UploadAttachmentRequest
    {
        [Required]
        public int MessageId { get; set; }

        [Required]
        public string MediaId { get; set; } = "";

        public IFormFile? File { get; set; }

        // nombre original
        public string? FileName { get; set; }

        // (si  el cliente pase la URL tras subirla por otro canal)
        public string? MediaUrl { get; set; }
        public string? MimeType { get; set; }
    }
}
