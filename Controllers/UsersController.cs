using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _users;

        public UsersController(IUserService users)
        {
            _users = users;
        }

        [HttpGet(Name = "GetAllUsers")]
        [SwaggerOperation(Summary = "Retrieve paged list of users")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var paged = await _users.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<UserDto>>(paged, "Users retrieved."));
        }

        [HttpGet("{id}", Name = "GetUserById")]
        [SwaggerOperation(Summary = "Retrieve a user by ID")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await _users.GetByIdAsync(id, ct);
            if (dto is null)
                return NotFound(new ApiResponse<object>(null, "User not found."));
            return Ok(new ApiResponse<UserDto>(dto, "User retrieved."));
        }

        [HttpPost(Name = "CreateUser")]
        [SwaggerOperation(Summary = "Create a new user (incluyendo sus roles)")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct = default)
        {
            var dto = await _users.CreateAsync(req, ct);
            return CreatedAtRoute("GetUserById", new { id = dto.UserId },
                new ApiResponse<UserDto>(dto, "User created."));
        }

        [HttpPut("{id}", Name = "UpdateUser")]
        [SwaggerOperation(Summary = "Update an existing user (y sus roles)")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserRequest req, CancellationToken ct = default)
        {
            if (id != req.UserId)
                return BadRequest(new ApiResponse<object>(null, "Mismatched user ID."));
            await _users.UpdateAsync(req, ct);
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteUser")]
        [SwaggerOperation(Summary = "Delete a user by ID")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
        {
            await _users.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpGet("agents", Name = "GetAgentsByRole")]
        [SwaggerOperation(Summary = "Lista de usuarios filtrado por rol")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AgentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgents([FromQuery] string role, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest(ApiResponse<object>.Fail("Debe especificar el parámetro 'role' para filtrar."));
            var agents = await _users.GetByRoleAsync(role, ct);
            return Ok(new ApiResponse<IEnumerable<AgentDto>>(agents, $"Usuarios con rol '{role}' obtenidos."));
        }
    }
}