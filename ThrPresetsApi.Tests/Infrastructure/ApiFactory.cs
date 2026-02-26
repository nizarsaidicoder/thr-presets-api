using System.Data.Common;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.LocalStack;
using Testcontainers.PostgreSql;
using ThrPresetsApi.Api.Data;
using TUnit.Core.Interfaces;

namespace ThrPresetsApi.Tests.Infrastructure;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncInitializer, IAsyncDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private readonly LocalStackContainer _localStack = new LocalStackBuilder("localstack/localstack:3")
        .Build();

    private Respawner? _respawner;
    private DbConnection? _dbConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => 
                options.UseNpgsql(_postgres.GetConnectionString()));

            services.RemoveAll<IAmazonS3>();
            services.AddSingleton<IAmazonS3>(_ => 
            {
                var config = new AmazonS3Config 
                { 
                    ServiceURL = _localStack.GetConnectionString(), 
                    ForcePathStyle = true 
                };
                return new AmazonS3Client(new Amazon.Runtime.BasicAWSCredentials("test", "test"), config);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _localStack.StartAsync());

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Initialize S3 Bucket for Tests
        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        await s3.PutBucketAsync("thr-presets-dev");

        // Initialize Respawner
        _dbConnection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_dbConnection != null && _respawner != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbSeeder.SeedTagsAsync(db);
        }
    }

    public new async ValueTask DisposeAsync()
    {
        if (_dbConnection != null) await _dbConnection.DisposeAsync();
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _localStack.DisposeAsync().AsTask());
        await base.DisposeAsync();
    }
}