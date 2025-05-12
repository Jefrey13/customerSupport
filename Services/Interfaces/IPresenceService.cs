namespace CustomerService.API.Services.Interfaces
{
    public interface IPresenceService
    {
        Task<IDictionary<int, DateTime?>> GetLastOnlineAsync(IEnumerable<int> userIds, CancellationToken cancellation = default);
        Task<DateTime?> GetLastOnlineAsync(int userId, CancellationToken cancellation = default);
    }
}
