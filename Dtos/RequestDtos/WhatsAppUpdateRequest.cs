namespace CustomerService.API.Dtos.RequestDtos
{
    public class WhatsAppUpdateRequest
    {
        public List<Entry> Entry { get; set; } = new();
    }

    public class Entry
    {
        public List<Change> Changes { get; set; } = new();
    }

    public class Change
    {
        public ChangeValue Value { get; set; } = new();
    }

    public class ChangeValue
    {
        public List<WARequestMessage> Messages { get; set; } = new();
    }

    public class WARequestMessage
    {
        public string From { get; set; } = null!;
        public TextDto? Text { get; set; }
        public ImageDto? Image { get; set; }
        public VideoDto? Video { get; set; }
        public DocumentDto? Document { get; set; }
        public string? Caption { get; set; }
    }

    public class TextDto { public string Body { get; set; } = null!; }
    public class ImageDto { public string Id { get; set; } = null!; }
    public class VideoDto { public string Id { get; set; } = null!; }
    public class DocumentDto
    {
        public string Id { get; set; } = null!;
        public string Filename { get; set; } = null!;
    }
}
