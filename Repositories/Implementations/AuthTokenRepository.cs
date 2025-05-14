using CustomerService.API.Data.Context;
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
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    throw new ArgumentException("El token no puede ser vacío.", nameof(token));

                var result = await _dbSet
                    .AsNoTracking()
                    .SingleOrDefaultAsync(t => t.Token == token, cancellation);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new Exception("No se ha podido acceder a la al token.");
            }
        }

        public async Task<IEnumerable<AuthToken>> GetActiveTokensAsync(int userId, CancellationToken cancellation = default)
        {
            if (userId <= 0)
                throw new ArgumentException("El UserId debe ser mayor que cero.", nameof(userId));

            return await _dbSet
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    !t.Revoked &&
                    !t.Used &&
                    t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellation);
        }

        public void Revoke(AuthToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.Revoked = true;
            _dbSet.Update(token);
        }
    }
}