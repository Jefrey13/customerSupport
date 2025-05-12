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
            config.NewConfig<CreateUserRequest, User>();

            config.NewConfig<User, UserDto>()
                .Map(d => d.UserId, s => s.UserId)
                .Map(d => d.FullName, s => s.FullName)
                .Map(d => d.Email, s => s.Email)
                .Map(d => d.IsActive, s => s.IsActive)
                .Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.Phone, s => s.Phone)
                .Map(d => d.Identifier, s => s.Identifier)
                .Map(d => d.CreatedAt, s => s.CreatedAt)
                .Map(d => d.UpdatedAt, s => s.UpdatedAt)
                .Map(d => d.ImageUrl, s => s.ImageUrl);

            config.NewConfig<Conversation, ConversationDto>()
                .Map(d => d.ConversationId, s => s.ConversationId)
                .Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.ClientUserId, s => s.ClientUserId)
                .Map(d => d.AssignedAgent, s => s.AssignedAgent)
                .Map(d => d.Status, s => s.Status)
                .Map(d => d.CreatedAt, s => s.CreatedAt)
                .Map(d => d.AssignedAt, s => s.AssignedAt)
                .Map(d => d.TotalMensajes, s => s.Messages.Count)
                .Map(d => d.UltimaActividad, s => s.Messages.Any() ? s.Messages.Max(m => m.CreatedAt) : s.CreatedAt)
                .Map(d => d.Duracion, s => DateTime.UtcNow - s.CreatedAt);

            config.NewConfig<Company, CompanyDto>()
                .Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.Name, s => s.Name)
                .Map(d => d.Address, s => s.Address)
                .Map(d => d.CreatedAt, s => s.CreatedAt);

            config.NewConfig<Message, MessageDto>();
        }
    }
}