using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Transmax_EagleRock_EagleBot.Services.Interfaces;

namespace Transmax_EagleRock_EagleBot.Services
{
    public class RabbitMQProducer : IMessageProducer
    {
        private readonly string? _hostname;
        private readonly string? _rabbitQueueName;

        public RabbitMQProducer(IConfiguration configuration)
       {
            _hostname = configuration["RabbitHostName"];
            _rabbitQueueName = configuration["RabbitQueueName"];
        }

        public void SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory { HostName = _hostname };
            var connection = factory.CreateConnection();
            
            using var channel = connection.CreateModel();
            channel.QueueDeclare(_rabbitQueueName, exclusive: false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish("", _rabbitQueueName, body: body);
        }
    }
}
