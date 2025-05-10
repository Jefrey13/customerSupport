namespace CustomerService.API.Dtos.RequestDtos
{
    public class ChangeValue
    {
        public List<WARequestMessage> Messages { get; set; } = new();
    }
}