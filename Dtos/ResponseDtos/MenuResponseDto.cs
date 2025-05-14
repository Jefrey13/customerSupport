namespace CustomerService.API.Dtos.ResponseDtos
{
    public class MenuResponseDto
    {
        public int MenuId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int Index { get; set; }
        public string Icon { get; set; } = null!;
    }
}
