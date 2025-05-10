using System.ComponentModel.DataAnnotations;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class StartConversationRequest
    {
        public int CompanyId { get; set; }

        public int? ClientUserId { get; set; }
    }
}