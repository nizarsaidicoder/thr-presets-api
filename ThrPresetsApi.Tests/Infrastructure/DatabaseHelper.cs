using System.Text;
using Microsoft.EntityFrameworkCore;
using ThrPresetsApi.Api.Data;

namespace ThrPresetsApi.Tests.Infrastructure;

public static class DatabaseHelper
{
    public static async Task ResetAsync(ApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.ExecuteSqlRawAsync(@"
        TRUNCATE TABLE 
            ""Users"", 
            ""Presets"", 
            ""Tags"", 
            ""PresetTags"", 
            ""Ratings"", 
            ""Favorites"", 
            ""Collections"", 
            ""CollectionItems"", 
            ""PresetReports"" 
        RESTART IDENTITY CASCADE;");
    }
}