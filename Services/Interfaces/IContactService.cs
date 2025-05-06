using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface IContactService
    {
        Task<PagedResponse<ContactDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default);
        Task<ContactDto> GetByIdAsync(Guid id, CancellationToken cancellation = default);
        Task<ContactDto> CreateAsync(CreateContactRequest request, CancellationToken cancellation = default);
        Task UpdateAsync(UpdateContactRequest request, CancellationToken cancellation = default);
        Task DeleteAsync(Guid id, CancellationToken cancellation = default);
    }
}
