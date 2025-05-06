using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register", Name = "Register")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Register a new user and contact")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var dto = await _auth.RegisterAsync(req);
            return CreatedAtRoute("Register", new ApiResponse<AuthResponseDto>(dto));
        }

        [HttpPost("login", Name = "Login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Authenticate user and return tokens")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var dto = await _auth.LoginAsync(req);
            return Ok(new ApiResponse<AuthResponseDto>(dto));
        }

        [HttpPost("refresh", Name = "RefreshToken")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Refresh access token using a valid refresh token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
        {
            var dto = await _auth.RefreshTokenAsync(req);
            return Ok(new ApiResponse<AuthResponseDto>(dto));
        }

        [HttpPost("forgot-password", Name = "ForgotPassword")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Send password reset link to user email")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            await _auth.ForgotPasswordAsync(req);
            return NoContent();
        }

        [HttpPost("reset-password", Name = "ResetPassword")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Reset user password using provided token")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            await _auth.ResetPasswordAsync(req);
            return NoContent();
        }

        [HttpGet("verify-email", Name = "VerifyEmail")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Verify user email using token")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            await _auth.VerifyEmailAsync(token);
            return NoContent();
        }
    }
}