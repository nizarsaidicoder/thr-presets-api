namespace ThrPresetsApi.Api.Features.Presets.DTOs;

public class CreatePresetDto
{
     public string? Name { get; init; }
     public string? Description { get; init; }
     public string? Source { get; init; }
     public IFormFile? File { get; init; }
     public bool? IsPublic { get; init; }
     public List<string>? TagIds { get; init; }
}