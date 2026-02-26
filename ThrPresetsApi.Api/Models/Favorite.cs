namespace ThrPresetsApi.Api.Models;

public class Favorite
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = null!;
    public string PresetId { get; set; } = null!;

    public User User { get; set; } = null!;
    public Preset Preset { get; set; } = null!;
}