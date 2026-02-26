namespace ThrPresetsApi.Api.Models;

public class Rating
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Stars { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = null!;
    public string PresetId { get; set; } = null!;

    public User User { get; set; } = null!;
    public Preset Preset { get; set; } = null!;
}