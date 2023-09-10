using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using EagleRock_EagleBot.Services;
using System.Text;

namespace EagleRock_EagleBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.ConfigureServices(builder.Configuration);
            var app = builder.Build();

            // Rabbit MQ            
            var factory = app.Services.GetService<ConnectionFactory>();
            var connection = factory.CreateConnection();
            var rabbitQueueName = builder.Configuration["RabbitQueueName"];
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
            Console.WriteLine($"Message received from RabbitMQ queue: {message}");
        }
    }
}