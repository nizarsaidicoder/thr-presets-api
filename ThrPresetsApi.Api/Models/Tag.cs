namespace ThrPresetsApi.Api.Models;

public enum TagType { AmpModel, AmpVersion, Genre, Style }

public class Tag
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public TagType Type { get; set; }

    public ICollection<PresetTag> Presets { get; set; } = [];
}