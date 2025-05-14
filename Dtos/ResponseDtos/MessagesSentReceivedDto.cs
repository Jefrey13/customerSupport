namespace CustomerService.API.Dtos.ResponseDtos
{
    public class MessagesSentReceivedDto
    {
        public IEnumerable<TemporalCountDto> Sent { get; set; }
        public IEnumerable<TemporalCountDto> Received { get; set; }
    }
}
