using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Net;
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
        private readonly EndPoint _endpoint;

        // Setting up the cache options - would usually be a larger expiration
        private readonly DistributedCacheEntryOptions _cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(60))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

        public CacheHelper(IDistributedCache cache, IConnectionMultiplexer multiplexer)
        {
            _cache = cache;
            _multiplexer = multiplexer;
            _endpoint = _multiplexer.GetEndPoints().First();
        }

        public IEnumerable<string> GetAllKeys()
        {
            var keys = _multiplexer.GetServer(_endpoint).Keys(pattern: "*").Select(x => x.ToString()).ToArray();
            return keys;
        }

        public async Task<T?> GetValue<T>(string key)
        {
            var data = await _cache.GetAsync(key);
            if (data != null)
            {
                using MemoryStream ms = new(data);
                return JsonSerializer.Deserialize<T>(ms);
            }

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
