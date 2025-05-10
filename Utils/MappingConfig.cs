// Utils/MappingConfig.cs
using System;
using System.Linq;
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
            // CreateUserRequest → User
            config.NewConfig<CreateUserRequest, User>()
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PasswordHash, src => src.Password) // seguirá el hashing en el service
                .Map(dest => dest.CompanyId, src => src.CompanyId)
                .Map(dest => dest.Phone, src => src.Phone)
                .Map(dest => dest.Identifier, src => src.Identifier)
                .Map(dest => dest.CreatedAt, src => DateTime.UtcNow)
                .IgnoreNullValues(true);

            // User → UserDto
            config.NewConfig<User, UserDto>()
                .Map(dest => dest.UserId, src => src.UserId)
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.CompanyId, src => src.CompanyId)
                .Map(dest => dest.Phone, src => src.Phone)
                .Map(dest => dest.Identifier, src => src.Identifier);

            // AppRole → RoleDto
            config.NewConfig<AppRole, RoleDto>()
                .Map(dest => dest.RoleId, src => src.RoleId)
                .Map(dest => dest.RoleName, src => src.RoleName)
                .Map(dest => dest.Description, src => src.Description);

            // Company → CompanyDto
            config.NewConfig<Company, CompanyDto>()
                .Map(dest => dest.CompanyId, src => src.CompanyId)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            // AuthResponse tuple → AuthResponseDto
            config.NewConfig<(string access, string refresh, DateTime exp, int userId), AuthResponseDto>()
                .Map(dest => dest.AccessToken, src => src.access)
                .Map(dest => dest.RefreshToken, src => src.refresh)
                .Map(dest => dest.ExpiresAt, src => src.exp)
                .Map(dest => dest.UserId, src => src.userId);
        }
    }
}
