using CustomerService.API.Models;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class MenuRequestDtos
    {
        public int MenuId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int Index { get; set; }
        public string Icon { get; set; }
    }
}