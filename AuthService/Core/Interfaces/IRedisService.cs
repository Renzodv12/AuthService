namespace AuthService.Core.Interfaces
{
    public interface IRedisService
    {
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
        Task<bool> KeyExistsAsync(string key);
        Task<bool> DeleteAsync(string key);
    }
}
