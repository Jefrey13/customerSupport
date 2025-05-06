namespace CustomerService.API.Dtos.ResponseDtos
{
    public class AuthResponseDto
    {
        public string AccessToken { get; init; } = "";
        public string RefreshToken { get; init; } = "";
        public DateTime ExpiresAt { get; init; }
        public Guid UserId { get; init; }
        public Guid ContactId { get; init; }
    }
}
