using ThrPresetsApi.Api.Features.Auth;

namespace ThrPresetsApi.Api.Configuration;

public static class ServicesConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<TokenService>();
        return services;
    }
}
