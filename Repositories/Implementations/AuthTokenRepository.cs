using CustomerService.API.Data.context;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Repositories.Implementations
{
    public class AuthTokenRepository : GenericRepository<AuthToken>, IAuthTokenRepository
    {
        public AuthTokenRepository(CustomerSupportContext context) : base(context) { }

        public async Task<AuthToken?> GetByTokenAsync(string token, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException(nameof(token));
            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(t => t.Token == token, cancellation);
        }

        public async Task<IEnumerable<AuthToken>> GetActiveTokensAsync(Guid userId, CancellationToken cancellation = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
            return await _dbSet
                .AsNoTracking()
                .Where(t => t.UserId == userId && !t.Revoked && !t.Used && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellation);
        }

        public void Revoke(AuthToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            token.Revoked = true;
            _dbSet.Update(token);
        }
    }

}
