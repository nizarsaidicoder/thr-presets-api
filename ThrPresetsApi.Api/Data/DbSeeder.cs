using Microsoft.EntityFrameworkCore;
using ThrPresetsApi.Api.Models;

namespace ThrPresetsApi.Api.Data;

public static class DbSeeder
{
    public static async Task SeedTagsAsync(AppDbContext db)
    {
        if (await db.Tags.AnyAsync()) return;

        var tags = new List<Tag>
        {
            // AmpModels (THR10/30 Series)
            new() { Name = "Clean", Type = TagType.AmpModel },
            new() { Name = "Crunch", Type = TagType.AmpModel },
            new() { Name = "Lead", Type = TagType.AmpModel },
            new() { Name = "Brit Hi", Type = TagType.AmpModel },
            new() { Name = "Modern", Type = TagType.AmpModel },
        
            // AmpVersions (Hardware generations)
            new() { Name = "THR10II", Type = TagType.AmpVersion },
            new() { Name = "THR30II", Type = TagType.AmpVersion },
            new() { Name = "THR10", Type = TagType.AmpVersion },

            // Genres
            new() { Name = "Blues", Type = TagType.Genre },
            new() { Name = "Metal", Type = TagType.Genre },
            new() { Name = "Jazz", Type = TagType.Genre },
        
            // Styles
            new() { Name = "High Gain", Type = TagType.Style },
            new() { Name = "Ambient", Type = TagType.Style },
            new() { Name = "Lo-Fi", Type = TagType.Style }
        };

        db.Tags.AddRange(tags);
        await db.SaveChangesAsync();
    }
}