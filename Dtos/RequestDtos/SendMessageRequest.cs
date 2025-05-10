using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class SendMessageRequest
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        public int SenderId { get; set; }

        public string? Content { get; set; }

        [Required]
        public string MessageType { get; set; } = "Text";

        public string? Caption { get; set; }

        public IFormFile? File { get; set; }
    }
}