using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using Transmax_EagleRock_EagleBot.Models;
using Transmax_EagleRock_EagleBot.Services.Interfaces;

namespace Transmax_EagleRock_EagleBot.Services
{
    public class CacheHelper : ICacheHelper
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IServer _server;

        // Setting up the cache options - would usually be a larger expiration, but kept lower for proof of concept
        private readonly DistributedCacheEntryOptions _cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(30))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

        public CacheHelper(IDistributedCache cache, IConnectionMultiplexer multiplexer, IConfiguration config)
        {
            _cache = cache;
            _multiplexer = multiplexer;
            _server = _multiplexer.GetServer(config.GetConnectionString("RedisConnString") ?? string.Empty);
        }

        /// <summary>
        /// Get all keys from the redis cache
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllKeys()
        {
            var keys = _server.Keys();
            return keys.Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// Obtain the value from the redis cache based on the specified key, and return as the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T?> GetValue<T>(string key)
        {
            var data = await _cache.GetAsync(key);
            if (data != null)
                return Utility.MapByteArrayTo<T>(data);

            return default;
        }

        public async Task SaveTrafficDataToCache(EagleBotData request)
        {
            var cachedDataString = JsonSerializer.Serialize(request);
            var dataToCache = Encoding.UTF8.GetBytes(cachedDataString);

            await _cache.SetAsync(request.Id.ToString(), dataToCache, _cacheEntryOptions);
        }
    }
}
