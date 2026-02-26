namespace ThrPresetsApi.Api.Models;

public class PresetTag
{
    public string PresetId { get; set; } = null!;
    public string TagId { get; set; } = null!;

    public Preset Preset { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}