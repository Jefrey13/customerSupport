namespace CustomerService.API.Services.Interfaces
{
    public interface ISignalRNotifyService
    {
        Task NotifyUserAsync(int userId, string method, object payload);
    }
}
