using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using Mapster;

namespace CustomerService.API.Utils
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Register
            config.NewConfig<RegisterRequest, User>()
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PasswordHash, src => src.Password) // hashed later in service
                .IgnoreNullValues(true);

            // User ↔ UserDto
            config.NewConfig<User, UserDto>()
                .Map(dest => dest.UserId, src => src.UserId)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.RoleIds, src => src.UserRoleUsers.Select(ur => ur.RoleId));

            // Role ↔ RoleDto
            config.NewConfig<AppRole, RoleDto>()
                .Map(dest => dest.RoleId, src => src.RoleId)
                .Map(dest => dest.RoleName, src => src.RoleName)
                .Map(dest => dest.Description, src => src.Description);

            // Contact ↔ ContactDto
            config.NewConfig<Contact, ContactDto>()
                .Map(dest => dest.ContactId, src => src.ContactId)
                .Map(dest => dest.CompanyName, src => src.CompanyName)
                .Map(dest => dest.ContactName, src => src.ContactName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.Phone, src => src.Phone)
                .Map(dest => dest.Country, src => src.Country)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            // AuthResponseDto
            config.NewConfig<(string access, string refresh, DateTime exp, Guid userId, Guid contactId), AuthResponseDto>()
                .Map(dest => dest.AccessToken, src => src.access)
                .Map(dest => dest.RefreshToken, src => src.refresh)
                .Map(dest => dest.ExpiresAt, src => src.exp)
                .Map(dest => dest.UserId, src => src.userId)
                .Map(dest => dest.ContactId, src => src.contactId);
        }
    }
}
