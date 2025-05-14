using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class MenusController : ControllerBase
    {
        private readonly IMenuService _menus;
        public MenusController(IMenuService menus) => _menus = menus;

        [HttpGet(Name = "GetMenus")]
        [SwaggerOperation(Summary = "Retrieve menu options for the current user")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<MenuResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        public async Task<IActionResult> Get(CancellationToken ct = default)
        {
            var roles = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .Distinct()
                            .ToList();

            if (!roles.Any())
                return StatusCode(
                  StatusCodes.Status403Forbidden,
                  ApiResponse<object>.Fail("No se encontraron roles en el token.")
                );

            var menus = await _menus.GetByRolesAsync(roles, ct);
            return Ok(new ApiResponse<IEnumerable<MenuResponseDto>>(menus, "Menús obtenidos."));
        }
    }
}