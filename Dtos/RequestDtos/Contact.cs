namespace CustomerService.API.Dtos.RequestDtos
{
    public class Contact
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public Profile Profile { get; set; }
    }
}