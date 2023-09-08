using StackExchange.Redis;
using Transmax_EagleRock_EagleBot.Services.Interfaces;

namespace Transmax_EagleRock_EagleBot.Services
{
    public static class ServicesConfiguration
    {
        public static void ConfigureServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddControllers().AddNewtonsoftJson();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var redisConnString = configuration.GetConnectionString("RedisConnString");

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnString));
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnString);
            services.AddScoped<ICacheHelper, CacheHelper>();
            services.AddScoped<IMessageProducer, RabbitMQProducer>();
        }
    }
}
