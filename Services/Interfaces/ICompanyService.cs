using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Dtos.RequestDtos;
namespace CustomerService.API.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyDto>> GetAllAsync(CancellationToken cancellation = default);
        Task<CompanyDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken cancellation = default);
        Task UpdateAsync(UpdateCompanyRequest request, CancellationToken cancellation = default);
        Task DeleteAsync(int id, CancellationToken cancellation = default);
    }
}
