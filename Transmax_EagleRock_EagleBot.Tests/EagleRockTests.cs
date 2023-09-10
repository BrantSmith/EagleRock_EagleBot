using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using StackExchange.Redis;
using System.Text.Json;
using Transmax_EagleRock_EagleBot.Controllers;
using Transmax_EagleRock_EagleBot.Models;
using Transmax_EagleRock_EagleBot.Services;
using Transmax_EagleRock_EagleBot.Services.Interfaces;

namespace Transmax_EagleRock_EagleBot.Tests
{
    public class Tests
    {
        private readonly Mock<IDistributedCache> _cache = new Mock<IDistributedCache>();
        private readonly Mock<IConnectionMultiplexer> _multiplexer = new Mock<IConnectionMultiplexer>();
        private readonly Mock<IMessageProducer> _messageProducer = new Mock<IMessageProducer>();
        private readonly Mock<IServer> _server = new Mock<IServer>();

        private EagleRockController _controller;

        private List<EagleBotData> _expectedResults = new List<EagleBotData>();
        private EagleBotData? _expectedResult;

        const string _byteStr = "eyJJZCI6IjNmYTg1ZjY0LTU3MTctNDU2Mi1iM2ZjLTJjOTYzZjY2YWZhNiIsIkxvY2F0aW9uIjp7IkxhdGl0dWRlIjotMjcuNDY4ODI5NjI2MTcxODkyLCJMb25naXR1ZGUiOjE1My4wMDkxNDMwNTc1ODM1N30sIlRpbWVzdGFtcCI6IjIwMjMtMDktMDhUMjI6NTc6MjguNDE4KzAwOjAwIiwiUm9hZE5hbWUiOiJMaXR0bGUgQ3JpYmIgU3QsIE1pbHRvbiIsIkRpcmVjdGlvbiI6MiwiUmF0ZU9mVHJhZmZpY0Zsb3ciOjIyLCJBdmdWZWhpY2xlU3BlZWQiOjQwfQ==\r\n";
        byte[] bytes = Convert.FromBase64String(_byteStr);

        [SetUp]
        public void Setup()
        {
            var id = Guid.NewGuid().ToString();
            var keys = new RedisKey[] { id };
      
            _cache.Setup(c => c.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(bytes);
            _expectedResult = Utility.MapByteArrayTo<EagleBotData>(bytes);
            if (_expectedResult != null)
                _expectedResults.Add(_expectedResult);

            _server.Setup(_ => _.Keys(
                    It.IsAny<int>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>())).Returns(keys);

            _multiplexer.Setup(m => m.GetServer(It.IsAny<string>(), It.IsAny<object?>())).Returns(_server.Object);


            var mockConfSection = new Mock<IConfigurationSection>();
            mockConfSection.SetupGet(m => m[It.Is<string>(s => s == "RedisConnString")]).Returns("mock value");

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(a => a.GetSection(It.Is<string>(s => s == "ConnectionStrings"))).Returns(mockConfSection.Object);

            _controller = new EagleRockController(new CacheHelper(_cache.Object, _multiplexer.Object, mockConfiguration.Object), _messageProducer.Object);
        }

        [Test]
        public async Task Test_GetAllActiveEagleBotsFromCache()
        {
            // Act
            var result = await _controller.GetAllActiveEagleBotsFromCache();

            // Assert
            var okObjectResult = result as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var model = okObjectResult.Value as List<EagleBotData>;
            Assert.NotNull(model);
            model[0].Should().BeEquivalentTo(_expectedResults[0]);
        }

        [Test]
        public async Task Test_TransferTrafficData_Success()
        {
            // Act
            var result = await _controller.TransferTrafficData(new EagleBotData
            {
                Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Location = new Coordinate {
                    Latitude = -27.468829626171894,
                    Longitude = 153.00914305758358
                },
                Timestamp = DateTimeOffset.Now,
                RoadName = "Little Cribb St, Milton",
                Direction = DirectionOfTraffic.NE,
                RateOfTrafficFlow = 22.0,
                AvgVehicleSpeed = 40.0
            });

            // Assert
            var okObjectResult = result as OkResult;
            Assert.NotNull(okObjectResult);
        }

        [Test]
        public async Task Test_TransferTrafficData_BadRequest()
        {
            // Act
            var result = await _controller.TransferTrafficData(new EagleBotData
            {
                Id = Guid.Empty,
                Location = new Coordinate
                {
                    Latitude = 0,
                    Longitude = 0
                },
                Timestamp = DateTimeOffset.Now,
                RoadName = "",
                Direction = DirectionOfTraffic.NE,
                RateOfTrafficFlow = 0,
                AvgVehicleSpeed = 0
            });

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);

            var error = badRequestResult.Value as string;
            Assert.NotNull(error);
            error.Should().ContainAll(
                "Id is not provided",
                "Latitude and longitude are not provided",
                "RoadName is not provided",
                "RateOfTrafficFlow is not provided",
                "AvgVehicleSpeed is not provided"
                );
        }

    }
}