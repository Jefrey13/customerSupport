using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Services.Interfaces;

namespace CustomerService.API.Services.Implementations
{
    public class PresenceService : IPresenceService
    {
        private readonly ConcurrentDictionary<int, DateTime> _lastOnline = new();

        public void MarkOnline(int userId)
        {
            _lastOnline[userId] = DateTime.UtcNow;
        }

        public void MarkOffline(int userId)
        {
            _lastOnline.TryRemove(userId, out _);
        }

        public Task<DateTime?> GetLastOnlineAsync(int userId, CancellationToken cancellation = default)
        {
            _lastOnline.TryGetValue(userId, out var dt);
            return Task.FromResult<DateTime?>(dt);
        }

        public Task<IDictionary<int, DateTime?>> GetLastOnlineAsync(IEnumerable<int> userIds, CancellationToken cancellation = default)
        {
            var result = userIds
                .Distinct()
                .ToDictionary(
                    id => id,
                    id => _lastOnline.TryGetValue(id, out var dt) ? (DateTime?)dt : null
                );
            return Task.FromResult<IDictionary<int, DateTime?>>(result);
        }
    }
}