using Microsoft.AspNetCore.Mvc;
using Transmax_EagleRock_EagleBot.Models;
using Transmax_EagleRock_EagleBot.Services.Interfaces;

namespace Transmax_EagleRock_EagleBot.Controllers
{
    [ApiController]
    [Route("")]
    public class EagleRockController : ControllerBase
    {
        private readonly ICacheHelper _cacheHelper;
        private readonly IMessageProducer _messagePublisher;

        public EagleRockController(ICacheHelper cacheHelper, IMessageProducer messagePublisher)
        {
            _cacheHelper = cacheHelper;
            _messagePublisher = messagePublisher;
        }

        [HttpGet("GetActiveEagleBots")]
        public async Task<IActionResult> GetAllActiveEagleBotsFromCache()
        {
            var results = new List<EagleBotData>();

            // Grab keys from redis cache
            var keys = _cacheHelper.GetAllKeys();

            // Pull values from cache and map to object
            foreach (var key in keys)
            {
                var record = await _cacheHelper.GetValue<EagleBotData>(key);
                if (record != null)
                    results.Add(record);
            }

            return new OkObjectResult(results);
        }

        [HttpPost("TransferTrafficData")]
        public async Task<IActionResult> TransferTrafficData(EagleBotData request)
        {
            await _cacheHelper.SaveTrafficDataToCache(request);
            _messagePublisher.SendMessage(request);
            return new OkResult();
        }        
    }
}