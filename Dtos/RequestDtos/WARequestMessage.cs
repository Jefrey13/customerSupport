namespace CustomerService.API.Dtos.RequestDtos
{
    public class WARequestMessage
    {
        public string From { get; set; } = null!;
        public TextDto? Text { get; set; }
        public ImageDto? Image { get; set; }
        public VideoDto? Video { get; set; }
        public DocumentDto? Document { get; set; }
        public string? Caption { get; set; }
    }
}