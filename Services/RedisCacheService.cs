using StackExchange.Redis;
using System.Text.Json;

namespace RedisCacheDemo.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _cache;

        public RedisCacheService()
        {
            var redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _cache = redis.GetDatabase();
        }

        // Lấy dữ liệu từ cache
        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.StringGetAsync(key);
            return data.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(data);
        }

        // Lưu dữ liệu vào cache
        public async Task SetAsync<T>(string key, T data, TimeSpan? expiry = null)
        {
            string jsonData = JsonSerializer.Serialize(data);
            await _cache.StringSetAsync(key, jsonData, expiry);
        }

        // Xóa dữ liệu khỏi cache
        public async Task RemoveAsync(string key)
        {
            await _cache.KeyDeleteAsync(key);
        }
    }
}
