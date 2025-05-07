using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register", Name = "Register")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Register a new user and contact")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _auth.RegisterAsync(req);
            var response = new ApiResponse<AuthResponseDto>(
                data: result,
                message: "Usuario registrado exitosamente."
            );
            // Si tu RegisterAsync retorna además un ID, podrías devolverlo en routeValues:
            return CreatedAtAction(nameof(Register), routeValues: null, value: response);
        }

        [HttpPost("login", Name = "Login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Authenticate user and return tokens")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _auth.LoginAsync(req);
            var response = new ApiResponse<AuthResponseDto>(
                data: result,
                message: "Inicio de sesión exitoso."
            );
            return Ok(response);
        }

        [HttpPost("refresh", Name = "RefreshToken")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Refresh access token using a valid refresh token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
        {
            var result = await _auth.RefreshTokenAsync(req);
            var response = new ApiResponse<AuthResponseDto>(
                data: result,
                message: "Token de acceso renovado correctamente."
            );
            return Ok(response);
        }

        [HttpPost("forgot-password", Name = "ForgotPassword")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Send password reset link to user email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            await _auth.ForgotPasswordAsync(req);
            var response = new ApiResponse<object>(
                data: null,
                message: "Enlace para restablecer contraseña enviado al correo."
            );
            return Ok(response);
        }

        [HttpPost("reset-password", Name = "ResetPassword")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Reset user password using provided token")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            await _auth.ResetPasswordAsync(req);
            var response = new ApiResponse<object>(
                data: null,
                message: "Contraseña restablecida correctamente."
            );
            return Ok(response);
        }

        [HttpGet("verify-email", Name = "VerifyEmail")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Verify user email using token")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            await _auth.VerifyEmailAsync(token);
            var response = new ApiResponse<object>(
                data: null,
                message: "Correo verificado exitosamente."
            );
            return Ok(response);
        }
    }
}