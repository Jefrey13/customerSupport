using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Data.Context;
using CustomerService.API.Repositories.Interfaces;

namespace CustomerService.API.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CustomerSupportContext _context;

        public UnitOfWork(
            CustomerSupportContext context,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IAuthTokenRepository authTokenRepository,
            IUserRoleRepository userRoleRepository,
            ICompanyRepository companyRepository,
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IAttachmentRepository attachmentRepository,
            IRoleMenuRepository roleMenuRepository,
            IMenuRepository menuRepository)
        {
            _context = context;
            Users = userRepository;
            Roles = roleRepository;
            AuthTokens = authTokenRepository;
            UserRoles = userRoleRepository;
            Companies = companyRepository;
            Conversations = conversationRepository;
            Messages = messageRepository;
            Attachments = attachmentRepository;
            Menus = menuRepository;
            RoleMenus = roleMenuRepository;
        }

        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IAuthTokenRepository AuthTokens { get; }
        public IUserRoleRepository UserRoles { get; }
        public ICompanyRepository Companies { get; }
        public IConversationRepository Conversations { get; }
        public IMessageRepository Messages { get; }
        public IAttachmentRepository Attachments { get; }

        public IMenuRepository Menus { get; }
        public IRoleMenuRepository RoleMenus { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellation = default) =>
            _context.SaveChangesAsync(cancellation);
    }
}