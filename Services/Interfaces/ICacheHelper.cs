﻿using Transmax_EagleRock_EagleBot.Models;

namespace Transmax_EagleRock_EagleBot.Services.Interfaces
{
    public interface ICacheHelper
    {
        IEnumerable<string> GetAllKeys();
        Task<T?> GetValue<T>(string key);
        Task SaveTrafficDataToCache(EagleBotData request);
    }
}
