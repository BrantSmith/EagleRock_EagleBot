using Microsoft.AspNetCore.Mvc;
using System.Text;
using EagleRock_EagleBot.Models;
using EagleRock_EagleBot.Services.Interfaces;

namespace EagleRock_EagleBot.Controllers
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

        /// <summary>
        /// Get latest traffic data for all active EagleBots 
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Save the traffic data from an EagleBot
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("TransferTrafficData")]
        public async Task<IActionResult> TransferTrafficData(EagleBotData request)
        {
            // Validate request
            var (valid, error) = ValidateRequest(request);
            if (!valid)
                return BadRequest(error);

            // Save data to cache
            await _cacheHelper.SaveTrafficDataToCache(request);

            // Send message to RabbitMQ
            _messagePublisher.SendMessage(request);
            return new OkResult();
        }

        /// <summary>
        /// Validate that expected values are provided
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [NonAction]
        private static (bool valid, string error) ValidateRequest(EagleBotData request)
        {
            var valid = true;
            var sb = new StringBuilder();

            if (request.Id == Guid.Empty)
            {
                valid = false;
                sb.AppendLine("Id is not provided");
            }
            if (request.Location.Latitude == 0 && request.Location.Longitude == 0)
            {
                valid = false;
                sb.AppendLine("Latitude and longitude are not provided");
            }
            if (string.IsNullOrWhiteSpace(request.RoadName))
            {
                valid = false;
                sb.AppendLine("RoadName is not provided");
            }
            if (request.RateOfTrafficFlow == 0)
            {
                valid = false;
                sb.AppendLine("RateOfTrafficFlow is not provided");
            }
            if (request.AvgVehicleSpeed == 0)
            {
                valid = false;
                sb.AppendLine("AvgVehicleSpeed is not provided");
            }

            return (valid, sb.ToString());
        }
    }
}