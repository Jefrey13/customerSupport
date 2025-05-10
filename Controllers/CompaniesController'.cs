using System;
using System.Collections.Generic;
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
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companies;

        public CompaniesController(ICompanyService companies)
        {
            _companies = companies;
        }

        [HttpGet(Name = "GetAllCompanies")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Retrieve list of companies")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CompanyDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var list = await _companies.GetAllAsync(ct);
            return Ok(new ApiResponse<IEnumerable<CompanyDto>>(list, "Companies retrieved."));
        }

        [HttpGet("{id}", Name = "GetCompanyById")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Retrieve a single company by ID")]
        [ProducesResponseType(typeof(ApiResponse<CompanyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await _companies.GetByIdAsync(id, ct);
            if (dto is null)
                return NotFound(new ApiResponse<object>(null, "Company not found."));
            return Ok(new ApiResponse<CompanyDto>(dto, "Company retrieved."));
        }

        [HttpPost(Name = "CreateCompany")]
        [SwaggerOperation(Summary = "Create a new company")]
        [ProducesResponseType(typeof(ApiResponse<CompanyDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCompanyRequest req, CancellationToken ct = default)
        {
            var dto = await _companies.CreateAsync(req, ct);
            return CreatedAtRoute("GetCompanyById", new { id = dto.CompanyId },
                new ApiResponse<CompanyDto>(dto, "Company created."));
        }

        [HttpPut("{id}", Name = "UpdateCompany")]
        [SwaggerOperation(Summary = "Update an existing company")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCompanyRequest req, CancellationToken ct = default)
        {
            if (id != req.CompanyId)
                return BadRequest(new ApiResponse<object>(null, "Mismatched company ID."));

            await _companies.UpdateAsync(req, ct);
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteCompany")]
        [SwaggerOperation(Summary = "Delete a company by ID")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
        {
            await _companies.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
