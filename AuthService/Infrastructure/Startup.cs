using AuthService.Core.Interfaces;
using AuthService.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AuthService.Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddDbContext<ApplicationDbContext>(options =>
                     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                return ConnectionMultiplexer.Connect(connectionString);
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}


