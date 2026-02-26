namespace ThrPresetsApi.Api.Features.Presets.DTOs;

public class UpdatePresetDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Source { get; init; }
    public List<string>? TagIds { get; init; }
    public IFormFile? File { get; init; }
    public bool? IsPublic { get; init; }
}