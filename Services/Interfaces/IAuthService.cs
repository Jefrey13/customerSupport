using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request, CancellationToken cancellation = default);
        Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellation = default);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellation = default);
        Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellation = default);
        Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellation = default);
        Task VerifyEmailAsync(string token, CancellationToken cancellation = default);
    }
}
