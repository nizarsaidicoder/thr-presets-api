namespace ThrPresetsApi.Api.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Preset> Presets { get; set; } = [];
    public ICollection<Rating> Ratings { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
    public ICollection<Collection> Collections { get; set; } = [];
    public ICollection<PresetReport> Reports { get; set; } = [];
}