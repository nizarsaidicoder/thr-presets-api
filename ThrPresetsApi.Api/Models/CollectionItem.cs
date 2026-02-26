namespace ThrPresetsApi.Api.Models;

public class CollectionItem
{
    public int Position { get; set; }
    public string? Note { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public string CollectionId { get; set; } = null!;
    public string PresetId { get; set; } = null!;

    public Collection Collection { get; set; } = null!;
    public Preset Preset { get; set; } = null!;
}