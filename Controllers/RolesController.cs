using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roles;

        public RolesController(IRoleService roles)
        {
            _roles = roles;
        }

        [HttpGet(Name = "GetAllRoles")]
        [SwaggerOperation(Summary = "Retrieve paged list of roles")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<RoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var paged = await _roles.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<RoleDto>>(paged, "Roles retrieved."));
        }

        [HttpGet("{id}", Name = "GetRoleById")]
        [SwaggerOperation(Summary = "Retrieve a role by ID")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await _roles.GetByIdAsync(id, ct);
            if (dto is null)
                return NotFound(new ApiResponse<object>(null, "Role not found."));
            return Ok(new ApiResponse<RoleDto>(dto, "Role retrieved."));
        }

        [HttpPost(Name = "CreateRole")]
        [SwaggerOperation(Summary = "Create a new role")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest req, CancellationToken ct = default)
        {
            var dto = await _roles.CreateAsync(req, ct);
            return CreatedAtRoute("GetRoleById", new { id = dto.RoleId },
                new ApiResponse<RoleDto>(dto, "Role created."));
        }

        [HttpPut("{id}", Name = "UpdateRole")]
        [SwaggerOperation(Summary = "Update an existing role")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateRoleRequest req, CancellationToken ct = default)
        {
            if (id != req.RoleId)
                return BadRequest(new ApiResponse<object>(null, "Mismatched role ID."));

            await _roles.UpdateAsync(req, ct);
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteRole")]
        [SwaggerOperation(Summary = "Delete a role by ID")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
        {
            await _roles.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}