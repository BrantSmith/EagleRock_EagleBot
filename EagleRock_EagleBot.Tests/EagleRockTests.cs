using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using StackExchange.Redis;
using EagleRock_EagleBot.Controllers;
using EagleRock_EagleBot.Models;
using EagleRock_EagleBot.Services;
using EagleRock_EagleBot.Services.Interfaces;

namespace EagleRock_EagleBot.Tests
{
    public class Tests
    {
        private readonly Mock<IDistributedCache> _mockCache = new();
        private readonly Mock<IConnectionMultiplexer> _mockMultiplexer = new();
        private readonly Mock<IMessageProducer> _mockMessageProducer = new();
        private readonly Mock<IServer> _mockServer = new();

        private EagleRockController _controller;

        private readonly List<EagleBotData> _expectedResults = new();
        private EagleBotData? _expectedResult;

        const string _byteStr = "eyJJZCI6IjNmYTg1ZjY0LTU3MTctNDU2Mi1iM2ZjLTJjOTYzZjY2YWZhNiIsIkxvY2F0aW9uIjp7IkxhdGl0dWRlIjotMjcuNDY4ODI5NjI2MTcxODkyLCJMb25naXR1ZGUiOjE1My4wMDkxNDMwNTc1ODM1N30sIlRpbWVzdGFtcCI6IjIwMjMtMDktMDhUMjI6NTc6MjguNDE4KzAwOjAwIiwiUm9hZE5hbWUiOiJMaXR0bGUgQ3JpYmIgU3QsIE1pbHRvbiIsIkRpcmVjdGlvbiI6MiwiUmF0ZU9mVHJhZmZpY0Zsb3ciOjIyLCJBdmdWZWhpY2xlU3BlZWQiOjQwfQ==\r\n";
        private readonly byte[] bytes = Convert.FromBase64String(_byteStr);

        [SetUp]
        public void Setup()
        {
            var id = Guid.NewGuid().ToString();
            var keys = new RedisKey[] { id };
      
            _mockCache.Setup(c => c.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(bytes);
            _expectedResult = Utility.MapByteArrayTo<EagleBotData>(bytes);
            if (_expectedResult != null)
                _expectedResults.Add(_expectedResult);

            _mockServer.Setup(_ => _.Keys(
                    It.IsAny<int>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>())).Returns(keys);

            _mockMultiplexer.Setup(m => m.GetServer(It.IsAny<string>(), It.IsAny<object?>())).Returns(_mockServer.Object);

            var mockConfSection = new Mock<IConfigurationSection>();
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfSection.SetupGet(m => m[It.Is<string>(s => s == "RedisConnString")]).Returns("mock value");
            mockConfSection.SetupGet(m => m[It.Is<string>(s => s == "RabbitConnString")]).Returns("mock value");
            mockConfiguration.Setup(a => a.GetSection(It.Is<string>(s => s == "ConnectionStrings"))).Returns(mockConfSection.Object);

            _controller = new EagleRockController(new CacheHelper(_mockCache.Object, _mockMultiplexer.Object, mockConfiguration.Object), _mockMessageProducer.Object);
        }

        [Test]
        public async Task Test_GetAllActiveEagleBotsFromCache()
        {
            // Act
            var result = await _controller.GetAllActiveEagleBotsFromCache();

            // Assert
            var okObjectResult = result as OkObjectResult;
            okObjectResult.Should().NotBeNull();

            var model = okObjectResult?.Value as List<EagleBotData>;
            model.Should().NotBeNull().And.HaveSameCount(_expectedResults);
            for (int i = 0; i < model?.Count; i++)
                model[i].Should().BeEquivalentTo(_expectedResults[i]);
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
            okObjectResult.Should().NotBeNull();
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
                RoadName = string.Empty,
                Direction = DirectionOfTraffic.NE,
                RateOfTrafficFlow = 0,
                AvgVehicleSpeed = 0
            });

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var error = badRequestResult?.Value as string;

            error.Should().NotBeNull();
            error.Should().ContainAll(
                "Id is not provided",
                "Latitude and longitude are not provided",
                "RoadName is not provided",
                "RateOfTrafficFlow is not provided",
                "AvgVehicleSpeed is not provided"
                );
        }

        // NOTE: Unable to test RabbitMQ, as there is not a way to mock ConnectionFactory.CreateConnection (returns IConnection)
        //[Test]
        //public async Task Test_RabbitMQ_SendMessage()
        //{
            
        //}

    }
}