using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Transmax_EagleRock_EagleBot.Services;
using System.Text;

namespace Transmax_EagleRock_EagleBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.ConfigureServices(builder.Configuration);
            var hostname = builder.Configuration["RabbitHostName"];
            var rabbitQueueName = builder.Configuration["RabbitQueueName"];

            var app = builder.Build();

            var factory = new ConnectionFactory { HostName = hostname };
            var connection = factory.CreateConnection();

            using var channel = connection.CreateModel();
            channel.QueueDeclare(rabbitQueueName, exclusive: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(queue: rabbitQueueName, autoAck: false, consumer: consumer);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        public static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(message);
        }
    }
}