using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using EagleRock_EagleBot.Services.Interfaces;

namespace EagleRock_EagleBot.Services
{
    public class RabbitMQProducer : IMessageProducer
    {
        private readonly ConnectionFactory _factory;
        private readonly string? _rabbitQueueName;

        public RabbitMQProducer(IConfiguration configuration, ConnectionFactory factory)
        {
            _factory = factory;
            _rabbitQueueName = configuration["RabbitQueueName"];
        }

        public void SendMessage<T>(T message)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(_rabbitQueueName, exclusive: false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish("", _rabbitQueueName, body: body);
        }
    }
}
