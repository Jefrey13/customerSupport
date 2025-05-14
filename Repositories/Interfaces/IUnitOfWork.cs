using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IAuthTokenRepository AuthTokens { get; }
        IUserRoleRepository UserRoles { get; }
        ICompanyRepository Companies { get; }
        IConversationRepository Conversations { get; }
        IMessageRepository Messages { get; }
        IAttachmentRepository Attachments { get; }

        IMenuRepository Menus { get; }
        IRoleMenuRepository RoleMenus { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellation = default);
    }
}