namespace CustomerService.API.Dtos.ResponseDtos
{
    public class AttachmentDto
    {
        public int AttachmentId { get; set; }
        public int MessageId { get; set; }
        public string MediaId { get; set; } = null!;
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
        public string? MediaUrl { get; set; }
    }
}