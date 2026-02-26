using Microsoft.EntityFrameworkCore;
using ThrPresetsApi.Api.Data;

namespace ThrPresetsApi.Api.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Default")));

        return services;
    }
}
