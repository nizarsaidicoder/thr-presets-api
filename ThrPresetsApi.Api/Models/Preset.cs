namespace ThrPresetsApi.Api.Models;

public class Preset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Slug { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Source { get; set; }
    public string S3Key { get; set; } = null!;
    public int FileSize { get; set; }
    public bool IsPublic { get; set; } = true;
    public int Downloads { get; set; } = 0;
    public double WilsonScore { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // FK
    public string? AuthorId { get; set; }
    public User? Author { get; set; }

    // Navigation
    public ICollection<PresetTag> Tags { get; set; } = [];
    public ICollection<Rating> Ratings { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
    public ICollection<CollectionItem> CollectionItems { get; set; } = [];
    public ICollection<PresetReport> Reports { get; set; } = [];
}