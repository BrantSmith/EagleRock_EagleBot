using RabbitMQ.Client;
using StackExchange.Redis;
using EagleRock_EagleBot.Services.Interfaces;

namespace EagleRock_EagleBot.Services
{
    public static class ServicesConfiguration
    {
        public static void ConfigureServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Redis
            var redisConnString = configuration.GetConnectionString("RedisConnString");
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnString));
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnString);

            // RabbitMQ
            var rabbitConnString = configuration.GetConnectionString("RabbitConnString");
            services.AddSingleton(_ => new ConnectionFactory { HostName = rabbitConnString });

            services.AddScoped<ICacheHelper, CacheHelper>();
            services.AddScoped<IMessageProducer, RabbitMQProducer>();
        }
    }
}
