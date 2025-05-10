using CustomerService.API.Models;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IAuthTokenRepository : IGenericRepository<AuthToken>
    {
        Task<AuthToken?> GetByTokenAsync(string token, CancellationToken cancellation = default);
        Task<IEnumerable<AuthToken>> GetActiveTokensAsync(int userId, CancellationToken cancellation = default);
        void Revoke(AuthToken token);
    }
}
