namespace CustomerService.API.Utils
{
    public class AuthResult
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public DateTime ExpiresAt { get; init; }

        public AuthResult(string accessToken, string refreshToken, DateTime expiresAt)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
        }
    }
}
