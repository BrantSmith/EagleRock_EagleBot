using EagleRock_EagleBot.Models;

namespace EagleRock_EagleBot.Services.Interfaces
{
    public interface ICacheHelper
    {
        IEnumerable<string> GetAllKeys();
        Task<T?> GetValue<T>(string key);
        Task SaveTrafficDataToCache(EagleBotData request);
    }
}
