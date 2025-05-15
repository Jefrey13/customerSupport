namespace CustomerService.API.Dtos.RequestDtos
{
    public class ChangeValue
    {
        public List<WARequestMessage> Messages { get; set; } = new();
        //public Contact? Contacts { get; set; }
       // public List<StatusDto> Statuses { get; set; }
    }
}