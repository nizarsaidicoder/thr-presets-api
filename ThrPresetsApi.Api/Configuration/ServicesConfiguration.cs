using Amazon.S3;
using ThrPresetsApi.Api.Features.Auth;
using ThrPresetsApi.Api.Features.Presets;
using ThrPresetsApi.Api.Features.Users;
using ThrPresetsApi.Api.Infrastructure.S3;

namespace ThrPresetsApi.Api.Configuration;

public static class ServicesConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<TokenService>();
        services.AddScoped<UserService>();
        services.AddAWSService<IAmazonS3>();
        services.AddScoped<IS3Service, S3Service>();
        services.AddScoped<PresetService>();
        return services;
    }
    public static IServiceCollection AddS3Infrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var awsOptions = configuration.GetAWSOptions();
        var s3Config = new AmazonS3Config();

        var serviceUrl = configuration["AWS:ServiceURL"];
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            s3Config.ServiceURL = serviceUrl;
            s3Config.ForcePathStyle = true; 
            awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(
                configuration["AWS:AccessKey"] ?? "test", 
                configuration["AWS:SecretKey"] ?? "test"
            );
        }
        else
        {
            awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"]
            );
        }

        services.AddDefaultAWSOptions(awsOptions);
        services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(awsOptions.Credentials, s3Config));
        services.AddScoped<IS3Service, S3Service>();

        return services;
    }
}
