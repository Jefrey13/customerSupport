using CustomerService.API.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Services.Implementations
{
    public class InMemoryPresenceService : IPresenceService
    {
        private static readonly ConcurrentDictionary<int, DateTime> _store = new();

        public Task<DateTime?> GetLastOnlineAsync(int userId, CancellationToken cancellation = default)
        {
            return Task.FromResult(_store.TryGetValue(userId, out var last)
                ? (DateTime?)last
                : null);
        }

        public Task<IDictionary<int, DateTime?>> GetLastOnlineAsync(IEnumerable<int> userIds, CancellationToken cancellation = default)
        {
            var result = userIds.Distinct()
                .ToDictionary(
                    id => id,
                    id => _store.TryGetValue(id, out var dt) ? (DateTime?)dt : null
                );
            return Task.FromResult((IDictionary<int, DateTime?>)result);
        }

        public Task UpdateLastOnlineAsync(int userId, CancellationToken cancellation = default)
        {
            _store[userId] = DateTime.UtcNow;
            return Task.CompletedTask;
        }
    }
}