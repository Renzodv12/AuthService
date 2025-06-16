using AuthService.Core.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace AuthService.Core.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                return await _database.StringSetAsync(key, serializedValue, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting key {Key} in Redis", key);
                return false;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting key {Key} from Redis", key);
                return default;
            }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key {Key} exists in Redis", key);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string key)
        {
            try
            {
                return await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting key {Key} from Redis", key);
                return false;
            }
        }
    }
}
